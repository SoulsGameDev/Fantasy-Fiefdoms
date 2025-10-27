using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding.Core
{
    /// <summary>
    /// ScriptableObject for saving and reusing PathfindingContext configurations.
    /// Allows designers to create preset configurations for different unit types or scenarios.
    /// </summary>
    [CreateAssetMenu(fileName = "New Pathfinding Preset", menuName = "Pathfinding/Context Preset", order = 1)]
    public class PathfindingContextPreset : ScriptableObject
    {
        [Header("Preset Information")]
        [Tooltip("Display name for this preset")]
        public string presetName = "New Preset";

        [TextArea(2, 4)]
        [Tooltip("Description of when to use this preset")]
        public string description = "Describe the intended use case for this pathfinding configuration.";

        [Header("Movement Settings")]
        [Tooltip("Maximum movement points (-1 for unlimited)")]
        public int maxMovementPoints = -1;

        [Tooltip("Maximum nodes to search before giving up")]
        public int maxSearchNodes = 10000;

        [Header("Traversal Rules")]
        [Tooltip("Allow moving through cells occupied by allies")]
        public bool allowMoveThroughAllies = false;

        [Tooltip("Allow moving through cells occupied by enemies")]
        public bool allowMoveThroughEnemies = false;

        [Tooltip("Only consider explored (non-fog-of-war) cells")]
        public bool requireExplored = true;

        [Tooltip("Allow diagonal movement (future feature)")]
        public bool allowDiagonalMovement = false;

        [Header("Strategic Preferences")]
        [Tooltip("Prefer paths through higher terrain")]
        public bool preferHighGround = false;

        [Tooltip("Avoid cells adjacent to enemies")]
        public bool avoidEnemyZones = false;

        [Header("Terrain Cost Multipliers")]
        [Tooltip("Custom cost multipliers for terrain types (1.0 = normal, 2.0 = double cost, 0.5 = half cost)")]
        public List<TerrainCostMultiplier> terrainCostMultipliers = new List<TerrainCostMultiplier>();

        [Header("Performance")]
        [Tooltip("Store diagnostic data for debugging (disable for production)")]
        public bool storeDiagnosticData = true;

        [Tooltip("Use cached results if available")]
        public bool useCaching = true;

        /// <summary>
        /// Creates a PathfindingContext from this preset
        /// </summary>
        public PathfindingContext CreateContext()
        {
            var context = new PathfindingContext
            {
                MaxMovementPoints = maxMovementPoints,
                MaxSearchNodes = maxSearchNodes,
                AllowMoveThroughAllies = allowMoveThroughAllies,
                AllowMoveThroughEnemies = allowMoveThroughEnemies,
                RequireExplored = requireExplored,
                AllowDiagonalMovement = allowDiagonalMovement,
                PreferHighGround = preferHighGround,
                AvoidEnemyZones = avoidEnemyZones,
                StoreDiagnosticData = storeDiagnosticData,
                UseCaching = useCaching
            };

            // Apply terrain cost multipliers
            foreach (var multiplier in terrainCostMultipliers)
            {
                if (!string.IsNullOrEmpty(multiplier.terrainName))
                {
                    context.TerrainCostMultipliers[multiplier.terrainName] = multiplier.costMultiplier;
                }
            }

            return context;
        }

        /// <summary>
        /// Applies settings from a PathfindingContext to this preset
        /// </summary>
        public void ApplyFromContext(PathfindingContext context)
        {
            maxMovementPoints = context.MaxMovementPoints;
            maxSearchNodes = context.MaxSearchNodes;
            allowMoveThroughAllies = context.AllowMoveThroughAllies;
            allowMoveThroughEnemies = context.AllowMoveThroughEnemies;
            requireExplored = context.RequireExplored;
            allowDiagonalMovement = context.AllowDiagonalMovement;
            preferHighGround = context.PreferHighGround;
            avoidEnemyZones = context.AvoidEnemyZones;
            storeDiagnosticData = context.StoreDiagnosticData;
            useCaching = context.UseCaching;

            // Convert terrain cost multipliers
            terrainCostMultipliers.Clear();
            foreach (var kvp in context.TerrainCostMultipliers)
            {
                terrainCostMultipliers.Add(new TerrainCostMultiplier
                {
                    terrainName = kvp.Key,
                    costMultiplier = kvp.Value
                });
            }
        }

        /// <summary>
        /// Creates a preset with default infantry unit settings
        /// </summary>
        public static PathfindingContextPreset CreateInfantryPreset()
        {
            var preset = CreateInstance<PathfindingContextPreset>();
            preset.presetName = "Infantry";
            preset.description = "Standard infantry movement. Moderate speed, can't pass through allies or enemies.";
            preset.maxMovementPoints = 5;
            preset.allowMoveThroughAllies = false;
            preset.allowMoveThroughEnemies = false;
            preset.requireExplored = true;
            return preset;
        }

        /// <summary>
        /// Creates a preset with cavalry unit settings
        /// </summary>
        public static PathfindingContextPreset CreateCavalryPreset()
        {
            var preset = CreateInstance<PathfindingContextPreset>();
            preset.presetName = "Cavalry";
            preset.description = "Fast cavalry movement. High speed on open terrain, penalized in forests/mountains.";
            preset.maxMovementPoints = 8;
            preset.allowMoveThroughAllies = true; // Can move through allies
            preset.requireExplored = true;
            preset.terrainCostMultipliers.Add(new TerrainCostMultiplier { terrainName = "Forest", costMultiplier = 2.0f });
            preset.terrainCostMultipliers.Add(new TerrainCostMultiplier { terrainName = "Mountains", costMultiplier = 3.0f });
            preset.terrainCostMultipliers.Add(new TerrainCostMultiplier { terrainName = "Grassland", costMultiplier = 0.5f });
            return preset;
        }

        /// <summary>
        /// Creates a preset for flying units
        /// </summary>
        public static PathfindingContextPreset CreateFlyingPreset()
        {
            var preset = CreateInstance<PathfindingContextPreset>();
            preset.presetName = "Flying";
            preset.description = "Flying unit movement. Ignores terrain costs, can move through enemies.";
            preset.maxMovementPoints = 10;
            preset.allowMoveThroughAllies = true;
            preset.allowMoveThroughEnemies = true;
            preset.requireExplored = false; // Can fly into unexplored areas
            preset.terrainCostMultipliers.Add(new TerrainCostMultiplier { terrainName = "Forest", costMultiplier = 1.0f });
            preset.terrainCostMultipliers.Add(new TerrainCostMultiplier { terrainName = "Mountains", costMultiplier = 1.0f });
            preset.terrainCostMultipliers.Add(new TerrainCostMultiplier { terrainName = "Ocean", costMultiplier = 1.0f });
            return preset;
        }

        /// <summary>
        /// Creates a preset for tactical combat movement
        /// </summary>
        public static PathfindingContextPreset CreateTacticalCombatPreset()
        {
            var preset = CreateInstance<PathfindingContextPreset>();
            preset.presetName = "Tactical Combat";
            preset.description = "Cautious tactical movement. Prefers high ground, avoids enemy zones.";
            preset.maxMovementPoints = 5;
            preset.allowMoveThroughAllies = true;
            preset.preferHighGround = true;
            preset.avoidEnemyZones = true;
            preset.requireExplored = true;
            return preset;
        }
    }

    /// <summary>
    /// Serializable terrain cost multiplier entry
    /// </summary>
    [System.Serializable]
    public struct TerrainCostMultiplier
    {
        [Tooltip("Name of the terrain type")]
        public string terrainName;

        [Tooltip("Cost multiplier (1.0 = normal, 2.0 = double cost, 0.5 = half cost)")]
        [Range(0.1f, 10f)]
        public float costMultiplier;
    }
}
