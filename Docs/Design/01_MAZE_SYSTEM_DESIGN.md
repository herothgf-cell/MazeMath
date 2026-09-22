# 01. Maze System Design

- Project: MazeMath
- Document type: Implementation-ready game system specification
- Target: Unity 2022.3.62f2 LTS, Web build + Android native app
- Primary audience: solo developer / Codex implementation agent
- Related specs: 02_CRAFTING_ENCHANT_DESIGN.md, 03_PUZZLE_QUESTION_SYSTEM_DESIGN.md

## 1. Purpose

MazeMath의 미로는 단순히 출구를 찾는 배경이 아니다. 플레이어가 길, 위치, 순서, 이전에 본 장치를 기억하고 다시 찾아가도록 만들어 작업기억과 공간 추론을 자연스럽게 사용하게 하는 핵심 게임 시스템이다.

핵심 플레이 루프는 다음과 같다.

탐색 → 단서 발견 → 문제/환경 퍼즐 해결 → 재료 획득 → 길/장비 해금 → 이전 위치로 되돌아감 → 심층부 진입 → 보스

## 2. Product Goals

1. 7세 플레이어가 별도 설명서 없이 30초 안에 이동과 목표를 이해할 수 있어야 한다.
2. 미로는 매 플레이 일부 구조가 달라지되, 필수 진행 경로와 퍼즐 품질은 항상 보장되어야 한다.
3. 한 번 지나간 장소를 기억하거나 지도에서 확인해 되돌아가는 경험을 챕터마다 최소 1회 제공한다.
4. 막다른 길 때문에 진행 불가능한 상태가 발생해서는 안 된다.
5. Web과 Android에서 동일한 시드라면 동일한 논리 구조를 생성해야 한다.
6. 키보드/게임패드가 없어도 Android 터치 UI만으로 모든 기능을 수행할 수 있어야 한다.
7. 길찾기 실패가 처벌로 느껴지기보다 힌트가 단계적으로 제공되어 다시 시도할 수 있어야 한다.

## 3. Non-Goals

- 완전 절차 생성형 로그라이크
- 수십 층 규모의 대형 던전
- 플레이어가 직접 블록을 설치해 미로를 만드는 샌드박스
- PvP, 온라인 멀티플레이
- 실시간 경쟁 타이머
- 길을 잃으면 처음부터 다시 시작하는 구조

## 4. Platform Baseline

### Unity

프로젝트 최초 생성 시점의 Unity 2022.3.62f2 LTS 패치 버전을 ProjectVersion.txt에 고정한다. 이후 팀/PC 간에는 동일한 Editor 버전을 사용한다.

### Web

Unity 2022.3 WebGL 기준으로 WebGL 2, WebAssembly, HTML5를 지원하는 64비트 데스크톱 브라우저를 대상으로 한다. Unity 2022.3 WebGL은 모바일 브라우저를 공식 지원하지 않으므로 Android에서는 네이티브 앱을 주 배포 방식으로 사용한다.

### Android

Unity 2022.3 자체 런타임은 Android 5.1 / API 22 이상을 지원한다. MazeMath의 제품 최소 사양은 API 23으로 시작하되 실제 스토어 제출 Target API는 배포 시점 Google Play 정책에 맞춘다. Android 개발 도구는 Unity 2022.3 호환 기준인 NDK r23b와 OpenJDK 11을 사용한다.

## 5. High-Level Architecture

Maze System은 다음 5개 계층으로 분리한다.

1. Authoring Data
   - ScriptableObject로 챕터, 방, 연결 규칙, 난이도 작성
2. Generation
   - 시드 기반으로 방 그래프 생성
3. Validation
   - 필수 경로, 잠금/열쇠, 퍼즐 선후관계 검증
4. Runtime
   - 방 활성화, 층 이동, 게이트, 체크포인트, 탐색 상태 관리
5. Presentation
   - 미니맵, Fog of War, 목표 표시, 힌트 표시

Scene에 모든 길을 하드코딩하지 않는다. 룸 템플릿과 연결 규칙을 데이터로 두고 런타임에 조합한다.

## 6. Recommended Folder Layout

