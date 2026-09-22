# Crafting & Enchant System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement MazeMath progression as material collection, guided/pattern crafting, equipment abilities, Knowledge XP, and reversible enchant choices that unlock new maze routes without grind or destructive failure.

**Architecture:** Inventory, crafting, equipment, and enchant logic are pure C# services backed by ScriptableObject definitions. Maze checks capabilities through `IEquipmentService.HasAbility`; learning content grants `LearningReward` through `IRewardService`. Workshop UI is a presentation layer over those services.

**Tech Stack:** Unity 2022.3.62f2 LTS, C#, ScriptableObjects, Unity Test Framework, project UI stack.

**Spec:** `Docs/Design/02_CRAFTING_ENCHANT_DESIGN.md`

## Global Constraints

- V1 materials: Scrap, Iron Plate, Gear, Energy Crystal, Rare Core.
- V1 equipment slots: Arm Tool, Utility Tool, Mobility, Defense, Sensor.
- V1 equipment: Mining Arm, Power Wrench, Jump Booster, Energy Shield, Explorer Sensor.
- V1 equipment has Tier 1 and Tier 2 only.
- Permanent durability loss and equipment destruction are prohibited.
- Pattern crafting failures never consume materials.
- Required progression recipes do not depend on Rare Core or random drops.
- Purchased enchants can be re-equipped for free.
- Required materials available in a normal chapter must total at least 120% of mandatory recipe cost.
- Inventory overflow must preserve rewards through PendingRewardQueue.

## Review Focus

1. Rapid repeated Craft input must not consume materials twice or duplicate the result.
2. Inventory-full reward grants must preserve the exact reward and deliver it later.
3. Equipment swap must remove previous slot ability immediately and expose only currently active abilities.
4. Re-equipping an already unlocked enchant must cost 0 Knowledge XP.
5. Save/reload during Workshop state must not lose consumed material or duplicate crafted equipment.

---

### Task 1: Add item, inventory, and item-stack domain model

**Files:**
- Create: `Assets/MazeMath/Scripts/Inventory/Data/ItemDefinition.cs`
- Create: `Assets/MazeMath/Scripts/Inventory/Runtime/ItemStack.cs`
- Create: `Assets/MazeMath/Scripts/Inventory/Runtime/IInventoryService.cs`
- Create: `Assets/MazeMath/Scripts/Inventory/Runtime/InventoryService.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Inventory/InventoryServiceTests.cs`

**Interfaces:**
- Produces `GetCount`, `Has`, `TryAdd`, `TryRemove`, `GetItems`.
- Consumes item definitions.

- [ ] **Step 1: Write failing stack-merge tests**

Assert adding 80 Scrap then 30 Scrap with MaxStack=99 yields stacks 99 and 11.

- [ ] **Step 2: Implement ItemStack and InventoryService**

Material capacity: 12 slots.
Equipment storage: separate service/task.
Quest items: not counted against 12 material slots.

- [ ] **Step 3: Add remove-atomicity test**

TryRemove count greater than owned must return false and leave inventory unchanged.

- [ ] **Step 4: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Inventory Assets/MazeMath/Tests/EditMode/Inventory
git commit -m "feat: add inventory service"
~~~

---

### Task 2: Add pending reward queue and reward service

**Files:**
- Create: `Assets/MazeMath/Scripts/Rewards/RewardService.cs`
- Create: `Assets/MazeMath/Scripts/Rewards/PendingRewardQueue.cs`
- Create: `Assets/MazeMath/Scripts/Rewards/RewardGrantResult.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Rewards/RewardServiceTests.cs`

**Interfaces:**
- Implements `IRewardService` defined by Puzzle/Question plan.
- Consumes `IInventoryService` and Knowledge XP service added later.

- [ ] **Step 1: Write failing overflow-preservation test**

Fill all material slots with distinct full stacks, grant a new material reward, assert:
- inventory unchanged
- PendingRewardQueue contains exact item/count
- grant result reports Pending

- [ ] **Step 2: Implement pending reward queue**

Queue entries include stable reward id to prevent duplicate delivery.

- [ ] **Step 3: Write failing duplicate-grant test**

Grant same RewardSource unique id twice; second grant must be ignored.

- [ ] **Step 4: Implement idempotent RewardService**

- [ ] **Step 5: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Rewards Assets/MazeMath/Tests/EditMode/Rewards
git commit -m "feat: preserve and deduplicate learning rewards"
~~~

---

### Task 3: Add equipment definitions, slots, and ability lookup

