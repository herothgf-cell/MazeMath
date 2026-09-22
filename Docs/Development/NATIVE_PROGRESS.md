# Native Development Progress

Branch: `feat/native-foundation`

## Fixed Environment

- Unity Editor: **2022.3.62f2**
- Input System: **1.6.1**
- Unity Test Framework: **1.1.33**
- Unity UI: editor-bound core package (`com.unity.ugui`)
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

1. **Isolation:** connector-backed feature branch substitutes for a local worktree because the container cannot clone GitHub. Cost if wrong: branch-level isolation is preserved, but local worktree helper/ledger scripts cannot be used.
2. **Unity version:** direct user requirement `2022.3.62f2` overrides the earlier Unity 6 assumption in design/plan documents.
3. **Web target:** Unity 2022.3 WebGL is treated as desktop-browser distribution; Android uses the native build.
4. **Editor-generated assets:** scenes/prefabs that require Unity serialization will be generated through an Editor setup utility rather than hand-writing fragile YAML where possible.

## Implemented Source (verification pending in Unity)

- Unity project version/package manifest baseline
- Runtime/EditMode/PlayMode assembly definitions
- Persistent `GameBootstrap` and `GameServices`
- Versioned `SaveService` + `PlayerPrefsSaveStore`
- Unified keyboard/mobile `GameInputService`
- `MobileControlBridge`
- `SafeAreaFitter`

## Authored Tests (not yet executed)

- `GameBootstrapTests`
- `SaveServiceTests`
- `GameInputTests`
- `SafeAreaFitterTests`

## Next

1. Editor project setup + scene/build settings generator
2. WebGL/Android build entry points
3. MazeGraph domain model
4. deterministic seed/PRNG
5. maze generator/validator
