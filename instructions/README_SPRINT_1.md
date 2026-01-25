# KeyboardTrainer — Complete Sprint 1 Implementation Package

**Project:** KeyboardTrainer (F# SAFE Stack typing learning app)  
**Sprint:** 1 (MVP Foundation)  
**Duration:** Jan 27 – Feb 7, 2026 (10 working days)  
**Team:** 2–3 developers  
**Status:** ✅ READY TO IMPLEMENT

---

## 📖 Documentation Index

### Quick Reference (Start Here)

| Document | Purpose | Read Time | Audience |
|----------|---------|-----------|----------|
| **[SPRINT_1_READY.md](SPRINT_1_READY.md)** | Sprint summary & checklist | 10 min | Everyone |
| **[KICKOFF.md](KICKOFF.md)** | Developer quick-start guide | 10 min | Developers |
| **[BRANCH_STRATEGY.md](BRANCH_STRATEGY.md)** | Git workflow & branching | 15 min | Developers |

### Core Documentation (Deep Dive)

| Document | Purpose | Read Time | Audience |
|----------|---------|-----------|----------|
| **[SPRINT_1_PLAN.md](SPRINT_1_PLAN.md)** | Complete sprint plan with all tasks | 45 min | Developers, SM, PO |
| **[TECHSPEC.md](TECHSPEC.md)** | Technical architecture (8 sections) | 60 min | Developers, Architects |
| **[BACKLOG.md](BACKLOG.md)** | 30 prioritized user stories (5 epics) | 30 min | PO, Developers |

### Framework & Process

| Document | Purpose | Read Time | Audience |
|----------|---------|-----------|----------|
| **[AGENT_INSTRUCTIONS.md](AGENT_INSTRUCTIONS.md)** | Master 6-phase guide (D→A→P→B→R→M) | 20 min | PO, Scrum Master |
| **[Roles.md](Roles.md)** | Role-based prompt library | 20 min | PO, Scrum Master |
| **[workflow.md](workflow.md)** | Agile workflow diagram | 5 min | Everyone |
| **[techstack.md](techstack.md)** | Tech decision framework | 15 min | Architects |

### Product & Requirements

| Document | Purpose | Read Time | Audience |
|----------|---------|-----------|----------|
| **[app.md](app.md)** | Product specification (KeyboardTrainer) | 10 min | PO, Stakeholders |

---

## 🌳 Git Branch Structure (Ready to Implement)

### All 10 Story Branches Created

```
main (base)
├── story/1-1-start-screen         [Epic 1, Story 1] 3 pts
├── story/1-2-db-api               [Epic 1, Story 2] 5 pts ⭐ CRITICAL
├── story/1-3-seed-data            [Epic 1, Story 3] 2 pts
├── story/2-1-typing-view          [Epic 2, Story 1] 5 pts
├── story/2-2-input-capture        [Epic 2, Story 2] 5 pts
├── story/2-3-metrics              [Epic 2, Story 3] 4 pts
├── story/2-4-keyboard             [Epic 2, Story 4] 6 pts ⭐ MOST COMPLEX
├── story/2-5-controls             [Epic 2, Story 5] 2 pts
├── story/2-6-completion           [Epic 2, Story 6] 3 pts
└── story/5-6-docker               [Epic 5, Story 6] 4 pts
```

**Total:** 39 story points across 10 branches

### Recommended Merge Order

```
1️⃣  story/1-2-db-api          (DB + API foundation)
    ├─ 2️⃣  story/1-3-seed-data    (Add seed data)
    ├─ 3️⃣  story/1-1-start-screen (Start Screen UI)
    ├─ 4️⃣  story/5-6-docker       (Docker infrastructure)
    └─ 5️⃣  story/2-1-typing-view  (Typing core)
         ├─ 6️⃣  story/2-2-input-capture
         ├─ 7️⃣  story/2-3-metrics
         ├─ 8️⃣  story/2-4-keyboard
         └─ 9️⃣  story/2-5-controls
              
🔟 story/2-6-completion       (Completion summary)
```

---

## 👥 Team Assignments (Suggested)

### Backend Developer (Dev 1)
- **Stories:** 1.2 (DB+API), 1.3 (Seed), 5.6 (Docker)
- **Points:** 11 (7+4 in parallel)
- **Effort:** 3.5 days
- **Start:** Monday, Jan 27 @ Task 1.2.1 (Database schema)
- **Key Deliverable:** Working API + seeded database by Wed

### Frontend Developer (Dev 2)
- **Stories:** 1.1 (Start Screen), 2.1–2.6 (Typing experience)
- **Points:** 28
- **Effort:** 2 weeks (ongoing)
- **Start:** Monday, Jan 27 @ Task 1.1.1 (Elmish setup)
- **Key Deliverable:** Complete typing view with metrics by Fri, Feb 7

### DevOps/Backend (Dev 1 parallel or Dev 3)
- **Story:** 5.6 (Docker Compose)
- **Points:** 4
- **Effort:** 1.5 days
- **Start:** After story/1-2-db-api merged (Wed, Jan 29)
- **Key Deliverable:** Single-command Docker setup

---

## 📅 Sprint Timeline

### Week 1: Foundation (Jan 27 – Jan 31)

```
MON 27          TUE 28          WED 29          THU 30          FRI 31
├───────────────┼───────────────┼───────────────┼───────────────┼─────────
Kick-off        Mid-sprint      [1.2 API MERGE] [1.1 MERGE]     Testing
[1.2 API work   Check-in        [Docker start]  [1.3 Seed]      [2.1 start]
[1.1 Start      [1.1 progress   [5.6 Docker     [Testing]       [2.2 start]
```

**Week 1 Target:** 20 pts (50% done)

### Week 2: Typing Experience (Feb 3 – Feb 7)

```
MON 3           TUE 4           WED 5           THU 6           FRI 7
├───────────────┼───────────────┼───────────────┼───────────────┼─────────
[2.1 MERGE]     [2.2 MERGE]     [2.3 MERGE]     [2.4 MERGE]     Demo!
[2.2 typing     [2.3 metrics    [2.4 keyboard   [2.5 controls]  [2.6 summary]
[2.3 metrics    [2.4 keyboard   [Testing]       [Testing]       [Review @10AM]
                                                                [Retro @12PM]
```

**Week 2 Target:** 19 pts (remaining 50% done)

**Sprint Complete by:** Friday, Feb 7 @ 5:00 PM

---

## ✅ Definition of Done (All Stories)

Each story is complete when:

- [ ] **Acceptance Criteria:** All items in SPRINT_1_PLAN.md met
- [ ] **Code Quality:** Peer-reviewed + 1 approval required
- [ ] **Tests:** 80%+ coverage on critical logic paths
- [ ] **Integration:** Merges cleanly to `main` without conflicts
- [ ] **Bugs:** Zero P0/P1 bugs (P2 acceptable if documented)
- [ ] **Documentation:** Code comments + README updates (if needed)
- [ ] **Responsive:** Frontend works on mobile/desktop
- [ ] **Accessible:** Keyboard-navigable (Phase 2, but start thinking about it)

---

## 🎯 Sprint Success Criteria

### Minimum Success (MVP Shipped)
- ✅ All 10 stories merged to `main`
- ✅ No P0 bugs
- ✅ App runnable via `docker-compose up`
- ✅ Sprint review demo shows: lessons → typing → metrics → complete

### Target Success
- All of above +
- ✅ All metrics verify correct (manual testing)
- ✅ French diacritics fully working
- ✅ Keyboard visualization accurate
- ✅ Zero P1 bugs
- ✅ Comprehensive README

### Stretch Success
- All of above +
- ✅ Story 3.1 (Lesson Editor) started
- ✅ Unit tests with 80%+ coverage
- ✅ CI/CD pipeline working
- ✅ Performance baseline established

---

## 🚀 Getting Started (Day 1 Checklist)

### Morning (8:30 AM – Before Kick-off)

**Developers:**
1. ✅ Pull latest `main`: `git pull origin main`
2. ✅ Verify all branches exist: `git branch | wc -l` (should be 11: 10 story + main)
3. ✅ Read KICKOFF.md (10 min)
4. ✅ Read your story section in SPRINT_1_PLAN.md (15 min)

**Scrum Master:**
1. ✅ Verify sprint board created
2. ✅ Check all developers in Slack #sprint-1
3. ✅ Confirm sprint review scheduled (Fri 10 AM)
4. ✅ Prepare kick-off meeting agenda

**Product Owner:**
1. ✅ Review all acceptance criteria
2. ✅ Prepare any clarification notes
3. ✅ Set up PR review process

### 9:00 AM – Sprint Kick-off Meeting (30 min)

**Agenda:**
1. Sprint goal recap (2 min)
2. Task assignment confirmation (3 min)
3. Local setup verification (5 min)
4. Any blockers? (10 min)
5. Daily standup schedule (2 min)
6. Q&A (6 min)

### 9:30 AM – Development Begins

**Backend Dev:**
```bash
git checkout story/1-2-db-api
git checkout -b task/1-2-1-schema  # Local task branch
# Start: Task 1.2.1 — Create database schema + migrations
```

**Frontend Dev:**
```bash
git checkout story/1-1-start-screen
git checkout -b task/1-1-1-routing  # Local task branch
# Start: Task 1.1.1 — Set up Elmish project + routing
```

### Afternoon (2 PM – First Check-in)

**What to do:**
- Commit first chunk of work (even if incomplete)
- Push to task branch: `git push origin task/1-x-x-xxx`
- Report progress in Slack #sprint-1

---

## 📊 Metrics & Tracking

### Daily Tracking

**Each end-of-day (5 PM):**
- Update sprint board (move cards to In Progress/Done)
- Commit and push all work
- Update burndown chart (manual or automated)
- Post progress in Slack: "Completed X tasks, working on Y"

### Weekly Check-in

**Wednesday @ 4 PM (mid-sprint):**
- Assess velocity
- Identify any at-risk stories
- Adjust plan if needed
- Surface blockers early

### Sprint Completion

**Friday, Feb 7:**
- **10:00 AM:** Sprint Review (2 hours) — Demo + stakeholder feedback
- **12:00 PM:** Sprint Retrospective (1.5 hours) — What went well? What to improve?
- **2:00 PM:** Prepare Sprint 2 backlog (30 min)

---

## 🛠 Tech Stack Overview

### Backend
- **Language:** F#
- **Framework:** Saturn (ASP.NET Core + Giraffe)
- **Database:** PostgreSQL 15
- **ORM/Query:** Dapper (lightweight, type-safe)
- **Migration:** SQL scripts + manual runner

### Frontend
- **Language:** F# (via Fable)
- **Framework:** Elmish (MVU architecture)
- **UI:** React (via Fable)
- **Routing:** Feliz Router
- **Build:** Vite (npm)
- **Styling:** CSS + optional Tailwind

### Infrastructure
- **Container:** Docker Compose (3 services)
- **Dev Server:** Vite (client), dotnet watch (server)
- **Testing:** xUnit (backend), Fable.QUnit (frontend)

---

## 📚 Key Files to Reference During Development

### Backend Development

```
src/Server/
├── Server.fs              ← Entry point, Saturn configuration
├── Api/
│   ├── Lessons.fs         ← REST endpoints for lessons
│   └── ErrorHandler.fs    ← Error middleware
├── Domain/
│   ├── Lesson.fs          ← Domain model + validation
│   └── Errors.fs          ← ApiError types
└── Data/
    ├── Db.fs              ← Database queries
    ├── Migrations/        ← SQL migration scripts
    └── Seeds.fs           ← Initial data

src/Shared/
├── Dtos.fs                ← Lesson, Session, Error DTOs
└── Validation.fs          ← Shared validation logic
```

### Frontend Development

```
src/Client/
├── App.fs                 ← Root component (Model/Msg/Update/View)
├── Pages/
│   ├── Start.fs           ← Start Screen page
│   ├── Typing.fs          ← Typing View page
│   └── Editor.fs          ← Lesson Editor page (Sprint 2)
├── Components/
│   ├── Keyboard.fs        ← Keyboard visualization
│   ├── Metrics.fs         ← Live metrics display
│   ├── LessonText.fs      ← Text with character highlighting
│   └── Forms.fs           ← Reusable form components
├── Services/
│   ├── Api.fs             ← HTTP client
│   ├── Keyboard.fs        ← Keyboard layout data
│   └── Typing.fs          ← Typing logic (WPM, accuracy)
├── i18n/
│   └── Translations.fs    ← EN/DE text (Phase 2)
└── Style/
    └── App.css            ← Global styles
```

### Docker

```
root/
├── docker-compose.yml     ← 3-service composition
├── Dockerfile.server      ← Server image (multi-stage)
├── Dockerfile.client      ← Client image (multi-stage)
└── nginx.conf             ← SPA routing for client
```

---

## 🎓 Learning Resources

### SAFE Stack
- [Official SAFE Docs](https://safe-stack.github.io/) — Framework overview
- [SAFE Template](https://github.com/SAFE-Stack/SAFE-template) — Starter project

### Elmish
- [Elmish Docs](https://elmish.github.io/) — State management library
- [Elm Architecture](https://guide.elm-lang.org/architecture/) — Conceptual model

### Saturn
- [Saturn Docs](https://saturnframework.org/) — Server framework
- [Giraffe](https://github.com/giraffe-fsharp/Giraffe) — HTTP handler library

### Fable
- [Fable Docs](https://fable.io/) — F# to JavaScript compiler
- [Fable React](https://fable.io/docs/react) — React bindings

### F#
- [F# Docs](https://docs.microsoft.com/en-us/dotnet/fsharp/) — Language reference
- [F# Style Guide](https://github.com/fsprojects/styleguide) — Conventions

---

## 🆘 Getting Help

### During Sprint

| Issue | Contact | Time | Response |
|-------|---------|------|----------|
| Quick question | Slack #sprint-1 | Any | < 30 min |
| Blocker | Tag Scrum Master | Any | < 1 hour |
| Code review | Assign reviewer | During hours | < 24 hours |
| Architecture decision | Tech lead | Daily standup | < 2 hours |
| Scope question | Product Owner | Daily standup | < 1 hour |

### Common Issues

| Problem | Solution |
|---------|----------|
| Port 5000/3000 in use | Change in docker-compose.yml or kill process |
| DB won't connect | Check DATABASE_URL env var; ensure postgres healthcheck passes |
| npm install fails | Delete package-lock.json; try again |
| dotnet build fails | Run `dotnet clean` then `dotnet restore` |
| Git merge conflict | Use VS Code Git UI; resolve conflicts; commit |

---

## 🎉 What's Next (After Sprint 1)

### Sprint 2 (Feb 10–21, Week 3–4)

**Scope:** Polish + Persistence + Testing

- Story 3.1–3.6: Lesson Editor (create/edit/delete)
- Story 4.1–4.3: Session Persistence (track results)
- Story 5.1–5.5: Accessibility + i18n + Unit tests

**Preparation:** Backlog refinement happens during Sprint 1 retro

### Sprint 3 (Feb 24 – Mar 7, Week 5–6)

**Scope:** Production Ready

- Story 5.7–5.8: Production Dockerfiles + Deployment
- Integration tests
- Performance optimization
- Deploy to Azure App Service

---

## 📋 Final Checklist (Before Monday Morning)

**All Team Members:**
- [ ] Repository cloned & updated
- [ ] .NET SDK 8.0 installed
- [ ] Node.js 20+ installed
- [ ] Docker installed & running
- [ ] Read KICKOFF.md
- [ ] All 10 branches visible locally
- [ ] Slack notification turned on

**Backend Developer:**
- [ ] Read TECHSPEC.md sections 2–3 (DB + API)
- [ ] Identified first task (1.2.1)
- [ ] Located Dapper examples in codebase

**Frontend Developer:**
- [ ] Read TECHSPEC.md sections 4–5 (Elmish + Keyboard)
- [ ] Identified first task (1.1.1)
- [ ] Located React/Fable examples in codebase

**Scrum Master:**
- [ ] Sprint board created & shared
- [ ] Slack #sprint-1 channel setup
- [ ] Kick-off meeting scheduled
- [ ] Burndown chart template ready

**Product Owner:**
- [ ] Read acceptance criteria for all 10 stories
- [ ] PR review process documented
- [ ] Sprint review scheduled (Fri 10 AM)
- [ ] Stakeholders invited

---

## 🚀 Ready to Launch!

**All systems go. Developers, grab your branches. Let's build! 🎯**

---

**Sprint 1 Implementation Package Created:** January 25, 2026  
**Status:** ✅ READY TO BEGIN Monday, January 27 @ 9:00 AM  
**Version:** 1.0

**Total Documentation:** 13 files | ~10,000 lines | ~50,000 words  
**Branch Strategy:** 10 stories | 39 story points | 10 working days