**Files:**
- Create: `Assets/MazeMath/Scripts/Inventory/Data/EquipmentDefinition.cs`
- Create: `Assets/MazeMath/Scripts/Equipment/Runtime/IEquipmentService.cs`
- Create: `Assets/MazeMath/Scripts/Equipment/Runtime/EquipmentService.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Equipment/EquipmentServiceTests.cs`

**Interfaces:**
- Produces:
  - `GetEquipped(EquipmentSlot slot)`
  - `Equip(string equipmentId)`
  - `HasAbility(string abilityId)`
  - `GetEquipmentTier(string equipmentId)`
- Consumes equipment definitions.

- [ ] **Step 1: Write failing slot replacement test**

Equip two Sensor-slot items; assert second is equipped and first slot ability is no longer active.

- [ ] **Step 2: Implement slot model and equipment ownership**

- [ ] **Step 3: Write ability lookup tests**

Assert base tier ability and active enchant modifier are exposed; unequipped equipment ability is not exposed unless defined as passive.

- [ ] **Step 4: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Inventory/Data/EquipmentDefinition.cs Assets/MazeMath/Scripts/Equipment Assets/MazeMath/Tests/EditMode/Equipment
git commit -m "feat: add equipment slots and abilities"
~~~

---

### Task 4: Add guided recipe model and transactional crafting

**Files:**
- Create: `Assets/MazeMath/Scripts/Crafting/Data/RecipeDefinition.cs`
- Create: `Assets/MazeMath/Scripts/Crafting/Runtime/ICraftingService.cs`
- Create: `Assets/MazeMath/Scripts/Crafting/Runtime/CraftingService.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Crafting/CraftingServiceTests.cs`

**Interfaces:**
- Produces `CanCraft`, `Craft`.
- Consumes inventory and unlocked recipe set.

- [ ] **Step 1: Write failing missing-material test**

Assert Craft returns MissingMaterial and inventory is unchanged.

- [ ] **Step 2: Write failing success transaction test**

Inventory has exact materials. Craft once. Assert:
- each ingredient removed exactly once
- result added exactly once
- second immediate craft fails if materials insufficient

- [ ] **Step 3: Implement transactional CraftingService**

Order:
validate recipe → unlock → inventory snapshot → ingredient availability → result capacity → remove → add → rollback on failure.

- [ ] **Step 4: Add rapid duplicate-call test**

Two calls without enough material for two results must yield one Success, one MissingMaterial.

- [ ] **Step 5: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Crafting Assets/MazeMath/Tests/EditMode/Crafting
git commit -m "feat: add transactional guided crafting"
~~~

---

### Task 5: Add Workshop and guided crafting UI

**Files:**
- Create: `Assets/MazeMath/Scripts/Crafting/Runtime/WorkshopRuntime.cs`
- Create: `Assets/MazeMath/Scripts/UI/Crafting/WorkshopView.cs`
- Create: `Assets/MazeMath/Scripts/UI/Crafting/GuidedRecipeView.cs`
- Create: `Assets/MazeMath/Prefabs/UI/Workshop.prefab`
- Create: `Assets/MazeMath/Tests/PlayMode/Crafting/WorkshopTests.cs`

**Interfaces:**
- Produces Craft / Upgrade / Enchant / Bag tabs.
- Consumes services only; UI does not mutate inventory directly.

- [ ] **Step 1: Write failing touch-only crafting test**

Open Workshop, select Power Wrench recipe, press Craft using UI callbacks; assert service receives one request and result panel updates.

- [ ] **Step 2: Implement Workshop tab controller and guided recipe presenter**

- [ ] **Step 3: Add unavailable-recipe UI test**

Missing material disables craft button and displays each current/required count.

- [ ] **Step 4: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Crafting Assets/MazeMath/Scripts/UI/Crafting Assets/MazeMath/Prefabs/UI/Workshop.prefab Assets/MazeMath/Tests/PlayMode/Crafting
git commit -m "feat: add workshop guided crafting UI"
~~~

---

### Task 6: Add Tier 1 equipment content and Maze equipment gates

**Files:**
- Create: `Assets/MazeMath/ScriptableObjects/Equipment/MiningArm_T1.asset`
- Create: `Assets/MazeMath/ScriptableObjects/Equipment/PowerWrench_T1.asset`
- Create: `Assets/MazeMath/ScriptableObjects/Equipment/JumpBooster_T1.asset`
- Create: `Assets/MazeMath/ScriptableObjects/Equipment/EnergyShield_T1.asset`
- Create: `Assets/MazeMath/ScriptableObjects/Equipment/ExplorerSensor_T1.asset`
- Create: `Assets/MazeMath/Scripts/Equipment/Runtime/EquipmentGateCapabilityAdapter.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Integration/EquipmentGateIntegrationTests.cs`

