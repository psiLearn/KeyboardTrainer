# KeyboardTrainer Product Backlog

**Project:** KeyboardTrainer (F# SAFE Stack typing learning app)  
**Tech Stack:** Saturn/Fable/Elmish + PostgreSQL + Docker Compose  
**Target Users:** Language learners (French) with touch typing practice focus  
**Difficulty Scale:** A1–C1  
**UI Language:** EN (with future DE support)  
**Last Updated:** January 25, 2026

---

## Epic Structure

This backlog is organized into **5 epics**, each representing a major capability. Each epic contains prioritized user stories with acceptance criteria (Gherkin-style).

---

## Epic 1: MVP Foundation — Start Screen & Lesson Display

**Goal:** Deliver a functional start screen with lesson selection and lesson list persistence.

### Story 1.1: Display Start Screen with App Title and CTAs
**Business Value:** 9 | **Risk:** 2 | **Effort:** 3pts

**As a** user  
**I want** to see the app title and main action buttons on startup  
**So that** I can navigate to start typing or manage lessons

**Acceptance Criteria:**
- [ ] Start Screen displays: app title "KeyboardTrainer", short instructions
- [ ] CTAs present: "Start typing", "Create lesson", "Continue last session" (grayed if no session exists)
- [ ] Lesson list selector shows all available lessons with title, difficulty, word/sentence tag
- [ ] Difficulty filter (A1/A2/B1/B2/C1) and content type filter (words/sentences) are available
- [ ] Lesson list is fetched from `/api/lessons` on component mount
- [ ] No lesson selected → "Start typing" button is disabled; selecting a lesson enables it

**Technical Notes:**
- Route: `/`
- Fetch `GET /api/lessons` on mount
- Use Elmish Cmd.batch for multiple async requests

---

### Story 1.2: Persist and Fetch Lessons from PostgreSQL
**Business Value:** 9 | **Risk:** 4 | **Effort:** 5pts

**As a** backend engineer  
**I want** to store lessons in PostgreSQL and expose CRUD endpoints  
**So that** lessons are persisted across sessions

**Acceptance Criteria:**
- [ ] Database schema: `lessons` table (id UUID, title string, difficulty enum, content_type enum, language fixed "FR", content text, created_at timestamp, updated_at timestamp)
- [ ] Endpoints implemented:
  - `GET /api/lessons` → returns list of all lessons (status 200)
  - `GET /api/lessons/{id}` → returns single lesson or 404
  - `POST /api/lessons` → accepts LessonDto (validation enforced server-side), returns 201 + location
  - `PUT /api/lessons/{id}` → updates lesson, returns 200 or 404
  - `DELETE /api/lessons/{id}` → deletes lesson, returns 204 or 404
- [ ] Error responses use consistent schema: `{ "message": "...", "statusCode": 400, "errors": {...} }`
- [ ] Database migrations run on app startup (or documented manual step)
- [ ] Seed data: 5 French lessons (A1–B1 difficulty) in DB
- [ ] No N+1 queries; index on `difficulty` and `language`

**Technical Notes:**
- Use Saturn routes + Giraffe handlers
- DTOs in `Shared` module
- Strongly typed DB access (e.g., Dapper, SqlHydra, or Spectre.Cli migrations)
- Return typed `Result<'a, ApiError>` from handlers

---

### Story 1.3: Seed Database with Initial French Lessons
**Business Value:** 5 | **Risk:** 1 | **Effort:** 2pts

**As a** tester  
**I want** sample lessons pre-loaded in the database  
**So that** I can immediately test the typing feature

**Acceptance Criteria:**
- [ ] Seed script (SQL or F# code) creates 5–7 French lessons covering A1–B1 levels
- [ ] Sample lessons include:
  - Words lesson (e.g., "bonjour café table").
  - Sentence lesson (e.g., "Je suis heureux. Où est la gare?").
  - Include French diacritics in content (é, è, ê, à, ç)
- [ ] Seed runs on first app startup if DB is empty
- [ ] Seed is idempotent (safe to re-run)

**Technical Notes:**
- Use migration tool or F# script in startup
- Confirm seed data is not accidentally wiped on app restart

---

## Epic 2: MVP Core — Typing View & Keyboard Visualization

**Goal:** Implement the core typing experience with German keyboard visualization and real-time metrics.

### Story 2.1: Render Typing View with Lesson Text
**Business Value:** 10 | **Risk:** 3 | **Effort:** 5pts

**As a** user  
**I want** to see the lesson text displayed with the current character highlighted  
**So that** I can practice typing

**Acceptance Criteria:**
- [ ] Typing View displays:
  - Lesson title and difficulty badge
  - Full lesson text (French, multiline if needed)
  - Current character position highlighted in a distinct color (e.g., yellow bg)
  - Correctly typed characters styled (green text or checkmark)
  - Incorrectly typed characters styled (red bg or red text)
- [ ] Cursor position updates as user types
- [ ] French diacritics (é, è, ê, ë, à, â, î, ï, ô, ù, û, ü, ç) render correctly
- [ ] Punctuation (apostrophe, « », …, etc.) is supported
- [ ] Text wraps properly; no overflow
- [ ] Accessibility: ARIA labels on text container; screenreader announces current character

**Technical Notes:**
- Route: `/typing/:lessonId`
- Model tracks: `typedText: string`, `cursorPosition: int`, `lessonText: string`
- Msg: `TypeKey`, `Backspace`, `UpdateCursorPosition`
- View: render lesson text with inline styling or CSS classes
- Consider using a monospace font for clarity

---

### Story 2.2: Capture User Typing Input with Dead Key Handling
**Business Value:** 9 | **Risk:** 5 | **Effort:** 5pts

**As a** user  
**I want** to type French characters using my keyboard  
**So that** I can accurately type lesson content

**Acceptance Criteria:**
- [ ] Typing input is captured on `keydown` or `input` events
- [ ] Dead key sequences (e.g., `^` + `e` → `ê`) are handled correctly
- [ ] French diacritics can be typed via:
  - Browser input method (compose key, etc.)
  - Direct Unicode input (clipboard paste)
  - IME (input method editor) support noted and tested
- [ ] Backspace correctly removes last character
- [ ] Space, punctuation, and special keys are captured
- [ ] Browser limitations (if any) are documented
- [ ] No unintended behavior from modifier keys (Shift, Alt, Ctrl)
- [ ] Accessibility: keyboard-only input; no mouse required

**Technical Notes:**
- Use Fable `onKeyDown` / `onInput` events
- Consider `keyCode` vs `key` vs `code` for cross-browser compat
- Test with French keyboard layout on browser dev tools
- Document workarounds for IME edge cases

---

### Story 2.3: Display Live Typing Metrics (WPM, Accuracy, Time)
**Business Value:** 9 | **Risk:** 2 | **Effort:** 4pts

**As a** user  
**I want** to see real-time typing metrics  
**So that** I can track my progress

**Acceptance Criteria:**
- [ ] Metrics displayed live:
  - WPM (words per minute) or CPM (characters per minute): calculated from `typed characters / elapsed time`
  - Accuracy %: `(correct chars / total typed chars) * 100`
  - Error count: number of incorrect keystrokes
  - Time elapsed: MM:SS format
- [ ] Metrics update after every keystroke
- [ ] Timer starts on first character typed, not on component mount
- [ ] Accuracy is 100% until first error
- [ ] WPM/CPM assumes average word = 5 characters
- [ ] Layout: metrics displayed in a fixed header or sidebar, easy to read

**Technical Notes:**
- Model: `startTime: DateTime option`, `typedCount: int`, `errorCount: int`
- Msg: `Tick` (timer event every 100ms)
- Calculate WPM = `(typedCount / 5) / (elapsedSeconds / 60)`
- Use `Browser.Window.setInterval` or Elmish `Sub` for timer
- Format time as string in View

---

### Story 2.4: Render German QWERTZ Keyboard Visualization
**Business Value:** 9 | **Risk:** 4 | **Effort:** 6pts

**As a** user  
**I want** to see a visual German QWERTZ keyboard  
**So that** I can learn key positions

**Acceptance Criteria:**
- [ ] Keyboard visualization shows:
  - All keys with correct German labels (QWERTZ layout, ü, ö, ä, etc.)
  - Next required key is highlighted (e.g., bright green or cyan bg)
  - Last pressed key shows feedback (e.g., flash animation or color change)
  - Error state: wrong key pressed → red highlight + shake animation
  - Shift state indicator (e.g., "Shift held: next char requires Shift")
  - Optional: finger hints toggle (show which finger to use)
- [ ] Keyboard layout is data-driven (not hard-coded DOM):
  - Use a `keyLayout: KeyRow array` data structure
  - Each `KeyRow` contains `keys: Key array`
  - Each `Key` has `label: string`, `code: string`, `row: int`, `col: int`
- [ ] Keyboard is responsive (scales on smaller screens)
- [ ] Accessibility: ARIA labels on each key; screenreader announces highlighted key

**Technical Notes:**
- Model: `nextKeyCode: string`, `lastKeyCode: string`, `shiftActive: bool`, `showHints: bool`
- Msg: `HighlightKey`, `ClearKeyHighlight`, `ToggleHints`
- Data structure: German QWERTZ layout (see example in architecture deliverable)
- CSS: flexible layout (grid or flexbox)
- Animation: CSS transitions for highlight; JS for shake on error

---

### Story 2.5: Add Session Controls (Restart, Pause, Return)
**Business Value:** 7 | **Risk:** 1 | **Effort:** 2pts

**As a** user  
**I want** to control the typing session (restart, pause, return to start)  
**So that** I can manage my practice

**Acceptance Criteria:**
- [ ] Buttons present:
  - "Restart lesson": clears typed text, resets timer, keeps same lesson
  - "Pause/Resume": pauses timer; disables input; shows pause overlay
  - "Return to Start": navigates back to start screen; does not save progress
- [ ] Restart confirms if >30 seconds elapsed (prevent accidental loss)
- [ ] Pause overlay shows pause text and current metrics snapshot
- [ ] Keyboard capture is disabled while paused

**Technical Notes:**
- Msg: `Restart`, `TogglePause`, `ReturnToStart`
- Modal/overlay for pause state
- Navigation: use Elmish Router to return to start screen

---

### Story 2.6: Display Completion Summary with Results
**Business Value:** 8 | **Risk:** 2 | **Effort:** 3pts

**As a** user  
**I want** to see a summary of my performance when I finish a lesson  
**So that** I can review my results

**Acceptance Criteria:**
- [ ] Completion is triggered when `typedText == lessonText` (all characters correct)
- [ ] Summary panel displays:
  - Total time elapsed
  - Final accuracy %
  - Final WPM/CPM
  - Error count
  - Error hotspots: 3–5 keys with highest error rate (if applicable)
  - Timestamp of completion
- [ ] Buttons: "Try again" (restart), "Next lesson" (load next difficulty), "Back to start"
- [ ] Optional: Save results to sessions table (Story 3.4)

**Technical Notes:**
- Detect completion in `Update` function: `if typedText.Length = lessonText.Length && isAllCorrect`
- Msg: `LessonComplete`, `ViewSummary`
- Render modal with results
- Prepare data for POST to sessions endpoint

---

## Epic 3: Lesson Management — Lesson Editor & CRUD

**Goal:** Allow users to create, edit, and delete lessons with validation and preview.

### Story 3.1: Display Lesson Editor Form (Create & Edit)
**Business Value:** 7 | **Risk:** 2 | **Effort:** 3pts

**As a** user  
**I want** to create or edit a lesson  
**So that** I can build custom practice content

**Acceptance Criteria:**
- [ ] Lesson Editor form has fields:
  - Title (text input, required, max 100 chars)
  - Difficulty (dropdown: A1, A2, B1, B2, C1)
  - Content type (radio: "Words", "Sentences")
  - Language (fixed display: "French")
  - Text content (textarea, required, max 5000 chars, support multiline)
  - Tags (optional text field)
- [ ] Form mode: "Create" or "Edit" (prefill on edit)
- [ ] Submit button text: "Create lesson" or "Save changes"
- [ ] Cancel button returns to start screen
- [ ] Live char count on textarea
- [ ] Validation feedback on blur (e.g., red border if empty, message if too long)

**Technical Notes:**
- Route: `/lessons/new` (create), `/lessons/:id/edit` (edit)
- Model: `editorState: { title, difficulty, contentType, textContent, tags }`
- Msg: `UpdateTitle`, `UpdateDifficulty`, `UpdateTextContent`, `SubmitLesson`, `CancelEdit`
- On edit route: fetch lesson and prefill form (Story 3.2)

---

### Story 3.2: Fetch and Prefill Lesson Editor on Edit
**Business Value:** 6 | **Risk:** 1 | **Effort:** 1pt

**As a** user editing a lesson  
**I want** the form to populate with the existing lesson data  
**So that** I can make changes

**Acceptance Criteria:**
- [ ] Route `/lessons/:id/edit` fetches lesson via `GET /api/lessons/{id}`
- [ ] On load, form fields are populated with lesson data
- [ ] Submit saves changes via `PUT /api/lessons/{id}`
- [ ] Success: return to start screen; show toast "Lesson updated"
- [ ] Error: display validation errors from server

**Technical Notes:**
- Cmd.ofPromise or Fable.Remoting for fetch
- Handle 404 (lesson not found) gracefully

---

### Story 3.3: Validate Lesson Input Client-Side and Server-Side
**Business Value:** 8 | **Risk:** 3 | **Effort:** 3pts

**As a** user  
**I want** to receive clear validation errors  
**So that** I can fix issues and save my lesson

**Acceptance Criteria:**
- [ ] Client-side validation:
  - Title: required, 1–100 characters
  - Text content: required, 1–5000 characters, no control characters (except newline, tab)
  - Difficulty: must be one of A1/A2/B1/B2/C1
  - Content type: must be words or sentences
- [ ] Server-side validation (re-validation):
  - Same checks as client-side
  - Normalize line endings (CRLF → LF)
  - Sanitize/reject disallowed control chars
  - Return 400 with detailed error messages
- [ ] Error messages display under field (red text)
- [ ] User cannot submit form if validation fails (button disabled)

**Technical Notes:**
- Create validation module: `LessonValidation.validate : LessonDto -> Result<ValidatedLesson, ValidationErrors>`
- Shared validation logic in Shared module (used by server)
- Client-side checks on blur/change; server re-validates on POST/PUT

---

### Story 3.4: Implement Lesson Preview Mode
**Business Value:** 5 | **Risk:** 1 | **Effort:** 2pts

**As a** lesson creator  
**I want** to preview how my lesson will look in Typing View  
**So that** I can verify formatting and special characters render correctly

**Acceptance Criteria:**
- [ ] "Preview" button in editor opens a modal/overlay
- [ ] Preview shows:
  - Lesson text exactly as it will appear in Typing View
  - French diacritics, punctuation, multiline rendering
  - Same styling as Typing View (monospace font, correct char highlighting on hover or fixed position)
- [ ] Close preview button returns to editor
- [ ] Preview does not count as a practice session (not saved)

**Technical Notes:**
- Component: reuse Typing View text rendering logic
- Msg: `OpenPreview`, `ClosePreview`
- Modal overlay

---

### Story 3.5: Create Lesson via POST Endpoint
**Business Value:** 8 | **Risk:** 2 | **Effort:** 2pts

**As a** backend  
**I want** to accept POST requests to create new lessons  
**So that** clients can save lessons

**Acceptance Criteria:**
- [ ] `POST /api/lessons` accepts `LessonCreateDto` (title, difficulty, contentType, textContent, tags)
- [ ] Validate input (Story 3.3)
- [ ] Insert into `lessons` table with `id = UUID.NewGuid()`, `created_at = now()`, `updated_at = now()`
- [ ] Return 201 Created with `Location` header pointing to `GET /api/lessons/{id}`
- [ ] Return new lesson in response body (with id)
- [ ] On client: show toast "Lesson created" + navigate to start screen

**Technical Notes:**
- Saturn route: `post [ route "" (fun next ctx -> ...)`
- Return `Accepted.CREATED` or `StatusCode 201`

---

### Story 3.6: Delete Lesson via DELETE Endpoint
**Business Value:** 6 | **Risk:** 1 | **Effort:** 1pt

**As a** user  
**I want** to delete a lesson  
**So that** I can remove lessons I no longer need

**Acceptance Criteria:**
- [ ] Delete button on lesson item (in list or editor)
- [ ] Confirmation dialog before deleting
- [ ] `DELETE /api/lessons/{id}` removes lesson from DB
- [ ] Return 204 No Content on success
- [ ] Return 404 if lesson not found
- [ ] On client: remove from list + show toast "Lesson deleted"

**Technical Notes:**
- Msg: `ConfirmDelete`, `DeleteLesson`, `CancelDelete`
- Modal confirmation
- Cmd.ofPromise for DELETE request

---

## Epic 4: Persistence & Analytics — Session Tracking (Optional for MVP)

**Goal:** Optionally track typing sessions and performance over time.

### Story 4.1: Create Sessions Table and POST Endpoint
**Business Value:** 5 | **Risk:** 2 | **Effort:** 3pts

**As a** developer  
**I want** to store typing session results in the database  
**So that** users can view their progress history

**Acceptance Criteria:**
- [ ] Database schema: `sessions` table (id UUID, lesson_id UUID, started_at timestamp, ended_at timestamp, wpm numeric, cpm numeric, accuracy numeric, error_count int, per_key_errors JSONB, created_at timestamp)
- [ ] Foreign key: `lesson_id` references `lessons.id` with ON DELETE CASCADE
- [ ] `POST /api/sessions` accepts SessionCreateDto (lesson_id, started_at, ended_at, wpm, cpm, accuracy, error_count, per_key_errors)
- [ ] Validate that lesson_id exists
- [ ] Insert into sessions table
- [ ] Return 201 with session object

**Technical Notes:**
- Define `PerKeyErrorCount` type: `Map<string, int>` or `JsonObject`
- Serialize per_key_errors to JSONB
- Migration: create table + index on lesson_id

---

### Story 4.2: Persist Session Results After Lesson Completion
**Business Value:** 6 | **Risk:** 2 | **Effort:** 2pts

**As a** user  
**I want** my typing results to be saved automatically  
**So that** I can track progress over time

**Acceptance Criteria:**
- [ ] When lesson completes (all text typed correctly):
  - Capture session data (start time, end time, WPM, CPM, accuracy, error count, error hotspots)
  - POST to `POST /api/sessions` automatically
  - Show success toast or spinner while saving
- [ ] If save fails (network error, server error):
  - Show error toast but don't block user from viewing results
  - Retry option or graceful degradation
- [ ] Don't block completion summary display on save

**Technical Notes:**
- Msg: `CompleteLesson`, `SaveSessionResult`, `SessionSaved`
- Use `Cmd.batch` to send POST and continue UI flow
- Handle errors gracefully with `Result` type

---

### Story 4.3: Display "Continue Last Session" Button
**Business Value:** 4 | **Risk:** 1 | **Effort:** 2pts

**As a** user  
**I want** to resume my last typing session  
**So that** I can quickly return to practice

**Acceptance Criteria:**
- [ ] Start Screen queries most recent incomplete session (or last session overall)
- [ ] If session exists: "Continue last session" button is enabled
- [ ] Clicking it opens Typing View with the same lesson, resume from where I left off (or restart same lesson)
- [ ] If no session exists: "Continue last session" button is grayed out

**Technical Notes:**
- Query: `SELECT * FROM sessions ORDER BY started_at DESC LIMIT 1`
- Route to lesson with session context
- MVP: restarting same lesson is simpler than resuming mid-session

---

## Epic 5: Polish & Deployment — Accessibility, i18n, Testing, Docker

**Goal:** Ensure accessibility, internationalization, testing, and deployability.

### Story 5.1: Ensure Keyboard-Only Accessibility
**Business Value:** 7 | **Risk:** 2 | **Effort:** 3pts

**As a** keyboard-only user  
**I want** to navigate and use the app without a mouse  
**So that** I can practice typing hands-on-keyboard

**Acceptance Criteria:**
- [ ] All interactive elements are keyboard accessible:
  - Buttons, dropdowns, textareas respond to Tab and Enter/Space
  - Links navigate via Enter
  - Modals trap focus (Tab stays within modal)
- [ ] Keyboard shortcuts documented (e.g., Escape to close modal)
- [ ] No mouse-only interactions (e.g., hover-only elements have keyboard fallback)
- [ ] Tab order is logical (left to right, top to bottom)
- [ ] Focus indicators are visible (outline, highlight, etc.)
- [ ] Tested with screen reader (NVDA or JAWS) + keyboard only

**Technical Notes:**
- Use semantic HTML (button, a, input, textarea, form, etc.)
- Add `tabIndex`, `aria-label`, `aria-describedby` as needed
- CSS: visible `:focus` and `:focus-visible` styles
- Test with browser accessibility inspector

---

### Story 5.2: Add ARIA Labels and Keyboard Descriptions
**Business Value:** 6 | **Risk:** 1 | **Effort:** 2pts

**As a** screenreader user  
**I want** elements to have descriptive labels  
**So that** I understand the app structure

**Acceptance Criteria:**
- [ ] ARIA attributes:
  - Buttons have descriptive text or `aria-label`
  - Keyboard visualization has `aria-live` region for current key announcement
  - Lesson text container has `role="region"` and `aria-label="Lesson content"`
  - Metrics display has `aria-live="polite"` for timer updates
  - Form fields have `<label>` associated with input
  - Error messages have `aria-describedby` pointing to error text
- [ ] Screenreader announces:
  - Current character being typed
  - Metrics changes (WPM, accuracy)
  - Completion summary
- [ ] Tested with at least one screenreader (NVDA)

**Technical Notes:**
- Fable `aria-*` attributes (may need custom helpers)
- Use `aria-live="polite"` for non-critical updates
- Use `aria-live="assertive"` only for critical feedback (errors)

---

### Story 5.3: Support UI Language Switching (EN / DE)
**Business Value:** 4 | **Risk:** 2 | **Effort:** 4pts

**As a** user  
**I want** to switch the UI language between English and German  
**So that** I can use the app in my preferred language

**Acceptance Criteria:**
- [ ] Language toggle in app header (EN / DE)
- [ ] All UI text translates (buttons, labels, error messages, tooltips)
- [ ] Lesson content remains French (unaffected by UI language)
- [ ] Language preference persisted to `localStorage`
- [ ] On app reload, UI language restored from localStorage
- [ ] Default language: EN
- [ ] Translations are complete for at least EN and DE

**Technical Notes:**
- Create `i18n` module with translation dictionaries: `{ en: {...}, de: {...} }`
- Model: `uiLanguage: Language` (EN or DE)
- Msg: `SwitchLanguage`
- View functions: `t (key: string)` helper that looks up current language translation
- localStorage key: "keyboardtrainer_language"

---

### Story 5.4: Add Unit Tests for Text Processing & Typing Logic
**Business Value:** 7 | **Risk:** 1 | **Effort:** 3pts

**As a** developer  
**I want** to have automated tests for typing logic  
**So that** correctness is verified and regressions are caught

**Acceptance Criteria:**
- [ ] Test module (e.g., `TypingLogic.Tests.fs`) covers:
  - Calculating WPM from character count and time
  - Calculating accuracy % from correct/total typed chars
  - Detecting character match (with diacritics support)
  - Detecting lesson completion (all chars typed correctly)
  - Error counting logic
- [ ] Client-side: Fable test (e.g., Fable.QUnit or Expecto)
- [ ] Server-side: xUnit or NUnit tests for validation logic
- [ ] Test coverage: aim for ≥80% of critical paths
- [ ] Tests run in CI (GitHub Actions or similar)

**Technical Notes:**
- Create separate Test projects
- Use assertions: `Assert.Equal(expected, actual)`
- Property-based tests for WPM/accuracy formulas (if using property testing library)

---

### Story 5.5: Add Server API Tests for Lesson CRUD
**Business Value:** 6 | **Risk:** 2 | **Effort:** 3pts

**As a** developer  
**I want** to test the lesson API endpoints  
**So that** correctness and error handling are verified

**Acceptance Criteria:**
- [ ] Test cases for each endpoint:
  - `GET /api/lessons`: returns 200 + list, empty list if no lessons
  - `GET /api/lessons/{id}`: returns 200 + lesson, or 404 if not found
  - `POST /api/lessons`: valid input returns 201, invalid input returns 400 with errors
  - `PUT /api/lessons/{id}`: valid update returns 200, invalid returns 400, not found returns 404
  - `DELETE /api/lessons/{id}`: returns 204, not found returns 404
- [ ] Validation error messages are clear and match contract
- [ ] Database state is correct after each operation
- [ ] Tests use in-memory or test database (isolated from production)
- [ ] Tests are deterministic and repeatable

**Technical Notes:**
- Use xUnit + HttpClient or TestServer from WebApplicationFactory
- Or: use Docker test container for Postgres (testcontainers-dotnet)
- Create fixtures for test data setup/teardown

---

### Story 5.6: Create Docker Compose for Local Development
**Business Value:** 10 | **Risk:** 3 | **Effort:** 4pts

**As a** developer  
**I want** to spin up the entire app stack locally with one command  
**So that** I can develop and test without manual setup

**Acceptance Criteria:**
- [ ] `docker-compose.yml` defines three services:
  - `db` (PostgreSQL 15+)
  - `server` (SAFE backend on port 5000)
  - `client` (Vite dev server on port 3000 or similar)
- [ ] Volumes:
  - `postgres_data` persists database
  - Source code mounted as volume for hot reload (if applicable)
- [ ] Environment variables:
  - `POSTGRES_DB=keyboardtrainer`, `POSTGRES_USER=trainer`, `POSTGRES_PASSWORD=<env>`
  - Server: `DATABASE_URL=postgresql://trainer:pwd@db:5434/keyboardtrainer`
- [ ] Healthchecks:
  - Database has healthcheck (psql query)
  - Server waits for db (depends_on with condition)
- [ ] One-command startup: `docker-compose up -d`
- [ ] Logs visible: `docker-compose logs -f`
- [ ] Cleanup: `docker-compose down -v` removes containers and volumes
- [ ] Documentation: README with startup steps, port mappings, troubleshooting

**Technical Notes:**
- Dockerfile (or multi-stage) for server and client
- Use official postgres image
- Client: Node + npm/yarn + Vite dev server
- Server: .NET SDK for build, thin runtime for execution
- Consider: separate docker-compose.dev.yml vs prod

---

### Story 5.7: Create Dockerfiles for Server and Client
**Business Value:** 9 | **Risk:** 2 | **Effort:** 3pts

**As a** DevOps engineer  
**I want** Dockerfiles that build and run the app  
**So that** the app can be containerized and deployed

**Acceptance Criteria:**
- [ ] `Dockerfile.server`:
  - Multi-stage build: builder stage (dotnet SDK) + runtime stage (aspnet runtime)
  - Build: `dotnet build`, `dotnet publish` with Release config
  - Runtime: expose port 5000, set entrypoint
  - Production-minded: minimal runtime image, no dev tools in final image
- [ ] `Dockerfile.client`:
  - Multi-stage build: builder (Node) + runtime (nginx or similar)
  - Build: `npm install`, `npm run build` (Vite build)
  - Runtime: nginx serving dist folder on port 3000
  - Gzip compression enabled
- [ ] Both images use lean base images (alpine or distroless if suitable)
- [ ] Both honor build arguments for version pinning (if needed)
- [ ] Images build successfully and run without errors

**Technical Notes:**
- Server Dockerfile references SAFE template conventions
- Consider health checks in Dockerfile HEALTHCHECK instruction
- Client: nginx.conf for SPA routing (/* → index.html)

---

### Story 5.8: Create Production Deployment Guide
**Business Value:** 5 | **Risk:** 1 | **Effort:** 2pts

**As a** operator  
**I want** clear deployment instructions  
**So that** the app can be deployed to production

**Acceptance Criteria:**
- [ ] Documentation covers:
  - Deploying to Azure App Service (target platform)
  - Environment variables and secrets management (Azure Key Vault)
  - Database migration strategy in production
  - Health checks and monitoring
  - Rollback procedure
  - SSL/TLS configuration
- [ ] Optional: GitHub Actions CI/CD workflow for automated deploy
- [ ] Checklist for pre-deployment verification

**Technical Notes:**
- Azure App Service reference: deployment slots, container registry, environment variables
- Secrets: use Azure Key Vault, not .env files in prod
- Database: migration tool to run on container startup (or manual step documented)

---

## Backlog Prioritization & Roadmap

### Phase 1: MVP (Sprint 1–3, ~3 weeks)
**Goal:** Functional typing app with basic lesson management.

**Included Stories:**
- Epic 1: 1.1, 1.2, 1.3
- Epic 2: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6
- Epic 3: 3.1, 3.3, 3.5, 3.6
- Epic 5: 5.6 (Docker for dev), 5.4 (basic tests)

**MVP Deliverables:**
- Working start screen with lesson selection
- Functional typing view with metrics and German keyboard visualization
- Lesson creation/deletion
- Local Docker dev environment
- Basic unit tests

---

### Phase 2: Polish (Sprint 4–5, ~2 weeks)
**Goal:** Refine UX, add accessibility, persistence.

**Included Stories:**
- Epic 3: 3.2, 3.4
- Epic 4: 4.1, 4.2, 4.3 (session tracking)
- Epic 5: 5.1, 5.2, 5.3, 5.4, 5.5, 5.7

**Deliverables:**
- Accessibility (keyboard-only, ARIA, screenreader support)
- UI language switching (EN/DE)
- Session persistence and "continue last session"
- Production Dockerfiles
- Comprehensive test coverage

---

### Phase 3: Production Ready (Sprint 6, ~1 week)
**Goal:** Deploy to Azure App Service.

**Included Stories:**
- Epic 5: 5.8 (deployment guide)
- CI/CD automation
- Monitoring and logging setup

**Deliverables:**
- Deployment guide
- GitHub Actions workflow
- Production checklist

---

## Definition of Done (Story Level)

Each user story is considered "Done" when:
- [ ] Acceptance criteria are met (100% functional)
- [ ] Code is peer-reviewed (PR approved)
- [ ] Unit/integration tests pass (if applicable story)
- [ ] Manual testing completed (QA sign-off)
- [ ] Documentation updated (if applicable)
- [ ] No P0 or P1 bugs
- [ ] Accessibility requirements met (WCAG 2.1 AA where applicable)
- [ ] Performance acceptable (no degradation from baseline)

---

## Dependency Map

```
1.1 (Start Screen)
  ↓
1.2 (DB + API) ← 1.3 (Seed data)
  ↓
2.1 (Typing View) ← 2.2 (Input capture) ← 2.3 (Metrics) ← 2.4 (Keyboard viz)
  ↓
3.1 (Editor form) ← 3.3 (Validation) ← 3.5 (POST endpoint)
  ↓
2.6 (Completion) → 4.2 (Persist session results)
  ↓
5.1/5.2 (Accessibility) ← 5.3 (i18n)
  ↓
5.6 (Docker dev) → 5.7 (Dockerfiles) → 5.8 (Deployment)
```

---

## Notes for Product Owner / Scrum Master

1. **Estimation:** Stories are estimated in points (1/2/3/5/8). Adjust based on team velocity.
2. **Ready to Start:** Stories 1.1–1.3 and 2.1–2.4 are ready for development (clear acceptance criteria).
3. **Refinement Needed:** Stories 4.x and some 5.x may need sprint-level refinement before grooming into sprint.
4. **Cross-Functional:** Assign Stories to:
   - Backend devs: 1.2, 3.5, 3.6, 4.1, 4.2, 5.5, 5.7 (server)
   - Frontend devs: 2.1–2.6, 3.1–3.4, 5.1–5.3, 5.7 (client)
   - DevOps/QA: 5.6, 5.8 (infrastructure)
5. **Risk Areas:** Dead key handling (2.2), keyboard visualization layout (2.4), Docker multi-service setup (5.6).

---

**Backlog Version:** 1.0  
**Last Updated:** January 25, 2026  
**Framework:** Agile + SAFE Stack  
**Project:** KeyboardTrainer
