using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Core
{
    /// <summary>
    /// Provides context and configuration options for pathfinding operations.
    /// Used to customize pathfinding behavior without changing algorithm implementations.
    /// </summary>
    public class PathfindingContext
    {
        /// <summary>
        /// Cells that should be treated as impassable for this search
        /// (in addition to terrain-based obstacles)
        /// </summary>
        public HashSet<HexCell> DynamicObstacles { get; set; }

        /// <summary>
        /// Whether the pathfinding should allow moving through cells occupied by allies
        /// </summary>
        public bool AllowMoveThroughAllies { get; set; }

        /// <summary>
        /// Whether the pathfinding should allow moving through cells occupied by enemies
        /// (for attack move or pass-through scenarios)
        /// </summary>
        public bool AllowMoveThroughEnemies { get; set; }

        /// <summary>
        /// Maximum movement points available for the path.
        /// If set, paths exceeding this cost will be rejected.
        /// Use -1 for unlimited movement.
        /// </summary>
        public int MaxMovementPoints { get; set; }

        /// <summary>
        /// Maximum number of nodes to explore before giving up.
        /// Prevents infinite loops and provides performance guarantees.
        /// </summary>
        public int MaxSearchNodes { get; set; }

        /// <summary>
        /// Whether to only consider explored (non-fog-of-war) cells
        /// </summary>
        public bool RequireExplored { get; set; }

        /// <summary>
        /// Whether to prefer paths through higher terrain (for strategic advantage)
        /// </summary>
        public bool PreferHighGround { get; set; }

        /// <summary>
        /// Whether to avoid cells adjacent to enemies (for safe movement)
        /// </summary>
        public bool AvoidEnemyZones { get; set; }

        /// <summary>
        /// Whether to include diagonal movement (if supported by hex grid orientation)
        /// Note: Standard hex grids have 6 neighbors, this is for future extensions
        /// </summary>
        public bool AllowDiagonalMovement { get; set; }

        /// <summary>
        /// The unit requesting the path (for unit-specific movement rules)
        /// Can be null for generic pathfinding
        /// </summary>
        public GameObject MovingUnit { get; set; }

        /// <summary>
        /// Custom cost multiplier for specific terrain types.
        /// Key: TerrainType name, Value: Cost multiplier (1.0 = normal, 2.0 = double cost, 0.5 = half cost)
        /// </summary>
        public Dictionary<string, float> TerrainCostMultipliers { get; set; }

        /// <summary>
        /// Whether to store detailed diagnostic information (cost maps, came-from maps)
        /// Disable for production to save memory
        /// </summary>
        public bool StoreDiagnosticData { get; set; }

        /// <summary>
        /// Whether to use cached results if available
        /// </summary>
        public bool UseCaching { get; set; }

        /// <summary>
        /// Creates a default pathfinding context with standard settings
        /// </summary>
        public PathfindingContext()
        {
            DynamicObstacles = new HashSet<HexCell>();
            TerrainCostMultipliers = new Dictionary<string, float>();

            // Default settings
            AllowMoveThroughAllies = false;
            AllowMoveThroughEnemies = false;
            MaxMovementPoints = -1; // Unlimited
            MaxSearchNodes = 10000; // Reasonable limit for most maps
            RequireExplored = true;
            PreferHighGround = false;
            AvoidEnemyZones = false;
            AllowDiagonalMovement = false;
            MovingUnit = null;
            StoreDiagnosticData = true;
            UseCaching = true;
        }

        /// <summary>
        /// Creates a pathfinding context with specific movement constraints
        /// </summary>
        public static PathfindingContext CreateWithMovementLimit(int maxMovement)
        {
            return new PathfindingContext
            {
                MaxMovementPoints = maxMovement
            };
        }

        /// <summary>
        /// Creates a pathfinding context for exploration (ignores fog of war)
        /// </summary>
        public static PathfindingContext CreateForExploration()
        {
            return new PathfindingContext
            {
                RequireExplored = false,
                MaxMovementPoints = -1
            };
        }

        /// <summary>
        /// Creates a pathfinding context for combat movement
        /// (allows through allies, avoids enemy zones)
        /// </summary>
        public static PathfindingContext CreateForCombat()
        {
            return new PathfindingContext
            {
                AllowMoveThroughAllies = true,
                AvoidEnemyZones = true,
                PreferHighGround = true
            };
        }

        /// <summary>
        /// Creates a copy of this context
        /// </summary>
        public PathfindingContext Clone()
        {
            return new PathfindingContext
            {
                DynamicObstacles = new HashSet<HexCell>(this.DynamicObstacles),
                AllowMoveThroughAllies = this.AllowMoveThroughAllies,
                AllowMoveThroughEnemies = this.AllowMoveThroughEnemies,
                MaxMovementPoints = this.MaxMovementPoints,
                MaxSearchNodes = this.MaxSearchNodes,
                RequireExplored = this.RequireExplored,
                PreferHighGround = this.PreferHighGround,
                AvoidEnemyZones = this.AvoidEnemyZones,
                AllowDiagonalMovement = this.AllowDiagonalMovement,
                MovingUnit = this.MovingUnit,
                TerrainCostMultipliers = new Dictionary<string, float>(this.TerrainCostMultipliers),
                StoreDiagnosticData = this.StoreDiagnosticData,
                UseCaching = this.UseCaching
            };
        }

        /// <summary>
        /// Checks if a cell should be treated as an obstacle
        /// </summary>
        public bool IsObstacle(HexCell cell)
        {
            if (cell == null)
                return true;

            // Check dynamic obstacles
            if (DynamicObstacles.Contains(cell))
                return true;

            // Check if cell is explored (if required)
            if (RequireExplored && !cell.PathfindingState.IsExplored)
                return true;

            return false;
        }

        /// <summary>
        /// Gets the effective movement cost for a cell, applying context-specific modifiers
        /// </summary>
        public int GetEffectiveMovementCost(HexCell cell)
        {
            if (cell == null || cell.TerrainType == null)
                return int.MaxValue;

            int baseCost = cell.TerrainType.movementCost;

            // Apply terrain cost multipliers
            if (TerrainCostMultipliers.TryGetValue(cell.TerrainType.terrainName, out float multiplier))
            {
                baseCost = Mathf.RoundToInt(baseCost * multiplier);
            }

            // Penalize cells adjacent to enemies if avoiding them
            if (AvoidEnemyZones)
            {
                // TODO: Check for adjacent enemies when unit system is implemented
                // baseCost += 5; // Example penalty
            }

            // Prefer high ground by reducing cost
            if (PreferHighGround)
            {
                // TODO: Use terrain height when available
                // if (cell.TerrainType.terrainName == "Mountains")
                //     baseCost = Mathf.Max(1, baseCost - 1);
            }

            return baseCost;
        }
    }
}
