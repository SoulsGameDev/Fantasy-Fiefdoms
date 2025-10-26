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
    private HexCell cell; // Reference to cell for guard evaluation

    // Enable/disable guards for this specific cell (overrides global setting)
    private bool? guardsEnabledOverride = null;

    // Event fired when a transition is blocked by guards
    public event Action<CellState, CellState, string> OnTransitionBlocked;

    public HexCellStateManager(HexCellInteractionState interactionState, HexCell cell = null)
    {
        this.interactionState = interactionState;
        this.cell = cell;
    }

    /// <summary>
    /// Set the cell reference (if not provided in constructor)
    /// </summary>
    public void SetCell(HexCell cell)
    {
        this.cell = cell;
    }

    /// <summary>
    /// Override guard evaluation for this cell
    /// </summary>
    public void SetGuardsEnabled(bool enabled)
    {
        guardsEnabledOverride = enabled;
    }

    /// <summary>
    /// Clear guard override (use global setting)
    /// </summary>
    public void ClearGuardsOverride()
    {
        guardsEnabledOverride = null;
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

        // Check if transition is valid and allowed by guards
        if (nextState != currentState)
        {
            if (EvaluateGuards(currentState, nextState, inputEvent))
            {
                interactionState.SetState(nextState);
            }
            // If guards blocked the transition, state remains unchanged
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

    // ===== Guard Evaluation =====

    /// <summary>
    /// Evaluate guards for a transition
    /// </summary>
    private bool EvaluateGuards(CellState from, CellState to, InputEvent inputEvent)
    {
        // Check if guards are disabled for this cell
        if (guardsEnabledOverride.HasValue && !guardsEnabledOverride.Value)
        {
            return true; // Guards disabled, allow transition
        }

        // Create guard context
        var context = new GuardContext(cell, from, to, inputEvent);

        // Evaluate using the registry
        var result = TransitionGuardRegistry.Instance.EvaluateTransition(context);

        if (!result.Success)
        {
            // Fire event for blocked transition
            OnTransitionBlocked?.Invoke(from, to, result.Reason);
        }

        return result.Success;
    }

    /// <summary>
    /// Check if a transition would be allowed (without executing it)
    /// </summary>
    public bool CanTransition(CellState toState, InputEvent inputEvent)
    {
        CellState currentState = interactionState.State;

        if (currentState == toState)
        {
            return false; // Can't transition to same state
        }

        // Check if this input would lead to this state
        CellState predictedState = PredictNextState(currentState, inputEvent);

        if (predictedState != toState)
        {
            return false; // This input wouldn't lead to target state
        }

        // Check guards
        var context = new GuardContext(cell, currentState, toState, inputEvent);
        return TransitionGuardRegistry.Instance.CanTransition(context);
    }

    /// <summary>
    /// Predict the next state for a given input (without guards)
    /// </summary>
    private CellState PredictNextState(CellState current, InputEvent inputEvent)
    {
        switch (current)
        {
            case CellState.Invisible:
                return HandleInvisibleState(inputEvent);
            case CellState.Visible:
                return HandleVisibleState(inputEvent);
            case CellState.Highlighted:
                return HandleHighlightedState(inputEvent);
            case CellState.Selected:
                return HandleSelectedState(inputEvent);
            case CellState.Focused:
                return HandleFocusedState(inputEvent);
            default:
                return current;
        }
    }

    // ===== Public Methods =====

    /// <summary>
    /// Reveal cell from fog of war
    /// </summary>
    public void RevealFromFog()
    {
        HandleInput(InputEvent.RevealFog);
    }

    /// <summary>
    /// Deselect the cell
    /// </summary>
    public void Deselect()
    {
        HandleInput(InputEvent.Deselect);
    }

    /// <summary>
    /// Force a state change without guard evaluation or input validation.
    /// Use with caution - primarily for undo/redo or debugging.
    /// </summary>
    public void ForceState(CellState state)
    {
        interactionState.SetState(state);
    }

    /// <summary>
    /// Attempt a transition with guard evaluation, returning success status
    /// </summary>
    public bool TryTransition(CellState toState, InputEvent inputEvent, out string failureReason)
    {
        CellState currentState = interactionState.State;
        failureReason = null;

        if (currentState == toState)
        {
            failureReason = "Already in target state";
            return false;
        }

        // Create context
        var context = new GuardContext(cell, currentState, toState, inputEvent);

        // Evaluate guards
        var result = TransitionGuardRegistry.Instance.EvaluateTransition(context);

        if (result.Success)
        {
            interactionState.SetState(toState);
            return true;
        }
        else
        {
            failureReason = result.Reason;
            OnTransitionBlocked?.Invoke(currentState, toState, result.Reason);
            return false;
        }
    }
}
