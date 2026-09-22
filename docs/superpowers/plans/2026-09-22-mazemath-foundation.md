# MazeMath Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create the minimal Unity 6 project foundation required for MazeMath to build reliably for Web and Android and to host the maze, learning, and progression subsystems behind stable interfaces.

**Architecture:** Use a single Bootstrap scene that constructs long-lived services, a Gameplay scene for the playable world, and pure C# domain logic wherever possible so EditMode tests can exercise systems without scene loading. Runtime services communicate through interfaces and ID-based contracts; UI and subsystem-specific MonoBehaviours do not directly depend on concrete implementations outside their domain.

**Tech Stack:** Unity 6 LTS, C#, Unity Test Framework, uGUI or Unity UI Toolkit only if selected consistently during implementation, Unity Input System, JSON serialization for save payloads.

**Spec:** `Docs/Design/01_MAZE_SYSTEM_DESIGN.md`, `Docs/Design/02_CRAFTING_ENCHANT_DESIGN.md`, `Docs/Design/03_PUZZLE_QUESTION_SYSTEM_DESIGN.md`

## Global Constraints

- Target Unity 6 LTS; pin the exact patch version in `ProjectSettings/ProjectVersion.txt`.
- Primary deployment targets are Web and Android.
- Android product minimum is API 23; store target API follows Google Play policy at release time.
- All gameplay must remain operable on Android touch without keyboard or hover.
- Pure domain logic must not depend on scene objects when an interface/data model is sufficient.
- Subsystems communicate through interfaces/events and stable string IDs.
- Direct cross-subsystem references to another subsystem's UI or MonoBehaviour are prohibited.
- Save access goes through `ISaveStore`; gameplay code does not call PlayerPrefs directly.
- No production code is added without a failing test first where Unity Test Framework can cover the behavior.

## Review Focus

1. Bootstrap is loaded twice or Gameplay is reopened: services must not duplicate or create two save writers.
2. Corrupt/empty save JSON: game must fall back to a valid fresh profile without crashing.
3. Web page refresh and Android process restart: last committed checkpoint/progression snapshot must deserialize without duplicate rewards.
4. Touch-only input: all required gameplay actions must have a mapped input action and visible control path.
5. Resolution/aspect ratio change: HUD root and modal panels must remain inside safe area and usable.

---

### Task 1: Create Unity project skeleton and package baseline

**Files:**
- Create: `Packages/manifest.json`
- Create: `Packages/packages-lock.json` via Unity
- Create: `ProjectSettings/ProjectVersion.txt` via Unity project creation
- Create: `Assets/MazeMath/Scenes/Bootstrap.unity`
- Create: `Assets/MazeMath/Scenes/Gameplay.unity`
- Create: `Assets/MazeMath/Tests/EditMode/MazeMath.EditMode.Tests.asmdef`
- Create: `Assets/MazeMath/Tests/PlayMode/MazeMath.PlayMode.Tests.asmdef`
- Create: `Assets/MazeMath/Scripts/MazeMath.Runtime.asmdef`

**Interfaces:**
- Produces: a compiling Unity project, test assemblies, Bootstrap and Gameplay scenes.
- Consumes: none.

- [ ] **Step 1: Create the Unity 6 LTS project in the repository root**

Use Unity Hub to create a 2D project directly in the cloned `MazeMath` repository. Keep the generated `Assets/`, `Packages/`, and `ProjectSettings/` folders at the root.

- [ ] **Step 2: Add Input System and Test Framework**

In Package Manager, ensure these packages are present:
- Input System
- Test Framework

Do not add third-party runtime packages.

- [ ] **Step 3: Create runtime and test assembly definitions**

`Assets/MazeMath/Scripts/MazeMath.Runtime.asmdef`:
~~~json
{
  "name": "MazeMath.Runtime",
  "rootNamespace": "MazeMath"
}
~~~

