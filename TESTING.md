# Flow Testing Documentation
## Overview
The Flow library includes a comprehensive test suite located in `TestFlow/Editor/` that validates all major components and flow control patterns. The tests are designed to run in both Unity's Test Runner and standard NUnit environments.
## Test Architecture
```mermaid
graph TB
subgraph "Test Structure"
TB[TestBase] --> TK[TestKernel]
TB --> TBar[TestBarrier]
TB --> TC[TestChannel]
TB --> TT[TestTrigger]
TB --> TTim[TestTimers]
TB --> TL[TestLog]
TB --> TLoop[TestLoops]
TB --> TCond[TestConditionals]
TB --> TRes[TestResumeAfter]
TB --> TSeq[TestSequenceTopology]
TB --> TFlow[TestFlowExtra]
TB --> TEvent[TestEventStream]
TB --> TAdv[TestAdvancedScenarios]
end
TB --> |Provides| Util[Test Utilities]
TB --> |Creates| Kern[Test Kernels]
TB --> |Manages| Setup[Setup/Teardown]
subgraph "Advanced Test Coverage"
TAdv --> |20 Tests| Complex[Complex Scenarios]
Complex --> Nested[Nested Barriers]
Complex --> Channels[Producer/Consumer]
Complex --> Error[Error Handling]
Complex --> Memory[Memory Cleanup]
Complex --> Timing[Advanced Timing]
Complex --> Factory[Factory Patterns]
Complex --> Flow[Complex Flows]
end
```
## Test Categories
### Core System Tests
#### TestKernel.cs
Tests the fundamental execution engine functionality:
```mermaid
sequenceDiagram
participant T as Test
participant K as Kernel
participant C as Coroutine
participant F as Future
T->>K: Create.Kernel()
T->>C: Factory.Coroutine(TestMethod)
K->>K: Root.Add(coroutine)
loop Kernel Steps
T->>K: Step()
K->>C: Step()
C-->>K: yield return value
end
T->>T: Assert coroutine values
T->>T: Assert coroutine completed
```
**Key Test Scenarios:**
- Coroutine lifecycle (creation, execution, completion)
- Future value setting and availability
- Generator stepping and value progression
- Kernel state management
#### TestBarrier.cs
Validates synchronization primitive behavior:
```mermaid
graph TD
START[Test Start] --> CREATE[Create Barrier]
CREATE --> ADD1[Add Task 1]
ADD1 --> ADD2[Add Task 2]
ADD2 --> ADD3[Add Task 3]
ADD3 --> STEP1[Step - None Complete]
STEP1 --> |Assert: Barrier Active| COMP1[Complete Task 1]
COMP1 --> STEP2[Step - 1 Complete]
STEP2 --> |Assert: Barrier Active| COMP2[Complete Task 2]
COMP2 --> STEP3[Step - 2 Complete]
STEP3 --> |Assert: Barrier Active| COMP3[Complete Task 3]
COMP3 --> STEP4[Step - All Complete]
STEP4 --> |Assert: Barrier Complete| END[Test End]
```
#### TestChannel.cs
Tests inter-generator communication:
```mermaid
sequenceDiagram
participant P as Producer
participant Ch as Channel
participant C as Consumer
participant T as Test
T->>Ch: Create Channel
T->>P: Create Producer
T->>C: Create Consumer
P->>Ch: Write(value1)
P->>Ch: Write(value2)
C->>Ch: Read()
Ch->>C: Return value1
T->>T: Assert value1 received
C->>Ch: Read()
Ch->>C: Return value2
T->>T: Assert value2 received
```
### Flow Control Tests
#### TestTrigger.cs
Validates "any of" completion semantics:
```mermaid
stateDiagram-v2
[*] --> WaitingForAny: Create Trigger with 3 tasks
WaitingForAny --> FirstComplete: Task 1 completes
FirstComplete --> TriggerFired: Trigger completes immediately
TriggerFired --> [*]: Other tasks cancelled/ignored
note right of FirstComplete: Tests that trigger fires on first completion
```
#### TestTimers.cs
Tests time-based execution patterns:
```mermaid
timeline
title Timer Test Execution Flow
section Setup
T0 : Create Timer(100ms)
: Start Timer
: Assert Active
section Execution
T50 : Step at 50ms
: Assert Still Active
T100 : Step at 100ms
: Assert Elapsed Event
: Assert Timer Complete
section Cleanup
T150 : Verify No Further Events
: Test Complete
```
### Advanced Pattern Tests
#### TestSequenceTopology.cs
Tests complex nested sequence patterns:
```mermaid
graph TB
subgraph "Nested Sequence Test"
S1[Main Sequence] --> S2[Sub-Sequence 1]
S1 --> S3[Sub-Sequence 2]
S2 --> A1[Action A]
S2 --> A2[Action B]
S3 --> B1[Barrier]
B1 --> B2[Parallel Task 1]
B1 --> B3[Parallel Task 2]
end
S1 --> |Verify Order| ASSERT[Assert Execution Order]
ASSERT --> |Check State| COMPLETE[All Complete]
```
#### TestResumeAfter.cs
Tests dependency-based execution control:
```mermaid
sequenceDiagram
participant G1 as Generator 1
participant G2 as Generator 2
participant T as Test
T->>G1: Create and Start
T->>G2: Create Generator 2
T->>G2: ResumeAfter(Generator 1)
Note over G2: Generator 2 Suspended
loop Steps
T->>G1: Step()
T->>G2: Step() - No Effect
end
G1->>G1: Complete()
G1->>G2: Resume Signal
Note over G2: Generator 2 Resumed
T->>G2: Step() - Now Active
T->>T: Assert correct sequencing
```
## Test Execution Patterns
### Synchronous Testing
```mermaid
graph LR
CREATE[Create Components] --> SETUP[Setup Test State]
SETUP --> EXECUTE[Execute Steps Manually]
EXECUTE --> ASSERT[Assert Expected State]
ASSERT --> CLEANUP[Cleanup Resources]
```
### Asynchronous Pattern Testing
```mermaid
graph TD
ASYNC_START[Start Async Operation] --> POLL{Poll for Completion}
POLL --> |Not Ready| STEP[Step Kernel]
STEP --> POLL
POLL --> |Complete| VERIFY[Verify Results]
VERIFY --> TIMEOUT{Timeout Check}
TIMEOUT --> |Within Limit| SUCCESS[Test Success]
TIMEOUT --> |Exceeded| FAIL[Test Failure]
```
## Running Tests
### Unity Environment
```csharp
// Tests run automatically in Unity Test Runner
// Located in TestFlow/Editor/ for Unity compatibility
// Use Window > General > Test Runner in Unity
```
### Standalone NUnit
```bash
# Run via NUnit test runner
nunit-console TestFlow.dll
# Or via Visual Studio Test Explorer
# Build solution and run all tests
```
### Test Configuration
```mermaid
graph TB
subgraph "Test Configuration"
CONFIG[TestBase Configuration] --> TIMEOUT[Test Timeouts]
CONFIG --> VERBOSITY[Logging Verbosity]
CONFIG --> CLEANUP[Auto Cleanup]
TIMEOUT --> |30 seconds| DEFAULT[Default Timeout]
VERBOSITY --> |Level 5| STANDARD[Standard Logging]
CLEANUP --> |Always| AUTO[Auto Dispose]
end
```
## Test Coverage Matrix
| Component | Unit Tests | Integration Tests | Performance Tests | Advanced Scenarios |
|-----------|------------|-------------------|-------------------|--------------------|
| Kernel | Basic execution | Multi-generator | Limited | Break mechanism |
| Generators | Lifecycle | Nesting | Limited | Suspend/Resume |
| Barriers | Synchronization | Timeout handling | None | Nested barriers |
| Triggers | Any-completion | Complex scenarios | None | Multiple sources |
| Futures | Value setting | Chaining | None | Timed futures |
| Timers | Basic timing | Periodic behavior | Limited | Multiple ticks |
| Channels | Read/Write | Producer/Consumer | None | Advanced patterns |
| Sequences | Ordering | Nested sequences | None | Deep nesting |
| Logging | Basic output | Limited | None | Limited |
| Factory | Object creation | Limited | None | All patterns |
| Error Handling | Basic | Limited | None | Coroutine errors |
| Memory Management | Basic cleanup | None | None | Completion cleanup |
**Legend:**
- Comprehensive coverage
- Partial coverage 
- No coverage
## Common Test Patterns
### Setup Pattern
```csharp
[SetUp]
public void SetUp()
{
kernel = Create.Kernel();
kernel.Log.Verbosity = 0; // Quiet during tests
factory = kernel.Factory;
}
```
### Assertion Helpers
```csharp
protected void AssertCompleted(ITransient transient, string context = "")
{
Assert.IsFalse(transient.Active, $"Expected {context} to be completed");
}
protected void AssertActive(ITransient transient, string context = "")
{
Assert.IsTrue(transient.Active, $"Expected {context} to be active");
}
```
### Timeout Protection
```csharp
protected void StepUntilComplete(ITransient target, int maxSteps = 1000)
{
int steps = 0;
while (target.Active && steps < maxSteps)
{
kernel.Step();
steps++;
}
Assert.IsFalse(target.Active, $"Operation did not complete within {maxSteps} steps");
}
```
### Advanced Scenario Tests
#### TestAdvancedScenarios.cs (20 New Tests)
Comprehensive test suite covering complex scenarios and edge cases:
**Nested and Complex Flow Tests:**
- `TestNestedBarriersWithTimeout()` - Tests complex barrier hierarchies with timeout handling
- `TestComplexSequenceWithConditionals()` - Validates conditional execution within sequences
- `TestDeepNestedSequences()` - Tests deeply nested sequence structures
- `TestComplexFlowWithAllPrimitives()` - Integration test using all flow control types
**Producer/Consumer and Communication:**
- `TestChannelProducerConsumerPattern()` - Advanced channel communication patterns
- `TestConcurrentBarrierAndTrigger()` - Tests interaction between barriers and triggers
**Timing and Synchronization:**
- `TestPeriodicTimerWithMultipleTicks()` - Multi-tick periodic timer validation
- `TestTimedFutureTimeout()` - Timed future timeout behavior
- `TestWaitMechanism()` - Kernel wait functionality
**Error Handling and Edge Cases:**
- `TestErrorHandlingInCoroutines()` - Exception handling within coroutines
- `TestKernelBreakMechanism()` - Kernel break functionality
- `TestMemoryCleanupOnCompletion()` - Resource cleanup validation
**Factory and API Patterns:**
- `TestFactoryCreationPatterns()` - Comprehensive factory method testing
- `TestFluentInterfaceChaining()` - Fluent API pattern validation
- `TestValueExpression()` - Value and expression generator testing
**Advanced Dependency Management:**
- `TestGeneratorSuspendResumeWithDependencies()` - Complex dependency scenarios
- `TestTriggerWithMultipleCompletionSources()` - Multi-source trigger patterns
- `TestActionSequencePattern()` - Action sequence functionality
- `TestGeneratorNamingAndDebugging()` - Debug and naming support
## Known Test Limitations
1. **Performance Benchmarks**: No automated performance regression testing
2. **Stress Testing**: Limited high-load scenario coverage (improved with new tests)
3. **Memory Leak Detection**: Basic cleanup testing (enhanced with new memory tests)
4. **Timing Sensitivity**: Timer tests may be sensitive to execution timing
5. **Thread Safety**: No multi-threaded testing scenarios
## Contributing Tests
When adding new tests, follow these patterns:
```mermaid
graph TD
NEW[New Feature] --> TEST[Write Test First]
TEST --> IMPL[Implement Feature]
IMPL --> VERIFY[Verify Test Passes]
VERIFY --> EDGE[Add Edge Cases]
EDGE --> DOC[Document Test]
DOC --> REVIEW[Code Review]
```
### Test Naming Convention
- `Test[ComponentName]` for component tests
- `Test[Feature][Scenario]` for specific scenarios
- Use descriptive method names: `TestBarrierCompletesWhenAllTasksFinish()`
### Test Structure
1. **Arrange**: Set up test conditions
2. **Act**: Execute the operation
3. **Assert**: Verify expected outcomes
4. **Cleanup**: Dispose resources (handled by TestBase)