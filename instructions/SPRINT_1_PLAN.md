# Sprint 1 Plan — KeyboardTrainer MVP Foundation

**Sprint Duration:** Week 1 (Jan 25 – Feb 1, 2026)  
**Team:** 2–3 developers (1 backend-focused, 1 frontend-focused, 1 shared/DevOps)  
**Team Capacity:** ~40 story points (assuming 2 devs @ 5d/week × 4 pts/day)  
**Scrum Master:** AI Agent (facilitate ceremonies, unblock impediments)  
**Product Owner:** AI Agent (prioritize scope, acceptance criteria)

---

## Sprint Goal

**"Enable users to view a lesson list and start a typing practice session with live metrics and German keyboard visualization."**

**Success Criteria:**
- ✓ Start Screen displays lesson list with selection
- ✓ Typing View shows lesson text with current character highlighted
- ✓ Live metrics (WPM, accuracy, time elapsed) update in real-time
- ✓ German QWERTZ keyboard visualization with next-key highlighting
- ✓ User can type French diacritics (basic input capture working)
- ✓ All code committed & running locally via Docker Compose
- ✓ No P0/P1 bugs blocking demo

---

## In-Scope User Stories (Sprint 1)

| Story | Title | Points | Owner | Status |
|-------|-------|--------|-------|--------|
| 1.1 | Display Start Screen with CTAs | 3 | FE | Ready |
| 1.2 | Persist & Fetch Lessons (DB + API) | 5 | BE | Ready |
| 1.3 | Seed Database with Initial Lessons | 2 | BE | Ready |
| 2.1 | Render Typing View with Lesson Text | 5 | FE | Ready |
| 2.2 | Capture Typing Input with Dead Keys | 5 | FE | Ready |
| 2.3 | Display Live Metrics (WPM, Accuracy) | 4 | FE | Ready |
| 2.4 | Render German QWERTZ Keyboard | 6 | FE | Ready |
| 2.5 | Add Session Controls (Restart, Pause) | 2 | FE | Ready |
| 2.6 | Display Completion Summary | 3 | FE | Ready |
| 5.6 | Create Docker Compose Setup | 4 | DevOps | Ready |

**Total Sprint Capacity: 39 points** ✓ Fits within ~40 pt team capacity

---

## Out-of-Scope (Defer to Sprint 2+)

- Story 3.x (Lesson Editor) — Defer to Sprint 2
- Story 4.x (Session Persistence) — Defer to Sprint 2
- Story 5.1–5.5 (Accessibility, i18n, tests) — Polish in Sprint 2
- Story 5.7–5.8 (Production Dockerfiles, deployment) — Sprint 3

---

## Definition of Done (Sprint Level)

Each story is DONE when:
- [ ] Code passes peer review (1 approval)
- [ ] Unit tests pass (target 80%+ coverage on logic)
- [ ] Feature tested manually against acceptance criteria
- [ ] No P0/P1 bugs (P2 bugs allowed if documented in backlog)
- [ ] Code committed to `main` branch
- [ ] Docker Compose environment runs without errors
- [ ] Feature demo'd in sprint review

---

## Sprint Backlog — Detailed Task Breakdown

### Story 1.1: Display Start Screen with CTAs (3 pts, FE Lead)

**Owner:** Frontend Developer  
**Time Estimate:** 3 days

**Tasks:**

1. **Task 1.1.1 — Set up Elmish project + routing** (1d)
   - [ ] Create SAFE project scaffold (or use existing template)
   - [ ] Add Feliz Router or Elmish.UrlParser dependency
   - [ ] Create Route type: `type Page = Start | Typing of Guid | Editor of EditorMode * Guid option`
   - [ ] Implement basic router + initial Model/Msg/Update
   - [ ] Verify routing works locally (navigate between pages via URL)
   - **Acceptance:** Router compiles; can navigate to `/`, `/typing/test-id`

2. **Task 1.1.2 — Create Start Screen component** (0.5d)
   - [ ] Build `Pages/Start.fs` component
   - [ ] Render: app title, short instructions, CTA buttons
   - [ ] Display empty lesson list (placeholder, no data yet)
   - [ ] Buttons: "Start typing" (disabled), "Create lesson" (navigate to editor), "Continue last session" (grayed)
   - [ ] Add basic CSS styling (grid layout, readable text)
   - **Acceptance:** Start page renders; buttons are interactive (navigate); layout is responsive

