using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Pathfinding.Algorithms;

namespace Pathfinding.Core
{
    /// <summary>
    /// Singleton manager for all pathfinding operations.
    /// Provides high-level pathfinding API with caching, algorithm selection, and threading support.
    /// </summary>
    public class PathfindingManager : Singleton<PathfindingManager>
    {
        // ========== ALGORITHM SELECTION ==========

        [Header("Algorithm Settings")]
        [SerializeField]
        [Tooltip("The pathfinding algorithm to use")]
        private AlgorithmType defaultAlgorithm = AlgorithmType.AStar;

        private Dictionary<AlgorithmType, IPathfindingAlgorithm> algorithms;
        private IPathfindingAlgorithm currentAlgorithm;

        /// <summary>
        /// Available pathfinding algorithms
        /// </summary>
        public enum AlgorithmType
        {
            AStar,              // Optimal paths with heuristic (default)
            Dijkstra,           // All shortest paths from source
            BFS,                // Fast unweighted pathfinding
            BestFirst,          // Fast greedy pathfinding (non-optimal)
            BidirectionalAStar, // Fast for long paths
            FlowField           // Many units to same destination
        }

        /// <summary>
        /// Gets or sets the currently active pathfinding algorithm
        /// </summary>
        public IPathfindingAlgorithm CurrentAlgorithm
        {
            get => currentAlgorithm;
            set => currentAlgorithm = value ?? throw new ArgumentNullException(nameof(value));
        }

        // ========== CACHING ==========

        [Header("Cache Settings")]
        [SerializeField]
        [Tooltip("Whether to cache path results")]
        private bool enableCaching = true;

        [SerializeField]
        [Tooltip("How long cached paths remain valid (seconds)")]
        private float cacheDuration = 5f;

        [SerializeField]
        [Tooltip("Maximum number of cached paths")]
        private int maxCacheSize = 100;

        private Dictionary<(HexCell, HexCell), CachedPath> pathCache;

        private struct CachedPath
        {
            public PathResult Result;
            public float Timestamp;
        }

        // ========== PERFORMANCE MONITORING ==========

        [Header("Performance")]
        [SerializeField]
        [Tooltip("Log performance statistics")]
        private bool logPerformance = false;

        private int totalPathsFound = 0;
        private int totalCacheHits = 0;
        private float totalComputationTime = 0f;

        // ========== EVENTS ==========

        /// <summary>
        /// Fired when a path is successfully found
        /// </summary>
        public event Action<PathResult> OnPathFound;

        /// <summary>
        /// Fired when pathfinding fails
        /// </summary>
        public event Action<PathResult> OnPathFailed;

        /// <summary>
        /// Fired when reachable cells are calculated
        /// </summary>
        public event Action<List<HexCell>> OnReachableCellsCalculated;

        // ========== INITIALIZATION ==========

        protected override void Awake()
        {
            base.Awake();
            InitializeAlgorithms();
            InitializeCache();
        }

        private void InitializeAlgorithms()
        {
            algorithms = new Dictionary<AlgorithmType, IPathfindingAlgorithm>
            {
                { AlgorithmType.AStar, new AStarPathfinding() },
                { AlgorithmType.Dijkstra, new DijkstraPathfinding() },
                { AlgorithmType.BFS, new BreadthFirstSearch() },
                { AlgorithmType.BestFirst, new BestFirstSearch() },
                { AlgorithmType.BidirectionalAStar, new BidirectionalAStar() },
                { AlgorithmType.FlowField, new FlowFieldPathfinding() }
            };

            currentAlgorithm = algorithms[defaultAlgorithm];
        }

        private void InitializeCache()
        {
            pathCache = new Dictionary<(HexCell, HexCell), CachedPath>(maxCacheSize);
        }

        // ========== MAIN PATHFINDING API ==========

        /// <summary>
        /// Finds a path from start to goal using default settings
        /// </summary>
        public PathResult FindPath(HexCell start, HexCell goal)
        {
            return FindPath(start, goal, new PathfindingContext());
        }

