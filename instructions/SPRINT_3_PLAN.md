# Sprint 3 Plan — Polish & Deployment Phase (Feb 24 – Mar 7, 2026)

**Sprint Goal:** "Accessibility + Deployment ready + Lesson editor + Full unit test coverage"

**Duration:** 2 weeks (10 business days)  
**Team:** 3 developers (1 backend, 1 frontend, 1 devops)  
**Total Story Points:** 19 points  
**Velocity Target:** 15–20 pts/sprint (sustainable pace)

---

## 📋 Sprint 3 Backlog

| Priority | Story | Title | Owner | Points | Status |
|----------|-------|-------|-------|--------|--------|
| 🟡 HIGH | 3.1 | Display Lesson Editor Form (Create & Edit) | Frontend | 3 | Ready |
| 🟡 HIGH | 3.3 | Validate Lesson Input Client & Server | Frontend | 3 | Ready |
| 🟢 MEDIUM | 5.1 | Ensure Keyboard-Only Accessibility | Frontend | 3 | Ready |
| 🟢 MEDIUM | 5.2 | Add ARIA Labels & Keyboard Descriptions | Frontend | 2 | Ready |
| 🔴 CRITICAL | 5.7 | Create Dockerfiles for Server & Client | DevOps | 3 | Ready |
| 🟡 HIGH | 5.8 | Create Production Deployment Guide | DevOps | 2 | Ready |
| 🔴 CRITICAL | 5.4 | Add Unit Tests for Text Processing & Typing Logic | Backend | 3 | Ready |

**Sprint Total:** 19 points

---

## 🎯 Sprint Success Criteria (Definition of Done)

By **Friday, Mar 7, 2026 @ 5:00 PM**, Sprint 3 is complete when:

- ✅ **Story 3.1 (Lesson Editor):** Create & edit forms fully functional; validation working
- ✅ **Story 3.3 (Validation - Frontend):** Client-side validation complete; matches server-side
- ✅ **Story 5.1 (Keyboard Accessibility):** All interactive elements keyboard-accessible; Tab order logical
- ✅ **Story 5.2 (ARIA Labels):** All elements have ARIA labels; screenreader tested
- ✅ **Story 5.4 (Unit Tests):** ≥10 tests for typing logic; ≥80% code coverage
- ✅ **Story 5.7 (Dockerfiles):** Production Dockerfiles build + run; optimized for size
- ✅ **Story 5.8 (Deployment Guide):** Clear, actionable deployment steps; checklist complete
- ✅ **Overall:** Zero P0 bugs; ≤1 P1 bug
- ✅ **Code Review:** All PRs reviewed + approved
- ✅ **MVP Ready:** App deployable to production; all core features working

---

## 📅 Weekly Schedule

### Week 1: Feb 24–28 (5 business days)

**Monday, Feb 24, 9:00 AM – Sprint Kick-Off**
- Review Sprint 3 goals (last sprint of MVP!)
- Assign developers to stories
- Tech lead walks through Dockerfile requirements
- Discuss accessibility expectations (WCAG 2.1 AA)
- Questions & blockers clarified
- Target: developers ready to start by 10 AM

**Tuesday–Thursday, Feb 25–27**
- **Backend Dev:**
  - **Tue:** Task 5.4.1 (test scaffolding + 1st test)
  - **Wed:** Task 5.4.2 (write remaining tests)
  - **Thu:** Task 5.4.3 (edge cases + coverage)
- **Frontend Dev:**
  - **Tue:** Task 3.1.1 (lesson editor form structure)
  - **Wed:** Task 3.1.2 (form state + validation)
  - **Thu:** Task 5.1.1–5.1.2 (keyboard accessibility)
- **DevOps:**
  - **Tue:** Task 5.7.1–5.7.2 (server + client Dockerfiles)
  - **Wed:** Task 5.7.3–5.7.4 (multi-stage build, healthchecks)
  - **Thu:** Task 5.8.1 (deployment guide draft)