3. **Task 1.1.3 — Add lesson selection UI** (0.5d)
   - [ ] Create lesson list component with mock data (5 lessons)
   - [ ] Display: title, difficulty badge (A1/A2/B1/B2/C1), content type tag (words/sentences)
   - [ ] Selectable rows (hover highlight, click to select)
   - [ ] Difficulty filter dropdown (filter locally for now)
   - [ ] Content type filter toggle
   - **Acceptance:** Can select lesson; filters work; selection is highlighted

4. **Task 1.1.4 — Connect to API (stub/mock)** (0.5d)
   - [ ] Create `Services/Api.fs` with stub fetch function
   - [ ] Stub returns mock lesson list initially
   - [ ] In `Msg/Update`, fetch lessons on Start page load (Cmd)
   - [ ] Display fetched lessons in list (or fallback to mock)
   - **Acceptance:** Page loads; lists lessons from mock/stub API

**Definition of Done:**
- ✓ Start Screen renders with lesson list and CTAs
- ✓ Lesson list displays: title, difficulty, type
- ✓ Filters work (difficulty, content type)
- ✓ Buttons are interactive; "Start typing" button navigates to Typing View when lesson selected
- ✓ No console errors; responsive layout

**Blocker Risk:** None (independent task)  
**PR Check:** Code review for component structure, naming, Elmish patterns

---

### Story 1.2: Persist & Fetch Lessons (DB + API) (5 pts, BE Lead)

**Owner:** Backend Developer  
**Time Estimate:** 2–2.5 days

**Tasks:**

1. **Task 1.2.1 — Create database schema + migrations** (1d)
   - [ ] Write SQL migration script: `001_init_schema.sql` (create lessons, sessions tables)
   - [ ] Run migration locally (verify tables created)
   - [ ] Create migration runner in F# (e.g., simple startup check & execute SQL)
   - [ ] Add health check query
   - **Acceptance:** Database tables exist; schema matches TechSpec; migrations run on app startup

2. **Task 1.2.2 — Implement Lesson CRUD API endpoints** (1.5d)
   - [ ] Define `Dtos.fs` in Shared: `LessonDto`, `LessonCreateDto`, `ApiErrorResponse`
   - [ ] Define `Domain/Lesson.fs` server-side type: `type Lesson = { Id: Guid; ... }`
   - [ ] Create `Data/Db.fs` with SQL queries:
     - `getAllLessons(): Lesson list`
     - `getLessonById(id: Guid): Lesson option`
     - `createLesson(lesson: Lesson): Guid` (return inserted id)
     - `updateLesson(lesson: Lesson): bool` (return true if updated)
     - `deleteLesson(id: Guid): bool` (return true if deleted)
   - [ ] Create `Api/Lessons.fs` with Saturn routes:
     - `GET /api/lessons` → returns all lessons (200 OK)
     - `GET /api/lessons/{id}` → returns lesson or 404
     - `POST /api/lessons` → create new lesson (201 Created)
     - `PUT /api/lessons/{id}` → update lesson (200 OK or 404)
     - `DELETE /api/lessons/{id}` → delete lesson (204 No Content or 404)
   - [ ] All responses follow error schema: `{ "message": "...", "statusCode": 200, "errors": {} }`
   - **Acceptance:** All 5 endpoints return correct status codes & data; database state is correct after operations

3. **Task 1.2.3 — Implement error handling & validation** (0.5d)
   - [ ] Create `Api/ErrorHandler.fs` middleware
   - [ ] Catch exceptions, return 500 with error schema
   - [ ] Validate input on server-side (non-empty title, valid difficulty, etc.)
   - [ ] Return 400 Bad Request with validation errors for invalid input
   - **Acceptance:** Invalid requests return 400 with clear error messages; unhandled exceptions return 500

**Definition of Done:**
- ✓ All 5 endpoints working (tested manually via curl or Postman)
- ✓ Database schema created & migrations run on startup
- ✓ Error responses follow schema
- ✓ Server logs structured errors (e.g., Serilog)
- ✓ No SQL injection vulnerabilities (parameterized queries)

**Blocker Risk:** Database connection failure (mitigate: test connection in startup)  
**PR Check:** Code review for SQL correctness, error handling, Dapper usage

---

### Story 1.3: Seed Database with Initial Lessons (2 pts, BE Lead)

**Owner:** Backend Developer  
**Time Estimate:** 1 day

**Tasks:**