`Assets/MazeMath/Tests/EditMode/MazeMath.EditMode.Tests.asmdef`:
~~~json
{
  "name": "MazeMath.EditMode.Tests",
  "references": ["MazeMath.Runtime"],
  "optionalUnityReferences": ["TestAssemblies"]
}
~~~

`Assets/MazeMath/Tests/PlayMode/MazeMath.PlayMode.Tests.asmdef`:
~~~json
{
  "name": "MazeMath.PlayMode.Tests",
  "references": ["MazeMath.Runtime"],
  "optionalUnityReferences": ["TestAssemblies"]
}
~~~

- [ ] **Step 4: Run a compilation smoke test**

Run Unity in batch mode with the exact installed editor path:
~~~bash
Unity -batchmode -quit -projectPath . -logFile -
~~~

Expected: process exits 0 with no C# compiler errors.

- [ ] **Step 5: Commit**

~~~bash
git add Assets Packages ProjectSettings
git commit -m "chore: initialize Unity MazeMath project"
~~~

---

### Task 2: Add core service bootstrap and duplicate protection

**Files:**
- Create: `Assets/MazeMath/Scripts/Core/IGameService.cs`
- Create: `Assets/MazeMath/Scripts/Core/GameBootstrap.cs`
- Create: `Assets/MazeMath/Scripts/Core/GameServices.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Core/GameBootstrapTests.cs`
- Modify: `Assets/MazeMath/Scenes/Bootstrap.unity`

**Interfaces:**
- Produces:
  - `IGameService.Initialize()`
  - `GameServices.Register<T>(T service)`
  - `GameServices.Get<T>()`
- Consumes: Unity lifecycle only.

- [ ] **Step 1: Write failing duplicate-bootstrap test**

~~~csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace MazeMath.Tests
{
    public class GameBootstrapTests
    {
        [UnityTest]
        public IEnumerator CreatingSecondBootstrapDoesNotCreateSecondRegistry()
        {
            var first = new GameObject("BootstrapA").AddComponent<GameBootstrap>();
            yield return null;
            var firstRegistry = GameServices.Instance;

            var second = new GameObject("BootstrapB").AddComponent<GameBootstrap>();
            yield return null;

            Assert.AreSame(firstRegistry, GameServices.Instance);
            Assert.AreEqual(1, Object.FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None).Length);
        }
    }
}
~~~

- [ ] **Step 2: Run PlayMode test and confirm it fails**

Run the Unity Test Framework PlayMode suite.

Expected: failure because `GameBootstrap` / `GameServices` do not exist.

- [ ] **Step 3: Implement minimal bootstrap**

`IGameService.cs`:
~~~csharp
namespace MazeMath.Core
{
    public interface IGameService
    {
        void Initialize();
    }
}
~~~

`GameServices.cs`:
~~~csharp
using System;
using System.Collections.Generic;

namespace MazeMath.Core
{
    public sealed class GameServices
    {
        public static GameServices Instance { get; private set; }
        private readonly Dictionary<Type, object> services = new();

        public static GameServices Ensure()
        {
            return Instance ??= new GameServices();
        }

        public void Register<T>(T service) where T : class
        {
            services[typeof(T)] = service;
        }

        public T Get<T>() where T : class
        {
            return services.TryGetValue(typeof(T), out var value) ? value as T : null;
        }
    }
}
~~~

`GameBootstrap.cs` must:
- destroy a second bootstrap GameObject in `Awake`
- call `DontDestroyOnLoad`
- call `GameServices.Ensure()`

- [ ] **Step 4: Run PlayMode test**

Expected: pass.

- [ ] **Step 5: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Core Assets/MazeMath/Tests/PlayMode/Core Assets/MazeMath/Scenes/Bootstrap.unity
git commit -m "feat: add persistent game bootstrap"
~~~

---

### Task 3: Add save abstraction and corruption fallback

