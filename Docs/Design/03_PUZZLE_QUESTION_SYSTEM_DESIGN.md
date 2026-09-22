# 03. Puzzle & Question System Design

- Project: MazeMath
- Document type: Implementation-ready game system specification
- Target: Unity 6 LTS, Web build + Android native app
- Related specs: 01_MAZE_SYSTEM_DESIGN.md, 02_CRAFTING_ENCHANT_DESIGN.md

## 1. Purpose

MazeMath의 문제 시스템은 "팝업 문제를 계속 푸는 수학 게임"이 아니라 탐험 흐름 속에서 계산, 규칙 찾기, 작업기억, 공간 추론을 자연스럽게 사용하게 만드는 시스템이다.

V1은 다음 5개 유형을 통합 지원한다.

1. Multiple Choice Question
2. Numeric Input Question
3. Step Input Question
4. Rule/Pattern Question
5. Environment Puzzle

Question과 Environment Puzzle은 표현 방식은 다르지만 동일한 Difficulty, Reward, Hint, Save 계약을 사용한다.

## 2. Design Goals

1. 객관식 선택지를 보고 역추정하는 플레이를 줄이기 위해 숫자 주관식 입력을 적극 사용한다.
2. 매 플레이 숫자/선택지/문제 순서가 달라져 반복 암기만으로 통과할 수 없게 한다.
3. 문제 난이도는 조금씩 상승하되 한 문제에서 여러 새로운 개념을 동시에 요구하지 않는다.
4. 환경 퍼즐은 화면 밖 게임 월드의 오브젝트를 직접 움직이거나 밟고 연결해 해결한다.
5. 정답을 틀려도 HP 감소나 재료 손실 같은 강한 처벌을 주지 않는다.
6. 90초 이상 막히면 정답이 아니라 관찰 방향을 알려주는 힌트를 제공한다.
7. PC와 모바일에서 숫자 입력 UX가 동일하게 이해 가능해야 한다.
8. 모든 문제 생성기는 Seed 기반으로 재현 가능해야 한다.

## 3. Non-Goals

- 초등 고학년 수준의 복잡한 수식
- 소수/분수 중심 문제
- 음수 계산
- 빠른 타이핑 경쟁
- 정답 실패 시 게임오버
- 긴 설명문 독해 문제
- 5분 이상 막히는 필수 퍼즐
- 랜덤 생성으로 정답이 없거나 복수 정답이 생기는 문제

## 4. Target Learning Axes

각 문제/퍼즐은 최소 하나의 LearningAxis를 가진다.

LearningAxis enum:

- Addition
- Subtraction
- Multiplication
- Division
- NumberSense
- Sequence
- RuleInference
- WorkingMemory
- SpatialRotation
- SpatialPlanning
- VisualSearch
- MultiStepPlanning

한 챕터에서 특정 축만 과도하게 반복하지 않도록 SessionBalancer가 분배한다.

## 5. Question Type Mix

### Early Game

- Multiple Choice: 50%
- Numeric Input: 35%
- Rule/Pattern: 10%
- Environment Puzzle: 5%

### Mid Game

- Multiple Choice: 30%
- Numeric Input: 40%
- Step Input: 10%
- Rule/Pattern: 10%
- Environment Puzzle: 10%

### Later Game

- Multiple Choice: 20%
- Numeric Input: 35%
- Step Input: 10%
- Rule/Pattern: 15%
- Environment Puzzle: 20%

Environment Puzzle은 플레이 시간이 길기 때문에 전체 "문제 수" 비율과 실제 "플레이 시간" 비율을 구분해 밸런싱한다.

## 6. Difficulty Bands

### Band 1 — Easy

목표 해결 시간: 10~20초

예:
- 7 + 5
- 13 - 4
- 2, 4, 6, ?
- 발판 3개 순서 기억

### Band 2 — Normal

목표 해결 시간: 20~45초

