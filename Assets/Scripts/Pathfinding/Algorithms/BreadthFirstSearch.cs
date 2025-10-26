using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Pathfinding.Core;

namespace Pathfinding.Algorithms
{
    /// <summary>
    /// Breadth-First Search (BFS) implementation for hex grids.
    ///
    /// USE CASE: Fast exploration when all terrain costs are EQUAL
    ///
    /// WHEN TO USE:
    /// - Simple reachability checks (can unit reach this cell?)
    /// - Zone of control calculations (which cells does a unit control?)
    /// - Flood fill operations (spread effects, vision, etc.)
    /// - Quick distance measurements when terrain cost doesn't matter
    /// - Finding closest target among many options
    /// - Level/turn generation for roguelikes
    ///
    /// ADVANTAGES:
    /// - Fastest algorithm for unweighted graphs
    /// - Simpler than Dijkstra or A*
    /// - Guaranteed shortest path in terms of steps (not cost)
    /// - Low memory usage (just a queue)
    /// - Explores cells layer by layer (useful for spreading effects)
    ///
    /// DISADVANTAGES:
    /// - Ignores terrain costs (treats all cells as equal)
    /// - Not optimal when terrain has varying costs
    /// - No heuristic guidance (explores uniformly in all directions)
    /// - Can waste time exploring irrelevant areas
    ///
    /// PERFORMANCE:
    /// - Time: O(V + E) where V = cells, E = edges (hex neighbors)
    /// - Space: O(V) for the queue
    /// - Faster than Dijkstra when all costs are 1
    /// - Much faster than A* when you need to explore an area
    ///
    /// TYPICAL USAGE:
    /// ```csharp
    /// // Quick check: can unit reach target?
    /// var bfs = new BreadthFirstSearch();
    /// var result = bfs.FindPath(unitCell, targetCell, context);
    /// if (result.Success) {
    ///     Debug.Log($"Can reach in {result.PathLength} steps");
    /// }
    ///
    /// // Find all cells within 3 steps
    /// var cells = bfs.GetCellsWithinSteps(unitCell, 3, context);
    /// // Useful for abilities with range but no terrain cost
    /// ```
    /// </summary>
    public class BreadthFirstSearch : IPathfindingAlgorithm
    {
        public string AlgorithmName => "BFS";
        public bool SupportsThreading => false; // Uses Unity API
        public string Description => "Fast pathfinding for uniform-cost terrain. " +
                                     "Ideal for simple reachability, zone control, and flood fill operations.";

        /// <summary>
        /// Finds shortest path in terms of STEPS (ignoring terrain costs)
        /// </summary>
        public PathResult FindPath(HexCell start, HexCell goal, PathfindingContext context)
        {
            if (start == null || goal == null)
                return PathResult.CreateFailure(start, goal, "Start or goal is null");

            if (start == goal)
                return CreateSingleCellPath(start, goal);

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Reset state
            ResetSearchState(start.Grid);

            var queue = new Queue<HexCell>();
            var visited = new HashSet<HexCell>();
            var cameFrom = new Dictionary<HexCell, HexCell>();
            var distance = new Dictionary<HexCell, int>();
            int nodesExplored = 0;

            // Initialize
            queue.Enqueue(start);
            visited.Add(start);
            distance[start] = 0;

            // BFS main loop
            while (queue.Count > 0)
            {
                if (nodesExplored >= context.MaxSearchNodes)
                {
                    stopwatch.Stop();
                    return PathResult.CreateFailure(start, goal,
                        $"Search exceeded max nodes ({context.MaxSearchNodes})",
                        nodesExplored, stopwatch.ElapsedMilliseconds);
                }

                HexCell current = queue.Dequeue();
                nodesExplored++;

                // Goal found!
                if (current == goal)
                {
                    stopwatch.Stop();
                    List<HexCell> path = ReconstructPath(cameFrom, start, goal);
                    int steps = distance[goal];

                    return PathResult.CreateSuccess(
                        start, goal, path, steps, nodesExplored,
                        stopwatch.ElapsedMilliseconds,
                        distance, cameFrom);
                }

                // Check movement limit (in steps, not cost)
                if (context.MaxMovementPoints >= 0 && distance[current] >= context.MaxMovementPoints)
                    continue;

                // Explore neighbors
                List<HexCell> neighbors = current.GetNeighbors();
                if (neighbors == null)
                    continue;

                foreach (HexCell neighbor in neighbors)
                {
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    // Check traversability
                    if (!IsTraversable(neighbor, context, neighbor == goal))
                        continue;

                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    distance[neighbor] = distance[current] + 1; // Always +1 for BFS
                    queue.Enqueue(neighbor);
                }
            }

            // No path found
            stopwatch.Stop();
            return PathResult.CreateFailure(start, goal, "No path exists",
                nodesExplored, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Gets all cells reachable within N steps (ignoring terrain costs)
        /// </summary>
        public List<HexCell> GetCellsWithinSteps(HexCell start, int maxSteps, PathfindingContext context)
        {
            if (start == null || maxSteps < 0)
                return new List<HexCell>();

            var reachable = new List<HexCell>();
            var queue = new Queue<(HexCell cell, int steps)>();
            var visited = new HashSet<HexCell>();

            queue.Enqueue((start, 0));
            visited.Add(start);
            reachable.Add(start);

            while (queue.Count > 0)
            {
                var (current, currentSteps) = queue.Dequeue();

                if (currentSteps >= maxSteps)
                    continue;

                var neighbors = current.GetNeighbors();
                if (neighbors == null)
                    continue;

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    if (!IsTraversable(neighbor, context, false))
                        continue;

                    visited.Add(neighbor);
                    reachable.Add(neighbor);
                    queue.Enqueue((neighbor, currentSteps + 1));
                }
            }

            return reachable;
        }

        /// <summary>
        /// Finds the closest cell from a list of targets
        /// </summary>
        public HexCell FindClosestTarget(HexCell start, List<HexCell> targets, PathfindingContext context)
        {
            if (start == null || targets == null || targets.Count == 0)
                return null;

            var targetsSet = new HashSet<HexCell>(targets);
            var queue = new Queue<(HexCell cell, int steps)>();
            var visited = new HashSet<HexCell>();

            queue.Enqueue((start, 0));
            visited.Add(start);

            while (queue.Count > 0)
            {
                var (current, steps) = queue.Dequeue();

                // Found a target!
                if (targetsSet.Contains(current))
                    return current;

                var neighbors = current.GetNeighbors();
                if (neighbors == null)
                    continue;

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    if (!IsTraversable(neighbor, context, false))
                        continue;

                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, steps + 1));
                }
            }

