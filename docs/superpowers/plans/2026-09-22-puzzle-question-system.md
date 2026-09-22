# Puzzle & Question System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement deterministic math questions, numeric input, adaptive session balancing, and world-space environment puzzles that unlock maze gates and grant progression rewards.

**Architecture:** Question generation remains pure C# and seed-driven. UI consumes immutable `QuestionInstance` data. Environment puzzles share a small runtime contract and signal completion through `PuzzleService`, which then calls maze/reward interfaces rather than directly modifying inventory or gate MonoBehaviours.

**Tech Stack:** Unity 2022.3.62f2 LTS, C#, Unity Test Framework, ScriptableObjects, project UI stack.

**Spec:** `Docs/Design/03_PUZZLE_QUESTION_SYSTEM_DESIGN.md`

## Global Constraints

- Same question seed yields same generated question.
- Numeric input uses an in-game keypad on mobile.
- Division in V1 has no remainder.
- Subtraction never yields negative answers.
- Required puzzles must be resettable and cannot soft-lock.
- Wrong answers never remove HP, XP, or materials.
- Hints escalate from observation → strategy → strong assist.
- Rewards are granted exactly once.
- Question/Puzzle code cannot reference inventory UI, maze UI, or workshop UI directly.

## Review Focus

1. Resuming an unfinished question after reload must not generate a different question.
2. Rapid double-submit must not grant reward twice.
3. Numeric keypad must reject empty input and normalize leading zeros safely.
4. Puzzle reset must restore only puzzle-local state, not global progress.
5. Generated rule/weight puzzles must have an intended valid solution and no accidental easier solution where the spec forbids it.

---

### Task 1: Add question domain model and generator registry

**Files:**
- Create: `Assets/MazeMath/Scripts/Questions/Data/QuestionTypes.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Data/QuestionTemplateDefinition.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Data/QuestionInstance.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Generation/IQuestionGenerator.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Generation/QuestionGeneratorRegistry.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Questions/QuestionModelTests.cs`

**Interfaces:**
- Produces: `QuestionInstance Generate(...)` contract and registry lookup by GeneratorId.
- Consumes: deterministic random utility from Maze plan.

- [ ] **Step 1: Write failing registry test**

Register a fake generator with id `addition`; assert lookup returns the same instance and unknown id returns false.

- [ ] **Step 2: Implement domain enums and registry**

Required enums:
- QuestionType
- LearningAxis
- DifficultyBand

- [ ] **Step 3: Run EditMode tests**

- [ ] **Step 4: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Questions Assets/MazeMath/Tests/EditMode/Questions
git commit -m "feat: add question domain model"
~~~

---

### Task 2: Implement addition and subtraction generators

**Files:**
- Create: `Assets/MazeMath/Scripts/Questions/Generation/Arithmetic/AdditionGenerator.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Generation/Arithmetic/SubtractionGenerator.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Questions/ArithmeticGeneratorTests.cs`

**Interfaces:**
- Produces deterministic integer question instances.
- Consumes: `QuestionTemplateDefinition`, `DeterministicRandom`.

- [ ] **Step 1: Write failing deterministic addition test**

Generate seed 123 twice and assert operands, prompt variables, and answer match.

- [ ] **Step 2: Implement addition generator**

Respect configured maximum sum and difficulty range.

- [ ] **Step 3: Write failing non-negative subtraction test**

Loop 1,000 seeds and assert answer >= 0.

- [ ] **Step 4: Implement subtraction generator**

Arrange operands so minuend >= subtrahend.

- [ ] **Step 5: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Questions/Generation/Arithmetic Assets/MazeMath/Tests/EditMode/Questions
git commit -m "feat: generate deterministic addition and subtraction"
~~~

---

### Task 3: Implement numeric answer validation and keypad UI

**Files:**
- Create: `Assets/MazeMath/Scripts/Questions/Runtime/AnswerValidator.cs`
- Create: `Assets/MazeMath/Scripts/UI/Question/NumericInputView.cs`
- Create: `Assets/MazeMath/Scripts/UI/Question/NumericKeypadView.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Questions/AnswerValidatorTests.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Questions/NumericKeypadTests.cs`

