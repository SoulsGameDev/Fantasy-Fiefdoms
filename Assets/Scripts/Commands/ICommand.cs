using System;
using UnityEngine;

/// <summary>
/// Interface for all undoable commands in the game.
/// Implements the Command Pattern for flexible undo/redo functionality.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Execute the command (perform the action)
    /// </summary>
    /// <returns>True if execution succeeded, false otherwise</returns>
    bool Execute();

    /// <summary>
    /// Undo the command (reverse the action)
    /// </summary>
    /// <returns>True if undo succeeded, false otherwise</returns>
    bool Undo();

    /// <summary>
    /// Redo the command (re-execute after undo)
    /// Default implementation calls Execute() again
    /// Override if redo logic differs from execute
    /// </summary>
    /// <returns>True if redo succeeded, false otherwise</returns>
    bool Redo() => Execute();

    /// <summary>
    /// Description of the command for UI/debugging
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Check if the command can be executed in the current game state
    /// </summary>
    /// <returns>True if command is valid, false otherwise</returns>
    bool CanExecute();

    /// <summary>
    /// Check if the command can be undone in the current game state
    /// </summary>
    /// <returns>True if command can be undone, false otherwise</returns>
    bool CanUndo();
}

/// <summary>
/// Abstract base class for commands with common functionality
/// </summary>
public abstract class CommandBase : ICommand
{
    protected bool isExecuted = false;

    public abstract string Description { get; }

    public abstract bool Execute();
    public abstract bool Undo();

    public virtual bool Redo()
    {
        return Execute();
    }

    public virtual bool CanExecute()
    {
        return !isExecuted;
    }

    public virtual bool CanUndo()
    {
        return isExecuted;
    }
}
