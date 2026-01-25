# Sprint 1 Implementation Kickoff

**Date:** January 25, 2026  
**Sprint Start:** Monday, January 27, 2026  
**Team:** 2–3 developers  
**Total Branches:** 10 (one per story)

---

## 🚀 Quick Start for Developers

### Setup (Run Once)

```bash
cd C:\Users\siebk\fsharp\KeyboardTrainer

# Ensure you're on main
git checkout main

# Update from origin
git pull origin main

# Verify all sprint branches exist
git branch | findstr story

# You should see:
#   story/1-1-start-screen
#   story/1-2-db-api
#   story/1-3-seed-data
#   story/2-1-typing-view
#   story/2-2-input-capture
#   story/2-3-metrics
#   story/2-4-keyboard
#   story/2-5-controls
#   story/2-6-completion
#   story/5-6-docker
```

---

## 📋 Task Assignment (Suggested)

### Backend Developer (2–2.5 days Week 1)

**Assigned Stories:**
- Story 1.2: Persist & Fetch Lessons (DB + API) — 5 pts
- Story 1.3: Seed Database — 2 pts
- Story 5.6: Docker Compose Setup — 4 pts (parallel)

**Branch:** `story/1-2-db-api`

**Start Here:**
```bash
git checkout story/1-2-db-api
git pull origin main

# You're now ready for:
# - Task 1.2.1: Create database schema + migrations
# - Task 1.2.2: Implement Lesson CRUD API
# - Task 1.2.3: Error handling & validation
```

---

### Frontend Developer (2–2.5 days Week 1)

**Assigned Stories:**
- Story 1.1: Start Screen — 3 pts
- Story 2.1: Typing View — 5 pts (start mid-week)
- Story 2.2: Input Capture — 5 pts (start late week)

**Primary Branch:** `story/1-1-start-screen`  
**Secondary Branch:** `story/2-1-typing-view`

**Start Here:**
```bash
git checkout story/1-1-start-screen
git pull origin main

# You're now ready for:
# - Task 1.1.1: Set up Elmish project + routing
# - Task 1.1.2: Create Start Screen component
# - Task 1.1.3: Add lesson selection UI
# - Task 1.1.4: Connect to API (stub/mock)
```

---

### DevOps / Backend Developer (optional parallel track)

**Assigned Story:**
- Story 5.6: Docker Compose Setup — 4 pts

**Branch:** `story/5-6-docker`

**Start After:** story/1-2-db-api is merged (needs working API)

**Start Here:**
```bash
git checkout story/5-6-docker
git pull origin main

# You're now ready for:
# - Task 5.6.1: Create docker-compose.yml
# - Task 5.6.2: Create Dockerfile.server
# - Task 5.6.3: Create Dockerfile.client
# - Task 5.6.4: Document local dev workflow
```

---

## 📅 Daily Workflow

### Morning Standup (9:00 AM)

1. What did I accomplish yesterday?
2. What am I working on today?
3. Any blockers?

### During Day

```bash
# 1. Start your branch (if not already)
git checkout story/YOUR-STORY

# 2. Create a local task branch for today's work
git checkout -b task/1-2-1-schema-migrations

# 3. Make changes, test locally
# (edit files, run docker-compose/npm, etc.)

# 4. Commit frequently with clear messages
git add src/Server/Data/Migrations/
git commit -m "task/1-2-1: create lessons table schema and migrations"

# 5. Push to origin (daily backup)
git push origin task/1-2-1-schema-migrations

# 6. Before end of day, push story branch
git checkout story/1-2-db-api
git merge task/1-2-1-schema-migrations
git push origin story/1-2-db-api
```

### End of Day

```bash
# 1. Ensure changes are pushed
git push origin story/YOUR-STORY

# 2. Update sprint board (Trello/GitHub Projects/etc.)
# Mark task as "in progress" or "done"

# 3. Document any blockers in Slack
# Example: "Task 1.2.1 blocked by missing .NET template docs"
```

---

## ✅ Definition of Ready (Start of Task)

Before starting any task, ensure:
- [ ] You have the task details from SPRINT_1_PLAN.md
- [ ] You understand the acceptance criteria
- [ ] You have the branch checked out
- [ ] You can build/run locally (dotnet build, npm install, etc.)
- [ ] You have a test/verification plan

---

## 🔄 Creating a Pull Request (PR)

### When Task is Complete

```bash
# 1. Ensure all work is committed
git status  # Should show "nothing to commit"

# 2. Update story branch with latest main
git fetch origin main
git rebase main

# 3. If conflicts, resolve them
# git status  # Shows conflicts
# (edit files to resolve)
# git add -A
# git rebase --continue

# 4. Push to origin
git push origin story/YOUR-STORY --force-with-lease

# 5. Create PR on GitHub/GitLab
# Title: [Story 1.2] Persist & Fetch Lessons from PostgreSQL
# Description (include):
# - Tasks completed
# - How to test
# - Known issues
# - Links to related PRs
```

### PR Checklist

- [ ] All acceptance criteria met
- [ ] Code review requested (1–2 reviewers)
- [ ] Tests added/updated
- [ ] No P0/P1 bugs
- [ ] Documentation updated
- [ ] Follows SAFE/F# conventions

---

## 🎯 Story & Task Overview

