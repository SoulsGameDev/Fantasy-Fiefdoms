using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding.Core;

namespace Pathfinding.Commands
{
    // =====================================================
    // MULTI-TURN PATHFINDING COMMANDS
    // =====================================================

    /// <summary>
    /// Command for finding and visualizing a multi-turn path.
    /// Supports undo/redo of visualization.
    /// </summary>
    public class FindMultiTurnPathCommand : CommandBase
    {
        private HexCell startCell;
        private HexCell goalCell;
        private int movementPerTurn;
        private PathfindingContext context;
        private MultiTurnPathResult result;
        private List<HexCell> previouslyMarkedCells;

        public override string Description => result != null && result.Success
            ? $"Find multi-turn path ({result.TurnsRequired} turns, {result.CompletePath.Count} cells)"
            : $"Find multi-turn path from {startCell.OffsetCoordinates} to {goalCell.OffsetCoordinates}";

        /// <summary>
        /// Gets the multi-turn path result (available after execution)
        /// </summary>
        public MultiTurnPathResult Result => result;

        public FindMultiTurnPathCommand(
            HexCell start,
            HexCell goal,
            int movementPerTurn,
            PathfindingContext context = null)
        {
            this.startCell = start;
            this.goalCell = goal;
            this.movementPerTurn = movementPerTurn;
            this.context = context ?? new PathfindingContext();
            this.previouslyMarkedCells = new List<HexCell>();
        }

        public override bool CanExecute()
        {
            return startCell != null && goalCell != null && movementPerTurn > 0 && !isExecuted;
        }

        public override bool Execute()
        {
            if (!CanExecute())
                return false;

            try
            {
                // Clear any previous path visualization
                ClearPreviousPath();

                // Find multi-turn path
                result = PathfindingManager.Instance.FindMultiTurnPath(
                    startCell, goalCell, movementPerTurn, context);

                if (result.Success)
                {
                    // Mark cells as part of the path
                    foreach (var cell in result.CompletePath)
                    {
                        cell.PathfindingState.IsPath = true;
                        previouslyMarkedCells.Add(cell);
                    }

                    Debug.Log(result.GetTurnBreakdown());
                }

                isExecuted = true;
                return result.Success;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to execute FindMultiTurnPathCommand: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
                return false;

            try
            {
                // Clear path visualization
                foreach (var cell in previouslyMarkedCells)
                {
                    if (cell != null && cell.PathfindingState != null)
                    {
                        cell.PathfindingState.ClearPath();
                    }
                }

                isExecuted = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to undo FindMultiTurnPathCommand: {e.Message}");
                return false;
            }
        }

        public override bool Redo()
        {
            if (!isExecuted && result != null && result.Success)
            {
                // Re-mark cells as part of path
                foreach (var cell in result.CompletePath)
                {
                    cell.PathfindingState.IsPath = true;
                }
                isExecuted = true;
                return true;
            }
            return false;
        }

        private void ClearPreviousPath()
        {
            if (startCell?.Grid != null)
            {
                PathfindingManager.Instance.ClearPaths(startCell.Grid);
            }
        }
    }

    /// <summary>
    /// Command for executing a single turn of a multi-turn path.
    /// Allows step-by-step execution of long paths.
    /// </summary>
    public class ExecuteTurnMovementCommand : MacroCommand
    {
        private GameObject unit;
        private MultiTurnPathResult multiTurnPath;
        private int turnIndex;

        public ExecuteTurnMovementCommand(
            GameObject unit,
            MultiTurnPathResult multiTurnPath,
            int turnIndex)
            : base($"Execute turn {turnIndex + 1} movement for {unit.name}")
        {
            this.unit = unit;
            this.multiTurnPath = multiTurnPath;
            this.turnIndex = turnIndex;

            // Get the path segment for this turn
            var turnPath = multiTurnPath.GetTurnPath(turnIndex);

            // Create move commands for each step in this turn
            for (int i = 0; i < turnPath.Count - 1; i++)
            {
                HexCell from = turnPath[i];
                HexCell to = turnPath[i + 1];
                AddCommand(new MoveUnitCommand(unit, from, to));
            }
        }

