# MazeMath Maze Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bootstrap MazeMath as a Unity 6.3 LTS project and implement the deterministic, testable maze-data foundation through Phase M3: stable seed generation, pure MazeGraph data, hybrid critical-path generation, and graph validation.

**Architecture:** Keep the first milestone independent of scenes and prefabs. All generation and validation logic lives in pure C# inside `MazeMath.Maze`; Unity-facing authoring data can be added later without changing graph contracts. NUnit EditMode tests drive each behavior, and later runtime systems consume the graph through small interfaces rather than reading scene objects.

**Tech Stack:** Unity 6.3 LTS (6000.3 line), C#/.NET Standard profile supported by Unity, Unity Test Framework/NUnit, GitHub repository.

**Spec:** `Docs/Design/01_MAZE_SYSTEM_DESIGN.md`

## Global Constraints

- Target engine line: Unity 6.3 LTS.
- Shipping targets: Web and Android.
- Maze generation must be deterministic for identical input seeds across both targets.
- Core maze generation/validation must not depend on MonoBehaviour, GameObject, Scene, Time, frame count, or platform-specific random APIs.
- Start → Boss must always be reachable in accepted graphs.
- Gate requirements must never be located only behind the gate they unlock.
- Chapter 1 must permit at most one required backtrack.
- Generation retries are bounded; after ten invalid attempts the system must return a known-safe layout.
- The first milestone creates no production UI, room art, combat, crafting, or puzzle presentation.
- Tests are written before implementation for every behavior-changing task.

## Review Focus

1. Seed determinism across process/platform boundaries — pin with golden-value hash/PRNG tests in Task 2.
2. Malformed graph inputs such as duplicate node IDs, dangling edges, or missing Start/Boss — pin with validation tests in Task 5.
3. Gate dependency cycles and requirement-behind-own-gate soft locks — pin with explicit cycle/ordering tests in Task 5.
4. Generator retry exhaustion — pin with a forced-invalid generator test that proves Safe Layout fallback in Task 6.
5. Excessive or accidental backtracking on the critical path — pin with path-metadata assertions in Tasks 4 and 5.

---

### Task 1: Bootstrap the Unity project and test assemblies

**Files:**
- Create: `ProjectSettings/ProjectVersion.txt`
- Create: `Packages/manifest.json`
- Create: `Packages/packages-lock.json` only after Unity resolves packages; do not hand-author it.
- Create: `Assets/MazeMath/Scripts/MazeMath.Runtime.asmdef`
- Create: `Assets/MazeMath/Tests/EditMode/MazeMath.EditModeTests.asmdef`
- Create: `Assets/MazeMath/Tests/EditMode/ProjectSmokeTests.cs`
- Create: `.gitignore`

**Interfaces:**
- Consumes: none.
- Produces: Unity project recognized by the chosen 6000.3 LTS editor, runtime assembly `MazeMath.Runtime`, EditMode test assembly `MazeMath.EditModeTests`.

- [ ] **Step 1: Write the smoke test before runtime code**

Create `Assets/MazeMath/Tests/EditMode/ProjectSmokeTests.cs`:

~~~csharp
using NUnit.Framework;

namespace MazeMath.Tests.EditMode
{
    public sealed class ProjectSmokeTests
    {
        [Test]
        public void TestAssembly_Loads()
        {
            Assert.That(typeof(ProjectSmokeTests).Assembly, Is.Not.Null);
        }
    }
}
~~~

- [ ] **Step 2: Create the assembly definitions**

`Assets/MazeMath/Scripts/MazeMath.Runtime.asmdef`:

~~~json
{
  "name": "MazeMath.Runtime",
  "rootNamespace": "MazeMath",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
~~~

`Assets/MazeMath/Tests/EditMode/MazeMath.EditModeTests.asmdef`:

~~~json
{
  "name": "MazeMath.EditModeTests",
  "rootNamespace": "MazeMath.Tests.EditMode",
  "references": [
    "MazeMath.Runtime"
  ],
  "includePlatforms": [
    "Editor"
  ],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": false,
  "defineConstraints": [
    "UNITY_INCLUDE_TESTS"
  ],
  "versionDefines": [],
  "noEngineReferences": false,
  "optionalUnityReferences": [
    "TestAssemblies"
  ]
}
~~~

- [ ] **Step 3: Pin the editor line and package manifest**

Use an installed Unity 6.3 LTS patch. If `6000.3.13f1` is installed, create:

`ProjectSettings/ProjectVersion.txt`:

~~~text
m_EditorVersion: 6000.3.13f1
m_EditorVersionWithRevision: 6000.3.13f1
~~~

If a newer 6000.3 LTS patch is installed locally, create the project with that editor and commit the exact value Unity writes instead of fabricating a revision hash.

`Packages/manifest.json` must include, at minimum, Unity Test Framework and built-in modules required by a blank 2D project. Prefer creating a blank 2D Core project in Unity Hub and retaining Unity-generated package versions rather than guessing package versions.

- [ ] **Step 4: Add Unity .gitignore**

Include at least:

~~~gitignore
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
UserSettings/
MemoryCaptures/
Recordings/
.vs/
.idea/
.DS_Store
~~~

Do not ignore `Packages/packages-lock.json`, `ProjectSettings/`, `Assets/*.meta`, or `Packages/manifest.json`.

- [ ] **Step 5: Run the EditMode smoke test**

Run from the Unity editor Test Runner, or batch mode when the local editor path is known:

~~~text
Unity -batchmode -nographics -projectPath <repo> -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -quit
~~~

Expected: `ProjectSmokeTests.TestAssembly_Loads` PASS and process exit code 0.

- [ ] **Step 6: Commit**

~~~bash
git add .gitignore Assets Packages ProjectSettings
git commit -m "chore: bootstrap MazeMath Unity project"
~~~

---

### Task 2: Implement stable hashing and deterministic PRNG

**Files:**
- Create: `Assets/MazeMath/Scripts/Core/Determinism/StableHash.cs`
- Create: `Assets/MazeMath/Scripts/Core/Determinism/DeterministicRandom.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeSeedService.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Determinism/StableHashTests.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Determinism/DeterministicRandomTests.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeSeedServiceTests.cs`

**Interfaces:**
- Consumes: `MazeMath.Runtime` assembly.
- Produces:
  - `uint StableHash.Fnv1A32(string value)`
  - `DeterministicRandom(uint seed)`
  - `uint DeterministicRandom.NextUInt()`
  - `int DeterministicRandom.NextInt(int minInclusive, int maxExclusive)`
  - `bool DeterministicRandom.NextBool()`
  - `uint MazeSeedService.CreateSeed(string profileId, string chapterId, int attemptIndex, int baseSeedOffset)`

- [ ] **Step 1: Write golden-value hash tests**

~~~csharp
using NUnit.Framework;
using MazeMath.Core.Determinism;

namespace MazeMath.Tests.EditMode.Determinism
{
    public sealed class StableHashTests
    {
        [TestCase("", 2166136261u)]
        [TestCase("MazeMath", 2441713404u)]
        public void Fnv1A32_ReturnsGoldenValue(string value, uint expected)
        {
            Assert.That(StableHash.Fnv1A32(value), Is.EqualTo(expected));
        }
    }
}
~~~

Before implementation, calculate/verify the literal golden values with an independent FNV-1a implementation. If `2441713404u` is not the correct FNV-1a value for UTF-8 `MazeMath`, replace the expected value in the test with the independently verified one before continuing.

- [ ] **Step 2: Run the hash test and verify RED**

Expected: compile failure because `StableHash` does not exist.

- [ ] **Step 3: Implement FNV-1a over explicit UTF-8 bytes**

~~~csharp
using System.Text;

namespace MazeMath.Core.Determinism
{
    public static class StableHash
    {
        public static uint Fnv1A32(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);

                for (int i = 0; i < bytes.Length; i++)
                {
                    hash ^= bytes[i];
                    hash *= 16777619u;
                }

                return hash;
            }
        }
    }
}
~~~