예:
- 16 + 7
- 23 - 8
- 4 × 3
- □ + 7 = 15
- 4칸 기억 경로
- 2개 상자 무게 조합

### Band 3 — Think

목표 해결 시간: 45~90초

예:
- 8 + 7 + 5
- 24 ÷ 4
- 규칙 기계 3쌍 관찰 후 출력 예측
- 간단 레이저 반사 2개
- 3~5회 이동 소코반

### Band 4 — Challenge

선택 콘텐츠에서만 주로 사용.
목표 해결 시간: 1~3분.

필수 스토리 진행에 연속 배치하지 않는다.

## 7. Arithmetic Scope

V1 범위:

### Addition

- 한 자리 + 한 자리
- 두 자리 + 한 자리
- 받아올림 포함 두 자리 + 한 자리
- 합 최대 기본 99

### Subtraction

- 한 자리
- 두 자리 - 한 자리
- 받아내림 포함
- 결과 음수 없음

### Multiplication

쉬운 구구단부터 시작.

초기 권장:
- ×2
- ×5
- ×10

이후:
- ×3
- ×4
- 나머지 단

### Division

나머지 없는 쉬운 나눗셈만 사용.

예:
- 10 ÷ 2
- 12 ÷ 3
- 20 ÷ 5

V1 기본 진행에서는 나머지 계산을 요구하지 않는다.

## 8. Core Data Architecture

문제는 ScriptableObject 템플릿 + 런타임 생성 인스턴스로 분리한다.

### QuestionTemplateDefinition

Fields:

- string TemplateId
- QuestionType Type
- LearningAxis PrimaryAxis
- List<LearningAxis> SecondaryAxes
- DifficultyBand Difficulty
- int MinChapter
- int Weight
- string PromptFormatKey
- string GeneratorId
- string ValidatorId
- HintPolicy HintPolicy
- RewardProfile RewardProfile

### QuestionInstance

런타임 순수 데이터.

Fields:

- string InstanceId
- string TemplateId
- int Seed
- string Prompt
- QuestionAnswer Answer
- List<string> Choices
- Dictionary<string, int> NumericVariables
- DifficultyBand Difficulty
- LearningAxis PrimaryAxis

UI는 QuestionInstance를 표시할 뿐 계산 로직을 포함하지 않는다.

## 9. Suggested Folder Layout

~~~text
Assets/
  MazeMath/
    Scripts/
      Questions/
        Data/
          QuestionTemplateDefinition.cs
          QuestionInstance.cs
          QuestionAnswer.cs
          QuestionDifficultyProfile.cs
        Generation/
          IQuestionGenerator.cs
          QuestionGeneratorRegistry.cs
          Arithmetic/
            AdditionGenerator.cs
            SubtractionGenerator.cs
            MultiplicationGenerator.cs
            DivisionGenerator.cs
          Pattern/
            NumberSequenceGenerator.cs
            MissingNumberGenerator.cs
        Runtime/
          QuestionService.cs
          AnswerValidator.cs
          QuestionSessionBalancer.cs
          QuestionProgressTracker.cs
      Puzzles/
        Data/
          PuzzleDefinition.cs
          PuzzleDifficultyProfile.cs
        Runtime/
          PuzzleController.cs
          PuzzleService.cs
          PuzzleHintService.cs
        Types/
          SequencePlatePuzzle.cs
          WeightBridgePuzzle.cs
          SokobanPuzzle.cs
          LaserMirrorPuzzle.cs
          MemoryPathPuzzle.cs
          PipeCircuitPuzzle.cs
          RuleMachinePuzzle.cs
      Rewards/
        RewardService.cs
        LearningReward.cs
      UI/
        Question/
          QuestionPanel.cs
          MultipleChoiceView.cs
          NumericInputView.cs
          NumericKeypadView.cs
          StepInputView.cs
          FeedbackView.cs
        Puzzle/
          PuzzleHintView.cs
~~~

## 10. Question Service Contract

