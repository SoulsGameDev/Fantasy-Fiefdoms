using System;
using UnityEngine;

public enum CellState{
    Invisible = 0,  // Fog of war
    Visible = 1,    // Explored, normal state
    Highlighted = 2, // Mouse hover (base interactive)
    Selected = 3,   // Clicked/active
    Focused = 4     // Camera focus
}

[Serializable]
public class HexCellInteractionState
{
    public CellState State{get; private set;}

    // General state change event
    public event Action<CellState, CellState> OnStateChanged; // (from, to)

    // Granular state events for specific transitions
    public event Action OnEnterInvisible;
    public event Action OnExitInvisible;
    public event Action OnEnterVisible;
    public event Action OnExitVisible;
    public event Action OnEnterHighlighted;
    public event Action OnExitHighlighted;
    public event Action OnEnterSelected;
    public event Action OnExitSelected;
    public event Action OnEnterFocused;
    public event Action OnExitFocused;

    public HexCellInteractionState(CellState initialState = CellState.Invisible)
    {
        State = initialState;
    }

    public void SetState(CellState newState)
    {
        if (State == newState) return;

        CellState previousState = State;

        // Trigger exit event for previous state
        TriggerExitEvent(previousState);

        // Update state
        State = newState;

        // Trigger enter event for new state
        TriggerEnterEvent(newState);

        // Trigger general state change event
        OnStateChanged?.Invoke(previousState, newState);
    }

    private void TriggerExitEvent(CellState state)
    {
        switch(state)
        {
            case CellState.Invisible:
                OnExitInvisible?.Invoke();
                break;
            case CellState.Visible:
                OnExitVisible?.Invoke();
                break;
            case CellState.Highlighted:
                OnExitHighlighted?.Invoke();
                break;
            case CellState.Selected:
                OnExitSelected?.Invoke();
                break;
            case CellState.Focused:
                OnExitFocused?.Invoke();
                break;
        }
    }

    private void TriggerEnterEvent(CellState state)
    {
        switch(state)
        {
            case CellState.Invisible:
                OnEnterInvisible?.Invoke();
                break;
            case CellState.Visible:
                OnEnterVisible?.Invoke();
                break;
            case CellState.Highlighted:
                OnEnterHighlighted?.Invoke();
                break;
            case CellState.Selected:
                OnEnterSelected?.Invoke();
                break;
            case CellState.Focused:
                OnEnterFocused?.Invoke();
                break;
        }
    }

    // Helper methods for state hierarchy queries
    public bool IsAtLeast(CellState state)
    {
        return State >= state;
    }

    public bool IsInteractive()
    {
        return State >= CellState.Highlighted;
    }

    public bool IsExplored()
    {
        return State >= CellState.Visible;
    }
}
