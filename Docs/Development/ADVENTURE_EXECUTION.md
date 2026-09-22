# Adventure execution ledger

Baseline: main `2d4fe32c7c986c1313f17a4e405ffbe7b743db17`.
Approved: stabilization; physical maze play; world puzzles/workshop; boss/save/clear; WebGL/Android validation. Native implementation on main, explicitly authorized. Unity 2022.3.62f2.
Reference read: local `yeonjun_block_ui_v1.html` and mobile preview image.

## Decisions and costs

- Movement is a pure C# fixed-step AABB motor presented by Unity. This permits real .NET movement/gate/ladder tests. Moving platforms, slopes and rigid-body dynamics would require extension.
- Adventure is a separate committed scene, not a replacement for user-authored Bootstrap/Gameplay. Legacy setup creates only missing scenes. Users must open Adventure for the new gameplay.
- First chapter is authored three-floor geometry, not fully random tile generation. Question and plate permutations are seeded; procedural generation experiments remain available separately.
- Adventure owns one versioned state snapshot; legacy demo inventory/save services are not run as a second authority. Existing question validation and puzzle rules are reused. Future data-driven catalog migration needs an adapter.
- Runtime UI uses project/OS Korean fonts where available and English LegacyRuntime fallback otherwise. Web Korean delivery requires a user-provided licensed font asset.
- No Unity Editor/build modules are available in this container. Actual .NET CI is used for pure logic; Unity import/rendering, EditMode/PlayMode, WebGL and APK gates remain unverified until the supplied script runs on a Unity machine. No release upload performed.

## Verified evidence before final runtime integration

- Laser initialization regression: run 35749788258, job 106820626307, 2 failing laser tests from an uninitialized field. The constructor fix initializes this.mirrors.
- Domain test run 35752031364 / d853310f848311afa4e23ae35d5788f7cd899d0f: GitHub Actions completed successfully.
- Overweight scale regression: run 35754747623, job 106837555770, 33 total / 32 passed / 1 failed. Removing an extra crate reached 8kg without marking the puzzle solved. Re-evaluation after removal addresses it.
- New regression tests cover multi-touch release, repeated rewards, data corruption, unfinished question recovery, equipped-only effects, floor access, locked-gate jump bypass, and the authored full route.
- Unity-specific tests are authored, not represented as executed. C#9 syntax checks do not validate Unity assembly API compatibility.

## Review

Final review is author self-review; no independent subagent reviewer was available. Reviewed scene GUID wiring, old scene preservation, source imports, editor/runtime separation, immutable pattern copies, one-authority rewards, modal input clearing, platform-specific input fallback and build output checks.

## Deferred polish

- Device-specific safe-area and typography tuning needs actual device screenshots.
- More varied authored layouts, additional world-space puzzle types, animation and audio polish are not release-complete.