~~~text
Assets/
  MazeMath/
    Scripts/
      Core/
        GameBootstrap.cs
        GameSession.cs
        Save/
          ISaveStore.cs
          PlayerPrefsSaveStore.cs
          SaveService.cs
      Maze/
        Data/
          MazeChapterDefinition.cs
          MazeDifficultyProfile.cs
          RoomTemplateDefinition.cs
          GateDefinition.cs
        Generation/
          MazeGenerator.cs
          MazeGraph.cs
          MazeValidator.cs
          MazeSeedService.cs
        Runtime/
          MazeRunController.cs
          RoomRuntime.cs
          GateRuntime.cs
          FloorTransition.cs
          CheckpointRuntime.cs
        Map/
          MazeMapService.cs
          FogOfWarService.cs
          ObjectiveMarkerService.cs
        Hint/
          MazeHintService.cs
      UI/
        Maze/
          MiniMapView.cs
          FloorIndicatorView.cs
          ObjectiveView.cs
          MazeHintView.cs
    ScriptableObjects/
      Maze/
    Prefabs/
      Maze/
    Scenes/
      Bootstrap.unity
      Gameplay.unity
Docs/
  Design/
    01_MAZE_SYSTEM_DESIGN.md
    02_CRAFTING_ENCHANT_DESIGN.md
    03_PUZZLE_QUESTION_SYSTEM_DESIGN.md
~~~

## 7. Core Data Model

### MazeChapterDefinition

ScriptableObject. 챕터별 미로 규칙을 정의한다.

Required fields:

- string ChapterId
- string DisplayName
- int FloorCount
- Vector2Int RequiredRoomCountRange
- Vector2Int OptionalRoomCountRange
- int MaxBranchDepth
- int RequiredBacktrackCount
- int MaxConsecutiveDeadEnds
- MazeDifficultyProfile Difficulty
- List<RoomTemplateDefinition> AllowedRooms
- List<string> RequiredRoomTags
- int BaseSeedOffset

### MazeDifficultyProfile

- int MinCriticalPathRooms
- int MaxCriticalPathRooms
- int MaxFloorTransitions
- int MaxSimultaneousObjectives
- int HintDelaySeconds
- int MapRevealRadius
- bool AllowOneWayGate
- bool AllowHiddenRoom
- bool AllowMultiFloorDependency

### RoomTemplateDefinition

- string TemplateId
- RoomType Type
- GameObject Prefab
- List<ConnectorDefinition> Connectors
- List<string> Tags
- int Weight
- bool CanBeCriticalPath
- bool CanBeOptional
- int FloorMin
- int FloorMax

RoomType enum:

- Start
- Corridor
- Junction
- Puzzle
- Question
- Reward
- Workshop
- Checkpoint
- Transition
- Boss
- Secret

### ConnectorDefinition

- string ConnectorId
- ConnectorDirection Direction
- Vector3 LocalPosition
- ConnectorSize Size
- List<string> CompatibleTags

## 8. MazeGraph Runtime Model

MazeGraph는 시각 오브젝트와 분리된 순수 C# 데이터 구조로 만든다.

### MazeNode

- string NodeId
- string TemplateId
- int Floor
- RoomType Type
- bool IsCriticalPath
- List<string> Tags

### MazeEdge

- string EdgeId
- string FromNodeId
- string ToNodeId
- EdgeType Type
- string GateId
- bool IsBidirectional

EdgeType:

- Open
- Door
- Ladder
- Elevator
- Locked
- PuzzleLocked
- EquipmentLocked
- Hidden

이 구조 덕분에 EditMode 테스트에서 실제 프리팹을 생성하지 않고 길찾기와 진행 가능성을 검증할 수 있다.

## 9. Hybrid Maze Generation

완전 랜덤이 아니라 "보장된 핵심 경로 + 랜덤 선택 분기" 방식으로 생성한다.

### Generation Steps

1. Seed 결정
2. Start와 Boss 노드 생성
3. 챕터 난이도에 맞는 Critical Path 길이 결정
4. Critical Path에 필수 Question/Puzzle/Workshop/Transition 슬롯 배치
5. Optional Branch 생성
6. Reward/Secret/Material 방 배치
7. Gate와 해제 수단 배치
8. 층 배정
9. Validation 실행
10. 실패하면 동일 Seed에서 파생 Seed로 최대 10회 재생성
11. 10회 실패 시 검증된 Safe Layout 사용

### Seed Rule

RunSeed = StableHash(ProfileId + ChapterId + AttemptIndex + BaseSeedOffset)

동일 입력은 Web/Android에서 동일 그래프를 생성해야 한다.

System.Random 구현 차이에 의존하지 않도록 프로젝트 전용 deterministic PRNG를 사용한다.

## 10. Critical Path Rules