**Interfaces:**
- Produces: integer response validation and keypad events.
- Consumes: `QuestionInstance.Answer`.

- [ ] **Step 1: Write failing validator tests**

Cover:
- correct integer
- incorrect integer
- empty input rejected
- `00023` normalized to 23
- maximum input length enforced

- [ ] **Step 2: Implement answer validator**

- [ ] **Step 3: Write failing keypad PlayMode test**

Simulate taps 2, 3, backspace, 4, submit. Assert submitted value is 24.

- [ ] **Step 4: Implement keypad**

Buttons:
1-9, 0, backspace, confirm.

- [ ] **Step 5: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Questions/Runtime Assets/MazeMath/Scripts/UI/Question Assets/MazeMath/Tests
git commit -m "feat: add numeric answer input"
~~~

---

### Task 4: Add multiple-choice and distractor generation

**Files:**
- Create: `Assets/MazeMath/Scripts/Questions/Generation/ChoiceDistractorBuilder.cs`
- Create: `Assets/MazeMath/Scripts/UI/Question/MultipleChoiceView.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Questions/ChoiceDistractorTests.cs`

**Interfaces:**
- Produces 3-4 unique choices with exactly one correct answer.

- [ ] **Step 1: Write failing uniqueness test**

For 1,000 generated questions assert:
- choices distinct
- correct answer occurs exactly once

- [ ] **Step 2: Implement misconception-based distractors**

Use near-answer and common carry/borrow errors before fallback random distinct values.

- [ ] **Step 3: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Questions/Generation/ChoiceDistractorBuilder.cs Assets/MazeMath/Scripts/UI/Question/MultipleChoiceView.cs Assets/MazeMath/Tests/EditMode/Questions
git commit -m "feat: add multiple choice questions"
~~~

---

### Task 5: Add multiplication, division, missing number, and step input

**Files:**
- Create: `Assets/MazeMath/Scripts/Questions/Generation/Arithmetic/MultiplicationGenerator.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Generation/Arithmetic/DivisionGenerator.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Generation/Pattern/MissingNumberGenerator.cs`
- Create: `Assets/MazeMath/Scripts/UI/Question/StepInputView.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Questions/ExtendedQuestionTests.cs`

**Interfaces:**
- Produces V1 arithmetic scope from spec.

- [ ] **Step 1: Write failing division remainder test**

Generate 1,000 questions; assert dividend % divisor == 0.

- [ ] **Step 2: Implement multiplication/division generators**

Early difficulty prioritizes ×2, ×5, ×10.

- [ ] **Step 3: Write missing-number test**

Example generated form `x + 7 = 15` must validate x=8 as the unique answer.

- [ ] **Step 4: Implement missing-number generator**

- [ ] **Step 5: Write StepInput progression test**

Second step disabled until first answer is correct.

- [ ] **Step 6: Implement StepInputView**

- [ ] **Step 7: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Questions Assets/MazeMath/Scripts/UI/Question Assets/MazeMath/Tests/EditMode/Questions
git commit -m "feat: add extended math question types"
~~~

---

### Task 6: Add sequence/rule questions and session balancer

**Files:**
- Create: `Assets/MazeMath/Scripts/Questions/Generation/Pattern/NumberSequenceGenerator.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Runtime/QuestionSessionBalancer.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Runtime/PlayerLearningState.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Questions/QuestionBalancerTests.cs`

**Interfaces:**
- Produces weighted next-question selection.
- Consumes recent signatures, learning-axis history, hint/attempt statistics.

- [ ] **Step 1: Write failing duplicate-prevention test**

Feed the last 10 signatures and assert selected next instance is not one of them.

- [ ] **Step 2: Write failing repetition-limit test**

Assert same LearningAxis and QuestionType are not selected more than 3 consecutive times when alternatives exist.

- [ ] **Step 3: Implement balancer**

- [ ] **Step 4: Implement simple one-band adaptive rule**

Raise/lower at most one DifficultyBand per evaluation.

- [ ] **Step 5: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Questions Assets/MazeMath/Tests/EditMode/Questions
git commit -m "feat: balance and adapt question sessions"
~~~