**Wednesday, Feb 26, 4:00 PM – Mid-Sprint Sync** (30 min)
- **Agenda:**
  - Each dev reports: "Done | In Progress | Blocked"
  - Discuss any tech blockers
  - Confirm on-track for Friday review target
- **Backend:** "Unit tests scaffolded + 50% complete"
- **Frontend:** "Lesson editor form + keyboard accessibility 70% complete"
- **DevOps:** "Dockerfiles built successfully; ready for testing"

**Friday, Feb 28, 10:00 AM – Sprint Midpoint Review** (1 hour)
- **Backend:** Demo unit tests running; show coverage report
- **Frontend:** Demo lesson editor form (create + edit flows); demo keyboard nav
- **DevOps:** Demo docker build; show image sizes + build times
- **Decision:** Any scope adjustments for second week?

**Friday, Feb 28, 12:00 PM – Retrospective (45 min)**
- What went well?
- What were blockers?
- Improvements for (potential) Sprint 4?

---

### Week 2: Mar 3–7 (5 business days)

**Monday, Mar 3**
- **Backend Dev:**
  - Finalize tests + verify coverage ≥80%
  - Code review + small fixes
- **Frontend Dev:**
  - Finalize lesson editor + ARIA labels
  - Task 5.2.1–5.2.3 (ARIA labels + live regions)
  - Accessibility testing (screenreader)
- **DevOps:**
  - Task 5.8.2–5.8.3 (deployment guide completion)
  - Prepare demo environment

**Tuesday–Thursday, Mar 4–6**
- **Backend Dev:** Final code review, prepare demo
- **Frontend Dev:** Finalize accessibility, test with NVDA/screenreader
- **DevOps:** Final deployment guide review, create checklist

**Friday, Mar 7, 10:00 AM – Final Sprint Review** (1 hour)
- **Live Demo:**
  - Frontend: Create lesson → edit → delete; verify keyboard nav works
  - Frontend: Navigate entire app via keyboard only
  - Backend: Run full test suite; show coverage
  - DevOps: Build + run production Dockerfiles; show deployment guide
- **Attendees:** Team, PO, stakeholders
- **Outcome:** MVP Accepted ✅ & Ready for Production Deployment
- **Discussion:** What's next? (Phase 2: Polish + Analytics)

**Friday, Mar 7, 12:00 PM – Retrospective + Sprint 3 Wrap-Up** (1 hour)
- Retrospective questions
- **MVP Celebration:** 🎉 Complete functional typing app!
- Prepare for production deployment (if scheduled)

---

## 👤 Developer Task Breakdown

### **BACKEND DEVELOPER**

#### Story 5.4: Add Unit Tests for Text Processing & Typing Logic (3 pts)

**Context:** Core typing logic (WPM, accuracy, character matching) needs robust unit test coverage to prevent regressions.

**Task 5.4.1:** Set Up Test Project & Scaffolding (1h)
- **Subtasks:**
  1. Create `KeyboardTrainer.Typing.Tests` (xUnit) if not exists
  2. Add dependencies: xunit, Fable.QUnit (if client-side tests needed)
  3. Create test fixtures for common data (sample lessons, typing scenarios)
  4. Create helper functions: `calculateWPM()`, `calculateAccuracy()`, etc.
- **Acceptance Criteria:**
  - ✅ Test project compiles
  - ✅ `dotnet test` runs successfully
  - ✅ Test framework ready for 10+ tests
- **Test:** Run `dotnet test`; verify no errors