필수 경로는 다음 조건을 모두 만족해야 한다.

- Start에서 Boss까지 경로가 1개 이상 존재
- 필수 퍼즐 이전에 해당 퍼즐 해결에 필요한 장비/정보가 존재
- Gate의 해제 수단은 Gate 뒤에 배치되지 않음
- Workshop이 필요한 제작을 요구한다면 Workshop이 먼저 등장
- 서로 다른 층을 오가는 필수 Backtrack은 Chapter 1에서는 최대 1회
- 필수 경로에 연속 막다른 방을 배치하지 않음
- 같은 종류의 퍼즐을 2회 연속 필수로 배치하지 않음

## 11. Room Distribution

### Chapter 1

- Floors: 1
- Critical rooms: 6~8
- Optional rooms: 2~3
- Junctions: 최대 2
- Required backtrack: 1
- Hidden room: 없음
- Equipment gate: 없음

목표: "아까 본 문으로 돌아가기" 학습.

### Chapter 2

- Floors: 2
- Critical rooms: 8~10
- Optional rooms: 3~4
- Junctions: 최대 3
- Required backtrack: 1~2
- Hidden room: 최대 1
- Equipment gate: 1

목표: 층 위치 + 열쇠/레버 기억.

### Chapter 3

- Floors: 2~3
- Critical rooms: 10~12
- Optional rooms: 4~5
- Multi-floor dependency: 허용
- Equipment gate: 1~2

목표: 순서 계획과 층간 이동.

### Chapter 4+

- Floors: 최대 3
- Critical rooms: 12~15
- Optional rooms: 4~6
- Multi-floor dependency: 최대 2
- Hidden room: 최대 2

7세 기준으로 미로 복잡도보다 퍼즐 조합 난이도를 올린다. 방 수는 15개 안팎을 권장 상한으로 둔다.

## 12. Backtracking Design

되돌아가기는 교육 목표지만 반복 이동 노동이 되어서는 안 된다.

필수 Backtrack의 좋은 예:

1. 1층에서 파란 문 발견
2. 2층에서 파란 스위치 발견
3. 스위치를 켜자 "아까 본 파란 문" 목표 표시
4. 1층으로 돌아가 문 통과

나쁜 예:

- 아무 정보 없이 모든 방을 다시 탐색
- 같은 긴 통로를 3회 이상 왕복
- 목표가 다른 층에 있다는 정보조차 없음

### Fast Return

한 번 해결한 긴 통로에는 다음 중 하나를 활성화할 수 있다.

- 숏컷 문
- 엘리베이터
- 체크포인트 이동
- 일방향 낙하 통로

이 기능은 사고 과정은 유지하면서 반복 이동만 줄인다.

## 13. Floor Transition

층 버튼으로 즉시 이동시키지 않는다. 플레이어가 실제 공간의 사다리/계단/엘리베이터까지 이동해야 한다.

FloorTransition API:

~~~csharp
public interface IFloorTransition
{
    int FromFloor { get; }
    int ToFloor { get; }
    bool CanUse(GameSession session);
    void Traverse();
}
~~~

Transition 사용 시:

1. 현재 방 탐색 상태 저장
2. 목적 층의 연결 방 활성화
3. 플레이어 SpawnPoint 이동
4. 카메라 경계 갱신
5. 미니맵 현재 층 변경
6. Objective Marker 재계산

## 14. Fog of War Map

전체 지도를 처음부터 공개하지 않는다.

### Reveal States

- Unknown: 검은 영역
- Seen: 방 윤곽만 표시
- Visited: 연결 통로와 아이콘 표시
- Cleared: 퍼즐/문제 완료 상태 표시

기본 규칙:

- 현재 방 입장 시 해당 방 Visited
- 인접 방은 기본 Unknown
- Explorer Sensor 장비에 따라 인접 방을 Seen 처리 가능
- 보물/비밀방은 관련 장비/인챈트 없이는 아이콘 미표시

## 15. Map UX

미니맵은 화면을 가리지 않는 우측 상단 소형 뷰를 기본으로 한다.

표시 항목:

- 현재 위치
- 방문한 방
- 현재 층
- 활성 목표
- 이미 발견한 잠긴 문
- Workshop / Checkpoint

전체 지도 버튼을 누르면 일시정지 상태로 확대한다.

Android 터치에서는 핀치 줌을 필수로 요구하지 않고 + / - 버튼도 제공한다.

## 16. Gates

