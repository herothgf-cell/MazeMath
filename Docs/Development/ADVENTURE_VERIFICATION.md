# Adventure verification record

## Actual executed check

Code commit: `b9c3276dbccef4835188682bd784f34dc8c73069`

GitHub Actions run: https://github.com/herothgf-cell/MazeMath/actions/runs/35755531050

Job: `106840210617`

Command:

```bash
dotnet test Tools/CoreTests/MazeMath.CoreTests.csproj --configuration Release --verbosity minimal --logger "trx;LogFileName=core.trx" --results-directory TestResults
```

Observed log result:

```text
Passed! - Failed: 0, Passed: 42, Skipped: 0, Total: 42
```

The 42 checks include real engine-independent C# logic and C#9 syntax/scene GUID checks. They cover physical walking/jumping/ladder/gate rules, the authored chapter route simulation, inventory/crafting transactions, reward deduplication, boss phase rules, state validation/serialization, waypoint navigation and regressions in laser/weight puzzles. Source parsing is exercised for Editor/WebGL/Android and legacy input defines.

TRX artifact: `core-test-results` in that workflow run.

## Regression evidence

Laser constructor tests failed with NullReferenceException before the `this.mirrors` initialization fix (run 35749788258).

Overweight scale correction failed before `RemoveWeight` re-evaluated the exact target (run 35754747623; 32 passed / 1 failed). Both regressions pass in the code commit above.

## Not executed here

- Unity 2022.3.62f2 project import and Unity assembly compilation.
- Unity Test Runner EditMode / PlayMode.
- Rendering/layout/playthrough in the actual Unity Game view.
- WebGL build and browser runtime.
- Android APK build, install and on-device play.
- Store signing, AAB submission or release.

Unity tests and `Tools/verify-unity.ps1` are provided for those next verification gates. Do not interpret source syntax tests or the pure C# route simulation as Unity rendering, UI layout, or target-platform build certification.

Review method: author self-review; no independent reviewer agent or Unity build service was available.
