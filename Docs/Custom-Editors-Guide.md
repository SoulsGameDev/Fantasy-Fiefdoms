# Custom Editors Guide

This guide explains the three custom Unity editors created for Fantasy-Fiefdoms to improve the game designer workflow.

## Overview

Three high-priority systems now have custom editors:

1. **PathfindingManager Editor** - Advanced pathfinding configuration and monitoring
2. **HexCellStateManager Visualizer** - Visual state machine diagram and testing tools
3. **PathfindingContextPreset Editor** - Reusable pathfinding configurations

---

## 1. PathfindingManager Custom Editor

**Location:** `Assets/Editor/PathfindingManagerEditor.cs`

### Features

#### Algorithm Selection
- **Visual Algorithm Selector** with descriptions for all 7 algorithms
- **Use Case Recommendations** for each algorithm
- **Runtime Algorithm Switching** during Play mode
- **Algorithm Comparison Table** - Opens a separate window showing detailed comparison

**Algorithms Available:**
- **A\*** - General-purpose optimal pathfinding
- **Dijkstra** - All shortest paths from source
- **BFS** - Fast unweighted pathfinding
- **Best-First** - Fast greedy (non-optimal)
- **Bidirectional A\*** - Fast for long distances
- **Flow Field** - Group movement optimization
- **JPS** - Ultra-fast for open maps

#### Cache Management
- Enable/disable caching with visual feedback
- Configure cache duration and size limits
- **Clear Cache** button for runtime testing
- Visual warnings when caching is disabled

#### Performance Monitoring
- Real-time statistics display (Play mode only):
  - Total paths computed
  - Cache hit rate
  - Average computation time
  - Current algorithm info
- **Reset Statistics** button
- **Log to Console** for detailed analysis

#### Testing Tools
- Clear reachability visualization
- Clear path visualization
- Quick testing buttons for Play mode

### How to Use

1. **Select PathfindingManager** in your scene hierarchy
2. **Choose an Algorithm** from the dropdown
3. **Read the Description** to understand when to use it
4. **Click "Show Algorithm Comparison Table"** for detailed comparison
5. **Enter Play Mode** to see live statistics and test features

### Best Practices

- Use **A\*** for general player/AI movement
- Use **Flow Field** when many units move to the same destination
- Use **JPS** for open maps with consistent terrain costs
- Use **Dijkstra** when you need paths to multiple destinations
- Enable caching for frequently requested paths
- Monitor cache hit rate - aim for >50% in typical gameplay

---

## 2. HexCellStateManager Visualizer

**Location:** `Assets/Editor/HexCellStateManagerEditor.cs`

### Features

#### Visual State Machine Diagram
- **5 Color-Coded States:**
  - Invisible (Dark Gray) - Fog of war
  - Visible (Light Gray) - Default state
  - Highlighted (Yellow) - Mouse hover
  - Selected (Green) - Mouse down
  - Focused (Blue) - F key pressed

- **Animated Transitions** showing all 14 possible state transitions
- **Visual Arrows** indicating input events and state flows

#### State Descriptions
- Detailed explanation for each state
- Color indicators matching the diagram
- Use case recommendations

#### Comprehensive Transition Table
- All 14 transitions listed with:
  - Source state
  - Target state
  - Input event trigger
  - Description

#### Interactive Testing (Play Mode)
- Select cells in the scene
- Test specific state transitions
- View transition results in console

### How to Use

1. **Open Window:** `Window > Hex Cell State Machine Visualizer`
2. **Study the Diagram** to understand state flow
3. **Read State Descriptions** for context
4. **Review Transition Table** for complete reference
5. **Enter Play Mode** to test transitions on actual cells

### Understanding the State Machine

**State Flow Example:**
```
Invisible → (RevealFog) → Visible → (MouseEnter) → Highlighted → (MouseDown) → Selected → (FKeyDown) → Focused
```

