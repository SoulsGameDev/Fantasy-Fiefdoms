using System;
using UnityEngine;

/// <summary>
/// Stores pathfinding-specific state for a HexCell.
/// Includes both persistent state (walkability, occupation) and
/// transient search state (costs, parent pointers).
/// </summary>
[Serializable]
public class HexCellPathfindingState
{
    // ========== PERSISTENT STATE (remains between pathfinding operations) ==========

    /// <summary>
    /// Whether this cell is part of a currently displayed path
    /// </summary>
    [field: SerializeField] public bool IsPath { get; set; }

    /// <summary>
    /// Whether units can move through this cell (based on terrain)
    /// </summary>
    [field: SerializeField] public bool IsWalkable { get; set; }

    /// <summary>
    /// Whether this cell is occupied by a unit or structure
    /// </summary>
    [field: SerializeField] public bool IsOccupied { get; set; }

    /// <summary>
    /// Whether this cell has been explored (not fog of war)
    /// </summary>
    [field: SerializeField] public bool IsExplored { get; set; }

    /// <summary>
    /// Whether this cell is reachable within current movement range
    /// (used for movement range visualization)
    /// </summary>
    [field: SerializeField] public bool IsReachable { get; set; }

    /// <summary>
    /// Whether this cell is temporarily reserved (e.g., another unit is pathing through it)
    /// </summary>
    [field: SerializeField] public bool IsReserved { get; set; }

    // ========== TRANSIENT SEARCH STATE (reset before each pathfinding operation) ==========

    /// <summary>
    /// Cost from start node to this node (g-cost in A*)
    /// </summary>
    [NonSerialized] public int GCost;

    /// <summary>
    /// Heuristic cost from this node to goal (h-cost in A*)
    /// </summary>
    [NonSerialized] public int HCost;

    /// <summary>
    /// Total estimated cost (f-cost in A*). Always equals GCost + HCost
    /// </summary>
    public int FCost => GCost + HCost;

    /// <summary>
    /// Parent cell in the path (for path reconstruction)
    /// </summary>
    [NonSerialized] public HexCell CameFrom;

    /// <summary>
    /// Whether this cell is currently in the open set (frontier)
    /// </summary>
    [NonSerialized] public bool IsInOpenSet;

    /// <summary>
    /// Whether this cell has been fully explored (in closed set)
    /// </summary>
    [NonSerialized] public bool IsInClosedSet;

    /// <summary>
    /// Movement cost to enter this cell (cached from TerrainType)
    /// </summary>
    [field: SerializeField] public int MovementCost { get; set; }

    /// <summary>
    /// Defense bonus provided by this cell's terrain
    /// </summary>
    [field: SerializeField] public float DefenseBonus { get; set; }

    /// <summary>
    /// Constructor initializes with default values
    /// </summary>
    public HexCellPathfindingState()
    {
        // Persistent state defaults
        IsPath = false;
        IsWalkable = true;  // Assume walkable until terrain sets otherwise
        IsOccupied = false;
        IsExplored = false;
        IsReachable = false;
        IsReserved = false;

        // Transient state defaults (will be reset before each search anyway)
        ResetSearchState();

        // Cost defaults
        MovementCost = 1;
        DefenseBonus = 0f;
    }

    /// <summary>
    /// Resets all transient search state before a new pathfinding operation.
    /// Call this before running A*, Dijkstra, BFS, etc.
    /// </summary>
    public void ResetSearchState()
    {
        GCost = int.MaxValue;  // Unreached by default
        HCost = 0;
        CameFrom = null;
        IsInOpenSet = false;
        IsInClosedSet = false;
    }

    /// <summary>
    /// Updates terrain-based properties from a TerrainType.
    /// Call this when terrain changes or during cell initialization.
    /// </summary>
    public void UpdateFromTerrain(TerrainType terrain)
    {
        if (terrain != null)
        {
            IsWalkable = terrain.isWalkable;
            MovementCost = terrain.movementCost;
            DefenseBonus = terrain.defenseBonus;
        }
        else
        {
            // Default values if no terrain
            IsWalkable = false;
            MovementCost = int.MaxValue;
            DefenseBonus = 0f;
        }
    }

    /// <summary>
    /// Clears path visualization
    /// </summary>
    public void ClearPath()
    {
        IsPath = false;
    }

    /// <summary>
    /// Clears reachability visualization
    /// </summary>
    public void ClearReachability()
    {
        IsReachable = false;
    }

    /// <summary>
    /// Checks if this cell can be traversed during pathfinding
    /// </summary>
    public bool IsTraversable()
    {
        return IsWalkable && !IsOccupied && !IsReserved;
    }

    /// <summary>
    /// Checks if this cell can be used as a path destination
    /// (stricter than traversable - can't end on reserved cells)
    /// </summary>
    public bool IsValidDestination()
    {
        return IsWalkable && !IsOccupied;
    }

    /// <summary>
    /// Returns a string representation for debugging
    /// </summary>
    public override string ToString()
    {
        return $"PathfindingState[Walkable:{IsWalkable}, Occupied:{IsOccupied}, " +
               $"Cost:{MovementCost}, G:{GCost}, H:{HCost}, F:{FCost}]";
    }
}
