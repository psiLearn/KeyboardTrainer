# MVP Sprint Roadmap — Role-Based Planning

**Project:** KeyboardTrainer  
**Framework:** 6-Phase Agile + SAFE Stack  
**Current Date:** January 25, 2026  
**MVP Duration:** 3 weeks (Sprint 1–3, Jan 27 – Feb 7, then Feb 10–14 + Feb 17–21)  
**Target Deployment:** End of Sprint 3

---

## 🎯 MVP Scope Overview

**MVP = Functional typing app users can start immediately**

| Metric | Target |
|--------|--------|
| **User Stories** | 20 stories (out of 30 total) |
| **Story Points** | 60–70 points |
| **Sprints** | 3 × 2-week sprints |
| **Team Size** | 3 developers (1 backend, 1 frontend, 1 devops) |
| **Key Deliverables** | Typing view, metrics, keyboard viz, lesson CRUD, Docker dev |

---

## 📋 Role-Based Sprint Planning

### **BACKEND DEVELOPER** — Focus: Data & API Layer

**Languages & Frameworks:** F# + Saturn + PostgreSQL + Dapper

#### Sprint 1 (Jan 27 – Feb 7)

| Story | Title | Points | Priority | Status |
|-------|-------|--------|----------|--------|
| **1.2** | Persist & Fetch Lessons from PostgreSQL | 5 | 🔴 CRITICAL | Ready |
| **1.3** | Seed Database with Initial French Lessons | 2 | 🟡 HIGH | Ready |
| **3.5** | Create Lesson via POST Endpoint | 2 | 🟡 HIGH | Ready |
| **3.6** | Delete Lesson via DELETE Endpoint | 1 | 🟡 HIGH | Ready |
| **4.1** | Create Sessions Table & POST Endpoint | 3 | 🟢 MEDIUM | Ready |

**Sprint 1 Subtotal:** 13 points | **Duration:** 1.5 weeks (by Feb 5)

**Daily Workflow:**
- **Monday 9 AM:** Kick-off; assign dev to task 1.2.1 (database schema)
- **Daily 9 AM:** 15-min standup; report progress on tasks
- **Tuesday PM:** Database schema + API endpoints ready for frontend to test
- **Wed 4 PM:** Mid-sprint check-in; verify no blockers
- **Friday 10 AM:** Sprint review; demo API + database
- **Friday 12 PM:** Retrospective

**Task Breakdown (Story 1.2):**
1. **1.2.1** – Design & migrate `lessons` table (2h)
2. **1.2.2** – Implement `GET /api/lessons` endpoint (2h)
3. **1.2.3** – Implement `GET /api/lessons/{id}`, `POST`, `PUT`, `DELETE` endpoints (4h)
4. **1.2.4** – Add error handling, validation, indexing (2h)

**Success Criteria:**
- ✅ All 5 endpoints return correct status codes
- ✅ Error responses match contract
- ✅ Database indexes created
- ✅ Frontend can fetch & display lesson list
- ✅ Zero P0 bugs

---

#### Sprint 2 (Feb 10 – Feb 21)

| Story | Title | Points | Priority | Status |
|-------|-------|--------|----------|--------|
| **4.2** | Persist Session Results After Lesson Completion | 2 | 🟡 HIGH | Ready |
| **3.3** | Validate Lesson Input Client & Server | 3 | 🟡 HIGH | Ready |
| **5.5** | Add Server API Tests for Lesson CRUD | 3 | 🟢 MEDIUM | Ready |

**Sprint 2 Subtotal:** 8 points | **Duration:** 2 weeks (by Feb 21)

**Daily Workflow:** (Same schedule as Sprint 1)

**Task Breakdown (Story 4.2):**
1. **4.2.1** – Implement `POST /api/sessions` endpoint (2h)
2. **4.2.2** – Add session validation & database insert (2h)
3. **4.2.3** – Test end-to-end session save flow (1h)

**Success Criteria:**
- ✅ Sessions persist to database
- ✅ Client can save session after lesson completion
- ✅ Validation tests pass (≥10 test cases)
- ✅ 80%+ code coverage on API handlers

---

#### Sprint 3 (Feb 24 – Mar 7)

| Story | Title | Points | Priority | Status |
|-------|-------|--------|----------|--------|
| **5.7** | Create Dockerfiles for Server & Client | 3 | 🔴 CRITICAL | Ready |
| **5.4** | Add Unit Tests for Text Processing & Typing Logic | 3 | 🟡 HIGH | Ready |

**Sprint 3 Subtotal:** 6 points | **Duration:** 2 weeks (by Mar 7)

