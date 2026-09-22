# 02. Crafting & Enchant System Design

- Project: MazeMath
- Document type: Implementation-ready game system specification
- Target: Unity 6 LTS, Web build + Android native app
- Related specs: 01_MAZE_SYSTEM_DESIGN.md, 03_PUZZLE_QUESTION_SYSTEM_DESIGN.md

## 1. Purpose

장비·성장 시스템은 마인크래프트의 핵심 재미인 "탐험 → 재료 획득 → 조합 → 더 좋은 도구 제작 → 새로운 공간 접근" 구조를 MazeMath에 맞게 단순화한다.

목표는 공격력 수치 상승 중심 RPG가 아니라, 새 장비를 만들수록 미로에서 할 수 있는 행동이 늘어나도록 만드는 것이다.

예:

- Mining Arm 제작 → 균열 벽 제거
- Power Wrench 제작 → 고장 난 엘리베이터 수리
- Jump Booster 제작 → 높은 플랫폼 접근
- Explorer Sensor 제작 → 비밀 공간 탐지
- Energy Shield 제작 → 위험 구간/보스 실수 허용

인챈트는 랜덤 뽑기가 아니라 플레이어가 2~3개의 의미 있는 능력 중 하나를 선택하는 방식으로 설계한다.

## 2. Design Goals

1. 7세 플레이어가 재료 이름과 제작 목표를 직관적으로 이해할 수 있어야 한다.
2. 장비 제작은 다음 탐험 목표와 직접 연결되어야 한다.
3. 레시피는 처음에는 안내형, 후반에는 기억/패턴 퍼즐형으로 확장한다.
4. 인챈트는 "좋고 나쁨"이 아니라 플레이 방식 차이를 만드는 선택지여야 한다.
5. 실패로 아이템을 잃거나 장비가 영구 파괴되는 처벌은 사용하지 않는다.
6. Web/Android에서 드래그 없이도 탭 조작만으로 제작 가능해야 한다.
7. 장비와 인챈트 효과는 데이터 주도형으로 추가할 수 있어야 한다.

## 3. Non-Goals

- 마인크래프트의 모든 제작 레시피 재현
- 수백 종류 아이템
- 랜덤 옵션 파밍
- 가챠
- 내구도 0에서 장비 영구 파괴
- 복잡한 경제/판매 시스템
- 장비 강화 실패
- 동일 장비 +1, +2, +3 식의 순수 수치 강화 반복

## 4. Core Progression Loop

미로 탐험
→ 문제/환경 퍼즐 해결
→ 재료 + Knowledge XP 획득
→ Workshop 발견
→ 장비 제작
→ 장착
→ 새로운 길/보물 접근
→ Knowledge XP로 인챈트
→ 이전 챕터 비밀 공간 재방문 가능

## 5. Resource Set

V1에서는 재료를 5종으로 제한한다.

### Scrap

- 기본 재료
- 상자, 문제, 일반 퍼즐에서 획득
- 대부분 초급 장비에 사용

### Iron Plate

- 강화 프레임 재료
- 중급 제작에 사용

### Gear

- 기계 장비 핵심 부품
- 퍼즐방/Workshop 주변에서 주로 획득

### Energy Crystal

- 전자/에너지 장비에 사용
- Question/Puzzle 보상으로 획득

### Rare Core

- 챕터 보스 또는 고난도 선택 퍼즐 보상
- 상위 레시피/고급 인챈트 해금에 사용
- 지나친 희소성을 만들지 않으며 필수 진행 장비에는 요구하지 않는다.

## 6. Currency Policy

화폐를 별도로 만들지 않는다.

성장 자원은 다음 두 축만 사용한다.

- Material: 제작용
- Knowledge XP: 인챈트/성장용

UI에서 자원 종류가 많아져 인지 부담이 커지는 것을 방지한다.

## 7. Equipment Slots

장비 슬롯은 5개 고정이다.

1. Arm Tool
2. Utility Tool
3. Mobility
4. Defense
5. Sensor

플레이어는 각 슬롯에 1개 장비만 장착한다.

V1 장비:

- Mining Arm
- Power Wrench
- Jump Booster
- Energy Shield
- Explorer Sensor

## 8. Equipment Definitions

### Mining Arm

Base ability:
- Cracked Wall 제거

Tier 2:
- Reinforced Wall 제거

