using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Guard that requires ALL sub-guards to pass (AND logic).
/// Fails on the first guard that fails.
/// </summary>
public class AndGuard : GuardBase
{
    private List<ITransitionGuard> guards = new List<ITransitionGuard>();
    private string name;

    public override string Name => name ?? $"And({guards.Count} guards)";
    public override string Description => $"All of: {string.Join(", ", guards.Select(g => g.Name))}";

    public AndGuard(string name = null)
    {
        this.name = name;
    }

    public AndGuard(string name, params ITransitionGuard[] guards) : this(name)
    {
        this.guards.AddRange(guards);
    }

    public AndGuard AddGuard(ITransitionGuard guard)
    {
        if (guard != null)
        {
            guards.Add(guard);
        }
        return this;
    }

    public AndGuard AddGuards(IEnumerable<ITransitionGuard> guards)
    {
        foreach (var guard in guards)
        {
            AddGuard(guard);
        }
        return this;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        if (guards.Count == 0)
        {
            return Allow();
        }

        foreach (var guard in guards)
        {
            var result = guard.Evaluate(context);
            if (!result.Success)
            {
                // Fail fast on first failure
                return GuardResult.Deny($"[{Name}] Failed: {result.Reason}");
            }
        }

        return Allow();
    }
}

/// <summary>
/// Guard that requires AT LEAST ONE sub-guard to pass (OR logic).
/// Succeeds on the first guard that passes.
/// </summary>
public class OrGuard : GuardBase
{
    private List<ITransitionGuard> guards = new List<ITransitionGuard>();
    private string name;

    public override string Name => name ?? $"Or({guards.Count} guards)";
    public override string Description => $"Any of: {string.Join(", ", guards.Select(g => g.Name))}";

    public OrGuard(string name = null)
    {
        this.name = name;
    }

    public OrGuard(string name, params ITransitionGuard[] guards) : this(name)
    {
        this.guards.AddRange(guards);
    }

    public OrGuard AddGuard(ITransitionGuard guard)
    {
        if (guard != null)
        {
            guards.Add(guard);
        }
        return this;
    }

    public OrGuard AddGuards(IEnumerable<ITransitionGuard> guards)
    {
        foreach (var guard in guards)
        {
            AddGuard(guard);
        }
        return this;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        if (guards.Count == 0)
        {
            return Allow();
        }

        List<string> failures = new List<string>();

        foreach (var guard in guards)
        {
            var result = guard.Evaluate(context);
            if (result.Success)
            {
                // Succeed fast on first success
                return Allow();
            }
            failures.Add(result.Reason);
        }

        // All guards failed
        return GuardResult.Deny($"[{Name}] All failed: {string.Join("; ", failures)}");
    }
}

/// <summary>
/// Guard that inverts the result of another guard (NOT logic).
/// </summary>
public class NotGuard : GuardBase
{
    private ITransitionGuard innerGuard;
    private string name;

    public override string Name => name ?? $"Not({innerGuard?.Name ?? "null"})";
    public override string Description => $"NOT {innerGuard?.Description ?? "null"}";

    public NotGuard(ITransitionGuard innerGuard, string name = null)
    {
        this.innerGuard = innerGuard;
        this.name = name;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        if (innerGuard == null)
        {
            return Deny("Inner guard is null");
        }

        var result = innerGuard.Evaluate(context);

        // Invert the result
        if (result.Success)
        {
            return Deny($"Inner guard passed (expected failure)");
        }
        else
        {
            return Allow();
        }
    }
}

/// <summary>
/// Guard that always passes (useful for testing or as a placeholder).
/// </summary>
public class AlwaysAllowGuard : GuardBase
{
    public override string Name => "AlwaysAllow";
    public override string Description => "Always allows transitions";

    public override GuardResult Evaluate(GuardContext context)
    {
        return Allow();
    }
}

/// <summary>
/// Guard that always fails (useful for testing or temporarily blocking transitions).
/// </summary>
public class AlwaysDenyGuard : GuardBase
{
    private string reason;

    public override string Name => "AlwaysDeny";
    public override string Description => "Always denies transitions";

    public AlwaysDenyGuard(string reason = "Transition blocked")
    {
        this.reason = reason;
    }

    public override GuardResult Evaluate(GuardContext context)
    {
        return Deny(reason);
    }
}

/// <summary>
/// Helper class for building complex guard expressions fluently.
/// </summary>
public static class GuardBuilder
{
    /// <summary>
    /// Create an AND guard
    /// </summary>
    public static AndGuard All(params ITransitionGuard[] guards)
    {
        return new AndGuard(null, guards);
    }

    /// <summary>
    /// Create an OR guard
    /// </summary>
    public static OrGuard Any(params ITransitionGuard[] guards)
    {
        return new OrGuard(null, guards);
    }

    /// <summary>
    /// Create a NOT guard
    /// </summary>
    public static NotGuard Not(ITransitionGuard guard)
    {
        return new NotGuard(guard);
    }

    /// <summary>
    /// Create a simple predicate guard
    /// </summary>
    public static PredicateGuard When(string name, Func<GuardContext, bool> predicate, string failureReason = null)
    {
        return new PredicateGuard(name, predicate, failureReason);
    }

    /// <summary>
    /// Create a guard that checks if transitioning from a specific state
    /// </summary>
    public static PredicateGuard FromState(CellState state)
    {
        return new PredicateGuard(
            $"FromState({state})",
            ctx => ctx.FromState == state,
            $"Must be transitioning from {state}"
        );
    }

    /// <summary>
    /// Create a guard that checks if transitioning to a specific state
    /// </summary>
    public static PredicateGuard ToState(CellState state)
    {
        return new PredicateGuard(
            $"ToState({state})",
            ctx => ctx.ToState == state,
            $"Must be transitioning to {state}"
        );
    }

    /// <summary>
    /// Create a guard that checks if using a specific input event
    /// </summary>
    public static PredicateGuard WithInput(InputEvent inputEvent)
    {
        return new PredicateGuard(
            $"WithInput({inputEvent})",
            ctx => ctx.InputEvent == inputEvent,
            $"Must be triggered by {inputEvent}"
        );
    }
}
