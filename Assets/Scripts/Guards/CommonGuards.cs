using System;
using UnityEngine;

// =====================================================
// COMMON GUARDS - Reusable validation logic
// =====================================================

/// <summary>
/// Guard that checks if a cell is explored (not in fog of war).
/// Prevents interaction with unexplored cells.
/// </summary>
public class IsExploredGuard : GuardBase
{
    public override string Name => "IsExplored";
    public override string Description => "Cell must be explored (not fog of war)";

    public override GuardResult Evaluate(GuardContext context)
    {
        if (context.Cell == null)
        {
            return Deny("Cell is null");
        }

        if (context.Cell.IsExplored())
        {
            return Allow();
        }

        return Deny($"Cell at {context.Cell.OffsetCoordinates} is not explored");
    }
}

/// <summary>
/// Guard that checks if a cell is currently visible (at least Visible state).
/// Similar to IsExplored but checks current state instead of historical exploration.
/// </summary>
public class IsVisibleGuard : GuardBase
{
    public override string Name => "IsVisible";
    public override string Description => "Cell must be visible";

    public override GuardResult Evaluate(GuardContext context)
    {
        if (context.Cell == null)
        {
            return Deny("Cell is null");
        }

        if (context.Cell.GetCurrentState() >= CellState.Visible)
        {
            return Allow();
        }

        return Deny($"Cell at {context.Cell.OffsetCoordinates} is not visible");
    }
}

/// <summary>
/// Guard that checks if moving "up" the state hierarchy is allowed.
/// Prevents skipping states (e.g., Invisible -> Selected without going through Visible).
/// </summary>
public class StateHierarchyGuard : GuardBase
{
    private bool allowSkipping;

    public override string Name => "StateHierarchy";
    public override string Description => allowSkipping
        ? "Allows state hierarchy skipping"
        : "Enforces sequential state progression";

    public StateHierarchyGuard(bool allowSkipping = false)
    {
        this.allowSkipping = allowSkipping;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        // Moving down the hierarchy is always allowed
        if (context.ToState < context.FromState)
        {
            return Allow();
        }

        // Moving to the same state is blocked (should be caught earlier)
        if (context.ToState == context.FromState)
        {
            return Deny("Cannot transition to the same state");
        }

        // If skipping is allowed, any upward transition is OK
        if (allowSkipping)
        {
            return Allow();
        }

        // Otherwise, only allow single-step transitions
        int stateDifference = (int)context.ToState - (int)context.FromState;
        if (stateDifference == 1)
        {
            return Allow();
        }

        return Deny($"Cannot skip from {context.FromState} to {context.ToState}. Must progress sequentially.");
    }
}

/// <summary>
/// Guard that checks if a cell is walkable (using pathfinding state).
/// Prevents selection/interaction with unwalkable cells.
/// </summary>
public class IsWalkableGuard : GuardBase
{
    public override string Name => "IsWalkable";
    public override string Description => "Cell must be walkable";

    public override GuardResult Evaluate(GuardContext context)
    {
        if (context.Cell == null)
        {
            return Deny("Cell is null");
        }

        // TODO: Access HexCellPathfindingState when it's exposed
        // For now, this is a placeholder implementation
        // bool isWalkable = context.Cell.PathfindingState?.IsWalkable ?? true;

        // Placeholder: Always allow (implement proper check when pathfinding state is accessible)
        return Allow();
    }
}

/// <summary>
/// Guard that checks if a cell is occupied by a unit.
/// Can be configured to allow or deny occupied cells.
/// </summary>
public class IsOccupiedGuard : GuardBase
{
    private bool requireOccupied;

    public override string Name => requireOccupied ? "MustBeOccupied" : "MustBeEmpty";
    public override string Description => requireOccupied
        ? "Cell must have a unit"
        : "Cell must be empty";

    public IsOccupiedGuard(bool requireOccupied = false)
    {
        this.requireOccupied = requireOccupied;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        if (context.Cell == null)
        {
            return Deny("Cell is null");
        }

        // TODO: Access HexCellPathfindingState.IsOccupied when exposed
        // bool isOccupied = context.Cell.PathfindingState?.IsOccupied ?? false;

        // Placeholder: Always allow (implement proper check when pathfinding state is accessible)
        return Allow();
    }
}

/// <summary>
/// Guard that checks game mode/state (e.g., not during cutscene, not paused, correct turn).
/// Requires a game state manager to be set in GuardContext.
/// </summary>
public class GameModeGuard : GuardBase
{
    private Func<object, bool> gameStatePredicate;
    private string modeName;

    public override string Name => $"GameMode({modeName})";
    public override string Description => $"Game must be in {modeName} mode";

    public GameModeGuard(string modeName, Func<object, bool> gameStatePredicate)
    {
        this.modeName = modeName;
        this.gameStatePredicate = gameStatePredicate;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        if (context.GameState == null)
        {
            // If no game state is provided, allow by default
            return Allow();
        }

        try
        {
            if (gameStatePredicate(context.GameState))
            {
                return Allow();
            }

            return Deny($"Game is not in {modeName} mode");
        }
        catch (Exception e)
        {
            Debug.LogError($"GameModeGuard exception: {e.Message}");
            return Deny($"Failed to check game mode: {e.Message}");
        }
    }
}