Enchant candidates:
- Echo Scan: 가까운 비밀 벽이 짧게 반짝임
- Lucky Dig: 선택형 재료 노드에서 추가 Scrap 획득 확률
- Efficient Strike: 연속 사용 에너지 비용 감소

### Power Wrench

Base ability:
- Broken Machine 수리
- 특정 Gate 전원 복구

Tier 2:
- Advanced Machine 수리

Enchant candidates:
- Quick Fix: 간단 수리 미니게임 단계 1개 감소
- Circuit Sense: 연결해야 할 첫 단자 강조
- Recycler: 수리 완료 시 소량 Scrap 반환

### Jump Booster

Base ability:
- 높은 발판 1단계 접근

Tier 2:
- 더 높은 발판 및 긴 간격 이동

Enchant candidates:
- Soft Landing: 낙하 위험 무시
- Double Pulse: 특정 퍼즐 구역에서 보조 점프 1회
- Route Scan: 도달 가능한 높은 발판 테두리 표시

### Energy Shield

Base ability:
- 보스/위험 구간 실수 1회 보호

Tier 2:
- 체크포인트마다 보호 2회

Enchant candidates:
- Recharge: 퍼즐 2개 해결 시 보호 1회 회복
- Stable Field: 시간 제한 환경 장치의 허용 시간 증가
- Reflect Hint: 보스 공격 패턴 예고 시간 증가

### Explorer Sensor

Base ability:
- 현재 방의 중요 상호작용 오브젝트 감지

Tier 2:
- 인접 방 일부 정보 감지

Enchant candidates:
- Memory I: 최근 방문 경로 강조
- Treasure I: 근처 보물방 신호
- Guide I: 힌트 발동 시 다음 방 방향 강조

## 9. Tier Policy

V1은 장비당 2 Tier만 사용한다.

- Tier 1: 첫 제작
- Tier 2: 챕터 진행 후 업그레이드

Tier 3 이상은 실제 플레이 테스트 후 추가한다.

Tier 상승은 동일 장비를 버리고 새 장비로 교체하는 것이 아니라 Upgrade Recipe를 수행해 같은 장비의 기능을 확장하는 방식이다.

## 10. Crafting Recipe Model

레시피는 두 종류다.

### Type A — Guided Recipe

초반 기본 방식.

필요 재료를 UI에 직접 표시한다.

예:

Power Wrench

- Scrap 2/2
- Gear 1/1
- Energy Crystal 1/1

모두 충족 시 제작 버튼 활성화.

### Type B — Pattern Recipe

후반 선택/비밀 장비에 사용.

3x3 Grid에 재료를 배치한다.

예:

~~~text
[ Gear ][ Empty ][ Gear ]
[ Empty][Crystal][ Empty]
[ Scrap][ Empty ][ Scrap]
~~~

패턴 단서는 미로 벽, 책상 메모, 기계 패널 등 환경에서 먼저 발견한다.

필수 진행 장비는 Pattern Recipe 암기를 요구하지 않는다.

## 11. Why 3x3 Crafting Is Limited

마인크래프트식 3x3은 재미 요소로 사용하되 학습 부담이 되지 않게 제한한다.

- Chapter 1: Guided only
- Chapter 2: Guided + 1개의 연습 Pattern
- Chapter 3: Optional Pattern 1~2개
- Chapter 4+: 비밀 장비/보너스 보상 중심

Pattern 실패 시 재료를 소모하지 않는다.

## 12. ScriptableObject Data

### ItemDefinition

Fields:

- string ItemId
- string DisplayNameKey
- Sprite Icon
- ItemCategory Category
- int MaxStack
- string DescriptionKey

ItemCategory:

- Material
- Equipment
- Quest
- RecipeClue

### EquipmentDefinition

Fields:

- string EquipmentId
- string DisplayNameKey
- EquipmentSlot Slot
- int Tier
- Sprite Icon
- List<string> AbilityIds
- List<EnchantDefinition> AvailableEnchants
- string UpgradeRecipeId

### RecipeDefinition

Fields:

- string RecipeId
- string ResultItemId
- int ResultCount
- RecipeType Type
- List<IngredientRequirement> Ingredients
- GridIngredient[9] Pattern
- string RequiredWorkshopTag
- string UnlockConditionId

### IngredientRequirement

- string ItemId
- int Count

## 13. Suggested Folder Layout