1. **Task 1.3.1 — Create seed data script** (1d)
   - [ ] Create `Data/Seeds.fs` with sample lessons (5–7 in French, A1–B1 levels)
   - [ ] Include French diacritics in content (é, è, à, ç, etc.)
   - [ ] Mix words and sentences lessons
   - [ ] Example lessons:
     - "Bonjour, ça va?" (words: greetings, A1)
     - "Je suis heureux." (sentence, A1)
     - "Au restaurant" (words: food/restaurant vocab, A2)
     - etc.
   - [ ] Seed function checks if lessons table is empty; if so, inserts seed data
   - [ ] Make seed idempotent (safe to re-run)
   - **Acceptance:** App starts; lessons table has 5+ sample lessons; seed is idempotent

2. **Task 1.3.2 — Integrate seed into startup** (0.5d)
   - [ ] Call seed function in `Program.fs` after migrations
   - [ ] Log seed results
   - [ ] Verify seed data is present via API: `GET /api/lessons`
   - **Acceptance:** Docker starts; API returns seeded lessons

**Definition of Done:**
- ✓ Database populated with 5+ French lessons
- ✓ Lessons include A1–B1 difficulties and both words/sentences types
- ✓ French diacritics render correctly
- ✓ Seed is idempotent (safe to re-run)
- ✓ Seed runs automatically on app startup

**Blocker Risk:** None (depends on Task 1.2)  
**PR Check:** Code review for seed data quality, French accuracy

---

### Story 2.1: Render Typing View with Lesson Text (5 pts, FE Lead)

**Owner:** Frontend Developer  
**Time Estimate:** 2–2.5 days

**Tasks:**

1. **Task 2.1.1 — Create Typing View component skeleton** (1d)
   - [ ] Create `Pages/Typing.fs` component
   - [ ] Fetch lesson from API via `GET /api/lessons/{id}` (passed in route params)
   - [ ] Add to Model: `typingState = { lessonId: Guid; lesson: Lesson; typedText: string; cursorPosition: int; ... }`
   - [ ] Render: lesson title, difficulty badge, start message
   - [ ] Add Msg types: `StartTyping`, `TypeKey`, `Backspace`, `TimerTick`, etc.
   - [ ] Add focus to text input (auto-focus on mount)
   - **Acceptance:** Route `/typing/{id}` loads; lesson data displays; input is focused

2. **Task 2.1.2 — Render lesson text with character highlighting** (1d)
   - [ ] Create `Components/LessonText.fs` component
   - [ ] Display full lesson text (French, multiline support)
   - [ ] Highlight current character at cursor position (yellow bg or similar)
   - [ ] Display typed characters with correct/incorrect styling:
     - Correct: green text or checkmark
     - Incorrect: red bg or red text
   - [ ] Support French diacritics (é, è, ê, ë, à, â, ç, ù, û, ü, etc.)
   - [ ] Support punctuation (apostrophe, « », …, commas, periods)
   - **Acceptance:** Lesson text renders correctly; character highlighting works as user types

3. **Task 2.1.3 — Connect typing input to state** (0.5d)
   - [ ] Capture keyboard input on text input element (onKeyDown or onInput)
   - [ ] On each keystroke: update `typedText`, increment `cursorPosition`
   - [ ] Check if typed character matches lesson text at current position
   - [ ] If match: mark as correct; if mismatch: mark as error
   - [ ] Store errors in state: `errors: Map<int, char>`
   - **Acceptance:** Typing updates state; correct/incorrect styling applies in real-time

**Definition of Done:**
- ✓ Typing View displays lesson text with correct/incorrect styling
- ✓ Current character is highlighted
- ✓ French diacritics and punctuation support working
- ✓ Typing updates state in real-time
- ✓ No console errors

**Blocker Risk:** Dead key input handling (defer advanced IME support to later; basic typing should work)  
**PR Check:** Code review for character matching logic, Fable React patterns

---

### Story 2.2: Capture Typing Input with Dead Keys (5 pts, FE Lead)

**Owner:** Frontend Developer  
**Time Estimate:** 2 days

**Tasks:**

1. **Task 2.2.1 — Implement robust input capture** (1d)
   - [ ] Use `onKeyDown` or `onInput` event (test both; choose best for diacritics)
   - [ ] Log keyboard events to console (for debugging)
   - [ ] Handle regular ASCII characters (a-z, A-Z, 0-9, punctuation)
   - [ ] Capture space, backspace, enter
   - [ ] Test with French keyboard layout (AZERTY) in browser dev tools
   - [ ] Document any browser limitations
   - **Acceptance:** Typing works for basic ASCII and common punctuation

