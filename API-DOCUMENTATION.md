# Flow API Documentation
## Overview
This document provides comprehensive API documentation for the Flow coroutine library, including interface contracts, implementation details, and usage patterns.
## Core Interfaces
### IKernel - Execution Engine
```mermaid
classDiagram
class IKernel {
<<interface>>
+EDebugLevel DebugLevel
+ILogger Log
+bool Break
+INode Root
+IFactory Factory
+ITimeFrame Time
+Update(float deltaSeconds)
+Wait(TimeSpan duration)
+BreakFlow()
}
class IGenerator {
<<interface>>
+object Value
+bool Running
+int StepNumber
+Step()
+Suspend()
+Resume()
+ResumeAfter(ITransient)
+SuspendAfter(ITransient)
}
class ITransient {
<<interface>>
+bool Active
+IKernel Kernel
+string Name
+Complete()
+CompleteAfter(ITransient)
+Then(Action)
+event TransientHandler Completed
}
IKernel --> IGenerator : manages
IGenerator --> ITransient : inherits
```
### IFactory - Object Creation
```mermaid
classDiagram
class IFactory {
<<interface>>
+IKernel Kernel
+INode Node(params IGenerator[])
+IGroup Group(params ITransient[])
+IGenerator Do(Action)
+ISequence Sequence(params IGenerator[])
+IBarrier Barrier(params ITransient[])
+ITrigger Trigger(params ITransient[])
+IFuture~T~ Future~T~()
+ITimer OneShotTimer(TimeSpan)
+IPeriodic PeriodicTimer(TimeSpan)
+ICoroutine Coroutine(Func...)
+ISubroutine~T~ Subroutine~T~(Func...)
}
IFactory --> INode : creates
IFactory --> IBarrier : creates
IFactory --> ITrigger : creates
IFactory --> IFuture : creates
IFactory --> ITimer : creates
IFactory --> ICoroutine : creates
```
## Flow Control Primitives
### Synchronization Primitives
```mermaid
graph TB
subgraph "Barrier - Wait for All"
B[Barrier] --> T1[Task 1]
B --> T2[Task 2]
B --> T3[Task 3]
T1 --> |Complete| BC[All Complete]
T2 --> BC
T3 --> BC
BC --> CONT1[Continue]
end
subgraph "Trigger - Wait for Any"
TR[Trigger] --> TT1[Task 1]
TR --> TT2[Task 2] 
TR --> TT3[Task 3]
TT1 --> |Complete| TC[Any Complete]
TT2 --> TC
TT3 --> TC
TC --> CONT2[Continue]
end
```
### Timing Primitives
```mermaid
classDiagram
class ITimer {
<<interface>>
+TimeSpan Interval
+event TransientHandler Elapsed
+Start()
+Stop()
+Reset()
}
class IPeriodic {
<<interface>>
+TimeSpan Period
+int TickCount
+event TransientHandler Tick
}
class ITimedBarrier {
<<interface>>
+TimeSpan TimeOut
+bool TimedOut
+event TransientHandler TimedOut
}
class ITimedFuture {
<<interface>>
+TimeSpan TimeOut
+bool Available
+T Value
}
ITimer <|-- IPeriodic
IBarrier <|-- ITimedBarrier
IFuture <|-- ITimedFuture
```
## Execution Flow Diagrams
### Kernel Execution Cycle
```mermaid
sequenceDiagram
participant App as Application
participant K as Kernel
participant R as Root Node
participant G1 as Generator 1
participant G2 as Generator 2
App->>K: Update(deltaTime)
K->>K: UpdateTime(deltaTime)
K->>K: Process()
K->>R: Step()
R->>G1: Step()
G1-->>R: yield/complete
R->>G2: Step()
G2-->>R: yield/complete
R-->>K: Step complete
K-->>App: Update complete
```
### Generator Lifecycle Management
```mermaid
stateDiagram-v2
[*] --> Created: Factory.Create()
Created --> Active: Add to Node
Active --> Running: Resume()
Running --> Stepping: Step()
Stepping --> Running: Continue execution
Stepping --> Suspended: yield return
Suspended --> Running: Resume()
Running --> Completed: Complete()
Completed --> [*]: Cleanup
Active --> Inactive: Remove from Node
Suspended --> Inactive: Remove from Node
Inactive --> [*]: Garbage Collection
```
### Event Propagation
```mermaid
graph TD
E[Event Occurs] --> H1[Handler 1]
E --> H2[Handler 2]
E --> H3[Handler 3]
H1 --> |May Trigger| C1[Completion 1]
H2 --> |May Trigger| C2[Completion 2]
H3 --> |May Trigger| C3[Completion 3]
C1 --> |Propagates| P[Parent Complete]
C2 --> P
C3 --> P
P --> |Chain Reaction| RC[Related Components]
RC --> |Continue| NEXT[Next Operations]
```
## Advanced Patterns
### Coroutine Composition
```mermaid
graph LR
subgraph "Sequence Pattern"
SEQ[Sequence] --> C1[Coroutine 1]
C1 --> |Complete| C2[Coroutine 2]
C2 --> |Complete| C3[Coroutine 3]
C3 --> |Complete| DONE1[Sequence Complete]
end
subgraph "Parallel Pattern" 
PAR[Barrier] --> P1[Parallel 1]
PAR --> P2[Parallel 2]
PAR --> P3[Parallel 3]
P1 --> |All Complete| DONE2[Barrier Complete]
P2 --> DONE2
P3 --> DONE2
end
subgraph "Conditional Pattern"
COND{Condition} --> |True| TRUE_PATH[True Branch]
COND --> |False| FALSE_PATH[False Branch]
TRUE_PATH --> DONE3[Continue]
FALSE_PATH --> DONE3
end
```
### Future/Promise Chain
```mermaid
sequenceDiagram
participant C as Consumer
participant F1 as Future<Input>
participant P as Processor 
participant F2 as Future<Output>
participant R as Result Handler
C->>F1: Create input future
C->>P: Start processing
P->>F1: ResumeAfter(inputFuture)
Note over P: Processor suspended
C->>F1: Set input value
F1->>P: Resume processor
P->>P: Process input
P->>F2: Set output value
F2->>R: Notify result available
R->>F2: Get result value
```
## Error Handling Patterns
```mermaid
graph TD
START[Start Operation] --> TRY{Try Execute}
TRY --> |Success| SUCCESS[Success]
TRY --> |Exception| CATCH[Catch Exception]
CATCH --> LOG[Log Error]
LOG --> RETRY{Retry?}
RETRY --> |Yes| WAIT[Wait Interval]
WAIT --> TRY
RETRY --> |No| FALLBACK[Execute Fallback]
FALLBACK --> FB_SUCCESS[Fallback Success]
FALLBACK --> FB_FAIL[Fallback Fail]
SUCCESS --> END[Complete]
FB_SUCCESS --> END
FB_FAIL --> ERROR[Error State]
```
## Best Practices
### Resource Management
```mermaid
graph TB
CREATE[Create Resource] --> USE[Use Resource]
USE --> |Auto| DISPOSE[Dispose on Complete]
USE --> |Manual| CLEANUP[Manual Cleanup]
DISPOSE --> |Event| HANDLERS[Completion Handlers]
CLEANUP --> HANDLERS
HANDLERS --> |Propagate| PARENT[Parent Cleanup]
PARENT --> GC[Garbage Collection]
```
### Performance Optimization
1. **Minimize Allocations**: Reuse generators and objects where possible
2. **Efficient Stepping**: Avoid deep nesting of sequences
3. **Memory Management**: Use completion events for cleanup
4. **Verbose Logging**: Check verbosity levels before expensive operations
### Thread Safety
Flow assumes single-threaded execution. For multi-threaded scenarios:
```mermaid
graph LR
MT[Multi-threaded App] --> SYNC[Synchronization Layer]
SYNC --> SINGLE[Single Thread]
SINGLE --> FLOW[Flow Kernel]
FLOW --> RESULT[Results]
RESULT --> SYNC
SYNC --> MT
```
The synchronization layer should marshal all Flow operations to a single thread to maintain consistency.