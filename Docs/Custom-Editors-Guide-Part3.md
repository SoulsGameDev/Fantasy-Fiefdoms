# Custom Editors Guide - Part 3

This document covers the low-priority custom editors added to Fantasy-Fiefdoms: MapGenerator and CameraController.

## 7. MapGenerator Enhanced Editor

**Location:** `Assets/Editor/MapGeneratorEditor.cs`

### Overview
Massively enhanced procedural map generation tool with real-time preview, statistics, and export capabilities. Transforms the basic 26-line editor into a comprehensive 840-line professional tool.

### Features Overview

The editor is organized into 6 main sections:
1. Quick Actions
2. Basic Settings
3. Noise Settings
4. Biome Distribution
5. Preview & Comparison
6. Map Statistics

---

### Quick Actions Section

**Generate Map Button (Large, Green)**
- Prominent button for generating maps
- Updates statistics and preview automatically
- Green color indicates primary action

**Random Seed Button**
- Generates random seed (0-999,999)
- Dice icon (🎲) for quick access
- Instantly regenerates if auto-update enabled

**Regenerate Similar**
- Creates similar map with slight seed variation
- Uses ±100 seed range
- Perfect for finding variations of good maps

**Export as PNG**
- Exports full-resolution map as PNG
- Filename includes seed: `Map_Seed12345.png`
- Save anywhere dialog
- Confirmation with file path

**Copy Settings**
- Copies all parameters to clipboard
- Useful for sharing configurations
- Includes: seed, scale, octaves, persistence, lacunarity, offset, size

---

### Basic Settings Section

**Use Hex Grid Size**
- Toggle to use HexGrid component dimensions
- Shows current width/height from HexGrid
- Manual width/height if disabled
- Total cells calculation

**Auto Update**
- Automatically regenerates on parameter change
- Maintained from original editor

**Generate On Start**
- Generate map when game starts
- Useful for runtime testing

**Use Threading**
- Enable/disable threaded generation
- Info box explains threading behavior
- Recommended for Play mode

---

### Noise Settings Section

Comprehensive Perlin noise parameter controls with real-time feedback.

**Seed Control**
- Integer field for seed entry
- Dice button for random generation
- Current seed displayed

**Noise Scale**
- Property field + slider (0.01-10)
- Hint: "Lower = zoomed out, Higher = zoomed in"
- Real-time visual feedback

**Octaves**
- Number of noise layers
- Hint shows current count
- "More layers = more detail"

**Persistence**
- Controls amplitude change between octaves
- Range: 0-1 (validated)
- Affects terrain roughness

**Lacunarity**
- Controls frequency change between octaves
- Minimum: 1.0
- Affects detail distribution

**Offset**
- Vector2 for noise map offset
- Useful for panning noise without changing seed

**Quick Presets:**
- **Smooth** (0.3 scale, 3 octaves, 0.5 persistence, 2.0 lacunarity)
  - Gentle rolling terrain
  - Few large features
  - Good for plains/ocean maps

- **Detailed** (0.5 scale, 6 octaves, 0.5 persistence, 2.0 lacunarity)
  - Balanced detail
  - Multiple terrain features
  - Good general-purpose setting

- **Rough** (0.8 scale, 8 octaves, 0.6 persistence, 2.5 lacunarity)
  - Highly detailed
  - Chaotic terrain
  - Good for mountainous regions

- **Islands** (0.4 scale, 5 octaves, 0.4 persistence, 2.2 lacunarity)
  - Archipelago-style generation
  - Multiple landmasses
  - Ocean-focused

- **Continents** (0.2 scale, 4 octaves, 0.5 persistence, 2.0 lacunarity)
  - Large landmasses
  - Fewer, bigger features
  - Continental scale

---

### Biome Distribution Section

Manage terrain height thresholds for biome placement.

**Biomes List**
- Standard Unity array property
- Add/remove terrain height entries
- Each entry: Height (0-1) + TerrainType reference

**Visual Distribution Bar**
- Horizontal bar showing height distribution
- Color-coded segments for each biome
- Percentage labels on wide segments
- Visual representation of biome ranges

**Example:**
```
Ocean (0-30%) | Beach (30-35%) | Grassland (35-70%) | Mountains (70-100%)
```

**Add Default Biomes**
- Placeholder for preset biome distributions
- Would load common biome setups

---

### Preview Section

Real-time visualization with advanced features.