        /// <summary>
        /// Finds a path from start to goal with custom context
        /// </summary>
        public PathResult FindPath(HexCell start, HexCell goal, PathfindingContext context)
        {
            if (start == null || goal == null)
            {
                Debug.LogError("PathfindingManager: Start or goal is null");
                return PathResult.CreateFailure(start, goal, "Start or goal is null");
            }

            // Check cache if enabled
            if (enableCaching && context.UseCaching)
            {
                var cached = GetCachedPath(start, goal);
                if (cached != null)
                {
                    totalCacheHits++;
                    if (logPerformance)
                        Debug.Log($"Cache hit for path {start.OffsetCoordinates} -> {goal.OffsetCoordinates}");
                    return cached;
                }
            }

            // Compute path using current algorithm
            PathResult result = currentAlgorithm.FindPath(start, goal, context);

            // Update statistics
            totalPathsFound++;
            totalComputationTime += result.ComputationTimeMs;

            // Cache result if successful
            if (result.Success && enableCaching && context.UseCaching)
            {
                CachePath(start, goal, result);
            }

            // Log performance
            if (logPerformance)
            {
                Debug.Log($"Pathfinding: {result}");
            }

            // Fire events
            if (result.Success)
                OnPathFound?.Invoke(result);
            else
                OnPathFailed?.Invoke(result);

            return result;
        }

        /// <summary>
        /// Finds a path asynchronously (runs on background thread if algorithm supports it)
        /// </summary>
        public async Task<PathResult> FindPathAsync(HexCell start, HexCell goal, PathfindingContext context)
        {
            // Check cache synchronously
            if (enableCaching && context.UseCaching)
            {
                var cached = GetCachedPath(start, goal);
                if (cached != null)
                    return cached;
            }

            PathResult result;

            if (currentAlgorithm.SupportsThreading)
            {
                // Run on background thread
                result = await Task.Run(() => currentAlgorithm.FindPath(start, goal, context));
            }
            else
            {
                // Run on main thread (algorithms using Unity API)
                result = currentAlgorithm.FindPath(start, goal, context);
                await Task.Yield(); // Yield to prevent blocking
            }

            // Cache and fire events on main thread
            if (result.Success && enableCaching && context.UseCaching)
            {
                CachePath(start, goal, result);
            }

            if (result.Success)
                OnPathFound?.Invoke(result);
            else
                OnPathFailed?.Invoke(result);

            return result;
        }

        /// <summary>
        /// Gets all cells reachable from start within maxMovement cost
        /// Uses Dijkstra-like expansion from the start point.
        /// </summary>
        public List<HexCell> GetReachableCells(HexCell start, int maxMovement)
        {
            if (start == null || maxMovement < 0)
                return new List<HexCell>();

            var context = new PathfindingContext
            {
                MaxMovementPoints = maxMovement,
                RequireExplored = true
            };

            var reachable = new List<HexCell>();
            var visited = new HashSet<HexCell>();
            var queue = new Queue<(HexCell cell, int cost)>();

            queue.Enqueue((start, 0));
            visited.Add(start);
            reachable.Add(start);

            while (queue.Count > 0)
            {
                var (current, currentCost) = queue.Dequeue();

                var neighbors = current.GetNeighbors();
                if (neighbors == null)
                    continue;

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    // Check traversability
                    if (!neighbor.PathfindingState.IsWalkable)
                        continue;

                    if (context.RequireExplored && !neighbor.PathfindingState.IsExplored)
                        continue;

                    // Calculate cost to reach neighbor
                    int moveCost = context.GetEffectiveMovementCost(neighbor);
                    int totalCost = currentCost + moveCost;

                    if (totalCost <= maxMovement)
                    {
                        visited.Add(neighbor);
                        reachable.Add(neighbor);
                        neighbor.PathfindingState.IsReachable = true;
                        queue.Enqueue((neighbor, totalCost));
                    }
                }
            }

            OnReachableCellsCalculated?.Invoke(reachable);
            return reachable;
        }

        // ========== MULTI-TURN PATHFINDING ==========

