# MazeMath Native development status

Target branch: **main** (owner explicitly authorized). Editor: **Unity 2022.3.62f2**.

## Playable chapter entry

Open `Assets/MazeMath/Scenes/Adventure.unity` or choose `MazeMath > Play Adventure`. Scene and script GUID metadata are committed. No first-run scene generator is required.

See [Adventure run/build guide](ADVENTURE_PLAY_GUIDE.md) for controls, progression, save behavior and validation commands.

## New Adventure implementation

- Block UI from the HTML reference: objective card, hearts, XP, six hotbar slots, inventory grid, workbench tabs, numeric keypad, touch controls.
- Physical side-scroll walking/jumping, 3 floors/15 areas, actual ladders, collision barriers, meaningful workbench backtracking and unlockable shortcut.
- World interactions for weight crates/scale, sequence plates and mirror power devices.
- Five Tier 1 tools, guided and 3×3 crafting, two selectable enchants per tool and equipped-only effects.
- Guardian with three puzzle shields, warning pulse, explicit final interaction and chapter-clear screen.
- Versioned single-state save, valid backup recovery, material overflow queue, claimed-reward records, unfinished math question restoration.
- Build menu targets Adventure without regenerating scenes. PowerShell script checks Unity test results and WebGL/APK outputs.

## Legacy stability

Old demo services remain available. Fixed laser constructor state, scale re-evaluation after removing excess weight, and enchant effects on unequipped legacy tools. Setup Project only creates missing legacy scenes; existing scenes and content are preserved.

## Verification boundaries

GitHub Actions runs real pure C# tests and source syntax checks. Unity-specific tests have been authored but not run in this environment. Unity Editor import/play, final UI layout, WebGL output, APK output and real-device play remain separate pending gates. A successful core workflow must not be described as a successful Unity build.

## Scope boundaries

The chapter layout is authored, not fully random. New-run questions and plate permutations are seeded. Tier 2 tools, later chapters, store signing/AAB release and cloud saves are not implemented in Adventure. No generated font binaries or copied Minecraft assets are included.