**Task 5.4.2:** Write Unit Tests for Typing Logic (1.5h)
- **Test Cases:** (aim for ≥10 tests)
  1. `CalculateWPM_ValidInput_ReturnsCorrectValue()` – Test WPM formula with known values
  2. `CalculateWPM_ZeroElapsedTime_ReturnsZero()` – Edge case: WPM = 0 if < 1 second
  3. `CalculateAccuracy_AllCorrect_Returns100()` – 100% accuracy when all chars correct
  4. `CalculateAccuracy_HalfCorrect_Returns50()` – 50% accuracy with 50% errors
  5. `CalculateAccuracy_NoTypedChars_Returns0()` – Edge case: 0% accuracy if nothing typed
  6. `IsCharacterMatch_ValidChar_ReturnsTrue()` – Single char match
  7. `IsCharacterMatch_DiacriticMatch_ReturnsTrue()` – French diacritics match correctly (é == é)
  8. `IsCharacterMatch_DiacriticMismatch_ReturnsFalse()` – é != e
  9. `IsLessonComplete_AllCharsTypedCorrectly_ReturnsTrue()` – Completion detected
  10. `IsLessonComplete_TypedTextLength_ReturnsFalse()` – Not complete if text length differs
  11. `CountErrors_TypoSequence_ReturnsCorrectCount()` – Error counting accurate
  12. `CountErrorsByKey_TracksPerKeyErrors_ReturnsMap()` – Per-key error tracking works

- **Acceptance Criteria:**
  - ✅ All 12+ tests pass
  - ✅ Tests are deterministic (same result on re-run)
  - ✅ Tests cover happy path + edge cases
  - ✅ Code coverage ≥80% for typing logic module
- **Test:** Run `dotnet test`; generate coverage report (e.g., via OpenCover or Coverlet)

**Task 5.4.3:** Verify Coverage & Edge Cases (0.5h)
- **Subtasks:**
  1. Run coverage tool: `dotnet test /p:CollectCoverage=true`
  2. Verify coverage ≥80% for:
     - `TypingLogic.calculateWPM`
     - `TypingLogic.calculateAccuracy`
     - `TypingLogic.isCharacterMatch`
     - `TypingLogic.isLessonComplete`
  3. Add tests for any uncovered branches
  4. Document any gaps (e.g., "Diacritics coverage limited to French; German not tested")
- **Acceptance Criteria:**
  - ✅ Coverage report shows ≥80%
  - ✅ All critical paths tested
  - ✅ Edge cases documented
- **Test:** Review coverage HTML report; verify green status

---

### **FRONTEND DEVELOPER**

#### Story 3.1: Display Lesson Editor Form (Create & Edit) (3 pts)

**Context:** Users need to create and edit lessons. The form must support validation feedback and be fully keyboard-accessible.

**Task 3.1.1:** Create Lesson Editor Form Structure (1h)
- **Subtasks:**
  1. Create new route: `/lessons/new` (create) + `/lessons/:id/edit` (edit)
  2. Design form layout (HTML structure + CSS):
     - Title field (text input, max 100 chars)
     - Difficulty dropdown (A1, A2, B1, B2, C1)
     - Content type radio buttons (Words, Sentences)
     - Language field (disabled, shows "French")
     - Text content textarea (max 5000 chars, support multiline)
     - Character counter below textarea
     - Buttons: Submit ("Create lesson" or "Save changes"), Cancel
  3. Add form styling: clear labels, nice spacing, accessible focus indicators
  4. Add accessibility: semantic HTML (form, fieldset, label, textarea, etc.)
- **Acceptance Criteria:**
  - ✅ Form renders on `/lessons/new` and `/lessons/:id/edit`
  - ✅ All form fields visible + properly labeled
  - ✅ Character counter updates live
  - ✅ Form is semantic HTML (no divs for form structure)
- **Test:** Navigate to `/lessons/new`; verify form appears with all fields

**Task 3.1.2:** Implement Form State & Submit Logic (1.5h)
- **Subtasks:**
  1. Add to Elmish model:
     - `editorState: { title: string; difficulty: Difficulty; contentType: ContentType; textContent: string }`
     - `isSubmitting: bool`
     - `submitError: string option`
  2. Add Msg: `UpdateTitle`, `UpdateDifficulty`, `UpdateContentType`, `UpdateTextContent`, `SubmitLesson`, `CancelEdit`, `SubmitSuccess`, `SubmitError`
  3. In Update (create mode):
     - Validate on blur (highlight invalid fields)
     - POST to `/api/lessons` with form data
     - On success: navigate to start screen + show toast "Lesson created"
     - On error: show error message in form
  4. In Update (edit mode):
     - Fetch lesson on mount (GET `/api/lessons/{id}`)
     - Prefill form with lesson data
     - PUT to `/api/lessons/{id}` on submit
     - On success: navigate to start screen + show toast "Lesson updated"