        /// <summary>
        /// Finds a multi-turn path when the destination is beyond single-turn movement range.
        /// Splits the path into segments that respect per-turn movement limits.
        /// </summary>
        public MultiTurnPathResult FindMultiTurnPath(HexCell start, HexCell goal, int movementPerTurn)
        {
            return FindMultiTurnPath(start, goal, movementPerTurn, new PathfindingContext());
        }

        /// <summary>
        /// Finds a multi-turn path with custom context.
        /// Automatically splits long paths into turn-based segments.
        /// </summary>
        public MultiTurnPathResult FindMultiTurnPath(
            HexCell start,
            HexCell goal,
            int movementPerTurn,
            PathfindingContext context)
        {
            if (start == null || goal == null)
            {
                return MultiTurnPathResult.CreateFailure(start, goal,
                    "Start or goal is null", movementPerTurn);
            }

            if (movementPerTurn <= 0)
            {
                return MultiTurnPathResult.CreateFailure(start, goal,
                    "Movement per turn must be positive", movementPerTurn);
            }

            // Find the complete path without movement limit
            var unlimitedContext = context.Clone();
            unlimitedContext.MaxMovementPoints = -1; // Remove movement limit for initial search
            unlimitedContext.UseCaching = false; // Don't cache unlimited paths

            PathResult completePath = FindPath(start, goal, unlimitedContext);

            if (!completePath.Success)
            {
                return MultiTurnPathResult.CreateFailure(start, goal,
                    completePath.FailureReason, movementPerTurn);
            }

            // Split path into turn segments
            return MultiTurnPathResult.CreateFromSinglePath(completePath, movementPerTurn, context);
        }

        /// <summary>
        /// Finds a multi-turn path asynchronously
        /// </summary>
        public async Task<MultiTurnPathResult> FindMultiTurnPathAsync(
            HexCell start,
            HexCell goal,
            int movementPerTurn,
            PathfindingContext context)
        {
            MultiTurnPathResult result;

            if (currentAlgorithm.SupportsThreading)
            {
                // Run on background thread
                result = await Task.Run(() => FindMultiTurnPath(start, goal, movementPerTurn, context));
            }
            else
            {
                // Run on main thread
                result = FindMultiTurnPath(start, goal, movementPerTurn, context);
                await Task.Yield();
            }

            return result;
        }

