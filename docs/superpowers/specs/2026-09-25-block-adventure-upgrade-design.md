# MazeMath Block Adventure Upgrade Design

- Date: 2026-09-25
- Target: Mobile browser edition (`Web/Instant`) first, with Unity parity later
- Status: Approved direction from playtest feedback; implementation spec
- Related:
  - `Docs/Design/01_MAZE_SYSTEM_DESIGN.md`
  - `Docs/Design/02_CRAFTING_ENCHANT_DESIGN.md`
  - `Docs/Design/03_PUZZLE_QUESTION_SYSTEM_DESIGN.md`
  - `Docs/Design/CHAPTER_2_5_ROADMAP.md`

## 1. Purpose

현재 MazeMath 모바일판은 블록형 그래픽과 3×3 제작을 갖췄지만, 플레이 행동은 여전히 “장치 조사 → 자동 처리” 비중이 높다.

이번 업그레이드의 목표는 Minecraft 에셋을 복제하지 않으면서도 아이가 다음 행동을 통해 **블록 세계를 직접 다루는 느낌**을 받게 하는 것이다.

핵심 루프:

탐험 → 문제/퍼즐로 공간 개방 → 직접 채굴 → 떨어진 재료 줍기 → 3×3 제작 → 핫바에서 도구 선택 → 블록/장치를 직접 조작 → 새 길 개척

또한 플레이 테스트에서 2→4→6 순서 발판은 “밟았는지 모르겠다 / 지금 몇 단계인지 모르겠다” 문제가 확인되었다. 발판 퍼즐은 입력 피드백과 현재 진행도를 명확히 보여주도록 개선한다.

## 2. Product Goals

1. 아이가 10초 이내에 “이 블록은 캐볼 수 있다”는 것을 알아차릴 수 있어야 한다.
2. Mining Arm은 벽을 자동 제거하는 열쇠가 아니라 실제 채굴 도구처럼 느껴져야 한다.
3. 재료는 가능한 경우 숫자만 증가하지 않고 월드 아이템으로 떨어져 플레이어가 직접 획득한다.
4. 핫바는 장식이 아니라 실제 장비 선택 인터페이스로 사용한다.
5. 블록 설치는 완전 자유 건축이 아니라 퍼즐/탐험용 지정 설치 위치에서만 제공한다.
6. 3×3 제작대는 현행 기본 제작 방식으로 유지하고 채굴/드롭과 자연스럽게 연결한다.
7. 2→4→6 같은 순서 발판은 각 입력마다 눌림, 빛, 텍스트 진행, 성공/실패 리셋이 즉시 보인다.
8. 모든 변경은 모바일 터치만으로 완료 가능해야 한다.
9. 기존 Chapter 1–5 저장과 완주 경로를 깨지 않는다.
10. 실패는 자원 손실보다 재시도와 힌트로 이어진다.

## 3. Scope

### 3.1 Block Mining

월드에 `MineNode`를 추가한다.

MineNode 종류:
- Soft Stone: 맨손/Mining Arm 모두 가능, Mining Arm이 더 빠름
- Cracked Stone: Mining Arm 필요
- Iron Ore: Mining Arm 필요, Scrap/Iron Plate 드롭
- Gear Ore: 후반부 기계 지역, Gear 드롭
- Crystal Ore: Energy Crystal 드롭

채굴 입력:
- 모바일: 채굴 가능한 블록 근처에서 핫바 Mining Arm 선택 후 `캐기` 버튼
- PC: 동일 대상에서 E 또는 사용 입력
- 1회 입력마다 hit count 증가
- 2~4 hit 후 파괴
- hit마다 crack stage 0→1→2→3
- 파괴 시 작은 파편 연출과 Pickup 생성

자동 연타/복잡한 공격 시스템은 만들지 않는다.

### 3.2 Pickup Drops

`PickupDrop` 데이터:
- materialId
- count
- x/y
- collected
- optional sourceNodeId

행동:
- 블록 파괴 시 월드에 1~3개의 작은 아이템 아이콘이 튀어나온다.
- 플레이어가 가까이 가면 자동 줍기.
- 줍는 순간 아이콘이 플레이어 쪽으로 짧게 이동하고 재료 수량 증가.
- 한 번 획득한 드롭은 중복 지급되지 않는다.
- 저장 직전 미획득 드롭은 source node와 함께 복원 가능하게 하거나, 간단한 버전에서는 파괴된 노드 보상을 pending pickup으로 저장한다.

### 3.3 Functional Hotbar

현재 하단 핫바를 실제 선택형으로 바꾼다.