**Interfaces:**
- Consumes `IMazeService` gate requirement AbilityId.
- Produces capability lookup from `IEquipmentService`.

- [ ] **Step 1: Write failing cracked-wall integration test**

Without `ability.break.cracked_wall`, gate traversal false. Equip Mining Arm T1, traversal true.

- [ ] **Step 2: Author five Tier 1 equipment definitions**

Ability IDs:
- `ability.break.cracked_wall`
- `ability.repair.basic_machine`
- `ability.jump.high_platform`
- `ability.shield.hazard`
- `ability.sensor.interaction`

- [ ] **Step 3: Implement integration adapter**

- [ ] **Step 4: Run tests and commit**

~~~bash
git add Assets/MazeMath/ScriptableObjects/Equipment Assets/MazeMath/Scripts/Equipment Assets/MazeMath/Tests/PlayMode/Integration
git commit -m "feat: connect equipment abilities to maze gates"
~~~

---

### Task 7: Add pattern crafting and recipe matcher

**Files:**
- Create: `Assets/MazeMath/Scripts/Crafting/Runtime/RecipeMatcher.cs`
- Create: `Assets/MazeMath/Scripts/UI/Crafting/PatternCraftingView.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Crafting/RecipeMatcherTests.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Crafting/PatternCraftingViewTests.cs`

**Interfaces:**
- Produces `MatchesPattern(recipeId, gridItemIds)`.
- Consumes 9-slot grid data.

- [ ] **Step 1: Write failing exact-pattern tests**

Correct 3x3 pattern returns true; one swapped material returns false.

- [ ] **Step 2: Implement RecipeMatcher**

Empty slots are explicit null/empty tokens.
Rotation/mirroring are false by default unless recipe definition enables them.

- [ ] **Step 3: Write failed-pattern material-preservation test**

Submit invalid arrangement; inventory unchanged.

- [ ] **Step 4: Implement tap-to-place and drag-to-place UI**

Both interaction modes must write through one grid-state controller.

- [ ] **Step 5: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Crafting Assets/MazeMath/Scripts/UI/Crafting Assets/MazeMath/Tests
git commit -m "feat: add 3x3 pattern crafting"
~~~

---

### Task 8: Add Knowledge XP and enchant definitions

**Files:**
- Create: `Assets/MazeMath/Scripts/Enchant/Data/EnchantDefinition.cs`
- Create: `Assets/MazeMath/Scripts/Enchant/Runtime/KnowledgeXpService.cs`
- Create: `Assets/MazeMath/Scripts/Enchant/Runtime/EnchantService.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Enchant/EnchantServiceTests.cs`

**Interfaces:**
- Produces:
  - `AddXp(int amount)`
  - `CanUnlock(string enchantId)`
  - `Unlock(string enchantId)`
  - `EquipEnchant(string equipmentId, string enchantId)`
- Consumes equipment service.

- [ ] **Step 1: Write failing XP insufficient test**

Attempt unlock with insufficient XP; result false and XP unchanged.

- [ ] **Step 2: Implement KnowledgeXpService**

XP never goes below zero.

- [ ] **Step 3: Write failing free re-equip test**

Unlock A once, switch to unlocked B, switch back to A. Assert second A equip costs zero XP.

- [ ] **Step 4: Implement EnchantService**

V1 active enchant count per equipment = 1.

- [ ] **Step 5: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Enchant Assets/MazeMath/Tests/EditMode/Enchant
git commit -m "feat: add knowledge XP and enchant choices"
~~~

---

### Task 9: Add five equipment enchant sets and behavioral modifiers

**Files:**
- Create: enchant assets under `Assets/MazeMath/ScriptableObjects/Enchants/`
- Create: `Assets/MazeMath/Scripts/Equipment/Runtime/EquipmentAbilityService.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Equipment/EquipmentEnchantBehaviorTests.cs`

**Interfaces:**
- Produces modifier-aware ability state.
- Consumes active enchant IDs.

- [ ] **Step 1: Write Explorer Sensor Memory test**

Equip Memory I; assert recent route reveal duration/radius modifier is returned by capability query.

- [ ] **Step 2: Author at least two enchants per equipment**

Mining Arm:
- Echo Scan
- Lucky Dig

Power Wrench:
- Quick Fix
- Circuit Sense

Jump Booster:
- Soft Landing
- Route Scan

Energy Shield:
- Recharge
- Stable Field

