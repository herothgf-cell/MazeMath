# MazeMath

MazeMath is a Unity maze-adventure game designed around exploration, math problems, environmental puzzles, robot equipment crafting, and gradual progression.

## Current development milestone

The current implementation branch is building the deterministic maze foundation described in:

- `Docs/Design/01_MAZE_SYSTEM_DESIGN.md`
- `Docs/Design/02_CRAFTING_ENCHANT_DESIGN.md`
- `Docs/Design/03_PUZZLE_QUESTION_SYSTEM_DESIGN.md`
- `Docs/Development/MAZE_FOUNDATION_STATUS.md`

## Unity setup

1. Install Unity Hub.
2. Install the exact Unity 6.3 LTS patch recorded in `ProjectSettings/ProjectVersion.txt`.
3. Add **Web Build Support** and **Android Build Support** modules to that Editor installation.
4. Clone this repository.
5. Open the repository root as a Unity project.
6. On the first open, allow Unity to create/resolve the package manifest, package lock, and `.meta` files that are not yet present.
7. Commit the Unity-generated project metadata before starting Scene or prefab work.
8. Open **Window > General > Test Runner** and run EditMode tests.

## Pure core tests

Maze generation and validation are intentionally written as pure C# so they can be verified without a running Unity Editor.

GitHub Actions runs:

```bash
dotnet test Tools/CoreTests/MazeMath.CoreTests.csproj --configuration Release
```

The pure test harness links the same C# files used by Unity; it is not a duplicate implementation.

## Project structure

```text
Assets/
  MazeMath/
    Scripts/
      Core/
      Maze/
    Tests/
      EditMode/
Docs/
  Design/
  Development/
  Plans/
ProjectSettings/
Tools/
  CoreTests/
```

## Platform targets

- Web
- Android

Build profiles and deployment automation are later milestones. The current foundation milestone does not claim a verified Unity Web or Android build yet.