---

### Task 7: Add QuestionService, hints, reward-once guard, and save

**Files:**
- Create: `Assets/MazeMath/Scripts/Questions/Runtime/QuestionService.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Runtime/QuestionHintService.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Runtime/LearningSaveData.cs`
- Create: `Assets/MazeMath/Scripts/Questions/Runtime/QuestionSaveAdapter.cs`
- Create: `Assets/MazeMath/Scripts/Rewards/IRewardService.cs`
- Create: `Assets/MazeMath/Scripts/Rewards/LearningReward.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Questions/QuestionServiceTests.cs`

**Interfaces:**
- Produces `IQuestionService` from spec.
- Produces `IRewardService.Grant` contract for progression subsystem.

- [ ] **Step 1: Write failing double-submit test**

Submit the same correct response twice. Assert completion event and reward grant occur once.

- [ ] **Step 2: Implement QuestionService completion guard**

- [ ] **Step 3: Write save/resume test**

Save unfinished question seed/serialized data; restore and assert same prompt/answer.

- [ ] **Step 4: Implement save adapter**

- [ ] **Step 5: Add hint level tests and implement hint service**

- [ ] **Step 6: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Questions Assets/MazeMath/Scripts/Rewards Assets/MazeMath/Tests/EditMode/Questions
git commit -m "feat: manage question lifecycle and rewards"
~~~

---

### Task 8: Add common environment puzzle framework

**Files:**
- Create: `Assets/MazeMath/Scripts/Puzzles/Runtime/IEnvironmentPuzzle.cs`
- Create: `Assets/MazeMath/Scripts/Puzzles/Runtime/PuzzleService.cs`
- Create: `Assets/MazeMath/Scripts/Puzzles/Runtime/PuzzleController.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Puzzles/PuzzleServiceTests.cs`

**Interfaces:**
- Produces puzzle states: Locked, Ready, Active, Solved.
- Produces completion event with PuzzleId.
- Consumes reward and maze gate interfaces.

- [ ] **Step 1: Write failing solve-once test**

Solve a fake puzzle twice; assert gate/reward callbacks happen once.

- [ ] **Step 2: Implement common service and controller**

- [ ] **Step 3: Write reset-scope test**

Puzzle reset restores puzzle-local state without clearing maze solved gates or inventory fake state.

- [ ] **Step 4: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Puzzles Assets/MazeMath/Tests/PlayMode/Puzzles
git commit -m "feat: add environment puzzle framework"
~~~

---

### Task 9: Implement Sequence Plate and Memory Path puzzles

**Files:**
- Create: `Assets/MazeMath/Scripts/Puzzles/Types/SequencePlatePuzzle.cs`
- Create: `Assets/MazeMath/Scripts/Puzzles/Types/MemoryPathPuzzle.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Puzzles/SequenceAndMemoryTests.cs`

**Interfaces:**
- Produces ordered-input puzzle state.

- [ ] **Step 1: Write sequence reset test**

Correct 2 → wrong 6 when expected 4 must reset progress to index 0.

- [ ] **Step 2: Implement SequencePlatePuzzle**

- [ ] **Step 3: Write memory-path difficulty test**

Easy path length=3, Normal=4, Think=5-6.

- [ ] **Step 4: Implement MemoryPathPuzzle**

- [ ] **Step 5: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Puzzles/Types Assets/MazeMath/Tests/EditMode/Puzzles
git commit -m "feat: add sequence and memory puzzles"
~~~

---

### Task 10: Implement Weight Bridge and Sokoban Lite

**Files:**
- Create: `Assets/MazeMath/Scripts/Puzzles/Types/WeightBridgePuzzle.cs`
- Create: `Assets/MazeMath/Scripts/Puzzles/Types/SokobanPuzzle.cs`
- Create: `Assets/MazeMath/Scripts/Puzzles/Validation/WeightPuzzleSolver.cs`
- Create: `Assets/MazeMath/Scripts/Puzzles/Validation/SokobanSolver.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Puzzles/PlanningPuzzleTests.cs`

**Interfaces:**
- Produces validated puzzle templates with known solutions.