- [ ] **Step 4: Write deterministic sequence tests before PRNG implementation**

Use a simple xorshift32 PRNG with explicit golden outputs.

~~~csharp
[Test]
public void NextUInt_SameSeed_ReturnsSameSequence()
{
    var a = new DeterministicRandom(123456789u);
    var b = new DeterministicRandom(123456789u);

    for (int i = 0; i < 100; i++)
        Assert.That(a.NextUInt(), Is.EqualTo(b.NextUInt()));
}

[Test]
public void NextInt_StaysInsideRequestedRange()
{
    var rng = new DeterministicRandom(42u);

    for (int i = 0; i < 1000; i++)
    {
        int value = rng.NextInt(3, 8);
        Assert.That(value, Is.GreaterThanOrEqualTo(3).And.LessThan(8));
    }
}
~~~

- [ ] **Step 5: Implement xorshift32 without UnityEngine.Random or System.Random**

~~~csharp
namespace MazeMath.Core.Determinism
{
    public sealed class DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(uint seed)
        {
            _state = seed == 0u ? 0x6D2B79F5u : seed;
        }

        public uint NextUInt()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
                throw new System.ArgumentOutOfRangeException(nameof(maxExclusive));

            uint range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % range);
        }

        public bool NextBool() => (NextUInt() & 1u) == 1u;
    }
}
~~~

- [ ] **Step 6: Write MazeSeedService test**

~~~csharp
[Test]
public void CreateSeed_IsStableAndAttemptSensitive()
{
    var service = new MazeSeedService();

    uint first = service.CreateSeed("default", "chapter-1", 0, 100);
    uint same = service.CreateSeed("default", "chapter-1", 0, 100);
    uint retry = service.CreateSeed("default", "chapter-1", 1, 100);

    Assert.That(same, Is.EqualTo(first));
    Assert.That(retry, Is.Not.EqualTo(first));
}
~~~

- [ ] **Step 7: Implement MazeSeedService**

Build one canonical string with invariant separators and hash it:

~~~csharp
using MazeMath.Core.Determinism;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeSeedService
    {
        public uint CreateSeed(
            string profileId,
            string chapterId,
            int attemptIndex,
            int baseSeedOffset)
        {
            string key = $"{profileId ?? string.Empty}|{chapterId ?? string.Empty}|{attemptIndex}|{baseSeedOffset}";
            return StableHash.Fnv1A32(key);
        }
    }
}
~~~

- [ ] **Step 8: Run all determinism tests**

Expected: all PASS.

- [ ] **Step 9: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Core Assets/MazeMath/Scripts/Maze/Generation Assets/MazeMath/Tests/EditMode
git commit -m "feat: add deterministic seed utilities"
~~~

---

