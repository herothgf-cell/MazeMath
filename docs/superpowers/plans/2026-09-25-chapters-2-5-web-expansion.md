# MazeMath Chapters 2–5 Web Expansion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convert the current single-chapter mobile web game into a five-chapter campaign, then implement Chapters 2–5 from `Docs/Design/CHAPTER_2_5_ROADMAP.md` without regressing Chapter 1 or the GitHub Pages mobile experience.

**Architecture:** Keep the no-build, static GitHub Pages deployment. Extract chapter data, puzzle rules, campaign save/migration, and rendering responsibilities from the current monolithic `game.js`. Chapter definitions become data consumed by shared movement, objective, tutorial, puzzle, boss, and renderer code; Chapter 1 is migrated first as a regression baseline before adding new chapters.

**Tech Stack:** Vanilla JavaScript (UMD/browser globals + CommonJS for Node tests), Canvas 2D, localStorage, Node 22 test runner, Playwright Chromium/WebKit, GitHub Actions, GitHub Pages.

**Spec:** `Docs/Design/CHAPTER_2_5_ROADMAP.md`

## Global Constraints

- Keep the current install-free mobile browser experience and GitHub Pages deployment.
- Preserve direct character movement, touch controls, jumping, ladders, crafting, equipment, enchantments, puzzles, bosses, and local browser save.
- Do not add new permanent equipment beyond the existing five tools.
- Multiplication/division stays easy; difficulty rises through reasoning, memory, route choice, and multi-step tasks rather than large numbers.
- Wrong answers, hints, and getting lost never remove XP/materials.
- Every objective must state **action + target** so a child can identify the next action within five seconds.
- New mechanics receive a one-time TutorialCue with “what / why / how”; repeated use gets a short reminder only.
- Chapter 1 existing saves must migrate forward rather than reset.
- Every chapter must have Chromium + WebKit mobile completion coverage before Pages deployment.

## Review Focus

1. **Existing v1 saves:** A Chapter 1 save made before campaign support must restore into Chapter 1 with inventory, solved flags, XP, and current position intact.
2. **Chapter boundary saves:** Closing/reloading immediately after clearing a boss must not relock the next chapter or duplicate rewards.
3. **Touch input during modal transitions:** Opening tutorial/question/chapter-clear dialogs must release held movement so the character never keeps walking.
4. **Small mobile screens:** 320×568 portrait and 667×375 landscape must keep movement, jump, use, and chapter navigation visible without page scrolling.
5. **Objective dead ends:** For every campaign state reachable in tests, `objectiveFor(state)` must resolve to a valid target/action instead of a stale or already-completed step.

---

### Task 1: Campaign Save v2 and Chapter Definition Contract

**Files:**
- Create: `Web/Instant/chapter-data.js`
- Modify: `Web/Instant/index.html`
- Modify: `Web/Instant/core.js`
- Modify: `Web/Instant/core.test.cjs`

**Interfaces:**
- Produces: `MMChapters.get(id)`, `MMChapters.list()`, `MMChapters.firstId`
- Produces: `MM.freshCampaign(seed)`, `MM.restore(raw)`, `MM.startChapter(state,id)`, `MM.unlocks(state)`
- Save shape: `{v:2, seed, chapter, unlockedChapters, completedChapters, chapterState, inventory, xp, ...}`

- [ ] **Step 1: Write failing save-migration and definition tests**

Add tests that require:

```js
const D=require('./chapter-data.js');

test('five chapter definitions expose stable ids and starts',()=>{
  assert.deepEqual(D.list().map(x=>x.id),['chapter-01','chapter-02','chapter-03','chapter-04','chapter-05']);
  assert.deepEqual(D.get('chapter-02').start,[8,0]);
  assert.equal(D.get('chapter-02').boss.id,'furnace-golem');
});

test('v1 save migrates to campaign v2 without progress loss',()=>{
  const legacy=C.fresh(123);
  C.complete(legacy,'math');
  legacy.x=22;
  const restored=C.restore(JSON.stringify(legacy));
  assert.equal(restored.v,2);
  assert.equal(restored.chapter,'chapter-01');
  assert.equal(restored.chapterState.x,22);
  assert.ok(restored.chapterState.flags.includes('math'));
});

test('chapter clear unlock is idempotent',()=>{
  const s=C.freshCampaign(9);
  C.markChapterComplete(s,'chapter-01');
  C.markChapterComplete(s,'chapter-01');
  assert.deepEqual(s.unlockedChapters,['chapter-01','chapter-02']);
});
```

