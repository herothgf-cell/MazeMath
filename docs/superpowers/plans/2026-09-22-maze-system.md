# Maze System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a deterministic, validated, multi-floor hybrid maze with gates, backtracking, fog-of-war mapping, objectives, hints, and checkpoint recovery.

**Architecture:** Keep maze generation as pure C# data first: `MazeGraph`, deterministic PRNG, generator, validator. Only after graph validation passes does the runtime layer instantiate room prefabs and bind map/gate/objective presenters. Maze progression talks to equipment and puzzle systems only through `IEquipmentService` and completion events/IDs.

**Tech Stack:** Unity 2022.3.62f2 LTS, C#, ScriptableObject authoring, Unity Test Framework, uGUI/selected project UI stack.

**Spec:** `Docs/Design/01_MAZE_SYSTEM_DESIGN.md`

## Global Constraints

- Hybrid generation: guaranteed critical path + random optional branches.
- Same seed must create the same graph on Web and Android.
- Chapter 1 required backtrack count <= 1.
- No gate may require an item/action available only behind that same gate.
- Required progression must never soft-lock.
- Floor movement happens through world transitions, not direct floor-number buttons.
- Hint use never reduces chapter completion rewards.
- Gameplay state persists through `ISaveStore`/SaveService only.

## Review Focus

1. Seed collision or platform-specific random order must not alter graph topology.
2. Gate dependency cycle must be rejected before runtime scene assembly.
3. Missing optional prefab must not make the critical path unusable.
4. Player reload inside a solved-gate room must restore open gate and visited map state.
5. Repeated hint triggers must not spam UI or advance more than one level per threshold.

---

### Task 1: Add maze graph domain model

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Data/MazeTypes.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeGraph.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeGraphTests.cs`

**Interfaces:**
- Produces: `MazeNode`, `MazeEdge`, `MazeGraph.AddNode`, `MazeGraph.AddEdge`, `MazeGraph.FindPath`.
- Consumes: foundation runtime assembly.

- [ ] **Step 1: Write failing path test**

~~~csharp
[Test]
public void FindPath_ReturnsStartToBoss_WhenConnected()
{
    var graph = new MazeGraph();
    graph.AddNode(new MazeNode("start", 0, RoomType.Start));
    graph.AddNode(new MazeNode("hall", 0, RoomType.Corridor));
    graph.AddNode(new MazeNode("boss", 0, RoomType.Boss));
    graph.AddEdge(new MazeEdge("e1", "start", "hall", EdgeType.Open, true));
    graph.AddEdge(new MazeEdge("e2", "hall", "boss", EdgeType.Open, true));

    CollectionAssert.AreEqual(
        new[] { "start", "hall", "boss" },
        graph.FindPath("start", "boss"));
}
~~~

- [ ] **Step 2: Run EditMode test**

Expected: fail because domain types do not exist.

- [ ] **Step 3: Implement domain types**

Required constructors:
~~~csharp
public MazeNode(string nodeId, int floor, RoomType type)
public MazeEdge(string edgeId, string fromNodeId, string toNodeId, EdgeType type, bool isBidirectional)
~~~

`FindPath` uses BFS and returns an empty list when unreachable.

- [ ] **Step 4: Add disconnected-path test and run suite**

Expected: connected test passes; disconnected returns empty.

- [ ] **Step 5: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze Assets/MazeMath/Tests/EditMode/Maze
git commit -m "feat: add maze graph domain model"
~~~

---

### Task 2: Add deterministic PRNG and stable seed service

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Generation/DeterministicRandom.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeSeedService.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeSeedTests.cs`

**Interfaces:**
- Produces: `int MazeSeedService.Create(string profileId, string chapterId, int attemptIndex, int baseOffset)`.
- Produces: deterministic `NextInt(min,max)`.
- Consumes: no Unity random APIs.

- [ ] **Step 1: Write failing repeatability test**

~~~csharp
[Test]
public void SameSeed_ProducesSameSequence()
{
    var a = new DeterministicRandom(12345);
    var b = new DeterministicRandom(12345);

    for (var i = 0; i < 100; i++)
        Assert.AreEqual(a.NextInt(0, 1000), b.NextInt(0, 1000));
}
~~~

- [ ] **Step 2: Run test and confirm failure**

- [ ] **Step 3: Implement fixed integer PRNG**

Use a small integer-only xorshift/PCG-style implementation with no `System.Random` or `UnityEngine.Random` dependency.

- [ ] **Step 4: Add stable hash test**

Assert the same strings produce the same 32-bit seed across repeated calls.

- [ ] **Step 5: Run EditMode tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze/Generation Assets/MazeMath/Tests/EditMode/Maze
git commit -m "feat: add deterministic maze random service"
~~~

---

