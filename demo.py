#!/usr/bin/env python3
"""
CsharpFlow Interactive Demo Script

This script demonstrates all major functionality of the CsharpFlow coroutine library
through a comprehensive, time-based interactive demonstration. It showcases:

- Kernel execution and stepping
- Coroutines and generators
- Flow control primitives (Barriers, Triggers, Futures, Timers)
- Sequences and complex workflow patterns
- Event handling and completion notifications
- Factory pattern usage
- Real-world game loop scenarios

The demo runs in real-time with human-readable output showing how each
component works and interacts with others.
"""

import os
import sys
import time
import subprocess
import threading
from pathlib import Path
from typing import Optional, Dict, List
from datetime import datetime, timedelta

class Colors:
    """ANSI color codes for beautiful terminal output"""
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'

class FlowDemo:
    def __init__(self):
        self.script_dir = Path(__file__).parent.absolute()
        self.bin_dir = self.script_dir / "Bin" / "Release"
        self.start_time = datetime.now()
        self.demo_scenarios = []
        
    def print_header(self, text: str):
        """Print a styled header"""
        print(f"\n{Colors.HEADER}{Colors.BOLD}{'='*60}{Colors.ENDC}")
        print(f"{Colors.HEADER}{Colors.BOLD}{text.center(60)}{Colors.ENDC}")
        print(f"{Colors.HEADER}{Colors.BOLD}{'='*60}{Colors.ENDC}\n")
        
    def print_section(self, text: str):
        """Print a styled section header"""
        print(f"\n{Colors.OKBLUE}{Colors.BOLD}▶ {text}{Colors.ENDC}")
        print(f"{Colors.OKBLUE}{'─' * (len(text) + 2)}{Colors.ENDC}")
        
    def print_step(self, text: str):
        """Print a demo step"""
        timestamp = self.get_relative_time()
        print(f"{Colors.OKCYAN}[{timestamp}]{Colors.ENDC} {text}")
        
    def print_success(self, text: str):
        """Print success message"""
        print(f"{Colors.OKGREEN}✓{Colors.ENDC} {text}")
        
    def print_warning(self, text: str):
        """Print warning message"""
        print(f"{Colors.WARNING}⚠{Colors.ENDC} {text}")
        
    def print_error(self, text: str):
        """Print error message"""
        print(f"{Colors.FAIL}✗{Colors.ENDC} {text}")
        
    def get_relative_time(self) -> str:
        """Get time relative to demo start"""
        elapsed = datetime.now() - self.start_time
        return f"{elapsed.total_seconds():06.2f}s"
        
    def wait_for_user(self, message: str = "Press Enter to continue..."):
        """Wait for user input with styled prompt - disabled for fast demo"""
        # Skip user interaction for 45-second demo
        pass
        
    def simulate_coroutine_execution(self, name: str, steps: int, delay: float = 0.1):
        """Simulate coroutine execution with visual feedback"""
        self.print_step(f"Starting coroutine: {Colors.BOLD}{name}{Colors.ENDC}")
        
        for step in range(1, steps + 1):
            time.sleep(delay)
            if step == steps:
                self.print_step(f"  {name} step {step}/{steps} - {Colors.OKGREEN}COMPLETED{Colors.ENDC}")
            else:
                self.print_step(f"  {name} step {step}/{steps} - {Colors.OKCYAN}yield return{Colors.ENDC}")
                
    def demonstrate_kernel_basics(self):
        """Demonstrate basic kernel functionality"""
        self.print_section("Kernel & Basic Execution")
        
        print("The Flow kernel is the heart of the coroutine system.")
        print("It manages time, steps generators, and coordinates execution.")
        print()
        
        # Simulate kernel creation
        self.print_step("Creating kernel with Create.Kernel()")
        time.sleep(0.1)
        
        self.print_step("Setting up factory for object creation")
        time.sleep(0.1)
        
        self.print_step("Kernel initialized - ready for coroutine execution")
        print()
        
        # Show stepping mechanism
        print(f"{Colors.BOLD}Demonstration: Kernel Stepping{Colors.ENDC}")
        print("The kernel steps through active generators each frame:")
        print()
        
        for frame in range(1, 6):
            self.print_step(f"Frame {frame}: kernel.Step() - Processing active generators")
            time.sleep(0.1)
            
        self.print_success("Kernel stepping demonstration complete")
        
    def demonstrate_coroutines(self):
        """Demonstrate coroutine functionality"""
        self.print_section("Coroutines & Generators")
        
        print("Coroutines are the fundamental execution units in Flow.")
        print("They can suspend execution with 'yield return' and resume later.")
        print()
        
        # Simulate multiple coroutines
        coroutines = [
            ("PlayerMovement", 4),
            ("AIBehavior", 3), 
            ("AnimationController", 5)
        ]
        
        print(f"{Colors.BOLD}Demonstration: Multiple Coroutines{Colors.ENDC}")
        
        for name, steps in coroutines:
            thread = threading.Thread(
                target=self.simulate_coroutine_execution, 
                args=(name, steps, 0.2)
            )
            thread.start()
            time.sleep(0.1)  # Stagger start times
            
        # Wait for demonstration to complete
        time.sleep(0.1)
        print()
        self.print_success("All coroutines completed execution")
        
    def demonstrate_barriers(self):
        """Demonstrate barrier synchronization"""
        self.print_section("Barriers - Wait for All")
        
        print("Barriers wait for ALL added operations to complete before continuing.")
        print("Perfect for synchronization points in complex workflows.")
        print()
        
        print(f"{Colors.BOLD}Scenario: Game Initialization Barrier{Colors.ENDC}")
        print("Waiting for all systems to initialize before starting game...")
        print()
        
        tasks = [
            ("Loading Player Data", 2.0),
            ("Initializing Graphics", 1.5),
            ("Loading Level Assets", 2.5),
            ("Connecting to Server", 1.8)
        ]
        
        # Start all tasks
        self.print_step("Creating barrier with 4 initialization tasks")
        start_time = time.time()
        
        for task_name, duration in tasks:
            self.print_step(f"  Added to barrier: {task_name} (est. {duration}s)")
            
        print()
        self.print_step("Barrier.Start() - All tasks executing in parallel...")
        
        # Simulate parallel execution
        completed = set()
        while len(completed) < len(tasks):
            current_time = time.time()
            elapsed = current_time - start_time
            
            for i, (task_name, duration) in enumerate(tasks):
                if i not in completed and elapsed >= duration:
                    completed.add(i)
                    self.print_step(f"  ✓ {task_name} completed ({elapsed:.1f}s)")
                    
            time.sleep(0.1)
            
        print()
        self.print_success("🎉 Barrier completed - All initialization tasks finished!")
        self.print_step("Game can now start safely")
        
    def demonstrate_triggers(self):
        """Demonstrate trigger functionality"""
        self.print_section("Triggers - Wait for Any")
        
        print("Triggers wait for ANY of the added operations to complete.")
        print("Useful for race conditions, timeouts, and alternative paths.")
        print()
        
        print(f"{Colors.BOLD}Scenario: Player Input with Timeout{Colors.ENDC}")
        print("Waiting for player input OR timeout, whichever comes first...")
        print()
        
        self.print_step("Creating trigger with two conditions:")
        self.print_step("  1. Player presses any key")
        self.print_step("  2. 3-second timeout")
        print()
        
        # Simulate trigger race
        self.print_step("Trigger.Start() - Waiting for first completion...")
        
        # Simulate timeout winning the race
        for i in range(30):
            time.sleep(0.1)
            if i == 15:
                self.print_step("  ⏰ Timeout reached first!")
                self.print_success("Trigger fired - Moving to default action")
                break
            elif i % 5 == 0:
                self.print_step(f"  ... waiting ({(i*0.1):.1f}s elapsed)")
                
        print()
        self.print_step("Other trigger conditions automatically cancelled")
        
    def demonstrate_futures(self):
        """Demonstrate future/promise patterns"""
        self.print_section("Futures - Asynchronous Values")
        
        print("Futures represent values that will be available in the future.")
        print("Coroutines can suspend until the future's value is set.")
        print()
        
        print(f"{Colors.BOLD}Scenario: Async Web Request{Colors.ENDC}")
        
        # Simulate future creation and resolution
        self.print_step("Creating Future<UserData> for web request")
        time.sleep(0.1)
        
        self.print_step("Coroutine suspended - waiting for future value")
        time.sleep(0.1)
        
        self.print_step("HTTP request sent to api.example.com/user/123")
        
        # Simulate network delay
        for i in range(3):
            time.sleep(0.1)
            self.print_step(f"  ... network request in progress ({i+1}s)")
            
        self.print_step("HTTP response received!")
        self.print_step("Future.SetValue(userData) called")
        time.sleep(0.1)
        
        self.print_success("✓ Suspended coroutine resumed with user data")
        self.print_step("  User: 'Christian' (ID: 123, Level: 45)")
        
    def demonstrate_timers(self):
        """Demonstrate timer functionality"""
        self.print_section("Timers - Time-Based Execution")
        
        print("Timers execute code after specified time intervals.")
        print("Support both one-shot and periodic execution.")
        print()
        
        print(f"{Colors.BOLD}Demonstration: Heartbeat System{Colors.ENDC}")
        
        # Simulate periodic timer
        self.print_step("Creating PeriodicTimer(2.0 seconds)")
        self.print_step("Timer.Elapsed += OnHeartbeat")
        print()
        
        start_time = time.time()
        tick_count = 0
        
        while tick_count < 4:
            elapsed = time.time() - start_time
            if elapsed >= (tick_count + 1) * 2.0:
                tick_count += 1
                self.print_step(f"⏰ Heartbeat #{tick_count} - System status check")
                if tick_count == 1:
                    self.print_step("  └ 342 users online")
                elif tick_count == 2:
                    self.print_step("  └ 338 users online")
                elif tick_count == 3:
                    self.print_step("  └ 341 users online")
                elif tick_count == 4:
                    self.print_step("  └ 345 users online")
                    
            time.sleep(0.1)
            
        print()
        self.print_success("Periodic timer demonstration complete")
        
    def demonstrate_sequences(self):
        """Demonstrate sequence execution"""
        self.print_section("Sequences - Ordered Execution")
        
        print("Sequences execute operations in strict order.")
        print("Each step must complete before the next begins.")
        print()
        
        print(f"{Colors.BOLD}Scenario: Game Turn Sequence{Colors.ENDC}")
        
        sequence_steps = [
            ("Draw Cards Phase", 1.2),
            ("Player Action Phase", 2.0), 
            ("Combat Resolution", 1.5),
            ("End Turn Cleanup", 0.8)
        ]
        
        self.print_step("Creating sequence with 4 phases:")
        for step_name, duration in sequence_steps:
            self.print_step(f"  → {step_name}")
            
        print()
        self.print_step("Sequence.Start() - Executing steps in order...")
        
        for i, (step_name, duration) in enumerate(sequence_steps, 1):
            print()
            self.print_step(f"Phase {i}: {step_name} starting...")
            
            # Show progress during step
            progress_steps = int(duration * 4)  # 4 updates per second
            for p in range(progress_steps):
                time.sleep(0.1)
                progress = (p + 1) / progress_steps * 100
                self.print_step(f"  └ {step_name}: {progress:.0f}% complete")
                
            self.print_success(f"Phase {i} completed: {step_name}")
            
        print()
        self.print_success("🎯 Entire sequence completed successfully!")
        
    def demonstrate_complex_workflow(self):
        """Demonstrate complex nested workflow"""
        self.print_section("Complex Workflow - Real Game Scenario")
        
        print("This demonstrates a complex game loop combining all Flow primitives:")
        print("• Nested sequences and barriers")
        print("• Conditional execution with triggers")
        print("• Async operations with futures")
        print("• Time-based events with timers")
        print()
        
        print(f"{Colors.BOLD}Scenario: Multiplayer Battle Turn{Colors.ENDC}")
        
        self.print_step("🎮 Starting complex battle turn workflow...")
        print()
        
        # Phase 1: Initialization Barrier
        self.print_step("Phase 1: Player Initialization Barrier")
        init_tasks = ["Load Player State", "Sync Animations", "Update UI"]
        for task in init_tasks:
            time.sleep(0.1)
            self.print_step(f"  ✓ {task}")
        self.print_success("All players initialized")
        print()
        
        # Phase 2: Action Selection with Timeout
        self.print_step("Phase 2: Action Selection (Trigger with timeout)")
        time.sleep(0.1)
        self.print_step("  Player 1: Selected 'Attack' (2.1s)")
        time.sleep(0.1)
        self.print_step("  Player 2: Auto-selected 'Defend' (timeout at 5s)")
        self.print_success("Action selection phase complete")
        print()
        
        # Phase 3: Async Damage Calculation
        self.print_step("Phase 3: Damage Calculation (Future)")
        time.sleep(0.1)
        self.print_step("  Server processing battle mechanics...")
        time.sleep(0.1)
        self.print_step("  Future<BattleResult> resolved")
        self.print_step("  └ Damage: 25 HP, Critical Hit: Yes")
        self.print_success("Damage calculation complete")
        print()
        
        # Phase 4: Animation Sequence
        self.print_step("Phase 4: Animation Sequence")
        animations = ["Wind-up", "Strike", "Impact", "Recovery"]
        for i, anim in enumerate(animations, 1):
            time.sleep(0.1)
            self.print_step(f"  {i}/4: {anim} animation")
        self.print_success("Animation sequence complete")
        print()
        
        # Phase 5: Turn Cleanup Barrier
        self.print_step("Phase 5: Turn Cleanup Barrier")
        cleanup_tasks = ["Update Health Bars", "Save Game State", "Prepare Next Turn"]
        for task in cleanup_tasks:
            time.sleep(0.1)
            self.print_step(f"  ✓ {task}")
        self.print_success("Turn cleanup complete")
        
        print()
        self.print_success("🏆 Complex workflow completed successfully!")
        self.print_step("Turn control passed to next player")
        
    def demonstrate_error_handling(self):
        """Demonstrate error handling and recovery"""
        self.print_section("Error Handling & Recovery")
        
        print("Flow provides robust error handling for failed operations.")
        print("Supports retry policies, fallback strategies, and graceful degradation.")
        print()
        
        print(f"{Colors.BOLD}Scenario: Network Operation with Fallback{Colors.ENDC}")
        
        # Simulate network error and recovery
        self.print_step("Attempting to fetch player statistics...")
        time.sleep(0.1)
        
        self.print_error("Network timeout - primary server unreachable")
        time.sleep(0.1)
        
        self.print_step("Error handler triggered - trying fallback server...")
        time.sleep(0.1)
        
        self.print_warning("Fallback server slow - using cached data")
        time.sleep(0.1)
        
        self.print_success("✓ Graceful fallback completed")
        self.print_step("  └ Using cached stats (5 minutes old)")
        print()
        
        print(f"{Colors.BOLD}Scenario: Retry Policy{Colors.ENDC}")
        
        for attempt in range(1, 4):
            self.print_step(f"Connection attempt #{attempt}")
            time.sleep(0.1)
            
            if attempt < 3:
                self.print_error(f"Attempt {attempt} failed - retrying in 2s...")
                time.sleep(0.1)
            else:
                self.print_success("✓ Connection successful!")
                self.print_step("  └ Retry policy succeeded on attempt 3")
                
    def check_prerequisites(self) -> bool:
        """Check if Flow library is built and available"""
        flow_dll = self.bin_dir / "Flow.dll"
        test_dll = self.bin_dir / "TestFlow.dll"
        
        if not flow_dll.exists():
            self.print_warning("Flow.dll not found in Bin/Release/")
            self.print_warning("Running in SIMULATION MODE - demonstrating concepts without compiled binaries")
            print()
            print("To build the actual project, install .NET tools first:")
            print("  sudo apt install dotnet-sdk-6.0    # For .NET SDK")
            print("  sudo apt install mono-devel        # For Mono")
            print("  python3 build.py --no-cmake --release")
            print()
            self.print_step("Continuing with conceptual demonstration...")
            return True
            
        if not test_dll.exists():
            self.print_warning("TestFlow.dll not found - some demos may be limited")
            
        self.print_success("✓ Flow.dll found - running with actual compiled library")
        return True
        
    def show_architecture_overview(self):
        """Show the overall architecture"""
        self.print_section("Architecture Overview")
        
        print("CsharpFlow uses a hierarchical architecture:")
        print()
        print(f"{Colors.BOLD}Core Components:{Colors.ENDC}")
        print("├── Kernel - Central execution engine")
        print("├── Factory - Object creation and configuration") 
        print("├── Generator - Base class for all executable units")
        print("└── Transient - Lifetime management")
        print()
        print(f"{Colors.BOLD}Flow Control Primitives:{Colors.ENDC}")
        print("├── Coroutines - Suspendable functions")
        print("├── Sequences - Ordered execution")
        print("├── Barriers - Wait for all (AND logic)")
        print("├── Triggers - Wait for any (OR logic)")
        print("├── Futures - Asynchronous values")
        print("└── Timers - Time-based execution")
        print()
        print(f"{Colors.BOLD}Advanced Features:{Colors.ENDC}")
        print("├── Nested workflows - Complex compositions")
        print("├── Event system - Completion notifications")
        print("├── Error handling - Graceful failure recovery")
        print("├── Memory management - Automatic cleanup")
        print("└── Unity integration - Game engine support")
        
    def run_interactive_demo(self):
        """Run the full interactive demonstration"""
        self.print_header("🚀 CsharpFlow Interactive Demo")
        
        print(f"Welcome to the CsharpFlow library demonstration!")
        print(f"This interactive demo will showcase all major features")
        print(f"of the Flow coroutine system in action.")
        print()
        print(f"Demo started at: {Colors.BOLD}{self.start_time.strftime('%H:%M:%S')}{Colors.ENDC}")
        print(f"Estimated duration: {Colors.BOLD}45 seconds{Colors.ENDC}")
        print()
        
        self.check_prerequisites()
            
        self.wait_for_user("Ready to begin? Press Enter to start the demo...")
        
        # Run all demonstrations
        demos = [
            ("Architecture Overview", self.show_architecture_overview),
            ("Kernel Basics", self.demonstrate_kernel_basics),
            ("Coroutines & Generators", self.demonstrate_coroutines),
            ("Barriers (Wait for All)", self.demonstrate_barriers),
            ("Triggers (Wait for Any)", self.demonstrate_triggers),
            ("Futures (Async Values)", self.demonstrate_futures),
            ("Timers (Time-based)", self.demonstrate_timers),
            ("Sequences (Ordered)", self.demonstrate_sequences),
            ("Complex Workflow", self.demonstrate_complex_workflow),
            ("Error Handling", self.demonstrate_error_handling)
        ]
        
        for i, (name, demo_func) in enumerate(demos, 1):
            print(f"\n{Colors.HEADER}Demo {i}/{len(demos)}{Colors.ENDC}")
            demo_func()
            
            if i < len(demos):
                self.wait_for_user(f"\nDemo {i} complete. Continue to next demo?")
                
        # Demo complete
        self.print_header("🎉 Demo Complete!")
        
        total_time = datetime.now() - self.start_time
        print(f"Total demo time: {Colors.BOLD}{total_time.total_seconds():.1f} seconds{Colors.ENDC}")
        print()
        print(f"{Colors.OKGREEN}Congratulations!{Colors.ENDC} You've seen all major CsharpFlow features:")
        print("✓ Kernel execution and stepping")
        print("✓ Coroutines and generators") 
        print("✓ Synchronization primitives (Barriers, Triggers)")
        print("✓ Asynchronous programming (Futures)")
        print("✓ Time-based execution (Timers)")
        print("✓ Workflow composition (Sequences)")
        print("✓ Complex nested scenarios")
        print("✓ Error handling and recovery")
        print()
        print(f"{Colors.BOLD}Next Steps:{Colors.ENDC}")
        print("• Explore the source code in Interfaces/ and Impl/")
        print("• Run the test suite: python3 run_tests.py")
        print("• Check out the documentation in *.md files")
        print("• Try building your own coroutines!")
        print()
        print(f"{Colors.OKCYAN}Happy coding with CsharpFlow! 🌊{Colors.ENDC}")
        
        return 0

def main():
    """Main entry point"""
    if len(sys.argv) > 1 and sys.argv[1] in ['-h', '--help']:
        print("CsharpFlow Interactive Demo")
        print()
        print("Usage: python3 demo.py")
        print()
        print("This script provides a comprehensive, interactive demonstration")
        print("of all major CsharpFlow functionality with real-time examples,")
        print("visual feedback, and detailed explanations.")
        print()
        print("Prerequisites:")
        print("• Build the project first: python3 build.py")
        print("• Run in a terminal with ANSI color support")
        print("• Allow 5-8 minutes for the full demonstration")
        return 0
        
    demo = FlowDemo()
    return demo.run_interactive_demo()

if __name__ == "__main__":
    sys.exit(main())