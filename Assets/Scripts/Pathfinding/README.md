# Pathfinding System

A highly optimized, enterprise-grade pathfinding system for Fantasy Fiefdoms, designed to integrate seamlessly with the existing hex grid, command, and guard systems.

## Overview

The pathfinding system provides:
- **Six Specialized Algorithms** - Choose the right algorithm for your use case
  - **A*** - Optimal pathfinding with heuristic guidance
  - **Dijkstra** - Find paths from one source to many destinations
  - **BFS** - Fast unweighted pathfinding for simple reachability
  - **Flow Field** - Efficient multi-unit movement to same destination
  - **Bidirectional A*** - Fast pathfinding for long distances
  - **Best-First** - Quick approximate pathfinding for AI
- **Caching** - Automatic result caching for improved performance
- **Threading Support** - Asynchronous pathfinding for large maps
- **Command Integration** - Full undo/redo support via the command system
- **Guard Validation** - Extensible validation using the guard system
- **Movement Range** - Calculate all reachable cells within movement points
- **Multi-Turn Pathfinding** - Split long paths into turn-based segments
- **Multi-unit Coordination** - Cell reservation system for coordinated movement

## Architecture

### Core Components

```
Pathfinding/
├── Core/
│   ├── PathfindingManager.cs      - Singleton manager, main API entry point
│   ├── IPathfindingAlgorithm.cs   - Strategy interface for algorithms
│   ├── PathResult.cs               - Contains path and diagnostic data
│   └── PathfindingContext.cs      - Configuration options for pathfinding
│
├── Algorithms/
│   ├── AStarPathfinding.cs        - A* implementation (optimal paths)
│   ├── DijkstraPathfinding.cs     - Dijkstra (one-to-many pathfinding)
│   ├── BreadthFirstSearch.cs      - BFS (fast unweighted pathfinding)
│   ├── FlowFieldPathfinding.cs    - Flow fields (multi-unit movement)
│   ├── BidirectionalAStar.cs      - Bidirectional A* (long paths)
│   └── BestFirstSearch.cs         - Best-First (fast approximate AI)
│
├── DataStructures/
│   └── PriorityQueue.cs           - Min-heap priority queue
│
├── Commands/
│   └── PathfindingCommands.cs     - Undo/redo commands for pathfinding
│
└── Guards/
    └── PathfindingGuards.cs       - Validation guards for pathfinding
```

### Integration Points

**HexCell Enhancement:**
- `HexCellPathfindingState` - Expanded to include pathfinding metadata (costs, parent pointers)

**Existing Systems:**
- Command System - All pathfinding operations are commands (undo/redo)
- Guard System - Walkability and occupation validation
- Singleton Pattern - PathfindingManager uses existing Singleton base

## Quick Start

### Basic Pathfinding

```csharp
// Find a simple path
PathResult path = PathfindingManager.Instance.FindPath(startCell, goalCell);

if (path.Success)
{
    Debug.Log($"Path found! Length: {path.PathLength}, Cost: {path.TotalCost}");

    // Visualize path
    foreach (var cell in path.Path)
    {
        cell.PathfindingState.IsPath = true;
    }
}
else
{
    Debug.Log($"No path: {path.FailureReason}");
}
```

### With Movement Limit

```csharp
var context = new PathfindingContext
{
    MaxMovementPoints = 10  // Limit to 10 movement points
};

PathResult path = PathfindingManager.Instance.FindPath(startCell, goalCell, context);

if (path.Success && path.TotalCost <= 10)
{
    // Execute movement
}
```

### Show Movement Range

```csharp
// Get all cells reachable within 5 movement points
List<HexCell> reachable = PathfindingManager.Instance.GetReachableCells(
    unit.CurrentCell,
    unit.MovementPoints
);

// Highlight reachable cells
foreach (var cell in reachable)
{
    cell.PathfindingState.IsReachable = true;
    // Add visual highlight
}
```

### Using Commands (With Undo/Redo)

