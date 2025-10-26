using System;
using UnityEngine;

/// <summary>
/// Command for changing a hex cell's interaction state with undo/redo support.
/// Integrates the Command Pattern with the FSM system.
/// </summary>
public class StateChangeCommand : CommandBase
{
    private HexCell cell;
    private CellState targetState;
    private CellState previousState;
    private InputEvent inputEvent;

    public override string Description =>
        $"Change {cell.OffsetCoordinates} from {previousState} to {targetState}";

    /// <summary>
    /// Create a state change command via input event
    /// </summary>
    public StateChangeCommand(HexCell cell, InputEvent inputEvent)
    {
        this.cell = cell;
        this.inputEvent = inputEvent;
        this.previousState = cell.GetCurrentState();
        this.targetState = PredictNextState(previousState, inputEvent);
    }

    /// <summary>
    /// Create a state change command with explicit target state
    /// </summary>
    public StateChangeCommand(HexCell cell, CellState targetState)
    {
        this.cell = cell;
        this.targetState = targetState;
        this.previousState = cell.GetCurrentState();
        this.inputEvent = InputEvent.Deselect; // Default
    }

    public override bool CanExecute()
    {
        // Can execute if cell is valid and state would actually change
        return cell != null && previousState != targetState;
    }

    public override bool CanUndo()
    {
        // Can undo if executed and cell is still valid
        return isExecuted && cell != null;
    }

    public override bool Execute()
    {
        if (!CanExecute())
        {
            return false;
        }

        try
        {
            // Store the state before changing
            previousState = cell.GetCurrentState();

            // Use HandleInput if we have an input event, otherwise force state
            if (inputEvent != InputEvent.Deselect)
            {
                cell.HandleInput(inputEvent);
            }
            else
            {
                // Direct state change (for programmatic state setting)
                cell.ForceState(targetState);
            }

            isExecuted = true;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to execute StateChangeCommand: {e.Message}");
            return false;
        }
    }

    public override bool Undo()
    {
        if (!CanUndo())
        {
            return false;
        }

        try
        {
            // Restore previous state
            cell.ForceState(previousState);
            isExecuted = false;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to undo StateChangeCommand: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Predict the next state based on current state and input event
    /// (Mirrors the logic in HexCellStateManager)
    /// </summary>
    private CellState PredictNextState(CellState current, InputEvent input)
    {
        switch (current)
        {
            case CellState.Invisible:
                return input == InputEvent.RevealFog ? CellState.Visible : CellState.Invisible;

            case CellState.Visible:
                switch (input)
                {
                    case InputEvent.MouseEnter: return CellState.Highlighted;
                    case InputEvent.MouseDown: return CellState.Selected;
                    case InputEvent.FKeyDown: return CellState.Focused;
                    default: return CellState.Visible;
                }

            case CellState.Highlighted:
                switch (input)
                {
                    case InputEvent.MouseExit: return CellState.Visible;
                    case InputEvent.MouseDown: return CellState.Selected;
                    case InputEvent.FKeyDown: return CellState.Focused;
                    default: return CellState.Highlighted;
                }

            case CellState.Selected:
                switch (input)
                {
                    case InputEvent.MouseExit: return CellState.Visible;
                    case InputEvent.MouseUp: return CellState.Highlighted;
                    case InputEvent.FKeyDown: return CellState.Focused;
                    case InputEvent.Deselect: return CellState.Visible;
                    default: return CellState.Selected;
                }

            case CellState.Focused:
                switch (input)
                {
                    case InputEvent.MouseExit: return CellState.Selected;
                    case InputEvent.MouseDown: return CellState.Selected;
                    case InputEvent.FKeyDown: return CellState.Visible;
                    case InputEvent.Deselect: return CellState.Visible;
                    default: return CellState.Focused;
                }

            default:
                return current;
        }
    }
}