### Week 1 Goals (Jan 27 – Feb 1)

| Story | Owner | Status | Target | Notes |
|-------|-------|--------|--------|-------|
| 1.1 | FE | 🟠 To Do | Wed | Start Screen + routing |
| 1.2 | BE | 🟠 To Do | Wed | DB + API (critical path) |
| 1.3 | BE | 🟠 To Do | Thu | Seed data |
| 2.1 | FE | 🟠 To Do | Fri | Typing View (depends on 1.2) |
| 2.2 | FE | 🟠 To Do | Next | Input capture (depends on 2.1) |
| 2.3 | FE | 🟠 To Do | Next | Metrics (depends on 2.1) |
| 2.4 | FE | 🟠 To Do | Next | Keyboard viz (depends on 2.1) |
| 2.5 | FE | 🟠 To Do | Next | Session controls (depends on 2.1) |
| 2.6 | FE | 🟠 To Do | Next | Completion (depends on 2.3) |
| 5.6 | DevOps | 🟠 To Do | Thu | Docker (depends on 1.2) |

Legend: 🟠 To Do | 🟡 In Progress | 🟢 Done

---

## 🛠 Useful Git Commands

### View your branch status

```bash
# Show current branch
git branch -v

# Show which branches have unpushed commits
git branch -v | findstr -v origin

# Show commits not in main
git log main..story/YOUR-STORY
```

### Keep branch up-to-date with main

```bash
# Fetch latest main
git fetch origin main

# Rebase your branch on main (preferred for clean history)
git rebase main

# OR merge main into your branch (alternative)
git merge main
```

### Undo mistakes

```bash
# Undo last commit (keep changes)
git reset --soft HEAD~1

# Undo last commit (discard changes)
git reset --hard HEAD~1

# Undo pushed commit (be careful!)
git revert HEAD~1  # Creates new commit that undoes changes
git push origin story/YOUR-STORY
```

### View changes

```bash
# Show uncommitted changes
git diff

# Show changes in current branch vs main
git diff main..story/YOUR-STORY

# Show commit history
git log story/YOUR-STORY -10
```

---

## 🚨 Blocker Escalation

**If you're blocked > 1 hour:**

1. **Post in Slack** with context:
   - What are you trying to do?
   - What's blocking you?
   - What have you tried?

2. **Scrum Master will help** within 30 min with:
   - Architecture clarification
   - Dependency unblocking
   - Pair programming

3. **If blocker > 24 hours**:
   - Escalate to PO
   - Re-prioritize sprint scope if needed

---

## 📚 Key Documentation

**Read Before Starting:**

1. [SPRINT_1_PLAN.md](SPRINT_1_PLAN.md) — Full sprint plan with task breakdown
2. [TECHSPEC.md](TECHSPEC.md) — Architecture, API contract, database schema
3. [BRANCH_STRATEGY.md](BRANCH_STRATEGY.md) — Branching guide & merge order
4. [app.md](app.md) — Product specification & requirements
5. [BACKLOG.md](BACKLOG.md) — Full user stories & acceptance criteria

**Tech References:**

- [SAFE Docs](https://safe-stack.github.io/) — Framework overview
- [Elmish Docs](https://elmish.github.io/) — State management
- [Saturn Docs](https://saturnframework.org/) — Server framework
- [Fable Docs](https://fable.io/) — F# to JavaScript

---

## ✨ Tips for Success

1. **Commit often** (every 30 min—1 hour) with clear messages
2. **Test locally** before pushing (run `dotnet build` or `npm run dev`)
3. **Ask questions early** — don't wait until stuck
4. **Review your own code first** before requesting review
5. **Keep PRs focused** — one story per PR, not multiple
6. **Comment complex logic** — help reviewers understand
7. **Update sprint board daily** — keep team informed

---

## 🎓 F# / SAFE Learning Resources

**If new to SAFE Stack:**

- `src/Server/Server.fs` — Example Saturn routes & error handling
- `src/Client/App.fs` — Example Elmish Model/Msg/Update/View
- `src/Shared/Dtos.fs` — Example typed DTOs

**In-Team Mentoring:**
- Pair program with experienced team member
- Review existing code before writing new
- Ask for code review feedback early and often

---

## 🏁 Sprint Success Checklist

By end of Sprint 1 (Feb 7), we should have:

- ✅ All 10 stories completed and merged to `main`
- ✅ App running locally via `docker-compose up`
- ✅ Start Screen loads with lessons from DB
- ✅ Can click "Start typing" → Typing View
- ✅ Can type, see metrics, keyboard highlights next key
- ✅ Can complete lesson → summary panel
- ✅ No P0/P1 bugs
- ✅ Code reviewed and approved
- ✅ Demo-ready for sprint review

---

## 📞 Quick Contacts

- **Scrum Master:** Slack channel #sprint-1
- **Product Owner:** Ask questions in standup
- **Backend Lead:** DM for architecture questions
- **Frontend Lead:** DM for component questions
- **DevOps:** DM for Docker/infrastructure issues

---

**Sprint 1 Begins:** Monday, January 27, 2026 @ 9:00 AM  
**First Standup:** Monday, January 27, 2026 @ 9:00 AM  
**Sprint Review:** Friday, February 7, 2026 @ 10:00 AM

**Ready? Let's build! 🚀**

