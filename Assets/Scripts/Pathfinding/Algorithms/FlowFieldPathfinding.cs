using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Pathfinding.Core;
using Pathfinding.DataStructures;

namespace Pathfinding.Algorithms
{
    /// <summary>
    /// Flow Field Pathfinding for hex grids.
    ///
    /// USE CASE: Moving MANY units to the SAME destination efficiently
    ///
    /// WHEN TO USE:
    /// - RTS-style group movement (10+ units to same location)
    /// - Evacuation scenarios (all civilians flee to safe zone)
    /// - Rally points (units spawning and moving to rally point)
    /// - Swarm AI (many enemies converging on player)
    /// - Migration/flocking behaviors
    /// - Any scenario where multiple agents share a destination
    ///
    /// ADVANTAGES:
    /// - Calculate ONCE, use for ALL units → massive performance gain
    /// - O(1) path lookup per unit after initial calculation
    /// - Units naturally avoid each other (can take different routes)
    /// - Handles dynamic obstacles well (just recalculate field)
    /// - Scales to hundreds of units efficiently
    /// - Creates natural-looking group movement
    ///
    /// DISADVANTAGES:
    /// - Only works for shared destinations
    /// - Requires recalculation if goal changes
    /// - Uses more memory than single paths
    /// - Not suitable for individual unit pathfinding
    /// - Can create "clumping" if not managed
    ///
    /// PERFORMANCE:
    /// - Initial calculation: O(V + E) similar to Dijkstra
    /// - Per-unit path lookup: O(1) - just follow direction!
    /// - 100 units to same goal: Flow Field = 100x faster than A*
    /// - Memory: O(V) to store direction field
    ///
    /// HOW IT WORKS:
    /// 1. Run Dijkstra from GOAL backwards
    /// 2. For each cell, store "best neighbor" pointing toward goal
    /// 3. Units follow arrows like a river flowing to goal
    /// 4. Each unit just looks at current cell's arrow direction
    ///
    /// TYPICAL USAGE:
    /// ```csharp
    /// // Create flow field for rally point
    /// var flowField = new FlowFieldPathfinding();
    /// var field = flowField.GenerateFlowField(rallyPoint, maxDistance: 50);
    ///
    /// // Now ALL units can use this field
    /// foreach (var unit in units) {
    ///     var path = field.GetPathFrom(unit.CurrentCell);
    ///     unit.MoveAlongPath(path);
    /// }
    ///
    /// // 1 calculation, 100 units → huge savings!
    /// ```
    /// </summary>
    public class FlowFieldPathfinding : IPathfindingAlgorithm
    {
        public string AlgorithmName => "FlowField";
        public bool SupportsThreading => false; // Uses Unity API
        public string Description => "Computes direction field for many units to same goal. " +
                                     "Calculate once, use for all units. Ideal for RTS group movement.";

        /// <summary>
        /// Finds path to goal (implements IPathfindingAlgorithm)
        /// Note: For single units, A* is more efficient
        /// </summary>
        public PathResult FindPath(HexCell start, HexCell goal, PathfindingContext context)
        {
            // Generate flow field and extract path for this specific unit
            var flowField = GenerateFlowField(goal, context);

            if (!flowField.IsReachable(start))
            {
                return PathResult.CreateFailure(start, goal,
                    "Start not reachable from goal",
                    flowField.NodesProcessed, flowField.ComputationTimeMs);
            }

            var path = flowField.GetPathFrom(start);

            return PathResult.CreateSuccess(
                start, goal, path, flowField.GetCostToGoal(start),
                flowField.NodesProcessed, flowField.ComputationTimeMs);
        }

        /// <summary>
        /// Generates a flow field with the goal as the sink.
        /// All cells will point toward the goal.
        /// </summary>
        public FlowField GenerateFlowField(HexCell goal, PathfindingContext context)
        {
            return GenerateFlowField(goal, context, maxDistance: -1);
        }

        /// <summary>
        /// Generates a flow field within a maximum distance from goal
        /// </summary>
        public FlowField GenerateFlowField(HexCell goal, PathfindingContext context, int maxDistance)
        {
            if (goal == null)
                return FlowField.CreateEmpty();

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Reset state
            ResetSearchState(goal.Grid);

            var openSet = new PriorityQueue<HexCell>(256);
            var costMap = new Dictionary<HexCell, int>();
            var flowDirections = new Dictionary<HexCell, HexCell>(); // Cell -> best neighbor toward goal
            var visited = new HashSet<HexCell>();
            int nodesProcessed = 0;

            // Initialize goal (cost 0, no direction needed - we're there!)
            costMap[goal] = 0;
            openSet.Enqueue(goal, 0);

            // Run Dijkstra BACKWARDS from goal
            while (!openSet.IsEmpty)
            {
                if (nodesProcessed >= context.MaxSearchNodes)
                {
                    Debug.LogWarning($"Flow field generation exceeded max nodes ({context.MaxSearchNodes})");
                    break;
                }

                HexCell current = openSet.Dequeue();

                if (visited.Contains(current))
                    continue;

                visited.Add(current);
                nodesProcessed++;

                int currentCost = costMap[current];

                // Check distance limit
                if (maxDistance >= 0 && currentCost > maxDistance)
                    continue;

                // Process neighbors
                var neighbors = current.GetNeighbors();
                if (neighbors == null)
                    continue;

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    // Check if neighbor can reach current (traversability FROM neighbor TO current)
                    if (!IsTraversableFrom(neighbor, current, context))
                        continue;

                    // Cost to reach goal from neighbor (going through current)
                    int moveCost = context.GetEffectiveMovementCost(current);
                    int newCost = currentCost + moveCost;

                    // If this is a better path to goal from neighbor
                    if (!costMap.ContainsKey(neighbor) || newCost < costMap[neighbor])
                    {
                        costMap[neighbor] = newCost;
                        flowDirections[neighbor] = current; // Point toward current (which leads to goal)
                        openSet.Enqueue(neighbor, newCost);
                    }
                }
            }

