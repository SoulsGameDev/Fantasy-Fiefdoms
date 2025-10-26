using System;
using UnityEngine;

/// <summary>
/// Context information passed to guards for evaluation.
/// Contains all relevant data about the transition being attempted.
/// </summary>
public class GuardContext
{
    // Cell information
    public HexCell Cell { get; set; }
    public CellState FromState { get; set; }
    public CellState ToState { get; set; }
    public InputEvent InputEvent { get; set; }

    // Game state information (extend as needed)
    public object GameState { get; set; } // Reference to global game state
    public object PlayerData { get; set; } // Reference to current player

    // Additional context
    public string Reason { get; set; } // For storing failure reason

    public GuardContext(HexCell cell, CellState from, CellState to, InputEvent inputEvent)
    {
        Cell = cell;
        FromState = from;
        ToState = to;
        InputEvent = inputEvent;
    }

    /// <summary>
    /// Create a minimal context for simple evaluations
    /// </summary>
    public static GuardContext Create(HexCell cell, CellState from, CellState to)
    {
        return new GuardContext(cell, from, to, InputEvent.Deselect);
    }
}

/// <summary>
/// Result of a guard evaluation with success status and optional reason for failure.
/// </summary>
public struct GuardResult
{
    public bool Success { get; private set; }
    public string Reason { get; private set; }

    private GuardResult(bool success, string reason = null)
    {
        Success = success;
        Reason = reason ?? string.Empty;
    }

    public static GuardResult Allow() => new GuardResult(true);
    public static GuardResult Deny(string reason) => new GuardResult(false, reason);

    public static implicit operator bool(GuardResult result) => result.Success;
}

/// <summary>
/// Interface for transition guards that validate if a state transition is allowed.
/// Implements the Strategy pattern for flexible validation logic.
/// </summary>
public interface ITransitionGuard
{
    /// <summary>
    /// Evaluate if the transition is allowed
    /// </summary>
    /// <param name="context">Context information about the transition</param>
    /// <returns>GuardResult indicating success or failure with reason</returns>
    GuardResult Evaluate(GuardContext context);

    /// <summary>
    /// Name of the guard for debugging/logging
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Optional description of what this guard checks
    /// </summary>
    string Description { get; }
}

/// <summary>
/// Abstract base class for guards with common functionality
/// </summary>
public abstract class GuardBase : ITransitionGuard
{
    public abstract string Name { get; }
    public virtual string Description => Name;

    public abstract GuardResult Evaluate(GuardContext context);

    /// <summary>
    /// Helper method for creating denial results with guard name prefix
    /// </summary>
    protected GuardResult Deny(string reason)
    {
        return GuardResult.Deny($"[{Name}] {reason}");
    }

    /// <summary>
    /// Helper method for creating success results
    /// </summary>
    protected GuardResult Allow()
    {
        return GuardResult.Allow();
    }
}

/// <summary>
/// Simple predicate-based guard for inline lambda expressions
/// </summary>
public class PredicateGuard : GuardBase
{
    private Func<GuardContext, GuardResult> predicate;
    private string name;
    private string description;

    public override string Name => name;
    public override string Description => description;

    public PredicateGuard(string name, Func<GuardContext, GuardResult> predicate, string description = null)
    {
        this.name = name;
        this.predicate = predicate;
        this.description = description ?? name;
    }

    /// <summary>
    /// Simplified constructor for bool predicates
    /// </summary>
    public PredicateGuard(string name, Func<GuardContext, bool> predicate, string failureReason = null, string description = null)
    {
        this.name = name;
        this.description = description ?? name;
        this.predicate = ctx => predicate(ctx)
            ? GuardResult.Allow()
            : GuardResult.Deny(failureReason ?? $"[{name}] Guard failed");
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        try
        {
            return predicate(context);
        }
        catch (Exception e)
        {
            Debug.LogError($"Guard {Name} threw exception: {e.Message}");
            return Deny($"Exception: {e.Message}");
        }
    }
}