Gate는 미로와 다른 시스템을 연결하는 핵심이다.

GateType:

- Key
- Switch
- Question
- EnvironmentPuzzle
- Equipment
- BossShield

### GateDefinition

- string GateId
- GateType Type
- string RequirementId
- bool PersistentAfterSolved
- string OpenFeedbackKey

Gate 상태는 MazeRunState에 저장한다.

## 17. Equipment Gate Rules

02 문서의 장비를 길 해금에 사용한다.

예:

- Mining Arm → 약한 균열 벽 제거
- Power Wrench → 고장 난 엘리베이터 수리
- Jump Booster → 높은 발판 접근
- Explorer Sensor → 비밀 벽 감지
- Energy Shield → 위험 통로 통과

규칙:

1. 필수 Equipment Gate를 배치할 때 필요한 장비는 해당 Gate 전에 반드시 획득/제작 가능해야 한다.
2. 선택 보물방에는 상위 장비를 요구할 수 있다.
3. 기존 챕터 재방문 시 새 장비로 과거의 비밀 공간을 열 수 있다.

## 18. Objective System

화면에 항상 한 개의 주 목표만 크게 표시한다.

예:

- "파란 스위치를 찾아보자"
- "1층에서 봤던 파란 문으로 돌아가자"
- "작업대에서 파워 렌치를 만들자"

보조 목표는 전체 지도 화면에서만 표시한다.

목표 데이터:

- ObjectiveId
- DisplayTextKey
- TargetNodeId
- TargetGateId
- HintPolicy
- CompletionCondition

## 19. Hint Escalation

길찾기 실패를 감지하는 조건:

- 동일한 방문 완료 방을 3회 이상 반복 진입
- 목표 진행 없이 90초 경과
- 서로 다른 막다른 길을 2회 연속 방문

힌트 단계:

### Level 1: 말 힌트

모모: "아까 아래층에서 파란 문을 본 것 같아요."

### Level 2: 층 힌트

미니맵에서 목표가 있는 층 탭이 깜빡인다.

### Level 3: 방향 힌트

현재 방 기준 다음 연결 방향만 표시한다. 전체 최단 경로 선은 표시하지 않는다.

### Level 4: 직접 안내

어린이 보호용 접근성 옵션. 목표 방까지 경로 화살표를 표시한다.

힌트 사용으로 챕터 클리어 보상을 차감하지 않는다.

## 20. Checkpoint and Recovery

체크포인트 위치:

- 챕터 시작
- 층 변경 직후 주요 허브
- 보스방 직전

체크포인트 저장 항목:

- RunSeed
- CurrentNodeId
- CurrentFloor
- VisitedNodeIds
- SeenNodeIds
- SolvedGateIds
- ActiveObjectiveId
- PlayerSpawnId
- Inventory snapshot reference
- Puzzle progress reference

앱 강제 종료 후에도 최근 체크포인트부터 복구할 수 있어야 한다.

## 21. Save Contract

~~~csharp
[Serializable]
public sealed class MazeRunState
{
    public string chapterId;
    public int runSeed;
    public string currentNodeId;
    public int currentFloor;
    public List<string> visitedNodeIds;
    public List<string> seenNodeIds;
    public List<string> solvedGateIds;
    public string activeObjectiveId;
    public string playerSpawnId;
}
~~~

저장 접근은 직접 PlayerPrefs를 호출하지 않고 ISaveStore를 통한다.

~~~csharp
public interface ISaveStore
{
    void Save(string key, string json);
    bool TryLoad(string key, out string json);
    void Delete(string key);
}
~~~

초기 구현은 PlayerPrefsSaveStore를 Web/Android 공통으로 사용한다. 향후 파일/클라우드 저장으로 교체할 수 있게 호출부를 분리한다.

## 22. Runtime Interfaces

~~~csharp
public interface IMazeService
{
    MazeGraph CurrentGraph { get; }
    MazeRunState CurrentState { get; }

    MazeGraph StartChapter(string chapterId, int runSeed);
    void EnterNode(string nodeId);
    void SolveGate(string gateId);
    bool CanTraverse(string edgeId);
}
~~~

~~~csharp
public interface IMazeMapService
{
    void MarkVisited(string nodeId);
    void MarkSeen(string nodeId);
    IReadOnlyCollection<string> GetVisitedNodes(int floor);
    IReadOnlyCollection<string> GetSeenNodes(int floor);
}
~~~