**Files:**
- Create: `Assets/MazeMath/Scripts/Core/Save/ISaveStore.cs`
- Create: `Assets/MazeMath/Scripts/Core/Save/PlayerPrefsSaveStore.cs`
- Create: `Assets/MazeMath/Scripts/Core/Save/SaveService.cs`
- Create: `Assets/MazeMath/Scripts/Core/Save/SaveEnvelope.cs`
- Create: `Assets/MazeMath/Tests/EditMode/Core/SaveServiceTests.cs`

**Interfaces:**
- Produces:
  - `ISaveStore.Save(string key, string json)`
  - `ISaveStore.TryLoad(string key, out string json)`
  - `ISaveStore.Delete(string key)`
  - `SaveService.Save<T>(string key, T data)`
  - `SaveService.TryLoad<T>(string key, out T data)`
- Consumes: Unity `JsonUtility`; PlayerPrefs only in `PlayerPrefsSaveStore`.

- [ ] **Step 1: Write failing tests using an in-memory fake store**

Tests:
1. roundtrip serializable payload.
2. missing key returns false.
3. corrupt JSON returns false and does not throw.
4. save envelope version is persisted.

Use a test-only fake implementing `ISaveStore`.

- [ ] **Step 2: Run EditMode tests**

Expected: fail because save contracts are missing.

- [ ] **Step 3: Implement save contracts**

`ISaveStore`:
~~~csharp
public interface ISaveStore
{
    void Save(string key, string json);
    bool TryLoad(string key, out string json);
    void Delete(string key);
}
~~~

`SaveEnvelope`:
~~~csharp
[System.Serializable]
public sealed class SaveEnvelope
{
    public int version;
    public string payload;
}
~~~

`SaveService` wraps payload JSON in a versioned envelope and catches deserialization exceptions, returning false.

- [ ] **Step 4: Run EditMode tests**

Expected: all save tests pass.

- [ ] **Step 5: Commit**

~~~bash
git add Assets/MazeMath/Scripts/Core/Save Assets/MazeMath/Tests/EditMode/Core
git commit -m "feat: add versioned save service"
~~~

---

### Task 4: Add unified gameplay input contract

**Files:**
- Create: `Assets/MazeMath/Input/MazeMath.inputactions`
- Create: `Assets/MazeMath/Scripts/Input/IGameInput.cs`
- Create: `Assets/MazeMath/Scripts/Input/GameInputService.cs`
- Create: `Assets/MazeMath/Scripts/UI/Mobile/MobileControlBridge.cs`
- Create: `Assets/MazeMath/Tests/PlayMode/Input/GameInputTests.cs`

**Interfaces:**
- Produces:
  - `Vector2 Move`
  - `bool JumpPressed`
  - `bool InteractPressed`
  - `bool MapPressed`
  - `bool InventoryPressed`
- Consumes: Unity Input System actions.

- [ ] **Step 1: Write failing tests for action state and mobile bridge**

Test keyboard-emulated action callbacks and direct mobile button injection through `MobileControlBridge`.

- [ ] **Step 2: Run PlayMode tests**

Expected: fail because contract is absent.

- [ ] **Step 3: Create actions**

Action map `Gameplay`:
- Move: WASD/arrows + on-screen control
- Jump: Space + touch
- Interact: E + touch
- Map: M + touch
- Inventory: B + touch

- [ ] **Step 4: Implement `GameInputService` and bridge**

Mobile UI must call the same service-level intent methods rather than separate gameplay code paths.

- [ ] **Step 5: Run PlayMode tests**

Expected: keyboard and bridge produce the same logical action state.

- [ ] **Step 6: Commit**

~~~bash
git add Assets/MazeMath/Input Assets/MazeMath/Scripts/Input Assets/MazeMath/Scripts/UI/Mobile Assets/MazeMath/Tests/PlayMode/Input
git commit -m "feat: add cross-platform gameplay input"
~~~

---

### Task 5: Add safe-area responsive HUD shell

