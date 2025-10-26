# Fantasy-Fiefdoms - Complete Repository Documentation

**Version:** 1.0
**Date:** October 26, 2025
**Repository:** SoulsGameDev/Fantasy-Fiefdoms

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Repository Structure](#2-repository-structure)
3. [Core Game Systems](#3-core-game-systems)
4. [Architectural Patterns](#4-architectural-patterns)
5. [Key Features](#5-key-features)
6. [Configuration & Setup](#6-configuration--setup)
7. [Testing Framework](#7-testing-framework)
8. [Development Workflow](#8-development-workflow)
9. [File Reference](#9-file-reference)
10. [Future Enhancements](#10-future-enhancements)

---

## 1. Project Overview

### 1.1 Project Description

**Fantasy-Fiefdoms** is a turn-based strategy game built with Unity, featuring a sophisticated hexagonal grid system. The game demonstrates enterprise-grade software architecture with advanced state management, undo/redo capabilities, and flexible validation systems.

### 1.2 Key Statistics

| Metric | Value |
|--------|-------|
| Total C# Lines | ~4,235 |
| Documentation Lines | 1,500+ |
| Core Systems | 7 major systems |
| Design Patterns | 9+ patterns |
| Test Coverage | Coordinate conversion system |
| Unity Version | 2022.x+ (inferred) |

### 1.3 Technology Stack

- **Engine:** Unity (with Universal Render Pipeline)
- **Camera System:** Cinemachine 2.9.7
- **Input:** Unity Input System 1.6.3
- **Testing:** NUnit (Unity Test Framework 1.1.33)
- **UI:** TextMeshPro 3.0.6
- **Graphics:** URP 14.0.8

### 1.4 Game Genre

Turn-based strategy game (similar to Civilization series) featuring:
- Hexagonal grid-based gameplay
- Fog of war system
- Territory exploration and selection
- Strategic camera controls
- Multi-layered interaction states

---

## 2. Repository Structure

### 2.1 Directory Layout

```
Fantasy-Fiefdoms/
├── Assets/
│   ├── Editor/                      # Unity Editor extensions
│   │   ├── HexGridEditor.cs
│   │   ├── HexGridMeshGeneratorEditor.cs
│   │   ├── MapGeneratorEditor.cs
│   │   └── MapDisplayEditor.cs
│   │
│   ├── Materials/                   # Visual materials (13 files)
│   │   ├── Grass.mat
│   │   ├── Water.mat
│   │   ├── Sand.mat
│   │   ├── Ice.mat
│   │   └── ... (9 more materials)
│   │
│   ├── Prefabs/                     # Game object templates
│   │   ├── Hex Base.prefab
│   │   ├── Grass.prefab
│   │   ├── Forest.prefab
│   │   ├── Mountains.prefab
│   │   └── ... (10+ prefabs)
│   │
│   ├── Scenes/                      # Unity scenes
│   │   ├── SampleScene.unity
│   │   ├── CameraTest Scene.unity
│   │   ├── Test Scene.unity
│   │   └── Noise Map Tester.unity
│   │
│   ├── ScriptableObjects/           # Data-driven configuration
│   │   └── TerrainTypes/
│   │
│   ├── Scripts/                     # Core game logic (~4,235 LOC)
│   │   ├── Commands/                # Undo/Redo system
│   │   │   ├── ICommand.cs
│   │   │   ├── CommandHistory.cs
│   │   │   ├── StateChangeCommand.cs
│   │   │   ├── MacroCommand.cs
│   │   │   └── ExampleCommands.cs
│   │   │
│   │   ├── Grid/                    # Hexagonal grid system
│   │   │   ├── HexGrid.cs
│   │   │   ├── HexCell.cs
│   │   │   ├── HexMetrics.cs
│   │   │   ├── HexCellStateManager.cs
│   │   │   ├── HexCellInteractionState.cs
│   │   │   ├── HexCellPathfindingState.cs
│   │   │   ├── HexGridMeshGenerator.cs
│   │   │   ├── MapGenerator.cs
│   │   │   └── TerrainType.cs
│   │   │
│   │   ├── Guards/                  # State validation system
│   │   │   ├── ITransitionGuard.cs
│   │   │   ├── CommonGuards.cs
│   │   │   ├── CompositeGuards.cs
│   │   │   └── TransitionGuardRegistry.cs
│   │   │
│   │   ├── UI/                      # User interface
│   │   │   ├── MapDisplay.cs
│   │   │   ├── LoadingUI.cs
│   │   │   ├── ParameterDisplay.cs
│   │   │   └── AllParameterDisplay.cs
│   │   │
│   │   ├── Video Specific/          # Visual effects
│   │   │   └── MapAnimator.cs
│   │   │
│   │   └── Core Scripts:
│   │       ├── CameraController.cs
│   │       ├── MouseController.cs
│   │       ├── OrbitCameraWithTime.cs
│   │       ├── Singleton.cs
│   │       ├── Noise.cs
│   │       ├── TextureGenerator.cs
│   │       ├── MainThreadDispatcher.cs
│   │       └── ResourceManager.cs
│   │
│   └── Tests/                       # Unit tests
│       └── HexMetricsTest.cs
│
├── Packages/                        # Unity package configuration
│   ├── manifest.json
│   └── packages-lock.json
│
├── ProjectSettings/                 # Unity project settings
│
├── Documentation:
│   ├── README.md
│   ├── FSM_FEATURE_ANALYSIS.md      # FSM system guide (v2.0)
│   ├── COMMAND_PATTERN_GUIDE.md     # Undo/Redo guide
│   ├── TRANSITION_GUARDS_GUIDE.md   # Validation guide
│   └── REPOSITORY_DOCUMENTATION.md  # This file
│
└── .git/                            # Version control
```

### 2.2 Code Organization by System

| System | Files | Lines of Code | Location |
|--------|-------|---------------|----------|
| Hexagonal Grid | 9 files | ~800 LOC | `Assets/Scripts/Grid/` |
| Command Pattern | 5 files | ~500 LOC | `Assets/Scripts/Commands/` |
| Guards System | 4 files | ~500 LOC | `Assets/Scripts/Guards/` |
| Core Systems | 8 files | ~400 LOC | `Assets/Scripts/` |
| UI Systems | 4 files | ~200 LOC | `Assets/Scripts/UI/` |
| Tests | 1 file | ~476 LOC | `Assets/Tests/` |
| **Total** | **31 files** | **~4,235 LOC** | - |

---

## 3. Core Game Systems

### 3.1 Hexagonal Grid System

**Purpose:** Provides the foundation for the game world using hexagonal tiles.

**Key Files:**
- `HexGrid.cs` - Grid manager and cell instantiation
- `HexCell.cs` - Individual hex tile (data + behavior)
- `HexMetrics.cs` - Mathematical utilities and coordinate conversions
- `HexGridMeshGenerator.cs` - Visual mesh generation
- `TerrainType.cs` - Terrain type definitions (ScriptableObject)

**Features:**

#### Coordinate Systems
The system supports three coordinate systems with bidirectional conversion:

1. **Offset Coordinates** (Column, Row)
   - Human-readable grid positions
   - Used for storage and display
   - Format: `(x, y)` where x = column, y = row

2. **Axial Coordinates** (Q, R)
   - Simplified cube coordinates
   - Q = x-axis, R = y-axis
   - Format: `(q, r)`

3. **Cube Coordinates** (Q, R, S)
   - Canonical hex representation
   - Property: `Q + R + S = 0`
   - Simplifies distance/neighbor calculations
   - Format: `(q, r, s)`

**Example conversions:**
```csharp
// Offset to Cube
Vector3 cubeCoords = HexMetrics.OffsetToCube(x, y, orientation);

// Cube to Offset
Vector2 offset = HexMetrics.CubeToOffset(cubeCoords, orientation);

// World position to grid coordinates
Vector2 hexCoords = HexMetrics.CoordinateToOffset(worldX, worldZ, hexSize, orientation);
```

#### Hex Orientations
- **FlatTop:** Flat side on top (┴ shape)
- **PointyTop:** Pointed side on top (◇ shape)

#### Grid Properties
```csharp
public int Width { get; private set; }           // Grid width in tiles
public int Height { get; private set; }          // Grid height in tiles
public float HexSize { get; private set; }       // Size of each hex
public HexOrientation Orientation { get; }       // FlatTop or PointyTop
public HexCell[,] Cells { get; private set; }   // 2D array of cells
```

#### Neighbor System
- Each hex has 6 neighbors
- Lazy neighbor calculation (computed on first access)
- Neighbor relationships managed by grid

#### Performance Optimizations
- **Threaded Generation:** Background thread for cell data generation
- **Batch Instantiation:** Cells instantiated in batches to prevent frame stutters
- **Event Pipeline:** Progress events for loading screens

**Events:**
```csharp
public event Action OnMapInfoGenerated;              // Data structure created
public event Action<float> OnCellBatchGenerated;     // Batch instantiated (progress 0-1)
public event Action OnCellInstancesGenerated;        // All cells ready
```

**File:** `Assets/Scripts/Grid/HexGrid.cs:1`

---

### 3.2 Finite State Machine (FSM) System

**Purpose:** Manages hex cell interaction states with hierarchical progression and event-driven behavior.

**Key Files:**
- `HexCellInteractionState.cs` - State holder with granular events (115 LOC)
- `HexCellStateManager.cs` - Transition logic and validation (302 LOC)
- `HexCell.cs` - FSM integration point

#### State Hierarchy

States are hierarchically organized with integer values (0-4):

```
CellState.Invisible (0)      → Fog of war, no interaction
    ↓
CellState.Visible (1)        → Explored, normal state
    ↓
CellState.Highlighted (2)    → Mouse hover
    ↓
CellState.Selected (3)       → Clicked/active
    ↓
CellState.Focused (4)        → Camera focused
```

**Hierarchy Benefits:**
- Check if state is "at least" a level: `cell.IsExplored()` checks `State >= Visible`
- Sequential progression validation via `StateHierarchyGuard`
- Downward transitions always allowed
- Upward transitions require validation

#### Input Events

```csharp
public enum InputEvent
{
    MouseEnter,   // Cursor enters hex bounds
    MouseExit,    // Cursor leaves hex bounds
    MouseDown,    // Left click pressed
    MouseUp,      // Left click released
    FKeyDown,     // F key pressed (focus)
    Deselect,     // Programmatic deselection
    RevealFog     // Fog of war reveal
}
```

#### State Transition Rules

| Current State | Input Event | Next State | Guard Required |
|--------------|-------------|------------|----------------|
| **Invisible** | RevealFog | Visible | IsExploredGuard |
| **Visible** | MouseEnter | Highlighted | - |
| **Visible** | MouseDown | Selected | Permissions |
| **Visible** | FKeyDown | Focused | - |
| **Highlighted** | MouseExit | Visible | - |
| **Highlighted** | MouseDown | Selected | Permissions |
| **Selected** | MouseExit | Visible | - |
| **Selected** | MouseUp | Highlighted | - |
| **Selected** | Deselect | Visible | - |
| **Selected** | FKeyDown | Focused | - |
| **Focused** | MouseDown | Selected | - |
| **Focused** | Deselect | Visible | - |

#### Event System

**General Events:**
```csharp
// State transition with from/to states
public event Action<CellState, CellState> OnStateChanged;

// Transition blocked with reason
public event Action<CellState, CellState, string> OnTransitionBlocked;
```

**Granular State Events:**
```csharp
// Enter/Exit events for each state
public event Action OnEnterInvisible, OnExitInvisible;
public event Action OnEnterVisible, OnExitVisible;
public event Action OnEnterHighlighted, OnExitHighlighted;
public event Action OnEnterSelected, OnExitSelected;
public event Action OnEnterFocused, OnExitFocused;
```

#### Usage Example

```csharp
// Subscribe to state changes
hexCell.InteractionState.OnEnterHighlighted += () => {
    hexCell.SetHighlightEffect(true);
};

hexCell.InteractionState.OnExitHighlighted += () => {
    hexCell.SetHighlightEffect(false);
};

// Trigger state change via input
hexCell.HandleInput(InputEvent.MouseEnter);

// Programmatic state change
hexCell.SetState(CellState.Selected);
```

**File References:**
- State Manager: `Assets/Scripts/Grid/HexCellStateManager.cs:1`
- State Holder: `Assets/Scripts/Grid/HexCellInteractionState.cs:1`
- Integration: `Assets/Scripts/Grid/HexCell.cs:1`

---

### 3.3 Transition Guards System

**Purpose:** Flexible, composable validation system for state transitions using the Strategy Pattern.

**Key Files:**
- `ITransitionGuard.cs` - Guard interface and base class (154 LOC)
- `CommonGuards.cs` - Reusable guard implementations (300+ LOC)
- `CompositeGuards.cs` - Logical combinators (200+ LOC)
- `TransitionGuardRegistry.cs` - Centralized management (250+ LOC)

#### Guard Architecture

```
ITransitionGuard (interface)
    ├── CanTransition(GuardContext) → GuardResult
    └── Name property

GuardBase (abstract base)
    ├── Common Guards (11 implementations)
    └── Composite Guards (3 implementations)

TransitionGuardRegistry (Singleton)
    ├── Global Guards (apply to all transitions)
    ├── To-State Guards (apply when entering state)
    ├── From-State Guards (apply when leaving state)
    └── Transition-Specific Guards (apply to specific pairs)
```

#### Common Guards

| Guard | Purpose | Example Use |
|-------|---------|-------------|
| **IsExploredGuard** | Prevents interaction with fog of war | Global guard |
| **IsVisibleGuard** | Checks current visibility | Selection validation |
| **StateHierarchyGuard** | Enforces sequential progression | Prevent skipping states |
| **IsWalkableGuard** | Validates terrain walkability | Unit movement |
| **IsOccupiedGuard** | Checks for unit occupation | Placement validation |
| **GameModeGuard** | Validates global game state | Turn-based logic |
| **PlayerPermissionGuard** | Checks ownership/turn | Multiplayer |
| **InputEventGuard** | Validates input type | Event filtering |
| **CooldownGuard** | Debounces rapid inputs | Rate limiting |
| **CellCoordinateGuard** | Zone/region restrictions | Map boundaries |
| **PredicateGuard** | Inline lambda validation | Custom logic |
| **StateTransitionGuard** | From/to state validation | Specific transitions |

#### Composite Guards

**AndGuard** - All must pass (fail-fast):
```csharp
var guard = new AndGuard("ComplexValidation")
    .AddGuard(new IsExploredGuard())
    .AddGuard(new IsWalkableGuard());
// Both must return true
```

**OrGuard** - Any must pass (succeed-fast):
```csharp
var guard = new OrGuard("AlternativeValidation")
    .AddGuard(new PlayerPermissionGuard("Owner", ...))
    .AddGuard(new PlayerPermissionGuard("Ally", ...));
// Either ownership check succeeds
```

**NotGuard** - Inverts result:
```csharp
var guard = new NotGuard("NotOccupied", new IsOccupiedGuard());
// Returns true if cell is NOT occupied
```

#### Guard Context

All guards receive context information:
```csharp
public class GuardContext
{
    public HexCell Cell { get; set; }         // Target cell
    public CellState FromState { get; set; }  // Current state
    public CellState ToState { get; set; }    // Desired state
    public InputEvent InputEvent { get; set; } // Input trigger
    public object GameState { get; set; }     // Global game state
    public object PlayerData { get; set; }    // Player information
}
```

#### Guard Result

```csharp
public struct GuardResult
{
    public bool Success { get; private set; }  // Pass/fail
    public string Reason { get; private set; } // Failure explanation
}
```

#### Registry Management

**Four Guard Levels:**
1. **Global Guards** - Apply to ALL transitions
2. **To-State Guards** - Apply when ENTERING a specific state
3. **From-State Guards** - Apply when LEAVING a specific state
4. **Transition-Specific** - Apply to specific (from, to) pairs

**Registration Examples:**
```csharp
var registry = TransitionGuardRegistry.Instance;

// Global guard (all transitions)
registry.AddGlobalGuard(new IsExploredGuard());

// To-state guard (entering Selected)
registry.AddToStateGuard(
    CellState.Selected,
    new PlayerPermissionGuard(...)
);

// From-state guard (leaving Focused)
registry.AddFromStateGuard(
    CellState.Focused,
    new CameraAvailableGuard()
);

// Transition-specific (Visible → Focused only)
registry.AddTransitionGuard(
    CellState.Visible,
    CellState.Focused,
    new FocusRequirementGuard()
);
```

#### Evaluation Order

When a transition is requested:
1. **Global guards** evaluated first (short-circuit on failure)
2. **From-state guards** evaluated next
3. **To-state guards** evaluated third
4. **Transition-specific guards** evaluated last

**File References:**
- Registry: `Assets/Scripts/Guards/TransitionGuardRegistry.cs:1`
- Common Guards: `Assets/Scripts/Guards/CommonGuards.cs:1`
- Composites: `Assets/Scripts/Guards/CompositeGuards.cs:1`

---

### 3.4 Command Pattern System

**Purpose:** Encapsulates actions as objects to support undo/redo, transaction rollback, and action history.

**Key Files:**
- `ICommand.cs` - Command interface (74 LOC)
- `CommandHistory.cs` - Undo/redo manager (150+ LOC)
- `StateChangeCommand.cs` - FSM integration (159 LOC)
- `MacroCommand.cs` - Composite command (156 LOC)
- `ExampleCommands.cs` - Template implementations (50+ LOC)

#### Command Interface

```csharp
public interface ICommand
{
    bool Execute();          // Perform the action
    bool Undo();            // Reverse the action
    bool Redo();            // Re-execute the action
    bool CanExecute();      // Pre-execution validation
    bool CanUndo();         // Undo validity check
    string Description { get; } // For UI/debug
}
```

#### Command Implementations

**StateChangeCommand** - FSM state transitions:
```csharp
// Create command for state change
var command = new StateChangeCommand(
    hexCell,
    InputEvent.MouseDown  // Or direct state: CellState.Selected
);

// Execute through history
CommandHistory.Instance.ExecuteCommand(command);

// Undo later
CommandHistory.Instance.Undo(); // Restores previous state
```

**MoveUnitCommand** (template example):
```csharp
public class MoveUnitCommand : CommandBase
{
    private Unit unit;
    private HexCell fromCell;
    private HexCell toCell;
    private Vector3 previousPosition;

    public override bool Execute()
    {
        if (!CanExecute()) return false;
        previousPosition = unit.transform.position;
        unit.MoveTo(toCell);
        return true;
    }

    public override bool Undo()
    {
        unit.MoveTo(fromCell);
        unit.transform.position = previousPosition;
        return true;
    }
}
```

**MacroCommand** - Composite commands (all-or-nothing):
```csharp
// Create multi-step transaction
var moveTransaction = new MacroCommand("Move and reveal");

// Add steps
moveTransaction.AddCommand(
    new StateChangeCommand(oldCell, InputEvent.Deselect)
);
moveTransaction.AddCommand(
    new MoveUnitCommand(unit, oldCell, newCell)
);
moveTransaction.AddCommand(
    new StateChangeCommand(newCell, CellState.Selected)
);
moveTransaction.AddCommand(
    new RevealFogCommand(visibleCells)
);

// Execute atomically (all succeed or all rollback)
bool success = CommandHistory.Instance.ExecuteCommand(moveTransaction);

// Undo reverses ALL steps in reverse order
CommandHistory.Instance.Undo();
```

#### CommandHistory Manager

**Features:**
```csharp
public class CommandHistory : Singleton<CommandHistory>
{
    // Core operations
    public bool ExecuteCommand(ICommand command);  // Execute and add to history
    public bool Undo();                            // Undo last command
    public bool Redo();                            // Redo last undone command
    public void UndoMultiple(int count);           // Undo multiple steps
    public void RedoMultiple(int count);           // Redo multiple steps

    // State queries
    public bool CanUndo { get; }                   // Is undo available?
    public bool CanRedo { get; }                   // Is redo available?
    public int UndoCount { get; }                  // Number of undo-able commands
    public int RedoCount { get; }                  // Number of redo-able commands

    // History management
    public void Clear();                           // Clear all history
    public void SetMaxHistorySize(int size);       // Limit memory (default 100)
    public List<string> GetUndoHistory(int count); // Recent actions
    public string GetUndoDescription();            // What will be undone
    public string GetRedoDescription();            // What will be redone

    // Events
    public event Action OnHistoryChanged;          // Fired when history changes
}
```

**Memory Management:**
- Default max history: 100 commands
- Configurable via `SetMaxHistorySize()`
- Oldest commands automatically removed when limit reached

#### Usage Patterns

**Simple Command:**
```csharp
// Create and execute
var cmd = new StateChangeCommand(cell, InputEvent.MouseDown);
CommandHistory.Instance.ExecuteCommand(cmd);

// Check if we can undo
if (CommandHistory.Instance.CanUndo)
{
    CommandHistory.Instance.Undo(); // Reverts the state change
}
```

**Macro Command (Transaction):**
```csharp
var transaction = new MacroCommand("Attack sequence");
transaction.AddCommand(new MoveUnitCommand(attacker, from, to));
transaction.AddCommand(new AttackCommand(attacker, defender));
transaction.AddCommand(new UpdateHealthCommand(defender, damage));

// Execute all or rollback all
if (CommandHistory.Instance.ExecuteCommand(transaction))
{
    Debug.Log("Attack succeeded");
}
else
{
    Debug.Log("Attack failed - all changes rolled back");
}
```

**History UI Integration:**
```csharp
// Subscribe to history changes
CommandHistory.Instance.OnHistoryChanged += UpdateUndoRedoButtons;

void UpdateUndoRedoButtons()
{
    undoButton.interactable = CommandHistory.Instance.CanUndo;
    redoButton.interactable = CommandHistory.Instance.CanRedo;

    undoButton.text = CommandHistory.Instance.GetUndoDescription();
    redoButton.text = CommandHistory.Instance.GetRedoDescription();
}
```

**File References:**
- Command History: `Assets/Scripts/Commands/CommandHistory.cs:1`
- State Command: `Assets/Scripts/Commands/StateChangeCommand.cs:1`
- Macro Command: `Assets/Scripts/Commands/MacroCommand.cs:1`

---

### 3.5 Camera Control System

**Purpose:** Manages camera modes, movement, zoom, and focus with smooth transitions.

**Key Files:**
- `CameraController.cs` - Camera manager (150+ LOC)
- `OrbitCameraWithTime.cs` - Auto-rotation camera (80+ LOC)

#### Camera Modes

```csharp
public enum CameraMode
{
    TopDown,  // Strategy overview (RTS-style)
    Focus     // Cell-focused view (close-up)
}
```

#### Features

**Camera References:**
```csharp
[SerializeField] private CinemachineVirtualCamera topDownCamera;
[SerializeField] private CinemachineVirtualCamera focusCamera;
```

**Movement Settings:**
```csharp
public float cameraSpeed = 10f;            // Movement speed
public float cameraDamping = 5f;           // Smooth damping
public Vector2 cameraBoundsMin = (-100, -100); // Map bounds
public Vector2 cameraBoundsMax = (100, 100);
```

**Zoom Settings:**
```csharp
public float cameraZoomSpeed = 1f;         // Zoom sensitivity
public float cameraZoomMin = 15f;          // Closest zoom
public float cameraZoomMax = 100f;         // Farthest zoom
public float cameraZoomDefault = 50f;      // Starting zoom
```

**Rotation Settings:**
```csharp
public bool enableRotation = false;        // Enable/disable rotation
public float cameraRotationSpeed = 50f;    // Rotation speed
```

#### Public Methods

```csharp
// Mode switching
public void ChangeCamera(CameraMode mode);

// Smooth panning
public void PanToCell(HexCell cell, float duration);
public void PanToPosition(Vector3 position, float duration);

// Zoom control
public void ZoomToLevel(float level, float duration);
public void ZoomIn(float duration = 0.5f);
public void ZoomOut(float duration = 0.5f);
public void ResetZoom(float duration = 0.5f);

// Rotation
public void RotateCamera(float angle, float duration);
```

#### Integration with FSM

```csharp
// When cell enters Focused state
hexCell.InteractionState.OnEnterFocused += () => {
    CameraController.Instance.ChangeCamera(CameraMode.Focus);
    CameraController.Instance.PanToCell(hexCell, 1.0f);
    CameraController.Instance.ZoomToLevel(20f, 1.0f);
};

// When cell exits Focused state
hexCell.InteractionState.OnExitFocused += () => {
    CameraController.Instance.ChangeCamera(CameraMode.TopDown);
    CameraController.Instance.ResetZoom(1.0f);
};
```

#### Cinemachine Integration

Uses Unity's **Cinemachine** package for:
- Smooth camera transitions
- Automatic blending between cameras
- Professional camera behaviors
- Damping and easing

**File Reference:** `Assets/Scripts/CameraController.cs:1`

---

### 3.6 Input System

**Purpose:** Handles mouse input and raycasting to the game world.

**Key Files:**
- `MouseController.cs` - Mouse input handler (44 LOC)

#### Features

```csharp
public class MouseController : Singleton<MouseController>
{
    // Mouse button events
    public Action<RaycastHit> OnLeftMouseClick;
    public Action<RaycastHit> OnRightMouseClick;
    public Action<RaycastHit> OnMiddleMouseClick;

    // Raycasts from screen to world
    // Detects game objects under cursor
    // Fires appropriate event based on button
}
```

#### Usage Pattern

```csharp
void Start()
{
    // Subscribe to click events
    MouseController.Instance.OnLeftMouseClick += HandleLeftClick;
}

void HandleLeftClick(RaycastHit hit)
{
    // Check if we hit a hex cell
    var hexCell = hit.collider.GetComponent<HexCell>();
    if (hexCell != null)
    {
        // Create command for state change
        var command = new StateChangeCommand(
            hexCell,
            InputEvent.MouseDown
        );

        // Execute through history (enables undo)
        CommandHistory.Instance.ExecuteCommand(command);
    }
}
```

#### Integration with Unity Input System

- Uses Unity's new Input System (1.6.3)
- Mouse position tracked via `Mouse.current.position`
- Camera raycast from screen to world space
- Layer-based filtering available

**File Reference:** `Assets/Scripts/MouseController.cs:1`

---

### 3.7 Map Generation System

**Purpose:** Procedural terrain generation using Perlin noise with biome mapping.

**Key Files:**
- `MapGenerator.cs` - Procedural generation (200+ LOC)
- `Noise.cs` - Perlin noise implementation (100+ LOC)
- `TextureGenerator.cs` - Color map generation (50+ LOC)
- `TerrainType.cs` - Terrain definitions (ScriptableObject)

#### Generation Parameters

```csharp
// Noise settings
public float NoiseScale = 0.5f;        // Zoom level of noise
public int Octaves = 6;                // Detail layers
public float Persistance = 0.5f;       // Amplitude decrease per octave
public float Lacunarity = 2f;          // Frequency increase per octave
public int Seed = 0;                   // Random seed
public Vector2 Offset = Vector2.zero;  // Map offset

// Biome mapping
public List<TerrainHeight> Biomes;     // Height → Terrain type
```

#### Terrain Height Definition

```csharp
[System.Serializable]
public class TerrainHeight
{
    public string name;           // Biome name
    public float height;          // Height threshold (0-1)
    public TerrainType terrain;   // Associated terrain type
    public Color color;           // Visualization color
}
```

#### Generation Pipeline

```csharp
// 1. Generate noise map (Perlin noise)
public float[,] GenerateNoiseMap(int width, int height);

// 2. Assign terrain types based on height
public TerrainType[,] AssignTerrainTypes(float[,] noiseMap);

// 3. Generate color map for visualization
public Color[] GenerateColorsFromTerrain(TerrainType[,] terrainMap);

// 4. Apply to hex grid
public void ApplyToHexGrid(HexGrid grid, TerrainType[,] terrainMap);
```

#### Perlin Noise Features

**Multi-Octave Noise:**
- Base frequency + harmonics (octaves)
- Each octave adds detail at smaller scale
- Persistance controls amplitude falloff
- Lacunarity controls frequency increase

**Normalization:**
- Output normalized to 0-1 range
- Supports both local and global normalization modes

**Seed-Based Randomization:**
- Deterministic generation from seed
- Same seed produces same map
- Offset allows variation with same seed

#### Threading Support

```csharp
public bool UseThreadedGeneration = true;  // Enable async generation
public bool AutoUpdate = true;              // Regenerate on param change
```

**Threaded Generation Flow:**
```csharp
Task.Run(() =>
{
    // Generate on background thread (CPU-intensive)
    float[,] noiseMap = Noise.GenerateNoiseMap(...);
    TerrainType[,] terrainMap = AssignTerrainTypes(noiseMap);

    // Switch back to main thread for Unity API calls
    MainThreadDispatcher.Instance.Enqueue(() =>
    {
        ApplyToHexGrid(grid, terrainMap);
        OnGenerationComplete?.Invoke();
    });
});
```

#### Events

```csharp
public event Action<float[,]> OnNoiseMapGenerated;
public event Action<TerrainType[,]> OnTerrainMapGenerated;
public event Action<Color[], int, int> OnColorMapGenerated;
public event Action OnGenerationComplete;
```

#### Terrain Types (ScriptableObjects)

Available terrain types:
- **Ocean** - Deep water
- **Beach** - Shallow water
- **Sand** - Desert/beach
- **Grass** - Plains
- **Forest** - Woodland
- **Mountains** - High elevation
- **Ice** - Frozen terrain
- **Custom** - User-defined types

**TerrainType Properties:**
```csharp
public class TerrainType : ScriptableObject
{
    public string terrainName;
    public Color color;
    public Material material;
    public bool isWalkable;
    public int movementCost;
    public float defenseBonus;
}
```

**File References:**
- Map Generator: `Assets/Scripts/Grid/MapGenerator.cs:1`
- Noise Generator: `Assets/Scripts/Noise.cs:1`
- Terrain Types: `Assets/Scripts/Grid/TerrainType.cs:1`

---

## 4. Architectural Patterns

### 4.1 Design Patterns Used

| Pattern | Implementation | Purpose | Files |
|---------|----------------|---------|-------|
| **Singleton** | `Singleton<T>` base class | Global access points | `CameraController`, `MouseController`, `CommandHistory`, `TransitionGuardRegistry` |
| **State Machine** | FSM with events | State-based behavior | `HexCellStateManager`, `HexCellInteractionState` |
| **Strategy** | `ITransitionGuard` | Interchangeable algorithms | All guard implementations |
| **Command** | `ICommand` interface | Encapsulate actions | All command implementations |
| **Composite** | Hierarchical structures | Treat objects uniformly | `MacroCommand`, `AndGuard`, `OrGuard` |
| **Observer** | C# events | Decouple components | FSM events, Command events |
| **Builder** | Fluent API | Construct complex objects | `GuardBuilder`, Command chaining |
| **Template Method** | Abstract base classes | Define algorithm skeleton | `GuardBase`, `CommandBase` |
| **Facade** | Simplified interfaces | Hide complexity | `HexGrid`, `CameraController` |

### 4.2 Architectural Principles

#### Separation of Concerns
- **Data** (HexCellInteractionState) separated from **Logic** (HexCellStateManager)
- **Validation** (Guards) separated from **State transitions** (FSM)
- **Actions** (Commands) separated from **History management** (CommandHistory)

#### Open/Closed Principle
- Open for extension via interfaces (`ICommand`, `ITransitionGuard`)
- Closed for modification (core systems don't change when adding guards/commands)

#### Single Responsibility
- `HexCellStateManager` - Only manages transitions
- `TransitionGuardRegistry` - Only manages guards
- `CommandHistory` - Only manages history
- `HexCell` - Only represents cell data/behavior

#### Dependency Inversion
- High-level modules depend on abstractions (`ICommand`, `ITransitionGuard`)
- Low-level modules implement abstractions
- Easy to swap implementations without changing client code

#### Event-Driven Architecture
- Loose coupling through events
- Components subscribe to events they care about
- Easy to add new behaviors without modifying existing code

### 4.3 Data Flow Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      INPUT LAYER                            │
│  MouseController, Keyboard, UI Events                       │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                   COMMAND LAYER                             │
│  StateChangeCommand, MoveUnitCommand, MacroCommand          │
│  - Encapsulates actions                                     │
│  - Supports undo/redo                                       │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                 VALIDATION LAYER                            │
│  TransitionGuardRegistry                                    │
│  - Global guards                                            │
│  - State-specific guards                                    │
│  - Transition-specific guards                               │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                  STATE MACHINE LAYER                        │
│  HexCellStateManager, HexCellInteractionState               │
│  - Evaluates transitions                                    │
│  - Fires events                                             │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                   PRESENTATION LAYER                        │
│  HexCell visual effects, Camera, UI updates                 │
│  - Subscribes to state events                               │
│  - Updates visuals                                          │
└─────────────────────────────────────────────────────────────┘
```

### 4.4 Dependency Graph

```
Core Infrastructure:
  Singleton.cs
      ↓
  CameraController ──────┐
  MouseController        │
  CommandHistory         ├─→ Game Systems Layer
  TransitionGuardRegistry│
      ↓                  │
  HexGrid ←──────────────┘
      ↓
  HexCell (integrates all systems)
      ├─→ HexCellInteractionState (FSM data)
      ├─→ HexCellStateManager (FSM logic)
      ├─→ StateChangeCommand (actions)
      └─→ Guards (validation)
```

---

## 5. Key Features

### 5.1 Hierarchical State System

**Feature:** Integer-based state hierarchy (0-4) enables level-based queries.

**Benefits:**
```csharp
// Check if cell is at least visible (Visible or higher)
public bool IsExplored() => CurrentState >= CellState.Visible;

// Check if cell is interactive (Highlighted or higher)
public bool IsInteractive() => CurrentState >= CellState.Highlighted;

// Check if cell is selected (Selected or higher)
public bool IsSelected() => CurrentState >= CellState.Selected;
```

**Implementation:**
- Sequential progression enforced by `StateHierarchyGuard`
- Downward transitions always allowed (deactivation)
- Upward transitions validated (activation)

### 5.2 Composite Guards

**Feature:** Combine multiple guards using logical operators.

**Example - Complex Selection Validation:**
```csharp
var selectionGuard = new AndGuard("SelectionValidation")
    .AddGuard(new IsExploredGuard())        // Must be explored
    .AddGuard(GuardBuilder.Any(             // AND (owner OR ally)
        new PlayerPermissionGuard("Owner", ...),
        new PlayerPermissionGuard("Ally", ...)
    ))
    .AddGuard(GuardBuilder.Not(             // AND NOT cooldown
        new CooldownGuard(0.5f)
    ));

TransitionGuardRegistry.Instance.AddToStateGuard(
    CellState.Selected,
    selectionGuard
);
```

**Benefits:**
- Expressive validation logic
- Reusable guard components
- Short-circuit evaluation for performance
- Clear failure reasons

### 5.3 Atomic Transactions (MacroCommand)

**Feature:** Multi-step operations with all-or-nothing semantics.

**Example - Unit Movement with Vision:**
```csharp
var moveTransaction = new MacroCommand("Move unit and reveal");

// Step 1: Deselect old location
moveTransaction.AddCommand(
    new StateChangeCommand(oldCell, InputEvent.Deselect)
);

// Step 2: Move unit (if fails, entire transaction rolls back)
moveTransaction.AddCommand(
    new MoveUnitCommand(unit, oldCell, newCell)
);

// Step 3: Select new location
moveTransaction.AddCommand(
    new StateChangeCommand(newCell, CellState.Selected)
);

// Step 4: Reveal fog of war
var visibleCells = GetCellsInRadius(newCell, unit.VisionRange);
moveTransaction.AddCommand(
    new RevealFogCommand(visibleCells)
);

// Execute atomically
bool success = CommandHistory.Instance.ExecuteCommand(moveTransaction);
// If ANY step fails, ALL steps are automatically rolled back

// Undo reverses ALL 4 steps in reverse order
CommandHistory.Instance.Undo();
```

**Benefits:**
- Guaranteed consistency
- Automatic rollback on failure
- Single undo/redo for multi-step operations
- Prevents partial state changes

### 5.4 Threaded Map Generation

**Feature:** CPU-intensive generation on background thread.

**Flow:**
```csharp
Task.Run(() =>
{
    // Heavy computation OFF main thread (no frame drops)
    float[,] noiseMap = Noise.GenerateNoiseMap(...);
    TerrainType[,] terrainMap = AssignTerrainTypes(noiseMap);
    Color[] colorMap = GenerateColorsFromTerrain(terrainMap);

    // Switch to main thread for Unity API calls
    MainThreadDispatcher.Instance.Enqueue(() =>
    {
        ApplyToHexGrid(grid, terrainMap);
        OnGenerationComplete?.Invoke();
    });
});
```

**Benefits:**
- No frame stutters during generation
- Responsive UI during loading
- Progress events for loading screens
- Safe Unity API usage via MainThreadDispatcher

### 5.5 Coordinate System Flexibility

**Feature:** Three coordinate systems with bidirectional conversion.

**Supported Systems:**
1. **Offset** - Human-readable (column, row)
2. **Axial** - Simplified cube (q, r)
3. **Cube** - Canonical hex (q, r, s where q+r+s=0)

**Benefits:**
- Offset for storage/display
- Axial for calculations
- Cube for distance/neighbors
- Seamless conversion between all three

**Usage:**
```csharp
// Storage in offset coordinates
Vector2 offset = new Vector2(5, 3);

// Convert to cube for distance calculation
Vector3 cube1 = HexMetrics.OffsetToCube(5, 3, orientation);
Vector3 cube2 = HexMetrics.OffsetToCube(8, 7, orientation);
int distance = HexMetrics.CubeDistance(cube1, cube2);

// Convert back to offset for display
Vector2 result = HexMetrics.CubeToOffset(cube1, orientation);
```

### 5.6 Event-Driven FSM

**Feature:** Granular events for each state and transition.

**Event Types:**
```csharp
// General transition event
OnStateChanged(fromState, toState)

// Granular per-state events
OnEnterVisible()
OnExitVisible()
OnEnterHighlighted()
// ... for all 5 states

// Blocked transition event
OnTransitionBlocked(from, to, reason)
```

**Benefits:**
- Loose coupling between systems
- Easy to add new behaviors
- Visual effects react to state changes
- UI updates automatically
- Audio triggers
- Analytics tracking

**Example - Multiple Systems React:**
```csharp
// Visual system
cell.OnEnterSelected += () => cell.SetSelectionEffect(true);

// Audio system
cell.OnEnterSelected += () => AudioManager.Play("select");

// Analytics system
cell.OnEnterSelected += () => Analytics.Track("CellSelected");

// UI system
cell.OnEnterSelected += () => UI.ShowCellInfo(cell);
```

### 5.7 Guard Registry Flexibility

**Feature:** Four levels of guard registration for fine-grained control.

**Levels:**
1. **Global** - All transitions must pass
2. **To-State** - All transitions TO a state must pass
3. **From-State** - All transitions FROM a state must pass
4. **Transition-Specific** - Only specific (from, to) pairs

**Example - Layered Validation:**
```csharp
var registry = TransitionGuardRegistry.Instance;

// Global: Must be explored for ANY interaction
registry.AddGlobalGuard(new IsExploredGuard());

// To-State: Entering Selected requires permissions
registry.AddToStateGuard(
    CellState.Selected,
    new PlayerPermissionGuard(...)
);

// From-State: Leaving Focused requires camera release
registry.AddFromStateGuard(
    CellState.Focused,
    new CameraAvailableGuard()
);

// Transition-Specific: Visible→Focused has special requirements
registry.AddTransitionGuard(
    CellState.Visible,
    CellState.Focused,
    new SpecialFocusGuard()
);
```

**Benefits:**
- Fine-grained control
- Reusable guard logic
- Clear validation hierarchy
- Easy to add/remove rules

---

## 6. Configuration & Setup

### 6.1 Unity Configuration

**Unity Version:** 2022.x or later (inferred from package versions)

**Render Pipeline:** Universal Render Pipeline (URP) 14.0.8

**Project Settings:**
- **Color Space:** Linear (modern rendering)
- **API Compatibility:** .NET Standard 2.1
- **Scripting Backend:** Mono (IL2CPP for builds)
- **Physics:** 3D and 2D systems enabled

### 6.2 Package Dependencies

```json
{
  "dependencies": {
    "com.unity.cinemachine": "2.9.7",
    "com.unity.inputsystem": "1.6.3",
    "com.unity.render-pipelines.universal": "14.0.8",
    "com.unity.test-framework": "1.1.33",
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.timeline": "1.7.4",
    "com.unity.visualscripting": "1.8.0",
    "com.unity.feature.worldbuilding": "1.0.1"
  }
}
```

**Key Packages:**
- **Cinemachine** - Professional camera system
- **Input System** - Modern input handling
- **URP** - Optimized rendering pipeline
- **Test Framework** - Unit testing (NUnit)
- **TextMeshPro** - High-quality text rendering

### 6.3 Scene Setup

**Available Scenes:**

| Scene | Purpose | Location |
|-------|---------|----------|
| SampleScene | Main gameplay | `Assets/Scenes/SampleScene.unity` |
| CameraTest Scene | Camera testing | `Assets/Scenes/CameraTest Scene.unity` |
| Test Scene | General testing | `Assets/Scenes/Test Scene.unity` |
| Noise Map Tester | Map generation | `Assets/Scenes/Noise Map Tester.unity` |

**Main Scene Components:**
- HexGrid GameObject (with HexGrid.cs)
- Main Camera (with CinemachineBrain)
- Virtual Cameras (TopDown, Focus)
- Event System (for UI)
- Lighting
- Post-Processing Volume

### 6.4 Prefab Configuration

**Core Prefabs:**
- `Hex Base.prefab` - Base hexagon template
- Terrain variants (Grass, Forest, Mountains, Ocean, etc.)
- UI prefabs
- Effect prefabs (particles, highlights)

**Prefab Structure:**
```
Hex Base
├── Mesh (MeshFilter + MeshRenderer)
├── Collider (MeshCollider for raycasting)
├── HexCell (script component)
│   ├── HexCellInteractionState
│   └── HexCellStateManager
└── Visual Effects (children)
    ├── Highlight Effect
    ├── Selection Effect
    └── Focus Effect
```

### 6.5 Material Configuration

**Available Materials:**
- Grass, Grass Dark
- Water, Ocean
- Sand, Beach
- Dirt
- Ice
- Rocks, Mountains
- Noise materials

**Material Properties:**
- URP/Lit shader
- Albedo color/texture
- Normal maps
- Smoothness/Metallic
- Emission (for effects)

### 6.6 ScriptableObject Configuration

**Terrain Types:**
Location: `Assets/ScriptableObjects/TerrainTypes/`

Create new terrain types:
```csharp
// In Unity Editor:
// Right-click → Create → Terrain Type
// Configure properties:
// - Name
// - Color
// - Material
// - Walkable flag
// - Movement cost
// - Defense bonus
```

### 6.7 Build Configuration

**Build Settings:**
- **Platform:** Standalone Windows (primary)
- **Architecture:** x64
- **Scripting Backend:** IL2CPP (optimized builds)
- **Burst Compilation:** Enabled for performance

**Build Artifacts:**
- BurstAotSettings_StandaloneWindows.json
- CommonBurstAotSettings.json
- Executable + Data folder

**Optimization Settings:**
- Managed Stripping Level: Medium
- Enable exceptions: Explicitly Only
- Script compilation: Optimize for performance

---

## 7. Testing Framework

### 7.1 Unit Test Configuration

**Framework:** NUnit (Unity Test Framework 1.1.33)

**Test Location:** `Assets/Tests/`

**Test Assembly Definition:**
```json
{
  "name": "FantasyFiefdoms.Tests",
  "references": [
    "UnityEngine.TestRunner",
    "UnityEditor.TestRunner",
    "FantasyFiefdoms.Runtime"
  ],
  "includePlatforms": [
    "Editor"
  ]
}
```

### 7.2 Test Coverage

**Current Tests:**
- **HexMetricsTest.cs** (476 LOC, 40+ test methods)

**Coverage Areas:**

| System | Tests | Coverage |
|--------|-------|----------|
| Coordinate Conversions | 40+ tests | ✅ Comprehensive |
| Axial ↔ Cube | 8 tests | ✅ Full |
| Offset ↔ Cube | 20+ tests | ✅ Full |
| World ↔ Offset | 10+ tests | ✅ Full |
| FlatTop Orientation | 20+ tests | ✅ Full |
| PointyTop Orientation | 20+ tests | ✅ Full |

**Example Test:**
```csharp
[Test]
public void OffsetToCubeConversion_FlatTop_Test1()
{
    int col = 2;
    int row = 1;
    HexOrientation orientation = HexOrientation.FlatTop;
    Vector3 expected = new Vector3(2, -1, -1);
    Vector3 actual = HexMetrics.OffsetToCube(col, row, orientation);
    Assert.AreEqual(expected, actual);
}
```

### 7.3 Running Tests

**Unity Test Runner:**
1. Window → General → Test Runner
2. PlayMode or EditMode tabs
3. Run All or Run Selected

**Command Line:**
```bash
# Run all tests
unity -runTests -batchmode -projectPath . -testResults results.xml

# Run specific test
unity -runTests -batchmode -projectPath . -testPlatform EditMode -editorTestsFilter HexMetricsTest
```

### 7.4 Test Best Practices

**Current Practices:**
- Deterministic tests (no randomness)
- Independent tests (no shared state)
- Clear naming: `MethodName_Scenario_ExpectedResult`
- Arranged-Act-Assert pattern
- Comprehensive edge case coverage

**Example:**
```csharp
[Test]
public void CubeDistance_AdjacentCells_ReturnsOne()
{
    // Arrange
    Vector3 cell1 = new Vector3(0, 0, 0);
    Vector3 cell2 = new Vector3(1, -1, 0);

    // Act
    int distance = HexMetrics.CubeDistance(cell1, cell2);

    // Assert
    Assert.AreEqual(1, distance);
}
```

---

## 8. Development Workflow

### 8.1 Adding a New Feature

**1. Create Guards (if validation needed):**
```csharp
// Assets/Scripts/Guards/MyCustomGuard.cs
public class MyCustomGuard : GuardBase
{
    public override GuardResult CanTransition(GuardContext context)
    {
        // Implement validation logic
        if (/* condition */)
            return GuardResult.Success();
        else
            return GuardResult.Failure("Reason");
    }
}
```

**2. Create Commands (if undoable action):**
```csharp
// Assets/Scripts/Commands/MyFeatureCommand.cs
public class MyFeatureCommand : CommandBase
{
    public override bool Execute()
    {
        // Perform action
        return true;
    }

    public override bool Undo()
    {
        // Reverse action
        return true;
    }
}
```

**3. Register Guards:**
```csharp
// In initialization code
TransitionGuardRegistry.Instance.AddGlobalGuard(
    new MyCustomGuard()
);
```

**4. Wire Up Input:**
```csharp
// In input handler
MouseController.Instance.OnLeftMouseClick += (hit) =>
{
    var command = new MyFeatureCommand(...);
    CommandHistory.Instance.ExecuteCommand(command);
};
```

**5. Add Tests:**
```csharp
// Assets/Tests/MyFeatureTests.cs
[Test]
public void MyFeature_Scenario_ExpectedResult()
{
    // Arrange, Act, Assert
}
```

### 8.2 Modifying State Behavior

**Adding a New State:**

1. **Update CellState enum:**
```csharp
public enum CellState
{
    Invisible = 0,
    Visible = 1,
    Highlighted = 2,
    Selected = 3,
    Focused = 4,
    MyNewState = 5  // Add here
}
```

2. **Add events to HexCellInteractionState:**
```csharp
public event Action OnEnterMyNewState;
public event Action OnExitMyNewState;

// Update SetState method to fire events
```

3. **Add transition rules to HexCellStateManager:**
```csharp
// Add allowed transitions
```

4. **Add guards if needed:**
```csharp
TransitionGuardRegistry.Instance.AddToStateGuard(
    CellState.MyNewState,
    new MyNewStateGuard()
);
```

5. **Wire up visual effects:**
```csharp
// In HexCell.cs
InteractionState.OnEnterMyNewState += () => {
    // Visual feedback
};
```

### 8.3 Adding New Terrain Types

**1. Create ScriptableObject:**
```
Unity Editor → Right-click → Create → Terrain Type
```

**2. Configure Properties:**
- Name
- Color
- Material reference
- Walkable flag
- Movement cost
- Defense bonus
- Other gameplay properties

**3. Create Prefab (if needed):**
- Duplicate existing terrain prefab
- Apply new material
- Adjust visual effects

**4. Add to Biome List:**
```csharp
// In MapGenerator
public List<TerrainHeight> Biomes = new List<TerrainHeight>
{
    new TerrainHeight {
        name = "MyTerrain",
        height = 0.7f,
        terrain = myTerrainType,
        color = Color.green
    }
};
```

### 8.4 Git Workflow

**Branching Strategy:**
- `main` - Stable releases
- `claude/*` - Feature branches (auto-created)
- `develop` - Integration branch (if used)

**Current Branch:**
```
claude/document-repository-systems-011CUW9uPU41uCCV6VUZPNjw
```

**Commit Convention:**
```
<type>: <subject>

<body>

🤖 Generated with Claude Code
Co-Authored-By: Claude <noreply@anthropic.com>
```

**Types:** feat, fix, docs, refactor, test, chore

### 8.5 Code Style Guidelines

**Naming Conventions:**
- Classes: PascalCase (`HexCell`)
- Methods: PascalCase (`GetNeighbor()`)
- Properties: PascalCase (`CurrentState`)
- Fields (private): camelCase (`_hexGrid`)
- Fields (public): PascalCase (`HexSize`)
- Events: PascalCase with On prefix (`OnStateChanged`)

**Organization:**
```csharp
public class MyClass
{
    // Fields (private)
    private int _myField;

    // Properties
    public int MyProperty { get; set; }

    // Events
    public event Action OnMyEvent;

    // Unity methods (Awake, Start, Update)
    void Start() { }

    // Public methods
    public void MyMethod() { }

    // Private methods
    private void MyPrivateMethod() { }
}
```

**Comments:**
- XML documentation for public APIs
- Inline comments for complex logic
- TODO comments for future work

---

## 9. File Reference

### 9.1 Core Scripts

| File | LOC | Purpose | Key Classes |
|------|-----|---------|-------------|
| `HexGrid.cs` | 100+ | Grid management | HexGrid |
| `HexCell.cs` | 200+ | Cell data/behavior | HexCell |
| `HexMetrics.cs` | 200+ | Math utilities | HexMetrics (static) |
| `HexCellStateManager.cs` | 302 | FSM logic | HexCellStateManager |
| `HexCellInteractionState.cs` | 115 | FSM data | HexCellInteractionState |
| `CommandHistory.cs` | 150+ | Command management | CommandHistory |
| `StateChangeCommand.cs` | 159 | FSM commands | StateChangeCommand |
| `MacroCommand.cs` | 156 | Composite commands | MacroCommand |
| `TransitionGuardRegistry.cs` | 250+ | Guard management | TransitionGuardRegistry |
| `CommonGuards.cs` | 300+ | Guard implementations | IsExploredGuard, etc. |
| `CompositeGuards.cs` | 200+ | Guard combinators | AndGuard, OrGuard, NotGuard |
| `CameraController.cs` | 150+ | Camera management | CameraController |
| `MouseController.cs` | 44 | Input handling | MouseController |
| `MapGenerator.cs` | 200+ | Procedural generation | MapGenerator |
| `Noise.cs` | 100+ | Perlin noise | Noise (static) |

### 9.2 Directory Quick Reference

```
Assets/Scripts/
├── Commands/           # Undo/redo system (5 files, ~500 LOC)
├── Grid/              # Hex grid system (9 files, ~800 LOC)
├── Guards/            # Validation system (4 files, ~500 LOC)
├── UI/                # User interface (4 files, ~200 LOC)
├── Video Specific/    # Visual effects (1 file, ~40 LOC)
└── [Root]             # Core systems (8 files, ~400 LOC)
```

### 9.3 Key Interfaces

| Interface | File | Purpose |
|-----------|------|---------|
| `ICommand` | `Commands/ICommand.cs` | Command pattern |
| `ITransitionGuard` | `Guards/ITransitionGuard.cs` | Guard pattern |

### 9.4 Key Enums

| Enum | File | Values |
|------|------|--------|
| `CellState` | `Grid/HexCellInteractionState.cs` | Invisible, Visible, Highlighted, Selected, Focused |
| `InputEvent` | `Grid/HexCellStateManager.cs` | MouseEnter, MouseExit, MouseDown, MouseUp, FKeyDown, Deselect, RevealFog |
| `CameraMode` | `CameraController.cs` | TopDown, Focus |
| `HexOrientation` | `Grid/HexMetrics.cs` | FlatTop, PointyTop |

### 9.5 Singleton Classes

| Class | Purpose | Access Pattern |
|-------|---------|----------------|
| `CameraController` | Camera management | `CameraController.Instance` |
| `MouseController` | Input handling | `MouseController.Instance` |
| `CommandHistory` | Command history | `CommandHistory.Instance` |
| `TransitionGuardRegistry` | Guard registry | `TransitionGuardRegistry.Instance` |
| `MainThreadDispatcher` | Thread safety | `MainThreadDispatcher.Instance` |

---

## 10. Future Enhancements

### 10.1 Not Yet Implemented

**From Documentation (FSM_FEATURE_ANALYSIS.md):**

1. **State Data/Context**
   - Store unit selections
   - Pending actions
   - Associated data per state

2. **Animation Coordination**
   - Smooth visual transitions
   - Animation state machine integration
   - Timeline integration

3. **Multi-Selection**
   - Shift+click for multiple cells
   - Group operations
   - Multi-unit commands

4. **State Persistence**
   - Save/load game state
   - Serialization system
   - State replay

5. **Debug Visualization**
   - Visual FSM inspector
   - State transition graph
   - Guard evaluation debugger

6. **Event Queuing**
   - Queue rapid inputs
   - Smooth input handling
   - Input buffering

7. **Metrics/Analytics**
   - Gameplay statistics
   - State duration tracking
   - Performance monitoring

8. **Configuration System**
   - ScriptableObjects for guards
   - ScriptableObjects for commands
   - Runtime guard configuration

### 10.2 Potential Features

**Gameplay Systems:**
- Unit system (movement, combat)
- Resource management
- Building/construction system
- Technology tree
- Diplomacy system
- Turn management
- Multiplayer support

**Technical Improvements:**
- Object pooling for cells
- LOD system for distant cells
- Async/await pattern throughout
- Save/load system
- Mod support
- Localization
- Accessibility features

**Visual Polish:**
- Particle effects
- Animation system
- Weather effects
- Day/night cycle
- Minimap
- UI polish

**AI Systems:**
- Pathfinding (A* for hex grid)
- AI players
- Unit AI behaviors
- Strategic AI

### 10.3 Performance Optimizations

**Potential Optimizations:**
- Guard result caching
- Neighbor lookup caching
- Mesh batching
- GPU instancing
- Occlusion culling
- Spatial partitioning

### 10.4 Code Quality

**Improvements:**
- Expand test coverage (FSM, Guards, Commands)
- Integration tests
- Performance benchmarks
- Code documentation (XML comments)
- API documentation generation
- Architecture decision records (ADRs)

---

## Appendix A: Quick Start Guide

### Getting Started (5 Minutes)

**1. Open Project:**
```bash
# Clone repository
git clone <repository-url>
cd Fantasy-Fiefdoms

# Open in Unity
# File → Open Project → Select folder
```

**2. Open Main Scene:**
```
Assets/Scenes/SampleScene.unity
```

**3. Play Scene:**
- Press Play button in Unity Editor
- Click on hex cells to interact
- Use F key to focus camera
- Mouse wheel to zoom

**4. Explore Code:**
- Start with `HexCell.cs` (core entity)
- Then `HexCellStateManager.cs` (FSM)
- Then `CommandHistory.cs` (undo/redo)

**5. Read Documentation:**
- `FSM_FEATURE_ANALYSIS.md` - FSM deep dive
- `COMMAND_PATTERN_GUIDE.md` - Undo/redo guide
- `TRANSITION_GUARDS_GUIDE.md` - Validation guide

---

## Appendix B: Common Tasks

### Task: Add Undo Button to UI

```csharp
public class UndoButton : MonoBehaviour
{
    [SerializeField] private Button undoButton;

    void Start()
    {
        undoButton.onClick.AddListener(OnUndoClick);
        CommandHistory.Instance.OnHistoryChanged += UpdateButton;
        UpdateButton();
    }

    void OnUndoClick()
    {
        CommandHistory.Instance.Undo();
    }

    void UpdateButton()
    {
        undoButton.interactable = CommandHistory.Instance.CanUndo;
        undoButton.GetComponentInChildren<Text>().text =
            CommandHistory.Instance.GetUndoDescription();
    }
}
```

### Task: Create Custom Guard

```csharp
public class CustomValidationGuard : GuardBase
{
    public CustomValidationGuard() : base("CustomValidation") { }

    public override GuardResult CanTransition(GuardContext context)
    {
        // Your validation logic
        if (/* valid */)
            return GuardResult.Success();
        else
            return GuardResult.Failure("Validation failed because...");
    }
}

// Register globally
TransitionGuardRegistry.Instance.AddGlobalGuard(
    new CustomValidationGuard()
);
```

### Task: Create Custom Command

```csharp
public class CustomCommand : CommandBase
{
    private object _previousState;

    public override string Description => "Custom action";

    public override bool CanExecute()
    {
        // Pre-execution check
        return true;
    }

    public override bool Execute()
    {
        if (!CanExecute()) return false;

        // Save state for undo
        _previousState = CaptureState();

        // Perform action
        DoAction();

        return true;
    }

    public override bool Undo()
    {
        // Restore previous state
        RestoreState(_previousState);
        return true;
    }

    public override bool Redo()
    {
        // Re-execute
        return Execute();
    }
}
```

---

## Appendix C: Troubleshooting

### Common Issues

**Issue: State transitions not working**
- Check guards are not blocking transition
- Subscribe to `OnTransitionBlocked` to see reason
- Verify TransitionGuardRegistry is initialized

**Issue: Undo not working**
- Ensure commands are executed through CommandHistory
- Verify `CanUndo()` returns true
- Check command implements `Undo()` correctly

**Issue: Camera not focusing**
- Verify Cinemachine Brain on main camera
- Check virtual cameras are configured
- Ensure CameraController is initialized

**Issue: Cells not generating**
- Check HexGrid component is attached
- Verify prefab references are set
- Check for errors in console
- Ensure threaded generation completed

### Debug Tools

**Enable Debug Logging:**
```csharp
// In HexCellStateManager
Debug.Log($"Transition: {fromState} → {toState}");

// In TransitionGuardRegistry
Debug.Log($"Guard '{guard.Name}' result: {result.Success} - {result.Reason}");

// In CommandHistory
Debug.Log($"Executed command: {command.Description}");
```

**Visual Debug:**
- Use Scene view to inspect cell states
- Enable Gizmos for visual debugging
- Use Unity Profiler for performance

---

## Conclusion

**Fantasy-Fiefdoms** is a well-architected Unity game demonstrating professional software engineering practices. The codebase features:

✅ **Clean Architecture** - Clear separation of concerns
✅ **Design Patterns** - 9+ patterns used correctly
✅ **Comprehensive FSM** - Hierarchical states with events
✅ **Flexible Validation** - Composable guard system
✅ **Undo/Redo** - Full command pattern implementation
✅ **Performance** - Threaded generation, optimizations
✅ **Extensibility** - Easy to add features
✅ **Documentation** - 1,500+ lines of guides
✅ **Testing** - Unit tests for core systems

**Total:** ~4,235 lines of well-organized C# code ready for production.

---

**Document Version:** 1.0
**Last Updated:** October 26, 2025
**Repository:** SoulsGameDev/Fantasy-Fiefdoms
**Branch:** `claude/document-repository-systems-011CUW9uPU41uCCV6VUZPNjw`
