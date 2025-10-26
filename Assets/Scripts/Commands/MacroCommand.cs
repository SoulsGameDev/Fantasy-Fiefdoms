using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Composite command that executes multiple commands as a single unit.
/// All sub-commands succeed together or fail together.
/// Useful for complex actions like "Move unit AND deselect old cell AND select new cell".
/// </summary>
public class MacroCommand : CommandBase
{
    private List<ICommand> commands = new List<ICommand>();
    private string description;

    public override string Description => description ?? $"Macro ({commands.Count} commands)";

    /// <summary>
    /// Public access to commands for memory estimation
    /// </summary>
    public IReadOnlyList<ICommand> Commands => commands;

    public MacroCommand(string description = null)
    {
        this.description = description;
    }

    public MacroCommand(string description, params ICommand[] commands) : this(description)
    {
        this.commands.AddRange(commands);
    }

    /// <summary>
    /// Add a command to the macro
    /// </summary>
    public void AddCommand(ICommand command)
    {
        if (command != null)
        {
            commands.Add(command);
        }
    }

    /// <summary>
    /// Add multiple commands to the macro
    /// </summary>
    public void AddCommands(IEnumerable<ICommand> commands)
    {
        foreach (var command in commands)
        {
            AddCommand(command);
        }
    }

    public override bool CanExecute()
    {
        // Can execute if at least one command exists and all can execute
        return commands.Count > 0 && commands.All(c => c.CanExecute());
    }

    public override bool CanUndo()
    {
        // Can undo if executed and all commands can undo
        return isExecuted && commands.All(c => c.CanUndo());
    }

    public override bool Execute()
    {
        if (!CanExecute())
        {
            return false;
        }

        List<ICommand> executedCommands = new List<ICommand>();

        try
        {
            // Execute all commands
            foreach (var command in commands)
            {
                if (!command.Execute())
                {
                    // Rollback already executed commands
                    Debug.LogWarning($"MacroCommand failed at: {command.Description}. Rolling back...");
                    RollbackCommands(executedCommands);
                    return false;
                }
                executedCommands.Add(command);
            }

            isExecuted = true;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"MacroCommand execution failed: {e.Message}. Rolling back...");
            RollbackCommands(executedCommands);
            return false;
        }
    }

    public override bool Undo()
    {
        if (!CanUndo())
        {
            return false;
        }

        try
        {
            // Undo commands in reverse order
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                if (!commands[i].Undo())
                {
                    Debug.LogError($"MacroCommand undo failed at: {commands[i].Description}");
                    // Continue undoing other commands even if one fails
                }
            }

            isExecuted = false;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"MacroCommand undo failed: {e.Message}");
            return false;
        }
    }

    public override bool Redo()
    {
        return Execute();
    }

    /// <summary>
    /// Rollback commands that were already executed (used when macro fails mid-execution)
    /// </summary>
    private void RollbackCommands(List<ICommand> executedCommands)
    {
        for (int i = executedCommands.Count - 1; i >= 0; i--)
        {
            try
            {
                executedCommands[i].Undo();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to rollback command {executedCommands[i].Description}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Get all sub-command descriptions for debugging
    /// </summary>
    public List<string> GetCommandDescriptions()
    {
        return commands.Select(c => c.Description).ToList();
    }
}