**Key Transitions:**
- **MouseEnter/Exit** - Hover effects
- **MouseDown/Up** - Selection
- **FKeyDown** - Focus toggle
- **Deselect** - Programmatic reset
- **RevealFog** - Exploration system

### Design Guidelines

- Use **Highlighted** for hover feedback
- Use **Selected** for active selection
- Use **Focused** for camera/UI focus
- Guards can prevent transitions based on game state

---

## 3. PathfindingContextPreset System

**Files:**
- `Assets/Scripts/Pathfinding/Core/PathfindingContextPreset.cs` - ScriptableObject
- `Assets/Editor/PathfindingContextPresetEditor.cs` - Custom Editor

### Features

#### Preset Management
- **ScriptableObject-based** presets for reusability
- **Template System** with 4 built-in templates:
  - Infantry - Standard ground units
  - Cavalry - Fast units with terrain preferences
  - Flying - Ignores terrain, unrestricted movement
  - Tactical Combat - Cautious, prefers high ground

#### Configuration Sections

**Movement Settings**
- Max Movement Points (-1 for unlimited)
- Max Search Nodes (performance limit)

**Traversal Rules**
- Allow move through allies
- Allow move through enemies
- Require explored (fog of war)
- Allow diagonal movement (future)

**Strategic Preferences**
- Prefer high ground
- Avoid enemy zones

**Terrain Cost Multipliers**
- Custom cost for each terrain type
- 1.0 = normal, 2.0 = double cost, 0.5 = half cost
- Quick-add buttons for common terrains

**Performance Settings**
- Store diagnostic data
- Use caching

#### Preset Creation Wizard
- **Menu:** `Assets > Create > Pathfinding > Preset Wizard`
- Create presets from templates
- Name and describe your preset
- Automatically saved as asset

### How to Use

#### Creating a New Preset

**Method 1: Manual Creation**
1. Right-click in Project window
2. `Create > Pathfinding > Context Preset`
3. Name your preset
4. Configure in Inspector

**Method 2: Preset Wizard**
1. `Assets > Create > Pathfinding > Preset Wizard`
2. Enter name and description
3. Choose template
4. Click "Create Preset"

#### Loading Templates
1. Select existing preset
2. Click template button (Infantry, Cavalry, etc.)
3. Confirm to load template settings
4. Customize as needed

#### Using Presets in Code

```csharp
// Load preset
public PathfindingContextPreset infantryPreset;

// Use in pathfinding
void FindPath(HexCell start, HexCell goal)
{
    PathfindingContext context = infantryPreset.CreateContext();
    PathResult result = PathfindingManager.Instance.FindPath(start, goal, context);
}
```

#### Terrain Cost Examples

**Cavalry (fast on plains, slow in forests):**
- Grassland: 0.5x cost
- Forest: 2.0x cost
- Mountains: 3.0x cost

**Flying (ignores terrain):**
- All terrain: 1.0x cost

**Infantry (standard movement):**
- Default terrain costs

### Best Practices

#### When to Create Presets
- Different unit types (infantry, cavalry, archers)
- Different AI behaviors (aggressive, cautious, explorer)
- Special abilities (flying, teleport, stealth)
- Game modes (combat, exploration, building)

#### Naming Conventions
- Use descriptive names: "Heavy Infantry", "Scout Cavalry", "Combat Mage"
- Include movement speed in name if relevant
- Use consistent prefixes for organization

#### Organization
Create folders:
```
Assets/ScriptableObjects/PathfindingPresets/
  ├── Infantry/
  ├── Cavalry/
  ├── Flying/
  ├── Special/
  └── AI/
```

---

## Integration Examples

### Example 1: Unit Movement System

```csharp
public class Unit : MonoBehaviour
{
    [SerializeField] private PathfindingContextPreset movementPreset;

    public void MoveTo(HexCell target)
    {
        PathfindingContext context = movementPreset.CreateContext();
        PathResult path = PathfindingManager.Instance.FindPath(currentCell, target, context);

        if (path.Success)
        {
            StartCoroutine(FollowPath(path.Path));
        }
    }
}
```

