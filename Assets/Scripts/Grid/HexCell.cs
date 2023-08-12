using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HexCell
{
    [Header("Cell Properties")]
    private HexOrientation orientation;
    [field:SerializeField] public TerrainType TerrainType { get; set; }
    [field:SerializeField] public Vector2 OffsetCoordinates { get; set; }
    [field:SerializeField] public Vector3 CubeCoordinates { get; private set; }
    [field:SerializeField] public Vector2 AxialCoordinates { get; private set; }
    [field:SerializeField] public List<HexCell> Neighbours { get; private set; }
    [field:SerializeField] public HexGrid Grid { get; set; }
    [field:SerializeField] public float HexSize { get; set; }


    [Header("Interaction Properties")]
    [SerializeField] public bool IsSelected;
    [SerializeField] public bool IsHighlighted;
    [SerializeField] public bool IsExplored;
    [field:SerializeField] public bool IsVisible { get; private set; }
    [field:SerializeField] public bool IsPath { get; private set; }
    [field:SerializeField] public bool IsOccupied { get; private set; }
    [field:SerializeField] public bool IsSelectable { get; private set; }


    [field:SerializeField] public Transform terrain { get; private set; }


    public void SetCoordinates(Vector2 offsetCoordinates, HexOrientation orientation)
    {
        this.orientation = orientation;
        OffsetCoordinates = offsetCoordinates;
        CubeCoordinates = HexMetrics.OffsetToCube(offsetCoordinates, orientation);
        AxialCoordinates = HexMetrics.CubeToAxial(CubeCoordinates);
    }

    public void CreateTerrain()
    {
        if(TerrainType == null)
        {
            Debug.LogError("TerrainType is null");
            return;
        }
        if(Grid == null)
        {
            Debug.LogError("Grid is null");
            return;
        }
        if (HexSize == 0)
        {
            Debug.LogError("HexSize is 0");
            return;
        }
        if (TerrainType.Prefab == null)
        {
            Debug.LogError("TerrainType Prefab is null");
            return;
        }

        Vector3 centrePosition = HexMetrics.Center(HexSize, (int)OffsetCoordinates.x, (int)OffsetCoordinates.y, orientation) + Grid.transform.position;
        terrain = UnityEngine.Object.Instantiate(TerrainType.Prefab, centrePosition, Quaternion.identity, Grid.transform);
    }


    public void SetNeighbours(List<HexCell> neighbours)
    {
        Neighbours = neighbours;
    }


}