### Task 3: Implement pure MazeGraph data and graph queries

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Data/RoomType.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Data/EdgeType.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeNode.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeEdge.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeGraph.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeGraphTests.cs`

**Interfaces:**
- Consumes: no Unity scene objects.
- Produces:
  - immutable-ish `MazeNode` and `MazeEdge` records/classes
  - `MazeGraph.AddNode`, `AddEdge`, `GetNode`, `GetOutgoingEdges`, `FindPath`
  - `StartNodeId` and `BossNodeId`

- [ ] **Step 1: Write graph construction/query tests**

~~~csharp
[Test]
public void FindPath_ReturnsStartToBossPath()
{
    var graph = new MazeGraph("start", "boss");
    graph.AddNode(new MazeNode("start", "start-room", 1, RoomType.Start, true));
    graph.AddNode(new MazeNode("middle", "corridor", 1, RoomType.Corridor, true));
    graph.AddNode(new MazeNode("boss", "boss-room", 1, RoomType.Boss, true));

    graph.AddEdge(new MazeEdge("e1", "start", "middle", EdgeType.Open, null, true));
    graph.AddEdge(new MazeEdge("e2", "middle", "boss", EdgeType.Open, null, true));

    Assert.That(graph.FindPath("start", "boss"),
        Is.EqualTo(new[] { "start", "middle", "boss" }));
}
~~~

Also add tests proving:
- duplicate node ID throws `InvalidOperationException`
- edge referencing a missing node throws `InvalidOperationException`
- unreachable target returns an empty path
- bidirectional edges can be traversed in both directions

- [ ] **Step 2: Run tests and verify RED**

Expected: compile failure because graph types do not exist.

- [ ] **Step 3: Implement enums and data objects**

`RoomType` values:

~~~csharp
Start,
Corridor,
Junction,
Puzzle,
Question,
Reward,
Workshop,
Checkpoint,
Transition,
Boss,
Secret
~~~

`EdgeType` values:

~~~csharp
Open,
Door,
Ladder,
Elevator,
Locked,
PuzzleLocked,
EquipmentLocked,
Hidden
~~~

`MazeNode` constructor arguments:

~~~text
string nodeId
string templateId
int floor
RoomType type
bool isCriticalPath
IEnumerable<string> tags = null
~~~

`MazeEdge` constructor arguments:

~~~text
string edgeId
string fromNodeId
string toNodeId
EdgeType type
string gateId
bool isBidirectional
~~~

- [ ] **Step 4: Implement MazeGraph with BFS pathfinding**

Use dictionaries keyed by node/edge ID and adjacency lists. `FindPath` returns node IDs including source and target. It must not mutate graph state.

- [ ] **Step 5: Run MazeGraphTests**

Expected: all PASS.

- [ ] **Step 6: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze/Data Assets/MazeMath/Scripts/Maze/Generation Assets/MazeMath/Tests/EditMode/Maze
git commit -m "feat: add maze graph data model"
~~~

---

### Task 4: Implement chapter generation input and critical-path generator

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Data/MazeGenerationSettings.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeGenerationRequest.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeGenerationResult.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeGenerator.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeGeneratorTests.cs`

**Interfaces:**
- Consumes:
  - `DeterministicRandom`
  - `MazeGraph`
- Produces:
  - `MazeGenerationSettings`
  - `MazeGenerationRequest`
  - `MazeGenerationResult MazeGenerator.Generate(MazeGenerationRequest request)`

- [ ] **Step 1: Define tests for critical-path bounds**

~~~csharp
[Test]
public void Generate_CreatesPathWithinRequestedBounds()
{
    var settings = new MazeGenerationSettings(
        minCriticalPathRooms: 6,
        maxCriticalPathRooms: 8,
        optionalRoomCount: 0,
        floorCount: 1,
        maxRequiredBacktracks: 1);

    var result = new MazeGenerator().Generate(
        new MazeGenerationRequest("chapter-1", 1234u, settings));

    var path = result.Graph.FindPath(
        result.Graph.StartNodeId,
        result.Graph.BossNodeId);

    Assert.That(path.Count, Is.InRange(6, 8));
}
~~~

Add tests:
- identical request seed yields identical node/edge IDs and types
- different seeds can yield at least two different valid layouts over a seed sample
- Start is first critical node and Boss is last
- requested `floorCount = 1` never assigns a node to another floor
- `floorCount = 2` produces at least one Transition edge when critical path is long enough

- [ ] **Step 2: Run tests and verify RED**

- [ ] **Step 3: Implement generation settings**

Constructor must validate:

- `minCriticalPathRooms >= 2`
- `maxCriticalPathRooms >= minCriticalPathRooms`
- `optionalRoomCount >= 0`
- `floorCount >= 1 && floorCount <= 3`
- `maxRequiredBacktracks >= 0`

Invalid values throw `ArgumentOutOfRangeException`.

