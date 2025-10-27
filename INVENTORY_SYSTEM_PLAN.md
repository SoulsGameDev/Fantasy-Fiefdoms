# Item & Inventory System - Implementation Plan

**Project:** Fantasy Fiefdoms
**Branch:** claude/plan-item-system-011CUXmz3FDqgbbhogmX9GKW
**Created:** 2025-10-27
**Status:** Phase 5 Complete ✓

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

### Phase 2: Equipment System ✅ COMPLETE
**Goal:** Equip/unequip system with effects

- [x] Implement EquipmentType ScriptableObject
- [x] Implement EquipmentInventory with slots
- [x] Implement EquipItemCommand
- [x] Implement UnequipItemCommand
- [x] Create CanEquipGuard
- [x] Implement IItemEffect interface
- [x] Implement StatModifierEffect
- [x] Implement BasicEffects (HealEffect, RestoreManaEffect, PassiveEffect)
- [x] Create ItemEffectFactory
- [x] Create StatModifierStorage component
- [x] Implement MoveItemCommand
- [x] Create EquipmentSystemExample

**Deliverable:** Can equip items and apply stat effects ✓

**Completed:** 2025-10-27

---

### Phase 3: UI System ✅ COMPLETE
**Goal:** Visual inventory management

- [x] Implement ItemSlotUI component (340+ lines)
- [x] Implement DragDropManager Singleton
- [x] Implement ItemTooltip display (320+ lines)
- [x] Implement InventoryPanel (350+ lines)
- [x] Implement EquipmentPanel (280+ lines)
- [x] Implement EquipmentSlotUI component
- [x] Wire UI to inventory events
- [x] Add visual feedback (hover, selection, rarity borders)
- [x] Create InventoryUIExample with keyboard shortcuts
- [x] Create comprehensive UI_GUIDE.md (680+ lines)

**Deliverable:** Fully functional inventory UI ✓

**Completed:** 2025-10-27

---

### Phase 4: Specialized Inventories ✅ COMPLETE
**Goal:** Merchant, quest, container inventories

- [x] Implement MerchantInventory (400+ lines)
  - Buy/sell pricing system with multipliers
  - Infinite stock mode
  - Auto-restock functionality
  - Merchant gold management
- [x] Implement QuestInventory (350+ lines)
  - Quest lifecycle tracking
  - Quest-item mapping
  - Auto-removal on quest completion
- [x] Implement ContainerInventory (400+ lines)
  - Lock/unlock system with keys
  - Lockpicking with skill checks
  - Loot generation based on container type
  - Auto-despawn settings
- [x] Create BuyItemCommand with full undo/redo
- [x] Create SellItemCommand with full undo/redo
- [x] Create TradeGuards (CanBuyGuard, CanSellGuard, CanOpenContainerGuard)
- [x] Create MerchantSystemExample with keyboard shortcuts

**Deliverable:** All inventory types functional ✓

**Completed:** 2025-10-27

---

### Phase 5: Consumables & Effects ✅ COMPLETE
**Goal:** Usable items and advanced effects

- [x] Implement ConsumableType ScriptableObject
  - Cast time, cooldown, combat restrictions
  - Target types (self, ally, enemy, ground)
  - Uses per stack, consume on use
  - Effect descriptions and validation
- [x] Implement UseItemCommand with full undo/redo
  - Cooldown integration
  - Effect application
  - Sound and animation triggers
- [x] Create AdvancedEffects system
  - BuffEffect: Temporary stat increases
  - DebuffEffect: Temporary stat decreases
  - DamageOverTimeEffect: Periodic damage (DoT)
  - HealOverTimeEffect: Periodic healing (HoT)
- [x] Implement CooldownTracker component
  - Per-item cooldown tracking
  - Event system for cooldown start/expire
  - Progress queries and reduction support
- [x] Implement EffectManager component
  - Active effect tracking with duration
  - Automatic effect ticking (DoT/HoT)
  - Effect stacking and refresh logic
  - Integration with ICharacterStats
- [x] Create ConsumableGuards
  - CanUseItemGuard: Validates item usage
  - CanApplyEffectGuard: Validates effect application
  - IsNotOnCooldownGuard: Cooldown validation
- [x] Create ConsumableSystemExample with comprehensive testing

**Deliverable:** Can use potions, scrolls, and consumables ✓

**Completed:** 2025-10-27

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
- **Completed:** 49
- **In Progress:** 0
- **Remaining:** 18
- **Progress:** 73.1%

