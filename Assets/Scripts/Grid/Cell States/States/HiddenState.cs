using UnityEngine;

public class HiddenState : BaseCellState
{
    public override CellState State => CellState.Hidden;

    public override void Enter(HexCell cell)
    {
        Debug.Log("Entering Hidden State");
    }

    public override void Exit(HexCell cell)
    {
        Debug.Log("Exiting Hidden State");
    }
}
