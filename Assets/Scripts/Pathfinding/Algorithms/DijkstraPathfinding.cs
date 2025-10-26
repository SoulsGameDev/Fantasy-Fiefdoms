using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Pathfinding.Core;
using Pathfinding.DataStructures;

namespace Pathfinding.Algorithms
{
    /// <summary>
    /// Dijkstra's algorithm implementation for hex grids.
    ///
    /// USE CASE: Finding shortest paths from ONE source to MANY/ALL destinations
    ///
    /// WHEN TO USE:
    /// - Computing threat ranges (show all cells an enemy can reach)
    /// - Finding influence zones (area controlled by a unit/building)
    /// - Calculating reachable cells with varying terrain costs
    /// - When you need paths from one source to multiple destinations
    /// - Building distance maps for AI decision making
    ///
    /// ADVANTAGES:
    /// - Finds shortest path to ALL cells in one run
    /// - More efficient than running A* multiple times from same source
    /// - No heuristic needed (simpler than A*)
    /// - Guaranteed optimal paths
    ///
    /// DISADVANTAGES:
    /// - Slower than A* for single source-to-goal queries
    /// - Explores more cells than A* (no heuristic guidance)
    /// - Uses more memory (stores distances to all reachable cells)
    ///
    /// PERFORMANCE:
    /// - Time: O((V + E) log V) where V = cells, E = edges (hex neighbors)
    /// - Space: O(V) to store distances
    /// - Typically explores entire reachable area
    ///
    /// TYPICAL USAGE:
    /// ```csharp
    /// // Get all cells enemy can threaten
    /// var dijkstra = new DijkstraPathfinding();
    /// var context = new PathfindingContext { MaxMovementPoints = enemyMovement };
    /// var result = dijkstra.FindAllPaths(enemyCell, context);
    ///
    /// // Now we have distances to all reachable cells
    /// foreach (var kvp in result.DistanceMap) {
    ///     HexCell cell = kvp.Key;
    ///     int distance = kvp.Value;
    ///     if (distance <= enemyMovement) {
    ///         // This cell is in threat range
    ///     }
    /// }
    /// ```
    /// </summary>
    public class DijkstraPathfinding : IPathfindingAlgorithm
    {
        public string AlgorithmName => "Dijkstra";
        public bool SupportsThreading => false; // Uses Unity API
        public string Description => "Finds shortest paths from one source to all destinations. " +
                                     "Ideal for threat ranges, influence maps, and multi-target pathfinding.";

        /// <summary>
        /// Finds path to a specific goal (implements IPathfindingAlgorithm)
        /// </summary>
        public PathResult FindPath(HexCell start, HexCell goal, PathfindingContext context)
        {
            // For single goal, run full Dijkstra and extract specific path
            var allPaths = FindAllPaths(start, context);

            if (!allPaths.DistanceMap.ContainsKey(goal))
            {
                return PathResult.CreateFailure(start, goal,
                    "Goal not reachable from start",
                    allPaths.NodesExplored, allPaths.ComputationTimeMs);
            }

            // Reconstruct path to goal
            List<HexCell> path = ReconstructPath(allPaths.CameFrom, start, goal);
            int totalCost = allPaths.DistanceMap[goal];

            return PathResult.CreateSuccess(
                start, goal, path, totalCost, allPaths.NodesExplored,
                allPaths.ComputationTimeMs,
                allPaths.DistanceMap, allPaths.CameFrom);
        }

        /// <summary>
        /// Finds shortest paths from start to ALL reachable cells.
        /// This is the main use case for Dijkstra's algorithm.
        /// </summary>
        public DijkstraResult FindAllPaths(HexCell start, PathfindingContext context)
        {
            if (start == null)
                return DijkstraResult.CreateFailure("Start cell is null");

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Reset pathfinding state
            ResetSearchState(start.Grid);

            var openSet = new PriorityQueue<HexCell>(256);
            var distanceMap = new Dictionary<HexCell, int>();
            var cameFrom = new Dictionary<HexCell, HexCell>();
            var visited = new HashSet<HexCell>();
            int nodesExplored = 0;

            // Initialize start
            start.PathfindingState.GCost = 0;
            distanceMap[start] = 0;
            openSet.Enqueue(start, 0);

            // Dijkstra's algorithm main loop
            while (!openSet.IsEmpty)
            {
                // Check node limit
                if (nodesExplored >= context.MaxSearchNodes)
                {
                    stopwatch.Stop();
                    Debug.LogWarning($"Dijkstra search exceeded max nodes ({context.MaxSearchNodes})");
                    break;
                }

                HexCell current = openSet.Dequeue();

                // Skip if already visited (can happen with priority queue updates)
                if (visited.Contains(current))
                    continue;

                visited.Add(current);
                nodesExplored++;

                int currentDistance = distanceMap[current];

                // Check movement limit
                if (context.MaxMovementPoints >= 0 && currentDistance > context.MaxMovementPoints)
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
                    if (!IsTraversable(neighbor, context))
                        continue;

                    // Calculate distance through current
                    int movementCost = context.GetEffectiveMovementCost(neighbor);
                    int newDistance = currentDistance + movementCost;

                    // Check movement limit
                    if (context.MaxMovementPoints >= 0 && newDistance > context.MaxMovementPoints)
                        continue;

                    // If this is a better path to neighbor
                    if (!distanceMap.ContainsKey(neighbor) || newDistance < distanceMap[neighbor])
                    {
                        distanceMap[neighbor] = newDistance;
                        cameFrom[neighbor] = current;
                        neighbor.PathfindingState.GCost = newDistance;

                        openSet.Enqueue(neighbor, newDistance);
                    }
                }
            }