**Daily Workflow:** (Same schedule)

**Success Criteria:**
- ✅ Server Dockerfile builds and runs
- ✅ Unit tests cover critical text processing functions
- ✅ No build failures

---

### **FRONTEND DEVELOPER** — Focus: UI/UX & Client Logic

**Languages & Frameworks:** F# + Fable + Elmish + React/HTML/CSS

#### Sprint 1 (Jan 27 – Feb 7)

| Story | Title | Points | Priority | Status |
|-------|-------|--------|----------|--------|
| **1.1** | Display Start Screen with App Title & CTAs | 3 | 🔴 CRITICAL | Ready |
| **2.1** | Render Typing View with Lesson Text | 5 | 🔴 CRITICAL | Ready |
| **2.2** | Capture User Typing Input with Dead Key Handling | 5 | 🟡 HIGH | Ready |
| **2.3** | Display Live Typing Metrics (WPM, Accuracy, Time) | 4 | 🟡 HIGH | Ready |

**Sprint 1 Subtotal:** 17 points | **Duration:** 2 weeks (by Feb 7)

**Daily Workflow:**
- **Monday 9 AM:** Kick-off; assign frontend dev to stories 1.1 + 2.1
- **Tue–Wed AM:** Implement 1.1 (start screen) + 2.1 (typing view) skeleton
- **Tue–Wed PM:** Implement 2.2 (input capture) + 2.3 (metrics)
- **Wed 4 PM:** Mid-sprint check-in; demo UI components
- **Thu:** Polish styling + keyboard integration
- **Friday 10 AM:** Sprint review; demo functional typing view
- **Friday 12 PM:** Retrospective

**Task Breakdown (Story 2.1):**
1. **2.1.1** – Create Elmish model + Msg types for typing view (1h)
2. **2.1.2** – Render lesson text with cursor highlight (2h)
3. **2.1.3** – Add CSS styling for character states (green/red/yellow) (2h)
4. **2.1.4** – Test multiline rendering + diacritics (1h)

**Task Breakdown (Story 2.2):**
1. **2.2.1** – Add keyboard event listeners (keydown, input) (1h)
2. **2.2.2** – Implement character matching logic (1h)
3. **2.2.3** – Handle dead keys & diacritics (2h)
4. **2.2.4** – Test with French keyboard layout (1h)

**Success Criteria:**
- ✅ Start screen renders with lesson list from API
- ✅ Typing view accepts user input + highlights correctly typed chars
- ✅ Metrics (WPM, accuracy, timer) update live
- ✅ French diacritics render + capture correctly
- ✅ No P0 bugs; smooth 60fps animations

---

#### Sprint 2 (Feb 10 – Feb 21)

| Story | Title | Points | Priority | Status |
|-------|-------|--------|----------|--------|
| **2.4** | Render German QWERTZ Keyboard Visualization | 6 | 🔴 CRITICAL | Ready |
| **2.5** | Add Session Controls (Restart, Pause, Return) | 2 | 🟡 HIGH | Ready |
| **2.6** | Display Completion Summary with Results | 3 | 🟡 HIGH | Ready |

**Sprint 2 Subtotal:** 11 points | **Duration:** 2 weeks (by Feb 21)

**Task Breakdown (Story 2.4):**
1. **2.4.1** – Create German QWERTZ layout data structure (1h)
2. **2.4.2** – Render keyboard grid with CSS Grid (2h)
3. **2.4.3** – Implement key highlighting on keypress (2h)
4. **2.4.4** – Add error animation + shift state indicator (1h)

**Success Criteria:**
- ✅ Keyboard renders correctly with all German keys (ü, ö, ä, etc.)
- ✅ Next required key highlights in real-time
- ✅ Error animation plays on wrong key
- ✅ Responsive design on mobile/tablet

---

#### Sprint 3 (Feb 24 – Mar 7)

| Story | Title | Points | Priority | Status |
|-------|-------|--------|----------|--------|
| **3.1** | Display Lesson Editor Form (Create & Edit) | 3 | 🟡 HIGH | Ready |
| **3.3** | Validate Lesson Input Client-Side | 3 | 🟡 HIGH | Ready |
| **5.1** | Ensure Keyboard-Only Accessibility | 3 | 🟢 MEDIUM | Ready |
| **5.2** | Add ARIA Labels & Keyboard Descriptions | 2 | 🟢 MEDIUM | Ready |

**Sprint 3 Subtotal:** 11 points | **Duration:** 2 weeks (by Mar 7)