- [ ] **Step 4: Implement V1 critical-path generation only**

Generation algorithm for this task:

1. Create deterministic RNG from request seed.
2. Pick critical path room count in inclusive configured range.
3. Create `start` and `boss` nodes.
4. Fill intermediate nodes from a fixed safe pattern:
   - Corridor
   - Junction
   - Question
   - Corridor
   - Puzzle
   - Workshop
   - Checkpoint
   repeat/truncate as needed, never duplicate Boss/Start.
5. Assign floors monotonically; when crossing a floor boundary, connect with `EdgeType.Ladder` and use `RoomType.Transition` for the destination node if appropriate.
6. Use stable IDs: `c00-start`, `c01-corridor`, ..., `cNN-boss`.
7. Mark every critical-path node `IsCriticalPath = true`.

Do not add gates or optional branches in this step.

- [ ] **Step 5: Run generator tests**

Expected: all PASS.

- [ ] **Step 6: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze Assets/MazeMath/Tests/EditMode/Maze
git commit -m "feat: generate deterministic critical maze path"
~~~

---

### Task 5: Implement MazeValidator and explicit failure codes

**Files:**
- Create: `Assets/MazeMath/Scripts/Maze/Validation/MazeValidationErrorCode.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Validation/MazeValidationIssue.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Validation/MazeValidationResult.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Validation/MazeValidator.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeValidatorTests.cs`

**Interfaces:**
- Consumes: `MazeGraph`, `MazeGenerationSettings`.
- Produces:
  - `MazeValidationResult MazeValidator.Validate(MazeGraph graph, MazeGenerationSettings settings)`
  - error codes:
    - `NoPathToBoss`
    - `MissingStart`
    - `MissingBoss`
    - `DuplicateNodeId`
    - `DanglingEdge`
    - `GateDependencyCycle`
    - `RequirementBehindOwnGate`
    - `ExcessiveBacktracking`
    - `InvalidFloorTransition`
    - `MissingCheckpointBeforeBoss`

- [ ] **Step 1: Write malformed-graph tests**

Because `MazeGraph` blocks some malformed states at construction time, expose validator-friendly read-only graph data and create test helper graphs for semantic failures.

Required tests:

~~~csharp
[Test]
public void Validate_ReportsNoPathToBoss()
{
    MazeGraph graph = TestGraphs.DisconnectedBoss();
    MazeValidationResult result = new MazeValidator().Validate(
        graph,
        MazeGenerationSettings.Chapter1Defaults());

    Assert.That(result.ErrorCodes, Does.Contain(MazeValidationErrorCode.NoPathToBoss));
}
~~~

Also test:
- no checkpoint immediately before final Boss segment when settings require it
- invalid floor transition jumping from floor 1 to floor 3
- Chapter 1 metadata reporting >1 required backtrack
- gate dependency cycle A → B → A
- a requirement node reachable only after its own gate

- [ ] **Step 2: Run tests and verify RED**

- [ ] **Step 3: Implement structural validation**

Check:
- Start/Boss IDs exist
- path Start → Boss exists
- every edge endpoint exists
- floor deltas on Ladder/Elevator are valid
- ordinary Open/Door edges do not accidentally jump floors

- [ ] **Step 4: Add dependency metadata required for gate validation**

Extend `MazeEdge` with optional:
- `string RequirementId`

Introduce `MazeRequirementPlacement`:
- `string RequirementId`
- `string NodeId`

Add `IReadOnlyList<MazeRequirementPlacement> Requirements` to `MazeGraph`.

Write tests first for these additions.

- [ ] **Step 5: Implement dependency-cycle and self-lock validation**

Build a dependency graph:
- Gate requirement R must be reachable before traversing the edge locked by R.
- If R itself requires another gated route, recursively inspect dependencies.
- Detect DFS recursion-stack cycles and emit `GateDependencyCycle`.
- Emit `RequirementBehindOwnGate` when every Start → requirement route crosses the gate that needs that same requirement.

