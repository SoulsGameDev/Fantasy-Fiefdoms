using UnityEngine;
using Pathfinding.Core;
using Pathfinding.Commands;

/// <summary>
/// Example usage patterns for the pathfinding system.
/// This class demonstrates common pathfinding scenarios and best practices.
/// </summary>
public class PathfindingExamples : MonoBehaviour
{
    [Header("Example Configuration")]
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private GameObject unitPrefab;

    // Example 1: Basic pathfinding
    public void Example_BasicPathfinding()
    {
        // Get start and goal cells
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[10, 10];

        // Find path
        PathResult path = PathfindingManager.Instance.FindPath(start, goal);

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
            Debug.LogWarning($"No path found: {path.FailureReason}");
        }
    }

    // Example 2: Pathfinding with movement limit
    public void Example_PathfindingWithMovementLimit()
    {
        HexCell start = hexGrid.Cells[5, 5];
        HexCell goal = hexGrid.Cells[15, 15];

        // Create context with movement limit
        var context = new PathfindingContext
        {
            MaxMovementPoints = 10
        };

        PathResult path = PathfindingManager.Instance.FindPath(start, goal, context);

        if (path.Success)
        {
            Debug.Log($"Path within movement budget: {path.TotalCost}/{context.MaxMovementPoints}");
        }
        else
        {
            Debug.LogWarning($"Path too long or doesn't exist: {path.FailureReason}");
        }
    }

    // Example 3: Show movement range
    public void Example_ShowMovementRange()
    {
        HexCell startCell = hexGrid.Cells[5, 5];
        int movementPoints = 5;

        // Calculate reachable cells
        var reachable = PathfindingManager.Instance.GetReachableCells(startCell, movementPoints);

        Debug.Log($"Found {reachable.Count} reachable cells within {movementPoints} movement points");

        // Cells are automatically marked as reachable
        // You can now highlight them visually
        foreach (var cell in reachable)
        {
            // cell.PathfindingState.IsReachable is already true
            // Add your visual highlight here
        }
    }

    // Example 4: Using commands for undo/redo
    public void Example_UsingCommandsForUndoRedo()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[10, 10];

        // Find path using command
        var findPathCmd = new FindPathCommand(start, goal);
        CommandHistory.Instance.ExecuteCommand(findPathCmd);

        if (findPathCmd.Result.Success)
        {
            Debug.Log("Path found and visualized");

            // Later, you can undo the visualization
            CommandHistory.Instance.Undo();
            Debug.Log("Path visualization removed");

            // Or redo it
            CommandHistory.Instance.Redo();
            Debug.Log("Path visualization restored");
        }
    }

    // Example 5: Asynchronous pathfinding
    public async void Example_AsyncPathfinding()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[20, 20];

        Debug.Log("Starting async pathfinding...");

        // This will run on a background thread if algorithm supports it
        PathResult path = await PathfindingManager.Instance.FindPathAsync(
            start,
            goal,
            new PathfindingContext()
        );

        Debug.Log($"Async pathfinding completed: {path}");

        if (path.Success)
        {
            // Visualize on main thread
            foreach (var cell in path.Path)
            {
                cell.PathfindingState.IsPath = true;
            }
        }
    }

    // Example 6: Custom pathfinding context
    public void Example_CustomContext()
    {
        HexCell start = hexGrid.Cells[5, 5];
        HexCell goal = hexGrid.Cells[15, 15];

        var context = new PathfindingContext
        {
            AllowMoveThroughAllies = true,      // Can path through friendly units
            RequireExplored = true,              // Only use explored cells
            MaxMovementPoints = 20,              // Movement budget
            PreferHighGround = true,             // Favor higher terrain
            AvoidEnemyZones = false,             // Don't avoid enemies (for attack)
            TerrainCostMultipliers = new System.Collections.Generic.Dictionary<string, float>
            {
                { "Forest", 2.0f },  // Double cost in forests
                { "Road", 0.5f }     // Half cost on roads
            }
        };

        PathResult path = PathfindingManager.Instance.FindPath(start, goal, context);

        if (path.Success)
        {
            Debug.Log($"Custom path found with cost: {path.TotalCost}");
        }
    }

    // Example 7: Cell occupation management
    public void Example_CellOccupation()
    {
        HexCell cell = hexGrid.Cells[5, 5];

        // Mark cell as occupied using command (supports undo/redo)
        var occupyCmd = new UpdateCellOccupationCommand(cell, true);
        CommandHistory.Instance.ExecuteCommand(occupyCmd);

        Debug.Log($"Cell {cell.OffsetCoordinates} is now occupied");

        // Pathfinding will now avoid this cell

        // Later, when unit moves away
        var vacateCmd = new UpdateCellOccupationCommand(cell, false);
        CommandHistory.Instance.ExecuteCommand(vacateCmd);

        Debug.Log($"Cell {cell.OffsetCoordinates} is now vacant");
    }

    // Example 8: Cell reservation for multi-unit coordination
    public void Example_CellReservation()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[10, 10];

        // Find path for first unit
        PathResult path = PathfindingManager.Instance.FindPath(start, goal);

        if (path.Success)
        {
            // Reserve cells along the path
            foreach (var cell in path.Path)
            {
                var reserveCmd = new ReserveCellCommand(cell, true);
                CommandHistory.Instance.ExecuteCommand(reserveCmd);
            }

            Debug.Log("Path reserved for unit movement");

            // Now other units will avoid these cells when pathfinding

            // After unit completes movement, unreserve cells
            foreach (var cell in path.Path)
            {
                var unreserveCmd = new ReserveCellCommand(cell, false);
                CommandHistory.Instance.ExecuteCommand(unreserveCmd);
            }

            Debug.Log("Path unreserved");
        }
    }

    // Example 9: Event subscriptions
    private void OnEnable()
    {
        // Subscribe to pathfinding events
        PathfindingManager.Instance.OnPathFound += HandlePathFound;
        PathfindingManager.Instance.OnPathFailed += HandlePathFailed;
        PathfindingManager.Instance.OnReachableCellsCalculated += HandleReachableCells;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (PathfindingManager.Instance != null)
        {
            PathfindingManager.Instance.OnPathFound -= HandlePathFound;
            PathfindingManager.Instance.OnPathFailed -= HandlePathFailed;
            PathfindingManager.Instance.OnReachableCellsCalculated -= HandleReachableCells;
        }
    }

    private void HandlePathFound(PathResult result)
    {
        Debug.Log($"Path found event: {result}");
        // Play sound effect, update UI, etc.
    }

    private void HandlePathFailed(PathResult result)
    {
        Debug.LogWarning($"Path failed event: {result.FailureReason}");
        // Show error message to player
    }

    private void HandleReachableCells(System.Collections.Generic.List<HexCell> cells)
    {
        Debug.Log($"Reachable cells calculated: {cells.Count} cells");
        // Update UI, highlight cells, etc.
    }

    // Example 10: Performance statistics
    public void Example_PerformanceStatistics()
    {
        // Get pathfinding statistics
        string stats = PathfindingManager.Instance.GetStatistics();
        Debug.Log(stats);

        // Output example:
        // Pathfinding Statistics:
        // Total Paths: 152
        // Cache Hits: 47 (30.9%)
        // Avg Computation Time: 2.35ms
        // Current Algorithm: A*

        // Reset statistics
        PathfindingManager.Instance.ResetStatistics();
    }

    // Example 11: Cache management
    public void Example_CacheManagement()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[10, 10];

        // First call - computes path
        PathResult path1 = PathfindingManager.Instance.FindPath(start, goal);
        Debug.Log($"First call: {path1.ComputationTimeMs}ms");

        // Second call - uses cache (much faster)
        PathResult path2 = PathfindingManager.Instance.FindPath(start, goal);
        Debug.Log($"Second call (cached): {path2.ComputationTimeMs}ms");

        // When terrain or occupation changes, invalidate cache
        HexCell changedCell = hexGrid.Cells[5, 5];
        changedCell.PathfindingState.IsOccupied = true;
        PathfindingManager.Instance.InvalidateCache(changedCell);

        // Next call will recompute
        PathResult path3 = PathfindingManager.Instance.FindPath(start, goal);
        Debug.Log($"Third call (recomputed): {path3.ComputationTimeMs}ms");

        // Or clear entire cache
        PathfindingManager.Instance.ClearCache();
    }

    // Example 12: Clearing visualizations
    public void Example_ClearVisualizations()
    {
        // Clear all path visualizations
        PathfindingManager.Instance.ClearPaths(hexGrid);

        // Clear all reachability visualizations
        PathfindingManager.Instance.ClearReachability(hexGrid);

        Debug.Log("All pathfinding visualizations cleared");
    }
}
