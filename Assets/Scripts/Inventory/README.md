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

## Current Status: Phase 5 Complete ✓

### Implemented (Phases 1-5)
- ✅ Core data structures (ItemStack, InventorySlot)
- ✅ Base Inventory class with O(1) lookups
- ✅ ItemType, EquipmentType, & ConsumableType ScriptableObjects
- ✅ InventoryManager Singleton
- ✅ Guard system (CanAdd, HasSpace, CanEquip, CanBuy, CanSell, CanOpenContainer, CanUseItem)
- ✅ Command system (Add, Remove, Move, Equip, Unequip, Buy, Sell, UseItem)
- ✅ Equipment system with slot management and two-handed weapon support
- ✅ Item effect system (StatModifier, Heal, RestoreMana, Passive, Buff, Debuff, DoT, HoT)
- ✅ Complete UI system with drag-and-drop
  - ItemSlotUI, InventoryPanel, EquipmentPanel
  - ItemTooltip with detailed stat display
  - DragDropManager with visual feedback
- ✅ Specialized inventory types
  - MerchantInventory: Buy/sell with pricing and restock
  - QuestInventory: Quest item management with lifecycle tracking
  - ContainerInventory: Locks, keys, lockpicking, and loot generation
- ✅ Consumable system with effects and cooldowns
  - ConsumableType: Potions, scrolls, food with targeting
  - UseItemCommand: Full undo/redo support
  - CooldownTracker: Per-item cooldown management
  - EffectManager: Duration tracking and automatic ticking
  - Advanced effects: Buffs, debuffs, DoT, HoT
- ✅ Example scripts (InventorySystemExample, EquipmentSystemExample, InventoryUIExample, MerchantSystemExample, ConsumableSystemExample)
- ✅ Comprehensive documentation (README, UI_GUIDE, planning doc)

### Coming Next (Phase 6)
- Unique items with random modifiers
- Durability system
- Item quality tiers
- Crafting system

### Future Phases
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

### MerchantInventory (Phase 4)

```csharp
using Inventory.Core;
using Inventory.Commands;

// Create merchant inventory
var merchant = new MerchantInventory(
    inventoryID: "merchant_general",
    maxSlots: 50,
    maxWeight: 500f,
    infiniteStock: false,
    buyPriceMultiplier: 1.2f,  // Player buys at 120% of base value
    sellPriceMultiplier: 0.5f, // Player sells at 50% of base value
    merchantGold: 5000
);

// Buy from merchant
int playerGold = 500;
var buyCmd = new BuyItemCommand(merchant, playerInventory, "health_potion", 3);
if (buyCmd.CanExecute())
{
    buyCmd.Execute();
    playerGold = PlayerPrefs.GetInt("PlayerGold", 0);
}

// Sell to merchant
var sellCmd = new SellItemCommand(merchant, playerInventory, "old_sword", 1);
sellCmd.Execute();

// Setup auto-restock
merchant.AddRestockItem("health_potion", quantity: 5, restockEveryNSeconds: 300f);
merchant.Restock(); // Manual restock
```

### QuestInventory (Phase 4)

```csharp
// Create quest inventory
var questInventory = new QuestInventory("player_quests", maxSlots: 30);

// Start quest
questInventory.StartQuest("find_ancient_artifact");

// Add quest item
ItemStack questItem = itemType.CreateStack(1);
questInventory.AddQuestItem("find_ancient_artifact", questItem);

// Check quest progress
bool hasItem = questInventory.HasQuestItem("find_ancient_artifact", "ancient_key", 1);

// Complete quest (removes quest items)
questInventory.CompleteQuest("find_ancient_artifact", removeItems: true);
```

### ContainerInventory (Phase 4)

```csharp
// Create container (chest, crate, barrel, etc.)
var chest = new ContainerInventory(
    inventoryID: "treasure_chest_01",
    containerType: ContainerType.Chest,
    maxSlots: 20,
    maxWeight: 100f
);

// Lock the container
chest.Lock(requiredKeyID: "golden_key", lockLevel: 3);

// Try to open (checks for key in player inventory)
if (chest.TryOpen(playerInventory, out string reason))
{
    Debug.Log("Chest opened!");
}

// Or try to pick the lock
if (chest.TryPickLock(playerLockpickingSkill: 5, out reason))
{
    Debug.Log("Lock picked successfully!");
}

// Generate random loot
chest.GenerateLoot(lootLevel: 2);

// Auto-despawn settings
chest.SetAutoDespawn(despawnTimeSeconds: 300f, despawnWhenEmpty: true);
```

### ConsumableType & Effects (Phase 5)

```csharp
using Inventory.Core;
using Inventory.Commands;
using Inventory.Effects;

// Setup player with required components
GameObject player = GameObject.Find("Player");
CooldownTracker cooldownTracker = player.AddComponent<CooldownTracker>();
EffectManager effectManager = player.AddComponent<EffectManager>();

// Use a consumable (health potion, buff, etc.)
var useCmd = new UseItemCommand(
    inventory: playerInventory,
    itemID: "health_potion",
    user: player,
    target: player, // Can target self or others
    cooldownTracker: cooldownTracker
);

if (useCmd.CanExecute())
{
    useCmd.Execute();
    Debug.Log("Used health potion!");
}

// Check cooldown status
bool isReady = !cooldownTracker.IsOnCooldown("health_potion");
float remaining = cooldownTracker.GetRemainingCooldown("health_potion");

// Check active effects
var activeEffects = effectManager.GetAllActiveEffects();
foreach (var effect in activeEffects)
{
    Debug.Log($"{effect.Description}: {effect.RemainingDuration:F1}s remaining");
}

// Validate before using
var guard = new CanUseItemGuard(playerInventory, "health_potion", player, cooldownTracker);
var result = guard.Evaluate(null);
if (!result.Success)
{
    Debug.LogWarning(result.Reason);
}

// Create consumable with effects (in Unity Editor)
/*
ConsumableType healthPotion = ScriptableObject.CreateInstance<ConsumableType>();
healthPotion.ItemID = "health_potion";
healthPotion.Name = "Health Potion";
healthPotion.CooldownSeconds = 5f;
healthPotion.ConsumeOnUse = true;

// Add heal effect
var healEffect = new ItemEffectData
{
    EffectType = ItemEffectType.Heal,
    Value = 50f,
    Description = "Restores 50 health"
};
healthPotion.ConsumableEffects.Add(healEffect);

// Add HoT effect
var hotEffect = new ItemEffectData
{
    EffectType = ItemEffectType.HealOverTime,
    Value = 10f,
    Duration = 5f,
    Description = "10 health per second for 5 seconds"
};
healthPotion.ConsumableEffects.Add(hotEffect);
*/
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

- Check example scripts for usage patterns:
  - `InventorySystemExample.cs` - Basic inventory operations
  - `EquipmentSystemExample.cs` - Equipment and effects
  - `InventoryUIExample.cs` - UI integration
  - `MerchantSystemExample.cs` - Trading and merchants
  - `ConsumableSystemExample.cs` - Consumables, effects, and cooldowns
- See `INVENTORY_SYSTEM_PLAN.md` for detailed architecture
- See `UI_GUIDE.md` for complete UI setup instructions
- All public APIs have XML documentation

## License

Part of the Fantasy Fiefdoms project.