```csharp
using Pathfinding.Commands;

// Find path as a command
var findPathCmd = new FindPathCommand(startCell, goalCell);
CommandHistory.Instance.ExecuteCommand(findPathCmd);

if (findPathCmd.Result.Success)
{
    // Move along path as a macro command
    var moveCmd = new MoveAlongPathCommand(unit, findPathCmd.Result);
    CommandHistory.Instance.ExecuteCommand(moveCmd);

    // Can undo entire movement
    CommandHistory.Instance.Undo();
}
```

### Asynchronous Pathfinding

```csharp
// For long paths on large maps
PathResult path = await PathfindingManager.Instance.FindPathAsync(startCell, goalCell);

// Path computation runs on background thread (if algorithm supports it)
// Result returns on main thread
```

## Advanced Usage

### Custom Pathfinding Context

```csharp
var context = new PathfindingContext
{
    AllowMoveThroughAllies = true,      // Can path through friendly units
    RequireExplored = true,              // Only use explored cells
    MaxMovementPoints = 15,              // Movement limit
    PreferHighGround = true,             // Favor higher terrain
    AvoidEnemyZones = true,              // Avoid cells near enemies
    TerrainCostMultipliers = new Dictionary<string, float>
    {
        { "Forest", 2.0f },  // Double cost in forests
        { "Road", 0.5f }     // Half cost on roads
    }
};

PathResult path = PathfindingManager.Instance.FindPath(start, goal, context);
```

### Cell Reservation (Multi-unit Coordination)

```csharp
// Reserve cells for a unit's path
foreach (var cell in plannedPath.Path)
{
    var reserveCmd = new ReserveCellCommand(cell, true);
    CommandHistory.Instance.ExecuteCommand(reserveCmd);
}

// Other units will avoid reserved cells during pathfinding

// After movement completes, unreserve
foreach (var cell in plannedPath.Path)
{
    var unreserveCmd = new ReserveCellCommand(cell, false);
    CommandHistory.Instance.ExecuteCommand(unreserveCmd);
}
```

### Custom Guards

```csharp
using Pathfinding.Guards;

// Create a custom guard for specific scenarios
public class OnlyGrasslandGuard : GuardBase
{
    public override GuardResult Evaluate(GuardContext context)
    {
        if (context.Cell.TerrainType.terrainName == "Grassland")
            return Allow();

        return Deny("Only grassland allowed for this unit");
    }
}

// Register with guard system
TransitionGuardRegistry.Instance.RegisterGuard(
    new OnlyGrasslandGuard(),
    GuardLevel.Conditional
);
```

### Event Subscriptions

```csharp
// Subscribe to pathfinding events
PathfindingManager.Instance.OnPathFound += (result) =>
{
    Debug.Log($"Path found: {result}");
    PlayPathFoundSound();
};

PathfindingManager.Instance.OnPathFailed += (result) =>
{
    Debug.LogWarning($"Path failed: {result.FailureReason}");
    ShowErrorMessage("Cannot reach destination");
};

PathfindingManager.Instance.OnReachableCellsCalculated += (cells) =>
{
    HighlightCells(cells);
};
```

## Performance Characteristics

### Algorithm Complexity

**A* Pathfinding:**
- Time: O(b^d) where b = branching factor (6 for hex grids), d = path depth
- Space: O(b^d) for storing explored nodes
- With good heuristic: Much better in practice

**Reachability Calculation:**
- Time: O(n) where n = number of cells within range
- Space: O(n) for visited set

### Performance Targets

| Map Size | Cells | Avg. Path Search | Max Acceptable |
|----------|-------|------------------|----------------|
| Small    | 50×50 (2,500) | < 1ms | 5ms |
| Medium   | 100×100 (10,000) | < 5ms | 20ms |
| Large    | 200×200 (40,000) | < 20ms | 50ms |

### Optimization Features

**Caching:**
- Stores recently computed paths (default 5 seconds)
- Automatic cache invalidation on map changes
- Configurable cache size limit (default 100 paths)

**Memory Pooling:**
- Reuses data structures across searches
- Minimizes garbage collection pressure
- Configurable pool sizes

**Early Termination:**
- Stops as soon as goal is reached
- Max node exploration limit (default 10,000)
- Movement point budget enforcement

## Configuration

