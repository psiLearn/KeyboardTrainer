# Sprint 2 Plan — Enhancement Phase (Feb 10 – Feb 21, 2026)

**Sprint Goal:** "Keyboard visualization + Session persistence + API test coverage"

**Duration:** 2 weeks (10 business days)  
**Team:** 3 developers (1 backend, 1 frontend, 1 devops/QA)  
**Total Story Points:** 19 points  
**Velocity Target:** 15–20 pts/sprint (sustainable pace)

---

## 📋 Sprint 2 Backlog

| Priority | Story | Title | Owner | Points | Status |
|----------|-------|-------|-------|--------|--------|
| 🔴 CRITICAL | 2.4 | Render German QWERTZ Keyboard Visualization | Frontend | 6 | Ready |
| 🟡 HIGH | 2.5 | Add Session Controls (Restart, Pause, Return) | Frontend | 2 | Ready |
| 🟡 HIGH | 2.6 | Display Completion Summary with Results | Frontend | 3 | Ready |
| 🟡 HIGH | 4.2 | Persist Session Results After Lesson Completion | Backend | 2 | Ready |
| 🟡 HIGH | 3.3 | Validate Lesson Input Client & Server | Backend | 3 | Ready |
| 🟢 MEDIUM | 5.5 | Add Server API Tests for Lesson CRUD | Backend | 3 | Ready |

**Sprint Total:** 19 points

---

## 🎯 Sprint Success Criteria (Definition of Done)

By **Friday, Feb 21, 2026 @ 5:00 PM**, Sprint 2 is complete when:

- ✅ **Story 2.4 (Keyboard):** Keyboard visualization renders correctly; next key highlights; error animations work
- ✅ **Story 2.5 (Controls):** Restart/pause/return buttons functional; confirmation dialogs work
- ✅ **Story 2.6 (Completion):** Summary screen displays all metrics; save button triggers API call
- ✅ **Story 4.2 (Session Save):** Sessions persist to DB; linked to lessons correctly
- ✅ **Story 3.3 (Validation):** Server-side validation comprehensive; error messages clear
- ✅ **Story 5.5 (Testing):** ≥10 API test cases; ≥80% handler coverage; all tests pass
- ✅ **Overall:** Zero P0 bugs; ≤1 P1 bug
- ✅ **Code Review:** All PRs reviewed + approved
- ✅ **Performance:** No performance regression from Sprint 1

---

## 📅 Weekly Schedule

### Week 1: Feb 10–14 (5 business days)

**Monday, Feb 10, 9:00 AM – Sprint Kick-Off**
- Review Sprint 2 goals
- Assign developers to stories
- Tech lead walks through Story 2.4 (keyboard design)
- Questions & blockers clarified
- Target: developers ready to start by 10 AM

**Tuesday–Thursday, Feb 11–13**
- **Backend Dev:**
  - **Tue:** Task 4.2.1 (session POST endpoint)
  - **Wed:** Task 3.3.1–3.3.2 (validation logic)
  - **Thu:** Begin Task 5.5.1 (test scaffolding)
- **Frontend Dev:**
  - **Tue–Wed:** Task 2.4.1–2.4.2 (keyboard data + render)
  - **Thu:** Task 2.4.3 (key highlighting)
- **DevOps:**
  - Monitor docker-compose performance
  - Support team with environment issues

**Wednesday, Feb 12, 4:00 PM – Mid-Sprint Sync** (30 min)
- **Agenda:**
  - Each dev reports: "Done | In Progress | Blocked"
  - Discuss any tech blockers
  - Confirm on-track for Friday review target
- **Backend:** "Session POST endpoint working + validated"
- **Frontend:** "Keyboard rendering + highlighting 80% complete"

**Friday, Feb 14, 10:00 AM – Sprint Midpoint Review** (1 hour)
- **Backend:** Demo session POST endpoint (Postman or similar)
- **Frontend:** Demo keyboard visualization (live in browser)
- **DevOps:** Confirm docker-compose ready for sprint usage
- **Decision:** Any scope adjustments for second week?

