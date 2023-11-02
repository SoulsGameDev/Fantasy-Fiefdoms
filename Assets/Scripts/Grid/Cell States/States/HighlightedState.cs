using UnityEngine;

public class HighlightedState : BaseCellState
{
    public override CellState State => CellState.Highlighted;

    public override void Enter(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is entering Highlighted State");
    }

    public override void Exit(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is exiting Highlighted State");
    }

    public override ICellState OnMouseExit()
    {
        return new VisibleState();
    }
}