- [ ] **Step 2: Run the focused tests and verify RED**

Run:

```bash
node --test Web/Instant/core.test.cjs
```

Expected: FAIL because `chapter-data.js`, campaign v2 helpers, and migration do not exist.

- [ ] **Step 3: Implement the chapter contract and migration**

`chapter-data.js` must use a UMD wrapper so Node tests can `require` it and the browser exposes `window.MMChapters`.

Each definition contains:

```js
{
  id:'chapter-02',
  order:2,
  title:'용암 제련소',
  floors:3,
  width:64,
  start:[8,0],
  requiredTool:1,
  palette:{sky:'#...', stone:'#...', accent:'#...'},
  objectives:[/* ordered declarative steps */],
  things:[/* interactable definitions */],
  ladders:[/* static + conditionally unlocked ladders */],
  boss:{id:'furnace-golem', phases:['boss.math','boss.power','boss.plates']}
}
```

`core.js` accepts both old v1 state and v2 campaign state. Migration copies the entire legacy Chapter 1 state under `chapterState`, creates `inventory` from its existing item/tool arrays, and initializes Chapter 1 as unlocked.

- [ ] **Step 4: Update browser script order**

In `index.html` load:

```html
<script src="chapter-data.js"></script>
<script src="core.js"></script>
<script src="game.js"></script>
```

- [ ] **Step 5: Run core tests and verify GREEN**

Run:

```bash
node --test Web/Instant/core.test.cjs
```

Expected: all tests pass.

- [ ] **Step 6: Commit**

```bash
git add Web/Instant/chapter-data.js Web/Instant/index.html Web/Instant/core.js Web/Instant/core.test.cjs
git commit -m "feat: add campaign chapter definitions and save migration"
```

---

### Task 2: Move Chapter 1 from Hard-Coded Logic to Shared Chapter Runtime

**Files:**
- Create: `Web/Instant/chapter-runtime.js`
- Create: `Web/Instant/world-renderer.js`
- Modify: `Web/Instant/index.html`
- Modify: `Web/Instant/core.js`
- Modify: `Web/Instant/game.js`
- Modify: `Web/Instant/core.test.cjs`
- Modify: `Web/Instant/browser.test.cjs`

**Interfaces:**
- Consumes: `MMChapters.get(chapterId)`
- Produces: `MMRuntime.objectiveFor(campaign)`, `MMRuntime.things(campaign)`, `MMRuntime.solids(campaign)`, `MMRuntime.ladders(campaign)`, `MMRuntime.interact(campaign, thingId)`
- Produces: `MMRenderer.draw(ctx, viewport, campaign, runtimeView)`

- [ ] **Step 1: Add Chapter 1 parity tests**

Pin the existing Chapter 1 route:

```js
test('data-driven chapter 1 keeps the current required route',()=>{
  const s=C.freshCampaign(11);
  assert.match(R.objectiveFor(s).text,/숫자 장치/);
  // complete first number lock
  R.completeStep(s,'math');
  assert.match(R.objectiveFor(s).text,/곡괭이 팔/);
  // then mining -> bridge -> laser -> plates -> boss phases -> boss inspect
});
```

Browser test must still complete Chapter 1 with touch input and verify first-craft tutorial + post-mirror “골렘 조사” guidance.

- [ ] **Step 2: Verify RED before refactor**

Run both:

```bash
node --test Web/Instant/core.test.cjs
node Web/Instant/browser.test.cjs
```

Expected: new shared-runtime tests fail because `chapter-runtime.js` and `world-renderer.js` are missing.

- [ ] **Step 3: Extract world/objective/interactable evaluation**

Move Chapter 1-specific `goal`, `waypoint`, `things`, static solids, ladder definitions, boss phase lists, and palette data out of `game.js`.

The runtime resolves condition expressions only through named predicates:

```js
const predicates={
  flag:(s,key)=>hasFlag(s,key),
  ownsTool:(s,i)=>s.inventory.owned[i],
  equippedTool:(s,i)=>s.inventory.equipped[i],
  chapterComplete:(s,id)=>s.completedChapters.includes(id)
};
```

Do not eval arbitrary strings.

- [ ] **Step 4: Extract Canvas world rendering**

Move environment and object drawing into `world-renderer.js`. Keep player/Momo drawing shared. Renderers read chapter palette and thing `kind`; they do not mutate game progress.

- [ ] **Step 5: Make `game.js` an orchestration layer**

`game.js` retains DOM event binding, dialogs, input collection, save calls, and high-level runtime calls. It no longer owns chapter coordinates, boss phase arrays, or world palettes.