~~~csharp
public interface IQuestionService
{
    QuestionInstance Create(string templateId, int seed);
    AnswerResult Submit(string instanceId, QuestionResponse response);
    QuestionHint GetHint(string instanceId, int hintLevel);
}
~~~

QuestionResponse supports:

- ChoiceIndex
- IntegerValue
- IntegerValues
- PatternSelection

## 11. Generator Contract

~~~csharp
public interface IQuestionGenerator
{
    QuestionInstance Generate(
        QuestionTemplateDefinition template,
        int seed,
        PlayerLearningState learningState);
}
~~~

각 Generator는 다음을 보장해야 한다.

- 정답이 하나 이상 존재
- Numeric Input의 정답은 정확히 하나
- 객관식 정답은 Choice 중 정확히 하나
- 오답 선택지는 중복되지 않음
- 선택지에 정답과 동치 표현 중복 없음
- Difficulty 범위를 벗어나는 숫자 생성 금지

## 12. Deterministic Random Rule

질문 생성은 01 미로와 마찬가지로 프로젝트 전용 deterministic PRNG를 사용한다.

QuestionSeed = StableHash(RunSeed + NodeId + AttemptIndex + TemplateId)

동일 QuestionSeed는 플랫폼과 프레임 수에 상관없이 동일 문제를 만든다.

재도전에서 똑같은 문제를 계속 보여주지 않도록 AttemptIndex가 증가한다.

단, 앱 강제 종료 후 같은 미완료 문제를 복구할 때는 저장된 QuestionInstance 또는 Seed를 복원해 문제를 바꾸지 않는다.

## 13. Multiple Choice Questions

사용 목적:

- 새 개념 도입
- 공간/도형 문제
- 계산보다 추론이 중요한 문제
- 초기 챕터

선택지 수:

- Easy: 3
- Normal 이상: 4

오답 Distractor 생성 규칙 예:

16 + 7 = 23

가능한 오답:
- 22: 1 차이
- 24: 1 차이
- 13: 받아올림 실수 기반

완전 무작위 숫자보다 흔한 실수와 가까운 숫자를 사용하되, 지나치게 유도하지 않는다.

선택지 순서는 Seed 기반 Shuffle.

## 14. Numeric Input Questions

주관식 숫자 입력을 핵심 유형으로 사용한다.

예:

16 + 7 = ?

UI:

~~~text
┌──────────┐
│    23    │
└──────────┘

[1] [2] [3]
[4] [5] [6]
[7] [8] [9]
[←] [0] [확인]
~~~

### Rules

- 모바일에서 OS 키보드를 기본으로 띄우지 않는다.
- 게임 내부 NumericKeypad를 사용한다.
- PC에서는 숫자 키와 게임 Keypad 모두 입력 가능.
- Enter = 확인
- Backspace = 한 자리 삭제
- 최대 입력 길이는 문제마다 지정.
- 앞의 0은 자동 정규화.
- 빈 값은 Submit 불가.
- V1은 음수 기호와 소수점을 제공하지 않는다.

## 15. Numeric Input Feedback

정답:

- 초록색/밝은 성공 표시
- 짧은 효과음
- "정답이에요!"
- 보상 표시

오답:

- 입력칸 흔들림
- "한 번 더 생각해 볼까요?"
- 입력값은 기본적으로 유지하여 아이가 자기 답을 볼 수 있게 한다.

2회 오답:

- Hint Level 1 제공 버튼 강조

3회 오답:

- Hint Level 2 자동 제안

정답 자체를 바로 공개하지 않는다.

## 16. Step Input Questions

계산 과정을 2단계로 분리한다.

예:

8 + 7 + 5

Step 1:
8 + 7 = [15]

Step 2:
15 + 5 = [20]

앞 Step이 틀리면 다음 Step을 잠근다.

목적:

- 계산 과정을 나눠 작업기억 부담 조절
- 중간값을 스스로 확인

필수 진행에서 3단계 이상은 사용하지 않는다.