            stopwatch.Stop();

            return DijkstraResult.CreateSuccess(
                start, distanceMap, cameFrom, nodesExplored, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Gets all cells within a certain distance from start
        /// </summary>
        public List<HexCell> GetCellsWithinDistance(HexCell start, int maxDistance, PathfindingContext context)
        {
            var result = FindAllPaths(start, context);
            var cells = new List<HexCell>();

            foreach (var kvp in result.DistanceMap)
            {
                if (kvp.Value <= maxDistance)
                {
                    cells.Add(kvp.Key);
                }
            }

            return cells;
        }

        /// <summary>
        /// Finds paths from start to multiple goals (more efficient than A* per goal)
        /// </summary>
        public Dictionary<HexCell, PathResult> FindPathsToMultipleGoals(
            HexCell start,
            List<HexCell> goals,
            PathfindingContext context)
        {
            var results = new Dictionary<HexCell, PathResult>();
            var allPaths = FindAllPaths(start, context);

            foreach (var goal in goals)
            {
                if (allPaths.DistanceMap.ContainsKey(goal))
                {
                    var path = ReconstructPath(allPaths.CameFrom, start, goal);
                    var result = PathResult.CreateSuccess(
                        start, goal, path, allPaths.DistanceMap[goal],
                        allPaths.NodesExplored, allPaths.ComputationTimeMs);
                    results[goal] = result;
                }
                else
                {
                    results[goal] = PathResult.CreateFailure(start, goal, "Not reachable");
                }
            }

            return results;
        }

        private bool IsTraversable(HexCell cell, PathfindingContext context)
        {
            if (context.IsObstacle(cell))
                return false;

            if (!cell.PathfindingState.IsWalkable)
                return false;

            if (cell.PathfindingState.IsOccupied && !context.AllowMoveThroughAllies)
                return false;

            if (cell.PathfindingState.IsReserved)
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
    }

    /// <summary>
    /// Result of Dijkstra's algorithm containing distances to all reachable cells
    /// </summary>
    public class DijkstraResult
    {
        public bool Success { get; private set; }
        public HexCell StartCell { get; private set; }
        public Dictionary<HexCell, int> DistanceMap { get; private set; }
        public Dictionary<HexCell, HexCell> CameFrom { get; private set; }
        public int NodesExplored { get; private set; }
        public float ComputationTimeMs { get; private set; }
        public string FailureReason { get; private set; }

        private DijkstraResult() { }

        public static DijkstraResult CreateSuccess(
            HexCell start,
            Dictionary<HexCell, int> distanceMap,
            Dictionary<HexCell, HexCell> cameFrom,
            int nodesExplored,
            float computationTimeMs)
        {
            return new DijkstraResult
            {
                Success = true,
                StartCell = start,
                DistanceMap = distanceMap,
                CameFrom = cameFrom,
                NodesExplored = nodesExplored,
                ComputationTimeMs = computationTimeMs
            };
        }

        public static DijkstraResult CreateFailure(string reason)
        {
            return new DijkstraResult
            {
                Success = false,
                FailureReason = reason,
                DistanceMap = new Dictionary<HexCell, int>(),
                CameFrom = new Dictionary<HexCell, HexCell>()
            };
        }

        public override string ToString()
        {
            if (Success)
            {
                return $"Dijkstra: {DistanceMap.Count} cells reachable, " +
                       $"Explored: {NodesExplored}, Time: {ComputationTimeMs:F2}ms";
            }
            return $"Dijkstra Failed: {FailureReason}";
        }
    }
}