            return null; // No target reachable
        }

        /// <summary>
        /// Performs flood fill from start, returning all connected cells
        /// Useful for region detection, vision, etc.
        /// </summary>
        public List<HexCell> FloodFill(HexCell start, PathfindingContext context, int maxCells = -1)
        {
            if (start == null)
                return new List<HexCell>();

            var filled = new List<HexCell>();
            var queue = new Queue<HexCell>();
            var visited = new HashSet<HexCell>();

            queue.Enqueue(start);
            visited.Add(start);
            filled.Add(start);

            while (queue.Count > 0)
            {
                if (maxCells > 0 && filled.Count >= maxCells)
                    break;

                HexCell current = queue.Dequeue();

                var neighbors = current.GetNeighbors();
                if (neighbors == null)
                    continue;

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    if (!IsTraversable(neighbor, context, false))
                        continue;

                    visited.Add(neighbor);
                    filled.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            return filled;
        }

        /// <summary>
        /// Gets distance in steps from start to all reachable cells
        /// Similar to Dijkstra but without terrain costs
        /// </summary>
        public Dictionary<HexCell, int> GetDistanceMap(HexCell start, PathfindingContext context)
        {
            var distanceMap = new Dictionary<HexCell, int>();

            if (start == null)
                return distanceMap;

            var queue = new Queue<(HexCell cell, int steps)>();
            var visited = new HashSet<HexCell>();

            queue.Enqueue((start, 0));
            visited.Add(start);
            distanceMap[start] = 0;

            while (queue.Count > 0)
            {
                var (current, steps) = queue.Dequeue();

                var neighbors = current.GetNeighbors();
                if (neighbors == null)
                    continue;

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    if (!IsTraversable(neighbor, context, false))
                        continue;

                    visited.Add(neighbor);
                    distanceMap[neighbor] = steps + 1;
                    queue.Enqueue((neighbor, steps + 1));
                }
            }

            return distanceMap;
        }

        private bool IsTraversable(HexCell cell, PathfindingContext context, bool isGoal)
        {
            if (context.IsObstacle(cell))
                return false;

            if (!cell.PathfindingState.IsWalkable)
                return false;

            if (cell.PathfindingState.IsOccupied && !isGoal && !context.AllowMoveThroughAllies)
                return false;

            if (cell.PathfindingState.IsReserved && !isGoal)
                return false;

            if (context.RequireExplored && !cell.PathfindingState.IsExplored)
                return false;

            return true;
        }

        private List<HexCell> ReconstructPath(Dictionary<HexCell, HexCell> cameFrom, HexCell start, HexCell goal)
        {
            var path = new List<HexCell> { goal };
            HexCell current = goal;

            while (cameFrom.ContainsKey(current) && current != start)
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

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

        private PathResult CreateSingleCellPath(HexCell start, HexCell goal)
        {
            var path = new List<HexCell> { start };
            return PathResult.CreateSuccess(start, goal, path, 0, 1, 0f);
        }
    }
}
