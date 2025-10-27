# Inventory UI System - Setup Guide

Complete guide for setting up and using the inventory UI components.

## Overview

The UI system provides ready-to-use components for displaying inventories with drag-and-drop functionality, tooltips, and visual feedback.

## Components

### 1. ItemSlotUI
Displays a single inventory slot with icon, quantity, and rarity border.

**Features:**
- Automatic icon and quantity display
- Rarity-based border coloring
- Hover highlighting
- Click and right-click events
- Drag-and-drop support
- Selection highlighting

### 2. DragDropManager (Singleton)
Handles all drag-and-drop operations between slots.

**Features:**
- Visual dragging with semi-transparent icon
- Automatic validation
- Command system integration for undo/redo
- Cross-inventory drag support

### 3. ItemTooltip
Shows detailed item information on hover.

**Features:**
- Item name with rarity color
- Description and stats
- Equipment requirements
- Special effects list
- Durability and quantity info
- Auto-positioning to stay on screen

### 4. InventoryPanel
Complete inventory display with grid layout.

**Features:**
- Automatic slot creation
- Capacity and weight display
- Configurable grid layout
- Selection management
- Show/hide functionality

### 5. EquipmentPanel
Displays equipment slots (Head, Chest, Weapon, etc.).

**Features:**
- Fixed equipment slots
- Total stats display (Armor, Damage)
- Equipped item visualization
- Integration with EquipmentInventory

## Setup Instructions

### Step 1: Create UI Prefabs

#### ItemSlotUI Prefab
```
ItemSlot (GameObject)
├── Image (Background)
├── Icon (Image)
├── Quantity (TextMeshPro)
├── RarityBorder (Image)
└── SelectionHighlight (Image)
```

1. Create empty GameObject: "ItemSlot"
2. Add ItemSlotUI script
3. Add Image component for background
4. Create child "Icon" with Image component
5. Create child "Quantity" with TextMeshProUGUI
6. Create child "RarityBorder" with Image (outline)
7. Create child "SelectionHighlight" with Image
8. Assign references in ItemSlotUI inspector
9. Save as prefab

#### ItemTooltip Prefab
```
ItemTooltip (GameObject)
├── Background (Image)
├── Border (Image)
├── Content (VerticalLayoutGroup)
│   ├── Name (TextMeshPro)
│   ├── Type (TextMeshPro)
│   ├── Description (TextMeshPro)
│   ├── Stats (TextMeshPro)
│   ├── Requirements (TextMeshPro)
│   └── Effects (TextMeshPro)
```

### Step 2: Create Inventory Panel

1. Create empty GameObject in Canvas: "InventoryPanel"
2. Add InventoryPanel script
3. Create child "SlotContainer" with GridLayoutGroup
4. Add title text (TextMeshProUGUI)
5. Add capacity text (TextMeshProUGUI) for "Slots: X/Y"
6. Add weight text (TextMeshProUGUI) for "Weight: X/Y"
7. Assign references in InventoryPanel inspector
8. Assign ItemSlotUI prefab
9. Configure grid settings (slots per row, spacing, etc.)

### Step 3: Create Equipment Panel

1. Create empty GameObject in Canvas: "EquipmentPanel"
2. Add EquipmentPanel script
3. Create child GameObjects for each equipment slot:
   - HeadSlot
   - ChestSlot
   - MainHandSlot
   - etc.
4. Add EquipmentSlotUI script to each slot
5. Position slots visually (e.g., helmet at top, weapon on left)
6. Add stats display text
7. Assign references in EquipmentPanel inspector

### Step 4: Setup Tooltip

1. Create ItemTooltip prefab (see structure above)
2. Place in Canvas (will auto-position)
3. Assign to InventoryPanel and EquipmentPanel

### Step 5: Initialize in Code