**Friday, Feb 14, 12:00 PM – Retrospective (45 min)**
- What went well?
- What was blockers?
- Improvements for Sprint 3?

---

### Week 2: Feb 17–21 (5 business days)

**Monday, Feb 17**
- **Backend Dev:**
  - Task 5.5.2–5.5.3 (write + run API tests)
  - Task 4.2.2 (session database insert validation)
- **Frontend Dev:**
  - Task 2.4.4 (error animation + shift indicator)
  - Task 2.5.1–2.5.2 (restart, pause, return buttons)
  - Task 2.6.1 (completion summary layout)
- **DevOps:**
  - Monitor tests running in CI

**Tuesday–Thursday, Feb 18–20**
- **Backend Dev:** Finalize tests, code review, small fixes
- **Frontend Dev:** Finalize controls + completion, animations, styling
- **DevOps:** Support, monitor, prepare demo environment

**Friday, Feb 21, 10:00 AM – Sprint Review** (1 hour)
- **Live Demo:**
  - Frontend: Type lesson → show keyboard + completion summary
  - Backend: Query database → show persisted sessions
  - DevOps: Run tests in CI
- **Attendees:** Team, PO, stakeholders
- **Outcome:** Sprint 2 accepted ✅ or items returned to backlog

**Friday, Feb 21, 12:00 PM – Retrospective** (45 min)
- Same as Week 1
- Record action items for Sprint 3

---

## 👤 Developer Task Breakdown

### **BACKEND DEVELOPER**

#### Story 4.2: Persist Session Results After Lesson Completion (2 pts)

**Context:** Session data is captured on client; backend must persist it to the `sessions` table created in Sprint 1 Story 4.1.

**Task 4.2.1:** Implement `POST /api/sessions` Endpoint (2h)
- **Subtasks:**
  1. Create `SessionCreateDto` (lesson_id, started_at, ended_at, wpm, cpm, accuracy, error_count, per_key_errors)
  2. Add Saturn route: `post [ route "" (fun next ctx -> ...)`
  3. Extract DTO from request body
  4. Return 201 Created with session object in response body
- **Acceptance Criteria:**
  - ✅ Endpoint returns 201 on valid input
  - ✅ Response includes Location header
  - ✅ Session object in response body matches DTO schema
- **Test:** Use Postman or curl to POST test data; verify response

**Task 4.2.2:** Validate & Insert Session to Database (2h)
- **Subtasks:**
  1. Validate lesson_id exists (FK constraint)
  2. Serialize per_key_errors to JSONB
  3. Insert to sessions table
  4. Add error handling (400 for invalid lesson, 500 for DB error)
- **Acceptance Criteria:**
  - ✅ DB insert successful
  - ✅ Foreign key constraint enforced
  - ✅ JSONB serialization works
  - ✅ Error responses follow contract
- **Test:** Query sessions table after POST; verify data integrity

**Task 4.2.3:** Integration Test (1h)
- **Subtasks:**
  1. Write test: POST /api/sessions → verify row in DB
  2. Test edge case: invalid lesson_id → 400 error
  3. Test edge case: malformed per_key_errors → validate/sanitize
- **Acceptance Criteria:**
  - ✅ Integration test passes
  - ✅ Database transaction rolls back on error
- **Test:** Run tests locally + in CI

---

#### Story 3.3: Validate Lesson Input Client & Server (3 pts)

**Context:** Client-side validation provides UX feedback; server-side re-validates for security & consistency.

**Task 3.3.1:** Create Validation Module (1.5h)
- **Subtasks:**
  1. Define `LessonValidation` module (Shared or Server)
  2. Implement `validate : LessonCreateDto -> Result<ValidatedLesson, ValidationError list>`
  3. Validate: title (1–100 chars), textContent (1–5000 chars), difficulty (A1|A2|B1|B2|C1), contentType (words|sentences)
- **Acceptance Criteria:**
  - ✅ Validation function compiles + type-safe
  - ✅ Returns meaningful error messages
  - ✅ Edge cases: empty string, max length, invalid enum
