using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Pathfinding.Core;
using Pathfinding.DataStructures;

namespace Pathfinding.Algorithms
{
    /// <summary>
    /// Best-First Search (Greedy) pathfinding for hex grids.
    ///
    /// USE CASE: FAST pathfinding when APPROXIMATE paths are acceptable
    ///
    /// WHEN TO USE:
    /// - Real-time strategy games (need speed over optimality)
    /// - AI pathfinding where "good enough" paths are fine
    /// - Preview paths (show approximate path quickly)
    /// - Situations where performance > path quality
    /// - NPCs with limited intelligence (don't need perfect paths)
    /// - Fast prototyping and testing
    ///
    /// ADVANTAGES:
    /// - FASTEST heuristic-based algorithm
    /// - Very simple implementation
    /// - Low memory usage
    /// - Good for straight-line scenarios
    /// - Often finds "good enough" paths quickly
    /// - Useful for real-time constraints
    ///
    /// DISADVANTAGES:
    /// - Does NOT guarantee optimal path
    /// - Can get stuck in dead ends
    /// - No cost consideration (only distance to goal)
    /// - May produce significantly longer paths
    /// - Poor performance with obstacles
    /// - Unpredictable behavior in complex terrain
    ///
    /// PERFORMANCE:
    /// - Best case: O(n) where n = straight-line distance
    /// - Worst case: Can explore entire map like BFS
    /// - Typically 2-5x faster than A* on open terrain
    /// - Path quality: 1-3x longer than optimal in worst case
    ///
    /// HOW IT WORKS:
    /// - Only uses heuristic (distance to goal)
    /// - Always expands cell closest to goal
    /// - No path cost tracking = faster but not optimal
    /// - "Bee-line" approach: heads straight for goal
    ///
    /// TYPICAL USAGE:
    /// ```csharp
    /// // Quick path for enemy AI
    /// var greedy = new BestFirstSearch();
    /// var result = greedy.FindPath(enemyPos, playerPos, context);
    ///
    /// // Fast but not perfect - acceptable for AI
    /// if (result.Success) {
    ///     enemy.FollowPath(result.Path);
    /// }
    /// ```
    /// </summary>
    public class BestFirstSearch : IPathfindingAlgorithm
    {
        public string AlgorithmName => "Best-First (Greedy)";
        public bool SupportsThreading => false; // Uses Unity API
        public string Description => "Very fast but non-optimal pathfinding. " +
                                     "Ideal for AI and situations where speed > path quality.";

        /// <summary>
        /// Finds path using greedy best-first search
        /// Warning: Path may not be optimal!
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

            var openSet = new PriorityQueue<HexCell>(256);
            var cameFrom = new Dictionary<HexCell, HexCell>();
            var visited = new HashSet<HexCell>();
            var costs = new Dictionary<HexCell, int>(); // Track costs for result
            int nodesExplored = 0;

            // Initialize - only use heuristic, no g-cost!
            openSet.Enqueue(start, CalculateHeuristic(start, goal));
            costs[start] = 0;

            while (!openSet.IsEmpty)
            {
                if (nodesExplored >= context.MaxSearchNodes)
                {
                    stopwatch.Stop();
                    return PathResult.CreateFailure(start, goal,
                        $"Search exceeded max nodes ({context.MaxSearchNodes})",
                        nodesExplored, stopwatch.ElapsedMilliseconds);
                }

                HexCell current = openSet.Dequeue();

                // Skip if already visited
                if (visited.Contains(current))
                    continue;

                visited.Add(current);
                nodesExplored++;

                // Goal reached!
                if (current == goal)
                {
                    stopwatch.Stop();
                    List<HexCell> path = ReconstructPath(cameFrom, start, goal);
                    int totalCost = costs[goal];

                    return PathResult.CreateSuccess(
                        start, goal, path, totalCost, nodesExplored,
                        stopwatch.ElapsedMilliseconds);
                }

                // Explore neighbors
                List<HexCell> neighbors = current.GetNeighbors();
                if (neighbors == null)
                    continue;

                foreach (HexCell neighbor in neighbors)
                {
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    if (!IsTraversable(neighbor, context, neighbor == goal))
                        continue;

                    // Track cost for result (even though we don't use it for priority)
                    int moveCost = context.GetEffectiveMovementCost(neighbor);
                    int newCost = costs[current] + moveCost;

                    if (!costs.ContainsKey(neighbor) || newCost < costs[neighbor])
                    {
                        costs[neighbor] = newCost;
                        cameFrom[neighbor] = current;

                        // Key difference from A*: only heuristic, no g-cost in priority!
                        int priority = CalculateHeuristic(neighbor, goal);
                        openSet.Enqueue(neighbor, priority);
                    }
                }
            }

            // No path found
            stopwatch.Stop();
            return PathResult.CreateFailure(start, goal, "No path exists",
                nodesExplored, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Quick check if goal is reachable (may not find shortest path)
        /// </summary>
        public bool IsReachable(HexCell start, HexCell goal, PathfindingContext context, int maxNodes = 1000)
        {
            var tempContext = context.Clone();
            tempContext.MaxSearchNodes = maxNodes;

            var result = FindPath(start, goal, tempContext);
            return result.Success;
        }

        /// <summary>
        /// Finds approximate closest target from a list
        /// Fast but may not find true closest
        /// </summary>
        public HexCell FindClosestTarget(HexCell start, List<HexCell> targets, PathfindingContext context)
        {
            if (targets == null || targets.Count == 0)
                return null;

            // Sort targets by heuristic distance
            var sortedTargets = new List<(HexCell cell, int dist)>();
            foreach (var target in targets)
            {
                int dist = CalculateHeuristic(start, target);
                sortedTargets.Add((target, dist));
            }

            sortedTargets.Sort((a, b) => a.dist.CompareTo(b.dist));

            // Try to reach each target in order of heuristic distance
            foreach (var (target, _) in sortedTargets)
            {
                var result = FindPath(start, target, context);
                if (result.Success)
                    return target;
            }

            return null;
        }

        private int CalculateHeuristic(HexCell from, HexCell to)
        {
            Vector3 fromCube = from.CubeCoordinates;
            Vector3 toCube = to.CubeCoordinates;

            int distance = (int)((Mathf.Abs(fromCube.x - toCube.x) +
                                  Mathf.Abs(fromCube.y - toCube.y) +
                                  Mathf.Abs(fromCube.z - toCube.z)) / 2);

            return distance;
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
