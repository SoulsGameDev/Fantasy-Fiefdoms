using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Core;
using Pathfinding.Commands;
using Pathfinding.Visualization;

/// <summary>
/// Example usage patterns for multi-turn pathfinding.
/// Demonstrates how to handle long-distance movement in turn-based games.
/// </summary>
public class MultiTurnPathfindingExamples : MonoBehaviour
{
    [Header("Example Configuration")]
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private MultiTurnPathVisualizer visualizer;

    [Header("Unit Settings")]
    [SerializeField] private int unitMovementPerTurn = 5;

    // Example 1: Basic multi-turn pathfinding
    public void Example_BasicMultiTurnPath()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[20, 20]; // Far away destination

        // Find multi-turn path
        MultiTurnPathResult path = PathfindingManager.Instance.FindMultiTurnPath(
            start, goal, unitMovementPerTurn);

        if (path.Success)
        {
            Debug.Log($"Multi-turn path found!");
            Debug.Log($"  Turns required: {path.TurnsRequired}");
            Debug.Log($"  Total cost: {path.TotalCost}");
            Debug.Log($"  Total cells: {path.CompletePath.Count}");

            // Print turn breakdown
            Debug.Log(path.GetTurnBreakdown());

            // Visualize the path
            if (visualizer != null)
            {
                visualizer.VisualizePath(path);
            }
        }
        else
        {
            Debug.LogWarning($"No path found: {path.FailureReason}");
        }
    }

    // Example 2: Execute path turn by turn
    public void Example_ExecutePathTurnByTurn()
    {
        HexCell start = hexGrid.Cells[5, 5];
        HexCell goal = hexGrid.Cells[25, 25];
        GameObject unit = Instantiate(unitPrefab);

        // Find multi-turn path
        MultiTurnPathResult path = PathfindingManager.Instance.FindMultiTurnPath(
            start, goal, unitMovementPerTurn);

        if (path.Success)
        {
            Debug.Log($"Executing {path.TurnsRequired}-turn movement");

            // Execute each turn individually
            for (int turn = 0; turn < path.TurnsRequired; turn++)
            {
                var turnPath = path.GetTurnPath(turn);
                var turnCost = path.GetTurnCost(turn);
                var endpoint = path.GetTurnEndpoint(turn);

                Debug.Log($"Turn {turn + 1}: Moving {turnPath.Count} cells, " +
                         $"Cost: {turnCost}/{unitMovementPerTurn}, " +
                         $"Endpoint: {endpoint.OffsetCoordinates}");

                // Execute this turn's movement using command
                var turnCmd = new ExecuteTurnMovementCommand(unit, path, turn);
                CommandHistory.Instance.ExecuteCommand(turnCmd);

                // In a real game, you would wait for the turn to end here
                // Then continue with the next turn
            }
        }
    }

    // Example 3: Using command for multi-turn path
    public void Example_MultiTurnPathCommand()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[15, 15];

        // Find and visualize using command (supports undo/redo)
        var findCmd = new FindMultiTurnPathCommand(start, goal, unitMovementPerTurn);
        CommandHistory.Instance.ExecuteCommand(findCmd);

        if (findCmd.Result.Success)
        {
            Debug.Log("Multi-turn path visualized");
            Debug.Log(findCmd.Result.GetTurnBreakdown());

            // Can undo the visualization
            // CommandHistory.Instance.Undo();
        }
    }

    // Example 4: Show multi-turn reachable cells
    public void Example_ShowMultiTurnReachability()
    {
        HexCell startCell = hexGrid.Cells[10, 10];
        int maxTurns = 3;

        // Get cells reachable in each turn
        Dictionary<int, List<HexCell>> cellsByTurn =
            PathfindingManager.Instance.GetMultiTurnReachableCells(
                startCell, unitMovementPerTurn, maxTurns);

        Debug.Log($"Multi-turn reachability from {startCell.OffsetCoordinates}:");
        for (int turn = 0; turn < maxTurns; turn++)
        {
            if (cellsByTurn.ContainsKey(turn))
            {
                Debug.Log($"  Turn {turn + 1}: {cellsByTurn[turn].Count} cells reachable");
            }
        }

        // Visualize
        if (visualizer != null)
        {
            visualizer.VisualizeReachability(cellsByTurn);
        }
    }

    // Example 5: Using command for multi-turn reachability
    public void Example_MultiTurnReachabilityCommand()
    {
        HexCell startCell = hexGrid.Cells[10, 10];
        int maxTurns = 4;

        var showCmd = new ShowMultiTurnReachableCommand(
            startCell, unitMovementPerTurn, maxTurns);
        CommandHistory.Instance.ExecuteCommand(showCmd);

        // Get the results
        var cellsByTurn = showCmd.CellsByTurn;
        Debug.Log($"Can reach cells in {cellsByTurn.Count} different turns");

        // Can undo/redo the visualization
        // CommandHistory.Instance.Undo();
        // CommandHistory.Instance.Redo();
    }

    // Example 6: Estimate turns to destination
    public void Example_EstimateTurnsToDestination()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[30, 30];

        // Quick estimation without full pathfinding
        int estimatedTurns = PathfindingManager.Instance.EstimateTurnsToReach(
            start, goal, unitMovementPerTurn);

        Debug.Log($"Estimated turns to reach destination: {estimatedTurns}");

        // Compare with actual path
        MultiTurnPathResult actualPath = PathfindingManager.Instance.FindMultiTurnPath(
            start, goal, unitMovementPerTurn);

        if (actualPath.Success)
        {
            Debug.Log($"Actual turns required: {actualPath.TurnsRequired}");
            Debug.Log($"Difference: {actualPath.TurnsRequired - estimatedTurns} turns");
        }
    }

    // Example 7: Check if destination is within single turn
    public void Example_CheckSingleTurnReachable()
    {
        HexCell start = hexGrid.Cells[5, 5];
        HexCell nearGoal = hexGrid.Cells[7, 7];
        HexCell farGoal = hexGrid.Cells[20, 20];

        // Check near destination
        MultiTurnPathResult nearPath = PathfindingManager.Instance.FindMultiTurnPath(
            start, nearGoal, unitMovementPerTurn);

        if (nearPath.Success && nearPath.IsSingleTurnPath())
        {
            Debug.Log($"Near goal can be reached in one turn!");
        }

        // Check far destination
        MultiTurnPathResult farPath = PathfindingManager.Instance.FindMultiTurnPath(
            start, farGoal, unitMovementPerTurn);

        if (farPath.Success && !farPath.IsSingleTurnPath())
        {
            Debug.Log($"Far goal requires {farPath.TurnsRequired} turns");
        }
    }

    // Example 8: Plan journey with multiple waypoints
    public void Example_PlanMultiWaypointJourney()
    {
        HexCell start = hexGrid.Cells[0, 0];

        // Create a journey with multiple waypoints
        List<HexCell> waypoints = new List<HexCell>
        {
            hexGrid.Cells[10, 5],   // First waypoint
            hexGrid.Cells[15, 15],  // Second waypoint
            hexGrid.Cells[20, 10],  // Third waypoint
            hexGrid.Cells[25, 0]    // Final destination
        };

        var journeyCmd = new PlanMultiTurnJourneyCommand(start, waypoints, unitMovementPerTurn);
        CommandHistory.Instance.ExecuteCommand(journeyCmd);

        if (journeyCmd.SegmentResults != null)
        {
            Debug.Log($"Journey planned with {waypoints.Count} waypoints");
            Debug.Log($"Total turns: {journeyCmd.GetTotalTurns()}");

            for (int i = 0; i < journeyCmd.SegmentResults.Count; i++)
            {
                var segment = journeyCmd.SegmentResults[i];
                Debug.Log($"  Segment {i + 1}: {segment.TurnsRequired} turns, " +
                         $"{segment.TotalCost} cost");
            }
        }
    }

    // Example 9: Get remaining path after partial completion
    public void Example_GetRemainingPath()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[20, 20];

        MultiTurnPathResult fullPath = PathfindingManager.Instance.FindMultiTurnPath(
            start, goal, unitMovementPerTurn);

        if (fullPath.Success)
        {
            Debug.Log($"Full path: {fullPath.TurnsRequired} turns");

            // Simulate completing 2 turns
            int completedTurns = 2;
            List<HexCell> remaining = fullPath.GetRemainingPath(completedTurns);

            Debug.Log($"After {completedTurns} turns, {remaining.Count} cells remaining");
            Debug.Log($"Turns left: {fullPath.TurnsRequired - completedTurns}");
        }
    }

    // Example 10: Async multi-turn pathfinding
    public async void Example_AsyncMultiTurnPathfinding()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[30, 30];

        Debug.Log("Starting async multi-turn pathfinding...");

        MultiTurnPathResult path = await PathfindingManager.Instance.FindMultiTurnPathAsync(
            start, goal, unitMovementPerTurn, new PathfindingContext());

        Debug.Log($"Async pathfinding completed: {path}");

        if (path.Success)
        {
            Debug.Log(path.GetTurnBreakdown());

            // Visualize
            if (visualizer != null)
            {
                visualizer.VisualizePath(path);
            }
        }
    }

    // Example 11: Custom context with multi-turn pathfinding
    public void Example_MultiTurnWithCustomContext()
    {
        HexCell start = hexGrid.Cells[5, 5];
        HexCell goal = hexGrid.Cells[25, 25];

        // Create custom context
        var context = new PathfindingContext
        {
            AllowMoveThroughAllies = true,
            RequireExplored = true,
            TerrainCostMultipliers = new Dictionary<string, float>
            {
                { "Forest", 2.0f },
                { "Road", 0.5f }
            }
        };

        MultiTurnPathResult path = PathfindingManager.Instance.FindMultiTurnPath(
            start, goal, unitMovementPerTurn, context);

        if (path.Success)
        {
            Debug.Log($"Custom multi-turn path: {path.TurnsRequired} turns");
            Debug.Log(path.GetTurnBreakdown());
        }
    }

    // Example 12: Visualize specific turn segment
    public void Example_HighlightSpecificTurn()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[20, 20];

        MultiTurnPathResult path = PathfindingManager.Instance.FindMultiTurnPath(
            start, goal, unitMovementPerTurn);

        if (path.Success && visualizer != null)
        {
            // First show the complete path
            visualizer.VisualizePath(path);

            // Wait or use a button press, then highlight turn 2
            int turnToHighlight = 1; // 0-based index
            visualizer.HighlightTurn(path, turnToHighlight);

            Debug.Log($"Highlighting turn {turnToHighlight + 1}");
        }
    }

    // Example 13: Show path progress
    public void Example_ShowPathProgress()
    {
        HexCell start = hexGrid.Cells[0, 0];
        HexCell goal = hexGrid.Cells[20, 20];

        MultiTurnPathResult path = PathfindingManager.Instance.FindMultiTurnPath(
            start, goal, unitMovementPerTurn);

        if (path.Success && visualizer != null)
        {
            // Show progress through the path (e.g., after completing 2 turns)
            int completedTurns = 2;
            visualizer.ShowPathUpToTurn(path, completedTurns);

            Debug.Log($"Showing progress: {completedTurns}/{path.TurnsRequired} turns completed");
        }
    }

    // Example 14: Calculate movement efficiency
    public void Example_CalculateMovementEfficiency()
    {
        HexCell start = hexGrid.Cells[5, 5];
        HexCell goal = hexGrid.Cells[20, 20];

        MultiTurnPathResult path = PathfindingManager.Instance.FindMultiTurnPath(
            start, goal, unitMovementPerTurn);

        if (path.Success)
        {
            float efficiency = path.GetAverageMovementEfficiency();
            Debug.Log($"Average movement efficiency: {efficiency:P0}");

            // Check each turn
            for (int i = 0; i < path.TurnsRequired; i++)
            {
                int turnCost = path.GetTurnCost(i);
                float turnEfficiency = (float)turnCost / unitMovementPerTurn;
                bool atCapacity = path.IsTurnAtCapacity(i);

                Debug.Log($"  Turn {i + 1}: {turnCost}/{unitMovementPerTurn} " +
                         $"({turnEfficiency:P0}){(atCapacity ? " [FULL]" : "")}");
            }
        }
    }

    // Example 15: Clear all visualizations
    public void Example_ClearVisualizations()
    {
        if (visualizer != null)
        {
            visualizer.ClearVisualization();
            Debug.Log("Multi-turn visualizations cleared");
        }

        // Also clear grid-based visualizations
        PathfindingManager.Instance.ClearPaths(hexGrid);
        PathfindingManager.Instance.ClearReachability(hexGrid);
    }
}
