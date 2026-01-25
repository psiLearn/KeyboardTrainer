# MVP Sprint Planning — Complete Overview

**Project:** KeyboardTrainer  
**Sprint Duration:** 3 × 2-week sprints (Jan 27 – Mar 7, 2026)  
**Total Points:** 60 points across 20 stories  
**Team Size:** 3 developers (Backend, Frontend, DevOps)  
**Status:** ✅ READY FOR SPRINT 1 KICK-OFF (Monday, Jan 27, 2026 @ 9:00 AM)

---

## 📚 Complete Planning Library

This MVP sprint roadmap is organized by **role** with detailed planning for each of the 3 sprints.

### **Master Planning Documents**

1. **[MVP_SPRINT_ROADMAP.md](MVP_SPRINT_ROADMAP.md)** ← **START HERE**
   - High-level overview of all 3 sprints
   - Role-based story allocation (Backend, Frontend, DevOps)
   - Sprint 1, 2, 3 summary tables
   - Team capacity & velocity planning
   - Master timeline (all 6 weeks)
   - MVP Definition of Done criteria
   - Dependency chain diagram

2. **[SPRINT_1_PLAN.md](SPRINT_1_PLAN.md)** — Jan 27 – Feb 7
   - 10 stories, 34 points
   - Backend: 13 pts (Stories 1.2, 1.3, 3.5, 3.6, 4.1)
   - Frontend: 17 pts (Stories 1.1, 2.1, 2.2, 2.3)
   - DevOps: 4 pts (Story 5.6)
   - **Goal:** Minimum viable typing app with database + API + Docker dev setup
   - Detailed task breakdown (Task 1.2.1–1.2.4, etc.)
   - Daily workflow + weekly schedule
   - Success criteria + risk register

3. **[SPRINT_2_PLAN.md](SPRINT_2_PLAN.md)** — Feb 10 – Feb 21
   - 6 stories, 19 points
   - Backend: 8 pts (Stories 4.2, 3.3, 5.5)
   - Frontend: 11 pts (Stories 2.4, 2.5, 2.6)
   - DevOps: 0 pts (supporting role)
   - **Goal:** Keyboard visualization + Session persistence + API test coverage
   - Detailed task breakdown for complex stories (keyboard render + highlighting)
   - Risk register + burndown targets

4. **[SPRINT_3_PLAN.md](SPRINT_3_PLAN.md)** — Feb 24 – Mar 7
   - 7 stories, 19 points
   - Backend: 3 pts (Story 5.4)
   - Frontend: 11 pts (Stories 3.1, 3.3, 5.1, 5.2)
   - DevOps: 5 pts (Stories 5.7, 5.8)
   - **Goal:** Accessibility + Lesson editor + Production Dockerfiles + Deployment ready
   - Accessibility testing with screenreader (NVDA)
   - Production deployment guide
   - **Outcome:** MVP complete & production-ready ✅

---

## 🎯 Quick Reference: Story Allocation by Role

### **BACKEND DEVELOPER** (26 points across 3 sprints)

| Sprint | Stories | Points | Focus |
|--------|---------|--------|-------|
| **Sprint 1** | 1.2, 1.3, 3.5, 3.6, 4.1 | 13 | Database + API endpoints |
| **Sprint 2** | 4.2, 3.3, 5.5 | 8 | Session persistence + API tests |
| **Sprint 3** | 5.4 | 3 | Unit test coverage |
| **Total** | **10 stories** | **24 pts** | Core backend infrastructure |

**Key Deliverables:**
- Lesson CRUD API (GET/POST/PUT/DELETE /lessons)
- Session API (POST /sessions)
- PostgreSQL schema + migrations
- ≥80% API test coverage
- ≥80% typing logic unit test coverage

---

### **FRONTEND DEVELOPER** (39 points across 3 sprints)

| Sprint | Stories | Points | Focus |
|--------|---------|--------|-------|
| **Sprint 1** | 1.1, 2.1, 2.2, 2.3 | 17 | Start screen + typing view + metrics |
| **Sprint 2** | 2.4, 2.5, 2.6 | 11 | Keyboard viz + controls + completion |
| **Sprint 3** | 3.1, 3.3, 5.1, 5.2 | 11 | Lesson editor + accessibility |
| **Total** | **10 stories** | **39 pts** | Full client-side experience |

**Key Deliverables:**
- Functional start screen with lesson selection
- Typing view with live metrics (WPM, accuracy, timer)
- German QWERTZ keyboard visualization with real-time highlighting
- Session controls (restart, pause, return)
- Completion summary with results
- Lesson create/edit forms with validation
- Full keyboard-only accessibility
- ARIA labels + screenreader support

---

