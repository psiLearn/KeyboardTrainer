# Game Exercise Implementation Plan

## Context Reviewed
- `instructions/games.md`
- `instructions/app.md`
- Existing code paths for exercises:
  - `GET /api/lessons/{id}/exercise`
  - `ContentType.Probability` in `src/Shared/Domain.fs`
  - Start screen lesson cards in `src/Client/Pages/StartScreen.fs`
  - Typing flow in `src/Client/Pages/TypingView.*`

## Goal
Implement a candy-crush-inspired typing game as a separate project, then link it from the main app as a lesson exercise type.

## Rule Assumptions (from `games.md`)
- Playfield has weighted random gems: 31% yellow, 27% red, 23% green, 19% blue.
- Playfield is 15 x 15.
- Player controls a white gem that starts in the middle cell.
- Left match keys: `a` blue, `s` green, `d` red, `f` yellow.
- Right match keys: `ö` blue, `l` green, `k` red, `j` yellow.
- Up match keys: `q` or `p` blue, `w` or `o` green, `e` or `i` red, `r` or `u` yellow.
- Down match keys: `y` or `-` blue, `x` or `.` green, `c` or `,` red, `v` or `m` yellow.
- Matching same color animates and collapses the connected neighboring same-color group, increases score with the selected formula, and spawns new gems from top.
- Point calculation is switchable between the current `2^(n-1)` formula and `n^2`.
- Correct movement swaps the player with the matched neighboring colored gem.
- Neighbor detection and scoring start from the moved colored gem at the player's previous position, not from the neighbor's original position.
- Row movement preserves gems by rotating columns; it does not delete a bottom gem and create a new top gem.
- Collapse refill moves existing non-collapsed gems down into gaps, then creates replacement gems at the top.
- Game settings allow showing letters inside gems, turning row movement on/off, choosing the point calculation, and changing the row movement interval in milliseconds.
- Game settings are collapsible to preserve playfield space.
- Game settings include a sound on/off toggle.
- These settings are also accepted as starting parameters in the lesson config JSON: `showLettersInGems` and `moveRows`.
- High scores are tracked per lesson in browser local storage and shown in a side table.
- Scoring has two layers:
  - level/page score resets for each lesson page and is used for `targetScore` completion.
  - game score accumulates across restarted levels in the same game session and is used for the high-score table.
- Seed multiple game lessons for guided practice:
  - still rows with letters
  - still rows without letters
  - slow, medium, and fast moving rows with letters
  - slow, medium, and fast moving rows without letters
- Restart can be triggered with `R` or the Space bar.

## Architecture Decision
- Keep game isolated in a new client project:
  - `src/Games/KeyboardTrainer.GemGame/KeyboardTrainer.GemGame.fsproj`
- Build output as static assets:
  - `src/Client/public/games/gem-game/index.html`
  - `src/Client/public/games/gem-game/game.js`
- Main app links to game by lesson `ContentType` and opens it inside an embedded host page (iframe), so game code remains independent.

## Step-by-Step Delivery

### Step 1: Define shared contracts for game exercises
Files:
- `src/Shared/Domain.fs`
- `src/Server/Database/Migrations/004_AddGemGameContentType.sql`

Tasks:
- Add `ContentType.GemGame`.
- Add optional game metadata DTO for exercise payload (seed, speed, board size, target score).
- Add Postgres enum migration for `content_type = 'gem_game'`.

Acceptance:
- Server starts and migrations run cleanly.
- Creating a lesson with `contentType = gem_game` succeeds.

### Step 2: Support game exercise resolution in API
Files:
- `src/Server/Handlers/LessonHandler.Exercise.fs`
- `src/Server/Database/LessonRepository.fs`
- `src/Client/ApiClient.Parsing.fs`

Tasks:
- Parse and serialize `gem_game` in repository and client parsing.
- Extend lesson exercise resolver so `GemGame` returns game config payload.

Acceptance:
- `GET /api/lessons/{id}/exercise` returns valid config for a gem-game lesson.

### Step 3: Scaffold separate game project
Files:
- `src/Games/KeyboardTrainer.GemGame/KeyboardTrainer.GemGame.fsproj`
- `src/Games/KeyboardTrainer.GemGame/Main.fs`
- `src/Games/KeyboardTrainer.GemGame/App.fs`
- `src/Games/KeyboardTrainer.GemGame/public/index.html`
- `package.json` (build script entry)
- `KeyboardTrainer.sln`

