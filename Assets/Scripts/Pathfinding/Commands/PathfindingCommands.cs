using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding.Core;

namespace Pathfinding.Commands
{
    // =====================================================
    // PATHFINDING COMMANDS - Undo/Redo support for pathfinding operations
    // =====================================================

    /// <summary>
    /// Command for finding and visualizing a path between two cells.
    /// Supports undo/redo of path visualization.
    /// </summary>
    public class FindPathCommand : CommandBase
    {
        private HexCell startCell;
        private HexCell goalCell;
        private PathfindingContext context;
        private PathResult result;
        private List<HexCell> previouslyMarkedCells;

        public override string Description => result != null && result.Success
            ? $"Find path from {startCell.OffsetCoordinates} to {goalCell.OffsetCoordinates} ({result.PathLength} cells)"
            : $"Find path from {startCell.OffsetCoordinates} to {goalCell.OffsetCoordinates}";

        /// <summary>
        /// Gets the pathfinding result (available after execution)
        /// </summary>
        public PathResult Result => result;

        public FindPathCommand(HexCell start, HexCell goal, PathfindingContext context = null)
        {
            this.startCell = start;
            this.goalCell = goal;
            this.context = context ?? new PathfindingContext();
            this.previouslyMarkedCells = new List<HexCell>();
        }

        public override bool CanExecute()
        {
            return startCell != null && goalCell != null && !isExecuted;
        }

        public override bool Execute()
        {
            if (!CanExecute())
                return false;

            try
            {
                // Clear any previous path visualization
                ClearPreviousPath();

                // Find the path
                result = PathfindingManager.Instance.FindPath(startCell, goalCell, context);

                if (result.Success)
                {
                    // Mark cells as part of the path
                    foreach (var cell in result.Path)
                    {
                        cell.PathfindingState.IsPath = true;
                        previouslyMarkedCells.Add(cell);
                    }
                }

                isExecuted = true;
                return result.Success;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to execute FindPathCommand: {e.Message}");
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
                Debug.LogError($"Failed to undo FindPathCommand: {e.Message}");
                return false;
            }
        }

        public override bool Redo()
        {
            if (!isExecuted && result != null && result.Success)
            {
                // Re-mark cells as part of path
                foreach (var cell in result.Path)
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
    /// Command for visualizing all cells reachable within a movement range.
    /// Supports undo/redo of reachability visualization.
    /// </summary>
    public class ShowReachableCommand : CommandBase
    {
        private HexCell startCell;
        private int maxMovement;
        private List<HexCell> reachableCells;

        public override string Description => reachableCells != null
            ? $"Show reachable cells from {startCell.OffsetCoordinates} ({reachableCells.Count} cells)"
            : $"Show reachable cells from {startCell.OffsetCoordinates}";

        /// <summary>
        /// Gets the list of reachable cells (available after execution)
        /// </summary>
        public List<HexCell> ReachableCells => reachableCells;

        public ShowReachableCommand(HexCell start, int maxMovement)
        {
            this.startCell = start;
            this.maxMovement = maxMovement;
            this.reachableCells = new List<HexCell>();
        }

        public override bool CanExecute()
        {
            return startCell != null && maxMovement >= 0 && !isExecuted;
        }

        public override bool Execute()
        {
            if (!CanExecute())
                return false;

            try
            {
                // Clear previous reachability
                ClearPreviousReachability();

                // Calculate reachable cells
                reachableCells = PathfindingManager.Instance.GetReachableCells(startCell, maxMovement);

                isExecuted = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to execute ShowReachableCommand: {e.Message}");
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
                foreach (var cell in reachableCells)
                {
                    if (cell != null && cell.PathfindingState != null)
                    {
                        cell.PathfindingState.ClearReachability();
                    }
                }

                isExecuted = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to undo ShowReachableCommand: {e.Message}");
                return false;
            }
        }

        public override bool Redo()
        {
            if (!isExecuted && reachableCells != null)
            {
                // Re-mark cells as reachable
                foreach (var cell in reachableCells)
                {
                    if (cell != null && cell.PathfindingState != null)
                    {
                        cell.PathfindingState.IsReachable = true;
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
    /// Command for moving a unit along a pre-computed path.
    /// This is a MacroCommand that executes individual move steps.
    /// </summary>
    public class MoveAlongPathCommand : MacroCommand
    {
        private GameObject unit;
        private PathResult path;

        public MoveAlongPathCommand(GameObject unit, PathResult path)
            : base($"Move {unit.name} along path ({path.PathLength} steps)")
        {
            this.unit = unit;
            this.path = path;

            // Create individual move commands for each step in the path
            for (int i = 0; i < path.Path.Count - 1; i++)
            {
                HexCell from = path.Path[i];
                HexCell to = path.Path[i + 1];

                // Use the existing MoveUnitCommand for each step
                AddCommand(new MoveUnitCommand(unit, from, to));
            }
        }

        public override bool CanExecute()
        {
            return unit != null && path != null && path.Success && base.CanExecute();
        }
    }

    /// <summary>
    /// Command for updating cell occupation state.
    /// Used when units move to/from cells.
    /// </summary>
    public class UpdateCellOccupationCommand : CommandBase
    {
        private HexCell cell;
        private bool newOccupied;
        private bool previousOccupied;

        public override string Description => $"Update occupation of {cell.OffsetCoordinates} to {newOccupied}";

        public UpdateCellOccupationCommand(HexCell cell, bool occupied)
        {
            this.cell = cell;
            this.newOccupied = occupied;
        }

        public override bool CanExecute()
        {
            return cell != null && !isExecuted;
        }

        public override bool Execute()
        {
            if (!CanExecute())
                return false;

            try
            {
                previousOccupied = cell.PathfindingState.IsOccupied;
                cell.PathfindingState.IsOccupied = newOccupied;

                // Invalidate cached paths involving this cell
                PathfindingManager.Instance.InvalidateCache(cell);

                isExecuted = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to execute UpdateCellOccupationCommand: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
                return false;

            try
            {
                cell.PathfindingState.IsOccupied = previousOccupied;

                // Invalidate cached paths involving this cell
                PathfindingManager.Instance.InvalidateCache(cell);

                isExecuted = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to undo UpdateCellOccupationCommand: {e.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Command for reserving/unreserving cells (temporary blocks during pathfinding).
    /// Useful for multi-unit coordination.
    /// </summary>
    public class ReserveCellCommand : CommandBase
    {
        private HexCell cell;
        private bool reserve;
        private bool previousReserved;

        public override string Description => reserve
            ? $"Reserve cell {cell.OffsetCoordinates}"
            : $"Unreserve cell {cell.OffsetCoordinates}";

        public ReserveCellCommand(HexCell cell, bool reserve)
        {
            this.cell = cell;
            this.reserve = reserve;
        }

        public override bool CanExecute()
        {
            return cell != null && !isExecuted;
        }

        public override bool Execute()
        {
            if (!CanExecute())
                return false;

            try
            {
                previousReserved = cell.PathfindingState.IsReserved;
                cell.PathfindingState.IsReserved = reserve;

                // Invalidate cached paths involving this cell
                PathfindingManager.Instance.InvalidateCache(cell);

                isExecuted = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to execute ReserveCellCommand: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
                return false;

            try
            {
                cell.PathfindingState.IsReserved = previousReserved;

                // Invalidate cached paths involving this cell
                PathfindingManager.Instance.InvalidateCache(cell);

                isExecuted = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to undo ReserveCellCommand: {e.Message}");
                return false;
            }
        }
    }
}
