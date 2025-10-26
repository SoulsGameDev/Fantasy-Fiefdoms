# Command Pattern Implementation Guide

## Overview

This project implements the **Command Pattern** to provide flexible, extensible undo/redo functionality for all game actions. The system is designed to work seamlessly with the FSM and can be extended to any game action (unit movement, building, combat, etc.).

## Why Command Pattern?

### Advantages
- ✅ **Encapsulates actions** - Each command knows how to execute AND undo itself
- ✅ **Flexible** - Works for any game action (state changes, movement, building, etc.)
- ✅ **Composable** - Can chain multiple commands together (MacroCommand)
- ✅ **Serializable** - Can save command history for replays or save files
- ✅ **Extensible** - Easy to add new command types without modifying existing code
- ✅ **Testable** - Each command can be unit tested independently

### Compared to Alternatives
- **State snapshots (Memento Pattern)**: More memory intensive, harder to serialize
- **Simple undo stack**: Not flexible, hard to extend to complex actions
- **Animation undo**: Only works for visual changes, not game logic

---

## Architecture

### Core Components

```
┌─────────────────┐
│    ICommand     │ ← Interface for all commands
└────────┬────────┘
         │
         ├─────────────────────────────┐
         │                             │
┌────────▼────────┐         ┌─────────▼──────────┐
│  CommandBase    │         │  MacroCommand      │
│  (Abstract)     │         │  (Composite)       │
└────────┬────────┘         └────────────────────┘
         │
         ├──────────────┬──────────────┬─────────────┐
         │              │              │             │
┌────────▼────────┐ ┌──▼──────────┐ ┌─▼──────┐   ┌──▼────────┐
│StateChange      │ │MoveUnit     │ │RevealFog│   │Build      │
│Command          │ │Command      │ │Command  │   │Command    │
└─────────────────┘ └─────────────┘ └─────────┘   └───────────┘
         │
         │
┌────────▼────────┐
│ CommandHistory  │ ← Singleton manager for undo/redo stacks
└─────────────────┘
```

### Files

- **ICommand.cs** - Interface and base class for all commands
- **CommandHistory.cs** - Singleton manager for undo/redo functionality
- **StateChangeCommand.cs** - FSM integration (change cell states)
- **MacroCommand.cs** - Composite command for multi-step actions
- **ExampleCommands.cs** - Template commands (movement, building, fog reveal)

---

## How to Use

### 1. Basic Usage - Execute a Command

```csharp
// Create a command
var command = new StateChangeCommand(hexCell, InputEvent.MouseEnter);

// Execute through CommandHistory (automatically adds to undo stack)
bool success = CommandHistory.Instance.ExecuteCommand(command);

if (success)
{
    Debug.Log("Command executed successfully");
}
```

### 2. Undo/Redo

```csharp
// Undo last command
if (CommandHistory.Instance.CanUndo)
{
    CommandHistory.Instance.Undo();
}

// Redo last undone command
if (CommandHistory.Instance.CanRedo)
{
    CommandHistory.Instance.Redo();
}

// Multiple undo/redo
CommandHistory.Instance.UndoMultiple(3); // Undo last 3 commands
CommandHistory.Instance.RedoMultiple(2); // Redo 2 commands
```

### 3. UI Integration

```csharp
// Subscribe to history changes for UI updates
CommandHistory.Instance.OnHistoryChanged += UpdateUndoRedoButtons;

void UpdateUndoRedoButtons()
{
    undoButton.interactable = CommandHistory.Instance.CanUndo;
    redoButton.interactable = CommandHistory.Instance.CanRedo;

    // Show what would be undone/redone
    undoButton.text = $"Undo: {CommandHistory.Instance.GetUndoDescription()}";
    redoButton.text = $"Redo: {CommandHistory.Instance.GetRedoDescription()}";
}
```

### 4. Keyboard Shortcuts

```csharp
void Update()
{
    // Ctrl+Z to undo
    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
    {
        CommandHistory.Instance.Undo();
    }

    // Ctrl+Y to redo
    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
    {
        CommandHistory.Instance.Redo();
    }
}
```

---

## Creating New Commands

### Simple Command Example

```csharp
public class MyCustomCommand : CommandBase
{
    private MyData data;
    private MyData previousState;

    public override string Description => "My custom action";

    public MyCustomCommand(MyData data)
    {
        this.data = data;
    }

    public override bool CanExecute()
    {
        // Validation logic
        return data != null && !isExecuted;
    }

    public override bool Execute()
    {
        if (!CanExecute()) return false;

        // Save state before changing
        previousState = data.Clone();

        // Perform action
        data.DoSomething();

        isExecuted = true;
        return true;
    }

    public override bool Undo()
    {
        if (!CanUndo()) return false;

        // Restore previous state
        data.RestoreFrom(previousState);

        isExecuted = false;
        return true;
    }
}
```

### Using MacroCommand for Complex Actions

