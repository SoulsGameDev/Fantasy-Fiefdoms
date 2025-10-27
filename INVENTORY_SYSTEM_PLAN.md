# Item & Inventory System - Implementation Plan

**Project:** Fantasy Fiefdoms
**Branch:** claude/plan-item-system-011CUXmz3FDqgbbhogmX9GKW
**Created:** 2025-10-27
**Status:** Phase 1 Complete ✓

---

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Implementation Phases](#implementation-phases)
4. [Progress Tracking](#progress-tracking)
5. [Technical Specifications](#technical-specifications)
6. [Integration Points](#integration-points)
7. [Testing Strategy](#testing-strategy)

---

## Overview

### Goals
- Create a modular, reusable item and inventory system
- Support multiple inventory types: bags, equipment, merchants, quest items, containers
- Optimize for performance and memory efficiency
- Integrate with existing Command, Guard, and Singleton patterns
- Enable undo/redo for all inventory operations
- Design for easy extraction to other projects

### Key Features
- Stackable and non-stackable items
- Equipment system with slot restrictions
- Item effects (stat modifiers, consumables, passives)
- Weight and capacity management
- Merchant buy/sell system
- Quest item restrictions
- Drag-and-drop UI
- Save/load serialization
- Unity Jobs & Burst ready architecture

---

## Architecture

### Layer Separation

```
┌─────────────────────────────────────┐
│   UI Layer (Unity MonoBehaviours)   │
│   - InventoryPanel, ItemSlotUI      │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Command Layer (ICommand)          │
│   - AddItem, RemoveItem, Equip      │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Core Logic (Pure C#)              │
│   - Inventory, ItemStack            │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Data Layer (ScriptableObjects)    │
│   - ItemType, EquipmentType         │
└─────────────────────────────────────┘
```

### Core Components

#### 1. Item Data (ScriptableObjects)
- **ItemType**: Base item definition
- **EquipmentType**: Weapons, armor, accessories
- **ConsumableType**: Potions, food, scrolls
- **QuestItemType**: Quest-specific items
- **MaterialType**: Crafting materials

#### 2. Runtime Instances
- **ItemStack** (struct): Lightweight for stackable items
- **ItemInstance** (class): Rich data for unique items with modifiers

#### 3. Inventory Containers
- **Inventory** (base): Core slot-based storage
- **PlayerInventory**: Standard bag with limits
- **EquipmentInventory**: Fixed equipment slots
- **MerchantInventory**: Buy/sell with pricing
- **QuestInventory**: Quest items only
- **ContainerInventory**: World chests/loot

#### 4. Command Integration
All inventory operations use ICommand for undo/redo:
- AddItemCommand
- RemoveItemCommand
- MoveItemCommand
- EquipItemCommand
- UnequipItemCommand
- UseItemCommand
- BuyItemCommand
- SellItemCommand

#### 5. Guard Validation
- CanAddItemGuard (space, weight)
- CanEquipGuard (requirements, restrictions)
- CanConsumeGuard (cooldowns, combat)
- CanSellGuard (not equipped, not quest item)
- CanDropGuard (not quest, not cursed)

#### 6. Effect System
- **IItemEffect** interface
- **StatModifierEffect**: +10 STR, +5% crit
- **ConsumableEffect**: Heal, buff, debuff
- **PassiveEffect**: Ongoing aura effects

---

## Implementation Phases

### Phase 1: Core Foundation ✅ COMPLETE
**Goal:** Basic item and inventory functionality

- [x] Create folder structure
- [x] Implement ItemType ScriptableObject
- [x] Implement ItemStack struct
- [x] Implement base Inventory class
- [x] Implement InventoryManager Singleton
- [x] Create CanAddItemGuard
- [x] Create HasSpaceGuard
- [x] Create AddItemCommand
- [x] Create RemoveItemCommand
- [x] Create example test script
- [x] Create README documentation

**Deliverable:** Can add/remove items from inventory with validation ✓

**Completed:** 2025-10-27

---

### Phase 2: Equipment System 📋 PENDING
**Goal:** Equip/unequip system with effects

- [ ] Implement EquipmentType ScriptableObject
- [ ] Implement EquipmentInventory with slots
- [ ] Implement EquipEquipCommand
- [ ] Implement UnequipItemCommand
- [ ] Create CanEquipGuard
- [ ] Implement IItemEffect interface
- [ ] Implement StatModifierEffect
- [ ] Wire effect application on equip/unequip

**Deliverable:** Can equip items and apply stat effects

---

### Phase 3: UI System 📋 PENDING
**Goal:** Visual inventory management

- [ ] Create InventoryPanel prefab
- [ ] Implement ItemSlotUI component
- [ ] Implement drag-and-drop handler
- [ ] Implement ItemTooltip display
- [ ] Create EquipmentPanel prefab
- [ ] Wire UI to inventory events
- [ ] Add visual feedback (hover, selection)

**Deliverable:** Fully functional inventory UI

---

### Phase 4: Specialized Inventories 📋 PENDING
**Goal:** Merchant, quest, container inventories

- [ ] Implement PlayerInventory with weight
- [ ] Implement MerchantInventory
- [ ] Create BuyItemCommand
- [ ] Create SellItemCommand
- [ ] Implement ContainerInventory
- [ ] Implement QuestInventory with restrictions
- [ ] Add merchant UI panel

**Deliverable:** All inventory types functional

---

### Phase 5: Consumables & Effects 📋 PENDING
**Goal:** Usable items and advanced effects

- [ ] Implement ConsumableType ScriptableObject
- [ ] Implement UseItemCommand
- [ ] Create ConsumableEffect (healing, buffs)
- [ ] Create PassiveEffect (auras)
- [ ] Implement cooldown system
- [ ] Add effect duration tracking
- [ ] Create effect UI indicators

**Deliverable:** Can use potions, scrolls, and consumables

---

### Phase 6: Advanced Features 📋 PENDING
**Goal:** Unique items, durability, modifiers

- [ ] Implement ItemInstance for unique items
- [ ] Add item durability system
- [ ] Add item modifier system (enchantments)
- [ ] Implement crafting system
- [ ] Add rarity tiers (common → legendary)
- [ ] Implement auto-sort feature
- [ ] Implement quick-stack feature

**Deliverable:** Full RPG inventory feature set

---

### Phase 7: Optimization & Polish 📋 PENDING
**Goal:** Performance, serialization, documentation

- [ ] Implement UI object pooling
- [ ] Add Dictionary quick-lookup optimization
- [ ] Profile and optimize hot paths
- [ ] Implement save/load serialization
- [ ] Add comprehensive XML documentation
- [ ] Create example items (10+ types)
- [ ] Write user guide documentation
- [ ] Create video tutorial

**Deliverable:** Production-ready system

---

## Progress Tracking

### Overall Progress
- **Total Tasks:** 67
- **Completed:** 11
- **In Progress:** 0
- **Remaining:** 56
- **Progress:** 16.4%

### Current Sprint
**Focus:** Phase 2 - Equipment System
**Started:** TBD
**Target Completion:** TBD

### Completed Milestones
- ✅ 2025-10-27: Architecture design completed
- ✅ 2025-10-27: Planning document created
- ✅ 2025-10-27: **Phase 1 Complete** - Core foundation implemented
  - Core data structures (ItemStack, InventorySlot, ItemType)
  - Base Inventory class with O(1) dictionary lookups
  - InventoryManager Singleton
  - Guard system (CanAddItemGuard, HasSpaceGuard)
  - Command system (AddItemCommand, RemoveItemCommand)
  - Example script and comprehensive documentation

### Blockers
None currently

---

## Technical Specifications

### File Structure

```
Assets/Scripts/Inventory/
├── Core/
│   ├── Inventory.cs                 # Base inventory container
│   ├── InventorySlot.cs            # Slot structure
│   ├── InventoryManager.cs         # Singleton manager
│   ├── PlayerInventory.cs          # Player bag
│   ├── EquipmentInventory.cs       # Equipment slots
│   ├── MerchantInventory.cs        # Merchant shop
│   ├── QuestInventory.cs           # Quest items
│   └── ContainerInventory.cs       # World containers
├── Data/
│   ├── ItemType.cs                 # Base item ScriptableObject
│   ├── EquipmentType.cs            # Equipment items
│   ├── ConsumableType.cs           # Consumable items
│   ├── QuestItemType.cs            # Quest items
│   ├── MaterialType.cs             # Crafting materials
│   ├── ItemRarity.cs               # Rarity enum
│   ├── EquipmentSlot.cs            # Slot enum
│   ├── ItemCategory.cs             # Category enum
│   ├── ItemStack.cs                # Stack struct
│   └── ItemInstance.cs             # Unique item class
├── Effects/
│   ├── IItemEffect.cs              # Effect interface
│   ├── StatModifierEffect.cs       # Stat modifications
│   ├── ConsumableEffect.cs         # Consumable effects
│   └── PassiveEffect.cs            # Passive auras
├── Commands/
│   ├── AddItemCommand.cs
│   ├── RemoveItemCommand.cs
│   ├── MoveItemCommand.cs
│   ├── EquipItemCommand.cs
│   ├── UnequipItemCommand.cs
│   ├── UseItemCommand.cs
│   ├── BuyItemCommand.cs
│   ├── SellItemCommand.cs
│   ├── DropItemCommand.cs
│   ├── SplitStackCommand.cs
│   └── MergeStackCommand.cs
├── Guards/
│   ├── CanAddItemGuard.cs
│   ├── HasSpaceGuard.cs
│   ├── CanEquipGuard.cs
│   ├── CanConsumeGuard.cs
│   ├── CanSellGuard.cs
│   ├── CanDropGuard.cs
│   └── InventoryGuardContext.cs
├── UI/
│   ├── InventoryPanel.cs
│   ├── ItemSlotUI.cs
│   ├── EquipmentPanel.cs
│   ├── MerchantPanel.cs
│   ├── ItemTooltip.cs
│   ├── DragDropHandler.cs
│   └── InventoryUIEvents.cs
└── Serialization/
    ├── InventorySaveData.cs
    ├── ItemStackData.cs
    └── InventorySerializer.cs
```

### Data Structures

#### ItemStack (Struct - 24 bytes)
```csharp
public struct ItemStack
{
    public ItemType Type;        // 8 bytes (reference)
    public int Quantity;         // 4 bytes
    public int Durability;       // 4 bytes (-1 if N/A)
    public Guid InstanceID;      // 16 bytes (for unique tracking)
}
```

#### InventorySlot (Struct - 32 bytes)
```csharp
public struct InventorySlot
{
    public ItemStack Stack;              // 24 bytes
    public SlotRestriction Restriction;  // 4 bytes (enum)
    public bool IsLocked;                // 1 byte
    // 3 bytes padding
}
```

### Performance Targets
- **Add/Remove Item:** < 1ms (O(1) with dictionary lookup)
- **Equip Item:** < 2ms (includes effect application)
- **UI Update:** < 16ms per frame (60 FPS)
- **Save/Load:** < 100ms for 1000 items
- **Memory:** < 1KB per inventory (20 slots)

### Optimization Strategies
1. **Struct-based data:** ItemStack as struct (value type)
2. **Object pooling:** UI slot reuse
3. **Dictionary lookup:** O(1) item finding
4. **Lazy evaluation:** Calculate totals on-demand
5. **Batch UI updates:** Once per frame
6. **Sparse storage:** Only allocate used slots
7. **Shared references:** ItemType ScriptableObjects

---

## Integration Points

### Existing Systems

#### 1. Command System
**Files:** `/Assets/Scripts/Commands/`
- All inventory operations extend `ICommand`
- Use `CommandHistory.Instance.ExecuteCommand()`
- Automatic undo/redo support

#### 2. Guard System
**Files:** `/Assets/Scripts/Guards/`
- Implement `ITransitionGuard` for validation
- Return `GuardResult(success, reason)`
- Called before command execution

#### 3. Singleton Pattern
**File:** `/Assets/Scripts/Singleton.cs`
- InventoryManager extends `Singleton<InventoryManager>`
- Thread-safe, DontDestroyOnLoad
- Global access via `InventoryManager.Instance`

#### 4. ResourceManager
**File:** `/Assets/Scripts/ResourceManager.cs`
- Add `public List<ItemType> ItemTypes`
- Register all items at startup
- Centralized item database

#### 5. UI System
**Files:** `/Assets/Scripts/UI/`
- Follow `ParameterDisplay.cs` pattern
- Use TextMeshPro for text
- MonoBehaviour-based components

---

## Testing Strategy

### Unit Tests
- Item stacking logic
- Inventory add/remove operations
- Equipment slot restrictions
- Guard validation logic
- Effect application/removal

### Integration Tests
- Command execution with guards
- UI event handling
- Save/load serialization
- Cross-inventory item movement

### Performance Tests
- 1000 item operations benchmark
- Memory allocation profiling
- UI rendering performance
- Serialization speed

### User Acceptance Tests
- Drag-and-drop functionality
- Equipment visual feedback
- Merchant buy/sell flow
- Quest item restrictions

---

## Risk Management

### Identified Risks
1. **Performance:** Large inventories (1000+ items)
   - *Mitigation:* Dictionary lookups, object pooling

2. **Serialization:** Complex item instances
   - *Mitigation:* Custom serialization, data compression

3. **UI Scalability:** Many open inventories
   - *Mitigation:* Object pooling, lazy instantiation

4. **Memory Leaks:** Event subscriptions
   - *Mitigation:* Proper unsubscribe, weak references

---

## Future Enhancements

### Post-Launch Features
- Item sets (bonus for wearing full set)
- Transmog system (visual customization)
- Item augmentation (sockets, gems)
- Storage expansion (bank, vault)
- Mail system (player-to-player trading)
- Auction house integration
- Item comparison UI
- Smart loot filtering
- Favorite items marking
- Item recycling/salvaging

### Unity Jobs Integration
Following the pathfinding Jobs pattern:
- Parallel inventory searches
- Batch effect calculations
- Optimized serialization
- Multi-threaded sorting

---

## Documentation

### Required Documentation
- [ ] XML doc comments on all public APIs
- [ ] User guide (how to use the system)
- [ ] Developer guide (how to extend)
- [ ] Example scripts
- [ ] Video tutorials
- [ ] API reference

### Example Items to Create
- [ ] Iron Sword (weapon)
- [ ] Leather Armor (armor)
- [ ] Health Potion (consumable)
- [ ] Quest Token (quest item)
- [ ] Gold Coins (currency)
- [ ] Wood (material)
- [ ] Magic Ring (accessory)
- [ ] Legendary Staff (unique item)
- [ ] Food Ration (consumable)
- [ ] Crafting Blueprint (special)

---

## Notes

### Design Decisions Log
- **2025-10-27:** Chose struct for ItemStack to optimize memory (24 bytes vs 40+ for class)
- **2025-10-27:** Separated ItemStack and ItemInstance for flexibility
- **2025-10-27:** Integrated with existing Command pattern for undo/redo
- **2025-10-27:** ScriptableObject pattern follows existing TerrainType
- **2025-10-27:** Dictionary-based caching for O(1) item lookups
- **2025-10-27:** Created comprehensive examples and documentation

### Open Questions
- None currently

---

## Version History

### v0.2.0 - Phase 1 Complete (2025-10-27)
- ✅ Core foundation implemented
- ✅ 11 core classes/structs created
- ✅ Full documentation and examples
- Ready for Phase 2 (Equipment System)

### v0.1.0 - Planning Phase (2025-10-27)
- Initial architecture design
- Planning document created
- Implementation phases defined

---

**Last Updated:** 2025-10-27
**Next Review:** After Phase 2 completion