2. **Task 2.2.2 — Handle dead keys and diacritics** (1d)
   - [ ] Test dead key sequences (e.g., `^` + `e` → `ê`) on browser's French input method
   - [ ] Verify diacritics from keyboard layout work (é via French layout, etc.)
   - [ ] Support copy-paste of French text (clipboard paste should work)
   - [ ] If IME not fully working in browser, document workaround (paste option)
   - [ ] Log which characters are received; no silent failures
   - **Acceptance:** French diacritics can be typed or pasted; no crashes on dead key input

3. **Task 2.2.3 — Handle backspace & special keys** (0.5d)
   - [ ] Backspace removes last character from `typedText`
   - [ ] Decrement `cursorPosition`
   - [ ] Clear error state for that position (if it was an error)
   - [ ] Arrow keys: no action (don't allow repositioning cursor mid-lesson)
   - **Acceptance:** Backspace works correctly; other keys don't cause unintended behavior

**Definition of Done:**
- ✓ Typing French diacritics (é, è, ê, ë, à, â, î, ï, ô, ù, û, ü, ç) works via keyboard or paste
- ✓ Backspace correctly removes characters
- ✓ No unintended behavior from modifier keys (Shift, Alt, Ctrl)
- ✓ Any browser limitations documented (e.g., "French IME on macOS Safari may require paste")
- ✓ No console errors or crashes

**Blocker Risk:** Dead key support may be browser-dependent; mitigate with paste fallback  
**PR Check:** Code review for input handling logic; manual testing with French keyboard layout

---

### Story 2.3: Display Live Metrics (WPM, Accuracy, Time) (4 pts, FE Lead)

**Owner:** Frontend Developer  
**Time Estimate:** 1.5 days

**Tasks:**

1. **Task 2.3.1 — Create metrics calculation logic** (1d)
   - [ ] Create `Services/Typing.fs` with calculation functions:
     - `calculateWPM (typedChars: int) (elapsedSeconds: float) : float`
     - `calculateAccuracy (totalTyped: int) (errors: int) : float`
     - `calculateCPM (typedChars: int) (elapsedSeconds: float) : float`
   - [ ] Add to Model: `startTime: DateTime option`, `elapsedSeconds: int`, `totalTyped: int`, `errorCount: int`
   - [ ] Msg: `Tick` (for timer), `TypingStarted` (sets startTime)
   - [ ] Update function: on first keystroke, set `startTime = now()`; on Tick, increment `elapsedSeconds`
   - [ ] Recalculate metrics on each keystroke
   - **Acceptance:** WPM, accuracy, CPM formulas are correct; metrics update in real-time

2. **Task 2.3.2 — Render metrics display component** (0.5d)
   - [ ] Create `Components/Metrics.fs` component
   - [ ] Display:
     - WPM or CPM (choose one for MVP; show both if space allows)
     - Accuracy % (0–100%)
     - Error count (int)
     - Time elapsed (MM:SS format)
   - [ ] Update every keystroke (or every timer tick)
   - [ ] Layout: fixed header or sidebar (easy to read without distracting)
   - [ ] Use monospace font for numbers (cleaner look)
   - **Acceptance:** Metrics display is visible, readable, and updates in real-time

3. **Task 2.3.3 — Implement timer loop** (0.5d)
   - [ ] Use `Browser.Window.setInterval` or Elmish `Sub` to emit `Tick` msg every 100ms
   - [ ] Format elapsed time as `MM:SS` (e.g., "00:45" for 45 seconds)
   - [ ] Timer starts on first keystroke (not on component mount)
   - [ ] Timer pauses when user pauses (implemented in Story 2.5)
   - **Acceptance:** Timer increments correctly; starts on first key; pauses work

**Definition of Done:**
- ✓ Metrics calculated correctly (WPM, CPM, accuracy, error count)
- ✓ All metrics display in real-time
- ✓ Timer starts on first keystroke
- ✓ Time format is readable (MM:SS)
- ✓ No console errors

**Blocker Risk:** Timer loop may cause performance issues if not implemented efficiently; test with 5+ min sessions  
**PR Check:** Code review for calculation accuracy; manual testing of metrics

---

### Story 2.4: Render German QWERTZ Keyboard (6 pts, FE Lead)

**Owner:** Frontend Developer  
**Time Estimate:** 2–2.5 days

**Tasks:**

1. **Task 2.4.1 — Define keyboard layout data structure** (0.5d)
   - [ ] Create `Services/Keyboard.fs` with German QWERTZ layout
   - [ ] Define `type KeyLayout = { Label: string; Code: string; Row: int; Col: int; Width: float; ... }`
   - [ ] Create `germanQwertzLayout : KeyLayout array` with all keys:
     - Row 0: digits (1–0, ß, ´)
     - Row 1: Q, W, E, R, T, Z, U, I, O, P, Ü
     - Row 2: A, S, D, F, G, H, J, K, L, Ö, Ä
     - Row 3: Y (at Z position), X, C, V, B, N, M, comma, period, dash
     - Row 4: Space bar
   - [ ] Map keyboard codes to layout (e.g., "KeyQ" → Q key)
   - [ ] Test layout visually (compare to standard German QWERTZ reference)
   - **Acceptance:** Layout data is complete and correct

2. **Task 2.4.2 — Render keyboard visualization** (1d)
   - [ ] Create `Components/Keyboard.fs` component
   - [ ] Render keys in grid layout (using CSS Grid or Flexbox)
   - [ ] Display key labels (Q, W, E, etc., and German keys: Ü, Ö, Ä, ß)
   - [ ] Style: dark background, light keys, readable font
   - [ ] Keyboard is responsive (scale on smaller screens)
   - [ ] Each key has a consistent size except space bar (wider)
   - **Acceptance:** Keyboard layout is visually correct; responsive

3. **Task 2.4.3 — Implement next-key highlighting** (1d)
   - [ ] Add to Model: `nextKeyCode: string`
   - [ ] In Update: on each keystroke, determine next expected key by looking at next character in lesson
   - [ ] Highlight next key in bright color (cyan or bright green)
   - [ ] Msg: `HighlightNextKey`, `ClearHighlight`
   - [ ] Test: type a few characters; verify correct key is highlighted
   - **Acceptance:** Next key is highlighted correctly as user types

4. **Task 2.4.4 — Add error highlighting & last-key feedback** (0.5d)
   - [ ] If wrong key pressed: show red highlight on that key + shake animation
   - [ ] Last pressed key: show subtle highlight (flash or outline)
   - [ ] Clear highlights on next keystroke
   - [ ] CSS: define shake animation (e.g., `@keyframes shake { ... }`)
   - **Acceptance:** Wrong key shows red; last key flashes; animations are smooth

**Definition of Done:**
- ✓ German QWERTZ keyboard layout rendered correctly
- ✓ Next key is highlighted in real-time as user types
- ✓ Wrong key shows error state (red, shake)
- ✓ Last key flashes
- ✓ Keyboard is responsive and readable
- ✓ No console errors

**Blocker Risk:** Keyboard layout mapping errors; mitigate with visual regression testing  
**PR Check:** Code review for layout correctness; manual testing with different lesson texts

---

### Story 2.5: Add Session Controls (Restart, Pause, Return) (2 pts, FE Lead)

**Owner:** Frontend Developer  
**Time Estimate:** 1 day

**Tasks:**

1. **Task 2.5.1 — Add Restart button** (0.3d)
   - [ ] Button: "Restart lesson"
   - [ ] On click: confirm dialog (if > 30 sec elapsed)
   - [ ] Reset state: `typedText = ""`, `cursorPosition = 0`, `startTime = None`, `errors = Map.empty`
   - [ ] Keep same lesson (don't navigate away)
   - **Acceptance:** Restart clears all typed text; timer resets

2. **Task 2.5.2 — Add Pause/Resume button** (0.4d)
   - [ ] Button: "Pause" (changes to "Resume" when paused)
   - [ ] On pause: stop timer, disable input, show pause overlay
   - [ ] Overlay text: "Paused" + current metrics snapshot
   - [ ] On resume: restart timer, enable input, hide overlay
   - [ ] Msg: `TogglePause`
   - **Acceptance:** Pause stops timer and input; resume restarts both

3. **Task 2.5.3 — Add Return to Start button** (0.3d)
   - [ ] Button: "Return to start screen"
   - [ ] On click: no confirmation (or confirm if > 30 sec)
   - [ ] Navigate to Start page (route back to `/`)
   - [ ] Don't save progress (Session 2+ feature)
   - **Acceptance:** Clicking returns to Start Screen without saving

**Definition of Done:**
- ✓ Restart button works; clears state; confirmation dialog works
- ✓ Pause/Resume buttons work; timer stops/resumes
- ✓ Return button navigates back; no progress saved
- ✓ Buttons are easy to find and click

**Blocker Risk:** None (simple state management)  
**PR Check:** Code review for state reset logic; manual testing of all buttons

---

### Story 2.6: Display Completion Summary (3 pts, FE Lead)

**Owner:** Frontend Developer  
**Time Estimate:** 1 day

**Tasks:**

1. **Task 2.6.1 — Detect lesson completion** (0.3d)
   - [ ] In Update: check if `typedText.Length = lesson.Content.Length && allCharactersCorrect`
   - [ ] If true: emit `CompleteLesson` msg
   - [ ] Set `sessionCompleted = true` in model
   - **Acceptance:** Completion is detected when all text is typed correctly

2. **Task 2.6.2 — Render completion summary panel** (0.7d)
   - [ ] Create summary component showing:
     - "Lesson Complete!" heading
     - Total time elapsed (formatted)
     - Final accuracy % (as text + progress bar)
     - Final WPM or CPM
     - Error count + error hotspots (top 3 keys with most errors)
   - [ ] Modal overlay (centered on screen, no other interaction)
   - [ ] Use same metrics formatting as live display
   - **Acceptance:** Summary panel displays all metrics clearly

3. **Task 2.6.3 — Add completion buttons** (0.3d)
   - [ ] Buttons in summary:
     - "Try again" → restart lesson (call Restart msg)
     - "Next lesson" → load next difficulty lesson (navigate to typing view with next lesson; if no next lesson, go to start)
     - "Back to start" → navigate to start screen
   - [ ] Buttons are clearly visible and easy to click
   - **Acceptance:** All buttons work; navigate correctly

4. **Task 2.6.4 — Prepare for session persistence** (0.3d)
   - [ ] Extract session data into structured format: `{ lessonId, startTime, endTime, wpm, accuracy, errorCount, errorHotspots }`
   - [ ] (Actual POST to API deferred to Sprint 2; just prepare structure here)
   - **Acceptance:** Session data structure is correct; ready for API call in future sprint

**Definition of Done:**
- ✓ Completion is detected when all text typed correctly
- ✓ Summary panel displays all metrics and error hotspots
- ✓ Buttons (Try again, Next lesson, Back) work and navigate correctly
- ✓ Modal styling is polished and readable
- ✓ No console errors

**Blocker Risk:** None (depends on earlier metrics tasks)  
**PR Check:** Code review for completion logic; manual testing of all buttons

---

### Story 5.6: Create Docker Compose Setup (4 pts, DevOps)

**Owner:** DevOps / Backend Developer  
**Time Estimate:** 1.5 days

**Tasks:**

1. **Task 5.6.1 — Create docker-compose.yml** (1d)
   - [ ] Define 3 services: `db` (Postgres), `server` (Saturn), `client` (Vite)
   - [ ] `db`: postgres:15-alpine, volume for data persistence, healthcheck
   - [ ] `server`: build from Dockerfile.server, depends_on db (healthcheck condition)
   - [ ] `client`: build from Dockerfile.client, dev mode (npm run dev)
   - [ ] Environment variables: DATABASE_URL, ASPNETCORE_ENVIRONMENT, etc.
   - [ ] Port mappings: db:5434, server:5000, client:3000
   - [ ] Network: single default network
   - [ ] Volume: postgres_data volume for persistence
   - **Acceptance:** docker-compose up -d starts all services; no errors

2. **Task 5.6.2 — Create Dockerfile.server** (0.3d)
   - [ ] Multi-stage build: SDK stage for compilation, runtime stage for execution
   - [ ] Build: dotnet restore, dotnet publish -c Release
   - [ ] Runtime: aspnet runtime image, expose port 5000
   - [ ] Entrypoint: dotnet Server.dll
   - [ ] Optional: healthcheck
   - **Acceptance:** Docker builds successfully; server runs and responds to requests

3. **Task 5.6.3 — Create Dockerfile.client** (0.2d)
   - [ ] Multi-stage build: Node stage for build, nginx stage for serving
   - [ ] Build: npm ci, npm run build (Vite build)
   - [ ] Runtime: nginx:alpine, copy dist to /usr/share/nginx/html
   - [ ] nginx.conf: SPA routing (/* → index.html)
   - [ ] Expose port 3000
   - **Acceptance:** Docker builds; client serves on port 3000

   Wait, for sprint 1 we want hot reload in dev. So maybe don't optimize yet. Let me adjust:
   - For MVP dev: `npm run dev` (Vite server in container with volume mount)
   - Production Dockerfile deferred to Sprint 3

4. **Task 5.6.4 — Document local dev workflow** (0.2d)
   - [ ] Create `DEVELOPMENT.md` with:
     - Prerequisites (Docker, .NET SDK, Node.js)
     - Startup: `docker-compose up -d`
     - Verify: `docker-compose logs` shows all services healthy
     - Access: http://localhost:3000 (client), http://localhost:5000/api (server)
     - Logs: `docker-compose logs -f server`
     - Teardown: `docker-compose down`
     - Troubleshooting (port conflicts, DB connection errors, etc.)
   - [ ] Include hotreload notes (auto-rebuild on code change in watch mode)
   - **Acceptance:** New developer can follow steps and get app running in < 5 min

**Definition of Done:**
- ✓ docker-compose.yml runs all 3 services
- ✓ Services are healthy (healthchecks pass)
- ✓ Client accessible at http://localhost:3000
- ✓ Server API accessible at http://localhost:5000/api/lessons
- ✓ Database persists across restarts (volume mount works)
- ✓ Hotreload works for both server and client
- ✓ Documentation is clear and complete

**Blocker Risk:** Port conflicts if ports already in use; document workaround (change docker-compose.yml ports)  
**PR Check:** Code review for Dockerfile correctness; manual testing of startup/shutdown

---

## Sprint Schedule

### Daily Standup (15 min)
**Time:** 9:00 AM each day  
**Format:**
1. What did I accomplish yesterday?
2. What am I working on today?
3. Any blockers?

### Sprint Timeline

| Day | Focus | Notes |
|-----|-------|-------|
| **Mon, Jan 27** | Kick-off meeting + project setup | Clarify scope, assign tasks, verify local setup (SAFE template, Docker) |
| **Tue–Wed, Jan 28–29** | Backend foundation (Stories 1.2, 1.3) | DB schema, API endpoints, seed data |
| **Tue–Wed, Jan 28–29** | Frontend foundation (Story 1.1, start 2.1) | Routing, Start Screen, Typing View skeleton |
| **Thu–Fri, Jan 30–31** | Typing logic (Stories 2.2, 2.3) | Input capture, metrics, timer |
| **Mon, Feb 3** | Keyboard viz (Story 2.4) | Layout data, rendering, highlighting |
| **Tue, Feb 4** | Session controls + completion (Stories 2.5, 2.6) | Buttons, summary panel |
| **Wed, Feb 5** | Docker setup (Story 5.6) | Compose file, Dockerfiles, documentation |
| **Thu–Fri, Feb 6–7** | Testing + bug fixes | Manual QA, fix P0/P1 bugs, prepare demo |
| **Fri, Feb 7** | Sprint Review + Retrospective | Demo app, gather feedback |

### Sprint Review (4 hours on Feb 7)
- Demo working app: Start Screen → select lesson → type → see metrics → complete
- Show German keyboard visualization
- Stakeholder Q&A
- Collect feedback for Sprint 2 refinement

### Sprint Retrospective (1.5 hours on Feb 7)
- Start/Stop/Continue format
- Team velocity: did we hit 39 points? If not, identify blockers
- Process improvements for Sprint 2

---

## Risk Register & Mitigation

| Risk | Severity | Mitigation | Owner |
|------|----------|-----------|-------|
| Dead key input not working in browser | High | Test early (Day 1); document limitations; provide paste workaround | FE Dev |
| Database connection failures in Docker | Medium | Implement healthchecks; clear error logs; test locally first | BE Dev |
| Keyboard layout mapping errors | Medium | Create unit tests for layout; visual comparison to German QWERTZ reference | FE Dev |
| Scope creep (features beyond stories) | High | Strict backlog discipline; PO blocks out-of-scope requests; document for backlog | Scrum Master |
| Team velocity lower than estimated | Medium | Adjust story points based on Day 1–2 actual progress; re-estimate remaining stories | Scrum Master |
| Docker image build failures | Low | Pre-build images locally; test Dockerfiles on Mac/Windows/Linux | DevOps |

---

## Definition of Ready (Pre-Sprint Checklist)

Before starting Sprint 1:
- [ ] All stories have acceptance criteria
- [ ] Tasks are broken down into 1–2 day chunks
- [ ] Team has estimated story points
- [ ] Blockers and dependencies identified
- [ ] SAFE template scaffolding available (or documented steps to create)
- [ ] Docker Compose template available (or documented steps)
- [ ] Team has access to necessary tools (Git, IDE, Docker, .NET SDK)
- [ ] Standup time confirmed with all team members
- [ ] Product Owner and Scrum Master roles assigned

**✓ All items checked; Sprint 1 is Ready to Start**

---

## Burndown & Tracking

**Ideal Sprint Burndown:**
- Sprint capacity: 39 points
- 10 working days (excluding weekends)
- Ideal burn: 3.9 points/day

**Tracking Method:**
- Daily standup: team updates task status (not started / in progress / done)
- Task board: Trello, GitHub Projects, or Azure DevOps
- End-of-day: team updates burndown chart
- Goal: stay close to ideal burndown line

**Weekly Check-in (Wed, Feb 5):**
- Assess progress mid-sprint
- If behind: reduce Sprint 2 scope; identify blockers
- If ahead: pull in stretch goals (e.g., start Story 3.1 Lesson Editor)

---

## Stretch Goals (if team is ahead of schedule)

If team completes core stories before end of sprint:

1. **Story 3.1: Lesson Editor form** (partial)
   - Create form UI (not save/delete yet)
   - Validation logic

2. **Story 5.1: Keyboard-only accessibility**
   - Ensure all buttons are keyboard-navigable
   - Add tabindex, focus indicators

3. **Unit tests for typing logic**
   - Test WPM/accuracy calculations
   - Test input matching logic

---

## Dependencies & Blockers Tracking

**External Blockers (external to team):**
- None identified for Sprint 1

**Internal Blockers (team dependencies):**
- Story 1.2 (API) must complete before Story 1.1 can integrate API (currently using mock)
- Story 2.1–2.6 depend on Story 1.2 being complete (need lessons from API)
- Story 5.6 (Docker) should be in parallel to avoid blocking frontend/backend progress

**Mitigation:**
- Pair programming for critical path items (1.2 → 1.1 integration)
- Feature branches for parallel work; merge when dependencies ready

---

## Sprint Success Criteria

**Minimum Success (MVP Shipped):**
- ✓ Start Screen displays lessons (from API)
- ✓ Typing View works: user can type, see metrics, keyboard highlights next key
- ✓ All code runs in Docker Compose
- ✓ No P0 bugs
- ✓ Demo-able to stakeholders

**Target Success:**
- All of above +
- ✓ All 10 stories completed
- ✓ Accuracy of metrics verified (manual tests)
- ✓ French diacritics fully working (no workarounds needed)
- ✓ Comprehensive README/documentation
- ✓ Positive team retrospective (no major blockers)

**Stretch Success:**
- All of above +
- ✓ Story 3.1 (Lesson Editor) partially done
- ✓ Unit tests for core logic (80%+ coverage)
- ✓ Accessibility audit (keyboard-only navigation verified)

---

## Communication & Reporting

**Daily:**
- Standup: 9:00 AM (15 min)
- Async update: Slack channel with task progress

**Weekly:**
- Burndown chart update (every Wed/Fri)
- Blockers escalation: if any blocker > 24h, escalate to PO

**End of Sprint:**
- Sprint Review: Feb 7 (10 AM, 2 hours)
- Sprint Retrospective: Feb 7 (12 PM, 1.5 hours)
- Updated backlog for Sprint 2: Feb 7 (after retro)

---

## Appendix: Task Estimation Rationale

**Story Point Scale:** 1 (trivial) → 2 (simple) → 3 (straightforward) → 5 (complex) → 8 (very complex)

| Story | Points | Rationale |
|-------|--------|-----------|
| 1.1 | 3 | Routing + UI components; straightforward |
| 1.2 | 5 | API design + DB integration; complex |
| 1.3 | 2 | Script + seed data; simple |
| 2.1 | 5 | Component hierarchy, styling, integration; complex |
| 2.2 | 5 | Input handling, diacritics support, edge cases; complex |
| 2.3 | 4 | Calculations + timer logic; moderately complex |
| 2.4 | 6 | Layout mapping + rendering + highlighting; most complex |
| 2.5 | 2 | Simple state management; straightforward |
| 2.6 | 3 | Modal component + logic; straightforward |
| 5.6 | 4 | Multi-service setup, Dockerfiles; moderately complex |

**Total: 39 points** ✓

---

**Sprint 1 Plan Created:** January 25, 2026  
**Version:** 1.0  
**Status:** Ready to Execute (☐ Kick-off meeting scheduled)

