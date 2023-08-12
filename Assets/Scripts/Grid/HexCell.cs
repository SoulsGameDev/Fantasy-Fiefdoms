using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HexCell
{
    [Header("Cell Properties")]
    [SerializeField] private TerrainType terrainType;
    [field:SerializeField] public Vector2 OffsetCoordinates { get; private set; }
    [field:SerializeField] public Vector3 CubeCoordinates { get; private set; }
    [field:SerializeField] public Vector2 AxialCoordinates { get; }
    [field:SerializeField] public List<HexCell> Neighbours { get; private set; }
    [field:SerializeField] public HexGrid Grid { get; private set; }
    [field:SerializeField] public HexOrientation Orientation { get; private set; }
    [field:SerializeField] public float HexSize { get; private set; }


    [Header("Interaction Properties")]
    [SerializeField] public bool IsSelected;
    [SerializeField] public bool IsHighlighted;
    [SerializeField] public bool IsExplored;
    [field:SerializeField] public bool IsVisible { get; private set; }
    [field:SerializeField] public bool IsPath { get; private set; }
    [field:SerializeField] public bool IsOccupied { get; private set; }
    [field:SerializeField] public bool IsSelectable { get; private set; }


    [field:SerializeField] public Transform terrain { get; private set; }



}