## 17. Missing Number Questions

예:

□ + 7 = 15

Numeric Input:
□ = [8]

또는:

4 × □ = 20

□ = [5]

Chapter 난이도에 따라 역연산 이해를 자연스럽게 도입한다.

## 18. Pattern & Rule Questions

예:

2, 4, 6, ?

또는 Rule Machine:

2 → 4
3 → 6
4 → ?

Rule 종류 V1:

- +1 / +2 / +5 / +10
- -1 / -2
- ×2
- 교대 규칙은 Optional Challenge에서만 사용

한 문제에서 규칙 후보가 여러 개 성립하지 않도록 생성 후 Validator가 검증한다.

## 19. Session Balancer

QuestionSessionBalancer는 직전 문제 기록을 보고 다음 문제 후보 가중치를 조정한다.

피해야 할 패턴:

- 덧셈 5개 연속
- Numeric Input만 6개 연속
- 같은 Template 2회 연속
- 같은 생성 숫자 조합의 짧은 반복

권장:

- 동일 PrimaryAxis 최대 3회 연속
- 동일 QuestionType 최대 3회 연속
- 직전 10문제 InstanceSignature 중복 금지

InstanceSignature 예:

TemplateId + normalized operands + answer

## 20. Adaptive Difficulty

초기 V1에서는 복잡한 AI 적응을 만들지 않는다.

간단한 규칙 기반으로만 조절한다.

PlayerLearningState 저장 항목:

- LearningAxis별 최근 10문제 정오
- 평균 시도 횟수
- 평균 해결 시간
- Hint 사용 횟수

상향 조건 예:

- 최근 5문제 중 4개 이상 정답
- 평균 시도 1.5 이하
- Hint 사용 1회 이하

하향 조건 예:

- 최근 5문제 중 2개 이하 정답
- 동일 문제 3회 이상 시도 반복
- Hint Level 2 이상 연속 사용

Difficulty는 한 번에 1 Band만 변화한다.

## 21. Environment Puzzle Definition

환경 퍼즐은 QuestionPanel을 띄우는 대신 실제 게임 월드 오브젝트와 상호작용해 해결한다.

공통 인터페이스:

~~~csharp
public interface IEnvironmentPuzzle
{
    string PuzzleId { get; }
    PuzzleState State { get; }

    void StartPuzzle();
    void ResetPuzzle();
    bool IsSolved();
    PuzzleHint GetHint(int level);
}
~~~

PuzzleState:

- Locked
- Ready
- Active
- Solved

## 22. Environment Puzzle Difficulty Standard

### Easy

10~20초.
관찰 즉시 행동 가능.

### Normal

30~60초.
1~2번 생각/시도 필요.

### Think

1~3분.
행동 순서 계획 필요.

필수 퍼즐이 90초 이상 진행 없이 정체되면 Hint Level 1을 제안한다.

3분 이상 미해결이면 Level 3 또는 Skip Assist 옵션을 제공할 수 있다.

Skip Assist로 통과해도 스토리 진행을 막지 않는다.

## 23. Puzzle Type A — Sequence Plates

### Example

바닥 발판:

2 / 6 / 4

벽 단서:

2 → 4 → 6

플레이어가 실제로 2 → 4 → 6 순서로 밟으면 문이 열린다.

### Difficulty

Easy:
- 3개 발판
- 단서 계속 표시

Normal:
- 4개 발판
- 단서가 5초 후 사라짐

Think:
- 4~5개
- 숫자 대신 색 + 숫자 혼합 가능

### Learning Axes

- WorkingMemory
- Sequence
- VisualSearch

### Fail Rule

틀린 발판을 밟으면 즉시 전체 데미지를 주지 않고 부드러운 Reset.

## 24. Puzzle Type B — Weight Bridge

### Example

필요 무게: 8

오브젝트:
- 상자 A = 3
- 상자 B = 5
- 상자 C = 2

