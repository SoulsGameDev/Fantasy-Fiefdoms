using System;
using UnityEngine;

public enum InputEvent
{
    MouseEnter,
    MouseExit,
    MouseDown,
    MouseUp,
    FKeyDown,
    Deselect,       // For programmatic deselection
    RevealFog       // For exploration/fog of war reveal
}

public class HexCellStateManager
{
    private HexCellInteractionState interactionState;

    public HexCellStateManager(HexCellInteractionState interactionState)
    {
        this.interactionState = interactionState;
    }

    public void HandleInput(InputEvent inputEvent)
    {
        CellState currentState = interactionState.State;
        CellState nextState = currentState;

        // State transition logic based on current state and input event
        switch (currentState)
        {
            case CellState.Invisible:
                nextState = HandleInvisibleState(inputEvent);
                break;
            case CellState.Visible:
                nextState = HandleVisibleState(inputEvent);
                break;
            case CellState.Highlighted:
                nextState = HandleHighlightedState(inputEvent);
                break;
            case CellState.Selected:
                nextState = HandleSelectedState(inputEvent);
                break;
            case CellState.Focused:
                nextState = HandleFocusedState(inputEvent);
                break;
        }

        if (nextState != currentState)
        {
            interactionState.SetState(nextState);
        }
    }

    private CellState HandleInvisibleState(InputEvent inputEvent)
    {
        // Invisible cells (fog of war) only respond to reveal events
        switch (inputEvent)
        {
            case InputEvent.RevealFog:
                return CellState.Visible;
            default:
                return CellState.Invisible; // Ignore all other inputs
        }
    }

    private CellState HandleVisibleState(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case InputEvent.MouseEnter:
                return CellState.Highlighted;
            case InputEvent.MouseDown:
                return CellState.Selected; // Direct selection
            case InputEvent.FKeyDown:
                return CellState.Focused; // Direct focus
            default:
                return CellState.Visible;
        }
    }

    private CellState HandleHighlightedState(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case InputEvent.MouseExit:
                return CellState.Visible;
            case InputEvent.MouseDown:
                return CellState.Selected;
            case InputEvent.FKeyDown:
                return CellState.Focused;
            default:
                return CellState.Highlighted;
        }
    }

    private CellState HandleSelectedState(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case InputEvent.MouseExit:
                return CellState.Visible; // Deselect on mouse exit
            case InputEvent.MouseUp:
                return CellState.Highlighted; // Release but still hovering
            case InputEvent.FKeyDown:
                return CellState.Focused;
            case InputEvent.Deselect:
                return CellState.Visible; // Programmatic deselection
            default:
                return CellState.Selected;
        }
    }

    private CellState HandleFocusedState(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case InputEvent.MouseExit:
                return CellState.Selected; // Step down one level
            case InputEvent.MouseDown:
                return CellState.Selected; // New interaction
            case InputEvent.FKeyDown:
                return CellState.Visible; // Toggle focus off
            case InputEvent.Deselect:
                return CellState.Visible; // Defocus completely
            default:
                return CellState.Focused;
        }
    }

    // Public methods for programmatic state changes (not tied to input)
    public void RevealFromFog()
    {
        HandleInput(InputEvent.RevealFog);
    }

    public void Deselect()
    {
        HandleInput(InputEvent.Deselect);
    }

    public void ForceState(CellState state)
    {
        interactionState.SetState(state);
    }
}