### **DEVOPS DEVELOPER** (9 points across 3 sprints)

| Sprint | Stories | Points | Focus |
|--------|---------|--------|-------|
| **Sprint 1** | 5.6 | 4 | Docker Compose dev environment |
| **Sprint 2** | (supporting) | — | Monitor + optimize docker setup |
| **Sprint 3** | 5.7, 5.8 | 5 | Production Dockerfiles + deployment guide |
| **Total** | **3 stories** | **9 pts** | Infrastructure & deployment |

**Key Deliverables:**
- Docker Compose for local development (3-service stack)
- Production Dockerfiles (multi-stage builds, optimized)
- Deployment guide for Azure App Service
- Pre-deployment + post-deployment checklists

---

## 📊 Sprint Breakdown

### **Sprint 1: Foundation** (Jan 27 – Feb 7)
**Goal:** "Functional typing app with database + API"

| Role | Stories | Points | Deliverables |
|------|---------|--------|--------------|
| Backend | 1.2, 1.3, 3.5, 3.6, 4.1 | 13 | Lesson CRUD API + sessions table |
| Frontend | 1.1, 2.1, 2.2, 2.3 | 17 | Start screen + typing view + metrics |
| DevOps | 5.6 | 4 | Docker dev environment |
| **TOTAL** | **10 stories** | **34 pts** | **Minimum viable typing app** |

**Success Metrics:**
- ✅ Users can view lessons + start typing
- ✅ Live metrics (WPM, accuracy, timer) working
- ✅ All API endpoints tested (CRUD)
- ✅ Team uses Docker locally by Wed Feb 5
- ✅ Zero P0 bugs

**Sprint 1 Demo (Fri Feb 7, 10 AM):**
- "Here's the lesson list → select a lesson → type → see metrics update live"
- Backend demo: API endpoints + database queries
- DevOps demo: `docker-compose up` starts everything

---

### **Sprint 2: Enhancement** (Feb 10 – Feb 21)
**Goal:** "Keyboard visualization + Session save + API tests"

| Role | Stories | Points | Deliverables |
|------|---------|--------|--------------|
| Backend | 4.2, 3.3, 5.5 | 8 | Session persistence + validation + API tests |
| Frontend | 2.4, 2.5, 2.6 | 11 | Keyboard viz + controls + completion summary |
| DevOps | — | — | Support role |
| **TOTAL** | **6 stories** | **19 pts** | **Enhanced UX + persistence** |

**Success Metrics:**
- ✅ Keyboard visualization shows next required key in real-time
- ✅ Wrong key triggers error animation (red + shake)
- ✅ Session results persist to database
- ✅ ≥10 API test cases with ≥80% coverage
- ✅ Completion summary displays all metrics
- ✅ Zero P0 bugs; ≤1 P1 bug

**Sprint 2 Demo (Fri Feb 21, 10 AM):**
- "Here's the keyboard learning real-time → type → see errors → complete lesson → see summary saved to database"
- Run API tests live; show coverage report

---

### **Sprint 3: Polish & Deploy** (Feb 24 – Mar 7)
**Goal:** "Accessibility + Lesson editor + Production ready"

| Role | Stories | Points | Deliverables |
|------|---------|--------|--------------|
| Backend | 5.4 | 3 | Unit test coverage (≥80%) |
| Frontend | 3.1, 3.3, 5.1, 5.2 | 11 | Lesson editor + keyboard accessibility + ARIA |
| DevOps | 5.7, 5.8 | 5 | Production Dockerfiles + deployment guide |
| **TOTAL** | **7 stories** | **19 pts** | **MVP complete & ready for production** |

**Success Metrics:**
- ✅ App fully usable via keyboard only
- ✅ Screenreader (NVDA) can navigate entire app
- ✅ Lesson create/edit forms working + validated
- ✅ Unit tests cover all critical logic (≥80%)
- ✅ Production Dockerfiles build + run (<5 min)
- ✅ Deployment guide tested + ready for ops
- ✅ Zero P0 bugs; zero P1 bugs (target)

**Sprint 3 Demo (Fri Mar 7, 10 AM):** 🎉
- "Here's the complete MVP: lessons, typing, metrics, keyboard, accessibility, deployed to production"
- Keyboard-only demo (no mouse)
- Screenreader demo (NVDA)
- Production deployment walkthrough
- **OUTCOME:** MVP Accepted ✅ Ready for go-live

---

## 🗓️ Master Timeline