3 + 5를 발판 위에 올리면 다리가 열린다.

### Difficulty

Easy:
- 정답 조합 1개
- 오브젝트 3개

Normal:
- 오브젝트 4개
- 불필요 상자 존재

Think:
- 두 개 발판에 목표 무게를 각각 맞춤

### Learning Axes

- Addition
- MultiStepPlanning
- SpatialPlanning

### Generation Constraint

정답 조합이 최소 1개이고 의도하지 않은 더 쉬운 조합이 없는지 Exhaustive Validator로 검사한다.

## 25. Puzzle Type C — Sokoban Lite

상자를 목표 칸으로 밀어 넣는다.

7세 기준 제한:

Easy:
- 상자 1개
- 목표 1개
- 최소 2~3회 밀기

Normal:
- 상자 1~2개
- 최소 3~5회 밀기

Think:
- 최대 2개 상자
- 최소 계획 단계 5~7

필수 콘텐츠에서 상자 3개 이상 금지.

### Anti Soft-Lock

- 언제든 Reset 버튼 제공
- Reset은 퍼즐 시작 위치로만 복구
- 퍼즐 외 진행 상태/보상은 되돌리지 않음

### Learning Axes

- SpatialPlanning
- MultiStepPlanning

## 26. Puzzle Type D — Laser Mirror

발사기 → 거울 → 목표 센서.

플레이어가 거울을 90도 단위로 회전한다.

V1 제한:

- 거울 최대 3개
- 빛 경로는 2D Grid 기반
- 분광/색 혼합 없음

Easy:
- 거울 1개

Normal:
- 거울 2개

Think:
- 거울 3개 + 불필요 거울 1개

### Learning Axes

- SpatialRotation
- RuleInference

## 27. Puzzle Type E — Memory Path

진입 시 안전 발판이 잠시 표시된다.

예:

시작 → 오른쪽 → 오른쪽 → 위 → 오른쪽

표시가 사라진 뒤 기억해서 이동.

난이도:

- Easy: 3칸
- Normal: 4칸
- Think: 5~6칸

2회 실패 시:

- 표시 시간을 늘림

3회 실패 시:

- 이미 밟은 정답 경로는 약하게 유지

### Learning Axes

- WorkingMemory
- Sequence

## 28. Puzzle Type F — Pipe/Circuit

전선/파이프 타일을 90도 회전해 발전기와 목표를 연결한다.

V1:

- Grid 3x3 또는 4x4
- T 분기 최소화
- Loop 정답 금지
- 최소 회전 횟수 기반 난이도 정의

Easy:
- 최소 2회 회전

Normal:
- 3~5회

Think:
- 5~8회

### Learning Axes

- SpatialRotation
- VisualSearch
- MultiStepPlanning

## 29. Puzzle Type G — Rule Machine

환경 오브젝트로 Rule Question을 표현한다.

기계 화면:

2 → 4
3 → 6
4 → ?

플레이어가 숫자 블록 8을 찾아 출력 슬롯에 넣거나 Numeric Pad로 8 입력.

Question 시스템과 Environment Puzzle의 중간 형태.

### Learning Axes

- RuleInference
- Multiplication 또는 Addition

## 30. Environment Puzzle Placement

01 Maze Node와 연결.

Puzzle Room에는 PuzzleId를 가진 PuzzleController 배치.

Maze Gate:

- GateType = EnvironmentPuzzle
- RequirementId = PuzzleId

Puzzle 해결 시:

1. PuzzleService 상태 Solved
2. RewardService 보상 지급
3. IMazeService.SolveGate(gateId)
4. Checkpoint 조건이면 저장

## 31. Boss Puzzle Integration

보스는 단순 HP 공격보다 "문제/퍼즐로 보호막 해제 → 짧은 액션 구간" 구조를 사용한다.

Example: Stone Golem

Shield 3개.

Phase 1:
- Numeric Input Question 2개 중 1개 해결
- Shield -1