Slots:
1. Mining Arm
2. Power Wrench
3. Jump Booster
4. Energy Shield
5. Explorer Sensor
6. Build Block

행동:
- 슬롯 탭 → selectedHotbar 변경
- 선택 슬롯은 두꺼운 테두리/위로 3px 상승
- 캐릭터 주변/손 위치에 선택 도구의 작은 아이콘 표시
- 상황 버튼 문구 변경:
  - Mining Arm + MineNode → `캐기`
  - Wrench + Machine → `수리`
  - Build Block + BuildSocket → `설치`
  - 그 외 → `조사`
- 장비를 아직 만들지 않았으면 슬롯은 잠김 표시

점프 부스터/실드/센서는 기존 패시브 효과를 유지하되 선택 시 자기 기능 대상이 더 강조된다.

### 3.4 Limited Block Placement

기존 Maze spec의 “자유 샌드박스 건축 없음”은 유지하되, 다음 예외를 추가한다.

`BuildSocket`이 있는 위치에서만 블록 설치 가능.

예:
- 2칸짜리 틈에 발판 블록 설치
- 높은 상자 앞에 1단 블록 설치
- 레이저 앞 임시 차단 블록
- 선택형 보물길을 위한 2~3개 블록

규칙:
- 자유 좌표 설치 없음
- 필수 경로를 영구적으로 막는 설치 없음
- 설치 블록은 다시 캐서 회수 가능
- 필수 진행에 필요한 블록 수는 이전 구간에서 항상 보장
- 최대 보유 Build Block 수는 초기 9개
- 첫 사용은 가이드가 설치 위치를 반투명 블록으로 보여준다.

### 3.5 3×3 Crafting Integration

현재 5종 장비 3×3 제작은 유지한다.

업그레이드:
- 채굴한 재료가 제작대 재료 선택 슬롯에 즉시 반영
- 재료 버튼은 블록 아이템처럼 표시
- 조합법 보기 시 3×3 ghost recipe 표시
- 첫 Mining Arm은 ghost recipe 기본 ON
- 이후 레시피는 기본 OFF
- 정확한 조합일 때만 오른쪽/아래 결과 슬롯에 장비 표시
- 잘못된 조합은 소모 없음

별도 “간편 제작” 버튼은 다시 추가하지 않는다.

## 4. Sequence Plate UX Upgrade

### 4.1 Problem

현재 2→4→6 발판은 플레이어가 실제로 밟아도:
- 눌렸다는 시각적 변화가 약함
- 현재 몇 단계인지 모름
- 정답 입력과 오답 리셋을 구분하기 어려움
- 점프로 다른 발판을 넘으라는 규칙이 즉시 이해되지 않음

### 4.2 New Plate States

각 발판은 다음 상태를 가진다.

- Idle: 기본 높이, 숫자 흰색
- Target: 다음 입력 대상일 때 약한 pulse
- PressedCorrect: 0.12초 아래로 눌림 + 초록/금빛 flash
- PressedWrong: 빨간 flash + 짧은 좌우 흔들림
- Completed: 계속 켜진 상태 + 체크 표시

### 4.3 Input Feedback

정답 발판을 밟으면:

`2 ✓   1 / 3`

형태의 작은 팝업을 캐릭터 위 또는 발판 위에 0.8초 표시한다.

다음 대상:
- Chapter 1: `다음 숫자: 4`
- Chapter 2 boss: `다음 숫자: 3`

상단 목표도 퍼즐 중에는:

`발판 1/3 · 다음은 4`

처럼 바뀐다.

### 4.4 Wrong Step

잘못된 발판을 밟으면:
- 해당 발판 빨간 flash
- 진행도 0으로 초기화
- 모든 완료 전 발판 원상복귀
- 안내: `순서가 달라요. 다시 2부터 시작해요.`
- HP/XP/재료 손실 없음

첫 실패 후 2초 동안 첫 번째 목표 발판에 pulse를 준다.

### 4.5 Completion

마지막 발판을 밟으면:
- 세 발판 모두 순차적으로 빛남
- `2 → 4 → 6 성공!` 또는 `2 → 3 → 5 성공!`
- 연결된 문/보호막이 실제로 열리는 연출
- 다음 목표가 즉시 갱신

## 5. Chapter Placement

### Chapter 1
- 기존 균열벽을 3-hit Cracked Stone 채굴로 교체
- 근처 선택 광석 2~3개 추가
- 2→4→6 발판 UX 개선
- 첫 BuildSocket 튜토리얼은 선택 보물길에 배치

### Chapter 2
- 용암 제련소에 Iron/Gear Ore
- Power Wrench 제작 재료 일부를 직접 채굴
- 보스 2→3→5 발판 동일 피드백 시스템 사용
- 선택 용암 틈 BuildSocket