- [ ] **Step 6: Run parity tests**

Run:

```bash
node --test Web/Instant/core.test.cjs
node Web/Instant/browser.test.cjs
```

Expected: Chapter 1 route, tutorials, persistence, touch controls, all four viewports, Chromium, and WebKit pass.

- [ ] **Step 7: Commit**

```bash
git add Web/Instant
git commit -m "refactor: make chapter one use shared campaign runtime"
```

---

### Task 3: Chapter Select, Unlock Flow, and Cross-Chapter Inventory

**Files:**
- Modify: `Web/Instant/game.js`
- Modify: `Web/Instant/core.js`
- Modify: `Web/Instant/index.html`
- Modify: `Web/Instant/browser.test.cjs`

**Interfaces:**
- Consumes: `MM.unlocks(state)`, `MM.startChapter(state,id)`
- Produces UI: `chapterSelect()`, `chapterClear(chapterId)`

- [ ] **Step 1: Add failing campaign navigation tests**

Browser tests require:

```js
assert.equal(await text('#chapter02 .lock'),'잠김');
// finish Chapter 1
assert.equal(await text('#chapter02 .lock'),'도전 가능');
await click('#chapter02');
assert.equal(await page.evaluate(()=>MMApp.state().chapter),'chapter-02');
```

Reload after selecting Chapter 2 must restore the same chapter and inventory.

- [ ] **Step 2: Verify RED**

Run `node Web/Instant/browser.test.cjs`; expected FAIL because there is no chapter-select UI.

- [ ] **Step 3: Implement title/chapter select**

Title screen gains “챕터 선택”. Locked chapters show the immediate prerequisite only. Chapter clear screen gains “다음 챕터” when unlocked.

- [ ] **Step 4: Define cross-chapter inventory rule**

Materials, tools, enchant unlocks, XP, and completed chapters are campaign-global. Position, checkpoints, puzzle flags, visited rooms, and pending question are chapter-local.

Starting a chapter resets only chapter-local state.

- [ ] **Step 5: Verify persistence + mobile layout**

Run browser tests at 393×852, 844×390, 320×568, and 667×375.

- [ ] **Step 6: Commit**

```bash
git add Web/Instant
git commit -m "feat: add chapter select and campaign progression"
```

---

### Task 4: Chapter 2 — 용암 제련소 Vertical Slice

**Files:**
- Modify: `Web/Instant/chapter-data.js`
- Modify: `Web/Instant/chapter-runtime.js`
- Modify: `Web/Instant/world-renderer.js`
- Modify: `Web/Instant/core.js`
- Modify: `Web/Instant/game.js`
- Modify: `Web/Instant/core.test.cjs`
- Modify: `Web/Instant/browser.test.cjs`

**Interfaces:**
- New puzzle runtime: `MMRuntime.pipePuzzle(state,puzzleId,action)`
- New platform runtime: `MMRuntime.platformState(state,platformId)`
- Chapter 2 boss phases: `boss.math`, `boss.power`, `boss.plates`

- [ ] **Step 1: Write Chapter 2 route and arithmetic tests**

Require 2/3/5-times multiplication and exact division questions:

```js
for(let seed=1;seed<200;seed++){
  const q=C.chapterQuestion(seed,'chapter-02','generator-a',2);
  if(q.op==='mul') assert.ok([2,3,5].includes(q.a));
  if(q.op==='div') assert.equal(q.a%q.b,0);
}
```

Route test:

```text
power-off terminal
→ explicit “파워 렌치 만들기” tutorial
→ repair generator A
→ route-memory branch to generator B
→ power bridge
→ furnace golem math
→ pipe connection
→ plate sequence
→ inspect boss
```

- [ ] **Step 2: Verify RED**

Run core/browser tests; expected failure for missing Chapter 2 interactions.

- [ ] **Step 3: Implement Chapter 2 definition**

Use 3 floors, 64 world units, two generator branches, one optional Mining Arm shortcut, and a visible return route to the workbench.

Tutorial copy when wrench is first needed:

```text
지금 만들 것: ⚒ 파워 렌치
왜 필요해요? 멈춘 발전기를 고칠 수 있어요.
어디서? 작업대에서 만들어요.
필요 재료: 철 조각 2 + 기어 1 + 에너지석 1
```

- [ ] **Step 4: Implement pipe/power puzzle**

Pipe cells rotate through 0/90/180/270. The puzzle is solved only when the source has a connected path to the target; wrong rotations consume nothing.

- [ ] **Step 5: Implement moving-platform state**