**Task Breakdown (Story 3.1):**
1. **3.1.1** – Create lesson editor form structure (HTML/CSS) (2h)
2. **3.1.2** – Implement form state management (Elmish) (2h)
3. **3.1.3** – Add validation feedback + live char count (1h)

**Success Criteria:**
- ✅ Create & edit forms functional
- ✅ Form validation shows errors clearly
- ✅ All fields accessible via keyboard
- ✅ ARIA labels complete + screenreader tested

---

### **DEVOPS DEVELOPER** — Focus: Infrastructure & Deployment

**Languages & Frameworks:** Docker, Docker Compose, Shell, YAML

#### Sprint 1 (Jan 27 – Feb 7)

| Story | Title | Points | Priority | Status |
|-------|-------|--------|----------|--------|
| **5.6** | Create Docker Compose for Local Development | 4 | 🔴 CRITICAL | Ready |

**Sprint 1 Subtotal:** 4 points | **Duration:** 1.5 weeks (by Feb 5)

**Daily Workflow:**
- **Monday 9 AM:** Kick-off; review docker-compose requirements
- **Tue–Wed AM:** Create docker-compose.yml + Dockerfiles
- **Wed PM:** Test three-service stack (db, server, client)
- **Thu:** Add healthchecks + documentation
- **Friday 10 AM:** Sprint review; demo `docker-compose up -d`
- **Friday 12 PM:** Retrospective

**Task Breakdown (Story 5.6):**
1. **5.6.1** – Define docker-compose.yml with postgres + server + client (2h)
2. **5.6.2** – Create Dockerfile.server (multi-stage build) (2h)
3. **5.6.3** – Create Dockerfile.client (Vite + nginx) (1h)
4. **5.6.4** – Add healthchecks + documentation (1h)

**Success Criteria:**
- ✅ `docker-compose up -d` starts all three services
- ✅ PostgreSQL accessible on localhost:5434
- ✅ Server accessible on localhost:5000
- ✅ Client accessible on localhost:3000
- ✅ Volumes persist data + enable hot reload
- ✅ README documented with startup steps

---

#### Sprint 2 (Feb 10 – Feb 21)

**Focus:** Maintain + optimize Docker setup based on feedback from Sprint 1

| Story | Title | Points | Priority | Status |
|-------|-------|--------|----------|--------|
| **(Supporting)** | Monitor Docker performance, optimize layer caching | — | 🟢 MEDIUM | On-Demand |

**Sprint 2 Subtotal:** 0 points (supporting work)

---

#### Sprint 3 (Feb 24 – Mar 7)

| Story | Title | Points | Priority | Status |
|-------|-------|--------|----------|--------|
| **5.7** | Create Dockerfiles for Server & Client | 3 | 🔴 CRITICAL | Ready |
| **5.8** | Create Production Deployment Guide | 2 | 🟡 HIGH | Ready |

**Sprint 3 Subtotal:** 5 points | **Duration:** 2 weeks (by Mar 7)

**Task Breakdown (Story 5.8):**
1. **5.8.1** – Document Azure App Service deployment steps (1h)
2. **5.8.2** – Document environment variables + secrets (Key Vault) (1h)
3. **5.8.3** – Create production checklist (0.5h)

**Success Criteria:**
- ✅ Production Dockerfiles tested locally
- ✅ Deployment guide is clear + actionable
- ✅ Database migration strategy documented
- ✅ Health check endpoints working

---

## 📊 Sprint Summary Table

### Sprint 1: Foundation (Jan 27 – Feb 7)

| Role | Stories | Points | Primary Goal |
|------|---------|--------|--------------|
| **Backend** | 1.2, 1.3, 3.5, 3.6, 4.1 | 13 | Database + API ready |
| **Frontend** | 1.1, 2.1, 2.2, 2.3 | 17 | Typing view + metrics |
| **DevOps** | 5.6 | 4 | Docker dev environment |
| **TOTAL** | **10 stories** | **34 points** | **Minimum viable typing app** |

**Deliverables by Fri Feb 7:**
- ✅ Lesson list from PostgreSQL
- ✅ Typing view with live WPM, accuracy, timer
- ✅ Input capture with diacritics support
- ✅ Docker dev setup (one-command local development)
- ✅ Basic test infrastructure

---

### Sprint 2: Enhancement (Feb 10 – Feb 21)

| Role | Stories | Points | Primary Goal |
|------|---------|--------|--------------|
| **Backend** | 4.2, 3.3, 5.5 | 8 | Session persistence + testing |
| **Frontend** | 2.4, 2.5, 2.6 | 11 | Keyboard viz + completion flow |
| **DevOps** | (supporting) | — | Support + optimize |
| **TOTAL** | **6 stories** | **19 points** | **Keyboard viz + session save** |