- **Test:** Unit tests for each validation rule

**Task 3.3.2:** Integrate Validation into POST/PUT Endpoints (1.5h)
- **Subtasks:**
  1. Call `validate()` in POST /api/lessons handler
  2. Call `validate()` in PUT /api/lessons/{id} handler
  3. Return 400 Bad Request with errors on validation failure
  4. Normalize line endings (CRLF → LF) before storing
- **Acceptance Criteria:**
  - ✅ Invalid lesson POST returns 400 with errors
  - ✅ Valid lesson POST returns 201
  - ✅ Database contains only validated data
- **Test:** Postman: POST valid + invalid lessons; check responses

---

#### Story 5.5: Add Server API Tests for Lesson CRUD (3 pts)

**Context:** Ensure all lesson endpoints (GET, POST, PUT, DELETE) work correctly with valid/invalid inputs.

**Task 5.5.1:** Set Up Test Project (1h)
- **Subtasks:**
  1. Create `KeyboardTrainer.Tests` (xUnit or NUnit project)
  2. Add dependencies: xunit, xunit.runner.visualstudio, Testcontainers.PostgreSQL (optional)
  3. Create test fixtures: InMemory DB or Docker test container
  4. Create test base class with setup/teardown
- **Acceptance Criteria:**
  - ✅ Test project compiles
  - ✅ Test runner (dotnet test) works
  - ✅ Test database isolated from production
- **Test:** Run `dotnet test`; verify no pre-existing tests fail

**Task 5.5.2:** Write Test Cases for Lesson CRUD (1.5h)
- **Test Cases:** (aim for ≥10 tests)
  1. `GetLessons_ReturnsAllLessons()` – GET / returns 200 + list
  2. `GetLessons_ReturnsEmptyList()` – GET / on empty DB returns 200 + []
  3. `GetLessonById_ReturnsLesson()` – GET /{id} returns 200 + lesson
  4. `GetLessonById_ReturnsNotFound()` – GET /{id} with non-existent id returns 404
  5. `PostLesson_CreatesLesson()` – POST / with valid data returns 201 + location header
  6. `PostLesson_ValidationFails()` – POST / with invalid data returns 400 + errors
  7. `PostLesson_DuplicateTitle()` – POST / with duplicate title allowed (optional business logic)
  8. `PutLesson_UpdatesLesson()` – PUT /{id} updates fields correctly
  9. `PutLesson_NotFound()` – PUT /{id} with non-existent id returns 404
  10. `DeleteLesson_DeletesLesson()` – DELETE /{id} returns 204 + row deleted
  11. `DeleteLesson_NotFound()` – DELETE /{id} with non-existent id returns 404
  12. `ErrorResponse_MatchesContract()` – Error response has message, statusCode, errors

- **Acceptance Criteria:**
  - ✅ All 12+ tests pass
  - ✅ Tests are independent (no order dependency)
  - ✅ Test data cleaned up after each test
  - ✅ ≥80% handler code coverage
- **Test:** Run tests locally + watch for flakiness

**Task 5.5.3:** Code Review + CI Integration (0.5h)
- **Subtasks:**
  1. Self-review test code
  2. Ensure tests run in CI (GitHub Actions or equivalent)
  3. Document any test database setup (env vars, migrations)
- **Acceptance Criteria:**
  - ✅ Tests pass in CI
  - ✅ No warnings or errors
- **Test:** Push to branch; verify CI passes

---

### **FRONTEND DEVELOPER**

#### Story 2.4: Render German QWERTZ Keyboard Visualization (6 pts)

**Context:** Users need a visual keyboard to learn key positions while typing.

**Task 2.4.1:** Create German QWERTZ Layout Data Structure (1h)
- **Subtasks:**
  1. Define `KeyRow` and `Key` types (code, label, row, col)
  2. Define `keyLayout : KeyRow array` with all German keys:
     - Row 1: 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, ß, ´
     - Row 2: Q, W, E, R, T, Z, U, I, O, P, Ü, +
     - Row 3: A, S, D, F, G, H, J, K, L, Ö, Ä, #
     - Row 4: <, Y, X, C, V, B, N, M, comma, period, -, shift
     - Row 5: ctrl, fn, win, alt, space, alt-gr, fn, menu, ctrl
  3. Add metadata: `nextKeyCode`, `lastKeyCode`, error state