**Preview Controls:**
- **Show Noise** toggle - Display grayscale noise map
- **Show Color** toggle - Display colored terrain map
- **Refresh Preview** button - Regenerate preview textures

**Octave Scrubber**
- Slider: -1 (All) to N-1 (octave count)
- View individual noise layers
- Understand how octaves combine
- Educational tool for learning noise

**Example Workflow:**
1. Set octaves to 6
2. Scrub through 0-5 to see each layer
3. Set to -1 to see combined result

**Noise Preview (256x256)**
- Grayscale visualization
- Shows height values directly
- Updates when octave slider changes

**Color Preview (256x256)**
- Colored terrain map
- Shows actual biome placement
- Matches in-game appearance

**Seed Comparison**
- Enter comparison seed
- Click "Generate" for side-by-side
- Compare different seeds with same parameters
- Useful for A/B testing

---

### Map Statistics Section

Detailed terrain distribution analysis.

**Terrain Distribution Display:**
- Color indicator (terrain color)
- Terrain name
- Percentage bar (visual)
- Percentage value + cell count
- Sorted by most common terrain

**Example Output:**
```
Ocean     [████████████████████] 45.2% (11,532)
Grassland [████████████        ] 30.1% (7,680)
Forest    [██████              ] 15.3% (3,904)
Mountains [████                ] 9.4% (2,400)
```

**Summary Info:**
- Total cells count
- Current seed
- Real-time updates

**Use Cases:**
- Verify biome balance
- Ensure map variety
- Compare different noise settings
- Debug biome thresholds

---

### Save/Load Presets Section

*(Placeholder for future MapGeneratorPreset ScriptableObject)*

Currently shows "Coming Soon" dialogs.

**Planned Features:**
- Save current settings as ScriptableObject
- Load saved presets
- Template library
- Share presets between projects

---

## 8. CameraController Preset System

**Files:**
- `Assets/Scripts/CameraPreset.cs` - ScriptableObject
- `Assets/Editor/CameraControllerEditor.cs` - Custom Editor

### Overview
Complete camera configuration management with reusable presets, visual feedback, and scene editing tools.

### CameraPreset ScriptableObject

**Fields:**
- presetName - Display name
- description - Use case explanation
- defaultCameraMode - TopDown or Focus
- Movement: speed, damping, bounds (min/max)
- Zoom: speed, min, max, default FOV
- Rotation: enable, speed

**Methods:**
- `ApplyToController()` - Apply preset to CameraController
- `CaptureFromController()` - Save current settings
- Uses reflection to access private fields

**Built-in Templates:**

**Exploration Preset**
```
Speed: 15 (fast panning)
Damping: 3 (snappy)
Zoom Default: 75 (wide view)
Rotation: Disabled
Use: Exploring large maps, scouting
```

**Tactical Combat Preset**
```
Speed: 8 (slower, precise)
Damping: 7 (smooth)
Zoom Default: 35 (close-up)
Zoom Range: 15-60 (tactical range)
Rotation: Enabled (40°/s)
Use: Turn-based combat, precise unit placement
```

**City Building Preset**
```
Speed: 12 (balanced)
Damping: 6 (smooth)
Zoom Default: 55 (medium view)
Rotation: Enabled (60°/s)
Use: Construction, resource management
```

**Cinematic Preset**
```
Speed: 5 (very slow)
Damping: 10 (very smooth)
Zoom Default: 45 (cinematic)
Zoom Speed: 0.5 (slow zoom)
Rotation: Enabled (30°/s slow rotation)
Use: Screenshots, trailers, cutscenes
```

---

### CameraController Custom Editor

**Camera References Section**
- Assign camera target, top-down camera, focus camera
- Set default camera mode
- Runtime mode switching (Play mode):
  - "Top Down" button
  - "Focus" button
  - Shows current mode

**Movement Settings Section**

Visual controls with feedback bars:

**Pan Speed**
- Range: 5-20
- Visual bar: Blue (slow) → Orange (fast)
- Description: "Higher = faster panning"
- Current value display

**Damping / Smoothing**
- Range: 1-10
- Visual bar: Blue (snappy) → Orange (smooth)
- Description: "Higher = smoother, more laggy"
- Trade-off between responsiveness and smoothness

**Quick Presets:**
- Fast (15 speed, 3 damping)
- Normal (10 speed, 5 damping)
- Slow (6 speed, 8 damping)