```
WEEK 1-2 (Jan 27 – Feb 7) — SPRINT 1 [Foundation]
├─ Mon 9 AM: Sprint 1 kick-off
├─ Tue-Wed: Backend (Task 1.2.1-1.2.2), Frontend (Task 1.1.1, 2.1.1-2.1.4)
├─ Wed 4 PM: Mid-sprint sync
├─ Fri 10 AM: Sprint 1 review (demo: start screen + typing view)
├─ Fri 12 PM: Retrospective
└─ Sun EOD: Sprint 1 → Main (PR merge)

WEEK 3-4 (Feb 10 – Feb 21) — SPRINT 2 [Enhancement]
├─ Mon 9 AM: Sprint 2 kick-off
├─ Tue-Thu: Backend (Task 4.2.1-4.2.3, 3.3.1, 5.5.1), Frontend (Task 2.4.1-2.4.4)
├─ Wed 4 PM: Mid-sprint sync
├─ Fri 10 AM: Sprint 2 review (demo: keyboard viz + session save + API tests)
├─ Fri 12 PM: Retrospective
└─ Sun EOD: Sprint 2 → Main (PR merge)

WEEK 5-6 (Feb 24 – Mar 7) — SPRINT 3 [Polish & Deploy]
├─ Mon 9 AM: Sprint 3 kick-off
├─ Tue-Wed: Backend (Task 5.4.1-5.4.3), Frontend (Task 3.1.1-3.1.3, 5.1.1-5.1.3)
├─ Wed 4 PM: Mid-sprint sync (accessibility testing feedback)
├─ Fri 10 AM: Final Sprint 3 review (demo: complete MVP + accessibility)
├─ Fri 12 PM: Retrospective + Celebration 🎉
└─ **MVP LAUNCH READY ✅**
```

---

## 📚 How to Use This Planning Package

### **For Developers**

1. **Sprint 1 Start (Mon Jan 27):**
   - Read [KICKOFF.md](KICKOFF.md) (developer onboarding)
   - Read [MVP_SPRINT_ROADMAP.md](MVP_SPRINT_ROADMAP.md) (context)
   - Read [SPRINT_1_PLAN.md](SPRINT_1_PLAN.md) → your role's section
   - Reference [SPRINT_1_PLAN.md](SPRINT_1_PLAN.md) daily for task details

2. **During Sprint 1:**
   - Daily standup: 9 AM (15 min)
   - Mid-sprint sync: Wed 4 PM (30 min)
   - Check in frequently with task breakdown in SPRINT_1_PLAN.md
   - Reference [BRANCH_STRATEGY.md](BRANCH_STRATEGY.md) for git workflow

3. **Before Sprint 2 (Mon Feb 10):**
   - Read [SPRINT_2_PLAN.md](SPRINT_2_PLAN.md) → your role's section
   - Repeat process

### **For Scrum Master / Tech Lead**

1. **Before Sprint Kick-Off:**
   - Review corresponding SPRINT_X_PLAN.md
   - Prepare slide deck from MVP_SPRINT_ROADMAP.md
   - Verify team capacity matches story points
   - Identify high-risk stories (noted in risk register)

2. **During Sprint:**
   - Daily standup: capture status from SPRINT_X_PLAN.md format
   - Mid-sprint check-in: verify burndown on track
   - End of sprint: conduct review + retrospective

### **For Product Owner / Stakeholders**

1. **Before Project Start:**
   - Review [BACKLOG.md](BACKLOG.md) (30 user stories)
   - Review MVP_SPRINT_ROADMAP.md (20 stories in MVP)
   - Understand phasing: Phase 1 (MVP) vs Phase 2 (Polish) vs Phase 3 (Deploy)

2. **During MVP (3 weeks):**
   - Attend Friday sprint reviews (10 AM weekly)
   - Provide feedback on demos
   - Approve/reject completed stories

3. **After MVP (Mar 7):**
   - Launch decision
   - Phase 2 planning (if applicable)

---

## 🎯 Success Criteria & Definition of Done

### **MVP Definition of Done** (All Sprints Complete)

✅ **Functional Completeness**
- Users can view lesson list + select lesson
- Users can type with live WPM, accuracy, timer
- German QWERTZ keyboard visualizes next key
- French diacritics render + input correctly
- Completion summary shows results
- Session results persist to database
- Users can create, edit, delete lessons

✅ **Technical Excellence**
- All API endpoints tested (≥10 test cases)
- Unit tests cover critical logic (≥80% coverage)
- Zero P0 bugs; ≤2 P1 bugs (optional post-MVP)
- Docker dev setup works for all developers
- Production Dockerfiles tested + optimized

✅ **Accessibility (WCAG 2.1 AA Target)**
- App fully usable via keyboard (no mouse required)
- Screenreader can navigate (ARIA labels + live regions)
- Tab order is logical
- Focus indicators always visible