02/03 시스템은 IMazeService.SolveGate(gateId)를 통해 길을 연다. 서로의 UI나 MonoBehaviour를 직접 참조하지 않는다.

## 23. Performance Requirements

Web/Android 공통 목표:

- 미로 생성: 일반 모바일 기기에서 200ms 이하를 목표
- Gameplay Scene 로딩 후 한 프레임에 모든 방 프리팹을 Instantiate하지 않음
- 현재 방 + 인접 방 중심으로 활성화
- 모바일에서 60fps 목표, 최소 허용 30fps
- 런타임 GC Allocation이 이동 중 반복적으로 발생하지 않도록 미니맵 아이콘과 방 오브젝트를 풀링

## 24. Input Requirements

PC:

- WASD / 방향키: 이동
- Space: 점프
- E: 상호작용
- M: 지도
- B: 가방

Android/Web Mobile:

- 좌측 가상 스틱 또는 좌/우 버튼
- 점프 버튼
- 상호작용 버튼
- 지도 버튼
- 가방 버튼

중요 상호작용은 hover에 의존하지 않는다.

## 25. Validation Rules

MazeValidator.Validate(graph, chapterDefinition)는 최소 다음 오류를 반환한다.

- NoPathToBoss
- MissingRequiredRoom
- GateDependencyCycle
- RequirementBehindOwnGate
- ExcessiveBacktracking
- InvalidFloorTransition
- UnreachableOptionalRoom
- ConsecutiveSamePuzzleType
- MissingCheckpointBeforeBoss

Critical 오류가 하나라도 있으면 해당 그래프를 사용하지 않는다.

## 26. EditMode Tests

필수 테스트:

1. 같은 seed는 같은 MazeGraph를 생성한다.
2. Start → Boss 경로가 항상 존재한다.
3. Gate 해제 수단이 Gate 뒤에만 존재하는 구조를 거부한다.
4. Chapter 1의 필수 backtrack 횟수가 1을 초과하지 않는다.
5. FloorTransition edge가 양쪽 유효 노드를 가진다.
6. RequiredRoomTag가 모두 그래프에 존재한다.
7. 1,000개 seed 생성 fuzz test에서 validation critical failure가 0이어야 한다.
8. Safe Layout fallback이 항상 유효해야 한다.

## 27. PlayMode Tests

1. 방 입장 시 Visited 상태와 미니맵이 갱신된다.
2. 잠긴 Gate는 Requirement 전에는 통과할 수 없다.
3. Gate 해결 후 즉시 통과 가능하다.
4. 층 이동 후 현재 층 UI와 카메라 경계가 갱신된다.
5. 체크포인트 저장 → Scene reload → 상태가 복구된다.
6. 90초 무진행 조건에서 Hint Level 1이 발생한다.
7. 장비 Gate 해제 이벤트가 02 시스템과 연결된다.
8. 문제/퍼즐 완료가 03 시스템을 통해 Gate를 해제한다.

## 28. Acceptance Criteria

Maze System V1 완료 조건:

- 최소 2층짜리 테스트 챕터 1개가 생성된다.
- 시작 → 보스까지 필수 진행이 가능하다.
- 플레이마다 3개 이상의 Optional 배치 요소가 달라진다.
- 한 번의 의미 있는 되돌아가기가 포함된다.
- Fog of War 미니맵이 작동한다.
- Key/Switch/Question/Puzzle/Equipment Gate 중 최소 4종이 작동한다.
- 힌트 Level 1~3이 작동한다.
- 저장 후 Web 새로고침/Android 앱 재시작 시 체크포인트 복구가 가능하다.
- 1,000 seed validation test가 통과한다.
- 키보드와 터치만으로 각각 챕터를 완주할 수 있다.

## 29. Implementation Order

Phase M1 — Pure data + MazeGraph
Phase M2 — Deterministic generator
Phase M3 — Validator + fuzz tests
Phase M4 — Room prefab assembly
Phase M5 — Floor transition + gates
Phase M6 — Fog of War + objective
Phase M7 — Hint escalation
Phase M8 — Save/recovery
Phase M9 — Equipment / Puzzle integration
Phase M10 — Web + Android smoke test

## 30. References

- Unity 2022.3 System Requirements: https://docs.unity3d.com/2022.3/Documentation/Manual/system-requirements.html
- Unity Web Browser Compatibility: https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-browsercompatibility.html
- Unity Android Requirements: https://docs.unity3d.com/2022.3/Documentation/Manual/android-requirements-and-compatibility.html