Phase 2:
- Sequence Plate
- Shield -1

Phase 3:
- Laser Mirror 또는 Weight Bridge
- Shield -1

보호막 0:
- 플레이어 장비를 사용한 짧은 공격/상호작용
- Boss clear

보스 대사 예:
"훌륭하십니다. 첫 번째 보호 장치를 해제하셨군요."

실패 시 조롱하거나 벌점을 주지 않는다.

## 32. Hint System

Question과 Puzzle은 공통 HintPolicy를 사용한다.

### Hint Level 1 — Observe

관찰 포인트.

예:
"10을 먼저 만들어 보면 어떨까요?"

"벽에 적힌 숫자 순서를 살펴보세요."

### Hint Level 2 — Strategy

해결 방법 방향.

예:
"16에 4를 더하면 먼저 20이 돼요."

"3kg과 5kg 상자를 함께 올려볼까요?"

### Hint Level 3 — Strong Assist

거의 정답 직전까지 지원.

예:
"16 + 7을 16 + 4 + 3으로 나눠 보세요."

환경 퍼즐은 다음 상호작용 오브젝트 강조.

### Hint Level 4 — Accessibility Assist

보호자 옵션에서 허용.

정답/정확한 경로를 표시하고 진행 가능.

## 33. Hint Reward Policy

힌트 사용으로 기본 클리어 보상을 제거하지 않는다.

Optional Challenge에만 "힌트 없이 해결" 보너스 배지를 줄 수 있다.

Knowledge XP는 학습 동기 강화를 위해 힌트 여부와 무관하게 기본 지급.

## 34. Reward Service

03 시스템은 직접 Inventory/Enchant를 변경하지 않는다.

~~~csharp
public interface IRewardService
{
    RewardGrantResult Grant(LearningReward reward, RewardSource source);
}
~~~

LearningReward:

- int KnowledgeXp
- List<ItemReward> Items
- List<string> UnlockRecipeIds
- List<string> UnlockClueIds

02 Progression System이 실제 인벤토리와 XP를 갱신한다.

## 35. Reward Examples

Easy Question:
- 5 Knowledge XP
- Scrap 0~1

Normal Numeric:
- 8 Knowledge XP
- Scrap 1

Think Question:
- 12 Knowledge XP
- Gear 또는 Energy Crystal 확률 보상

Environment Puzzle:
- 10~15 Knowledge XP
- Material 1~2

Boss Phase:
- 20 Knowledge XP

Boss Clear:
- 고정 Story Reward
- 선택 Rare Core

필수 제작 재료 확보는 확률에만 의존하지 않는다.

## 36. Wrong Answer Policy

금지:

- HP 감소
- 가진 재료 삭제
- XP 감소
- 문제 1회 실패로 방 처음부터 리셋
- 부정적인 NPC 대사

사용:

- 짧은 시각 피드백
- 다시 시도
- 단계 힌트
- 필요 시 난이도 완화

## 37. Save Data

~~~csharp
[Serializable]
public sealed class LearningSaveData
{
    public List<AxisProgressSaveData> axisProgress;
    public List<QuestionProgressSaveData> activeQuestions;
    public List<PuzzleProgressSaveData> activePuzzles;
    public List<string> solvedPuzzleIds;
}
~~~

QuestionProgressSaveData:

- instanceId
- templateId
- seed
- attemptIndex
- serializedQuestionData
- hintLevel
- incorrectAttempts

PuzzleProgressSaveData:

- puzzleId
- state
- serializedRuntimeState
- hintLevel
- attemptCount

환경 퍼즐이 Scene 재로드 시 중간 상태 복원이 어렵다면 V1에서는 "해결 전 상태로 Reset"을 허용하되 이미 지급된 보상은 중복 지급하지 않는다.

## 38. UI Rules

### Question Panel

화면 중앙.
배경 게임은 Pause 또는 입력 잠금.

표시:

- 문제 텍스트
- 필요한 그림
- 정답 입력
- 힌트
- 진행 상태

### Numeric Keypad

터치 타깃 최소 48dp 상당 크기 확보.

숫자 버튼 간 충분한 간격.

확인 버튼은 숫자 버튼보다 시각적으로 구분.

### Environment Puzzle

화면 중앙 팝업보다 게임 월드를 우선.

필요 시 상단에 한 줄 목표만 표시.

예:
"상자 무게의 합을 8로 만들어 다리를 내려보자."

## 39. Accessibility

기본 제공:

- 시간 제한 없음
- 색만으로 정답/오답 구분하지 않음
- 숫자 + 아이콘 병행
- 한글 가독성이 좋은 폰트
- 효과음 없이도 피드백 이해 가능
- 모션 감소 옵션 고려
- Hint Level 4 보호자 옵션
- 문제 읽기 부담을 줄이기 위한 짧은 문장

향후 TTS를 추가할 수 있도록 Prompt는 Localization Key 기반으로 관리한다.

## 40. Localization

모든 Prompt/Feedback/Hint는 코드 문자열 하드코딩 금지.

Localization Key 예:

- QUESTION.ADD.INPUT
- QUESTION.FEEDBACK.CORRECT
- QUESTION.FEEDBACK.RETRY
- PUZZLE.WEIGHT.OBJECTIVE
- HINT.MEMORY_PATH.LEVEL1

숫자/연산자는 런타임 포맷 인자로 삽입.

## 41. Analytics Events

로컬 디버그 로그부터 정의하고 외부 분석 SDK는 나중에 선택한다.

Event:

- question_started
- question_answered
- question_hint_used
- question_completed
- puzzle_started
- puzzle_reset
- puzzle_hint_used
- puzzle_completed

Properties:

- templateId / puzzleId
- learningAxis
- difficulty
- attemptCount
- solveSeconds
- hintLevel
- platform

개인 식별 정보는 저장하지 않는다.

## 42. EditMode Tests

Question:

1. 같은 seed → 같은 문제.
2. Numeric Input 정답이 정확히 하나.
3. 객관식 정답 Choice가 정확히 하나.
4. Choice 중복 없음.
5. Chapter 범위를 벗어난 연산 숫자 생성 없음.
6. Division은 나머지 없는 문제만 생성.
7. Subtraction 결과 음수 없음.
8. Pattern Question의 규칙이 유일.
9. SessionBalancer가 동일 Template 연속을 제한.
10. 최근 10개 InstanceSignature 중복 방지.

Puzzle:

11. Weight Puzzle은 최소 한 개 정답 조합 존재.
12. Weight Puzzle 의도치 않은 쉬운 정답 검출.
13. Sokoban 기본 템플릿이 해결 가능.
14. Laser 경로가 목표에 도달하는 해답 존재.
15. Pipe Puzzle이 최소 회전 수 조건 만족.
16. Memory Path 길이가 Difficulty 범위 내.

## 43. PlayMode Tests

1. Numeric Keypad만으로 정답 제출 가능.
2. PC 숫자 키 입력과 Keypad 결과 동일.
3. 오답 후 입력/힌트 상태 정상.
4. 정답 후 Reward가 정확히 1회 지급.
5. 문제 완료 시 Question Gate 해제.
6. Sequence Plate 오답 → Reset → 재시도 가능.
7. Sokoban Reset이 다른 진행 상태를 되돌리지 않음.
8. Puzzle 완료 시 EnvironmentPuzzle Gate 해제.
9. Boss Shield가 각 Phase 해결당 정확히 1 감소.
10. Save/Reload 후 완료 보상이 중복 지급되지 않음.
11. Android 해상도에서 Keypad 버튼 겹침 없음.
12. Web 해상도 변경 후 Question Panel이 화면 밖으로 벗어나지 않음.

## 44. Content Authoring Rules

새 Question을 추가할 때:

1. LearningAxis 정의
2. Difficulty 정의
3. Generator 선택
4. 숫자 범위 설정
5. Hint 1~3 작성
6. RewardProfile 연결
7. 1,000 seed generator validation 실행

새 Environment Puzzle을 추가할 때:

1. 30초 내 규칙 설명 가능 여부 확인
2. Reset 가능
3. Soft Lock 없음
4. Hint 1~3 존재
5. Solver 또는 사전 검증된 Template 보유
6. Gate/Reward 연결
7. 터치 입력만으로 해결 가능

## 45. Difficulty Safety Rules

필수 진행 문제:

- Challenge Band 2개 연속 금지
- 같은 LearningAxis 3개 초과 연속 금지
- 같은 Environment Puzzle 종류 연속 금지
- 3분 이상 막히는 경우 Assist 제공
- 문제 실패 때문에 미로 전체를 다시 돌게 하지 않음

## 46. Acceptance Criteria

Puzzle & Question V1 완료 조건:

Question:
- Multiple Choice 구현
- Numeric Input 구현
- Step Input 구현
- Missing Number 구현
- Pattern/Rule 구현
- 덧셈/뺄셈/쉬운 곱셈/쉬운 나눗셈 Generator 구현
- Seed 기반 문제 변경
- 직전 문제 중복 방지
- Hint Level 1~3

Environment:
- Sequence Plates 구현
- Weight Bridge 구현
- Sokoban Lite 구현
- Memory Path 구현
- Laser 또는 Pipe 중 최소 1개 구현
- Reset 및 Soft Lock 방지
- Maze Gate 연동

Integration:
- Reward → 02 시스템 연동
- Gate → 01 시스템 연동
- Boss 3단계 Puzzle Shield 데모
- Web + Android 터치 조작
- Save/Reload 시 중복 보상 없음

## 47. Implementation Order

Phase Q1 — Question data model + deterministic generator contract
Phase Q2 — Addition/Subtraction generators
Phase Q3 — Numeric Input + in-game keypad
Phase Q4 — Multiple Choice + distractor validation
Phase Q5 — Multiplication/Division + Missing Number
Phase Q6 — SessionBalancer + adaptive rules
Phase P1 — Common PuzzleController contract
Phase P2 — Sequence Plate + Memory Path
Phase P3 — Weight Bridge
Phase P4 — Sokoban Lite
Phase P5 — Laser or Pipe puzzle
Phase QP1 — Reward integration
Phase QP2 — Maze Gate integration
Phase QP3 — Boss shield integration
Phase QP4 — Save/Reload regression
Phase QP5 — Web + Android smoke test

## 48. Cross-System Contracts

Consumes from 01:

- RunSeed
- NodeId
- IMazeService.SolveGate
- GateDefinition

Consumes from 02:

- IRewardService implementation
- Knowledge XP / Material reward destination

Provides:

- IQuestionService
- IEnvironmentPuzzle / PuzzleService
- LearningReward
- Question/Puzzle completion events

Subsystem boundary rule:

Question/Puzzle 코드가 Maze UI, Inventory UI, Workshop UI를 직접 참조하지 않는다. 완료 결과는 service/event를 통해 전달한다.

## 49. Environment Puzzle Quality Bar

환경 퍼즐은 다음 질문에 모두 Yes여야 필수 콘텐츠에 넣을 수 있다.

1. 아이가 화면을 보고 무엇을 만질 수 있는지 알 수 있는가?
2. 실패 이유를 이해할 수 있는가?
3. Reset 후 즉시 다시 시도할 수 있는가?
4. 정답을 몰라도 1~2번 실험하며 규칙을 배울 수 있는가?
5. 계산만 하는 팝업과 다른 플레이 경험을 주는가?
6. 해결이 미로의 문, 다리, 엘리베이터, 보스 등 실제 세계 변화로 이어지는가?

이 기준을 만족하지 못하면 Environment Puzzle이 아니라 일반 Question으로 처리한다.
