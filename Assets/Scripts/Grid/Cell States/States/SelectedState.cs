using UnityEngine;

public class SelectedState : BaseCellState
{
    public override CellState State => CellState.Selected;

    public override void Enter(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is entering Selected State");
        CameraController.Instance.onDeselectAction += cell.OnDeselect;
        CameraController.Instance.onFocusAction += cell.OnFocus;
        CameraController.Instance.IsLocked = true;
        CameraController.Instance.CameraTarget.transform.position = cell.Terrain.transform.position;
    }

    public override void Exit(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is exiting Selected State");
        CameraController.Instance.onDeselectAction -= cell.OnDeselect;
        CameraController.Instance.onFocusAction -= cell.OnFocus;
        CameraController.Instance.IsLocked = false;
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