### PathfindingManager Settings (Inspector)

```csharp
[Header("Algorithm Settings")]
public AlgorithmType defaultAlgorithm = AlgorithmType.AStar;

[Header("Cache Settings")]
public bool enableCaching = true;
public float cacheDuration = 5f;  // seconds
public int maxCacheSize = 100;

[Header("Performance")]
public bool logPerformance = false;
```

### PathfindingContext Settings

```csharp
MaxSearchNodes = 10000;           // Prevent infinite searches
MaxMovementPoints = -1;           // -1 = unlimited
RequireExplored = true;           // Fog of war enforcement
StoreDiagnosticData = true;       // Store cost maps for debugging
UseCaching = true;                // Enable result caching
```

## Testing

Run tests via Unity Test Runner:

```csharp
// Tests/PathfindingTests.cs
[Test]
public void PriorityQueue_EnqueueDequeue_ReturnsLowestPriority()
{
    // Tests priority queue correctness
}

[Test]
public void HexCellPathfindingState_IsTraversable_ChecksAllConditions()
{
    // Tests state validation
}
```

## Debugging

### Performance Statistics

```csharp
string stats = PathfindingManager.Instance.GetStatistics();
Debug.Log(stats);

// Output:
// Pathfinding Statistics:
// Total Paths: 152
// Cache Hits: 47 (30.9%)
// Avg Computation Time: 2.35ms
// Current Algorithm: A*
```

### Diagnostic Data

```csharp
var context = new PathfindingContext { StoreDiagnosticData = true };
PathResult result = PathfindingManager.Instance.FindPath(start, goal, context);

if (result.Success)
{
    // Inspect explored nodes
    foreach (var (cell, cost) in result.CostMap)
    {
        Debug.Log($"Cell {cell.OffsetCoordinates}: Cost {cost}");
    }

    // Visualize path reconstruction
    HexCell current = goal;
    while (result.CameFrom.ContainsKey(current))
    {
        current = result.CameFrom[current];
        Debug.Log($"Came from: {current.OffsetCoordinates}");
    }
}
```

## Algorithm Selection Guide

The pathfinding system includes six specialized algorithms, each optimized for different scenarios. Choose the right algorithm to maximize performance.

### Quick Reference

| Scenario | Best Algorithm | Why |
|----------|---------------|-----|
| Single path to one goal | **A*** | Optimal path with good performance |
| Paths from one source to many goals | **Dijkstra** | Computes all paths in one pass |
| Many units to same goal | **Flow Field** | Calculate once, use for all units |
| Very long paths (200+ hexes) | **Bidirectional A*** | ~2x faster than A* |
| Fast AI pathfinding (approximate OK) | **Best-First** | 2-5x faster, slightly longer path |
| Simple reachability (ignore terrain cost) | **BFS** | Fastest for unweighted checks |

### A* (AStar)

**Use Case:** Standard optimal pathfinding from start to goal

**When to Use:**
- Single unit moving to single destination
- Need guaranteed optimal (shortest cost) path
- Standard turn-based strategy movement
- Most common pathfinding scenario

**Advantages:**
- Always finds optimal path
- Efficient with good heuristic
- Well-balanced for most use cases

**Disadvantages:**
- Slower than greedy algorithms
- Not optimal for multi-destination scenarios

**Example:**
```csharp
PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.AStar);
var path = PathfindingManager.Instance.FindPath(start, goal, context);
```

### Dijkstra

**Use Case:** Finding paths from one source to many/all destinations

**When to Use:**
- Computing enemy threat ranges (all reachable cells)
- Finding nearest resource among many options
- Influence map generation
- Paths to multiple targets from one source

**Advantages:**
- One search finds distances to ALL reachable cells
- Much faster than running A* multiple times
- Perfect for "range of influence" calculations

**Disadvantages:**
- Slower than A* for single-destination paths
- Explores more nodes than A* (no heuristic)

