# UI follow-up — Cozy UI 3.1

Unity 2022.3.62f2 / main. This patch preserves the approved cream/mint cute robot presentation and the crafting/enchant/puzzle rules.

## Confirmed code gaps

The earlier polish updated AdventureHud, but the old Gameplay demo still called QuestionPanelView. That panel retained a 720x820 size, 42-point prompt and no success-dismiss action. Bootstrap also selected build index 1, so opening the previously documented Bootstrap/Gameplay entry could still show the old experience. These are confirmed source gaps; the user's actual local scene/version could not be observed.

## Changes

- Bootstrap now calls AdventureEntry.EnsureCurrent instead of loading build index 1.
- A scene-load hook redirects active old Chapter1VerticalSliceController scenes before their Start creates the demo HUD. The current Adventure instance is reused. Only known generated legacy HUD canvases/raycasters are disabled, and only during Play. Scene files and hand-authored content are not overwritten.
- Adventure, Bootstrap and the original Gameplay demo use the same cute robot art, compact header, separate control dock and cropped world viewport.
- Adventure's answer-dismiss clock also runs in a dedicated Update independently from presentation/LateUpdate. The existing shared QuestionFeedbackFlow prevents duplicate closes.
- Standalone QuestionPanelView now observes QuestionSession.Completed, shows short success feedback and hides itself after 0.6 seconds of unscaled time. Incorrect/invalid answers stay open. Hiding/replacing a question cancels and unsubscribes the previous session.
- The standalone question panel/keypad/choices now use AdventureTheme: cream cards, mint accents, rounded surfaces, normal-weight readable text. Prompt maximum 28, answer 26, keys 22, labels 16. Numeric buttons size from their actual parent, not a hard-coded 118x88 cell.
- Header identifies the running revision as Cozy UI 3.1. No new external image/font dependency is introduced.

## Run

Exit Play, preserve any local edits, then run `git pull --ff-only origin main`. Open `MazeMath > Play Adventure` or `Assets/MazeMath/Scenes/Adventure.unity` and Play. Bootstrap/old Gameplay also route to this current experience at runtime. No Setup Project, scene recreation or save reset is needed. Existing adventure saves retain their key/version.

## Validation and boundaries

`Tools/CoreTests/UiEntryRegressionTests.cs` covers startup policy and source integration checks for the legacy entry, shared dismissal, independent HUD timing and removal of oversized fixed question views. These checks are not a Unity player build.

`Assets/MazeMath/Tests/PlayMode/UI/QuestionEntryFollowupTests.cs` exercises actual panel lifecycle with numeric/multiple-choice answers, zero timeScale, replacement and external completion events, plus old-scene handoff. These Unity tests are authored but must run on a machine with Unity 2022.3.62f2. The implementation environment does not have that Editor.

Check 1280x720, 1920x1080 and Android landscape in Unity. The current design keeps UI outside the camera area, not over the walking path; live screenshot review remains necessary for device-specific fonts or unexpected local canvases.