### Example 2: Dynamic Context Modification

```csharp
public void MoveWithCombatContext(HexCell target)
{
    PathfindingContext context = basePreset.CreateContext();

    // Override specific settings for combat
    context.PreferHighGround = true;
    context.AvoidEnemyZones = true;

    PathResult path = PathfindingManager.Instance.FindPath(currentCell, target, context);
}
```

### Example 3: Algorithm Selection Based on Distance

```csharp
public void SmartPathfinding(HexCell start, HexCell goal)
{
    int distance = CalculateDistance(start, goal);

    if (distance > 20)
    {
        // Use Bidirectional A* for long paths
        PathfindingManager.Instance.SetAlgorithm(AlgorithmType.BidirectionalAStar);
    }
    else
    {
        // Use standard A* for short paths
        PathfindingManager.Instance.SetAlgorithm(AlgorithmType.AStar);
    }

    PathResult path = PathfindingManager.Instance.FindPath(start, goal, context);
}
```

### Example 4: Group Movement with Flow Field

```csharp
public void MoveGroupToPosition(List<Unit> units, HexCell destination)
{
    PathfindingContext context = new PathfindingContext();

    // Generate flow field once
    FlowField field = PathfindingManager.Instance.GenerateFlowField(destination, context);

    // Each unit follows the field (O(1) per unit)
    foreach (Unit unit in units)
    {
        Vector2Int direction = field.GetDirectionAt(unit.CurrentCell);
        unit.MoveInDirection(direction);
    }
}
```

---

## Performance Tips

### PathfindingManager
1. **Choose the Right Algorithm**
   - A* for general use
   - JPS for open maps
   - Flow Field for groups
   - BFS for simple range checks

2. **Enable Caching**
   - Set appropriate cache duration (3-5 seconds typical)
   - Monitor cache hit rate
   - Clear cache when map changes

3. **Use Async Pathfinding**
   - For non-critical paths
   - Prevents frame stuttering
   - Enable threading support

### PathfindingContext Presets
1. **Limit Terrain Multipliers**
   - Only define non-default terrain costs
   - Use reasonable multipliers (0.5x to 3.0x)

2. **Set Reasonable Movement Limits**
   - MaxMovementPoints prevents expensive searches
   - MaxSearchNodes provides hard limit

3. **Disable Diagnostic Data in Production**
   - Saves memory
   - Faster pathfinding
   - Enable only for debugging

---

## Troubleshooting

### PathfindingManager Shows "Play Mode Only"
- Many features require Play mode to access runtime data
- Enter Play mode to test algorithms and view statistics

### State Machine Transitions Not Working
- Check if guards are enabled in TransitionGuardRegistry
- Verify cell has proper HexCellInteractionState component
- Review guard evaluation logic

### Preset Not Affecting Pathfinding
- Ensure you call `preset.CreateContext()` to generate PathfindingContext
- Verify preset is assigned in Inspector
- Check if MaxMovementPoints allows reaching destination

### Cache Not Working
- Verify "Enable Caching" is checked on PathfindingManager
- Ensure PathfindingContext has `UseCaching = true`
- Cache clears when switching algorithms

---

## Future Enhancements

Potential additions to these editors:

1. **PathfindingManager**
   - In-editor pathfinding test tool (click two cells to test)
   - Real-time cost heatmap visualization
   - Performance profiling graphs
   - Algorithm benchmark comparison

2. **State Machine Visualizer**
   - Runtime state tracking (show current state of all cells)
   - Transition history playback
   - Guard failure visualization
   - Custom transition recording

3. **PathfindingContext Presets**
   - Visual terrain cost editor with map preview
   - Preset comparison tool
   - Auto-optimization suggestions
   - Unit type templates library

---

## Credits

Custom editors created for Fantasy-Fiefdoms pathfinding system.

**Created:** 2025
**Version:** 1.0
**Systems Covered:** PathfindingManager, HexCellStateManager, PathfindingContext

For questions or issues, see the main project README.