✅ **Deployment Readiness**
- Production Dockerfiles build + run
- Deployment guide complete + tested
- Environment variable / secrets strategy documented
- Health check endpoints working

✅ **Documentation**
- README updated with quick-start
- API documentation complete
- Deployment guide with pre/post checklists
- Known limitations documented

---

## 📋 Document Index

| Document | Purpose | Audience | Read Time |
|----------|---------|----------|-----------|
| **MVP_SPRINT_ROADMAP.md** | Master 3-sprint overview | Entire team | 20 min |
| **SPRINT_1_PLAN.md** | Detailed Sprint 1 tasks | Developers | 30 min |
| **SPRINT_2_PLAN.md** | Detailed Sprint 2 tasks | Developers | 30 min |
| **SPRINT_3_PLAN.md** | Detailed Sprint 3 tasks + deployment | Developers + Ops | 30 min |
| **KICKOFF.md** | Developer onboarding | Developers | 15 min |
| **BRANCH_STRATEGY.md** | Git workflow | Developers | 15 min |
| **TECHSPEC.md** | Technical architecture | Developers | 45 min |
| **BACKLOG.md** | Full product backlog (30 stories) | PO + Tech Lead | 45 min |
| **SPRINT_1_READY.md** | Sprint readiness checklist | Team | 10 min |
| **SPRINT_1_SUMMARY.md** | Executive summary | Stakeholders | 10 min |

---

## 🚀 Next Steps (RIGHT NOW)

### **Before Sprint Kick-Off (Mon Jan 27, 9 AM)**

**All Developers:**
- [ ] Read [MVP_SPRINT_ROADMAP.md](MVP_SPRINT_ROADMAP.md) (20 min)
- [ ] Read [KICKOFF.md](KICKOFF.md) (15 min)
- [ ] Install dependencies: .NET SDK 8.0, Node.js 20+, Docker, Docker Compose
- [ ] Clone repo + `git pull origin main`
- [ ] Verify `docker-compose up` works locally (tests Docker setup)
- [ ] Add calendar blocks: standups 9 AM daily, sprint reviews Fri 10 AM

**Backend Dev:**
- [ ] Read [SPRINT_1_PLAN.md](SPRINT_1_PLAN.md) → Backend section (20 min)
- [ ] Review TECHSPEC.md → Database section (15 min)
- [ ] Prepare: database migration tool + sample data

**Frontend Dev:**
- [ ] Read [SPRINT_1_PLAN.md](SPRINT_1_PLAN.md) → Frontend section (20 min)
- [ ] Review TECHSPEC.md → Elmish MVU design (15 min)
- [ ] Prepare: Fable dev environment + React knowledge

**DevOps Dev:**
- [ ] Read [SPRINT_1_PLAN.md](SPRINT_1_PLAN.md) → DevOps section (10 min)
- [ ] Review TECHSPEC.md → Docker setup section (15 min)
- [ ] Verify docker-compose.yml is in place

**Tech Lead / Scrum Master:**
- [ ] Review all SPRINT_1_PLAN.md, MVP_SPRINT_ROADMAP.md
- [ ] Create Slack channel: #sprint-1
- [ ] Schedule sprint review (Fri 10 AM) + retro (Fri 12 PM)
- [ ] Prepare kick-off presentation

---

## 💬 Questions & Support

**During Sprint 1:**
- **Questions:** Ask in daily standup (9 AM) or Slack #sprint-1
- **Blockers:** Escalate to Scrum Master if > 1 hour stuck
- **Architecture:** Consult tech lead in standup or sync

**Documentation Issues:**
- Found a typo or unclear section in planning docs? Note it → will refine after Sprint 1

---

## 📈 Expected Outcomes (By Mar 7, 2026)

✅ **Code Deployed & Tested**
- 20 user stories implemented
- 60 story points delivered
- 3 git branches merged (main branch contains everything)
- 0 P0 bugs (blocker-free)
- ≤2 P1 bugs (optional post-MVP fixes)

✅ **Team Competency**
- Team familiar with SAFE Stack (Saturn, Fable, Elmish, PostgreSQL)
- Team comfortable with F# + Elmish patterns
- Team experienced with Agile ceremonies (standup, retro, review)
- Development velocity established (~20 pts/sprint)

✅ **Product Ready**
- Typing app is functional + usable
- User can practice typing with real lessons
- App is deployed + accessible to users
- Feedback mechanism in place for Phase 2

---

**Document Version:** 1.0  
**Last Updated:** January 25, 2026  
**MVP Start Date:** Monday, January 27, 2026 @ 9:00 AM  
**MVP Target:** Friday, March 7, 2026 @ 5:00 PM  
**Status:** ✅ READY FOR TEAM

---

**Good luck, team! Let's build something great! 🚀**
