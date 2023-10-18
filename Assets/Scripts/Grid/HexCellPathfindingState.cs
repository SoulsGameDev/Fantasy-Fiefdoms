using System;
using UnityEngine;

[Serializable]
public class HexCellPathfindingState{
    //
    [field: SerializeField] public bool IsPath { get; set; }
    [field: SerializeField] public bool IsWalkable{get; set;}
    [field: SerializeField] public bool IsOccupied { get; set; }
    [field: SerializeField] public bool IsExplored { get; set; }
}