# Inventory System - Quick Start Guide

A modular, optimized inventory system for Unity with full undo/redo support.

## Features

- **Modular Design**: Easy to extract and use in other projects
- **Undo/Redo**: All operations use the Command pattern
- **Optimized**: Dictionary-based O(1) lookups, struct-based data
- **Flexible**: Supports bags, equipment, merchants, quest items, containers
- **Validated**: Guard system prevents invalid operations
- **Extensible**: Easy to add new item types, effects, and inventory types

## Quick Start

### 1. Create an Item Type

Right-click in Project view → Create → Inventory → Item Type

Configure the item properties:
- **ItemID**: Unique identifier
- **Name**: Display name
- **Category**: Equipment, Consumable, Quest, etc.
- **Rarity**: Common, Uncommon, Rare, Epic, Legendary, Mythic
- **MaxStack**: 1 for non-stackable, 99+ for stackable
- **Weight**: Used for weight-based inventories
- **Value**: Base price/worth

### 2. Add Items to InventoryManager

1. Create an empty GameObject in your scene
2. Add the `InventoryManager` component (it's a Singleton)
3. Drag your ItemType ScriptableObjects into the ItemTypes list

### 3. Create and Use an Inventory

```csharp
using Inventory.Core;
using Inventory.Data;
using Inventory.Commands;

// Create an inventory
var playerBag = new Inventory("player_bag", maxSlots: 20, maxWeight: 100f);

// Register with manager
InventoryManager.Instance.RegisterInventory(playerBag);

// Create an item stack
ItemType healthPotion = InventoryManager.Instance.FindItemType("health_potion");
ItemStack stack = healthPotion.CreateStack(5); // 5 potions

// Add items using command (supports undo/redo)
var addCmd = new AddItemCommand(playerBag, stack);
if (addCmd.CanExecute())
{
    addCmd.Execute();
}

// Query inventory
int potionCount = playerBag.GetItemQuantity("health_potion");
bool hasEnough = playerBag.ContainsItem("health_potion", 3);
float totalWeight = playerBag.GetTotalWeight();

// Remove items
var removeCmd = new RemoveItemCommand(playerBag, "health_potion", 2);
removeCmd.Execute();
```

### 4. Integrate with CommandHistory (Optional)

For undo/redo support:

```csharp
var addCmd = new AddItemCommand(playerBag, stack);
CommandHistory.Instance.ExecuteCommand(addCmd);

// Later, undo with Ctrl+Z or programmatically:
CommandHistory.Instance.Undo();
CommandHistory.Instance.Redo();
```

## Architecture Overview

```
┌─────────────────────────────────────┐
│   UI Layer                          │
│   (Phase 3 - Not yet implemented)  │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Command Layer ✓                   │
│   - AddItemCommand                  │
│   - RemoveItemCommand               │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Guard Layer ✓                     │
│   - CanAddItemGuard                 │
│   - HasSpaceGuard                   │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Core Logic ✓                      │
│   - Inventory                       │
│   - InventorySlot                   │
│   - ItemStack                       │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Data Layer ✓                      │
│   - ItemType (ScriptableObject)     │
│   - Enums (Rarity, Category, etc.)  │
└─────────────────────────────────────┘
```

## Current Status: Phase 1 Complete ✓

### Implemented (Phase 1)
- ✅ Core data structures (ItemStack, InventorySlot)
- ✅ Base Inventory class with O(1) lookups
- ✅ ItemType ScriptableObject
- ✅ InventoryManager Singleton
- ✅ Guard system (CanAddItemGuard, HasSpaceGuard)
- ✅ Command system (AddItemCommand, RemoveItemCommand)
- ✅ Example script and documentation

### Coming Next (Phase 2)
- Equipment system with slots
- EquipItemCommand / UnequipItemCommand
- Stat modifier effects
- Equipment restrictions

### Future Phases
- Phase 3: UI system
- Phase 4: Specialized inventories (merchant, quest, container)
- Phase 5: Consumables and advanced effects
- Phase 6: Unique items, durability, crafting
- Phase 7: Optimization, serialization, polish

## File Structure

```
Assets/Scripts/Inventory/
├── Core/
│   ├── Inventory.cs              ✓ Base inventory container
│   ├── InventorySlot.cs          ✓ Slot structure
│   ├── InventoryManager.cs       ✓ Singleton manager
│   └── (more to come)
├── Data/
│   ├── ItemType.cs               ✓ Base item ScriptableObject
│   ├── ItemStack.cs              ✓ Lightweight item stack struct
│   ├── ItemRarity.cs             ✓ Rarity enum
│   ├── ItemCategory.cs           ✓ Category enum
│   ├── EquipmentSlot.cs          ✓ Equipment slot enum
│   └── SlotRestriction.cs        ✓ Slot restriction enum
├── Commands/
│   ├── AddItemCommand.cs         ✓ Add items with undo/redo
│   └── RemoveItemCommand.cs      ✓ Remove items with undo/redo
├── Guards/
│   ├── InventoryGuardContext.cs  ✓ Context for validation
│   ├── CanAddItemGuard.cs        ✓ Validate add operations
│   └── HasSpaceGuard.cs          ✓ Validate space availability
├── Examples/
│   └── InventorySystemExample.cs ✓ Usage examples
└── README.md                     ✓ This file
```

## Example: Testing the System

1. Create a test ItemType:
   - Right-click → Create → Inventory → Item Type
   - Set ItemID: "health_potion"
   - Set Name: "Health Potion"
   - Set MaxStack: 99
   - Set Category: Consumable

2. Create a GameObject with `InventorySystemExample` script
3. Assign the health potion ItemType to the test field
4. Press Play
5. Check the Console for example output

## API Reference

### Inventory Class

```csharp
// Creation
var inv = new Inventory("id", maxSlots, maxWeight);

// Adding items
bool success = inv.TryAddItem(itemStack, out int remaining);

// Removing items
bool success = inv.TryRemoveItem(itemID, quantity, out int removed);

// Queries
int qty = inv.GetItemQuantity(itemID);
bool has = inv.ContainsItem(itemID, quantity);
float weight = inv.GetTotalWeight();
int value = inv.GetTotalValue();
int emptySlots = inv.GetEmptySlotCount();
List<ItemStack> all = inv.GetAllItems();

// Slot access
InventorySlot slot = inv.GetSlot(index);
ItemStack stack = inv.GetStack(index);
bool success = inv.SetStack(index, stack, out string reason);
```

### ItemType ScriptableObject

```csharp
// Create stack from type
ItemStack stack = itemType.CreateStack(quantity);

// Properties
string itemType.ItemID;
string itemType.Name;
ItemCategory itemType.Category;
ItemRarity itemType.Rarity;
int itemType.MaxStack;
float itemType.Weight;
int itemType.Value;
bool itemType.IsStackable;
Color itemType.RarityColor;
```

### ItemStack Struct

```csharp
// Creation
var stack = new ItemStack(itemType, quantity, durability);

// Properties
bool stack.IsEmpty;
bool stack.IsValid;
bool stack.CanStack;
int stack.RemainingSpace;
float stack.TotalWeight;
int stack.TotalValue;

// Operations
stack.TryAddToStack(amount, out remainder);
stack.TryRemoveFromStack(amount, out removed);
stack.TrySplit(amount, out newStack);
bool canMerge = stack.CanMergeWith(otherStack);
```

## Performance Notes

- **Add/Remove**: O(1) average case with Dictionary lookup
- **Memory**: ~24 bytes per ItemStack, ~32 bytes per InventorySlot
- **Caching**: Item locations cached for fast repeated lookups
- **Events**: UI can subscribe to OnSlotChanged for efficient updates

## Integration with Existing Systems

This inventory system integrates with Fantasy Fiefdoms' existing patterns:

- **Singleton Pattern**: InventoryManager extends `Singleton<T>`
- **Command Pattern**: All operations use `ICommand` for undo/redo
- **Guard Pattern**: Validation uses `ITransitionGuard`
- **ScriptableObjects**: ItemType follows `TerrainType` pattern

## Need Help?

- Check `InventorySystemExample.cs` for usage examples
- See `INVENTORY_SYSTEM_PLAN.md` for detailed architecture
- All public APIs have XML documentation

## License

Part of the Fantasy Fiefdoms project.