### Current Sprint
**Focus:** Phase 6 - Advanced Features
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
- ✅ 2025-10-27: **Phase 2 Complete** - Equipment system implemented
  - EquipmentType with comprehensive stats
  - EquipmentInventory with slot management
  - Effect system (IItemEffect, StatModifierEffect, BasicEffects)
  - ItemEffectFactory and StatModifierStorage
  - Commands (EquipItemCommand, UnequipItemCommand, MoveItemCommand)
  - Guards (CanEquipGuard, CanUnequipGuard)
- ✅ 2025-10-27: **Phase 3 Complete** - UI system implemented
  - ItemSlotUI with drag-and-drop (IPointerHandler interfaces)
  - DragDropManager Singleton with visual feedback
  - ItemTooltip with detailed stat display
  - InventoryPanel with auto slot creation
  - EquipmentPanel with fixed equipment slots
  - Complete UI_GUIDE.md documentation
- ✅ 2025-10-27: **Phase 4 Complete** - Specialized inventories implemented
  - MerchantInventory with buy/sell/restock
  - QuestInventory with quest lifecycle tracking
  - ContainerInventory with locks and loot generation
  - Trade commands (BuyItemCommand, SellItemCommand)
  - Trade guards and merchant example script
- ✅ 2025-10-27: **Phase 5 Complete** - Consumables and effects system implemented
  - ConsumableType with targeting and cooldowns
  - UseItemCommand with full undo/redo
  - Advanced effects (Buff, Debuff, DoT, HoT)
  - CooldownTracker for per-item cooldown management
  - EffectManager for duration tracking and ticking
  - Consumable guards and comprehensive example

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
- **2025-10-27:** Used price multipliers for merchant flexibility (buy/sell rates)
- **2025-10-27:** Implemented infinite stock mode for essential merchants
- **2025-10-27:** Added quest-item mapping to track which items belong to which quests
- **2025-10-27:** Container system supports multiple types (Chest, Crate, Barrel, Corpse, etc.)
- **2025-10-27:** Lock system supports both key-based and skill-based unlocking
- **2025-10-27:** All trade operations use Command pattern for full transaction history
- **2025-10-27:** EffectManager uses Update loop for automatic effect ticking and cleanup
- **2025-10-27:** CooldownTracker tracks per-item cooldowns with event system for UI updates
- **2025-10-27:** ConsumableType supports targeting modes (self, ally, enemy, ground)
- **2025-10-27:** Effects can stack or refresh based on configuration
- **2025-10-27:** ActiveEffect class tracks duration, tick rate, and expiration automatically

### Open Questions
- None currently

---

## Version History

### v0.6.0 - Phase 5 Complete (2025-10-27)
- ✅ Consumables and effects system implemented
- ✅ 7 new classes created (ConsumableType, UseItemCommand, AdvancedEffects, CooldownTracker, EffectManager, ConsumableGuards, ConsumableSystemExample)
- ✅ Full consumable system with cooldowns and duration tracking
- ✅ Advanced effect types (Buff, Debuff, DoT, HoT)
- ✅ Effect ticking and automatic cleanup
- ✅ Comprehensive example with real-time GUI display
- Ready for Phase 6 (Advanced Features)

### v0.5.0 - Phase 4 Complete (2025-10-27)
- ✅ Specialized inventory types implemented
- ✅ 10 additional classes created (MerchantInventory, QuestInventory, ContainerInventory, etc.)
- ✅ Full merchant trading system with undo/redo
- ✅ Quest item management and container system
- ✅ MerchantSystemExample with comprehensive testing
- Ready for Phase 5 (Consumables & Effects)

### v0.4.0 - Phase 3 Complete (2025-10-27)
- ✅ Complete UI system implemented
- ✅ 9 UI components created (ItemSlotUI, DragDropManager, ItemTooltip, etc.)
- ✅ Full drag-and-drop functionality with visual feedback
- ✅ Comprehensive UI_GUIDE.md (680+ lines)
- Ready for Phase 4 (Specialized Inventories)

### v0.3.0 - Phase 2 Complete (2025-10-27)
- ✅ Equipment system implemented
- ✅ 12 components created (EquipmentType, EquipmentInventory, effect system, etc.)
- ✅ Full stat modifier and effect system
- ✅ Equipment commands and guards
- Ready for Phase 3 (UI System)

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
**Next Review:** After Phase 6 completion