### Task 3: Add authoring ScriptableObjects

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Data/MazeChapterDefinition.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Data/MazeDifficultyProfile.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Data/RoomTemplateDefinition.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Data/GateDefinition.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeDefinitionTests.cs`

**Interfaces:**
- Produces the fields defined in 01 spec.
- Consumes: `RoomType`, `EdgeType`.

- [ ] **Step 1: Write failing chapter validation test**

Instantiate a `MazeChapterDefinition` in memory with `floorCount = 0` and assert `ValidateAuthoring()` returns an error.

- [ ] **Step 2: Implement data assets and `ValidateAuthoring`**

Validation must reject:
- floorCount < 1
- min rooms > max rooms
- negative branch depth
- null difficulty profile
- missing Start/Boss-compatible room tags

- [ ] **Step 3: Run tests**

Expected: valid fixture passes, invalid fixture reports exact errors.

- [ ] **Step 4: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze/Data Assets/MazeMath/Tests/EditMode/Maze
git commit -m "feat: add maze authoring definitions"
~~~

---

### Task 4: Implement critical-path generator

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeGenerator.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeGeneratorTests.cs`

**Interfaces:**
- Produces: `MazeGraph Generate(MazeChapterDefinition chapter, int seed)`.
- Consumes: deterministic PRNG + authoring data.

- [ ] **Step 1: Write failing critical-path test**

Generate a fixed test chapter and assert:
- one Start
- one Boss
- path Start->Boss exists
- critical node count in configured range

- [ ] **Step 2: Implement minimal linear critical path**

Start → corridor/question/puzzle slots → Boss.

- [ ] **Step 3: Run test**

Expected: pass.

- [ ] **Step 4: Add optional branch test**

For a profile requiring 3 optional rooms, assert exactly 3 non-critical nodes are reachable from the main graph.

- [ ] **Step 5: Implement weighted optional branches**

Respect `MaxBranchDepth` and do not attach optional rooms after Boss.

- [ ] **Step 6: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze/Generation/MazeGenerator.cs Assets/MazeMath/Tests/EditMode/Maze/MazeGeneratorTests.cs
git commit -m "feat: generate hybrid maze graph"
~~~

---

### Task 5: Implement maze validator and 1,000-seed fuzz test

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeValidator.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeValidationResult.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeValidatorTests.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeFuzzTests.cs`

**Interfaces:**
- Produces: `MazeValidationResult Validate(MazeGraph graph, MazeChapterDefinition chapter)`.
- Consumes: graph, gate metadata, chapter constraints.

- [ ] **Step 1: Write failing invalid-gate dependency test**

Create Key Gate A whose key node exists only beyond A. Assert error `RequirementBehindOwnGate`.

- [ ] **Step 2: Implement validation errors**

Support:
- NoPathToBoss
- MissingRequiredRoom
- GateDependencyCycle
- RequirementBehindOwnGate
- ExcessiveBacktracking
- InvalidFloorTransition
- UnreachableOptionalRoom
- ConsecutiveSamePuzzleType
- MissingCheckpointBeforeBoss

- [ ] **Step 3: Run validator tests**

- [ ] **Step 4: Write fuzz test**

~~~csharp
[Test]
public void ThousandSeeds_HaveNoCriticalValidationErrors()
{
    for (var seed = 0; seed < 1000; seed++)
    {
        var graph = generator.Generate(chapter, seed);
        var result = validator.Validate(graph, chapter);
        Assert.IsFalse(result.HasCriticalErrors, $"seed={seed}: {result}");
    }
}
~~~

- [ ] **Step 5: Adjust generator only through failing test cases until fuzz passes**

Do not weaken validator rules to make the test green.

- [ ] **Step 6: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze/Generation Assets/MazeMath/Tests/EditMode/Maze
git commit -m "feat: validate generated maze progression"
~~~

---

### Task 6: Add room runtime assembly

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Runtime/RoomRuntime.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Runtime/MazeRunController.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Runtime/RoomPool.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Maze/MazeRoomRuntimeTests.cs`

**Interfaces:**
- Produces: current room + adjacent room activation.
- Consumes: validated `MazeGraph`.

- [ ] **Step 1: Write failing activation test**

Entering node B activates B and direct neighbors; non-neighbor node remains inactive.

- [ ] **Step 2: Implement room registry/pool and controller**

Do not Instantiate every graph room on every entry. Reuse generated room instances during a run.

- [ ] **Step 3: Run PlayMode test**

- [ ] **Step 4: Add missing optional prefab fallback test**

A missing optional room prefab must log one controlled error and skip that optional room; a missing critical room prefab must abort run start and use Safe Layout.

- [ ] **Step 5: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze/Runtime Assets/MazeMath/Tests/PlayMode/Maze
git commit -m "feat: assemble maze rooms at runtime"
~~~

---