        public override bool CanExecute()
        {
            return unit != null && multiTurnPath != null &&
                   multiTurnPath.Success && turnIndex >= 0 &&
                   turnIndex < multiTurnPath.TurnsRequired &&
                   base.CanExecute();
        }
    }

    /// <summary>
    /// Command for executing a complete multi-turn path across multiple turns.
    /// This is a macro that contains ExecuteTurnMovementCommand for each turn.
    /// </summary>
    public class ExecuteMultiTurnPathCommand : MacroCommand
    {
        private GameObject unit;
        private MultiTurnPathResult multiTurnPath;

        public ExecuteMultiTurnPathCommand(GameObject unit, MultiTurnPathResult multiTurnPath)
            : base($"Execute {multiTurnPath.TurnsRequired}-turn path for {unit.name}")
        {
            this.unit = unit;
            this.multiTurnPath = multiTurnPath;

            // Create a command for each turn
            for (int turn = 0; turn < multiTurnPath.TurnsRequired; turn++)
            {
                AddCommand(new ExecuteTurnMovementCommand(unit, multiTurnPath, turn));
            }
        }

        public override bool CanExecute()
        {
            return unit != null && multiTurnPath != null &&
                   multiTurnPath.Success && base.CanExecute();
        }
    }

    /// <summary>
    /// Command for visualizing multi-turn reachable cells.
    /// Shows which cells can be reached in each turn.
    /// </summary>
    public class ShowMultiTurnReachableCommand : CommandBase
    {
        private HexCell startCell;
        private int movementPerTurn;
        private int maxTurns;
        private Dictionary<int, List<HexCell>> cellsByTurn;
        private Dictionary<HexCell, int> cellOriginalTurnValues;

        public override string Description
        {
            get
            {
                if (cellsByTurn != null)
                {
                    int totalCells = 0;
                    foreach (var list in cellsByTurn.Values)
                        totalCells += list.Count;
                    return $"Show {maxTurns}-turn reachable cells from {startCell.OffsetCoordinates} ({totalCells} cells)";
                }
                return $"Show {maxTurns}-turn reachable cells from {startCell.OffsetCoordinates}";
            }
        }

        /// <summary>
        /// Gets the cells grouped by turn (available after execution)
        /// </summary>
        public Dictionary<int, List<HexCell>> CellsByTurn => cellsByTurn;

        public ShowMultiTurnReachableCommand(HexCell start, int movementPerTurn, int maxTurns)
        {
            this.startCell = start;
            this.movementPerTurn = movementPerTurn;
            this.maxTurns = maxTurns;
            this.cellOriginalTurnValues = new Dictionary<HexCell, int>();
        }

        public override bool CanExecute()
        {
            return startCell != null && movementPerTurn > 0 && maxTurns > 0 && !isExecuted;
        }