- **Acceptance Criteria:**
  - ✅ Type-safe: all keys have valid labels + codes
  - ✅ Layout matches standard QWERTZ (testable visually)
  - ✅ Data is immutable (no mutation during typing)
- **Test:** Manual verification that layout matches reference image

**Task 2.4.2:** Render Keyboard Grid with CSS Grid (2h)
- **Subtasks:**
  1. Create view component `KeyboardView` (Elmish)
  2. Render HTML structure: `<div class="keyboard">` containing `<div class="key">` for each key
  3. Add CSS Grid: responsive grid layout (e.g., 12 columns)
  4. Style keys: background, border, text, monospace font, padding
  5. Style row spacing (gap between rows)
- **Acceptance Criteria:**
  - ✅ Keyboard renders on page
  - ✅ Keys are aligned in rows (visually match QWERTZ layout)
  - ✅ Responsive: keyboard scales on mobile (smaller font, tighter spacing)
  - ✅ Accessible: each key has `aria-label` (e.g., "Q")
- **Test:** Open in browser; visually compare with reference QWERTZ layout

**Task 2.4.3:** Implement Key Highlighting on Keypress (2h)
- **Subtasks:**
  1. Add to Elmish model: `nextKeyCode : string option` (e.g., Some "KeyQ")
  2. Add Msg: `HighlightKey of string`, `ClearKeyHighlight`
  3. In Update: extract key code from typing event → dispatch `HighlightKey`
  4. In View: add CSS class to key if code matches `nextKeyCode` (e.g., `<div class="key highlight">`)
  5. Style highlight: bright green or cyan background, maybe pulse animation
- **Acceptance Criteria:**
  - ✅ Next required key highlights immediately when lesson starts
  - ✅ Highlight updates after each keystroke
  - ✅ Highlight clears when wrong key is pressed (shows error instead)
  - ✅ Smooth animation (CSS transition, no jarring color change)
- **Test:** Type a lesson; verify correct key highlights with each keystroke

**Task 2.4.4:** Add Error Animation + Shift State Indicator (1h)
- **Subtasks:**
  1. Model: add `errorKeyCode : string option`, `shiftActive : bool`
  2. Msg: `ErrorKey of string`, `ClearErrorKey`, `ToggleShift`
  3. Update: capture wrong key → dispatch `ErrorKey`; detect Shift key → dispatch `ToggleShift`
  4. View: add CSS class `.error` to wrong key (red bg); add `.shift-active` indicator text
  5. CSS: shake animation on error (e.g., `@keyframes shake { 0%, 100% { transform: translateX(0) } 50% { transform: translateX(-5px) } }`)
- **Acceptance Criteria:**
  - ✅ Wrong key shows red highlight + shake animation
  - ✅ Shift indicator shows when Shift is held (e.g., "Shift held")
  - ✅ Animation is smooth (60fps) and doesn't cause jank
  - ✅ ARIA live region announces error (optional but nice)
- **Test:** Press wrong key; verify shake animation + red highlight

---

#### Story 2.5: Add Session Controls (Restart, Pause, Return) (2 pts)

**Context:** Users need to control the typing session (restart, pause, return to start).

**Task 2.5.1:** Create Restart, Pause, Return Buttons (1h)
- **Subtasks:**
  1. Add to Elmish model: `isPaused : bool`
  2. Add Msg: `Restart`, `TogglePause`, `ReturnToStart`
  3. Create view: three buttons in fixed header/toolbar
  4. Style buttons: clear, visible, accessible (large click target, icon + text)
- **Acceptance Criteria:**
  - ✅ Buttons render in fixed position
  - ✅ Buttons are keyboard accessible (Tab + Enter)
  - ✅ Button text is clear (e.g., "Restart Lesson", "Pause", "Return to Start")
  - ✅ Buttons have ARIA labels + tooltips