**Deliverables by Fri Feb 21:**
- ✅ German QWERTZ keyboard visualization
- ✅ Session controls (restart, pause, return)
- ✅ Completion summary with results
- ✅ Session persistence to database
- ✅ API test coverage (≥10 tests)

---

### Sprint 3: Polish & Deployment (Feb 24 – Mar 7)

| Role | Stories | Points | Primary Goal |
|------|---------|--------|--------------|
| **Backend** | 5.4 | 3 | Unit test coverage |
| **Frontend** | 3.1, 3.3, 5.1, 5.2 | 11 | Lesson editor + accessibility |
| **DevOps** | 5.7, 5.8 | 5 | Production Dockerfiles + deploy guide |
| **TOTAL** | **7 stories** | **19 points** | **Accessibility + deployment ready** |

**Deliverables by Fri Mar 7:**
- ✅ Lesson create/edit fully functional
- ✅ Full keyboard + screenreader accessibility
- ✅ Unit test coverage (≥80%)
- ✅ Production Dockerfiles tested
- ✅ Deployment guide complete
- ✅ **MVP READY FOR PRODUCTION DEPLOYMENT**

---

## 🎯 MVP Definition of Done

By end of Sprint 3 (Mar 7, 2026), the MVP is complete when:

### Functional Completeness
- ✅ Users can view lesson list on start screen
- ✅ Users can select a lesson and start typing
- ✅ Live metrics (WPM, accuracy, timer) display and update in real-time
- ✅ German QWERTZ keyboard visualization shows next required key
- ✅ French diacritics input and rendering works flawlessly
- ✅ Session completion triggers summary screen
- ✅ Users can create, edit, and delete custom lessons
- ✅ Session results persist to database

### Technical Excellence
- ✅ All endpoints tested + API contract validated
- ✅ Unit tests cover critical logic (≥80% coverage)
- ✅ Zero P0 bugs; ≤2 P1 bugs (tracked for Phase 2)
- ✅ Docker dev setup works for all team members
- ✅ Production Dockerfiles build + run without errors
- ✅ Database migrations run cleanly

### Accessibility
- ✅ App fully usable via keyboard (no mouse required)
- ✅ ARIA labels + live regions implemented
- ✅ Tested with screenreader (NVDA or equivalent)
- ✅ Focus order is logical; focus indicators visible

### Deployment Readiness
- ✅ Deployment guide is complete + tested
- ✅ Environment variable / secrets strategy documented
- ✅ Health checks working
- ✅ Docker images optimized + lean

### Documentation
- ✅ README updated with quick-start
- ✅ API documentation complete (endpoints + error codes)
- ✅ Deployment guide clear + actionable
- ✅ Known limitations documented

---

## 🔄 Dependency Chain

```
Sprint 1 Foundation (all critical path):
  Backend:  1.2 (DB) ← 1.3, 3.5, 3.6, 4.1 (all depend on 1.2)
  Frontend: 1.1 (start screen) + 2.1 (typing view) ← 2.2 (input) ← 2.3 (metrics)
  DevOps:   5.6 (Docker setup, must be ready by Wed for team to use)

Sprint 2 Enhancement (builds on Sprint 1):
  Frontend: 2.4 (keyboard) uses input from 2.2
  Backend:  4.2 (session save) uses sessions table from 4.1
  Frontend: 2.5, 2.6 (controls, completion) depend on 2.1 + 2.4

Sprint 3 Polish (refinement):
  Frontend: 3.1 (editor) uses API from 1.2
  Both:     5.1, 5.2 (accessibility) applied to all previous features
  Backend:  5.4 (tests) validate existing code
  DevOps:   5.7, 5.8 (Dockerfiles, deployment) finalize infrastructure
```

---

## 👥 Team Capacity & Velocity

**Assumption:** 3 full-time developers, 2-week sprints, 40 hours/week, 80% delivery capacity (20% overhead)

| Role | Hours/Week | Effective Delivery | Typical Capacity |
|------|------------|-------------------|------------------|
| **Backend** | 40h | 32h | 8–10 pts/sprint |
| **Frontend** | 40h | 32h | 12–15 pts/sprint |
| **DevOps** | 40h | 32h | 4–6 pts/sprint |

**Sprint 1 Allocation:** Backend 13pts, Frontend 17pts, DevOps 4pts → **All within capacity ✅**

**Sprint 2 Allocation:** Backend 8pts, Frontend 11pts, DevOps 0pts → **All within capacity ✅**

**Sprint 3 Allocation:** Backend 3pts, Frontend 11pts, DevOps 5pts → **All within capacity ✅**

