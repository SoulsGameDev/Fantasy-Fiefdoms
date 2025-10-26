using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Key for identifying a specific state transition.
/// </summary>
public struct TransitionKey : IEquatable<TransitionKey>
{
    public CellState From { get; private set; }
    public CellState To { get; private set; }

    public TransitionKey(CellState from, CellState to)
    {
        From = from;
        To = to;
    }

    public bool Equals(TransitionKey other)
    {
        return From == other.From && To == other.To;
    }

    public override bool Equals(object obj)
    {
        return obj is TransitionKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, To);
    }

    public override string ToString()
    {
        return $"{From} -> {To}";
    }

    public static bool operator ==(TransitionKey left, TransitionKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TransitionKey left, TransitionKey right)
    {
        return !left.Equals(right);
    }
}

/// <summary>
/// Registry for managing transition guards.
/// Supports global guards (apply to all transitions) and specific guards (apply to specific transitions).
/// Singleton pattern for global access.
/// </summary>
public class TransitionGuardRegistry
{
    private static TransitionGuardRegistry instance;
    public static TransitionGuardRegistry Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TransitionGuardRegistry();
                instance.InitializeDefaultGuards();
            }
            return instance;
        }
    }

    // Global guards that apply to ALL transitions
    private List<ITransitionGuard> globalGuards = new List<ITransitionGuard>();

    // Guards specific to certain transitions
    private Dictionary<TransitionKey, List<ITransitionGuard>> transitionGuards =
        new Dictionary<TransitionKey, List<ITransitionGuard>>();

    // Guards that apply to any transition FROM a specific state
    private Dictionary<CellState, List<ITransitionGuard>> fromStateGuards =
        new Dictionary<CellState, List<ITransitionGuard>>();

    // Guards that apply to any transition TO a specific state
    private Dictionary<CellState, List<ITransitionGuard>> toStateGuards =
        new Dictionary<CellState, List<ITransitionGuard>>();

    // Enable/disable guard evaluation globally (useful for debugging)
    private bool guardsEnabled = true;

    private TransitionGuardRegistry() { }

    /// <summary>
    /// Initialize default guards that should always be active
    /// </summary>
    private void InitializeDefaultGuards()
    {
        // Example: Always require cells transitioning to interactive states to be explored
        AddToStateGuard(CellState.Highlighted, new IsExploredGuard());
        AddToStateGuard(CellState.Selected, new IsExploredGuard());
        AddToStateGuard(CellState.Focused, new IsExploredGuard());

        // Example: Prevent same-state transitions (already handled in FSM, but as failsafe)
        AddGlobalGuard(new PredicateGuard(
            "NoSameStateTransition",
            ctx => ctx.FromState != ctx.ToState,
            "Cannot transition to the same state"
        ));
    }

    // ===== Global Guards =====

    /// <summary>
    /// Add a guard that applies to ALL transitions
    /// </summary>
    public void AddGlobalGuard(ITransitionGuard guard)
    {
        if (guard != null && !globalGuards.Contains(guard))
        {
            globalGuards.Add(guard);
        }
    }

    /// <summary>
    /// Remove a global guard
    /// </summary>
    public void RemoveGlobalGuard(ITransitionGuard guard)
    {
        globalGuards.Remove(guard);
    }

    /// <summary>
    /// Clear all global guards
    /// </summary>
    public void ClearGlobalGuards()
    {
        globalGuards.Clear();
    }

    // ===== Transition-Specific Guards =====

    /// <summary>
    /// Add a guard for a specific transition
    /// </summary>
    public void AddTransitionGuard(CellState from, CellState to, ITransitionGuard guard)
    {
        var key = new TransitionKey(from, to);

        if (!transitionGuards.ContainsKey(key))
        {
            transitionGuards[key] = new List<ITransitionGuard>();
        }

        if (guard != null && !transitionGuards[key].Contains(guard))
        {
            transitionGuards[key].Add(guard);
        }
    }

    /// <summary>
    /// Remove a guard for a specific transition
    /// </summary>
    public void RemoveTransitionGuard(CellState from, CellState to, ITransitionGuard guard)
    {
        var key = new TransitionKey(from, to);

        if (transitionGuards.ContainsKey(key))
        {
            transitionGuards[key].Remove(guard);
        }
    }

    /// <summary>
    /// Clear all guards for a specific transition
    /// </summary>
    public void ClearTransitionGuards(CellState from, CellState to)
    {
        var key = new TransitionKey(from, to);
        transitionGuards.Remove(key);
    }

    // ===== From-State Guards =====

    /// <summary>
    /// Add a guard that applies to ANY transition FROM a specific state
    /// </summary>
    public void AddFromStateGuard(CellState fromState, ITransitionGuard guard)
    {
        if (!fromStateGuards.ContainsKey(fromState))
        {
            fromStateGuards[fromState] = new List<ITransitionGuard>();
        }

        if (guard != null && !fromStateGuards[fromState].Contains(guard))
        {
            fromStateGuards[fromState].Add(guard);
        }
    }

    /// <summary>
    /// Remove a from-state guard
    /// </summary>
    public void RemoveFromStateGuard(CellState fromState, ITransitionGuard guard)
    {
        if (fromStateGuards.ContainsKey(fromState))
        {
            fromStateGuards[fromState].Remove(guard);
        }
    }

    // ===== To-State Guards =====

    /// <summary>
    /// Add a guard that applies to ANY transition TO a specific state
    /// </summary>
    public void AddToStateGuard(CellState toState, ITransitionGuard guard)
    {
        if (!toStateGuards.ContainsKey(toState))
        {
            toStateGuards[toState] = new List<ITransitionGuard>();
        }

        if (guard != null && !toStateGuards[toState].Contains(guard))
        {
            toStateGuards[toState].Add(guard);
        }
    }

    /// <summary>
    /// Remove a to-state guard
    /// </summary>
    public void RemoveToStateGuard(CellState toState, ITransitionGuard guard)
    {
        if (toStateGuards.ContainsKey(toState))
        {
            toStateGuards[toState].Remove(guard);
        }
    }

    // ===== Evaluation =====

    /// <summary>
    /// Evaluate all applicable guards for a transition
    /// </summary>
    public GuardResult EvaluateTransition(GuardContext context)
    {
        if (!guardsEnabled)
        {
            return GuardResult.Allow();
        }

        // Collect all applicable guards
        List<ITransitionGuard> applicableGuards = new List<ITransitionGuard>();

        // Add global guards
        applicableGuards.AddRange(globalGuards);

        // Add from-state guards
        if (fromStateGuards.ContainsKey(context.FromState))
        {
            applicableGuards.AddRange(fromStateGuards[context.FromState]);
        }

        // Add to-state guards
        if (toStateGuards.ContainsKey(context.ToState))
        {
            applicableGuards.AddRange(toStateGuards[context.ToState]);
        }

        // Add transition-specific guards
        var key = new TransitionKey(context.FromState, context.ToState);
        if (transitionGuards.ContainsKey(key))
        {
            applicableGuards.AddRange(transitionGuards[key]);
        }

        // Evaluate all guards (AND logic - all must pass)
        foreach (var guard in applicableGuards)
        {
            var result = guard.Evaluate(context);
            if (!result.Success)
            {
                // Log the failure
                Debug.LogWarning($"Transition {context.FromState} -> {context.ToState} blocked: {result.Reason}");
                return result;
            }
        }

        return GuardResult.Allow();
    }

    /// <summary>
    /// Check if a transition would be allowed without executing it
    /// </summary>
    public bool CanTransition(GuardContext context)
    {
        return EvaluateTransition(context).Success;
    }

    // ===== Configuration =====

    /// <summary>
    /// Enable or disable all guard evaluation globally
    /// </summary>
    public void SetGuardsEnabled(bool enabled)
    {
        guardsEnabled = enabled;
        Debug.Log($"Guards {(enabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Clear all guards (global and specific)
    /// </summary>
    public void ClearAllGuards()
    {
        globalGuards.Clear();
        transitionGuards.Clear();
        fromStateGuards.Clear();
        toStateGuards.Clear();
    }

    /// <summary>
    /// Reset to default guards
    /// </summary>
    public void ResetToDefaults()
    {
        ClearAllGuards();
        InitializeDefaultGuards();
    }

    // ===== Debugging =====

    /// <summary>
    /// Get a summary of all registered guards for debugging
    /// </summary>
    public string GetGuardSummary()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine($"=== Guard Registry Summary ===");
        sb.AppendLine($"Guards Enabled: {guardsEnabled}");
        sb.AppendLine($"Global Guards: {globalGuards.Count}");
        foreach (var guard in globalGuards)
        {
            sb.AppendLine($"  - {guard.Name}: {guard.Description}");
        }

        sb.AppendLine($"Transition-Specific Guards: {transitionGuards.Count}");
        foreach (var kvp in transitionGuards)
        {
            sb.AppendLine($"  {kvp.Key}: {kvp.Value.Count} guards");
            foreach (var guard in kvp.Value)
            {
                sb.AppendLine($"    - {guard.Name}");
            }
        }

        sb.AppendLine($"From-State Guards: {fromStateGuards.Count}");
        foreach (var kvp in fromStateGuards)
        {
            sb.AppendLine($"  From {kvp.Key}: {kvp.Value.Count} guards");
        }

        sb.AppendLine($"To-State Guards: {toStateGuards.Count}");
        foreach (var kvp in toStateGuards)
        {
            sb.AppendLine($"  To {kvp.Key}: {kvp.Value.Count} guards");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Log the guard summary to console
    /// </summary>
    public void LogGuardSummary()
    {
        Debug.Log(GetGuardSummary());
    }
}