- **Test:** Manual: Tab through buttons; verify each one is reachable

**Task 2.5.2:** Implement Pause/Resume Logic + Confirmation (1h)
- **Subtasks:**
  1. In Update (Restart): clear typedText, reset timer; if elapsed > 30s, show confirmation modal
  2. In Update (TogglePause): if not paused → pause timer, disable input; if paused → resume timer, enable input
  3. Create modal component: "Are you sure? You'll lose progress." with Cancel/Confirm buttons
  4. In Update (ReturnToStart): navigate to start screen (Elmish Router)
- **Acceptance Criteria:**
  - ✅ Restart clears text + resets timer
  - ✅ Restart shows confirmation if > 30s elapsed
  - ✅ Pause disables input + freezes timer
  - ✅ Resume continues from paused state
  - ✅ Return to Start navigates without saving progress
- **Test:** Type → pause → resume → verify metrics continue; type → restart → verify reset

---

#### Story 2.6: Display Completion Summary with Results (3 pts)

**Context:** When lesson is complete, show performance summary and save results to backend.

**Task 2.6.1:** Create Completion Summary Layout (1.5h)
- **Subtasks:**
  1. Add to Elmish model: `lessonComplete : bool`
  2. Add Msg: `LessonComplete`
  3. In Update: detect when typedText == lessonText && all chars correct → dispatch `LessonComplete`
  4. Create view component `CompletionSummary`: modal overlay with:
     - Title: "Lesson Complete! 🎉"
     - Metrics: Total time, Final accuracy %, Final WPM, Error count
     - Error hotspots (optional): top 3 keys with most errors
     - Buttons: "Try Again", "Next Lesson" (disabled if no next), "Back to Start"
  5. Style: centered modal, clear typography, colors for metrics (green for good, yellow for OK)
- **Acceptance Criteria:**
  - ✅ Summary appears when lesson is complete
  - ✅ All metrics displayed correctly
  - ✅ Buttons are functional (try again, next, back)
  - ✅ Modal is accessible (focus trap, Escape to close)
- **Test:** Complete a lesson; verify summary appears with correct metrics

**Task 2.6.2:** Implement Save Session Result (1h)
- **Subtasks:**
  1. Add to model: `savingSession : bool`, `sessionSaveError : string option`
  2. On `LessonComplete`: capture session data (startTime, endTime, wpm, accuracy, errorCount, per_key_errors)
  3. Build `SessionCreateDto`
  4. Cmd.ofPromise: POST to `/api/sessions` (from Sprint 1 Story 4.1)
  5. On success: set model state, show toast "Session saved ✓"
  6. On error: show toast "Failed to save session" but don't block completion summary
- **Acceptance Criteria:**
  - ✅ POST /api/sessions is called with correct data
  - ✅ Success: session persists to DB
  - ✅ Error: user sees message but can continue
  - ✅ Spinner/loading state shows while saving
- **Test:** Complete lesson; check database for new session row

**Task 2.6.3:** Add "Next Lesson" Navigation (0.5h)
- **Subtasks:**
  1. Query next lesson from lesson list (by difficulty + ID)
  2. "Next Lesson" button loads next lesson (or stays disabled if no next)
  3. Navigate to typing view with new lesson
- **Acceptance Criteria:**
  - ✅ "Next Lesson" button functional if next lesson exists
  - ✅ Button disabled if no next lesson
  - ✅ Clicking navigates to new lesson (clears metrics, starts fresh)
- **Test:** Complete lesson → click "Next Lesson" → verify new lesson loads

---

### **DEVOPS DEVELOPER**

**Primary Role in Sprint 2:** Maintain + optimize Docker setup, support team environment.

**Supporting Tasks:**
1. **Monitor Docker Performance** (ongoing)
   - Watch for build time issues
   - Monitor volume mount performance (if hot reload is slow)
   - Log any docker-compose issues from team