### Task 7: Implement gates and floor transitions

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Runtime/GateRuntime.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Runtime/FloorTransition.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Runtime/IMazeService.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Maze/GateAndFloorTests.cs`

**Interfaces:**
- Produces the `IMazeService` contract from spec.
- Consumes: equipment capability lookup via interface; puzzle completion IDs later.

- [ ] **Step 1: Write failing locked-gate test**

Assert `CanTraverse(edgeId)` false before requirement and true after `SolveGate(gateId)`.

- [ ] **Step 2: Implement gate state**

Gate visuals subscribe to state change; domain service owns solved IDs.

- [ ] **Step 3: Write floor transition test**

Traverse from floor 0 to 1 and assert current floor, current node, spawn, and floor-change event are updated once.

- [ ] **Step 4: Implement floor transition**

- [ ] **Step 5: Run PlayMode tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze/Runtime Assets/MazeMath/Tests/PlayMode/Maze
git commit -m "feat: add maze gates and floor transitions"
~~~

---

### Task 8: Implement fog-of-war map and objective tracking

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Map/MazeMapService.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Map/FogOfWarService.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Map/ObjectiveMarkerService.cs`
- Create: `Assets/MazeMath/Scripts/UI/Maze/MiniMapView.cs`
- Create: `Assets/MazeMath/Scripts/UI/Maze/ObjectiveView.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeMapTests.cs`

**Interfaces:**
- Produces visited/seen/cleared map state per floor.
- Consumes: room-entry events.

- [ ] **Step 1: Write failing reveal-state tests**

Unknown → Seen → Visited → Cleared must only advance, never regress.

- [ ] **Step 2: Implement map services**

- [ ] **Step 3: Add floor-filter test**

Floor 0 query must not return Floor 1 visited nodes.

- [ ] **Step 4: Implement presenters**

Presenter reads state; it does not own state.

- [ ] **Step 5: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze/Map Assets/MazeMath/Scripts/UI/Maze Assets/MazeMath/Tests/EditMode/Maze
git commit -m "feat: add maze fog of war and objectives"
~~~

---

### Task 9: Implement progressive maze hints

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Hint/MazeHintService.cs`
- Create: `Assets/MazeMath/Scripts/UI/Maze/MazeHintView.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeHintTests.cs`

**Interfaces:**
- Produces hint levels 1-4.
- Consumes elapsed no-progress time, repeat-room counts, objective target.

- [ ] **Step 1: Write failing threshold tests**

Assert:
- 89 seconds no-progress => no hint
- 90 seconds => Level 1 available
- repeated entry threshold can also trigger Level 1
- repeated update calls do not raise multiple levels in the same threshold event

- [ ] **Step 2: Implement hint state machine**

- [ ] **Step 3: Add next-direction path test for Level 3**

Only next room direction is exposed, not entire route.

- [ ] **Step 4: Run tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze/Hint Assets/MazeMath/Scripts/UI/Maze/MazeHintView.cs Assets/MazeMath/Tests/EditMode/Maze/MazeHintTests.cs
git commit -m "feat: add progressive maze hints"
~~~

---

### Task 10: Persist and restore MazeRunState

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Runtime/MazeRunState.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Runtime/MazeRunSaveAdapter.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Maze/MazeRecoveryTests.cs`

**Interfaces:**
- Produces state fields exactly matching spec.
- Consumes: `SaveService`.

- [ ] **Step 1: Write failing restore test**

Start run → visit node → solve gate → move floor → save → destroy runtime → restore.

Assert same:
- runSeed
- currentNodeId
- currentFloor
- visited IDs
- seen IDs
- solved gate IDs
- active objective

- [ ] **Step 2: Implement save adapter**

- [ ] **Step 3: Add corrupt/missing state fallback test**

Maze starts from chapter Start node without crashing.

- [ ] **Step 4: Run PlayMode tests and commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze/Runtime Assets/MazeMath/Tests/PlayMode/Maze/MazeRecoveryTests.cs
git commit -m "feat: persist maze run state"
~~~

---

### Task 11: Build Chapter 1 vertical slice

**Files:**
- Create: `Assets/MazeMath/ScriptableObjects/Maze/Chapter01.asset`
- Create: room prefabs under `Assets/MazeMath/Prefabs/Maze/Chapter01/`
- Modify: `Assets/MazeMath/Scenes/Gameplay.unity`
- Create: `Assets/MazeMath/Tests/PlayMode/Maze/Chapter01SmokeTests.cs`

**Interfaces:**
- Consumes all maze runtime pieces.
- Produces first playable 1-floor chapter.

- [ ] **Step 1: Create smoke test**

Programmatically start Chapter 1 and assert:
- 6-8 critical rooms
- 2-3 optional rooms
- <= 2 junctions
- one required backtrack
- no hidden room
- Start-to-Boss path valid

- [ ] **Step 2: Author room prefabs and chapter asset**

Include:
- Start
- Junction
- Question placeholder
- Puzzle placeholder
- Reward
- Checkpoint
- Boss placeholder

- [ ] **Step 3: Run EditMode + PlayMode suites**

Expected: 0 failures.

- [ ] **Step 4: Run Web smoke build**

Expected: build exits 0.

- [ ] **Step 5: Run Android smoke build**

Expected: build exits 0 where Android module exists.

- [ ] **Step 6: Commit**

~~~bash
git add Assets/MazeMath
git commit -m "feat: add Chapter 1 maze vertical slice"
~~~

## Plan Completion Gate

Maze V1 is complete only when the Chapter 1 vertical slice is playable from Start to Boss, one meaningful backtrack exists, fog-of-war and hints work, saved state restores, 1,000-seed fuzz validation has zero critical failures, and both Web/Android smoke builds succeed.
