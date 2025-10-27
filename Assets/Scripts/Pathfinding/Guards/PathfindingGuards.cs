using System;
using UnityEngine;

namespace Pathfinding.Guards
{
    // =====================================================
    // PATHFINDING GUARDS - Validation for pathfinding operations
    // =====================================================

    /// <summary>
    /// Guard that checks if a cell is walkable (based on terrain).
    /// Prevents pathfinding through impassable terrain like mountains or water.
    /// </summary>
    public class PathWalkableGuard : GuardBase
    {
        public override string Name => "PathWalkable";
        public override string Description => "Cell terrain must be walkable";

        public override GuardResult Evaluate(GuardContext context)
        {
            if (context.Cell == null)
            {
                return Deny("Cell is null");
            }

            if (context.Cell.PathfindingState == null)
            {
                return Deny("PathfindingState is null");
            }

            if (context.Cell.PathfindingState.IsWalkable)
            {
                return Allow();
            }

            string terrainName = context.Cell.TerrainType?.terrainName ?? "Unknown";
            return Deny($"Cell at {context.Cell.OffsetCoordinates} has unwalkable terrain: {terrainName}");
        }
    }

    /// <summary>
    /// Guard that checks if a cell is occupied by a unit or structure.
    /// Can be configured to allow movement through allies.
    /// </summary>
    public class PathOccupationGuard : GuardBase
    {
        private bool allowAllies;
        private bool allowEnemies;

        public override string Name => "PathOccupation";
        public override string Description => allowAllies
            ? "Cell must be unoccupied (allies allowed)"
            : "Cell must be unoccupied";

        /// <summary>
        /// Creates a path occupation guard
        /// </summary>
        /// <param name="allowMoveThroughAllies">Whether to allow pathing through allied units</param>
        /// <param name="allowMoveThroughEnemies">Whether to allow pathing through enemy units (for attack moves)</param>
        public PathOccupationGuard(bool allowMoveThroughAllies = false, bool allowMoveThroughEnemies = false)
        {
            this.allowAllies = allowMoveThroughAllies;
            this.allowEnemies = allowMoveThroughEnemies;
        }

        public override GuardResult Evaluate(GuardContext context)
        {
            if (context.Cell == null)
            {
                return Deny("Cell is null");
            }

            if (!context.Cell.PathfindingState.IsOccupied)
            {
                return Allow();
            }

            // TODO: When unit system is implemented, check if occupied by ally or enemy
            // For now, occupied cells are blocked unless allowAllies/allowEnemies is set

            if (allowAllies || allowEnemies)
            {
                // Future: Check unit ownership here
                // For now, just allow if configured to do so
                return Allow();
            }

            return Deny($"Cell at {context.Cell.OffsetCoordinates} is occupied");
        }
    }

    /// <summary>
    /// Guard that validates a path doesn't exceed maximum movement points.
    /// Used to enforce turn-based movement limits.
    /// </summary>
    public class PathMovementCostGuard : GuardBase
    {
        private int maxMovementPoints;
        private bool enforceLimit;

        public override string Name => "PathMovementCost";
        public override string Description => enforceLimit
            ? $"Path cost must not exceed {maxMovementPoints} movement points"
            : "No movement cost limit";

        /// <summary>
        /// Creates a movement cost guard
        /// </summary>
        /// <param name="maxMovementPoints">Maximum allowed movement points (-1 for unlimited)</param>
        public PathMovementCostGuard(int maxMovementPoints)
        {
            this.maxMovementPoints = maxMovementPoints;
            this.enforceLimit = maxMovementPoints >= 0;
        }

        public override GuardResult Evaluate(GuardContext context)
        {
            if (!enforceLimit)
            {
                return Allow();
            }

            if (context.Cell == null)
            {
                return Deny("Cell is null");
            }

            // Check the accumulated cost to this cell (GCost)
            int currentCost = context.Cell.PathfindingState.GCost;

            if (currentCost == int.MaxValue)
            {
                // Cell hasn't been reached yet, can't evaluate
                return Allow();
            }

            if (currentCost <= maxMovementPoints)
            {
                return Allow();
            }

            return Deny($"Path cost ({currentCost}) exceeds maximum movement points ({maxMovementPoints})");
        }
    }

    /// <summary>
    /// Guard that checks if a cell has been explored (not fog of war).
    /// Prevents pathfinding through unexplored areas.
    /// </summary>
    public class PathExplorationGuard : GuardBase
    {
        public override string Name => "PathExploration";
        public override string Description => "Cell must be explored for pathfinding";

        public override GuardResult Evaluate(GuardContext context)
        {
            if (context.Cell == null)
            {
                return Deny("Cell is null");
            }

            if (context.Cell.PathfindingState.IsExplored)
            {
                return Allow();
            }

            return Deny($"Cell at {context.Cell.OffsetCoordinates} is not explored (fog of war)");
        }
    }

    /// <summary>
    /// Guard that checks if a cell is temporarily reserved.
    /// Used to prevent multiple units from pathing through the same cell.
    /// </summary>
    public class PathReservationGuard : GuardBase
    {
        public override string Name => "PathReservation";
        public override string Description => "Cell must not be reserved";

        public override GuardResult Evaluate(GuardContext context)
        {
            if (context.Cell == null)
            {
                return Deny("Cell is null");
            }

            if (!context.Cell.PathfindingState.IsReserved)
            {
                return Allow();
            }

            return Deny($"Cell at {context.Cell.OffsetCoordinates} is temporarily reserved");
        }
    }

    /// <summary>
    /// Guard that validates terrain-specific movement restrictions.
    /// Can be configured with custom cost multipliers for different terrains.
    /// </summary>
    public class PathTerrainCostGuard : GuardBase
    {
        private int maxTerrainCost;

        public override string Name => "PathTerrainCost";
        public override string Description => $"Terrain cost must not exceed {maxTerrainCost}";

        /// <summary>
        /// Creates a terrain cost guard
        /// </summary>
        /// <param name="maxTerrainCost">Maximum allowed cost for a single terrain type</param>
        public PathTerrainCostGuard(int maxTerrainCost)
        {
            this.maxTerrainCost = maxTerrainCost;
        }

        public override GuardResult Evaluate(GuardContext context)
        {
            if (context.Cell == null)
            {
                return Deny("Cell is null");
            }

            int terrainCost = context.Cell.PathfindingState.MovementCost;

            if (terrainCost <= maxTerrainCost)
            {
                return Allow();
            }

            string terrainName = context.Cell.TerrainType?.terrainName ?? "Unknown";
            return Deny($"Terrain '{terrainName}' has cost {terrainCost}, exceeds maximum {maxTerrainCost}");
        }
    }

    /// <summary>
    /// Composite guard that combines all standard pathfinding validations.
    /// Convenient for common pathfinding scenarios.
    /// </summary>
    public class StandardPathfindingGuard : CompositeGuard
    {
        public StandardPathfindingGuard(
            bool requireExplored = true,
            bool allowOccupied = false,
            int maxMovementPoints = -1)
            : base("StandardPathfinding")
        {
            // Add standard pathfinding guards
            AddGuard(new PathWalkableGuard());

            if (requireExplored)
            {
                AddGuard(new PathExplorationGuard());
            }

            if (!allowOccupied)
            {
                AddGuard(new PathOccupationGuard());
            }

            AddGuard(new PathReservationGuard());

            if (maxMovementPoints >= 0)
            {
                AddGuard(new PathMovementCostGuard(maxMovementPoints));
            }
        }

        public override string Description => "Standard pathfinding validation (walkable, explored, unoccupied)";
    }
}
