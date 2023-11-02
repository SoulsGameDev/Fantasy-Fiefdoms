using UnityEngine;

public class VisibleState : BaseCellState
{
    public override CellState State => CellState.Visible;

    public override void Enter(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is entering Visible State");
    }

    public override void Exit(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is exiting Visible State");
    }

    public override ICellState OnMouseEnter()
    {
        return new HighlightedState();
    }
}