Movement is deterministic and pauses while a modal is open. Collision uses current platform bounds; falling returns to checkpoint without removing materials/XP.

- [ ] **Step 6: Implement Furnace Golem**

HUD always shows:

```text
용광로 골렘 · 보호막 2/3
다음: 오른쪽 배관 장치를 조사해 연결하세요.
```

After final phase: “보호막 0개 → 골렘에게 다가가 조사”.

- [ ] **Step 7: Add full Chapter 2 Chromium/WebKit completion**

The browser test completes Chapter 1, selects Chapter 2, crafts the wrench, repairs both generators, solves power bridge and all boss phases, reloads once mid-chapter, then clears Chapter 2.

- [ ] **Step 8: Commit**

```bash
git add Web/Instant
git commit -m "feat: add chapter two lava foundry"
```

---

### Task 5: Chapter 3 — 하늘 창고

**Files:**
- Modify: `Web/Instant/chapter-data.js`
- Modify: `Web/Instant/chapter-runtime.js`
- Modify: `Web/Instant/world-renderer.js`
- Modify: `Web/Instant/core.js`
- Modify: `Web/Instant/browser.test.cjs`

**Interfaces:**
- Produces: `MMRuntime.sokoban(state,puzzleId,move)`
- Produces: `MMRuntime.memoryPath(state,puzzleId,input)`

- [ ] **Step 1: Add failing rule tests**

Test solvable small Sokoban layouts, reset without inventory loss, seeded memory paths, and missing-number/rule questions.

- [ ] **Step 2: Verify RED**

Run core tests; expected missing puzzle runtime failures.

- [ ] **Step 3: Implement Chapter 3 definition**

Theme: sky warehouse; strong vertical routing; Jump Booster required for the main route but standard jump remains enough for tutorial approach areas.

- [ ] **Step 4: Implement Sokoban Lite**

Only 1–2 crates and compact boards. Detect unsolvable stuck corners and expose “퍼즐 다시 시작” without punishment.

- [ ] **Step 5: Implement memory path**

Show 3 tiles, hide them, then require the same order. First failure replays automatically; second failure enables Momo hint highlighting the first tile.

- [ ] **Step 6: Implement Warehouse Manager boss**

Phases: crate-position puzzle → memory path → missing-number question → inspect boss.

- [ ] **Step 7: Add mobile completion test + reload**

Test Chapter 3 clear in Chromium/WebKit and recovery after falling.

- [ ] **Step 8: Commit**

```bash
git add Web/Instant
git commit -m "feat: add chapter three sky warehouse"
```

---

### Task 6: Chapter 4 — 수정 회로 연구소

**Files:**
- Modify: `Web/Instant/chapter-data.js`
- Modify: `Web/Instant/chapter-runtime.js`
- Modify: `Web/Instant/world-renderer.js`
- Modify: `Web/Instant/core.js`
- Modify: `Web/Instant/browser.test.cjs`

**Interfaces:**
- Produces: `MMRuntime.ruleMachine(state,puzzleId,choice)`
- Produces: `MMRuntime.circuitSwitch(state,puzzleId,switchId)`
- Extends existing laser runtime for 3–4 mirrors.

- [ ] **Step 1: Add failing reasoning/puzzle tests**

Rule-machine cases use concrete attributes such as shape/number/color with one deterministic valid answer. Multi-mirror laser tests assert source-to-target connectivity.

- [ ] **Step 2: Verify RED**

Run core tests.

- [ ] **Step 3: Implement Chapter 4 definition**

Sensor reveals clue locations and route markers, never the answer itself. Energy Shield absorbs one hazard mistake.

- [ ] **Step 4: Implement rule machine and circuit switches**

Wrong choice resets only the local puzzle and never charges materials/XP.

- [ ] **Step 5: Implement Crystal Core Guardian**

Phases: rule machine → multi-laser → conditional switch selection. Every phase updates shield count + next exact action.

- [ ] **Step 6: Add full mobile completion tests**

Include one intentional wrong answer and one shield absorption to prove no punitive progression loss.

- [ ] **Step 7: Commit**

```bash
git add Web/Instant
git commit -m "feat: add chapter four crystal circuit lab"
```

---

### Task 7: Chapter 5 — 별빛 코어 타워 and Final Boss

**Files:**
- Modify: `Web/Instant/chapter-data.js`
- Modify: `Web/Instant/chapter-runtime.js`
- Modify: `Web/Instant/world-renderer.js`
- Modify: `Web/Instant/core.js`
- Modify: `Web/Instant/game.js`
- Modify: `Web/Instant/browser.test.cjs`