```csharp
// Example: Move unit to new cell (involves multiple steps)
public ICommand CreateComplexMoveCommand(Unit unit, HexCell from, HexCell to)
{
    var macro = new MacroCommand($"Move {unit.name}");

    // Step 1: Deselect old cell
    macro.AddCommand(new StateChangeCommand(from, CellState.Visible));

    // Step 2: Move unit
    macro.AddCommand(new MoveUnitCommand(unit, from, to));

    // Step 3: Select new cell
    macro.AddCommand(new StateChangeCommand(to, CellState.Selected));

    // Step 4: Reveal fog around destination
    var cellsToReveal = GetCellsInRadius(to, unit.visionRange);
    macro.AddCommand(new RevealFogCommand(cellsToReveal));

    // All commands execute/undo as a single unit
    return macro;
}

// Usage
var command = CreateComplexMoveCommand(myUnit, currentCell, targetCell);
CommandHistory.Instance.ExecuteCommand(command);

// Undo will reverse ALL steps in reverse order
CommandHistory.Instance.Undo();
```

---

## FSM Integration

### StateChangeCommand

The `StateChangeCommand` integrates the command pattern with the FSM:

```csharp
// Method 1: Via input event (follows FSM transition rules)
var command = new StateChangeCommand(hexCell, InputEvent.MouseEnter);
CommandHistory.Instance.ExecuteCommand(command);

// Method 2: Direct state change (bypasses FSM rules)
var command = new StateChangeCommand(hexCell, CellState.Selected);
CommandHistory.Instance.ExecuteCommand(command);
```

### Example: Replace Direct State Changes

**Before (no undo):**
```csharp
hexCell.HandleInput(InputEvent.MouseEnter);
```

**After (with undo):**
```csharp
var command = new StateChangeCommand(hexCell, InputEvent.MouseEnter);
CommandHistory.Instance.ExecuteCommand(command);
```

Now the state change can be undone!

---

## Best Practices

### 1. Always Use CommandHistory

❌ **Don't:**
```csharp
var command = new StateChangeCommand(hexCell, InputEvent.MouseEnter);
command.Execute(); // Won't be added to undo stack!
```

✅ **Do:**
```csharp
var command = new StateChangeCommand(hexCell, InputEvent.MouseEnter);
CommandHistory.Instance.ExecuteCommand(command); // Properly tracked
```

### 2. Validate Before Execute

```csharp
public override bool CanExecute()
{
    // Check all preconditions
    return unit != null
        && targetCell != null
        && targetCell.IsWalkable
        && unit.HasEnoughMovementPoints()
        && !isExecuted;
}
```

### 3. Store Original State in Constructor

```csharp
public MyCommand(SomeData data)
{
    this.data = data;
    this.originalState = data.Clone(); // ← Capture state NOW
}
```

### 4. Handle Failures Gracefully

```csharp
public override bool Execute()
{
    try
    {
        // Your logic here
        return true;
    }
    catch (Exception e)
    {
        Debug.LogError($"Command failed: {e.Message}");
        return false; // Don't add failed commands to history
    }
}
```

### 5. Use Descriptive Names

```csharp
public override string Description =>
    $"Move {unit.name} from {fromCell.Coordinates} to {toCell.Coordinates}";
```

This helps with:
- Debugging
- UI display ("Undo: Move Warrior from (0,0) to (1,2)")
- Replay systems

---

## Advanced Features

### 1. Command Validation

```csharp
var command = new MoveUnitCommand(unit, fromCell, toCell);

// Check if valid before executing
if (command.CanExecute())
{
    CommandHistory.Instance.ExecuteCommand(command);
}
else
{
    Debug.Log("Cannot move unit: invalid move");
}
```

### 2. History Inspection

```csharp
// Get recent undo history for UI display
List<string> recentActions = CommandHistory.Instance.GetUndoHistory(10);

foreach (var action in recentActions)
{
    Debug.Log($"Recent: {action}");
}
```

### 3. Clear History

```csharp
// Clear all history (e.g., when starting new game or loading save)
CommandHistory.Instance.Clear();
```

### 4. Limit History Size

```csharp
// Prevent memory issues with very long games
CommandHistory.Instance.SetMaxHistorySize(50); // Keep last 50 commands
```

### 5. Conditional Undo

```csharp
// Only allow undo of certain command types during tutorial
if (CommandHistory.Instance.CanUndo)
{
    var description = CommandHistory.Instance.GetUndoDescription();

    if (description.Contains("Move") || description.Contains("Select"))
    {
        CommandHistory.Instance.Undo();
    }
    else
    {
        Debug.Log("Cannot undo this action during tutorial");
    }
}
```

---

## Common Patterns

### Pattern 1: Transactional Updates

When multiple related changes should succeed or fail together:

```csharp
var transaction = new MacroCommand("Build city");
transaction.AddCommand(new DeductResourcesCommand("gold", 100));
transaction.AddCommand(new BuildStructureCommand(cell, "City"));
transaction.AddCommand(new AddPopulationCommand(city, 5));

// All succeed together or all fail (with automatic rollback)
bool success = CommandHistory.Instance.ExecuteCommand(transaction);
```

