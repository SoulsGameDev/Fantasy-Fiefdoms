using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Pathfinding.Core;
using Pathfinding.DataStructures;

namespace Pathfinding.Algorithms
{
    /// <summary>
    /// A* pathfinding algorithm implementation optimized for hex grids.
    /// Guarantees optimal paths when using an admissible heuristic.
    /// </summary>
    public class AStarPathfinding : IPathfindingAlgorithm
    {
        public string AlgorithmName => "A*";
        public bool SupportsThreading => false; // Uses Unity API (Vector3 in HexMetrics)
        public string Description => "Optimal pathfinding with heuristic guidance. Best for single-source, single-target paths.";

        /// <summary>
        /// Finds the optimal path from start to goal using A* algorithm
        /// </summary>
        public PathResult FindPath(HexCell start, HexCell goal, PathfindingContext context)
        {
            // Validation
            if (start == null || goal == null)
                return PathResult.CreateFailure(start, goal, "Start or goal is null");

            if (start == goal)
                return CreateSingleCellPath(start, goal);

            if (!goal.PathfindingState.IsValidDestination())
                return PathResult.CreateFailure(start, goal, "Goal is not a valid destination");

            if (context.RequireExplored && !goal.PathfindingState.IsExplored)
                return PathResult.CreateFailure(start, goal, "Goal is not explored (fog of war)");

            // Start timing
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Reset pathfinding state on all relevant cells
            ResetSearchState(start.Grid);

            // Initialize data structures
            var openSet = new PriorityQueue<HexCell>(256);
            var costMap = new Dictionary<HexCell, int>();
            var cameFrom = new Dictionary<HexCell, HexCell>();
            int nodesExplored = 0;

            // Initialize start cell
            start.PathfindingState.GCost = 0;
            start.PathfindingState.HCost = CalculateHeuristic(start, goal);
            start.PathfindingState.IsInOpenSet = true;
            openSet.Enqueue(start, start.PathfindingState.FCost);
            costMap[start] = 0;

            // A* main loop
            while (!openSet.IsEmpty)
            {
                // Check node limit
                if (nodesExplored >= context.MaxSearchNodes)
                {
                    stopwatch.Stop();
                    return PathResult.CreateFailure(start, goal,
                        $"Search exceeded max nodes ({context.MaxSearchNodes})",
                        nodesExplored, stopwatch.ElapsedMilliseconds);
                }

                // Get cell with lowest f-cost
                HexCell current = openSet.Dequeue();
                current.PathfindingState.IsInOpenSet = false;
                current.PathfindingState.IsInClosedSet = true;
                nodesExplored++;

                // Goal reached - reconstruct path
                if (current == goal)
                {
                    stopwatch.Stop();
                    List<HexCell> path = ReconstructPath(cameFrom, current);
                    int totalCost = current.PathfindingState.GCost;

                    // Check movement point limit
                    if (context.MaxMovementPoints >= 0 && totalCost > context.MaxMovementPoints)
                    {
                        return PathResult.CreateFailure(start, goal,
                            $"Path cost ({totalCost}) exceeds max movement points ({context.MaxMovementPoints})",
                            nodesExplored, stopwatch.ElapsedMilliseconds);
                    }

                    return PathResult.CreateSuccess(
                        start, goal, path, totalCost, nodesExplored,
                        stopwatch.ElapsedMilliseconds,
                        context.StoreDiagnosticData ? costMap : null,
                        context.StoreDiagnosticData ? cameFrom : null);
                }

                // Explore neighbors
                List<HexCell> neighbors = current.GetNeighbors();
                if (neighbors == null)
                    continue;

                foreach (HexCell neighbor in neighbors)
                {
                    if (neighbor == null)
                        continue;

                    // Skip if already fully explored
                    if (neighbor.PathfindingState.IsInClosedSet)
                        continue;

                    // Check if neighbor is traversable
                    if (!IsTraversable(neighbor, context, neighbor == goal))
                        continue;

                    // Calculate tentative g-cost
                    int movementCost = context.GetEffectiveMovementCost(neighbor);
                    int tentativeGCost = current.PathfindingState.GCost + movementCost;

                    // Check if this path to neighbor is better than any previous one
                    bool isNewCell = !neighbor.PathfindingState.IsInOpenSet;
                    bool isBetterPath = tentativeGCost < neighbor.PathfindingState.GCost;

                    if (isNewCell || isBetterPath)
                    {
                        // Update neighbor's costs and parent
                        neighbor.PathfindingState.GCost = tentativeGCost;
                        neighbor.PathfindingState.HCost = CalculateHeuristic(neighbor, goal);
                        neighbor.PathfindingState.CameFrom = current;
                        cameFrom[neighbor] = current;
                        costMap[neighbor] = tentativeGCost;

                        if (isNewCell)
                        {
                            // Add to open set
                            neighbor.PathfindingState.IsInOpenSet = true;
                            openSet.Enqueue(neighbor, neighbor.PathfindingState.FCost);
                        }
                        else
                        {
                            // Update priority in open set
                            openSet.UpdatePriority(neighbor, neighbor.PathfindingState.FCost);
                        }
                    }
                }
            }

            // No path found
            stopwatch.Stop();
            return PathResult.CreateFailure(start, goal, "No path exists",
                nodesExplored, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Calculates the heuristic distance between two cells using hex distance
        /// </summary>
        private int CalculateHeuristic(HexCell from, HexCell to)
        {
            // Use cube coordinate distance (Manhattan distance for hex grids)
            Vector3 fromCube = from.CubeCoordinates;
            Vector3 toCube = to.CubeCoordinates;

            int distance = (int)((Mathf.Abs(fromCube.x - toCube.x) +
                                  Mathf.Abs(fromCube.y - toCube.y) +
                                  Mathf.Abs(fromCube.z - toCube.z)) / 2);

            // Multiply by average movement cost to make heuristic more accurate
            // (Assumes average terrain cost of 1, adjust if needed)
            return distance;
        }

        /// <summary>
        /// Checks if a cell can be traversed during pathfinding
        /// </summary>
        private bool IsTraversable(HexCell cell, PathfindingContext context, bool isGoal)
        {
            // Context-specific obstacles
            if (context.IsObstacle(cell))
                return false;

            // Terrain walkability
            if (!cell.PathfindingState.IsWalkable)
                return false;

            // Occupation checks (goal can be occupied for attack moves)
            if (cell.PathfindingState.IsOccupied && !isGoal)
            {
                // Allow moving through allies if specified
                if (!context.AllowMoveThroughAllies)
                    return false;

                // TODO: Check if occupied by ally when unit system is implemented
            }

            // Reservation check (temporary blocks)
            if (cell.PathfindingState.IsReserved && !isGoal)
                return false;

            // Exploration requirement
            if (context.RequireExplored && !cell.PathfindingState.IsExplored)
                return false;

            return true;
        }

        /// <summary>
        /// Reconstructs the path from start to goal using the cameFrom map
        /// </summary>
        private List<HexCell> ReconstructPath(Dictionary<HexCell, HexCell> cameFrom, HexCell current)
        {
            var path = new List<HexCell> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Resets pathfinding state on all cells in the grid
        /// </summary>
        private void ResetSearchState(HexGrid grid)
        {
            if (grid == null || grid.Cells == null)
                return;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    HexCell cell = grid.Cells[x, y];
                    if (cell != null && cell.PathfindingState != null)
                    {
                        cell.PathfindingState.ResetSearchState();
                    }
                }
            }
        }

        /// <summary>
        /// Creates a path result for the trivial case where start == goal
        /// </summary>
        private PathResult CreateSingleCellPath(HexCell start, HexCell goal)
        {
            var path = new List<HexCell> { start };
            return PathResult.CreateSuccess(start, goal, path, 0, 1, 0f);
        }
    }
}