- [ ] **Step 1: Write weight exhaustive-solution test**

Given target 8 and weights 3,5,2, assert intended combination 3+5 exists; for a strict template reject one with an unintended single weight 8.

- [ ] **Step 2: Implement WeightPuzzleSolver and runtime**

- [ ] **Step 3: Write Sokoban solvability test**

Feed a small board and assert solver finds a path within allowed push depth.

- [ ] **Step 4: Implement Sokoban Lite with Reset**

- [ ] **Step 5: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Puzzles Assets/MazeMath/Tests/EditMode/Puzzles
git commit -m "feat: add weight and sokoban puzzles"
~~~

---

### Task 11: Implement Laser Mirror or Pipe Circuit V1

**Files:**
- Create: `Assets/MazeMath/Scripts/Puzzles/Types/LaserMirrorPuzzle.cs`
- Create: `Assets/MazeMath/Scripts/Puzzles/Validation/LaserPathSolver.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Puzzles/LaserPuzzleTests.cs`

**Interfaces:**
- Produces 2D-grid reflection puzzle with 90-degree mirror rotation.

- [ ] **Step 1: Write failing known-layout solver test**

Provide a fixed grid requiring two mirror rotations; assert solver reaches target.

- [ ] **Step 2: Implement laser path simulation**

Stop on:
- wall
- out of bounds
- loop detection
- target hit

- [ ] **Step 3: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Puzzles Assets/MazeMath/Tests/EditMode/Puzzles
git commit -m "feat: add laser mirror puzzle"
~~~

---

### Task 12: Integrate question/puzzles with Maze gates and rewards

**Files:**
- Create: `Assets/MazeMath/Scripts/Questions/Runtime/QuestionGateBinding.cs`
- Create: `Assets/MazeMath/Scripts/Puzzles/Runtime/PuzzleGateBinding.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Integration/LearningGateIntegrationTests.cs`

**Interfaces:**
- Consumes: `IMazeService.SolveGate`.
- Produces: gate solve exactly once after successful completion.

- [ ] **Step 1: Write failing Question Gate integration test**

Correct answer → gate opens. Wrong answer → gate remains closed.

- [ ] **Step 2: Implement QuestionGateBinding**

- [ ] **Step 3: Write failing Puzzle Gate integration test**

Puzzle solve → reward once + gate open once.

- [ ] **Step 4: Implement PuzzleGateBinding**

- [ ] **Step 5: Run PlayMode tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Questions Assets/MazeMath/Scripts/Puzzles Assets/MazeMath/Tests/PlayMode/Integration
git commit -m "feat: connect learning systems to maze gates"
~~~

---

### Task 13: Build three-phase Golem boss learning demo

**Files:**
- Create: `Assets/MazeMath/Scripts/Boss/GolemBossController.cs`
- Create: `Assets/MazeMath/Prefabs/Boss/GolemBoss.prefab`
- Create: `Assets/MazeMath/Tests/PlayMode/Boss/GolemBossTests.cs`

**Interfaces:**
- Consumes question/puzzle completion events.
- Produces shield count 3 → 0 and boss clear event.

- [ ] **Step 1: Write failing shield decrement test**

Each unique completed phase removes exactly one shield. Duplicate completion does not remove another.

- [ ] **Step 2: Implement three phases**

Phase 1: Numeric Input
Phase 2: Sequence Plate
Phase 3: Laser or Weight Bridge

- [ ] **Step 3: Add boss completion test**

Assert boss cannot clear at shield > 0 and clears at 0 after final interaction.

- [ ] **Step 4: Run full EditMode + PlayMode suite and commit**

~~~bash
git add Assets/MazeMath/Scripts/Boss Assets/MazeMath/Prefabs/Boss Assets/MazeMath/Tests/PlayMode/Boss
git commit -m "feat: add puzzle-driven golem boss"
~~~

## Plan Completion Gate

Puzzle/Question V1 is complete when all specified question types generate deterministically, numeric keypad works on touch, at least five environment puzzle types are playable/resettable, rewards and maze gates fire exactly once, save/resume is stable, the Golem demo works, and full EditMode/PlayMode suites plus Web/Android smoke builds pass.
