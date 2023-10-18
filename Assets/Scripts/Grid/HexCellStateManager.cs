using System;
using UnityEngine;
public enum InputEvent
{
    MouseEnter,
    MouseExit,
    MouseDown,
    MouseUp,
    FKeyDown
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
        CellState nextState;

        switch (currentState)
        {
            case CellState.Invisible:
                // Determine next state based on inputEvent...
                nextState = CellState.Visible;
                break;
            case CellState.Visible:
                // Determine next state based on inputEvent...
                nextState = CellState.Invisible;
                break;
            // Handle other states...
            default:
                nextState = currentState;
                break;
        }

        interactionState.SetState(nextState);
    }
}
