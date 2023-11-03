using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusedState : BaseCellState
{
    public override CellState State => CellState.Focused;

    public override void Enter(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is entering Focused State");
        CameraController.Instance.onDeselectAction += cell.OnDeselect;
        CameraController.Instance.ChangeCamera(CameraMode.Focus);
        CameraController.Instance.IsLocked = true;
    }

    public override void Exit(HexCell cell)
    {
        Debug.Log($"Cell {cell.AxialCoordinates} is exiting Focused State");
        CameraController.Instance.onDeselectAction -= cell.OnDeselect;
        CameraController.Instance.ChangeCamera(CameraMode.TopDown);
        CameraController.Instance.IsLocked = false;
    }

    public override ICellState OnDeselect()
    {
        return new SelectedState();
    }
}