~~~text
Assets/
  MazeMath/
    Scripts/
      Inventory/
        Data/
          ItemDefinition.cs
          EquipmentDefinition.cs
          InventoryDefinition.cs
        Runtime/
          InventoryService.cs
          EquipmentService.cs
          ItemStack.cs
      Crafting/
        Data/
          RecipeDefinition.cs
          RecipeCatalog.cs
        Runtime/
          CraftingService.cs
          RecipeMatcher.cs
          WorkshopRuntime.cs
      Enchant/
        Data/
          EnchantDefinition.cs
          EnchantCatalog.cs
        Runtime/
          EnchantService.cs
          KnowledgeXpService.cs
      Equipment/
        Runtime/
          EquipmentAbilityService.cs
          Abilities/
            MiningAbility.cs
            RepairAbility.cs
            JumpBoostAbility.cs
            ShieldAbility.cs
            SensorAbility.cs
      UI/
        Inventory/
        Crafting/
        Enchant/
    ScriptableObjects/
      Items/
      Equipment/
      Recipes/
      Enchants/
~~~

## 14. Inventory Model

V1 기본 인벤토리:

- Material slots: 12
- Equipment slots: 장비 슬롯과 별도 보관 10
- Quest item: 전용 무제한 목록
- Hotbar: 6

재료는 MaxStack 99.

인벤토리가 가득 찬 상태에서 Material 보상을 얻으면 동일 아이템 스택을 우선 합친다.

그래도 공간이 없으면 보상을 잃지 않고 PendingRewardQueue에 보관하고 다음 Workshop/Checkpoint에서 자동 수령한다.

## 15. Inventory API

~~~csharp
public interface IInventoryService
{
    int GetCount(string itemId);
    bool Has(string itemId, int count);
    bool TryAdd(string itemId, int count);
    bool TryRemove(string itemId, int count);
    IReadOnlyList<ItemStack> GetItems();
}
~~~

## 16. Equipment API

~~~csharp
public interface IEquipmentService
{
    string GetEquipped(EquipmentSlot slot);
    bool Equip(string equipmentId);
    bool HasAbility(string abilityId);
    int GetEquipmentTier(string equipmentId);
}
~~~

Maze System은 EquipmentDefinition을 직접 참조하지 않고 HasAbility(abilityId)로 진행 가능 여부를 확인한다.

예:

- ability.break.cracked_wall
- ability.repair.basic_machine
- ability.jump.high_platform
- ability.shield.hazard
- ability.sensor.hidden_wall

## 17. Crafting API

~~~csharp
public interface ICraftingService
{
    bool CanCraft(string recipeId);
    CraftResult Craft(string recipeId);
    bool MatchesPattern(string recipeId, IReadOnlyList<string> gridItemIds);
}
~~~

CraftResult:

- Success
- RecipeLocked
- MissingMaterial
- InventoryFull
- InvalidPattern

InventoryFull은 실제 아이템 손실 없이 PendingRewardQueue 또는 즉시 장착 선택 UI로 해결한다.

## 18. Crafting Transaction Rule

제작은 원자적으로 처리한다.

1. Recipe 유효성 확인
2. Unlock 확인
3. 재료 수량 확인
4. 결과 저장 공간 확인
5. 재료 차감
6. 결과 지급
7. SaveService 저장
8. UI 갱신

5 이후 실패가 발생하면 재료/결과 상태를 이전 상태로 롤백한다.

## 19. Recipe Unlock

레시피 해금 방법:

- 챕터 진입 자동
- NPC/모모 대화
- Recipe Clue 발견
- 특정 환경 퍼즐 해결
- Boss 클리어

V1 필수 진행 레시피는 자동 또는 명시적 이벤트로 반드시 획득한다.

선택 콘텐츠만 숨겨진 레시피를 사용한다.

## 20. Workshop Design

Workshop은 미로 중간 허브 역할을 한다.

기능:

- Craft
- Upgrade
- Enchant
- 장비 교체
- 현재 필요한 재료 확인
- Pending Reward 수령

Workshop 입장 시 게임은 기본적으로 안전 상태가 되며 시간 압박이 없다.

UI 탭:

1. 만들기
2. 강화
3. 인챈트
4. 가방

7세 기준으로 한 화면에 모든 기능을 동시에 노출하지 않는다.

## 21. Crafting UX

Guided Recipe 화면:

~~~text
[장비 그림]  파워 렌치

필요 재료
철 조각      2 / 2   ✓
기어         1 / 1   ✓
에너지 수정   0 / 1

