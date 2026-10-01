# 15.2 — Continuous golem quizzes and visual reasoning

User brief: keep the 15.1 graphics, chapters, crafting and machines. Golem questions should continue without repeated conversations; add visual reasoning inspired by positional card actions and equal-symbol number bonds, reduce repeated questions, increase chapter question count and ramp difficulty gently.

Scope: deployed mobile web bundle, not Unity. Existing native execution/main/Pages workflow continues. No new art service, font, analytics or gameplay dependencies.

## Implementation ledger

- Read live main 2765eeb5566f99db592bbe6a3c66bf800cdad4e3. Actual runtime is embedded in index.html; the adjacent JavaScript files are a legacy 10.1 test copy.
- Rules RED: four missing-feature tests failed; card simulator/symbol uniqueness and graded budgets passed after implementation.
- Integration RED: no generated 15.2 release; checked build adapter now assembles quiz helpers with the exact deployed source and syntax-checks every emitted script.
- Variety RED: old pipe/laser each only used two layouts. Generate seeded visible paths; retain original constructors for pending old saves.
- Ruling: keep all original task and boss IDs. Append one field thinking device; boss sizes are 5 (1–3), 6 (4–6), 7 (7–9), 8 (10–12), 9 (13–15). Both new reasoning types occur in every chapter; chapter 5 has cards in the field and symbols at the golem rather than discarding an existing boss problem.
- Ruling: preserve unfinished problems across close/reload. Identical unsolved questions are intentional resumability, not repetition to reroll. Newly assigned variants check the last 128 fingerprints (up to 256 candidates). Authored teaching sequences may recur; a finite layout pool is not an infinite no-repeat guarantee.
- Ruling: new quiz helpers are modular under Web/Instant/quiz. build.cjs reads the existing 15.1 bundle and writes only _site/index.html with checked source anchors. The old modules are never accidentally published as the new game. Workflows must test and upload _site, not Web/Instant.
- Completed legacy chapters remain complete; no lost tools/XP or forced re-clear.
- No workbook photos/textures are shipped. Original card operations and shape diagrams implement the problem genres.

## Difficulty

Cards: 4/5/6/7/9 cards and 1/2/2/3/3 operations by three-chapter bands. Questions move from finding a position, to unchanged-card counts, to the explicitly defined max-minus-min difference among unchanged cards. All instructions refer to the current row.

Symbols: two then three connected clues, small integer answers, unique solutions checked independently by enumeration. The same shape always has the same value within a problem.

Arithmetic: gentle chapter-sized additive bounds; 2/3/5 multiplication tables and exact division. Memory never exceeds five symbols. No time limit or wrong-answer resource penalties.

## Verification boundaries

Local Chromium uses explicit inline/stub mode if localhost navigation is blocked. That mode does not prove real HTTP localStorage/reload. The Pages workflow must run both Chromium and WebKit over HTTP and test the generated 15.2 artifact before publish. Completion records here must distinguish those environments.

## Final self-review and local evidence

- Fixed the boss decoration hook: progress/pause must be added after every puzzle dispatch, not only after the mechanical dispatcher. Observed RED then GREEN.
- Fixed symbol answer layout so the numeric keypad stays compact instead of duplicating a large prompt.
- Fixed randomized pipe source/target markers to align with their actual logical row. Observed RED then GREEN.
- Final local Node suite: 14/14 passed. Includes 900 independently enumerated symbol cases, 1,800 card cases, 18,000 arithmetic cases and three complete 15-chapter model-navigation runs.
- Final local Chromium: all 105 golem questions across the 15 chapters plus reasoning interactions, pause/cancel, saved pending variant restoration and four mobile layouts passed. This local run used MM_INLINE=1, so it does not claim real HTTP storage or WebKit validation.
- Browser golem cases use valid prepared saves. The separate route test uses normal movement/collision and validated solver actions; neither is a substitute for child playtesting or a real-device performance test.
- No independent reviewer is available; self-review findings are recorded above. Remote CI must re-run these checks and both HTTP browser engines before Pages upload.