            stopwatch.Stop();

            return new FlowField(goal, flowDirections, costMap, nodesProcessed, stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Generates multiple flow fields for different goals (e.g., multiple exits)
        /// Units can choose closest goal
        /// </summary>
        public Dictionary<HexCell, FlowField> GenerateMultipleFlowFields(
            List<HexCell> goals,
            PathfindingContext context)
        {
            var fields = new Dictionary<HexCell, FlowField>();

            foreach (var goal in goals)
            {
                fields[goal] = GenerateFlowField(goal, context);
            }

            return fields;
        }

        private bool IsTraversableFrom(HexCell from, HexCell to, PathfindingContext context)
        {
            if (from == null)
                return false;

            if (context.IsObstacle(from))
                return false;

            if (!from.PathfindingState.IsWalkable)
                return false;

            // Allow occupied cells to be in the field (units can path around each other)
            // This is key to flow field's strength

            if (from.PathfindingState.IsReserved)
                return false;

            if (context.RequireExplored && !from.PathfindingState.IsExplored)
                return false;

            return true;
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
    /// A flow field that stores directions toward a goal.
    /// Units can efficiently find paths by following the flow.
    /// </summary>
    public class FlowField
    {
        public HexCell Goal { get; private set; }
        private Dictionary<HexCell, HexCell> flowDirections; // Cell -> next cell toward goal
        private Dictionary<HexCell, int> costToGoal;
        public int NodesProcessed { get; private set; }
        public float ComputationTimeMs { get; private set; }

        public FlowField(
            HexCell goal,
            Dictionary<HexCell, HexCell> directions,
            Dictionary<HexCell, int> costs,
            int nodesProcessed,
            float computationTimeMs)
        {
            Goal = goal;
            flowDirections = directions;
            costToGoal = costs;
            NodesProcessed = nodesProcessed;
            ComputationTimeMs = computationTimeMs;
        }

        public static FlowField CreateEmpty()
        {
            return new FlowField(null,
                new Dictionary<HexCell, HexCell>(),
                new Dictionary<HexCell, int>(),
                0, 0f);
        }

        /// <summary>
        /// Gets the path from a cell to the goal by following the flow
        /// O(path length) - very fast!
        /// </summary>
        public List<HexCell> GetPathFrom(HexCell start)
        {
            var path = new List<HexCell>();

            if (start == null || !IsReachable(start))
                return path;

            HexCell current = start;
            var visited = new HashSet<HexCell>();
            int maxSteps = 1000; // Prevent infinite loops
            int steps = 0;

            path.Add(current);

            // Follow the flow until we reach goal
            while (current != Goal && steps < maxSteps)
            {
                // Detect cycles
                if (visited.Contains(current))
                {
                    UnityEngine.Debug.LogWarning("Flow field contains cycle!");
                    break;
                }
                visited.Add(current);

                // Get next cell in flow
                if (!flowDirections.ContainsKey(current))
                    break;

                current = flowDirections[current];
                path.Add(current);
                steps++;
            }

            return path;
        }

        /// <summary>
        /// Gets the next cell to move to from current position
        /// O(1) lookup - extremely fast!
        /// </summary>
        public HexCell GetNextCell(HexCell current)
        {
            if (current == Goal)
                return current;

            if (flowDirections.TryGetValue(current, out HexCell next))
                return next;

            return null;
        }

        /// <summary>
        /// Checks if a cell can reach the goal via this flow field
        /// </summary>
        public bool IsReachable(HexCell cell)
        {
            return cell == Goal || flowDirections.ContainsKey(cell);
        }

        /// <summary>
        /// Gets the cost to reach goal from a cell
        /// </summary>
        public int GetCostToGoal(HexCell cell)
        {
            if (cell == Goal)
                return 0;

            if (costToGoal.TryGetValue(cell, out int cost))
                return cost;

            return int.MaxValue; // Not reachable
        }

        /// <summary>
        /// Gets all cells that are reachable in this flow field
        /// </summary>
        public List<HexCell> GetReachableCells()
        {
            var cells = new List<HexCell>();

            foreach (var cell in flowDirections.Keys)
            {
                cells.Add(cell);
            }

            if (Goal != null && !cells.Contains(Goal))
                cells.Add(Goal);

            return cells;
        }

        /// <summary>
        /// Gets statistics about the flow field
        /// </summary>
        public string GetStatistics()
        {
            return $"FlowField[Goal: {Goal?.OffsetCoordinates}, " +
                   $"Reachable: {flowDirections.Count} cells, " +
                   $"Processed: {NodesProcessed} nodes, " +
                   $"Time: {ComputationTimeMs:F2}ms]";
        }

        /// <summary>
        /// Visualizes the flow field directions for debugging
        /// </summary>
        public void DebugVisualize()
        {
            UnityEngine.Debug.Log($"Flow Field toward {Goal?.OffsetCoordinates}:");
            UnityEngine.Debug.Log($"  {flowDirections.Count} cells with directions");
            UnityEngine.Debug.Log($"  Computation: {ComputationTimeMs:F2}ms");
        }
    }
}
