# Native Development Progress

Branch: `feat/native-foundation`

## Fixed Environment

- Unity Editor: **2022.3.62f2**
- Input System: **1.6.1**
- Unity Test Framework: **1.1.33**
- Unity UI: `com.unity.ugui@1.0.0`
- Web distribution: Unity 2022.3 WebGL for supported desktop browsers
- Mobile distribution: Android native app
- Android product minimum: API 23
- Unity 2022.3 Android toolchain baseline: NDK r23b / OpenJDK 11

## Execution Notes

- Native/inline implementation selected by the project owner.
- Development is isolated on `feat/native-foundation`; `main` is not being used for implementation writes.
- The local execution container has Git but no Unity Editor, `dotnet`, `mcs`, or `csc`.
- The local container cannot resolve github.com, so repository changes are made through the authorized GitHub connector rather than a local git worktree.
- Automated Unity RED/GREEN runs therefore remain **pending**. Test files are committed before their matching implementations where practical, but no test-pass claim is made until Unity 2022.3.62f2 runs them.

## Rulings

1. **Isolation:** connector-backed feature branch substitutes for a local worktree because the container cannot clone GitHub.
2. **Unity version:** direct user requirement `2022.3.62f2` overrides the earlier Unity 6 assumption in design/plan documents.
3. **Web target:** Unity 2022.3 WebGL is treated as desktop-browser distribution; Android uses the native build.
4. **Editor-generated assets:** scenes/prefabs that require Unity serialization are generated through `ProjectSetup` rather than hand-writing fragile Unity YAML.

## Implemented Source (verification pending in Unity)

### Foundation
- Project version/package manifest pinned for Unity 2022.3.62f2
- Runtime / Editor / EditMode / PlayMode assembly definitions
- Persistent `GameBootstrap` / `GameServices`
- Versioned `SaveService` + `PlayerPrefsSaveStore`
- Unified keyboard/mobile `GameInputService`
- `MobileControlBridge`
- `SafeAreaFitter`
- `ProjectSetup` scene/build-settings generator
- WebGL and Android build entry points + build validation

### Maze
- `MazeGraph`, `MazeNode`, `MazeEdge`
- Deterministic PRNG + stable RunSeed
- ScriptableObject chapter/difficulty/room/gate definitions
- Hybrid critical-path + optional-branch generator
- Topology validator + 1,000-seed test authored
- `MazeRuntimeService` / `MazeRunState`
- Gate solved state and traversal check
- Fog-of-war `MazeMapService`
- Maze run save adapter
- Progressive maze hint service

### Questions
- Numeric subjective answer validator
- Numeric keypad input buffer
- Question data model
- Deterministic addition/subtraction generators

### Environment Puzzles
- Common environment-puzzle contract
- Sequence Plate puzzle
- Memory Path puzzle

### Crafting
- Item definitions / stack model
- Atomic material inventory service
- Guided recipe definition
- Transactional crafting service with rollback

## Authored Tests (Unity execution pending)

- Foundation: Bootstrap, SaveService, GameInput, SafeAreaFitter, ProjectSetup, BuildValidation
- Maze: Graph, Seed, Authoring, Generator, Validator, Runtime, Map, SaveAdapter, Hint
- Questions: NumericAnswerValidator, NumericInputBuffer, ArithmeticGenerator
- Puzzles: Sequence/Memory
- Inventory/Crafting: InventoryService, CraftingService

## Next Implementation Batch

1. Numeric keypad uGUI presenter + question service/reward-once guard
2. Multiple choice + multiplication/division + missing-number questions
3. Weight Bridge + Sokoban Lite + Laser/pipe puzzle
4. Equipment slots and five robot tools
5. 3x3 pattern crafting
6. Knowledge XP + enchant system
7. Maze equipment-gate integration
8. Chapter 1 vertical slice authoring
9. Unity 2022.3.62f2 EditMode/PlayMode execution and build smoke tests on a Unity-capable machine
