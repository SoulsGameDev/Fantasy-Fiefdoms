using System;
using UnityEngine;

[Serializable]
public class HexCellInteractionState
{
    //Mose Interactions
    [field: SerializeField] public bool IsFocused { get; set; }
    [field: SerializeField] public bool IsSelected { get; set; }
    [field: SerializeField] public bool IsHighlighted { get; set; }
    [field: SerializeField] public bool IsExplored { get; set; }
    [field: SerializeField] public bool IsVisible { get; set; }
    [field: SerializeField] public bool IsSelectable { get; set; }
    
    //
    [field: SerializeField] public bool IsPath { get; set; }
    [field: SerializeField] public bool IsWalkable{get; set;}
    [field: SerializeField] public bool IsOccupied { get; set; }
    

    //TODO: Add more states
    //TODO: Add state change events
    //TODO: Add helper methods to change state
    //TODO: Create a state machine to handle state changes if the number of states gets too large
}
