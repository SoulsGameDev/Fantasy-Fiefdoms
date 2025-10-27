using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Core
{
    /// <summary>
    /// Contains the result of a pathfinding operation, including the path,
    /// cost information, and diagnostic data.
    /// </summary>
    public class PathResult
    {
        /// <summary>
        /// Whether a valid path was found
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// The sequence of cells forming the path from start to goal.
        /// Empty if no path was found.
        /// </summary>
        public List<HexCell> Path { get; private set; }

        /// <summary>
        /// Total movement cost of the path (sum of terrain costs)
        /// </summary>
        public int TotalCost { get; private set; }

        /// <summary>
        /// Number of nodes explored during the search
        /// </summary>
        public int NodesExplored { get; private set; }

        /// <summary>
        /// Time taken to compute the path in milliseconds
        /// </summary>
        public float ComputationTimeMs { get; private set; }

        /// <summary>
        /// Reason for failure if Success is false
        /// </summary>
        public string FailureReason { get; private set; }

        /// <summary>
        /// Starting cell of the path
        /// </summary>
        public HexCell StartCell { get; private set; }

        /// <summary>
        /// Target cell of the path
        /// </summary>
        public HexCell GoalCell { get; private set; }

        /// <summary>
        /// Cost map showing the cost to reach each explored cell from the start.
        /// Useful for visualization and debugging.
        /// </summary>
        public Dictionary<HexCell, int> CostMap { get; private set; }

        /// <summary>
        /// Parent map used for path reconstruction.
        /// Maps each cell to the cell it came from.
        /// </summary>
        public Dictionary<HexCell, HexCell> CameFrom { get; private set; }

        /// <summary>
        /// Private constructor - use factory methods to create instances
        /// </summary>
        private PathResult()
        {
            Path = new List<HexCell>();
            CostMap = new Dictionary<HexCell, int>();
            CameFrom = new Dictionary<HexCell, HexCell>();
        }

        /// <summary>
        /// Creates a successful path result
        /// </summary>
        public static PathResult CreateSuccess(
            HexCell start,
            HexCell goal,
            List<HexCell> path,
            int totalCost,
            int nodesExplored,
            float computationTimeMs,
            Dictionary<HexCell, int> costMap = null,
            Dictionary<HexCell, HexCell> cameFrom = null)
        {
            return new PathResult
            {
                Success = true,
                StartCell = start,
                GoalCell = goal,
                Path = path ?? new List<HexCell>(),
                TotalCost = totalCost,
                NodesExplored = nodesExplored,
                ComputationTimeMs = computationTimeMs,
                CostMap = costMap ?? new Dictionary<HexCell, int>(),
                CameFrom = cameFrom ?? new Dictionary<HexCell, HexCell>(),
                FailureReason = null
            };
        }

        /// <summary>
        /// Creates a failed path result
        /// </summary>
        public static PathResult CreateFailure(
            HexCell start,
            HexCell goal,
            string reason,
            int nodesExplored = 0,
            float computationTimeMs = 0f)
        {
            return new PathResult
            {
                Success = false,
                StartCell = start,
                GoalCell = goal,
                Path = new List<HexCell>(),
                TotalCost = 0,
                NodesExplored = nodesExplored,
                ComputationTimeMs = computationTimeMs,
                FailureReason = reason,
                CostMap = new Dictionary<HexCell, int>(),
                CameFrom = new Dictionary<HexCell, HexCell>()
            };
        }

        /// <summary>
        /// Returns a formatted string representation of the path result
        /// </summary>
        public override string ToString()
        {
            if (Success)
            {
                return $"Path Found: {Path.Count} cells, Cost: {TotalCost}, " +
                       $"Explored: {NodesExplored} nodes, Time: {ComputationTimeMs:F2}ms";
            }
            else
            {
                return $"Path Failed: {FailureReason}, " +
                       $"Explored: {NodesExplored} nodes, Time: {ComputationTimeMs:F2}ms";
            }
        }

        /// <summary>
        /// Gets the length of the path (number of steps, not movement cost)
        /// </summary>
        public int PathLength => Path.Count;

        /// <summary>
        /// Checks if the path is empty
        /// </summary>
        public bool IsEmpty => Path.Count == 0;

        /// <summary>
        /// Gets the first cell in the path (same as StartCell for valid paths)
        /// </summary>
        public HexCell FirstCell => Path.Count > 0 ? Path[0] : null;

        /// <summary>
        /// Gets the last cell in the path (same as GoalCell for valid paths)
        /// </summary>
        public HexCell LastCell => Path.Count > 0 ? Path[Path.Count - 1] : null;
    }
}