        public override bool Execute()
        {
            if (!CanExecute())
                return false;

            try
            {
                // Clear previous reachability
                ClearPreviousReachability();

                // Calculate multi-turn reachable cells
                cellsByTurn = PathfindingManager.Instance.GetMultiTurnReachableCells(
                    startCell, movementPerTurn, maxTurns);

                // Mark cells as reachable and store which turn they're reachable in
                // You could extend HexCellPathfindingState to have a "ReachableInTurn" property
                // For now, just mark them as reachable
                foreach (var kvp in cellsByTurn)
                {
                    foreach (var cell in kvp.Value)
                    {
                        cell.PathfindingState.IsReachable = true;
                        // Store turn info for visualization (you might add a property for this)
                    }
                }

                int totalCells = 0;
                foreach (var list in cellsByTurn.Values)
                    totalCells += list.Count;

                Debug.Log($"Multi-turn reachability: {totalCells} cells reachable in {maxTurns} turns");
                for (int turn = 0; turn < maxTurns; turn++)
                {
                    if (cellsByTurn.ContainsKey(turn))
                    {
                        Debug.Log($"  Turn {turn + 1}: {cellsByTurn[turn].Count} cells");
                    }
                }

                isExecuted = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to execute ShowMultiTurnReachableCommand: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
                return false;

            try
            {
                // Clear reachability visualization
                if (cellsByTurn != null)
                {
                    foreach (var turnCells in cellsByTurn.Values)
                    {
                        foreach (var cell in turnCells)
                        {
                            if (cell != null && cell.PathfindingState != null)
                            {
                                cell.PathfindingState.ClearReachability();
                            }
                        }
                    }
                }

                isExecuted = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to undo ShowMultiTurnReachableCommand: {e.Message}");
                return false;
            }
        }

        public override bool Redo()
        {
            if (!isExecuted && cellsByTurn != null)
            {
                // Re-mark cells as reachable
                foreach (var turnCells in cellsByTurn.Values)
                {
                    foreach (var cell in turnCells)
                    {
                        if (cell != null && cell.PathfindingState != null)
                        {
                            cell.PathfindingState.IsReachable = true;
                        }
                    }
                }
                isExecuted = true;
                return true;
            }
            return false;
        }

        private void ClearPreviousReachability()
        {
            if (startCell?.Grid != null)
            {
                PathfindingManager.Instance.ClearReachability(startCell.Grid);
            }
        }
    }

    /// <summary>
    /// Command for planning a multi-turn journey with waypoints.
    /// Useful for complex navigation that requires stopping at specific points.
    /// </summary>
    public class PlanMultiTurnJourneyCommand : CommandBase
    {
        private HexCell startCell;
        private List<HexCell> waypoints;
        private int movementPerTurn;
        private List<MultiTurnPathResult> segmentResults;

        public override string Description => $"Plan journey with {waypoints.Count} waypoints";

        /// <summary>
        /// Gets the planned path segments (available after execution)
        /// </summary>
        public List<MultiTurnPathResult> SegmentResults => segmentResults;

        public PlanMultiTurnJourneyCommand(
            HexCell start,
            List<HexCell> waypoints,
            int movementPerTurn)
        {
            this.startCell = start;
            this.waypoints = waypoints ?? new List<HexCell>();
            this.movementPerTurn = movementPerTurn;
            this.segmentResults = new List<MultiTurnPathResult>();
        }

        public override bool CanExecute()
        {
            return startCell != null && waypoints != null &&
                   waypoints.Count > 0 && movementPerTurn > 0 && !isExecuted;
        }

        public override bool Execute()
        {
            if (!CanExecute())
                return false;

            try
            {
                segmentResults.Clear();
                HexCell currentStart = startCell;

                // Plan path to each waypoint
                foreach (var waypoint in waypoints)
                {
                    var segment = PathfindingManager.Instance.FindMultiTurnPath(
                        currentStart, waypoint, movementPerTurn);

                    if (!segment.Success)
                    {
                        Debug.LogError($"Failed to find path to waypoint {waypoint.OffsetCoordinates}");
                        return false;
                    }

                    segmentResults.Add(segment);
                    currentStart = waypoint;
                }

                // Calculate total turns
                int totalTurns = 0;
                foreach (var segment in segmentResults)
                {
                    totalTurns += segment.TurnsRequired;
                }

                Debug.Log($"Journey planned: {waypoints.Count} waypoints, {totalTurns} total turns");

                isExecuted = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to execute PlanMultiTurnJourneyCommand: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
                return false;

            segmentResults.Clear();
            isExecuted = false;
            return true;
        }

        public override bool Redo()
        {
            // Re-execute the planning
            if (!isExecuted)
            {
                return Execute();
            }
            return false;
        }

        /// <summary>
        /// Gets the total number of turns required for the entire journey
        /// </summary>
        public int GetTotalTurns()
        {
            int total = 0;
            foreach (var segment in segmentResults)
            {
                total += segment.TurnsRequired;
            }
            return total;
        }
    }
}
