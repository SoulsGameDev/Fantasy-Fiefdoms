using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages command history for undo/redo functionality.
/// Singleton pattern for global access throughout the game.
/// </summary>
public class CommandHistory
{
    private static CommandHistory instance;
    public static CommandHistory Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CommandHistory();
            }
            return instance;
        }
    }

    private Stack<ICommand> undoStack = new Stack<ICommand>();
    private Stack<ICommand> redoStack = new Stack<ICommand>();

    private int maxHistorySize = 100; // Prevent memory issues
    private long maxMemoryBytes = 10 * 1024 * 1024; // 10MB default limit
    private long currentMemoryUsage = 0; // Estimated memory usage

    // Events for UI updates
    public event Action OnHistoryChanged;

    // Properties for UI/debugging
    public bool CanUndo => undoStack.Count > 0;
    public bool CanRedo => redoStack.Count > 0;
    public int UndoCount => undoStack.Count;
    public int RedoCount => redoStack.Count;

    private CommandHistory() { }

    /// <summary>
    /// Estimate memory usage of a command (rough approximation)
    /// </summary>
    private long EstimateCommandMemorySize(ICommand command)
    {
        // Base command overhead
        long size = 64; // Rough estimate for object overhead

        // Check if it's a macro command
        if (command is MacroCommand macro)
        {
            // MacroCommand contains multiple commands
            foreach (var subCommand in macro.Commands)
            {
                size += EstimateCommandMemorySize(subCommand);
            }
        }
        else
        {
            // Estimate based on command type
            // StateChangeCommand: ~200 bytes (cell reference, states, etc.)
            // Other commands may vary
            size += 200;
        }

        return size;
    }

    /// <summary>
    /// Check and enforce memory limits
    /// </summary>
    private void EnforceMemoryLimit()
    {
        while (currentMemoryUsage > maxMemoryBytes && undoStack.Count > 0)
        {
            // Remove oldest command
            var tempStack = new Stack<ICommand>();
            ICommand removedCommand = null;

            // Pop all but the last item
            while (undoStack.Count > 1)
            {
                tempStack.Push(undoStack.Pop());
            }

            // Remove the oldest (last) item
            if (undoStack.Count > 0)
            {
                removedCommand = undoStack.Pop();
                currentMemoryUsage -= EstimateCommandMemorySize(removedCommand);
            }

            // Restore stack
            while (tempStack.Count > 0)
            {
                undoStack.Push(tempStack.Pop());
            }

            Debug.Log($"Removed oldest command to enforce memory limit. Current usage: {currentMemoryUsage / 1024}KB");
        }
    }

    /// <summary>
    /// Execute a command and add it to history
    /// </summary>
    public bool ExecuteCommand(ICommand command)
    {
        if (command == null)
        {
            Debug.LogError("Cannot execute null command");
            return false;
        }

        if (!command.CanExecute())
        {
            Debug.LogWarning($"Command cannot be executed: {command.Description}");
            return false;
        }

        bool success = command.Execute();

        if (success)
        {
            undoStack.Push(command);
            redoStack.Clear(); // Clear redo stack when new command is executed

            // Track memory usage
            currentMemoryUsage += EstimateCommandMemorySize(command);

            // Enforce memory limit
            EnforceMemoryLimit();

            // Limit history size
            if (undoStack.Count > maxHistorySize)
            {
                TrimHistory();
            }

            OnHistoryChanged?.Invoke();
            Debug.Log($"Executed: {command.Description} (History memory: {currentMemoryUsage / 1024}KB)");
        }
        else
        {
            Debug.LogError($"Failed to execute command: {command.Description}");
        }

        return success;
    }

    /// <summary>
    /// Undo the last command
    /// </summary>
    public bool Undo()
    {
        if (!CanUndo)
        {
            Debug.LogWarning("Nothing to undo");
            return false;
        }

        ICommand command = undoStack.Pop();

        if (!command.CanUndo())
        {
            Debug.LogWarning($"Command cannot be undone: {command.Description}");
            undoStack.Push(command); // Put it back
            return false;
        }

        bool success = command.Undo();

        if (success)
        {
            redoStack.Push(command);
            OnHistoryChanged?.Invoke();
            Debug.Log($"Undone: {command.Description}");
        }
        else
        {
            Debug.LogError($"Failed to undo command: {command.Description}");
            undoStack.Push(command); // Put it back if undo failed
        }

        return success;
    }

    /// <summary>
    /// Redo the last undone command
    /// </summary>
    public bool Redo()
    {
        if (!CanRedo)
        {
            Debug.LogWarning("Nothing to redo");
            return false;
        }

        ICommand command = redoStack.Pop();

        if (!command.CanExecute())
        {
            Debug.LogWarning($"Command cannot be redone: {command.Description}");
            redoStack.Push(command); // Put it back
            return false;
        }

        bool success = command.Redo();

        if (success)
        {
            undoStack.Push(command);
            OnHistoryChanged?.Invoke();
            Debug.Log($"Redone: {command.Description}");
        }
        else
        {
            Debug.LogError($"Failed to redo command: {command.Description}");
            redoStack.Push(command); // Put it back if redo failed
        }

        return success;
    }

    /// <summary>
    /// Clear all command history
    /// </summary>
    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
        currentMemoryUsage = 0;
        OnHistoryChanged?.Invoke();
        Debug.Log("Command history cleared");
    }

    /// <summary>
    /// Undo multiple commands at once
    /// </summary>
    public bool UndoMultiple(int count)
    {
        bool allSuccess = true;
        for (int i = 0; i < count && CanUndo; i++)
        {
            if (!Undo())
            {
                allSuccess = false;
                break;
            }
        }
        return allSuccess;
    }

    /// <summary>
    /// Redo multiple commands at once
    /// </summary>
    public bool RedoMultiple(int count)
    {
        bool allSuccess = true;
        for (int i = 0; i < count && CanRedo; i++)
        {
            if (!Redo())
            {
                allSuccess = false;
                break;
            }
        }
        return allSuccess;
    }

    /// <summary>
    /// Get description of the command that would be undone
    /// </summary>
    public string GetUndoDescription()
    {
        return CanUndo ? undoStack.Peek().Description : "Nothing to undo";
    }

    /// <summary>
    /// Get description of the command that would be redone
    /// </summary>
    public string GetRedoDescription()
    {
        return CanRedo ? redoStack.Peek().Description : "Nothing to redo";
    }

    /// <summary>
    /// Get all undo descriptions for UI display
    /// </summary>
    public List<string> GetUndoHistory(int maxCount = 10)
    {
        List<string> history = new List<string>();
        int count = 0;
        foreach (var command in undoStack)
        {
            if (count >= maxCount) break;
            history.Add(command.Description);
            count++;
        }
        return history;
    }

    /// <summary>
    /// Get all redo descriptions for UI display
    /// </summary>
    public List<string> GetRedoHistory(int maxCount = 10)
    {
        List<string> history = new List<string>();
        int count = 0;
        foreach (var command in redoStack)
        {
            if (count >= maxCount) break;
            history.Add(command.Description);
            count++;
        }
        return history;
    }

    /// <summary>
    /// Set maximum history size (default: 100)
    /// </summary>
    public void SetMaxHistorySize(int size)
    {
        maxHistorySize = Mathf.Max(1, size);
        TrimHistory();
    }

    /// <summary>
    /// Set maximum memory usage in bytes (default: 10MB)
    /// </summary>
    public void SetMaxMemoryBytes(long bytes)
    {
        maxMemoryBytes = Mathf.Max(1024, bytes);
        EnforceMemoryLimit();
    }

    /// <summary>
    /// Get current memory usage estimate in bytes
    /// </summary>
    public long GetCurrentMemoryUsage()
    {
        return currentMemoryUsage;
    }

    /// <summary>
    /// Get current memory usage in human-readable format
    /// </summary>
    public string GetMemoryUsageString()
    {
        if (currentMemoryUsage < 1024)
            return $"{currentMemoryUsage} B";
        else if (currentMemoryUsage < 1024 * 1024)
            return $"{currentMemoryUsage / 1024} KB";
        else
            return $"{currentMemoryUsage / (1024 * 1024)} MB";
    }

    /// <summary>
    /// Trim history to max size (removes oldest commands)
    /// </summary>
    private void TrimHistory()
    {
        if (undoStack.Count <= maxHistorySize) return;

        // Convert to list, remove oldest, convert back
        var tempList = new List<ICommand>(undoStack);
        tempList.RemoveRange(maxHistorySize, tempList.Count - maxHistorySize);
        undoStack = new Stack<ICommand>(tempList);
        tempList.Reverse();
        undoStack = new Stack<ICommand>(tempList);
    }
}
