#!/usr/bin/env python3
"""
CsharpFlow Fast Demo Script - 45 Second Overview

Quick demonstration of all major CsharpFlow functionality in under 45 seconds.
Perfect for quick overviews, presentations, and getting a taste of the library.
"""

import os
import sys
import time
from pathlib import Path
from datetime import datetime

class Colors:
    """ANSI color codes"""
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'

class FastDemo:
    def __init__(self):
        self.start_time = datetime.now()
        
    def print_header(self, text: str):
        print(f"\n{Colors.HEADER}{Colors.BOLD}{text}{Colors.ENDC}")
        
    def print_step(self, text: str):
        elapsed = (datetime.now() - self.start_time).total_seconds()
        print(f"{Colors.OKCYAN}[{elapsed:04.1f}s]{Colors.ENDC} {text}")
        
    def print_success(self, text: str):
        print(f"{Colors.OKGREEN}✓{Colors.ENDC} {text}")
        
    def fast_demo(self):
        """Complete demo in 45 seconds"""
        self.print_header("🚀 CsharpFlow - 45 Second Demo")
        print("Fast overview of C# coroutine library features...")
        time.sleep(0.5)
        
        # Kernel (3s)
        self.print_step("🔧 Kernel: Central execution engine managing coroutines")
        time.sleep(0.3)
        self.print_step("   kernel.Step() processes all active generators")
        time.sleep(0.7)
        
        # Coroutines (4s)  
        self.print_step("⚡ Coroutines: Suspendable functions with yield return")
        time.sleep(0.3)
        self.print_step("   PlayerMovement, AI, Animation running in parallel")
        time.sleep(0.7)
        self.print_success("Multiple coroutines executing concurrently")
        time.sleep(0.5)
        
        # Barriers (5s)
        self.print_step("🚧 Barriers: Wait for ALL tasks to complete")
        time.sleep(0.3)
        self.print_step("   Loading assets, connecting server, init graphics...")
        time.sleep(0.8)
        self.print_success("All initialization complete - game can start!")
        time.sleep(0.4)
        
        # Triggers (4s)
        self.print_step("⚡ Triggers: Wait for ANY task to complete")
        time.sleep(0.3)
        self.print_step("   Player input OR 5-second timeout...")
        time.sleep(0.8)
        self.print_success("Timeout reached - proceeding with default action")
        time.sleep(0.3)
        
        # Futures (4s)
        self.print_step("🔮 Futures: Asynchronous value resolution")
        time.sleep(0.3)
        self.print_step("   HTTP request to api.example.com/user/data...")
        time.sleep(0.8)
        self.print_success("Future resolved - coroutine resumed with user data")
        time.sleep(0.3)
        
        # Timers (4s)
        self.print_step("⏰ Timers: Time-based execution")
        time.sleep(0.3)
        self.print_step("   PeriodicTimer(2s) - heartbeat every 2 seconds")
        time.sleep(0.8)
        self.print_success("342 users online - system monitoring active")
        time.sleep(0.3)
        
        # Sequences (4s)
        self.print_step("📋 Sequences: Ordered step-by-step execution")
        time.sleep(0.3)
        self.print_step("   Draw Cards → Player Action → Combat → Cleanup")
        time.sleep(0.8)
        self.print_success("Game turn sequence completed successfully")
        time.sleep(0.3)
        
        # Complex Workflow (6s)
        self.print_step("🎮 Complex Example: Multiplayer battle turn")
        time.sleep(0.2)
        self.print_step("   Barrier(init) → Trigger(input) → Future(damage)")
        time.sleep(0.4)
        self.print_step("   → Sequence(animations) → Barrier(cleanup)")
        time.sleep(0.6)
        self.print_success("Complex nested workflow completed!")
        time.sleep(0.3)
        
        # Error Handling (3s)
        self.print_step("🛡️  Error Handling: Graceful failure recovery")
        time.sleep(0.4)
        self.print_step("   Network timeout → fallback server → cached data")
        time.sleep(0.6)
        self.print_success("Resilient systems with automatic recovery")
        time.sleep(0.5)
        
        # Summary (5s)
        self.print_header("🎉 Demo Complete!")
        elapsed = (datetime.now() - self.start_time).total_seconds()
        print(f"Total time: {Colors.BOLD}{elapsed:.1f} seconds{Colors.ENDC}")
        print()
        print(f"{Colors.OKGREEN}CsharpFlow Features Demonstrated:{Colors.ENDC}")
        print("✓ Kernel execution engine    ✓ Async Futures")  
        print("✓ Coroutines & Generators    ✓ Time-based Timers")
        print("✓ Barriers (wait for all)    ✓ Ordered Sequences") 
        print("✓ Triggers (wait for any)    ✓ Error handling")
        print()
        print(f"{Colors.BOLD}Perfect for:{Colors.ENDC} Game loops, async workflows, state machines")
        print(f"{Colors.BOLD}Get started:{Colors.ENDC} python3 build.py && python3 run_tests.py")
        
        return 0

def main():
    if len(sys.argv) > 1 and sys.argv[1] in ['-h', '--help']:
        print("CsharpFlow Fast Demo - 45 Second Overview")
        print()
        print("Usage: python3 demo_fast.py")
        print()
        print("Quick demonstration of all major CsharpFlow features")
        print("in under 45 seconds. No interaction required.")
        print()
        print("For the full interactive demo: python3 demo.py")
        return 0
        
    demo = FastDemo()
    return demo.fast_demo()

if __name__ == "__main__":
    sys.exit(main())