/// <summary>
/// Guard that checks player permissions (e.g., is it the player's turn, owns the cell/unit).
/// </summary>
public class PlayerPermissionGuard : GuardBase
{
    private Func<object, GuardContext, bool> permissionCheck;
    private string permissionName;

    public override string Name => $"Permission({permissionName})";
    public override string Description => $"Player must have {permissionName} permission";

    public PlayerPermissionGuard(string permissionName, Func<object, GuardContext, bool> permissionCheck)
    {
        this.permissionName = permissionName;
        this.permissionCheck = permissionCheck;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        if (context.PlayerData == null)
        {
            // If no player data provided, allow by default
            return Allow();
        }

        try
        {
            if (permissionCheck(context.PlayerData, context))
            {
                return Allow();
            }

            return Deny($"Player does not have {permissionName} permission");
        }
        catch (Exception e)
        {
            Debug.LogError($"PlayerPermissionGuard exception: {e.Message}");
            return Deny($"Failed to check permission: {e.Message}");
        }
    }
}

/// <summary>
/// Guard that checks if the transition is happening during a specific input event.
/// Useful for restricting certain transitions to specific user actions.
/// </summary>
public class InputEventGuard : GuardBase
{
    private InputEvent requiredEvent;
    private bool invert;

    public override string Name => invert
        ? $"Not({requiredEvent})"
        : $"Is({requiredEvent})";

    public override string Description => invert
        ? $"Input must NOT be {requiredEvent}"
        : $"Input must be {requiredEvent}";

    public InputEventGuard(InputEvent requiredEvent, bool invert = false)
    {
        this.requiredEvent = requiredEvent;
        this.invert = invert;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        bool matches = context.InputEvent == requiredEvent;

        if (invert)
        {
            matches = !matches;
        }

        if (matches)
        {
            return Allow();
        }

        return Deny(invert
            ? $"Input must not be {requiredEvent}"
            : $"Input must be {requiredEvent}");
    }
}

/// <summary>
/// Guard that checks if transitioning from or to specific states.
/// Useful for restricting certain transitions.
/// </summary>
public class StateTransitionGuard : GuardBase
{
    private CellState? requiredFromState;
    private CellState? requiredToState;

    public override string Name => "StateTransition";
    public override string Description =>
        $"Transition from {requiredFromState?.ToString() ?? "any"} to {requiredToState?.ToString() ?? "any"}";

    public StateTransitionGuard(CellState? from = null, CellState? to = null)
    {
        this.requiredFromState = from;
        this.requiredToState = to;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        if (requiredFromState.HasValue && context.FromState != requiredFromState.Value)
        {
            return Deny($"Must transition from {requiredFromState.Value}, currently {context.FromState}");
        }

        if (requiredToState.HasValue && context.ToState != requiredToState.Value)
        {
            return Deny($"Must transition to {requiredToState.Value}, attempting {context.ToState}");
        }

        return Allow();
    }
}

/// <summary>
/// Guard that checks cell coordinates (useful for tutorial or special zones).
/// </summary>
public class CellCoordinateGuard : GuardBase
{
    private Func<Vector2, bool> coordinatePredicate;
    private string zoneName;

    public override string Name => $"CellZone({zoneName})";
    public override string Description => $"Cell must be in {zoneName} zone";

    public CellCoordinateGuard(string zoneName, Func<Vector2, bool> coordinatePredicate)
    {
        this.zoneName = zoneName;
        this.coordinatePredicate = coordinatePredicate;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        if (context.Cell == null)
        {
            return Deny("Cell is null");
        }

        try
        {
            if (coordinatePredicate(context.Cell.OffsetCoordinates))
            {
                return Allow();
            }

            return Deny($"Cell at {context.Cell.OffsetCoordinates} is not in {zoneName} zone");
        }
        catch (Exception e)
        {
            Debug.LogError($"CellCoordinateGuard exception: {e.Message}");
            return Deny($"Failed to check coordinates: {e.Message}");
        }
    }
}

/// <summary>
/// Guard that prevents rapid state changes (debouncing).
/// Useful for preventing double-clicks or spam.
/// </summary>
public class CooldownGuard : GuardBase
{
    private float cooldownSeconds;
    private float lastTransitionTime = -999f;

    public override string Name => $"Cooldown({cooldownSeconds}s)";
    public override string Description => $"Must wait {cooldownSeconds} seconds between transitions";

    public CooldownGuard(float cooldownSeconds)
    {
        this.cooldownSeconds = cooldownSeconds;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        float currentTime = Time.time;
        float timeSinceLastTransition = currentTime - lastTransitionTime;

        if (timeSinceLastTransition >= cooldownSeconds)
        {
            lastTransitionTime = currentTime;
            return Allow();
        }

        float remainingCooldown = cooldownSeconds - timeSinceLastTransition;
        return Deny($"Cooldown active: {remainingCooldown:F1}s remaining");
    }

    /// <summary>
    /// Reset the cooldown timer
    /// </summary>
    public void Reset()
    {
        lastTransitionTime = -999f;
    }
}