**Total MVP Effort:** 60 points across 3 sprints = **20 points/sprint average** (sustainable pace)

---

## 📅 Master Timeline

```
WEEK 1 (Jan 27 – Feb 2)
├─ Mon 9 AM: Sprint 1 kick-off
├─ Mon–Fri: Backend starts 1.2 (DB), Frontend starts 1.1 + 2.1, DevOps finalizes docker-compose
├─ Wed 4 PM: Mid-sprint sync
└─ Fri: Sprint 1 midpoint review

WEEK 2 (Feb 3 – Feb 7)
├─ Mon–Fri: Backend finishes 1.2 + starts 1.3, Frontend finishes 2.1 + 2.2 + 2.3, DevOps finalizes docker-compose
├─ Fri 10 AM: Sprint 1 review (demo: start screen + typing view + DB)
├─ Fri 12 PM: Retrospective
└─ Fri EOD: Sprint 1 → Main (PR merge)

WEEK 3 (Feb 10 – Feb 16)  [SPRINT 2 START]
├─ Mon 9 AM: Sprint 2 kick-off
├─ Mon–Fri: Backend starts 4.2 + 3.3, Frontend starts 2.4 + 2.5, DevOps monitors docker perf
├─ Wed 4 PM: Mid-sprint sync
└─ Fri: Sprint 2 midpoint review

WEEK 4 (Feb 17 – Feb 21)
├─ Mon–Fri: Backend finishes 4.2 + 3.3 + 5.5, Frontend finishes 2.4 + 2.5 + 2.6, DevOps support
├─ Fri 10 AM: Sprint 2 review (demo: keyboard viz + session save + completion summary)
├─ Fri 12 PM: Retrospective
└─ Fri EOD: Sprint 2 → Main (PR merge)

WEEK 5 (Feb 24 – Mar 2)  [SPRINT 3 START]
├─ Mon 9 AM: Sprint 3 kick-off
├─ Mon–Fri: Backend starts 5.4, Frontend starts 3.1 + 3.3 + 5.1, DevOps starts 5.7 + 5.8
├─ Wed 4 PM: Mid-sprint sync
└─ Fri: Sprint 3 midpoint review

WEEK 6 (Mar 3 – Mar 7)
├─ Mon–Fri: Finalize all stories, add polish + accessibility fixes
├─ Fri 10 AM: Sprint 3 review (demo: complete MVP + accessibility + deployment guide)
├─ Fri 12 PM: Retrospective
└─ **Fri EOD: MVP LAUNCH READY ✅**
```

---

## 🚀 Next Steps for Each Role

### Backend Developer
1. Read [SPRINT_1_PLAN.md](SPRINT_1_PLAN.md) → focus on Task 1.2 (database schema)
2. Review TECHSPEC.md → database section (schema, migrations, DTOs)
3. Monday 9 AM: Start Task 1.2.1 (design & migrate `lessons` table)
4. Check [BRANCH_STRATEGY.md](BRANCH_STRATEGY.md) for git workflow

### Frontend Developer
1. Read [SPRINT_1_PLAN.md](SPRINT_1_PLAN.md) → focus on Tasks 1.1 + 2.1–2.3
2. Review TECHSPEC.md → Elmish MVU design + keyboard visualization data structure
3. Monday 9 AM: Start Task 1.1.1 (Elmish model + Msg for start screen)
4. Check [KICKOFF.md](KICKOFF.md) for daily workflow + standup times

### DevOps Developer
1. Read [SPRINT_1_PLAN.md](SPRINT_1_PLAN.md) → focus on Task 5.6 (Docker Compose)
2. Review TECHSPEC.md → Docker setup section (3-service compose, Dockerfiles)
3. Monday 9 AM: Start Task 5.6.1 (docker-compose.yml structure)
4. Ensure docker-compose.yml ready by Wednesday for team testing

---

## 📖 Reference Documentation

- [SPRINT_1_PLAN.md](SPRINT_1_PLAN.md) — Detailed task breakdown for all stories
- [TECHSPEC.md](TECHSPEC.md) — Architecture & technical decisions
- [BACKLOG.md](BACKLOG.md) — Full product backlog (30 stories)
- [BRANCH_STRATEGY.md](BRANCH_STRATEGY.md) — Git workflow + merge strategy
- [KICKOFF.md](KICKOFF.md) — Developer onboarding + daily workflow

---

**Roadmap Version:** 1.0  
**Last Updated:** January 25, 2026  
**Next Review:** Weekly during standups  
**Prepared By:** Scrum Master / Tech Lead