**Example:**
```csharp
// Get distances to all cells enemy can reach
var result = PathfindingManager.Instance.FindAllPathsFrom(enemyPosition, context);

foreach (var (cell, distance) in result.DistanceMap)
{
    if (distance <= enemyMovement)
    {
        // This cell is threatened!
        cell.PathfindingState.IsPath = true;
    }
}

// Or find paths to multiple specific targets
var dijkstra = PathfindingManager.Instance.GetAlgorithm(
    PathfindingManager.AlgorithmType.Dijkstra) as DijkstraPathfinding;
var paths = dijkstra.FindPathsToMultipleGoals(start, targets, context);
```

### BFS (Breadth-First Search)

**Use Case:** Fast unweighted pathfinding

**When to Use:**
- Ability/spell range checking (ignore terrain costs)
- Quick reachability tests
- Finding cells within N steps
- Flood fill operations

**Advantages:**
- Fastest algorithm (no priority queue overhead)
- Simple and predictable
- Perfect when terrain cost doesn't matter

**Disadvantages:**
- Ignores terrain movement costs
- Not suitable for realistic movement

**Example:**
```csharp
// Get all cells within 3 steps for spell targeting
var cellsInRange = PathfindingManager.Instance.GetCellsWithinSteps(
    casterPosition, abilityRange, context);

foreach (var cell in cellsInRange)
{
    cell.PathfindingState.IsReachable = true;
}
```

### Flow Field

**Use Case:** Many units moving to same destination

**When to Use:**
- RTS group commands (select 50 units, attack here)
- All units converging on rally point
- Formation movement
- Swarm AI behavior

**Advantages:**
- Calculate once, use for unlimited units
- ~100x faster than running A* for each unit
- Natural-looking group movement
- O(1) path lookup per unit

**Disadvantages:**
- Only works when all units have same goal
- Requires full field generation (more upfront cost)

**Example:**
```csharp
// Generate flow field to rally point
var flowField = PathfindingManager.Instance.GenerateFlowField(rallyPoint, context);

// All units can now follow the field
foreach (var unit in selectedUnits)
{
    var path = flowField.GetPathFrom(unit.CurrentCell); // Instant!
    unit.FollowPath(path);
}
```

### Bidirectional A*

**Use Case:** Long paths on large maps

**When to Use:**
- Strategic map navigation (200+ hex distance)
- Cross-continental travel
- Very large maps where A* is too slow
- When distance between start/goal is large

**Advantages:**
- ~2x faster than standard A* for long paths
- Explores fewer nodes
- Still finds optimal path

**Disadvantages:**
- Slightly more complex
- Minimal benefit for short paths
- Small overhead for path reconstruction

**Example:**
```csharp
PathfindingManager.Instance.SetAlgorithm(
    PathfindingManager.AlgorithmType.BidirectionalAStar);

// Path across entire map (0,0) to (199,199)
var path = PathfindingManager.Instance.FindPath(start, goal, context);
// Much faster than standard A* for this distance!
```

### Best-First (Greedy)

**Use Case:** Fast approximate pathfinding for AI

**When to Use:**
- Enemy AI pathfinding
- Background NPC movement
- Real-time pathfinding (60 FPS requirement)
- When "good enough" paths are acceptable

**Advantages:**
- 2-5x faster than A*
- Very responsive for real-time gameplay
- Good for non-critical pathfinding

**Disadvantages:**
- Path may be 1-3x longer than optimal
- No optimality guarantee
- Can get stuck in local minima

**Example:**
```csharp
PathfindingManager.Instance.SetAlgorithm(PathfindingManager.AlgorithmType.BestFirst);

// Fast AI pathfinding for 10 enemies
foreach (var enemy in enemies)
{
    var path = PathfindingManager.Instance.FindPath(
        enemy.CurrentCell, playerCell, context);
    enemy.FollowPath(path);
}
// All 10 paths computed in <5ms total!
```

### Switching Algorithms Dynamically

You can switch algorithms at runtime based on the situation:

```csharp
public PathResult SmartPathfinding(HexCell start, HexCell goal, PathfindingContext context)
{
    // Calculate distance
    int distance = CalculateHexDistance(start, goal);

    // Choose algorithm based on distance
    if (distance > 200)
    {
        // Long path - use bidirectional A*
        PathfindingManager.Instance.SetAlgorithm(
            PathfindingManager.AlgorithmType.BidirectionalAStar);
    }
    else if (context.IgnoreTerrainCost)
    {
        // Just checking reachability - use BFS
        PathfindingManager.Instance.SetAlgorithm(
            PathfindingManager.AlgorithmType.BFS);
    }
    else
    {
        // Standard case - use A*
        PathfindingManager.Instance.SetAlgorithm(
            PathfindingManager.AlgorithmType.AStar);
    }

    return PathfindingManager.Instance.FindPath(start, goal, context);
}
```

### Performance Comparison

For typical scenarios on 100×100 map:

| Algorithm | Single Path | 100 Paths | All Paths | Notes |
|-----------|------------|-----------|-----------|-------|
| A* | 3ms | 300ms | N/A | Optimal single-target |
| Dijkstra | 8ms | 8ms | 8ms | One search for all! |
| BFS | 1ms | 100ms | N/A | Fastest but unweighted |
| Flow Field | 10ms | 10ms | 10ms | Generate once + 100× O(1) lookups |
| Bidirectional A* | 1.5ms | 150ms | N/A | ~2x faster for long paths |
| Best-First | 0.8ms | 80ms | N/A | Fastest but non-optimal |

*Times are approximate and depend on map complexity, obstacles, and path length*

### Example Usage Patterns

See `Assets/Scripts/Pathfinding/Examples/AlgorithmExamples.cs` for comprehensive examples including:
- Real-world scenarios (RTS groups, threat displays, AI combat)
- Performance comparisons between algorithms
- When to use each algorithm
- Algorithm switching strategies

## Multi-Turn Pathfinding

For turn-based strategy games, units often need to plan paths that span multiple turns. The multi-turn pathfinding system automatically splits long paths into turn-based segments.

### Features

- **Automatic Path Splitting** - Divides paths based on movement budget per turn
- **Turn Endpoints** - Identifies where units will stop each turn
- **Multi-Turn Reachability** - Shows cells reachable in each of N turns
- **Turn Estimation** - Quick distance-based turn count estimation
- **Journey Planning** - Multi-waypoint paths across many turns
- **Progress Tracking** - Get remaining path after partial completion
- **Color-Coded Visualization** - Different colors for each turn segment

### Basic Usage

```csharp
// Find a multi-turn path
HexCell start = hexGrid.Cells[0, 0];
HexCell goal = hexGrid.Cells[25, 25];
int movementPerTurn = 5;

MultiTurnPathResult path = PathfindingManager.Instance.FindMultiTurnPath(
    start, goal, movementPerTurn);

if (path.Success)
{
    Debug.Log($"Path requires {path.TurnsRequired} turns");
    Debug.Log($"Total cost: {path.TotalCost}");

    // Print turn breakdown
    Debug.Log(path.GetTurnBreakdown());

    // Check if reachable in one turn
    if (path.IsSingleTurnPath())
    {
        Debug.Log("Can reach in one turn!");
    }
}
```

### Execute Path Turn by Turn

```csharp
MultiTurnPathResult path = PathfindingManager.Instance.FindMultiTurnPath(
    start, goal, movementPerTurn);

if (path.Success)
{
    // Execute each turn separately
    for (int turn = 0; turn < path.TurnsRequired; turn++)
    {
        var turnPath = path.GetTurnPath(turn);
        var turnCost = path.GetTurnCost(turn);
        var endpoint = path.GetTurnEndpoint(turn);

        Debug.Log($"Turn {turn + 1}: {turnPath.Count} cells, " +
                 $"Cost: {turnCost}/{movementPerTurn}");

        // Execute this turn's movement
        var turnCmd = new ExecuteTurnMovementCommand(unit, path, turn);
        CommandHistory.Instance.ExecuteCommand(turnCmd);

        // Wait for turn to end...
    }
}
```

### Multi-Turn Reachability

Show which cells can be reached in each of N turns:

```csharp
HexCell start = hexGrid.Cells[10, 10];
int movementPerTurn = 5;
int maxTurns = 3;

Dictionary<int, List<HexCell>> cellsByTurn =
    PathfindingManager.Instance.GetMultiTurnReachableCells(
        start, movementPerTurn, maxTurns);

foreach (var kvp in cellsByTurn)
{
    int turnNumber = kvp.Key + 1;
    int cellCount = kvp.Value.Count;
    Debug.Log($"Turn {turnNumber}: {cellCount} cells reachable");
}

// Visualize with color coding
visualizer.VisualizeReachability(cellsByTurn);
```

### Quick Turn Estimation

Estimate turns needed without full pathfinding:

```csharp
int estimatedTurns = PathfindingManager.Instance.EstimateTurnsToReach(
    start, goal, movementPerTurn);

Debug.Log($"Estimated: {estimatedTurns} turns");

// Compare with actual
MultiTurnPathResult actualPath = PathfindingManager.Instance.FindMultiTurnPath(
    start, goal, movementPerTurn);

Debug.Log($"Actual: {actualPath.TurnsRequired} turns");
```

### Multi-Waypoint Journeys

Plan complex routes with multiple stops:

```csharp
HexCell start = hexGrid.Cells[0, 0];
List<HexCell> waypoints = new List<HexCell>
{
    hexGrid.Cells[10, 5],   // Stop 1
    hexGrid.Cells[15, 15],  // Stop 2
    hexGrid.Cells[20, 10],  // Stop 3
    hexGrid.Cells[25, 0]    // Final destination
};

var journeyCmd = new PlanMultiTurnJourneyCommand(start, waypoints, movementPerTurn);
CommandHistory.Instance.ExecuteCommand(journeyCmd);

Debug.Log($"Total turns: {journeyCmd.GetTotalTurns()}");
```

### Path Progress Tracking

Handle partially completed paths:

```csharp
MultiTurnPathResult fullPath = PathfindingManager.Instance.FindMultiTurnPath(
    start, goal, movementPerTurn);

// After completing 2 turns
int completedTurns = 2;
List<HexCell> remaining = fullPath.GetRemainingPath(completedTurns);

Debug.Log($"Completed {completedTurns}/{fullPath.TurnsRequired} turns");
Debug.Log($"Remaining: {remaining.Count} cells");

// Visualize progress
visualizer.ShowPathUpToTurn(fullPath, completedTurns);
```

### Movement Efficiency

Analyze how well turns are utilized:

```csharp
MultiTurnPathResult path = PathfindingManager.Instance.FindMultiTurnPath(
    start, goal, movementPerTurn);

float avgEfficiency = path.GetAverageMovementEfficiency();
Debug.Log($"Average efficiency: {avgEfficiency:P0}");

// Check each turn
for (int i = 0; i < path.TurnsRequired; i++)
{
    bool full = path.IsTurnAtCapacity(i);
    int cost = path.GetTurnCost(i);
    Debug.Log($"Turn {i + 1}: {cost}/{movementPerTurn} " +
             (full ? "[FULL]" : ""));
}
```

### Visualization

Use `MultiTurnPathVisualizer` for color-coded path display:

```csharp
[SerializeField] private MultiTurnPathVisualizer visualizer;

// Visualize complete path with turn colors
visualizer.VisualizePath(multiTurnPath);

// Highlight specific turn
visualizer.HighlightTurn(multiTurnPath, turnIndex: 1);

// Show progress up to current turn
visualizer.ShowPathUpToTurn(multiTurnPath, completedTurns: 2);

// Clear visualization
visualizer.ClearVisualization();
```

### Commands

All multi-turn operations support undo/redo:

```csharp
// Find and visualize multi-turn path
var findCmd = new FindMultiTurnPathCommand(start, goal, movementPerTurn);
CommandHistory.Instance.ExecuteCommand(findCmd);

// Execute entire multi-turn path
var executeCmd = new ExecuteMultiTurnPathCommand(unit, findCmd.Result);
CommandHistory.Instance.ExecuteCommand(executeCmd);

// Show multi-turn reachability
var reachCmd = new ShowMultiTurnReachableCommand(start, movementPerTurn, maxTurns: 3);
CommandHistory.Instance.ExecuteCommand(reachCmd);

// All can be undone
CommandHistory.Instance.Undo();
```

