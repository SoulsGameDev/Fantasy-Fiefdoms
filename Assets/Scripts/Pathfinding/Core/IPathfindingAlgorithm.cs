using UnityEngine;

namespace Pathfinding.Core
{
    /// <summary>
    /// Strategy interface for pathfinding algorithms.
    /// Allows swapping between different algorithms (A*, Dijkstra, BFS, etc.)
    /// without changing client code.
    /// </summary>
    public interface IPathfindingAlgorithm
    {
        /// <summary>
        /// Finds a path from start to goal using this algorithm
        /// </summary>
        /// <param name="start">Starting cell</param>
        /// <param name="goal">Target cell</param>
        /// <param name="context">Pathfinding options and constraints</param>
        /// <returns>PathResult containing the path or failure information</returns>
        PathResult FindPath(HexCell start, HexCell goal, PathfindingContext context);

        /// <summary>
        /// Name of the algorithm (e.g., "A*", "Dijkstra", "BFS")
        /// </summary>
        string AlgorithmName { get; }

        /// <summary>
        /// Whether this algorithm supports threaded execution
        /// (i.e., doesn't call Unity API methods that require main thread)
        /// </summary>
        bool SupportsThreading { get; }

        /// <summary>
        /// Description of the algorithm's characteristics and use cases
        /// </summary>
        string Description { get; }
    }
}
