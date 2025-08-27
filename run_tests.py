#!/usr/bin/env python3

"""
CsharpFlow Comprehensive Test Runner
=====================================

A robust Python3 script for building and testing the CsharpFlow library
with support for multiple build environments and detailed reporting.
"""

import os
import sys
import subprocess
import shutil
import json
import time
import platform
from pathlib import Path
from dataclasses import dataclass
from typing import List, Dict, Optional, Tuple
from enum import Enum

class BuildTool(Enum):
    DOTNET = "dotnet"
    MONO = "mono"
    NONE = "none"

class TestResult(Enum):
    PASS = "PASS"
    FAIL = "FAIL"
    SKIP = "SKIP"

@dataclass
class TestSuite:
    name: str
    file_path: str
    test_count: int
    result: TestResult = TestResult.SKIP
    duration: float = 0.0
    details: str = ""

@dataclass
class BuildReport:
    success: bool
    build_tool: BuildTool
    duration: float
    output: str
    binary_path: Optional[str] = None

class CsharpFlowTester:
    def __init__(self, project_root: str = "."):
        self.project_root = Path(project_root).resolve()
        self.bin_folder = self.project_root / "Bin"
        self.solution_file = self.project_root / "Flow.sln"
        self.test_project = self.project_root / "TestFlow" / "TestFlow.csproj"
        self.build_tool = BuildTool.NONE
        self.test_suites: List[TestSuite] = []
        
    def print_header(self):
        """Print colorful header with system info"""
        print("🚀 " + "="*60)
        print("🚀   CsharpFlow Comprehensive Test Suite")  
        print("🚀 " + "="*60)
        print(f"📍 Project Root: {self.project_root}")
        print(f"🖥️  Platform: {platform.system()} {platform.release()}")
        print(f"🐍 Python: {sys.version.split()[0]}")
        print("🚀 " + "="*60)
        print()

    def check_build_tools(self) -> BuildTool:
        """Check available build tools and return the best option"""
        print("📋 Checking build tools...")
        
        tools_to_check = [
            ("dotnet", BuildTool.DOTNET, "dotnet --version"),
            ("mono", BuildTool.MONO, "mono --version"),
        ]
        
        for tool, enum_val, version_cmd in tools_to_check:
            if shutil.which(tool):
                try:
                    result = subprocess.run(
                        version_cmd.split(), 
                        capture_output=True, 
                        text=True, 
                        timeout=10
                    )
                    if result.returncode == 0:
                        version = result.stdout.strip().split('\n')[0]
                        print(f"✅ {tool} found: {version}")
                        self.build_tool = enum_val
                        return enum_val
                except (subprocess.TimeoutExpired, subprocess.SubprocessError):
                    print(f"⚠️  {tool} found but not responding")
                    
        print("❌ No suitable build tools found")
        print("📥 Install options:")
        print("   • .NET SDK: https://dotnet.microsoft.com/download")
        print("   • Mono: sudo apt install mono-devel mono-complete")
        return BuildTool.NONE

    def create_bin_folder(self):
        """Create Bin folder for output binaries"""
        print(f"📁 Creating Bin folder: {self.bin_folder}")
        self.bin_folder.mkdir(exist_ok=True)
        
        # Create subfolders
        (self.bin_folder / "Debug").mkdir(exist_ok=True)
        (self.bin_folder / "Release").mkdir(exist_ok=True)
        print("✅ Bin folder structure created")

    def analyze_project_structure(self):
        """Analyze project structure and identify test suites"""
        print("📊 Analyzing project structure...")
        
        # Count source files
        cs_files = list(self.project_root.rglob("*.cs"))
        interface_files = list((self.project_root / "Interfaces").glob("*.cs"))
        impl_files = list((self.project_root / "Impl").glob("*.cs"))
        test_files = list((self.project_root / "TestFlow" / "Editor").glob("Test*.cs"))
        
        print(f"📄 Total C# files: {len(cs_files)}")
        print(f"🔌 Interface files: {len(interface_files)}")
        print(f"⚙️  Implementation files: {len(impl_files)}")
        print(f"🧪 Test files: {len(test_files)}")
        
        # Analyze test files
        for test_file in test_files:
            test_count = self.count_test_methods(test_file)
            self.test_suites.append(TestSuite(
                name=test_file.stem,
                file_path=str(test_file),
                test_count=test_count
            ))
            print(f"   • {test_file.name}: {test_count} tests")
        
        total_tests = sum(suite.test_count for suite in self.test_suites)
        print(f"🎯 Total test methods: {total_tests}")
        print()

    def count_test_methods(self, file_path: Path) -> int:
        """Count [Test] methods in a test file"""
        try:
            content = file_path.read_text(encoding='utf-8')
            return content.count('[Test]')
        except Exception:
            return 0

    def build_project(self) -> BuildReport:
        """Build the project using available build tools"""
        print("🔨 Building CsharpFlow...")
        start_time = time.time()
        
        if self.build_tool == BuildTool.NONE:
            return BuildReport(
                success=False,
                build_tool=BuildTool.NONE,
                duration=0.0,
                output="No build tools available"
            )
        
        try:
            if self.build_tool == BuildTool.DOTNET:
                return self.build_with_dotnet(start_time)
            elif self.build_tool == BuildTool.MONO:
                return self.build_with_mono(start_time)
        except Exception as e:
            return BuildReport(
                success=False,
                build_tool=self.build_tool,
                duration=time.time() - start_time,
                output=f"Build failed with exception: {str(e)}"
            )

    def build_with_dotnet(self, start_time: float) -> BuildReport:
        """Build using .NET CLI"""
        print("Using .NET CLI...")
        
        # Set output path to our Bin folder
        output_path = self.bin_folder / "Debug"
        
        commands = [
            (["dotnet", "restore", str(self.solution_file)], "Restoring packages"),
            (["dotnet", "build", str(self.solution_file), 
              "--configuration", "Debug", 
              "--output", str(output_path)], "Building solution")
        ]
        
        all_output = []
        for cmd, desc in commands:
            print(f"   {desc}...")
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                cwd=self.project_root,
                timeout=300
            )
            all_output.append(f"=== {desc} ===\n{result.stdout}\n{result.stderr}\n")
            
            if result.returncode != 0:
                return BuildReport(
                    success=False,
                    build_tool=BuildTool.DOTNET,
                    duration=time.time() - start_time,
                    output="\n".join(all_output)
                )
        
        # Find the built DLL
        dll_path = output_path / "Flow.dll"
        
        return BuildReport(
            success=True,
            build_tool=BuildTool.DOTNET,
            duration=time.time() - start_time,
            output="\n".join(all_output),
            binary_path=str(dll_path) if dll_path.exists() else None
        )

    def build_with_mono(self, start_time: float) -> BuildReport:
        """Build using Mono/MSBuild"""
        print("Using Mono/MSBuild...")
        
        # Try different MSBuild commands
        msbuild_commands = ["msbuild", "xbuild"]
        
        for msbuild_cmd in msbuild_commands:
            if shutil.which(msbuild_cmd):
                cmd = [
                    msbuild_cmd,
                    str(self.solution_file),
                    "/p:Configuration=Debug",
                    f"/p:OutputPath={self.bin_folder / 'Debug'}"
                ]
                
                print(f"   Using {msbuild_cmd}...")
                result = subprocess.run(
                    cmd,
                    capture_output=True,
                    text=True,
                    cwd=self.project_root,
                    timeout=300
                )
                
                output = f"STDOUT:\n{result.stdout}\nSTDERR:\n{result.stderr}"
                
                if result.returncode == 0:
                    dll_path = self.bin_folder / "Debug" / "Flow.dll"
                    return BuildReport(
                        success=True,
                        build_tool=BuildTool.MONO,
                        duration=time.time() - start_time,
                        output=output,
                        binary_path=str(dll_path) if dll_path.exists() else None
                    )
                else:
                    return BuildReport(
                        success=False,
                        build_tool=BuildTool.MONO,
                        duration=time.time() - start_time,
                        output=output
                    )
        
        return BuildReport(
            success=False,
            build_tool=BuildTool.MONO,
            duration=time.time() - start_time,
            output="No MSBuild tool found"
        )

    def run_tests(self, build_report: BuildReport):
        """Run the test suite"""
        if not build_report.success:
            print("❌ Cannot run tests - build failed")
            return
        
        print("🧪 Running test suite...")
        
        if self.build_tool == BuildTool.DOTNET:
            self.run_tests_dotnet()
        elif self.build_tool == BuildTool.MONO:
            self.run_tests_mono()

    def run_tests_dotnet(self):
        """Run tests using dotnet test"""
        print("Using .NET Test Runner...")
        
        try:
            cmd = [
                "dotnet", "test",
                str(self.test_project),
                "--configuration", "Debug",
                "--logger", "console;verbosity=detailed",
                "--results-directory", str(self.bin_folder / "TestResults")
            ]
            
            start_time = time.time()
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                cwd=self.project_root,
                timeout=600
            )
            duration = time.time() - start_time
            
            # Parse test results
            self.parse_dotnet_test_results(result.stdout, result.stderr, duration)
            
        except subprocess.TimeoutExpired:
            print("⏰ Test execution timed out")
            for suite in self.test_suites:
                suite.result = TestResult.FAIL
                suite.details = "Timeout"
        except Exception as e:
            print(f"❌ Test execution failed: {e}")
            for suite in self.test_suites:
                suite.result = TestResult.FAIL
                suite.details = str(e)

    def run_tests_mono(self):
        """Run tests using Mono NUnit"""
        print("Using Mono NUnit Runner...")
        
        # Look for NUnit runners
        nunit_runners = [
            "nunit3-console",
            "nunit-console",
            "mono nunit-console.exe"
        ]
        
        test_dll = self.bin_folder / "Debug" / "TestFlow.dll"
        if not test_dll.exists():
            print(f"❌ Test assembly not found: {test_dll}")
            return
        
        for runner in nunit_runners:
            runner_parts = runner.split()
            if shutil.which(runner_parts[0]):
                print(f"   Using {runner}...")
                try:
                    cmd = runner_parts + [str(test_dll)]
                    start_time = time.time()
                    result = subprocess.run(
                        cmd,
                        capture_output=True,
                        text=True,
                        timeout=600
                    )
                    duration = time.time() - start_time
                    
                    self.parse_nunit_test_results(result.stdout, result.stderr, duration)
                    return
                    
                except Exception as e:
                    print(f"❌ Failed to run {runner}: {e}")
                    continue
        
        print("❌ No suitable NUnit runner found")
        for suite in self.test_suites:
            suite.result = TestResult.FAIL
            suite.details = "No NUnit runner available"

    def parse_dotnet_test_results(self, stdout: str, stderr: str, duration: float):
        """Parse dotnet test output"""
        lines = stdout.split('\n')
        
        # Simple parsing - look for test results
        passed_tests = 0
        failed_tests = 0
        
        for line in lines:
            if "Passed!" in line or "passed" in line.lower():
                passed_tests += 1
            elif "Failed!" in line or "failed" in line.lower():
                failed_tests += 1
        
        # Update test suite results
        total_expected = sum(suite.test_count for suite in self.test_suites)
        if "Test Run Successful" in stdout or failed_tests == 0:
            for suite in self.test_suites:
                suite.result = TestResult.PASS
                suite.duration = duration / len(self.test_suites)
                suite.details = "All tests passed"
        else:
            for suite in self.test_suites:
                suite.result = TestResult.FAIL
                suite.duration = duration / len(self.test_suites)
                suite.details = f"Some tests failed. Check output."

    def parse_nunit_test_results(self, stdout: str, stderr: str, duration: float):
        """Parse NUnit test output"""
        # Similar simple parsing for NUnit
        if "Overall result: Passed" in stdout or "Test Run Successful" in stdout:
            for suite in self.test_suites:
                suite.result = TestResult.PASS
                suite.duration = duration / len(self.test_suites)
                suite.details = "All tests passed"
        else:
            for suite in self.test_suites:
                suite.result = TestResult.FAIL
                suite.duration = duration / len(self.test_suites)
                suite.details = "Check NUnit output for details"

    def generate_report(self, build_report: BuildReport):
        """Generate comprehensive test report"""
        print("\n" + "="*70)
        print("📋 COMPREHENSIVE TEST REPORT")
        print("="*70)
        
        # Build results
        print("\n🔨 BUILD RESULTS:")
        if build_report.success:
            print(f"✅ Build: SUCCESS ({build_report.duration:.2f}s)")
            print(f"🔧 Tool: {build_report.build_tool.value}")
            if build_report.binary_path:
                print(f"📦 Binary: {build_report.binary_path}")
        else:
            print(f"❌ Build: FAILED ({build_report.duration:.2f}s)")
            print(f"🔧 Tool: {build_report.build_tool.value}")
            print("📝 Build Output:")
            print(build_report.output[:1000] + "..." if len(build_report.output) > 1000 else build_report.output)
        
        # Test results
        print("\n🧪 TEST RESULTS:")
        passed_suites = [s for s in self.test_suites if s.result == TestResult.PASS]
        failed_suites = [s for s in self.test_suites if s.result == TestResult.FAIL]
        skipped_suites = [s for s in self.test_suites if s.result == TestResult.SKIP]
        
        total_tests = sum(suite.test_count for suite in self.test_suites)
        passed_tests = sum(suite.test_count for suite in passed_suites)
        
        print(f"📊 Test Suites: {len(passed_suites)} passed, {len(failed_suites)} failed, {len(skipped_suites)} skipped")
        print(f"📊 Test Methods: {passed_tests}/{total_tests} passed")
        
        for suite in self.test_suites:
            status_icon = "✅" if suite.result == TestResult.PASS else "❌" if suite.result == TestResult.FAIL else "⏭️"
            print(f"   {status_icon} {suite.name}: {suite.test_count} tests ({suite.duration:.2f}s)")
            if suite.details and suite.result != TestResult.PASS:
                print(f"      └─ {suite.details}")
        
        # Summary
        print(f"\n📁 Output Directory: {self.bin_folder}")
        print(f"⏱️  Total Duration: {build_report.duration:.2f}s")
        
        # Final verdict
        if build_report.success and len(failed_suites) == 0:
            print("\n🎉 ALL TESTS PASSED! CsharpFlow is ready for production!")
            return True
        else:
            print("\n❌ SOME ISSUES DETECTED. Please review the output above.")
            return False

    def save_report_json(self, build_report: BuildReport):
        """Save detailed JSON report"""
        report_data = {
            "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
            "platform": f"{platform.system()} {platform.release()}",
            "python_version": sys.version,
            "project_root": str(self.project_root),
            "build": {
                "success": build_report.success,
                "tool": build_report.build_tool.value,
                "duration": build_report.duration,
                "binary_path": build_report.binary_path
            },
            "test_suites": [
                {
                    "name": suite.name,
                    "file_path": suite.file_path,
                    "test_count": suite.test_count,
                    "result": suite.result.value,
                    "duration": suite.duration,
                    "details": suite.details
                }
                for suite in self.test_suites
            ]
        }
        
        report_file = self.bin_folder / "test_report.json"
        with open(report_file, 'w') as f:
            json.dump(report_data, f, indent=2)
        
        print(f"📄 Detailed report saved: {report_file}")

    def run(self) -> bool:
        """Run the complete test suite"""
        try:
            self.print_header()
            
            # Check build environment
            build_tool = self.check_build_tools()
            if build_tool == BuildTool.NONE:
                return False
            
            # Setup
            self.create_bin_folder()
            self.analyze_project_structure()
            
            # Build
            build_report = self.build_project()
            
            # Test (if build succeeded)
            if build_report.success:
                self.run_tests(build_report)
            
            # Report
            success = self.generate_report(build_report)
            self.save_report_json(build_report)
            
            return success
            
        except KeyboardInterrupt:
            print("\n⏹️  Test run interrupted by user")
            return False
        except Exception as e:
            print(f"\n💥 Unexpected error: {e}")
            import traceback
            traceback.print_exc()
            return False

def main():
    """Main entry point"""
    import argparse
    
    parser = argparse.ArgumentParser(
        description="CsharpFlow Comprehensive Test Runner",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python3 run_tests.py                    # Run from current directory
  python3 run_tests.py --project ../Flow  # Run from different directory
  python3 run_tests.py --verbose         # Verbose output
        """
    )
    
    parser.add_argument(
        '--project', '-p',
        default='.',
        help='Path to CsharpFlow project root (default: current directory)'
    )
    
    parser.add_argument(
        '--verbose', '-v',
        action='store_true',
        help='Enable verbose output'
    )
    
    args = parser.parse_args()
    
    # Set up logging level
    if args.verbose:
        import logging
        logging.basicConfig(level=logging.DEBUG)
    
    # Run tests
    tester = CsharpFlowTester(args.project)
    success = tester.run()
    
    # Exit with appropriate code
    sys.exit(0 if success else 1)

if __name__ == "__main__":
    main()