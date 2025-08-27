# Flow - C# Coroutine Kernel ![Foo](flow-small.jpg)

[![Build status](https://ci.appveyor.com/api/projects/status/github/cschladetsch/flow?svg=true)](https://ci.appveyor.com/project/cschladetsch/flow)
[![CodeFactor](https://www.codefactor.io/repository/github/cschladetsch/flow/badge)](https://www.codefactor.io/repository/github/cschladetsch/flow)
[![License](https://img.shields.io/github/license/cschladetsch/flow.svg?label=License&maxAge=86400)](./LICENSE.txt)
![Release](https://img.shields.io/github/release/cschladetsch/flow.svg?label=Release&maxAge=60)

A comprehensive C# coroutine-based kernel library for .NET that provides cooperative multitasking and flow control primitives. Flow enables developers to build complex async workflows using coroutines, futures, barriers, triggers, and timers.

## Key Features

- **Coroutine-based execution** - Cooperative multitasking without thread overhead
- **Rich flow control primitives** - Barriers, Triggers, Futures, Timers, Sequences
- **Unity Integration** - Full compatibility with Unity 4.0+ and modern versions
- **Event-driven architecture** - Comprehensive completion and timing event system
- **Type-safe generics** - Strongly typed futures and generators
- **Flexible factory pattern** - Easy creation and configuration of flow objects

## Quick Start

```csharp
// Create a kernel and basic coroutine
var kernel = Create.Kernel();
var coro = kernel.Factory.Coroutine(MyCoroutine);
kernel.Root.Add(coro);

// Step the kernel in your update loop
kernel.Step(); // or kernel.Update(deltaTime)
```

## Core Components

### Kernel
The central execution engine that manages time and steps all active generators.

### Generators
Base execution units including coroutines, subroutines, and sequences.

### Flow Control
- **Barriers** - Wait for multiple operations to complete
- **Triggers** - Execute when any of multiple conditions are met
- **Futures** - Represent values that will be available in the future
- **Timers** - Time-based execution control

## Documentation

Current documentation is available at [GamaSutra](http://www.gamasutra.com/view/news/177397/Indepth_Flow__A_coroutine_kernel_for_Net.php) (note: formatting may be inconsistent).

The original detailed post was published on AltDevBlogADay but is no longer available.

## Testing

The [tests](TestFlow/Editor) are located in `TestFlow/Editor/` for Unity compatibility. The test suite covers:

- Kernel execution and stepping
- Coroutine lifecycle management  
- Barriers and synchronization primitives
- Future and timer functionality
- Channel communication
- Flow control structures

Run tests using your preferred NUnit test runner. These tests serve as both validation and usage examples.

## Usage Examples

### Basic Timer Example
```csharp
private void CreateHeartbeat()
{
    New.PeriodicTimer(TimeSpan.FromMinutes(2)).Elapsed += tr =>
    {
        Get<UserCount>("user/alive").Then(result =>
        {
            if (result.Succeeded(out var val))
            {
                _activeUsers.Value = val.Num;
                Info($"{val.Num} users online.");
            }
        });
    };
}
```

### Game Loop with Sequences and Barriers
```csharp
public void GameLoop()
{
    Root.Add(
        New.Sequence(
            New.Coroutine(StartGame).Named("StartGame"),
            New.While(() => !_gameOver,
                New.Coroutine(PlayerTurn).Named("Turn")),
            New.Coroutine(EndGame).Named("EndGame")
        ).Named("GameLoop")
    );
}
```

### Complex Barrier Synchronization
```csharp
private IEnumerator StartGame(IGenerator self)
{
    var start = New.Sequence(
        New.Barrier(
            WhitePlayer.StartGame(),
            BlackPlayer.StartGame()
        ).Named("Init Game"),
        New.Barrier(
            WhitePlayer.DrawInitialCards(),
            BlackPlayer.DrawInitialCards()
        ).Named("Deal Cards"),
        New.Barrier(
            New.TimedBarrier(
                TimeSpan.FromSeconds(Parameters.MulliganTimer),
                WhitePlayer.AcceptCards(),
                BlackPlayer.AcceptCards()
            ).Named("Mulligan"),
            New.Sequence(
                WhitePlayer.PlaceKing(),
                BlackPlayer.PlaceKing()
            ).Named("Place Kings")
        ).Named("Preceedings")
    ).Named("Start Game");
    
    start.Completed += (tr) => Info("StartGame completed");
    yield return start;
}
```

### Debugging and Tracing
The `.Named()` extension method enables debugging and tracing. The library provides extensive runtime visualization to monitor kernel execution in real-time.

## Architecture

Flow uses a composition-based approach combining various flow control primitives. Below are the architectural diagrams for the major systems:

### Core System Architecture

```mermaid
graph TB
    K[Kernel] --> R[Root Node]
    K --> F[Factory]
    K --> T[TimeFrame]
    K --> L[Logger]
    
    F --> |Creates| G[Generators]
    F --> |Creates| TR[Transients]
    F --> |Creates| FC[Flow Controls]
    
    R --> |Contains| N[Node Collection]
    N --> |Steps| G
    
    G --> CO[Coroutines]
    G --> SU[Subroutines]
    G --> SE[Sequences]
    
    FC --> BA[Barriers]
    FC --> TI[Triggers]
    FC --> FU[Futures]
    FC --> TM[Timers]
```

### Flow Control Primitives

```mermaid
graph LR
    subgraph "Sequential Flow"
        SEQ[Sequence] --> |Step 1| A[Operation A]
        A --> |Step 2| B[Operation B]
        B --> |Step 3| C[Operation C]
    end
    
    subgraph "Parallel Flow"
        BAR[Barrier] --> PA[Operation A]
        BAR --> PB[Operation B]
        BAR --> PC[Operation C]
        PA --> |All Complete| DONE[Continue]
        PB --> DONE
        PC --> DONE
    end
    
    subgraph "Conditional Flow"
        TRIG[Trigger] --> TA[Operation A]
        TRIG --> TB[Operation B] 
        TRIG --> TC[Operation C]
        TA --> |Any Complete| CONT[Continue]
        TB --> CONT
        TC --> CONT
    end
```

### Generator Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Created: Factory.Create()
    Created --> Running: Resume()
    Running --> Suspended: Suspend()
    Suspended --> Running: Resume()
    Running --> Completed: Complete()
    Completed --> [*]
    
    Running --> Stepping: Step()
    Stepping --> Running: Continue
    Stepping --> Suspended: yield return
    Stepping --> Completed: End of method
```

### Barrier Synchronization

```mermaid
sequenceDiagram
    participant K as Kernel
    participant B as Barrier
    participant A as Task A
    participant C as Task B  
    participant D as Task C
    
    K->>B: Create Barrier
    K->>A: Add Task A
    K->>C: Add Task B
    K->>D: Add Task C
    
    loop Step Execution
        K->>B: Step()
        B->>A: Step()
        B->>C: Step()
        B->>D: Step()
    end
    
    A->>B: Complete()
    C->>B: Complete()
    D->>B: Complete()
    B->>K: All Tasks Complete
```

### Future/Promise Pattern

```mermaid
sequenceDiagram
    participant P as Producer
    participant F as Future<T>
    participant C as Consumer
    
    C->>F: Create Future
    C->>F: ResumeAfter(future)
    Note over C: Consumer Suspended
    
    P->>F: Set Value
    F->>F: Available = true
    F->>C: Resume Consumer
    C->>F: Get Value
```

### Timer Execution Flow

```mermaid
graph TD
    T[Timer Created] --> S[Start Timer]
    S --> W{Wait for Interval}
    W --> |Time Elapsed| E[Trigger Elapsed Event]
    E --> OS{OneShot Timer?}
    OS --> |Yes| C[Complete]
    OS --> |No| W
    C --> D[Timer Destroyed]
    
    subgraph "Periodic Timer"
        P[Periodic] --> W2{Wait for Interval}
        W2 --> |Time Elapsed| E2[Trigger Elapsed Event]
        E2 --> W2
    end
```

This architecture eliminates the need to manually track state across update calls in game loops or async workflows.

## Installation

### Unity
1. Clone or download this repository
2. Copy the `Flow` folder into your Unity project's `Assets` folder
3. The library will be automatically recognized via the included `.asmdef` files

### .NET Projects
1. Clone the repository
2. Build the `Flow.sln` solution
3. Reference the compiled `Flow.dll` in your project

Alternatively, you can include the source files directly in your project.

## Performance Notes

### Verbose Logging
The `Verbose()` logging method evaluates all arguments even when the verbosity level would prevent output. Use caution with expensive operations:

```csharp
Verbosity = 10;
Verbose(15, $"Result: {ExpensiveFunction()}"); // ExpensiveFunction() still executes!
```

For performance-critical code, check verbosity levels before logging:
```csharp
if (Verbosity >= 15)
    Verbose(15, $"Result: {ExpensiveFunction()}");
```

## Requirements

- .NET Framework 4.8 or later
- Unity 4.0+ (for Unity integration)
- NUnit (for running tests)