Explorer Sensor:
- Memory I
- Guide I

- [ ] **Step 3: Implement ability modifier lookup**

Do not hardcode UI behavior inside EnchantService. Return typed modifier values/flags through EquipmentAbilityService.

- [ ] **Step 4: Run tests and commit**

~~~bash
git add Assets/MazeMath/ScriptableObjects/Enchants Assets/MazeMath/Scripts/Equipment Assets/MazeMath/Tests/EditMode/Equipment
git commit -m "feat: add equipment enchant behaviors"
~~~

---

### Task 10: Add Tier 2 upgrades

**Files:**
- Create Tier 2 assets under `Assets/MazeMath/ScriptableObjects/Equipment/`
- Create upgrade recipes under `Assets/MazeMath/ScriptableObjects/Recipes/`
- Create: `Assets/MazeMath/Tests/EditMode/Equipment/EquipmentUpgradeTests.cs`

**Interfaces:**
- Produces same equipment identity with tier advanced from 1 to 2.
- Consumes crafting transaction.

- [ ] **Step 1: Write failing upgrade-preserves-enchant test**

Upgrade owned T1 equipment to T2; assert active unlocked enchant remains associated with equipment.

- [ ] **Step 2: Implement upgrade path**

Do not create duplicate equipped items. Replace tier metadata/definition reference atomically.

- [ ] **Step 3: Add five Tier 2 definitions and upgrade recipes**

- [ ] **Step 4: Run tests and commit**

~~~bash
git add Assets/MazeMath/ScriptableObjects/Equipment Assets/MazeMath/ScriptableObjects/Recipes Assets/MazeMath/Tests/EditMode/Equipment
git commit -m "feat: add tier two equipment upgrades"
~~~

---

### Task 11: Add progression save/recovery

**Files:**
- Create: `Assets/MazeMath/Scripts/Inventory/Runtime/ProgressionSaveData.cs`
- Create: `Assets/MazeMath/Scripts/Inventory/Runtime/ProgressionSaveAdapter.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Progression/ProgressionRecoveryTests.cs`

**Interfaces:**
- Consumes foundation `SaveService`.
- Persists inventory, equipment, recipes, enchants, XP, pending rewards.

- [ ] **Step 1: Write failing roundtrip recovery test**

Craft equipment → equip → unlock enchant → queue overflow reward → save → rebuild services → restore.

Assert all state matches and pending reward is not duplicated.

- [ ] **Step 2: Implement save adapter**

- [ ] **Step 3: Add interrupted-workshop consistency test**

Save before and after completed crafting transaction; never allow state where ingredients are removed but result absent.

- [ ] **Step 4: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Inventory/Runtime Assets/MazeMath/Tests/PlayMode/Progression
git commit -m "feat: persist progression state"
~~~

---

### Task 12: Author V1 progression balance and end-to-end chapter test

**Files:**
- Create recipe assets for five Tier 1 equipment and at least two Pattern recipes.
- Create: `Assets/MazeMath/Scripts/Progression/ChapterProgressionValidator.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Progression/ChapterProgressionValidatorTests.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Integration/ChapterProgressionSmokeTests.cs`

**Interfaces:**
- Consumes Chapter 1 maze rewards and recipe costs.
- Produces validation that mandatory material availability >= 120% mandatory recipe cost.

- [ ] **Step 1: Write failing 120-percent material test**

Fixture with required Scrap cost 10 and guaranteed availability 11 must fail; availability 12 must pass.

- [ ] **Step 2: Implement ChapterProgressionValidator**

Random-only rewards do not count toward guaranteed mandatory material.

- [ ] **Step 3: Build end-to-end smoke test**

Flow:
question/puzzle reward → inventory → Workshop → craft required tool → equip → open Equipment Gate → save → reload → gate remains traversable.

- [ ] **Step 4: Run complete EditMode and PlayMode suites**

Expected: 0 failures.

- [ ] **Step 5: Run Web and Android smoke builds**

Expected: both build pipelines succeed in configured environments.

- [ ] **Step 6: Commit**

~~~bash
git add Assets/MazeMath
git commit -m "feat: complete crafting and enchant vertical slice"
~~~

## Plan Completion Gate

Crafting/Enchant V1 is complete when five materials and five equipment families work, guided and pattern crafting are transactional, Tier 1→2 upgrades preserve state, at least two enchants per equipment can be unlocked/re-equipped, overflow rewards are never lost, three or more equipment abilities open real maze routes, progression saves correctly, mandatory material availability passes the 120% rule, and Web/Android smoke builds pass.
