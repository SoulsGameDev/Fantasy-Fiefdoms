# Pathfinding System

A highly optimized, enterprise-grade pathfinding system for Fantasy Fiefdoms, designed to integrate seamlessly with the existing hex grid, command, and guard systems.

## Overview

The pathfinding system provides:
- **A* Algorithm** - Optimal pathfinding with heuristic guidance
- **Caching** - Automatic result caching for improved performance
- **Threading Support** - Asynchronous pathfinding for large maps
- **Command Integration** - Full undo/redo support via the command system
- **Guard Validation** - Extensible validation using the guard system
- **Movement Range** - Calculate all reachable cells within movement points
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
│   └── AStarPathfinding.cs        - A* implementation for hex grids
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

## Future Enhancements

Planned features for future versions:

- **Dijkstra's Algorithm** - For finding shortest paths to all cells
- **Flow Field Pathfinding** - For efficient multi-unit movement to same destination
- **Hierarchical Pathfinding** - For very large maps (divide into regions)
- **Jump Point Search** - Grid-optimized A* variant
- **Multi-turn Pathfinding** - Split long paths into per-turn segments
- **Dynamic Obstacle Avoidance** - Real-time path adjustment
- **Path Smoothing** - Visual path optimization

## API Reference

### PathfindingManager

**Main Methods:**
- `FindPath(start, goal, context)` - Synchronous pathfinding
- `FindPathAsync(start, goal, context)` - Asynchronous pathfinding
- `GetReachableCells(start, maxMovement)` - Calculate movement range
- `ClearPaths(grid)` - Clear path visualization
- `ClearReachability(grid)` - Clear reachability visualization
- `InvalidateCache(cells)` - Invalidate cached paths
- `SetAlgorithm(type)` - Change pathfinding algorithm

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
