using System;
using UnityEngine;

public enum CellState{
    Invisible,
    Visible,
    Highlighted,
    Selected,
    Focused
}

[Serializable]
public class HexCellInteractionState
{
    public CellState State{get; private set;}

    public event Action<CellState> OnStateChanged;

    public void SetState(CellState state){
        State = state;
        OnStateChanged?.Invoke(state);
    }
}