[재료가 더 필요해요]
~~~

재료가 충족되면:

~~~text
[ 만들기 ]
~~~

제작 후 짧은 조립 애니메이션과 실제 사용처 예시를 보여준다.

"이제 고장 난 기계를 고칠 수 있어요!"

## 22. Pattern Crafting UX

모바일에서 drag만 강제하지 않는다.

지원 입력:

- 아이템 탭 → Grid 슬롯 탭
- drag & drop
- Grid 슬롯 탭 → 제거

잘못된 배치는 빨간 X 대신 중립 상태로 둔다.

"다시 배치해 볼까요?" 형태의 피드백을 사용한다.

## 23. Knowledge XP

문제/퍼즐 해결 시 Knowledge XP 획득.

기본 V1 예시:

- Easy Question: 5 XP
- Normal Question: 8 XP
- Hard Question: 12 XP
- Environment Puzzle: 10~15 XP
- Boss Puzzle Phase: 20 XP

연속 실패했다고 XP를 차감하지 않는다.

## 24. Enchant Model

EnchantDefinition fields:

- string EnchantId
- string DisplayNameKey
- string DescriptionKey
- EquipmentSlot CompatibleSlot
- int Level
- int KnowledgeXpCost
- string AbilityModifierId
- List<string> PrerequisiteEnchantIds

V1은 장비별 최대 1개의 활성 인챈트를 허용한다.

나중에 테스트 후 2슬롯으로 확장 가능하게 데이터는 List 구조로 저장한다.

## 25. Enchant Choice

무작위 인챈트를 사용하지 않는다.

Workshop에서 해당 장비에 가능한 2~3개 선택지를 항상 보여준다.

예:

Explorer Sensor

- Memory I: 최근 방문 경로 강조
- Treasure I: 근처 보물 신호
- Guide I: 힌트 시 다음 방 방향 표시

아이콘 + 한 문장 효과로 이해할 수 있게 한다.

선택 전에 "미리보기"가 가능해야 한다.

## 26. Re-Enchant Policy

선택을 잘못했다고 영구 손해가 생기지 않게 한다.

- Workshop에서 언제든 장착 인챈트 교체 가능
- 이미 구매한 인챈트 재장착은 무료
- 새로운 인챈트 최초 해금에만 Knowledge XP 사용

즉, XP는 "능력 구매"에 사용하고 "능력 변경"에는 사용하지 않는다.

## 27. Durability Policy

영구 내구도는 사용하지 않는다.

이유:

- 7세에게 장비 파괴는 불필요한 상실감
- 탐험 중 필수 장비 파괴 시 Soft Lock 가능
- 반복 재료 파밍 유도 가능

대신 일부 능력에는 Charge를 사용한다.

예:

Energy Shield Charge 1/1

Checkpoint 도달 또는 특정 퍼즐 해결 시 자동 회복.

장비 자체는 절대 사라지지 않는다.

## 28. Equipment Gate Integration

01 문서의 GateDefinition에서 RequirementId를 AbilityId로 사용한다.

예:

Equipment Gate:
- GateType = Equipment
- RequirementId = ability.break.cracked_wall

MazeRunController는 IEquipmentService.HasAbility를 호출한다.

장비 UI, 제작 UI 또는 구체 클래스에 직접 의존하지 않는다.

## 29. Question/Puzzle Reward Integration

03 시스템은 RewardService에 결과를 전달한다.

예:

~~~csharp
public readonly struct LearningReward
{
    public readonly int knowledgeXp;
    public readonly IReadOnlyList<ItemReward> items;
}
~~~

RewardService가 Inventory와 KnowledgeXpService를 갱신한다.

문제 시스템이 InventoryService를 직접 호출하지 않는다.

## 30. Save Data

~~~csharp
[Serializable]
public sealed class ProgressionSaveData
{
    public List<ItemStackSaveData> inventory;
    public List<EquipmentSaveData> equipment;
    public List<string> unlockedRecipeIds;
    public List<string> unlockedEnchantIds;
    public int knowledgeXp;
    public List<PendingRewardSaveData> pendingRewards;
}
~~~

EquipmentSaveData:

- equipmentId
- tier
- equipped
- activeEnchantIds

## 31. Balance Rules

필수 진행 장비:

- 현재 챕터를 정상적으로 탐험하면 필요한 재료의 120% 이상을 획득할 수 있게 배치
- 한 개 선택 상자를 놓쳤다고 진행이 막혀서는 안 됨
- Rare Core를 필수 장비에 요구하지 않음

