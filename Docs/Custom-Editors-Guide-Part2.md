# Custom Editors Guide - Part 2

This document covers the medium-priority custom editors added to Fantasy-Fiefdoms.

## 4. TerrainType Enhanced Inspector

**Location:** `Assets/Editor/TerrainTypeEditor.cs`

### Overview
Enhanced inspector for TerrainType ScriptableObjects with visual previews, movement cost management, and a terrain library browser.

### New TerrainType Fields Added
Updated `TerrainType.cs` to include:
- **terrainName** (string) - Used for pathfinding cost multiplier lookups
- **movementCost** (int) - Movement cost for pathfinding (default: 1)
- **isWalkable** (bool) - Whether terrain can be traversed (default: true)

### Features

#### Basic Properties Section
- **Terrain Name** - String identifier for pathfinding
- **Display Name** - Human-readable name
- **Description** - Detailed description

#### Visual Properties Section
- **Color Preview** - Large color swatch showing terrain color
- **Prefab Assignment** - 3D model for hex cells
  - Visual feedback if prefab is/isn't assigned
- **Icon Display** - Shows icon sprite if assigned (64x64 preview)

#### Pathfinding Properties Section
- **Walkability Toggle** - Mark terrain as passable/impassable
- **Movement Cost Slider** - Configure movement cost (1-10)
- **Visual Cost Bar** - Color-coded bar (green=fast, red=slow)
- **Cost Descriptions** - Automatic recommendations:
  - 1: "Very Fast - roads, plains"
  - 2: "Normal - grassland, light forest"
  - 3: "Slow - forests, hills"
  - 5: "Very Slow - mountains, swamps"
  - 10: "Nearly Impassable"

**Quick Preset Buttons:**
- Fast (1) - Roads, plains
- Normal (2) - Standard terrain
- Slow (3) - Forests, hills
- Very Slow (5) - Mountains, swamps
- Impassable (10) - Automatically sets isWalkable = false

#### Preview Section
- **3D Prefab Preview** - Visual preview of assigned prefab (256x256)
- **Color Preview Banner** - Large color strip with terrain name overlay

#### Usage Information Section
Shows terrain usage statistics during Play mode:
- Number of cells using this terrain type
- Percentage of map coverage
- Pathfinding impact calculations
- Estimated cells per turn (based on 5 movement points)

#### Quick Actions
- **Duplicate** - Create copy of terrain type
- **Find in Scene** - Select all cells using this terrain (Play mode)
- **Show All Terrain Types** - Opens library window

### Terrain Type Library Window

**Access:** `Window > Terrain Type Library`

**Features:**
- Visual card browser for all terrain types
- Each card shows:
  - Color indicator bar
  - Icon preview (if assigned)
  - Terrain name
  - Movement cost
  - Walkability status
  - Description
- **Select Button** - Quick access to any terrain type
- **Refresh Button** - Reload terrain types from project
- Sorted by movement cost

### How to Use

#### Creating Terrain Types
1. Right-click in Project window
2. `Create > TBS > TerrainType`
3. Configure in enhanced inspector

#### Configuring Movement Cost
1. Select terrain type
2. Use quick preset buttons or manual slider
3. View visual feedback bar
4. Read recommendations

#### Finding Usage
1. Enter Play mode
2. Select terrain type
3. View "Usage Information" section
4. Click "Find in Scene" to select all cells

### Best Practices

**Movement Cost Guidelines:**
- **1** - Fast movement (roads, paths, open plains)
- **2** - Normal movement (grassland, light vegetation)
- **3** - Slow movement (forests, rough terrain)
- **5** - Very slow (mountains, dense forest)
- **10** - Nearly impassable (set isWalkable = false instead)

**Terrain Naming:**
- Use consistent terrainName values
- Match names in PathfindingContext presets
- Example: "Forest", "Mountains", "Grassland"

---

## 5. CommandHistory Debug Window

**Location:** `Assets/Editor/CommandHistoryDebugWindow.cs`

### Overview
Photoshop-style history panel for visualizing and managing undo/redo operations. Essential for debugging command pattern implementation.

### Features

