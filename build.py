#!/usr/bin/env python3
"""
CsharpFlow Build Script for Ubuntu/Linux
Provides easy building with CMake or direct tools
"""

import os
import sys
import shutil
import argparse
import subprocess
from pathlib import Path
from typing import Optional, List

class Colors:
    """ANSI color codes for terminal output"""
    RED = '\033[0;31m'
    GREEN = '\033[0;32m'
    YELLOW = '\033[1;33m'
    BLUE = '\033[0;34m'
    NC = '\033[0m'  # No Color

class BuildScript:
    def __init__(self):
        self.script_dir = Path(__file__).parent.absolute()
        self.bin_dir = self.script_dir / "Bin"
        
    def print_status(self, message: str):
        """Print status message in green"""
        print(f"{Colors.GREEN}[INFO]{Colors.NC} {message}")
        
    def print_warning(self, message: str):
        """Print warning message in yellow"""
        print(f"{Colors.YELLOW}[WARN]{Colors.NC} {message}")
        
    def print_error(self, message: str):
        """Print error message in red"""
        print(f"{Colors.RED}[ERROR]{Colors.NC} {message}")
        
    def check_tool(self, tool_name: str) -> Optional[str]:
        """Check if a tool is available and return its path"""
        tool_path = shutil.which(tool_name)
        if tool_path:
            self.print_status(f"Found {tool_name}: {tool_path}")
            return tool_path
        return None
        
    def run_command(self, command: List[str], cwd: Optional[Path] = None) -> bool:
        """Run a command and return success status"""
        try:
            cmd_str = ' '.join(command)
            self.print_status(f"Running: {cmd_str}")
            
            result = subprocess.run(
                command,
                cwd=cwd or self.script_dir,
                check=True,
                capture_output=False
            )
            return True
            
        except subprocess.CalledProcessError as e:
            self.print_error(f"Command failed with exit code {e.returncode}")
            return False
        except FileNotFoundError:
            self.print_error(f"Command not found: {command[0]}")
            return False
            
    def create_directories(self):
        """Create necessary build directories"""
        self.print_status("Creating build directories...")
        (self.bin_dir / "Debug").mkdir(parents=True, exist_ok=True)
        (self.bin_dir / "Release").mkdir(parents=True, exist_ok=True)
        
    def clean_build(self):
        """Clean previous build artifacts"""
        self.print_status("Cleaning previous build...")
        
        # Remove build directory
        build_dir = self.script_dir / "build"
        if build_dir.exists():
            shutil.rmtree(build_dir)
            
        # Clean Bin directory
        if self.bin_dir.exists():
            shutil.rmtree(self.bin_dir)
            
    def build_with_cmake(self, build_type: str, unity_support: bool) -> bool:
        """Build using CMake"""
        self.print_status("Building with CMake...")
        
        if not self.check_tool("cmake"):
            self.print_error("CMake not found. Please install cmake:")
            print("  sudo apt install cmake")
            return False
            
        # Prepare CMake arguments
        cmake_args = ["-B", "build", f"-DCMAKE_BUILD_TYPE={build_type}"]
        if unity_support:
            cmake_args.append("-DUNITY_BUILD=ON")
            
        # Configure
        self.print_status("Configuring with CMake...")
        if not self.run_command(["cmake"] + cmake_args):
            return False
            
        # Build
        self.print_status("Building with CMake...")
        if not self.run_command(["cmake", "--build", "build", "--config", build_type]):
            return False
            
        return True
        
    def build_direct(self, build_type: str) -> bool:
        """Build directly with .NET tools"""
        self.print_status("Building directly with .NET tools...")
        
        output_dir = str(self.bin_dir / build_type)
        
        # Try different build tools in order of preference
        if self.check_tool("dotnet"):
            self.print_status("Using .NET CLI...")
            
            # Build Flow library
            self.print_status("Building Flow library...")
            if not self.run_command(["dotnet", "build", "Flow.csproj", "-c", build_type, "-o", output_dir]):
                return False
                
            # Build Flow tests
            self.print_status("Building Flow tests...")
            if not self.run_command(["dotnet", "build", "TestFlow/TestFlow.csproj", "-c", build_type, "-o", output_dir]):
                return False
                
        elif self.check_tool("msbuild"):
            self.print_status("Using MSBuild...")
            
            # Build Flow library
            self.print_status("Building Flow library...")
            if not self.run_command([
                "msbuild", "Flow.csproj", 
                f"/p:Configuration={build_type}", 
                f"/p:OutputPath={output_dir}/"
            ]):
                return False
                
            # Build Flow tests
            self.print_status("Building Flow tests...")
            if not self.run_command([
                "msbuild", "TestFlow/TestFlow.csproj",
                f"/p:Configuration={build_type}",
                f"/p:OutputPath={output_dir}/"
            ]):
                return False
                
        elif self.check_tool("xbuild"):
            self.print_warning("Using legacy xbuild (consider upgrading to dotnet or msbuild)...")
            
            # Build Flow library
            self.print_status("Building Flow library...")
            if not self.run_command([
                "xbuild", "Flow.csproj",
                f"/p:Configuration={build_type}",
                f"/p:OutputPath={output_dir}/"
            ]):
                return False
                
            # Build Flow tests
            self.print_status("Building Flow tests...")
            if not self.run_command([
                "xbuild", "TestFlow/TestFlow.csproj",
                f"/p:Configuration={build_type}",
                f"/p:OutputPath={output_dir}/"
            ]):
                return False
                
        else:
            self.print_error("No suitable .NET build tool found!")
            print("Please install one of the following:")
            print("  - .NET SDK: sudo apt install dotnet-sdk-6.0")
            print("  - Mono: sudo apt install mono-devel")
            return False
            
        return True
        
    def show_build_results(self, build_type: str):
        """Show information about built files"""
        build_dir = self.bin_dir / build_type
        
        flow_dll = build_dir / "Flow.dll"
        if flow_dll.exists():
            size = flow_dll.stat().st_size
            self.print_status(f"Flow.dll: {size:,} bytes")
            
        # Look for test binary
        test_files = list(build_dir.glob("TestFlow.*"))
        if test_files:
            test_file = test_files[0]
            size = test_file.stat().st_size
            self.print_status(f"Test binary: {test_file.name} ({size:,} bytes)")
            
    def main(self):
        """Main build script entry point"""
        parser = argparse.ArgumentParser(
            description="CsharpFlow Build Script for Ubuntu/Linux",
            formatter_class=argparse.RawDescriptionHelpFormatter,
            epilog="""
Examples:
  %(prog)s                    # Build in Release mode with CMake
  %(prog)s --debug           # Build in Debug mode
  %(prog)s --no-cmake        # Build directly with dotnet/mono
  %(prog)s --unity --debug   # Build with Unity support in Debug mode
            """
        )
        
        parser.add_argument(
            "-d", "--debug",
            action="store_const",
            const="Debug",
            dest="build_type",
            help="Build in Debug mode"
        )
        
        parser.add_argument(
            "-r", "--release", 
            action="store_const",
            const="Release",
            dest="build_type",
            help="Build in Release mode (default)"
        )
        
        parser.add_argument(
            "--no-cmake",
            action="store_true",
            help="Skip CMake and use direct build tools"
        )
        
        parser.add_argument(
            "-u", "--unity",
            action="store_true",
            help="Enable Unity support"
        )
        
        parser.add_argument(
            "-c", "--clean",
            action="store_true",
            help="Clean build (remove build directory)"
        )
        
        args = parser.parse_args()
        
        # Set defaults
        build_type = args.build_type or "Release"
        use_cmake = not args.no_cmake
        
        self.print_status("CsharpFlow Build Script")
        self.print_status(f"Build Type: {build_type}")
        self.print_status(f"Unity Support: {args.unity}")
        self.print_status(f"Use CMake: {use_cmake}")
        
        # Clean if requested
        if args.clean:
            self.clean_build()
            
        # Create directories
        self.create_directories()
        
        # Build
        success = False
        if use_cmake:
            success = self.build_with_cmake(build_type, args.unity)
        else:
            success = self.build_direct(build_type)
            
        if not success:
            self.print_error("Build failed!")
            return 1
            
        self.print_status("Build completed successfully!")
        self.print_status(f"Binaries are in: Bin/{build_type}/")
        
        # Show results
        self.show_build_results(build_type)
        
        self.print_status("Build script completed successfully!")
        print()
        print("Next steps:")
        print("  - Run tests: python3 run_tests.py")
        print(f"  - Or manually: dotnet test TestFlow/TestFlow.csproj")
        print(f"  - Or with nunit-console: nunit-console Bin/{build_type}/TestFlow.dll")
        
        return 0

if __name__ == "__main__":
    builder = BuildScript()
    sys.exit(builder.main())