### Pattern 2: Delayed Execution

Store commands for later execution (e.g., turn-based games):

```csharp
private List<ICommand> plannedActions = new List<ICommand>();

// Planning phase
plannedActions.Add(new MoveUnitCommand(unit1, from1, to1));
plannedActions.Add(new MoveUnitCommand(unit2, from2, to2));

// Execution phase
foreach (var command in plannedActions)
{
    CommandHistory.Instance.ExecuteCommand(command);
}
```

### Pattern 3: Conditional Chains

Execute commands based on previous results:

```csharp
var moveCommand = new MoveUnitCommand(unit, from, to);

if (CommandHistory.Instance.ExecuteCommand(moveCommand))
{
    // Move succeeded, now reveal fog
    var revealCommand = new RevealFogCommand(to, unit.visionRange);
    CommandHistory.Instance.ExecuteCommand(revealCommand);
}
```

---

## Integration Examples

### Example 1: Mouse Click Handler

```csharp
public class HexCellClickHandler : MonoBehaviour
{
    void OnMouseDown()
    {
        HexCell cell = GetComponent<HexCell>();

        // Create and execute command for cell selection
        var command = new StateChangeCommand(cell, InputEvent.MouseDown);
        CommandHistory.Instance.ExecuteCommand(command);
    }
}
```

### Example 2: Unit Movement System

```csharp
public class UnitController : MonoBehaviour
{
    public void MoveUnitToCell(Unit unit, HexCell targetCell)
    {
        // Create complex move command with all side effects
        var macro = new MacroCommand($"Move {unit.name}");

        // Deselect current cell
        if (unit.CurrentCell != null)
        {
            macro.AddCommand(new StateChangeCommand(unit.CurrentCell, CellState.Visible));
        }

        // Move unit
        macro.AddCommand(new MoveUnitCommand(unit, unit.CurrentCell, targetCell));

        // Select new cell
        macro.AddCommand(new StateChangeCommand(targetCell, CellState.Selected));

        // Reveal fog
        var cellsInVision = GetCellsInRadius(targetCell, unit.visionRange);
        macro.AddCommand(new RevealFogCommand(cellsInVision));

        // Execute all as one action
        CommandHistory.Instance.ExecuteCommand(macro);
    }
}
```

### Example 3: Save/Load System

```csharp
// Commands can be serialized for save files or replays
[Serializable]
public class SerializableCommand
{
    public string commandType;
    public string commandData;
}

// TODO: Implement command serialization
// This allows saving entire game history for replays
```

---

## Troubleshooting

### Issue: Undo doesn't restore visual state

**Solution**: Make sure state changes trigger events that update visuals:

```csharp
public override bool Undo()
{
    // Restore state
    cell.ForceState(previousState);

    // Make sure visual feedback is triggered
    // (This happens automatically if using the event system)

    isExecuted = false;
    return true;
}
```

### Issue: MacroCommand fails mid-execution

**Solution**: MacroCommand automatically rolls back. Check logs:

```csharp
var macro = new MacroCommand("Complex action");
// Add commands...

if (!CommandHistory.Instance.ExecuteCommand(macro))
{
    Debug.Log("MacroCommand failed and was rolled back");
    // Check which sub-command failed in the logs
}
```

### Issue: Memory usage growing too large

**Solution**: Limit history size:

```csharp
// In game initialization
CommandHistory.Instance.SetMaxHistorySize(50);
```

Or clear history at checkpoints:

```csharp
// When player saves game or reaches checkpoint
CommandHistory.Instance.Clear();
```

---

## Future Enhancements

### Potential Additions

1. **Command Serialization** - Save/load command history for replays
2. **Command Compression** - Merge similar consecutive commands
3. **Async Commands** - Support for long-running operations
4. **Command Validation Pipeline** - Chain validators before execution
5. **Command Metrics** - Track execution times, success rates
6. **Visual Command History** - In-game timeline of actions
7. **Multiplayer Support** - Sync commands across network
8. **Command Scripting** - Define commands in data files

---

## Summary

The Command Pattern provides a powerful, flexible foundation for undo/redo in your strategy game. Key benefits:

✅ **Every game action can be undone** - Not just state changes
✅ **Composable** - Combine simple commands into complex actions
✅ **Extensible** - Easy to add new command types
✅ **Testable** - Each command is independently testable
✅ **Clean integration** - Works seamlessly with FSM and game systems

**Next Steps:**
1. Replace direct game actions with commands
2. Add undo/redo UI buttons
3. Implement keyboard shortcuts (Ctrl+Z, Ctrl+Y)
4. Create commands for all major game actions
5. Test thoroughly with complex scenarios

---

**Document Version**: 1.0
**Last Updated**: 2025-10-26
**Author**: Claude (AI Assistant)