- [ ] **Step 6: Run validator tests**

Expected: all PASS.

- [ ] **Step 7: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Maze Assets/MazeMath/Tests/EditMode/Maze
git commit -m "feat: validate maze graph progression safety"
~~~

---

### Task 6: Add optional branches, bounded retries, and safe-layout fallback

**Files:**
- Modify: `Assets/MazeMath/Scripts/Maze/Generation/MazeGenerator.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/SafeMazeFactory.cs`
- Create: `Assets/MazeMath/Scripts/Maze/Generation/MazeGenerationCoordinator.cs`
- Modify: `Assets/MazeMath/Tests/EditMode/Maze/MazeGeneratorTests.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Maze/MazeGenerationCoordinatorTests.cs`

**Interfaces:**
- Consumes:
  - `MazeGenerator`
  - `MazeValidator`
  - `MazeSeedService`
- Produces:
  - optional branch generation
  - `MazeGenerationCoordinator.GenerateValidated(...)`
  - `SafeMazeFactory.CreateChapter1SafeLayout()`

- [ ] **Step 1: Write optional-branch tests**

Required behavior:

~~~csharp
[Test]
public void Generate_WithOptionalRooms_AddsReachableNonCriticalNodes()
{
    MazeGenerationSettings settings = MazeGenerationSettings.Chapter1Defaults()
        .WithOptionalRoomCount(3);

    MazeGenerationResult result = new MazeGenerator().Generate(
        new MazeGenerationRequest("chapter-1", 99u, settings));

    int optionalCount = result.Graph.Nodes.Count(n => !n.IsCriticalPath);

    Assert.That(optionalCount, Is.EqualTo(3));
    Assert.That(result.Graph.Nodes.Where(n => !n.IsCriticalPath)
        .All(n => result.Graph.FindPath(result.Graph.StartNodeId, n.NodeId).Count > 0),
        Is.True);
}
~~~

Also test:
- optional branch depth does not exceed configured max
- optional room cannot replace Start/Boss
- repeated identical seed generates identical optional branch attachment points

- [ ] **Step 2: Implement simple optional branches**

For V1:
- attach optional nodes only to eligible Corridor/Junction critical nodes
- branch depth 1 by default, maximum 2 when configured
- room types cycle through Reward, Secret, Question
- all optional branches are bidirectionally reachable
- no gate logic yet

- [ ] **Step 3: Write coordinator retry/fallback tests**

Inject generator/validator abstractions so tests can force failure.

~~~csharp
[Test]
public void GenerateValidated_AfterTenInvalidAttempts_ReturnsSafeLayout()
{
    var generator = new AlwaysInvalidGenerator();
    var validator = new AlwaysInvalidValidator();
    var coordinator = new MazeGenerationCoordinator(
        generator,
        validator,
        new MazeSeedService(),
        new SafeMazeFactory());

    MazeGenerationResult result = coordinator.GenerateValidated(
        profileId: "default",
        chapterId: "chapter-1",
        baseSeedOffset: 10,
        settings: MazeGenerationSettings.Chapter1Defaults());

    Assert.That(result.UsedSafeLayout, Is.True);
    Assert.That(result.AttemptCount, Is.EqualTo(10));
}
~~~

- [ ] **Step 4: Introduce testable interfaces**

~~~csharp
public interface IMazeGenerator
{
    MazeGenerationResult Generate(MazeGenerationRequest request);
}

public interface IMazeValidator
{
    MazeValidationResult Validate(
        MazeGraph graph,
        MazeGenerationSettings settings);
}
~~~

Make `MazeGenerator` and `MazeValidator` implement them.

- [ ] **Step 5: Implement coordinator**

Algorithm:
1. For `attemptIndex = 0..9`
2. create seed through `MazeSeedService`
3. generate graph
4. validate graph
5. return first valid result with `UsedSafeLayout = false`
6. after ten failures return `SafeMazeFactory` graph with `UsedSafeLayout = true`