**Files:**
- Create: `Assets/MazeMath/Scripts/UI/SafeAreaFitter.cs`
- Create: `Assets/MazeMath/Prefabs/UI/GameplayHUD.prefab`
- Create: `Assets/MazeMath/Tests/PlayMode/UI/SafeAreaFitterTests.cs`
- Modify: `Assets/MazeMath/Scenes/Gameplay.unity`

**Interfaces:**
- Produces: stable HUD anchors for objective, minimap, hotbar, mobile movement/actions.
- Consumes: `Screen.safeArea`.

- [ ] **Step 1: Write failing layout test**

Create a Canvas + SafeAreaFitter in test, inject a simulated safe rect, assert target RectTransform anchors remain inside the rect.

- [ ] **Step 2: Run test and confirm failure**

- [ ] **Step 3: Implement SafeAreaFitter and HUD anchors**

Required anchors:
- top-left: objective
- top-right: floor/minimap
- bottom-center: hotbar
- bottom-left: mobile movement
- bottom-right: jump/interact/map/inventory

- [ ] **Step 4: Run PlayMode UI test**

Expected: pass across simulated 16:9, 20:9, and 4:3 safe areas.

- [ ] **Step 5: Commit**

~~~bash
git add Assets/MazeMath/Scripts/UI Assets/MazeMath/Prefabs/UI Assets/MazeMath/Tests/PlayMode/UI Assets/MazeMath/Scenes/Gameplay.unity
git commit -m "feat: add responsive gameplay HUD shell"
~~~

---

### Task 6: Add build smoke scripts for Web and Android

**Files:**
- Create: `Assets/MazeMath/Editor/Build/BuildWeb.cs`
- Create: `Assets/MazeMath/Editor/Build/BuildAndroid.cs`
- Create: `Assets/MazeMath/Editor/Build/BuildValidation.cs`

**Interfaces:**
- Produces:
  - menu/batch entry points for Web build
  - menu/batch entry points for Android build
  - pre-build scene and project validation
- Consumes: Bootstrap and Gameplay scenes.

- [ ] **Step 1: Write editor validation test**

Validate that both required scenes exist and are included in build settings.

- [ ] **Step 2: Run EditMode test and confirm failure**

- [ ] **Step 3: Implement validation and build entry points**

Web output:
`Build/Web/`

Android output:
`Build/Android/MazeMath.apk` for local smoke builds.

- [ ] **Step 4: Run compile + EditMode + PlayMode suites**

Run:
~~~bash
Unity -batchmode -quit -projectPath . -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -logFile -
Unity -batchmode -quit -projectPath . -runTests -testPlatform PlayMode -testResults TestResults/playmode.xml -logFile -
~~~

Expected: 0 failed tests.

- [ ] **Step 5: Run Web build smoke**

Run:
~~~bash
Unity -batchmode -quit -projectPath . -executeMethod MazeMath.Editor.BuildWeb.PerformBuild -logFile -
~~~

Expected: build exits 0 and `Build/Web/index.html` exists.

- [ ] **Step 6: Run Android build smoke where Android module is installed**

Run:
~~~bash
Unity -batchmode -quit -projectPath . -executeMethod MazeMath.Editor.BuildAndroid.PerformBuild -logFile -
~~~

Expected: build exits 0 and `Build/Android/MazeMath.apk` exists.

- [ ] **Step 7: Commit**

~~~bash
git add Assets/MazeMath/Editor ProjectSettings
git commit -m "build: add Web and Android build entry points"
~~~

## Plan Completion Gate

Foundation is complete only when:
- Unity compiles from a fresh clone.
- EditMode and PlayMode tests both report 0 failures.
- duplicate Bootstrap test passes.
- corrupt save test passes.
- touch and keyboard route into the same input contract.
- safe-area tests pass.
- Web build succeeds.
- Android build succeeds on an editor install with Android Build Support.

After this plan, execute the Maze System plan before Puzzle/Question and Crafting/Enchant integration.
