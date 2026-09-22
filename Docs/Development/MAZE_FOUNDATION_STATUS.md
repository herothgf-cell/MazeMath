# Maze Foundation Development Status

- Project: MazeMath
- Branch: `feat/maze-foundation`
- Scope: Maze System phases M1-M3
- Engine target: Unity 6.3 LTS
- Deployment targets: Web and Android
- Design authority: `Docs/Design/01_MAZE_SYSTEM_DESIGN.md`
- Implementation plan: `Docs/Plans/2026-09-22_MAZE_FOUNDATION_IMPLEMENTATION_PLAN.md`

## Implemented

### Deterministic core

- `StableHash.Fnv1A32(string)`
- `DeterministicRandom`
- `MazeSeedService`

The generator does not use `UnityEngine.Random`, frame time, scene state, or platform-specific random APIs.

### Pure maze graph

- `MazeNode`
- `MazeEdge`
- `MazeRequirementPlacement`
- `MazeGraph`
- BFS path queries
- bidirectional and one-way edge support
- gate requirement metadata
- required-backtrack metadata

### Deterministic generation

- `MazeGenerationSettings`
- `MazeGenerationRequest`
- `MazeGenerationResult`
- `IMazeGenerator`
- `MazeGenerator`

Current generator behavior:

- deterministic critical path
- Start and Boss rooms
- Checkpoint immediately before Boss
- one to three floors
- Ladder connection when the critical path changes floor
- deterministic optional branches
- Chapter 1 default: 6-8 critical rooms and 3 optional rooms

### Progression safety validation

- `IMazeValidator`
- `MazeValidator`
- `MazeValidationResult`
- `MazeValidationIssue`
- `MazeValidationErrorCode`

Validated conditions include:

- Start/Boss presence
- Start-to-Boss reachability
- valid floor transitions
- Checkpoint before Boss
- maximum required backtracking
- requirement-behind-own-gate soft lock
- gate requirement dependency cycles

### Retry and fallback

- `MazeGenerationCoordinator`
- `SafeMazeFactory`

Generation is attempted at most 10 times. If every generated graph is invalid, a known-safe Chapter 1 graph is returned instead of allowing a broken run.

## Automated verification

Pure C# logic is compiled and tested independently of Unity through:

- `Tools/CoreTests/MazeMath.CoreTests.csproj`
- `.github/workflows/core-tests.yml`

The suite covers:

- stable hash and deterministic PRNG
- graph queries and invalid graph construction
- deterministic critical-path generation
- floor transitions
- optional branches
- progression safety validation
- self-lock and dependency-cycle detection
- bounded retry behavior
- safe-layout fallback
- 1,000-seed generation/validation fuzz test

## Unity-side bootstrap

Present:

- `ProjectSettings/ProjectVersion.txt`
- `Assets/MazeMath/Scripts/MazeMath.Runtime.asmdef`
- `Assets/MazeMath/Tests/EditMode/MazeMath.EditModeTests.asmdef`
- `Assets/MazeMath/Tests/EditMode/ProjectSmokeTests.cs`
- Unity-focused `.gitignore`

The exact `Packages/manifest.json`, `Packages/packages-lock.json`, and Unity-generated `.meta` files are intentionally not hand-authored in this milestone because this execution environment cannot launch the target Unity Editor. Open the repository once with the exact Unity version in `ProjectVersion.txt`, allow Unity to generate/resolve those files, then commit them.

## Not implemented yet

The following items belong to Phase M4 or later and must not be treated as complete:

- Room prefab assembly
- Gameplay Scene runtime
- player movement and interaction
- FloorTransition MonoBehaviours
- runtime Gate MonoBehaviours
- Fog of War minimap UI
- objective markers
- hint escalation UI/runtime
- save and checkpoint recovery
- crafting and equipment
- question system
- environmental puzzles
- boss integration
- Web build profile and deploy pipeline
- Android build profile, signing, and deploy pipeline

## Stable contracts for Phase M4

Phase M4 may rely on these names:

- `StableHash.Fnv1A32`
- `DeterministicRandom`
- `MazeSeedService`
- `MazeNode`
- `MazeEdge`
- `MazeRequirementPlacement`
- `MazeGraph`
- `MazeGenerationSettings`
- `MazeGenerationRequest`
- `MazeGenerationResult`
- `IMazeGenerator`
- `IMazeValidator`
- `MazeGenerationCoordinator`
- `SafeMazeFactory`

Runtime presentation code should consume these contracts instead of duplicating generation logic inside MonoBehaviours.

## Next milestone

Phase M4 should assemble room prefabs from a validated `MazeGraph` and prove a minimal playable flow:

1. Bootstrap scene
2. Gameplay scene
3. Room prefab contract and connector anchors
4. Graph-to-room assembly
5. player spawn and room traversal
6. Ladder/FloorTransition runtime
7. Gate runtime shell
8. camera bounds
9. PlayMode tests
10. Web/Android smoke-build setup

No crafting, questions, or polished UI should be added before this runtime skeleton is proven.
