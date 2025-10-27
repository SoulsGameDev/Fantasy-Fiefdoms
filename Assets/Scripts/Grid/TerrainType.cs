using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainType", menuName = "TBS/TerrainType")]
public class TerrainType : ScriptableObject
{
    [Header("Basic Properties")]
    [SerializeField] private int ID;
    [field:SerializeField] public string terrainName { get; private set; }
    [field:SerializeField] public string Name { get; private set; }
    [field:SerializeField] public string Description { get; private set; }

    [Header("Visual")]
    [field:SerializeField] public Color Colour { get; private set; }
    [field:SerializeField] public Transform Prefab { get; private set; }
    [field:SerializeField] public Sprite Icon { get; private set; }

    [Header("Pathfinding")]
    [field:SerializeField]
    [Tooltip("Movement cost to enter this terrain type (1 = normal, 2 = slow, 10 = very slow/impassable)")]
    public int movementCost { get; private set; } = 1;

    [field:SerializeField]
    [Tooltip("Whether this terrain is walkable by default")]
    public bool isWalkable { get; private set; } = true;

    private void OnValidate()
    {
        // Auto-sync terrainName with Name if not manually set
        if (string.IsNullOrEmpty(terrainName))
        {
            terrainName = Name;
        }

        // Ensure movement cost is at least 1
        if (movementCost < 1)
        {
            movementCost = 1;
        }
    }
}