---

**Zoom Settings Section**

**Zoom Speed**
- FOV change rate
- Higher = faster zoom

**FOV Range Configuration:**
- Min FOV (Zoomed In): 15-60 typical
- Max FOV (Zoomed Out): 60-120 typical
- Default FOV: Starting view

**Visual Zoom Range:**
- Horizontal bar showing 0-179° range
- Blue section = usable range
- Yellow marker = default position
- Labels at min/max values

**Example Visualization:**
```
|    [=====|======|==========]                    |
0    15   30 ↑   60          100                 179
     Min  Default Max
```

**Quick Presets:**
- Close-Up (15-60, default 35)
- Normal (15-100, default 50)
- Wide (30-120, default 75)

---

**Rotation Settings Section**

**Enable Rotation Toggle**
- Turn camera rotation on/off
- Info box when disabled

**Rotation Speed**
- Degrees per second
- Visual speed bar (20-100 range)
- Higher = faster rotation

---

**Camera Bounds Section**

Define movement area on XZ plane.

**Bounds Configuration:**
- Min (Bottom-Left): Vector2
- Max (Top-Right): Vector2
- Bounds Size display (calculated)

**Edit in Scene View**
- Toggle to show handles
- Yellow boundary rectangle
- Drag corner handles to adjust
- Real-time updates
- Undo support

**Quick Size Presets:**
- Small (50x50)
- Medium (100x100)
- Large (200x200)

**Center Bounds on Origin**
- Button to center bounds at (0,0)
- Maintains current size
- Useful after manual adjustments

**Scene View Editing:**
1. Enable "Edit in Scene View"
2. Yellow rectangle appears
3. Drag corner handles
4. Bounds update in real-time
5. Disable when done

---

**Camera Presets Section**

**Built-in Templates:**
- 4 buttons for instant preset application
- Exploration, Combat, Building, Cinematic
- Confirmation dialog with description
- One-click application

**Custom Presets:**

**Save As New Preset:**
1. Configure camera settings
2. Click "Save As New Preset..."
3. Choose location and name
4. Creates CameraPreset asset
5. Automatically selects new asset

**Load From Preset:**
1. Click "Load From Preset..."
2. Select CameraPreset asset
3. Confirms before applying
4. Overwrites current settings

**Show All Camera Presets:**
- Opens Camera Preset Library window
- Browse all presets in project
- Quick selection

---

### Camera Preset Library Window

**Access:** `Window > Camera Preset Library`

**Features:**
- Lists all CameraPreset assets
- Sortable by name
- Visual cards with info:
  - Preset name (large, bold)
  - Description
  - Key stats (speed, zoom)
  - Select button

**Card Layout Example:**
```
┌─────────────────────────────────────┐
│ Tactical Combat                     │
│ Close-up view for tactical          │
│ decisions...                        │
│ Speed: 8 | Zoom: 35                 │
│                          [Select]   │
└─────────────────────────────────────┘
```

**Actions:**
- Refresh button - Reload preset list
- Click "Select" - Opens preset in inspector
- Ping in project window

---

## Workflow Examples

### MapGenerator Workflow

**Finding the Perfect Map:**
1. Set desired size (width x height)
2. Click "Random Seed" multiple times
3. Use "Regenerate Similar" on good seeds
4. Adjust noise parameters with presets
5. View statistics to verify biome balance
6. Export favorite maps as PNG

**Understanding Noise:**
1. Generate a map
2. Set octaves to 6
3. Scrub octave slider from 0-5
4. Observe how layers combine
5. Adjust parameters based on understanding

**Comparing Seeds:**
1. Generate map with current seed
2. Enter similar seed in "Compare Seed"
3. Click "Generate" comparison
4. View side-by-side
5. Choose best result

### CameraController Workflow

**Setting Up Game Camera:**
1. Start with closest template (Combat, Exploration, etc.)
2. Adjust speed/damping to feel
3. Set zoom range for gameplay needs
4. Enable "Edit in Scene View"
5. Adjust bounds to match level size
6. Test in Play mode
7. Save as custom preset

**Creating Cinematic Camera:**
1. Load Cinematic preset
2. Slow down speed to 3-5
3. Increase damping to 12
4. Set tight zoom range (40-60 FOV)
5. Enable slow rotation (20°/s)
6. Save as "Screenshot Camera"