## Future Enhancements

Planned features for future versions:

- **Unity Job System & Burst Compiler** - Multithreaded pathfinding with SIMD optimizations
  - Use C# Job System for parallel path calculations
  - Burst compile hot paths for maximum performance
  - Support batch pathfinding for multiple units
  - Thread-safe data structures for concurrent access
- **Hierarchical Pathfinding** - For very large maps (divide into regions)
- **Jump Point Search** - Grid-optimized A* variant with symmetry breaking
- **Dynamic Obstacle Avoidance** - Real-time path adjustment for moving obstacles
- **Path Smoothing** - Visual path optimization and corner cutting

## API Reference

### PathfindingManager

**Main Methods:**
- `FindPath(start, goal, context)` - Synchronous pathfinding
- `FindPathAsync(start, goal, context)` - Asynchronous pathfinding
- `GetReachableCells(start, maxMovement)` - Calculate movement range
- `FindMultiTurnPath(start, goal, movementPerTurn)` - Multi-turn pathfinding
- `FindMultiTurnPathAsync(start, goal, movementPerTurn, context)` - Async multi-turn
- `GetMultiTurnReachableCells(start, movementPerTurn, maxTurns)` - Multi-turn reachability
- `EstimateTurnsToReach(start, goal, movementPerTurn)` - Quick turn estimation
- `ClearPaths(grid)` - Clear path visualization
- `ClearReachability(grid)` - Clear reachability visualization
- `InvalidateCache(cells)` - Invalidate cached paths
- `SetAlgorithm(type)` - Change pathfinding algorithm
- `GetAlgorithm(type)` - Get algorithm instance for direct access

**Algorithm-Specific Methods:**
- `FindAllPathsFrom(start, context)` - Dijkstra: distances to all reachable cells
- `GenerateFlowField(goal, context)` - Flow Field: direction field for multi-unit movement
- `GetCellsWithinSteps(start, maxSteps, context)` - BFS: cells within N steps (unweighted)
- `GetAlgorithmInfo()` - Get documentation for all available algorithms

### PathResult

**Properties:**
- `Success` - Whether path was found
- `Path` - List of cells forming the path
- `TotalCost` - Sum of movement costs
- `NodesExplored` - Number of cells examined
- `ComputationTimeMs` - Time taken in milliseconds
- `FailureReason` - Explanation if path failed
- `CostMap` - Costs to reach each explored cell (diagnostic)
- `CameFrom` - Parent pointers for path reconstruction (diagnostic)

### MultiTurnPathResult

**Properties:**
- `Success` - Whether path was found
- `CompletePath` - Full path from start to goal
- `PathPerTurn` - List of turn segments (List<List<HexCell>>)
- `CostPerTurn` - Movement cost for each turn
- `TurnsRequired` - Number of turns to complete path
- `TotalCost` - Sum of all turn costs
- `MovementPerTurn` - Movement budget per turn
- `TurnEndpoints` - Cells where unit stops each turn

**Methods:**
- `GetTurnPath(turnIndex)` - Get cells for specific turn
- `GetTurnCost(turnIndex)` - Get cost for specific turn
- `GetTurnEndpoint(turnIndex)` - Get endpoint for turn
- `IsSingleTurnPath()` - Check if reachable in one turn
- `GetRemainingPath(completedTurns)` - Get unfinished portion
- `GetTurnBreakdown()` - Formatted string of all turns
- `GetAverageMovementEfficiency()` - Average turn utilization
- `IsTurnAtCapacity(turnIndex)` - Check if turn uses full movement

### PathfindingContext

**Key Options:**
- `MaxMovementPoints` - Movement budget (-1 = unlimited)
- `RequireExplored` - Enforce fog of war
- `AllowMoveThroughAllies` - Path through friendly units
- `MaxSearchNodes` - Prevent infinite searches
- `TerrainCostMultipliers` - Custom terrain costs
- `DynamicObstacles` - Temporary impassable cells

## License

Part of the Fantasy Fiefdoms project.

## Contact

For questions or issues related to the pathfinding system, please refer to the main project documentation.