### Chapter 3
- 높은 창고에서 Build Block 1개를 발판 보조에 사용 가능
- Jump Booster 없이 필수 진행을 우회시키지는 않음

### Chapter 4
- Crystal Ore 채굴
- Sensor 선택 시 채굴 가능한 숨은 광맥/BuildSocket 윤곽 표시

### Chapter 5
- 5종 도구를 모두 한 번씩 선택해서 사용하는 짧은 종합 구간
- 자유 건축이 아닌 기존 퍼즐 조합만 사용

## 6. Architecture

새 파일:
- `Web/Instant/block-runtime.js`
  - MineNode state
  - mining hit validation
  - drop creation/collection
  - BuildSocket placement/reclaim
- `Web/Instant/block-renderer.js`
  - crack stages
  - ore block rendering
  - pickups
  - placement ghost
  - plate animation state

수정:
- `Web/Instant/core.js`
  - save-safe block state helpers
- `Web/Instant/chapter-data.js`
  - per-chapter mine nodes/build sockets
- `Web/Instant/chapter-runtime.js`
  - sequence plate progress contract
- `Web/Instant/game.js`
  - hotbar selection, contextual action, orchestration only
- `Web/Instant/index.html`
  - hotbar selected/locked/pressed visual styles
- `Web/Instant/core.test.cjs`
  - pure block/sequence rules
- `Web/Instant/browser.test.cjs`
  - touch mining, pickup, hotbar, block placement, plate feedback

## 7. Save Compatibility

기존 campaign v2 저장을 유지한다.

chapterState에 선택적으로 추가:
- blockNodes: object keyed by node id with hit/destroyed state
- pendingDrops
- placedBlocks
- selectedHotbar
- sequenceProgress

필드가 없는 기존 저장은 기본값으로 보정한다.

기존 제작 장비/XP/챕터 완료 상태는 그대로 유지한다.

## 8. Test Requirements

### Core
- MineNode requires correct tool when configured
- exact hit count destroys node once
- reward/drop cannot duplicate
- collecting a pickup increments exact material once
- BuildSocket accepts only build block and only when empty
- reclaim restores one build block
- no required route can consume more build blocks than guaranteed supply
- sequence correct order advances 0→1→2→3
- wrong plate resets to 0
- completed sequence is idempotent
- existing campaign v2 save restores with default block fields

### Browser
Chromium + WebKit:
- select Mining Arm hotbar
- mine a Chapter 1 cracked block through visible crack stages
- collect dropped item
- place/reclaim one Build Block on mobile
- complete 2→4→6 and observe visible 1/3, 2/3, 3/3 feedback
- intentionally step wrong plate and see reset feedback
- complete Chapter 2 2→3→5 with same system
- 320×568 and 667×375 controls remain inside viewport
- Chapter 1–5 campaign remains completable

## 9. Non-Goals

- Minecraft assets, sounds, UI textures, or exact visual copying
- unlimited freeform building
- large survival inventory
- hunger, durability, day/night cycle
- real-time combat overhaul
- procedural voxel terrain
- physics-based dropped item simulation
- multiplayer

## 10. Success Criteria

Playtest success:
1. 아이가 Mining Arm을 만든 뒤 별도 설명 없이 채굴 가능한 블록을 한 번 이상 직접 캔다.
2. 떨어진 재료를 직접 줍고 작업대로 돌아가 제작과 연결한다.
3. 핫바에서 필요한 도구를 스스로 한 번 이상 선택한다.
4. 2→4→6 퍼즐에서 “내가 2를 눌렀다 / 다음은 4다”를 화면만 보고 이해한다.
5. 잘못 밟아도 왜 리셋됐는지 이해한다.
6. 기존 5챕터 완주 자동 테스트가 계속 통과한다.

## 11. Conflict Resolution with Existing Specs

- `01_MAZE_SYSTEM_DESIGN.md`의 “플레이어가 직접 블록을 설치해 미로를 만드는 샌드박스” Non-Goal은 **완전 자유 건축 금지**로 재해석한다. 이번 사양의 BuildSocket 기반 제한 설치는 허용한다.
- `02_CRAFTING_ENCHANT_DESIGN.md`의 Chapter 1 Guided-only / Pattern 제한은 최신 사용자 플레이 테스트 결정으로 대체한다. **현재 웹판에서는 5종 장비 모두 3×3 Pattern Recipe가 기본 제작 방식**이다.
- Unity 본편은 이번 단계의 직접 구현 범위가 아니다. 웹판 검증 후 동일 행동을 Unity에 이식한다.