- **Acceptance Criteria:**
  - ✅ Form state managed correctly
  - ✅ Create lesson: POST works + lesson appears in list
  - ✅ Edit lesson: GET + PUT work + changes persist
  - ✅ Error messages display clearly (red text, maybe icon)
  - ✅ Submit button disabled while submitting (loading state)
- **Test:** Create lesson with form → verify it appears in lesson list; edit → verify changes

**Task 3.1.3:** Add Real-Time Validation Feedback (0.5h)
- **Subtasks:**
  1. On blur for each field, run client-side validation
  2. Show validation errors:
     - Title: "Required" (if empty), "Max 100 characters" (if too long)
     - Content: "Required" (if empty), "Max 5000 characters" (if too long)
     - Difficulty/ContentType: highlight if required field
  3. Style errors: red border + error message below field
  4. Disable submit button if validation fails
- **Acceptance Criteria:**
  - ✅ Validation feedback appears on blur
  - ✅ Error messages are clear + actionable
  - ✅ Submit disabled until all fields valid
- **Test:** Type invalid input → blur → verify error appears; fix → verify error clears

---

#### Story 3.3: Validate Lesson Input Client-Side (3 pts)

**Context:** Frontend must validate lesson input locally for UX; server-side validation (implemented in Sprint 2) provides security.

**Task 3.3.1:** Implement Client-Side Validation Module (1.5h)
- **Subtasks:**
  1. Create `LessonValidation.fs` (Shared or Client module)
  2. Define validation function: `validate : LessonCreateDto -> Result<ValidatedLesson, ValidationError list>`
  3. Implement validation rules:
     - Title: required, 1–100 chars, no control characters
     - Text content: required, 1–5000 chars, allow newlines + tabs
     - Difficulty: must be A1|A2|B1|B2|C1
     - ContentType: must be Words|Sentences
  4. Return `Result` with detailed error messages
- **Acceptance Criteria:**
  - ✅ Validation function type-safe
  - ✅ All rules enforced
  - ✅ Error messages are user-friendly (not technical)
- **Test:** Unit tests for each validation rule (5–10 tests)

**Task 3.3.2:** Integrate Validation into Form (1h)
- **Subtasks:**
  1. Call `validate()` on form blur + submit
  2. Display errors under each field
  3. Disable submit until validation passes
  4. Show spinner while POST/PUT is in-flight
- **Acceptance Criteria:**
  - ✅ Form shows validation errors in real-time
  - ✅ Submit disabled if validation fails
  - ✅ User gets clear feedback before submit
- **Test:** Try to submit form with invalid data → verify errors appear + submit blocked

**Task 3.3.3:** Coordinate with Server Validation (Server already done in Sprint 2) (0.5h)
- **Subtasks:**
  1. Ensure client validation rules match server (from Sprint 2)
  2. Document any differences (if client is stricter or lenient)
  3. Test: submit valid client data → verify server accepts (201/200)
- **Acceptance Criteria:**
  - ✅ No surprises: client-valid data always passes server
  - ✅ Server 400 errors (if any) are rare + documented
- **Test:** Submit from form → check POST response is 201 or 200

---

#### Story 5.1: Ensure Keyboard-Only Accessibility (3 pts)

**Context:** App must be fully operable via keyboard (no mouse required). Essential for accessibility compliance + typing app users.

**Task 5.1.1:** Enable Tab Navigation Through All Interactive Elements (1.5h)
- **Subtasks:**
  1. Audit app for all interactive elements: buttons, links, inputs, dropdowns, modals, keyboard
  2. Ensure all elements are in Tab order (default or explicit `tabIndex`)
  3. For keyboard visualization: make each key a focusable element (`<button>` or `<div tabIndex=0>`)
  4. Test: Press Tab repeatedly → verify logical left-to-right, top-to-bottom order
  5. Test: Focus indicators visible on all elements (outline, highlight, etc.)
