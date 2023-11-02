using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusedState : BaseCellState
{
    public override CellState State => CellState.Focused;

    public override void Enter(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is entering Focused State");
    }

    public override void Exit(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is exiting Focused State");
    }

    public override ICellState OnDeselect()
    {
        return new SelectedState();
    }
}