**Per-Level Camera Settings:**
1. Create preset per level
2. Different bounds per level
3. Load appropriate preset on level load
4. Save player preferences

---

## Best Practices

### MapGenerator

**Noise Parameters:**
- Start with presets, then adjust
- Lower scale = bigger features
- More octaves = more detail (slower)
- Higher persistence = rougher terrain
- Use comparison for A/B testing

**Biome Distribution:**
- Ocean should be 0.0-0.3 typically
- Beach narrow band (0.3-0.35)
- Grassland largest (0.35-0.7)
- Mountains highest (0.7-1.0)
- Check statistics for balance

**Seed Management:**
- Save good seeds in text file
- Export PNG for reference
- Copy settings to clipboard
- Document what works

**Performance:**
- Disable threading in editor for stability
- Enable threading in builds
- Smaller maps generate faster
- Preview is downsampled (256x256)

### CameraController

**Movement Feel:**
- Fast games: High speed (15+), low damping (3-5)
- Tactical games: Medium speed (8-12), medium damping (5-7)
- Cinematic: Low speed (5-8), high damping (8-12)

**Zoom Ranges:**
- Combat: Tight range (15-60) for consistent view
- Exploration: Wide range (30-120) for flexibility
- Building: Medium range (20-80) for detail work

**Bounds:**
- Set slightly larger than playable area
- Allows viewing edges
- Prevents camera from going too far
- Use scene editing for precision

**Rotation:**
- Disable for purely top-down games
- Enable for tactical 3D games
- Slower rotation for cinematic feel
- Faster rotation for quick orientation

---

## Tips and Tricks

### MapGenerator

**Finding Island Maps:**
1. Use "Islands" preset
2. Random seed until you see archipelago
3. Adjust ocean biome height (0-0.4 for more ocean)
4. Export good ones for level design

**Consistent Terrain:**
1. Use "Smooth" or "Detailed" presets
2. Lower octaves (3-4) for simplicity
3. Higher scale (0.8-1.5) for consistency

**Dramatic Landscapes:**
1. Use "Rough" preset
2. High octaves (7-8)
3. High persistence (0.6-0.8)
4. View individual octaves to understand chaos

### CameraController

**Quick Level Switching:**
1. Create preset per level
2. Store reference in level data
3. Apply on level load:
```csharp
levelPreset.ApplyToController(CameraController.Instance);
```

**Player Preference System:**
1. Capture current settings to preset
2. Save preset path to PlayerPrefs
3. Load on game start

**Dynamic Camera:**
```csharp
// Switch to combat camera during battle
combatPreset.ApplyToController(CameraController.Instance);

// Switch back to exploration
explorationPreset.ApplyToController(CameraController.Instance);
```

---

## Troubleshooting

### MapGenerator

**Preview Not Updating:**
- Click "Refresh Preview"
- Ensure map generated successfully
- Check console for errors

**Statistics Wrong:**
- Generate new map
- Statistics update automatically
- Check biome assignments

**Export Fails:**
- Ensure map generated first
- Check disk permissions
- Verify save path is valid

**Slow Generation:**
- Reduce map size
- Lower octave count
- Disable threading in editor
- Use threading in builds

### CameraController

**Preset Not Applying:**
- Ensure using reflection correctly
- Check console for field warnings
- CameraController fields must be serialized

**Bounds Not Showing:**
- Enable "Edit in Scene View"
- Ensure scene view open
- Check gizmos enabled

**Camera Too Fast/Slow:**
- Test in Play mode (build feels different)
- Adjust damping, not just speed
- Use presets as starting points

---

## Future Enhancements

Potential additions:

### MapGenerator
- MapGeneratorPreset ScriptableObject
- Preset library like camera
- Batch generation (multiple seeds)
- Heightmap export
- Custom biome shapes
- Region-based generation
- Undo/redo support

### CameraController
- Camera path recording
- Smooth transitions between presets
- Per-mode zoom ranges
- Multiple camera target support
- Shake/trauma system
- Look-ahead prediction
- Cinematic camera paths

---

## Credits

Enhanced editors created for Fantasy-Fiefdoms.

**Part 3 Created:** 2025
**Version:** 1.0
**Systems Covered:** MapGenerator, CameraController

See also:
- [Custom-Editors-Guide.md](Custom-Editors-Guide.md) - High-priority editors
- [Custom-Editors-Guide-Part2.md](Custom-Editors-Guide-Part2.md) - Medium-priority editors