- **Acceptance Criteria:**
  - ✅ Tab navigates through all interactive elements
  - ✅ Tab order is logical (left-to-right, top-to-bottom)
  - ✅ Focus indicator always visible (outline, border, bg color change)
  - ✅ No keyboard traps (can Tab away from any element)
  - ✅ Modals trap focus (Tab stays within modal, Escape closes)
- **Test:** Navigate app with keyboard only; verify all features reachable

**Task 5.1.2:** Implement Keyboard Shortcuts & Handle Special Keys (1h)
- **Subtasks:**
  1. Add keyboard shortcut: Escape → close modal
  2. Add keyboard shortcut: Space + Enter → activate focused button
  3. Ensure Return/Space work on buttons + links (semantic HTML)
  4. Test: Type in textarea → Tab advances, Escape doesn't close (text input captures Escape)
  5. Test: In modal → Escape closes modal (focus returns to trigger button)
- **Acceptance Criteria:**
  - ✅ Escape closes modals
  - ✅ Space/Enter activate buttons + links
  - ✅ Text inputs don't hijack browser shortcuts
  - ✅ No console errors on special key presses
- **Test:** Open modal → press Escape → verify modal closes; focus on button → press Enter → verify action

**Task 5.1.3:** Verify Focus Trap + Restore (0.5h)
- **Subtasks:**
  1. Test: Open modal → Tab through all elements → Tab from last should loop to first
  2. Test: Close modal → focus returns to trigger button
  3. Test: No focus loss when elements are hidden (e.g., page scroll doesn't move focus)
- **Acceptance Criteria:**
  - ✅ Focus trapped in modal (Tab loops)
  - ✅ Focus restored to trigger button on modal close
- **Test:** Manual accessibility check; use browser dev tools to verify focus state

---

#### Story 5.2: Add ARIA Labels & Keyboard Descriptions (2 pts)

**Context:** Screenreader users need text descriptions of elements. ARIA labels + live regions make app narrative + feedback clear.

**Task 5.2.1:** Add ARIA Labels to All Interactive Elements (1h)
- **Subtasks:**
  1. Audit all buttons: add `aria-label` if text isn't clear (e.g., icon-only buttons)
     - Example: `<button aria-label="Restart lesson">🔄</button>`
  2. Audit form fields: ensure `<label>` is associated with input (via `for` attribute)
     - Example: `<label htmlFor="title-input">Lesson Title</label><input id="title-input" />`
  3. Audit dropdowns: add `aria-label` + `aria-haspopup="listbox"`
  4. Audit keyboard: each key should have `aria-label` (e.g., "Q key" or "Next required key: Q")
  5. Audit error messages: add `aria-describedby` to inputs (link to error text)
- **Acceptance Criteria:**
  - ✅ All interactive elements have descriptive labels (via text, aria-label, or label)
  - ✅ Form errors linked via aria-describedby
  - ✅ No unlabeled buttons or inputs
- **Test:** Use screenreader (NVDA) to navigate; all elements should be announced clearly

**Task 5.2.2:** Add Live Regions for Dynamic Updates (0.5h)
- **Subtasks:**
  1. Add `aria-live="polite"` to metrics display (WPM, accuracy, timer)
     - Screenreader announces changes without user focusing element
  2. Add `aria-live="assertive"` to error/completion messages
     - Immediate announcement (higher priority)
  3. Test: Type lesson → changes in metrics should be announced (if verbose output enabled in screenreader)
- **Acceptance Criteria:**
  - ✅ Metrics announce changes via live regions
  - ✅ Errors announced with assertive priority
  - ✅ No duplicate announcements
- **Test:** NVDA + verbose mode → verify live region updates announced

**Task 5.2.3:** Test with Screenreader + Document Limitations (0.5h)
- **Subtasks:**
  1. Install NVDA (free screenreader) on Windows
  2. Test app with NVDA:
     - Start → navigate to lesson → complete lesson
     - Verify all text, labels, errors announced
     - Note any gaps (e.g., keyboard visualization may be hard to describe)
  3. Document any limitations (e.g., "Keyboard visualization requires visual context; screenreader provides key codes instead")
  4. Create a11y testing checklist for future sprints
- **Acceptance Criteria:**
  - ✅ App navigable with screenreader
  - ✅ User can select lesson, type, see results with screenreader only
  - ✅ Limitations documented
- **Test:** 30-min full screenreader test; document findings

---

### **DEVOPS DEVELOPER**

#### Story 5.7: Create Dockerfiles for Server & Client (3 pts)

**Context:** Production-ready Dockerfiles optimize image size, build time, and runtime security.

**Task 5.7.1:** Create Multi-Stage Dockerfile for Server (1.5h)
- **Subtasks:**
  1. Create `Dockerfile.server` (or `Dockerfile` in server dir):
     ```dockerfile
     # Build stage
     FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
     WORKDIR /src
     COPY . .
     RUN dotnet restore
     RUN dotnet publish -c Release -o /app

     # Runtime stage
     FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
     WORKDIR /app
     COPY --from=builder /app .
     EXPOSE 5000
     ENV ASPNETCORE_URLS=http://+:5000
     HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
       CMD dotnet --version || exit 1
     ENTRYPOINT ["dotnet", "KeyboardTrainer.Server.dll"]
     ```
  2. Multi-stage build: builder stage (SDK, large) → runtime stage (aspnet runtime, small)
  3. Alpine base: lean runtime image (~200MB vs 500MB+)
  4. Healthcheck: allows orchestrator to verify app is ready
- **Acceptance Criteria:**
  - ✅ Dockerfile builds without errors
  - ✅ Final image is <300MB
  - ✅ `docker run` starts app on localhost:5000
  - ✅ Healthcheck responds 200 after startup
- **Test:** `docker build -f Dockerfile.server -t trainer-server .` → `docker run -p 5000:5000 trainer-server` → curl localhost:5000/health

**Task 5.7.2:** Create Multi-Stage Dockerfile for Client (1h)
- **Subtasks:**
  1. Create `Dockerfile.client` (or `Dockerfile` in client dir):
     ```dockerfile
     # Build stage
     FROM node:20-alpine AS builder
     WORKDIR /app
     COPY package*.json ./
     RUN npm install
     COPY . .
     RUN npm run build

     # Runtime stage (nginx)
     FROM nginx:alpine
     COPY --from=builder /app/dist /usr/share/nginx/html
     COPY nginx.conf /etc/nginx/nginx.conf
     EXPOSE 3000
     CMD ["nginx", "-g", "daemon off;"]
     ```
  2. Build stage: Node + npm, runs `npm run build` (Vite build)
  3. Runtime stage: nginx serves dist/ folder
  4. nginx.conf: SPA routing (/* → index.html), gzip compression enabled
  5. Create nginx.conf:
     ```nginx
     events {}
     http {
       gzip on;
       gzip_types text/plain text/css application/json application/javascript;
       server {
         listen 3000;
         root /usr/share/nginx/html;
         location / {
           try_files $uri $uri/ /index.html;
         }
       }
     }
     ```
- **Acceptance Criteria:**
  - ✅ Dockerfile builds without errors
  - ✅ Final image is <50MB
  - ✅ `docker run` starts nginx on localhost:3000
  - ✅ Static assets serve correctly
  - ✅ SPA routing works (/* redirects to index.html)
- **Test:** `docker build -f Dockerfile.client -t trainer-client .` → `docker run -p 3000:3000 trainer-client` → curl localhost:3000 (should return HTML)

**Task 5.7.3:** Optimize Build Performance & Image Size (0.5h)
- **Subtasks:**
  1. Add `.dockerignore` file: exclude node_modules, .git, .env, etc.
  2. Server: use .NET layer caching (COPY .csproj first, then source)
  3. Client: use npm layer caching (COPY package.json first, then source)
  4. Test build time: aim for <5 min total
  5. Verify image sizes:
     - Server: <300MB
     - Client: <50MB
- **Acceptance Criteria:**
  - ✅ Build time <5 min
  - ✅ Images optimized for size
  - ✅ Layer caching working (rebuild is faster if only source changed)
- **Test:** Measure build time + image sizes; compare before/after .dockerignore

---

#### Story 5.8: Create Production Deployment Guide (2 pts)

**Context:** Clear deployment instructions enable the ops team to deploy to Azure App Service.

**Task 5.8.1:** Document Azure App Service Deployment (1h)
- **Subtasks:**
  1. Create `DEPLOYMENT_GUIDE.md`:
     - **Prerequisites:** Azure account, Azure CLI, Docker installed locally
     - **Steps:**
       1. Build Dockerfiles: `docker build -t trainer-server ...`, `docker build -t trainer-client ...`
       2. Create Azure Container Registry (ACR)
       3. Push images to ACR
       4. Create App Service plan (e.g., Standard B1)
       5. Create App Service (with ACR image)
       6. Set environment variables (DATABASE_URL, etc.) in App Settings
       7. Create database (Azure Database for PostgreSQL)
       8. Run migrations (via container startup or manual script)
       9. Health check: curl `/health` endpoint
       10. Enable monitoring (Application Insights)
     - **Rollback:** Document how to revert to previous image if deployment fails
  2. Add troubleshooting section: common issues + fixes
- **Acceptance Criteria:**
  - ✅ Steps are clear + actionable
  - ✅ Someone with Azure knowledge can follow guide independently
  - ✅ No missing dependencies or prerequisites
- **Test:** Review with tech lead; iterate on clarity

**Task 5.8.2:** Document Environment Variables & Secrets Management (0.5h)
- **Subtasks:**
  1. List all environment variables used by app:
     - `DATABASE_URL` (server)
     - `ASPNETCORE_ENVIRONMENT` (Development | Staging | Production)
     - `ASPNETCORE_URLS` (default: http://+:5000)
  2. Document secrets management:
     - Use Azure Key Vault (not .env files in prod)
     - Reference Key Vault secrets in App Service settings
     - Example: `@Microsoft.KeyVault(SecretUri=https://...)`
  3. Document rotation policy (if applicable)
- **Acceptance Criteria:**
  - ✅ All env vars documented
  - ✅ Secrets strategy is secure (no hardcoded secrets)
- **Test:** Verify App Service configuration matches guide

**Task 5.8.3:** Create Pre-Deployment Checklist (0.5h)
- **Subtasks:**
  1. Create checklist in `DEPLOYMENT_GUIDE.md`:
     - [ ] All tests passing (CI/CD green)
     - [ ] Docker images build without errors
     - [ ] Database migration script tested locally
     - [ ] Environment variables configured in Azure
     - [ ] Healthcheck endpoint returns 200
     - [ ] App Service slots set up (staging for testing before prod)
     - [ ] Rollback plan documented + tested
     - [ ] Monitoring + alerts configured
     - [ ] Backups enabled
     - [ ] DNS/domain configured
  2. Add post-deployment verification:
     - [ ] App starts (healthcheck passes)
     - [ ] Lesson list loads (GET /api/lessons returns 200)
     - [ ] Can create lesson (POST returns 201)
     - [ ] Can type lesson (no 500 errors)
     - [ ] Session saves (POST /api/sessions succeeds)
- **Acceptance Criteria:**
  - ✅ Checklist is comprehensive + covers risks
  - ✅ Someone can use checklist to deploy safely
- **Test:** Review with ops person (if available); ask for feedback

---

## 🎯 Risk Register

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|-----------|
| ARIA live regions cause excessive announcements | Medium | Low | Test with screenreader; adjust aria-live frequency |
| Focus trap in modal is buggy (focus escapes) | Low | High | Test extensively with keyboard; use accessibility library if needed |
| Production Dockerfiles fail to build in CI | Medium | High | Test builds locally before committing; verify CI has Docker installed |
| Database migrations fail in production | Low | Critical | Test migrations locally + in staging; document rollback steps |
| Accessibility testing takes longer than estimated | High | Medium | Start testing early; use automated tools (axe) + manual NVDA testing |

---

## 📊 Burndown Target

**Total Story Points:** 19  
**Days:** 10 business days (2 weeks)

**Target Burndown:**
- **Start (Mon Feb 24):** 19 pts
- **End of Tue Feb 25:** 15 pts (4 pts done)
- **End of Wed Feb 26:** 10 pts (9 pts done = 47%)
- **End of Thu Feb 27:** 6 pts (63% done)
- **End of Fri Feb 28:** 4 pts (79% done)
- **End of Mon Mar 3:** 2 pts (89% done)
- **End of Tue Mar 4:** 1 pt (95% done)
- **End of Fri Mar 7:** 0 pts (100% done ✅)

---

## 📝 Definition of Done (Per Story)

Each story is done when:

- ✅ All acceptance criteria met (100% functional)
- ✅ Code passes peer review (≥1 approval)
- ✅ Unit + integration tests pass (if applicable)
- ✅ Manual testing completed (developer + QA sign-off)
- ✅ No P0 or P1 bugs
- ✅ Accessibility tested (if applicable): keyboard nav + screenreader
- ✅ Documentation updated (inline comments, README, deployment guide)
- ✅ PR merged to main

---

## 👥 Code Review Plan

**Code Review Protocol:**
- All PRs require ≥1 approval before merge
- Accessibility PRs (5.1, 5.2): Accessibility lead reviews (focus: WCAG compliance, keyboard nav)
- Lesson Editor PRs (3.1, 3.3): Frontend lead reviews (focus: form logic, validation)
- Unit Test PRs (5.4): Backend lead reviews (focus: test coverage, test quality)
- Dockerfile PRs (5.7, 5.8): Tech lead reviews (focus: security, performance)

**Pair Programming (Recommended):**
- Story 5.2 (ARIA + screenreader): pair new dev with accessibility expert
- Story 5.7 (Dockerfiles): pair if ops person is unfamiliar with multi-stage builds

---

## 📞 Escalation & Communication

**Blockers:**
- **<1 hour:** Self-resolve or ask in standup
- **1–4 hours:** Slack #sprint-3 or sync with tech lead
- **>4 hours:** Escalate to Scrum Master for priority adjustment

**Daily Standup:**
- **Time:** Monday–Friday, 9:00 AM
- **Duration:** 15 minutes
- **Format:** "Done yesterday | Doing today | Blockers"
- **Attendees:** All developers + Scrum Master + Tech Lead + (Optional) Accessibility Consultant

**Accessibility Testing Session (Optional):**
- **Wed Feb 26, 3:00 PM**
- Invite accessibility expert to observe keyboard nav + screenreader testing
- Gather feedback early; make adjustments before Friday review

---

## 🚀 Next Steps (After Sprint 3)

**Friday, Mar 7, 5:00 PM – Sprint 3 Complete ✅ MVP READY!**

**What's Next?**
- **Production Deployment** (immediately, if scheduled)
  - Follow DEPLOYMENT_GUIDE.md
  - Execute pre-deployment checklist
  - Post-deployment verification
- **Go-Live Monitoring** (first week of March)
  - Monitor error rates, uptime, user feedback
  - Hotfix critical issues only
- **Phase 2 Planning** (next sprint)
  - Backlog refinement for Stories 4.3, 5.3, 5.4 (analytics, i18n, more tests)
  - Roadmap discussion: what's next after MVP?

**Celebration:** 🎉 Launch party (virtual or in-person) to celebrate MVP completion!

---

**Plan Version:** 1.0  
**Last Updated:** January 25, 2026  
**Prepared By:** Scrum Master / Tech Lead  
**Next Review:** Weekly during standups  
**MVP Completion Target:** Friday, March 7, 2026
