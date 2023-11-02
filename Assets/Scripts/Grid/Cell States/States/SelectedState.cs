using UnityEngine;

public class SelectedState : BaseCellState
{
    public override CellState State => CellState.Selected;

    public override void Enter(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is entering Selected State");
    }

    public override void Exit(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is exiting Selected State");
    }

    public override ICellState OnDeselect()
    {
        return new VisibleState();
    }

    public override ICellState OnFocus()
    {
        return new FocusedState();
    }
}