Tasks:
- Create standalone Fable mini-app with its own Elmish model/update/view.
- Add build script to emit `game.js` into `src/Client/public/games/gem-game/`.
- Add project to solution for discoverability.

Acceptance:
- Running the new build script generates game assets.
- Opening `/games/gem-game/index.html` shows a running game shell.

### Step 4: Implement game engine and controls
Files:
- `src/Games/KeyboardTrainer.GemGame/GameTypes.fs`
- `src/Games/KeyboardTrainer.GemGame/GameLogic.fs`
- `src/Games/KeyboardTrainer.GemGame/App.fs`

Tasks:
- Implement weighted board generation and tick loop.
- Implement left/right hit windows.
- Apply color-key mapping and scoring by connected collapse size.
- Collapse connected neighbouring same-color gems.
- Animate collapsing gems before replacing them.
- Score collapsed groups with the selected mode: `2^(n-1)` or `n^2`.
- Spawn replacement gems from top after collapse.
- Add settings for letter overlays, row movement on/off, and row movement interval.
- Add collapsible settings UI.
- Add generated Web Audio sounds for hit, miss, and finish states.
- Add sound on/off setting.
- Add side high-score table and persist completed scores locally.
- Separate level/page score from game score in the model and UI.
- Use level/page score for target completion and completion payload score.
- Use game score for high-score ranking, while showing the level/page score beside each high-score row.
- Preserve the selected point calculation on restart and allow `scoreMode` as a starting parameter.
- Read `showLettersInGems` and `moveRows` from the game config so lessons can define the initial setting state.
- Add Space bar as a restart shortcut in addition to `R`.
- Keep the current runtime settings when restarting: letter overlays, row movement on/off, row movement interval, sound setting, and settings expanded/collapsed state.

Acceptance:
- Key presses map to expected color channels.
- Correct hits increase score and refresh board consistently.
- Wrong hits produce penalty or miss feedback.
- Letter overlay, movement on/off, scoring mode, and movement interval settings update the running game.

### Step 5: Embed game in main app as an exercise
Files:
- `src/Client/App.Types.fs`
- `src/Client/App.fs`
- `src/Client/Pages/StartScreen.fs`
- `src/Client/public/style.css`

Tasks:
- Add `GameView` route/state in main app.
- Add launcher behavior: selecting a `GemGame` lesson opens game host view.
- Render game in iframe pointing to `/games/gem-game/index.html`.

Acceptance:
- Start screen can launch the game from lesson cards.
- Typing lessons still open the normal typing page unchanged.

### Step 6: Link score result back to exercise/session
Files:
- `src/Games/KeyboardTrainer.GemGame/App.fs`
- `src/Client/App.fs`
- `src/Server/Handlers/SessionHandler.fs` (if extending payload)
- `src/Shared/Domain.fs`

Tasks:
- Post message from iframe to host on game complete (`score`, `duration`, `misses`).
- Host converts result into session save call (reuse existing `/api/sessions` first).
- Optionally tag session mode as `gem_game`.

Acceptance:
- Completing a game creates a session entry.
- Metrics page shows the new result entry.

### Step 7: Seed and expose first game exercise lesson
Files:
- `src/Server/Database/Migrations/005_SeedGemGameLesson.sql`
- `src/Client/Pages/StartScreen.fs`

Tasks:
- Seed one `gem_game` lesson with config JSON in `content`.
- Seed additional `gem_game` lesson variants for stationary rows, slow rows, medium rows, fast rows, and letter visibility on/off.
- Add lesson-card label text: `Game Exercise`.

Acceptance:
- Fresh DB contains one playable game lesson.
- Card appears and launches correctly.

### Step 8: Tests and hardening
Files:
- `tests/KeyboardTrainer.Server.Tests/*`
- `tests/e2e/*`

Tasks:
- Add API tests for `gem_game` lesson read and exercise payload.
- Add e2e smoke test: launch game lesson and submit one result.
- Add keyboard mapping unit tests for game logic.

Acceptance:
- Test suite passes for new game flow.
- No regression in existing typing flow tests.

## Execution Order
1. Step 1
2. Step 2
3. Step 3
4. Step 4
5. Step 5
6. Step 6
7. Step 7
8. Step 8

## Risk Controls
- Keep game isolated in `src/Games` to avoid destabilizing typing view logic.
- Reuse existing lesson + exercise API shape before introducing new endpoints.
- Gate integration by `ContentType` so legacy lessons remain untouched.
- Add migration and parser updates first to avoid runtime enum mismatch failures.