        /// <summary>
        /// Gets all cells reachable within N turns from start position.
        /// Returns cells grouped by turn they can be reached in.
        /// </summary>
        public Dictionary<int, List<HexCell>> GetMultiTurnReachableCells(
            HexCell start,
            int movementPerTurn,
            int maxTurns)
        {
            var cellsByTurn = new Dictionary<int, List<HexCell>>();

            if (start == null || movementPerTurn <= 0 || maxTurns <= 0)
                return cellsByTurn;

            var visited = new HashSet<HexCell>();
            var queue = new Queue<(HexCell cell, int cost, int turn)>();

            // Initialize with start cell
            queue.Enqueue((start, 0, 0));
            visited.Add(start);
            cellsByTurn[0] = new List<HexCell> { start };

            var context = new PathfindingContext
            {
                RequireExplored = true
            };

            while (queue.Count > 0)
            {
                var (current, currentCost, currentTurn) = queue.Dequeue();

                // Stop if we've reached max turns
                if (currentTurn >= maxTurns)
                    continue;

                var neighbors = current.GetNeighbors();
                if (neighbors == null)
                    continue;

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null || visited.Contains(neighbor))
                        continue;

                    if (!neighbor.PathfindingState.IsWalkable)
                        continue;

                    if (context.RequireExplored && !neighbor.PathfindingState.IsExplored)
                        continue;

                    int moveCost = context.GetEffectiveMovementCost(neighbor);
                    int newCost = currentCost + moveCost;
                    int newTurn = currentTurn;

                    // Check if we need to move to next turn
                    if (newCost > movementPerTurn)
                    {
                        newTurn++;
                        newCost = moveCost; // Reset cost for new turn

                        if (newTurn >= maxTurns)
                            continue;
                    }

                    visited.Add(neighbor);

                    // Add to appropriate turn list
                    if (!cellsByTurn.ContainsKey(newTurn))
                    {
                        cellsByTurn[newTurn] = new List<HexCell>();
                    }
                    cellsByTurn[newTurn].Add(neighbor);

                    queue.Enqueue((neighbor, newCost, newTurn));
                }
            }

            return cellsByTurn;
        }

        /// <summary>
        /// Estimates how many turns it would take to reach goal from start.
        /// Uses straight-line distance and average terrain cost.
        /// </summary>
        public int EstimateTurnsToReach(HexCell start, HexCell goal, int movementPerTurn)
        {
            if (start == null || goal == null || movementPerTurn <= 0)
                return -1;

            // Calculate hex distance
            Vector3 startCube = start.CubeCoordinates;
            Vector3 goalCube = goal.CubeCoordinates;

            int hexDistance = (int)((Mathf.Abs(startCube.x - goalCube.x) +
                                     Mathf.Abs(startCube.y - goalCube.y) +
                                     Mathf.Abs(startCube.z - goalCube.z)) / 2);

            // Estimate based on average terrain cost of 1
            int estimatedCost = hexDistance;

            // Calculate turns needed
            int turns = Mathf.CeilToInt((float)estimatedCost / movementPerTurn);

            return Mathf.Max(1, turns);
        }

        /// <summary>
        /// Clears all reachability visualization
        /// </summary>
        public void ClearReachability(HexGrid grid)
        {
            if (grid == null || grid.Cells == null)
                return;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var cell = grid.Cells[x, y];
                    if (cell != null && cell.PathfindingState != null)
                    {
                        cell.PathfindingState.ClearReachability();
                    }
                }
            }
        }

        /// <summary>
        /// Clears all path visualization
        /// </summary>
        public void ClearPaths(HexGrid grid)
        {
            if (grid == null || grid.Cells == null)
                return;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var cell = grid.Cells[x, y];
                    if (cell != null && cell.PathfindingState != null)
                    {
                        cell.PathfindingState.ClearPath();
                    }
                }
            }
        }

        // ========== CACHE MANAGEMENT ==========

        private PathResult GetCachedPath(HexCell start, HexCell goal)
        {
            var key = (start, goal);
            if (pathCache.TryGetValue(key, out CachedPath cached))
            {
                // Check if cache is still valid
                if (Time.time - cached.Timestamp < cacheDuration)
                {
                    return cached.Result;
                }
                else
                {
                    // Remove expired entry
                    pathCache.Remove(key);
                }
            }
            return null;
        }

        private void CachePath(HexCell start, HexCell goal, PathResult result)
        {
            // Enforce cache size limit
            if (pathCache.Count >= maxCacheSize)
            {
                // Simple eviction: clear entire cache (could be improved with LRU)
                pathCache.Clear();
            }

            var key = (start, goal);
            pathCache[key] = new CachedPath
            {
                Result = result,
                Timestamp = Time.time
            };
        }

        /// <summary>
        /// Invalidates cached paths involving any of the specified cells
        /// Call this when map changes (terrain, occupation, etc.)
        /// </summary>
        public void InvalidateCache(params HexCell[] affectedCells)
        {
            if (affectedCells == null || affectedCells.Length == 0)
            {
                pathCache.Clear();
                return;
            }

            var toRemove = new List<(HexCell, HexCell)>();
            foreach (var key in pathCache.Keys)
            {
                foreach (var cell in affectedCells)
                {
                    if (key.Item1 == cell || key.Item2 == cell)
                    {
                        toRemove.Add(key);
                        break;
                    }
                }
            }

            foreach (var key in toRemove)
            {
                pathCache.Remove(key);
            }
        }

        /// <summary>
        /// Clears all cached paths
        /// </summary>
        public void ClearCache()
        {
            pathCache.Clear();
        }

        // ========== ALGORITHM MANAGEMENT ==========

        /// <summary>
        /// Sets the active algorithm by type
        /// </summary>
        public void SetAlgorithm(AlgorithmType type)
        {
            if (algorithms.TryGetValue(type, out IPathfindingAlgorithm algorithm))
            {
                currentAlgorithm = algorithm;
                defaultAlgorithm = type;

                // Clear cache when switching algorithms
                ClearCache();
            }
            else
            {
                Debug.LogWarning($"Algorithm type {type} not found");
            }
        }

        /// <summary>
        /// Sets a custom algorithm implementation
        /// </summary>
        public void SetCustomAlgorithm(IPathfindingAlgorithm algorithm)
        {
            if (algorithm == null)
                throw new ArgumentNullException(nameof(algorithm));

            currentAlgorithm = algorithm;
            ClearCache();
        }

        // ========== STATISTICS ==========

        /// <summary>
        /// Gets performance statistics
        /// </summary>
        public string GetStatistics()
        {
            float avgTime = totalPathsFound > 0 ? totalComputationTime / totalPathsFound : 0f;
            float cacheHitRate = totalPathsFound > 0 ? (float)totalCacheHits / totalPathsFound * 100f : 0f;

            return $"Pathfinding Statistics:\n" +
                   $"Total Paths: {totalPathsFound}\n" +
                   $"Cache Hits: {totalCacheHits} ({cacheHitRate:F1}%)\n" +
                   $"Avg Computation Time: {avgTime:F2}ms\n" +
                   $"Current Algorithm: {currentAlgorithm.AlgorithmName}";
        }

        /// <summary>
        /// Resets all statistics
        /// </summary>
        public void ResetStatistics()
        {
            totalPathsFound = 0;
            totalCacheHits = 0;
            totalComputationTime = 0f;
        }

        // ========== ALGORITHM-SPECIFIC HELPERS ==========

        /// <summary>
        /// Runs Dijkstra's algorithm to find distances to all reachable cells.
        /// Use this when you need paths from one source to many destinations.
        /// </summary>
        public DijkstraResult FindAllPathsFrom(HexCell start, PathfindingContext context)
        {
            var dijkstra = algorithms[AlgorithmType.Dijkstra] as DijkstraPathfinding;
            if (dijkstra == null)
            {
                Debug.LogError("Dijkstra algorithm not available");
                return DijkstraResult.CreateFailure("Dijkstra not initialized");
            }

            return dijkstra.FindAllPaths(start, context);
        }

        /// <summary>
        /// Generates a flow field for efficient multi-unit pathfinding to the same goal.
        /// Use this when moving many units to the same destination.
        /// </summary>
        public FlowField GenerateFlowField(HexCell goal, PathfindingContext context)
        {
            var flowField = algorithms[AlgorithmType.FlowField] as FlowFieldPathfinding;
            if (flowField == null)
            {
                Debug.LogError("FlowField algorithm not available");
                return FlowField.CreateEmpty();
            }

            return flowField.GenerateFlowField(goal, context);
        }

        /// <summary>
        /// Gets cells within a certain number of steps (ignoring terrain cost).
        /// Fast unweighted reachability check.
        /// </summary>
        public List<HexCell> GetCellsWithinSteps(HexCell start, int maxSteps, PathfindingContext context)
        {
            var bfs = algorithms[AlgorithmType.BFS] as BreadthFirstSearch;
            if (bfs == null)
            {
                Debug.LogError("BFS algorithm not available");
                return new List<HexCell>();
            }

            return bfs.GetCellsWithinSteps(start, maxSteps, context);
        }

        /// <summary>
        /// Gets the algorithm instance for a specific type.
        /// Allows access to algorithm-specific methods.
        /// </summary>
        public IPathfindingAlgorithm GetAlgorithm(AlgorithmType type)
        {
            if (algorithms.TryGetValue(type, out IPathfindingAlgorithm algorithm))
            {
                return algorithm;
            }
            return null;
        }

        /// <summary>
        /// Gets information about all available algorithms
        /// </summary>
        public string GetAlgorithmInfo()
        {
            var info = "Available Pathfinding Algorithms:\n\n";

            foreach (var kvp in algorithms)
            {
                var algo = kvp.Value;
                info += $"{kvp.Key} ({algo.AlgorithmName}):\n";
                info += $"  {algo.Description}\n";
                info += $"  Threading: {(algo.SupportsThreading ? "Yes" : "No")}\n\n";
            }

            return info;
        }
    }
}