```csharp
using Inventory.Core;
using Inventory.UI;

// Create inventory
var playerBag = new Inventory("player_bag", maxSlots: 20, maxWeight: 100f);

// Initialize UI
inventoryPanel.Initialize(playerBag, "Player Inventory");

// Create equipment
var equipment = new EquipmentInventory("player_equipment", gameObject);

// Initialize equipment UI
equipmentPanel.Initialize(equipment, "Equipment");

// DragDropManager auto-initializes as Singleton
```

## Usage Examples

### Show/Hide Inventory
```csharp
inventoryPanel.Show();
inventoryPanel.Hide();
inventoryPanel.Toggle();
```

### Handle Slot Events
```csharp
ItemSlotUI slot = inventoryPanel.GetSlotUI(0);
slot.OnSlotClicked += (s) => {
    Debug.Log($"Clicked slot {s.SlotIndex}");
};
```

### Custom Drag-Drop Handling
```csharp
// DragDropManager automatically handles drag/drop
// But you can customize:
DragDropManager.Instance.SetUseCommandSystem(true); // For undo/redo
DragDropManager.Instance.SetDragImageAlpha(0.6f);   // Transparency
```

### Disable Drag-Drop for Specific Slot
```csharp
ItemSlotUI slot = inventoryPanel.GetSlotUI(0);
slot.SetDragDropEnabled(false); // Lock slot
```

## Customization

### Colors
Edit in ItemSlotUI inspector:
- Empty Color: Background when empty
- Occupied Color: Background with item
- Highlight Color: Background on hover
- Can Drop Color: Green feedback when can drop
- Cannot Drop Color: Red feedback when cannot drop

### Layout
Edit in InventoryPanel:
- Slots Per Row: Grid columns
- Slot Spacing: Gap between slots
- Slot Size: Width/height of each slot

### Tooltip
Edit in ItemTooltip:
- Offset X/Y: Distance from cursor
- Max Width: Tooltip maximum width
- Background Color: Tooltip background

## Keyboard Shortcuts (Example Scene)

- **I** - Toggle Inventory
- **E** - Toggle Equipment
- **T** - Add Test Item
- **R** - Remove Random Item
- **Q** - Equip Random Weapon

## Tips

1. **Performance**: ItemSlotUI updates only when needed via events
2. **Pooling**: Consider object pooling for large inventories
3. **Mobile**: ItemSlotUI supports touch events (Unity handles automatically)
4. **Accessibility**: Add TextMeshPro size scaling for readability
5. **Theming**: Use Unity UI Themes for consistent colors

## Troubleshooting

### Slots not appearing
- Check SlotContainer has GridLayoutGroup
- Verify ItemSlotUI prefab is assigned
- Ensure Inventory is initialized before panel

### Drag-drop not working
- Check DragDropManager exists in scene
- Verify Canvas has GraphicRaycaster
- Ensure EventSystem is in scene

### Tooltip not showing
- Check tooltip GameObject is active
- Verify tooltip is assigned to panel
- Ensure Canvas is in ScreenSpaceOverlay or has Camera

### Icons not displaying
- Verify ItemType has Icon sprite assigned
- Check Image component on Icon child
- Ensure sprites are imported correctly

## Advanced

### Custom Slot UI
Extend ItemSlotUI for custom behavior:
```csharp
public class CustomSlotUI : ItemSlotUI
{
    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();
        // Add custom visual logic
    }
}
```

### Multi-Inventory Drag
DragDropManager automatically handles cross-inventory drags using MoveItemCommand.

### Event Integration
```csharp
// Subscribe to all slot events
foreach (var slot in inventoryPanel.SlotUIElements)
{
    slot.OnSlotClicked += HandleClick;
    slot.OnDragBegin += HandleDragStart;
    slot.OnDragEnd += HandleDragEnd;
}
```

## Next Steps

- See `InventoryUIExample.cs` for complete working example
- Check Phase 4 for merchant/trade UI components
- Explore UI prefab templates (coming soon)

## License

Part of the Fantasy Fiefdoms inventory system.