선택 장비/인챈트:

- 선택 퍼즐/비밀방에서 빠르게 해금 가능
- 미획득 상태에서도 메인 스토리 클리어 가능

## 32. Anti-Grind Rules

동일 쉬운 문제를 반복해서 무한 XP/재료 획득하는 구조를 금지한다.

- 각 Question Node 최초 클리어 보상 100%
- 재도전 연습 보상 0~20%
- Chapter clear 후 Practice Mode는 학습용 점수만 제공 가능
- Boss 반복 파밍으로 Rare Core 무한 획득 금지

필요한 필수 재료가 부족하면 Grind를 요구하지 않고 Recovery Chest 또는 Workshop 보급으로 복구한다.

## 33. Minecraft Inspiration Boundary

가져올 요소:

- 탐험 중 재료 획득
- 작업대
- 재료 조합
- 3x3 패턴 레시피
- 도구가 새로운 행동을 가능하게 함
- 인챈트로 같은 도구의 플레이 방식 변화

그대로 가져오지 않을 요소:

- 동일 아이콘/텍스처/명칭
- Minecraft 고유 UI 복제
- 동일 레시피
- 동일 인챈트 이름
- 내구도 파괴
- 수십 개 아이템 관리

MazeMath만의 로봇 조립대, 회로 코어, 센서 인챈트 비주얼을 사용한다.

## 34. EditMode Tests

1. Guided Recipe의 재료가 충분하면 CanCraft가 true.
2. 재료가 부족하면 false이고 수량 변화 없음.
3. 제작 성공 시 재료 차감과 결과 지급이 정확히 한 번 발생.
4. Pattern Recipe 회전 허용 여부가 정의대로 동작.
5. 잘못된 Pattern 실패 시 재료 소모 없음.
6. Equip 시 같은 Slot 기존 장비가 해제.
7. AbilityId가 Tier/Enchant에 따라 정확히 반환.
8. 이미 구매한 Enchant 재장착에 XP가 소모되지 않음.
9. XP 부족 시 Enchant unlock 실패 + XP 유지.
10. Inventory overflow 시 PendingReward에 보존.

## 35. PlayMode Tests

1. Workshop 진입 → 제작 → 장착까지 터치만으로 가능.
2. Mining Arm 장착 후 Cracked Wall Gate를 연다.
3. 장비 미장착 시 해당 Gate를 열 수 없다.
4. Explorer Sensor 인챈트가 01 미니맵 표시를 바꾼다.
5. Question Reward가 Material/XP에 반영된다.
6. Save → 앱 재실행 후 인벤토리/장비/인챈트 복구.
7. 제작 애니메이션 중 중복 탭으로 2회 제작되지 않는다.
8. Web/Android에서 같은 Save Data를 정상 직렬화한다.

## 36. Acceptance Criteria

Crafting & Enchant V1 완료 조건:

- 5종 Material 구현
- 5종 Equipment 구현
- 장비당 Tier 1~2 구현
- Guided Recipe 최소 5개
- Pattern Recipe 최소 2개
- Workshop UI 구현
- 장비 슬롯/가방/Hotbar 구현
- Knowledge XP 구현
- 장비당 최소 2개 Enchant 선택 가능
- 최소 3종 Equipment Gate가 실제 미로와 연결
- 제작 실패/인벤토리 가득 참 상황에서 아이템 손실 0
- 저장/복구 가능
- PC + Android 터치 모두 완전 조작 가능

## 37. Implementation Order

Phase C1 — Item/Inventory data model
Phase C2 — InventoryService + save
Phase C3 — Guided Crafting
Phase C4 — Workshop UI
Phase C5 — Equipment slots + AbilityId
Phase C6 — Maze Equipment Gate integration
Phase C7 — Pattern Crafting
Phase C8 — Knowledge XP
Phase C9 — Enchant choice + re-equip
Phase C10 — Android/Web UX and regression

## 38. Dependencies

Depends on:

- 01: IMazeService, Equipment Gate
- Core: ISaveStore / SaveService

Provides to 01:

- IEquipmentService.HasAbility

Consumes from 03:

- LearningReward via RewardService

The three systems communicate through small service interfaces and ID strings. MonoBehaviour references across subsystem boundaries are prohibited unless UI presenter wiring is involved.