#### Visual History Display
- **Undo Stack** - Executed commands (shown with ✓ icon)
- **Redo Stack** - Undone commands (shown with ○ icon)
- **Current State Marker** - Orange line separator showing present state
- **Command Descriptions** - Each command shows its description
- **Index Numbers** - Command position in stack (#0, #1, etc.)

#### Toolbar Controls
- **⟲ Undo** - Undo last command
- **⟳ Redo** - Redo last undone command
- **Stats Toggle** - Show/hide statistics
- **Settings Toggle** - Show/hide settings panel
- **Auto Toggle** - Enable/disable auto-refresh
- **↻ Refresh** - Manual refresh button

#### Statistics Dashboard
Shows real-time command system metrics:
- **Undo Stack Count** - Number of commands that can be undone
- **Redo Stack Count** - Number of commands that can be redone
- **Total Commands** - Combined undo + redo count
- **Memory Usage** - Estimated memory with visual bar
  - Color-coded: Green (low) to Red (high)
  - Percentage of 10MB limit

#### Settings Panel
- **Clear All History** - Remove all commands (with confirmation)
- **Quick Undo/Redo:**
  - Undo 5 - Undo 5 commands at once
  - Undo 10 - Undo 10 commands at once
  - Redo 5 - Redo 5 commands at once
  - Redo 10 - Redo 10 commands at once

#### Legend
Visual guide explaining indicators:
- ✓ (Green) - Executed command (can be undone)
- ○ (Gray) - Undone command (can be redone)
- Orange bar - Current state marker

### How to Use

#### Opening the Window
`Window > Command History Debugger`

#### Viewing History
1. Enter Play mode
2. Execute game actions that use commands
3. Watch history update in real-time (if auto-refresh enabled)

#### Debugging Commands
1. Check if commands appear in history
2. Verify command descriptions are clear
3. Monitor memory usage
4. Test undo/redo functionality

#### Managing History
1. Open Settings panel
2. Use quick undo/redo for batch operations
3. Clear history when needed
4. Monitor memory usage bar

### Understanding the Display

**Color Coding:**
- **Green text/icons** - Successfully executed commands
- **Gray text/icons** - Commands that have been undone
- **White on blue** - Selected command (future feature)
- **Orange** - Current state position

**Example Display:**
```
✓ Move unit to (5,3)          #0
✓ Build structure             #1
✓ Reveal fog at (6,4)         #2
▼ CURRENT STATE ▼
○ Attack enemy                #0
○ End turn                    #1
```

This shows:
- 3 executed commands (can be undone)
- Current game state
- 2 undone commands (can be redone)

### Integration with Commands

The window automatically works with any class implementing `ICommand`:
- StateChangeCommand
- MoveUnitCommand
- RevealFogCommand
- BuildStructureCommand
- MacroCommand
- Custom commands

All commands automatically appear if executed through `CommandHistory.Instance.ExecuteCommand()`

---

## 6. HexCell Inspector

**Location:** `Assets/Editor/HexCellInspector.cs`

### Overview
Comprehensive debugging window for inspecting hex cell data, showing all coordinate systems, states, and pathfinding information.

### Features

#### Selection Tools
- **Cell Object Field** - Drag-drop GameObject with HexCell
- **Use Current Selection** - Automatically select from scene hierarchy

#### Coordinate Systems Section
Displays all three coordinate systems simultaneously:

**Offset Coordinates:**
- X: Row position
- Y: Column position
- Standard grid coordinates

**Cube Coordinates:**
- X, Y, Z values
- Constraint: X + Y + Z = 0
- Used for distance calculations

**Axial Coordinates:**
- Q: Diagonal axis
- R: Row axis
- Compact representation

**Visual Reference Diagram** - Shows coordinate system explanations

#### Terrain Information Section
- **Color Banner** - Large preview of terrain color with name
- **Terrain Properties:**
  - Name
  - Movement cost
  - Walkability status
  - Description
- Color-coded visual feedback

#### Interaction State Section
Shows cell's current interaction state:

**Current State Display:**
- Color-coded banner matching state
- State name in large text
- Full description
- Possible transitions from current state

**State Colors:**
- Invisible: Dark Gray
- Visible: Light Gray
- Highlighted: Yellow
- Selected: Green
- Focused: Blue

**Transition List:**
Shows all valid input events and resulting states
Example: "MouseEnter → Highlighted"

#### Pathfinding State Section

**Persistent State Flags:**
- ✓ Walkable - Cell can be pathfinded through
- ✓ Occupied - Cell has a unit on it
- ✓ Explored - Cell is visible (not in fog)
- ✓ Reachable - Cell reachable with current movement
- ✓ Reserved - Cell reserved for future movement
- ✓ Path - Cell is part of active path

**Pathfinding Costs:**
- Movement Cost - Base terrain cost
- G Cost - Cost from start node
- H Cost - Heuristic to goal
- F Cost - Total cost (G + H)

**Search State:**
- In Open Set - Cell in frontier
- In Closed Set - Cell already explored
- Came From - Previous cell in path

#### Neighbors Section

**Neighbor Count:** Shows X/6 neighbors

**Visual Hex Diagram:**
- Central hex (selected cell)
- 6 surrounding hexes (neighbors)
- Color-coded by terrain type

**Neighbor List:**
- Coordinate of each neighbor
- Terrain color swatch
- Terrain type name

#### Quick Actions (Play Mode)

**State Actions:**
- **Reveal** - Remove fog of war (RevealFog event)
- **Select** - Select cell (MouseDown event)
- **Focus** - Focus camera (FKeyDown event)
- **Deselect** - Clear selection (Deselect event)

**Pathfinding Actions:**
- **Toggle Walkable** - Make cell passable/impassable
- **Toggle Explored** - Toggle fog of war

### How to Use

#### Opening the Window
`Window > Hex Cell Inspector`

#### Selecting a Cell
**Method 1: Drag-Drop**
1. Find cell GameObject in hierarchy
2. Drag to "Cell Object" field

**Method 2: Selection Button**
1. Select cell in scene hierarchy
2. Click "Use Current Selection"

#### Debugging Cell State
1. Enter Play mode
2. Select cell
3. View all sections:
   - Check coordinates are correct
   - Verify terrain assignment
   - Monitor interaction state changes
   - Inspect pathfinding data
   - View neighbor connections

#### Testing State Transitions
1. Select cell
2. Use Quick Actions to trigger events
3. Watch Interaction State section update
4. Verify visual effects in scene

#### Debugging Pathfinding
1. Select cell involved in pathfinding
2. Check Pathfinding State section:
   - Verify walkability
   - Check G/H/F costs
   - Confirm open/closed set status
   - Trace path via "Came From"

### Understanding Coordinate Systems

**When to Use Each:**

**Offset (Row-Column):**
- Array indexing: `cells[x, y]`
- Grid iteration
- Storage and serialization

**Cube (X, Y, Z):**
- Distance calculations: `(|x1-x2| + |y1-y2| + |z1-z2|) / 2`
- Hexagonal algorithms
- Rotation and symmetry

**Axial (Q, R):**
- Compact storage (only 2 values)
- UI display
- Input/output

All conversions handled automatically by HexMetrics utility.

---

## Quick Reference

### All Editor Windows

| Window | Menu Path | Keyboard Shortcut |
|--------|-----------|-------------------|
| Algorithm Comparison | Opened from PathfindingManager | - |
| State Machine Visualizer | Window > Hex Cell State Machine Visualizer | - |
| Preset Wizard | Assets > Create > Pathfinding > Preset Wizard | - |
| Terrain Library | Window > Terrain Type Library | - |
| Command History | Window > Command History Debugger | - |
| Hex Cell Inspector | Window > Hex Cell Inspector | - |

### Editor-Enhanced Components

| Component | Editor Location | Key Features |
|-----------|----------------|--------------|
| PathfindingManager | Automatic in Inspector | Algorithm selection, performance stats, caching |
| HexCellStateManager | Opens visualizer window | State machine diagram, transitions |
| PathfindingContextPreset | Automatic in Inspector | Template loading, terrain costs |
| TerrainType | Automatic in Inspector | Visual previews, movement costs |
| CommandHistory | Opens debug window | History visualization, undo/redo |
| HexCell | Opens inspector window | Coordinates, states, neighbors |

---

## Workflow Integration

### Setting Up Terrain
1. Create terrain types with TerrainType editor
2. Configure movement costs using presets
3. Use Terrain Library to review all types
4. Test in Play mode and check usage statistics

### Configuring Pathfinding
1. Create PathfindingContext presets for unit types
2. Set terrain cost multipliers per preset
3. Assign presets to units or behaviors
4. Use PathfindingManager to select algorithms
5. Monitor performance via statistics dashboard

### Debugging Gameplay
1. **State Issues:** Use HexCell Inspector to see cell states
2. **Pathfinding Problems:** Check PathfindingManager performance
3. **Undo/Redo Issues:** Open CommandHistory debug window
4. **Terrain Confusion:** Use Terrain Library to compare types

### Testing State Machines
1. Open State Machine Visualizer
2. Study transition diagram
3. Enter Play mode
4. Use HexCell Inspector Quick Actions
5. Verify transitions match expectations

---

## Tips and Tricks

### Performance Optimization
- Monitor PathfindingManager cache hit rate (target: >50%)
- Check CommandHistory memory usage (should stay below 80%)
- Use JPS algorithm for open maps (10-40x speedup)

### Designer Workflow
- Create PathfindingContext presets for each unit type
- Use TerrainType presets for consistency
- Open multiple editor windows for side-by-side comparison
- Enable auto-refresh in CommandHistory for real-time debugging

### Debugging Strategy
1. Start with HexCell Inspector to verify cell data
2. Check TerrainType if movement costs seem wrong
3. Review PathfindingManager if paths are inefficient
4. Use CommandHistory to trace user actions
5. Reference State Machine Visualizer for valid transitions

---

## Credits

Custom editors created for Fantasy-Fiefdoms pathfinding and grid systems.

**Part 2 Created:** 2025
**Version:** 1.0
**Systems Covered:** TerrainType, CommandHistory, HexCell

See [Custom-Editors-Guide.md](Custom-Editors-Guide.md) for high-priority editors (PathfindingManager, HexCellStateManager, PathfindingContext).