2. **Support Test Infrastructure** (later in sprint)
   - Backend team's tests need test DB setup
   - Optionally provide Docker test container for PostgreSQL
3. **Prepare for Sprint 3** (end of sprint)
   - Review production Dockerfile requirements (Story 5.7)
   - Research Azure App Service deployment options (Story 5.8)

**No formal story points assigned; ~4 hours/week supporting work**

---

## 🎯 Risk Register

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| Keyboard visualization layout doesn't match German QWERTZ standard | Medium | High | Verify layout against reference early (Tue); adjust if needed |
| Session save endpoint causes performance issues (high DB load) | Low | Medium | Test with ~100 concurrent saves; optimize query if needed |
| Pause/Resume logic bugs affect timer | Medium | Medium | Unit test timer logic separately; test manually |
| Validation logic mismatch (client vs server) | Medium | High | Share validation code between client + server (Shared module) |
| API tests flaky in CI | Medium | Medium | Use isolated test DB; ensure deterministic test data |

---

## 📊 Burndown Target

**Total Story Points:** 19  
**Days:** 10 business days (2 weeks)

**Target Burndown:**
- **Start (Mon Feb 10):** 19 pts
- **End of Tue Feb 11:** 16 pts (3 pts done)
- **End of Wed Feb 12:** 12 pts (7 pts done = 37%)
- **End of Thu Feb 13:** 8 pts (50% done)
- **End of Fri Feb 14:** 6 pts (68% done)
- **End of Mon Feb 17:** 4 pts (79% done)
- **End of Tue Feb 18:** 2 pts (89% done)
- **End of Fri Feb 21:** 0 pts (100% done ✅)

If burndown deviates >20% from target, adjust scope or request help.

---

## 📝 Definition of Done (Per Story)

Each story is done when:

- ✅ All acceptance criteria met (100% functional)
- ✅ Code passes peer review (2+ approvals for backend/frontend)
- ✅ Unit + integration tests pass (if applicable)
- ✅ Manual testing completed (developer + QA sign-off)
- ✅ No P0 or P1 bugs
- ✅ Documentation updated (inline comments, README if needed)
- ✅ PR merged to main (via git merge, tracked in BRANCH_STRATEGY.md)

---

## 👥 Pair Programming / Code Review Plan

**Code Review Protocol:**
- All PRs require ≥1 approval before merge
- Frontend PRs: Frontend lead reviews (focus: Elmish patterns, accessibility)
- Backend PRs: Backend lead reviews (focus: API design, validation, testing)
- DevOps PRs: Tech lead reviews

**Pair Programming (Optional):**
- Recommend pairing on Story 2.4 (keyboard) if frontend dev is new to Elmish
- Recommend pairing on Task 5.5 (testing) if backend dev is new to xUnit

---

## 📞 Escalation & Communication

**Blockers:**
- **<1 hour:** Self-resolve or ask in standup
- **1–4 hours:** Slack #sprint-2 or sync with team lead
- **>4 hours:** Escalate to Scrum Master for priority adjustment

**Daily Standup:**
- **Time:** Monday–Friday, 9:00 AM
- **Duration:** 15 minutes
- **Format:** "Done yesterday | Doing today | Blockers"
- **Attendees:** All developers + Scrum Master + Tech Lead

**Mid-Sprint Sync:**
- **Wed 4:00 PM, 30 minutes**
- Discuss progress, adjust scope if needed

---

## 🚀 Next Steps (After Sprint 2)

**Friday, Feb 21, 5:00 PM – Sprint 2 Complete ✅**

**What's Next?**
- Sprint 3 planning (Polish & Deployment)
- Prepare for accessibility focus (WCAG 2.1 AA)
- Schedule production deployment discussion with stakeholders

**Carry-Over (if any):**
- Document all incomplete stories → add to Sprint 3 backlog
- Record technical debt or improvements for retrospective

---

**Plan Version:** 1.0  
**Last Updated:** January 25, 2026  
**Prepared By:** Scrum Master / Tech Lead  
**Next Review:** Weekly during standups