Do not catch arbitrary exceptions from programmer errors; only normal invalid-graph results cause retry.

- [ ] **Step 6: Implement hand-authored safe layout**

Chapter 1 safe graph:

~~~text
Start
  ↓
Corridor
  ↓
Question
  ↓
Junction ─→ Reward(optional)
  ↓
Puzzle
  ↓
Checkpoint
  ↓
Boss
~~~

No locks, one floor, all IDs stable, validation must pass.

- [ ] **Step 7: Add 1,000-seed fuzz validation test**

~~~csharp
[Test]
public void GenerateValidated_OneThousandSeeds_AllReturnValidGraph()
{
    var coordinator = TestComposition.CreateCoordinator();
    var validator = new MazeValidator();
    MazeGenerationSettings settings = MazeGenerationSettings.Chapter1Defaults();

    for (int i = 0; i < 1000; i++)
    {
        MazeGenerationResult result = coordinator.GenerateValidated(
            "default",
            "chapter-1",
            i,
            settings);

        MazeValidationResult validation = validator.Validate(result.Graph, settings);

        Assert.That(validation.IsValid, Is.True, $"seed index {i}");
    }
}
~~~

- [ ] **Step 8: Run the complete EditMode suite**

Run:

~~~text
Unity -batchmode -nographics -projectPath <repo> -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -quit
~~~

Expected: process exit code 0, zero failed tests.

- [ ] **Step 9: Commit**

~~~bash
git add Assets/MazeMath
git commit -m "feat: complete deterministic maze foundation"
~~~

---

### Task 7: Document the foundation API and handoff to runtime-room assembly

**Files:**
- Create: `Docs/Development/MAZE_FOUNDATION_STATUS.md`
- Modify: `README.md` if it exists; otherwise create it.

**Interfaces:**
- Consumes: Tasks 1–6 public interfaces.
- Produces: a human-readable snapshot of what is implemented and exactly what Phase M4 may rely on.

- [ ] **Step 1: Write API status document**

Document these stable contracts:

~~~text
StableHash.Fnv1A32
DeterministicRandom
MazeSeedService
MazeNode
MazeEdge
MazeGraph
MazeGenerationSettings
MazeGenerationRequest
MazeGenerationResult
IMazeGenerator
IMazeValidator
MazeGenerationCoordinator
SafeMazeFactory
~~~

Also list explicitly not implemented yet:

~~~text
Room prefab assembly
Scene runtime
FloorTransition MonoBehaviours
Fog of War UI
Objectives/hints
Save/recovery
Crafting/equipment
Questions/environment puzzles
Boss integration
Web build profile
Android build profile
~~~

- [ ] **Step 2: Add local Unity setup instructions**

README must state:

1. Install Unity Hub.
2. Install the exact `ProjectSettings/ProjectVersion.txt` Unity 6.3 LTS patch.
3. Include Web Build Support and Android Build Support modules.
4. Clone repository.
5. Open repository folder as Unity project.
6. Run EditMode tests before gameplay development.

- [ ] **Step 3: Verify documentation against committed code**

Manually compare every public contract listed in `MAZE_FOUNDATION_STATUS.md` with source names/signatures. Remove any claim not present in code.

- [ ] **Step 4: Commit**

~~~bash
git add Docs README.md
git commit -m "docs: document maze foundation milestone"
~~~

## Plan Self-Review Result

- Spec coverage for this milestone: M1, M2, and M3 are covered; runtime M4+ requirements are intentionally deferred to the next implementation plan.
- Placeholder scan: no TODO/TBD placeholders are permitted in the saved plan.
- Type consistency: generation and validation interfaces are defined before coordinator use.
- Test coverage: determinism, malformed graphs, gate dependencies, backtracking, retry exhaustion, safe fallback, and 1,000-seed fuzz validation are explicitly covered.
- Scope boundary: crafting and puzzle systems remain separate plans and do not enter this milestone.