**Interfaces:**
- Consumes all shared puzzle runtimes from Tasks 4–6.
- Final boss phases: number engine, equipment gate, spatial puzzle, memory puzzle, final core inspect.

- [ ] **Step 1: Add failing all-tool and campaign-finish tests**

Require each of the five existing tools to have one meaningful gate/effect and assert campaign clear only after all five boss phases.

- [ ] **Step 2: Verify RED**

Run core tests.

- [ ] **Step 3: Implement Chapter 5 definition**

Mix visual motifs from Chapters 1–4. Seed problem instances and selected optional puzzle layouts, but never randomize required route topology into an unsolvable state.

- [ ] **Step 4: Implement final boss objective chain**

Example live HUD:

```text
별빛 코어 골렘 · 보호막 2/5
다음: 오른쪽 전력 장치에서 파워 렌치를 사용하세요.
```

Each phase completion emits a 3–4 second toast and immediately updates the persistent top objective.

- [ ] **Step 5: Implement campaign ending**

Clear screen shows Chapters 1–5 completion, no score/rank, collected core count, and buttons for “챕터 다시 선택” and “남은 보물 찾기”.

- [ ] **Step 6: Add full campaign Playwright run**

A single Chromium run completes Chapters 1–5; WebKit runs each chapter independently from prepared valid campaign saves to control CI duration. Both must test at least one portrait and one landscape viewport.

- [ ] **Step 7: Commit**

```bash
git add Web/Instant
git commit -m "feat: add chapter five starlight core tower"
```

---

### Task 8: TutorialCue Coverage, Accessibility, Pages Deployment Gate

**Files:**
- Modify: `Web/Instant/chapter-data.js`
- Modify: `Web/Instant/game.js`
- Modify: `Web/Instant/browser.test.cjs`
- Modify: `.github/workflows/instant-play-tests.yml`
- Modify: `.github/workflows/pages.yml`
- Modify: `Web/Instant/README.md`

**Interfaces:**
- Produces: `tutorialCueFor(state, eventId)`
- Pages deploy consumes successful core + browser test artifacts.

- [ ] **Step 1: Add objective completeness test**

Iterate all declarative objective steps in Chapters 1–5 and assert non-empty action text, target location, and completion predicate.

- [ ] **Step 2: Add tutorial-once test**

For first wrench, Jump Booster, Sensor, Shield, Sokoban, memory path, rule machine, and final boss transition:

```js
assert.ok(tutorialCueFor(s,'first-wrench'));
ackTutorial(s,'first-wrench');
assert.equal(tutorialCueFor(s,'first-wrench'),null);
```

- [ ] **Step 3: Gate Pages on browser tests**

Change Pages workflow so its build job runs:

```yaml
- run: node --test Web/Instant/core.test.cjs
- run: npm install --no-save --ignore-scripts playwright@1.63.0
- run: npx playwright install --with-deps chromium webkit
- run: node Web/Instant/browser.test.cjs
```

Only upload/deploy Pages after both pass.

- [ ] **Step 4: Re-run full verification**

Run locally/CI:

```bash
node --test Web/Instant/core.test.cjs
node Web/Instant/browser.test.cjs
```

Expected: zero failures; screenshots created for supported mobile viewports.

- [ ] **Step 5: Verify Pages deployment**

After push, require GitHub Actions `deploy-pages` build and deploy jobs to conclude `success`. Verify `https://herothgf-cell.github.io/MazeMath/` serves the new Chapter Select title and Chapter 1 remains playable.

- [ ] **Step 6: Update README**

Document campaign progression, browser-save migration, chapter learning goals, supported mobile orientation, and the distinction from Unity WebGL.

- [ ] **Step 7: Commit**

```bash
git add Web/Instant .github/workflows Web/Instant/README.md
git commit -m "test: gate five chapter campaign deployment"
```

## Self-Review

- Spec coverage: Chapter data, objectives, puzzle definitions, boss definitions, TutorialCue, save migration, Chapters 2–5 themes/learning/puzzles/bosses, mobile testing, and no-punishment constraints are all mapped to Tasks 1–8.
- Placeholder scan: no TBD/TODO/“similar to” steps remain.
- Type consistency: campaign helpers are introduced in Task 1; shared runtime/renderer in Task 2; later tasks consume those interfaces.
- Review Focus coverage: v1 migration Task 1; chapter-boundary/reload Task 3 and 4; touch/modal input Task 2/8; small viewports Task 3/8; objective dead-end enumeration Task 8.
