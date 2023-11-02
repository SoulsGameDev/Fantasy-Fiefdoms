public interface ICellState
{
    CellState State { get; }

    void Enter(HexCell cell);
    void Exit(HexCell cell);
    
    //Possible transition triggers
    ICellState OnMouseEnter();
    ICellState OnMouseExit();
    ICellState OnSelect();
    ICellState OnDeselect();
    ICellState OnFocus();
}
