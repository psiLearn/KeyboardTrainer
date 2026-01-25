# Sprint 1 Branch Structure

**Sprint Duration:** Jan 27 – Feb 7, 2026  
**Total Stories:** 10  
**Total Branches:** 10 (one per story)

---

## Branch Naming Convention

Format: `story/{epic}-{story}-{slug}`

Example: `story/1-1-start-screen` (Epic 1, Story 1, slug: start-screen)

---

## Sprint 1 Branches

### Backend Stories

#### `story/1-2-db-api` (Story 1.2 — Persist & Fetch Lessons)
**Owner:** Backend Developer  
**Tasks:**
- [ ] Task 1.2.1 — Create database schema + migrations
- [ ] Task 1.2.2 — Implement Lesson CRUD API endpoints
- [ ] Task 1.2.3 — Implement error handling & validation
**Effort:** 5 pts | **Status:** Not Started  
**PR Check:** SQL correctness, error handling, Dapper usage

**When ready to merge:**
```bash
git checkout main
git pull origin main
git checkout story/1-2-db-api
git rebase main  # Keep history clean
git push origin story/1-2-db-api
# Create Pull Request on GitHub/GitLab
# Wait for approval, then:
git checkout main
git merge --squash story/1-2-db-api
git push origin main
git branch -d story/1-2-db-api
```

---

#### `story/1-3-seed-data` (Story 1.3 — Seed Database)
**Owner:** Backend Developer  
**Tasks:**
- [ ] Task 1.3.1 — Create seed data script
- [ ] Task 1.3.2 — Integrate seed into startup
**Effort:** 2 pts | **Status:** Not Started  
**Depends On:** story/1-2-db-api (merge first)  
**PR Check:** Seed data quality, French accuracy, idempotency

---

#### `story/5-6-docker` (Story 5.6 — Docker Compose Setup)
**Owner:** DevOps / Backend Developer  
**Tasks:**
- [ ] Task 5.6.1 — Create docker-compose.yml
- [ ] Task 5.6.2 — Create Dockerfile.server
- [ ] Task 5.6.3 — Create Dockerfile.client
- [ ] Task 5.6.4 — Document local dev workflow
**Effort:** 4 pts | **Status:** Not Started  
**Depends On:** story/1-2-db-api (needs working API)  
**PR Check:** Docker build success, service startup, port mappings

---

### Frontend Stories

#### `story/1-1-start-screen` (Story 1.1 — Display Start Screen)
**Owner:** Frontend Developer  
**Tasks:**
- [ ] Task 1.1.1 — Set up Elmish project + routing
- [ ] Task 1.1.2 — Create Start Screen component
- [ ] Task 1.1.3 — Add lesson selection UI
- [ ] Task 1.1.4 — Connect to API (stub/mock)
**Effort:** 3 pts | **Status:** Not Started  
**PR Check:** Elmish patterns, component structure, responsive layout

**Merge Flow:**
- Create PR against `main`
- Link to story/1-2-db-api (will integrate real API after merge)
- Approval required

---

#### `story/2-1-typing-view` (Story 2.1 — Typing View with Lesson Text)
**Owner:** Frontend Developer  
**Tasks:**
- [ ] Task 2.1.1 — Create Typing View component skeleton
- [ ] Task 2.1.2 — Render lesson text with character highlighting
- [ ] Task 2.1.3 — Connect typing input to state
**Effort:** 5 pts | **Status:** Not Started  
**Depends On:** story/1-1-start-screen (routing), story/1-2-db-api (API)  
**PR Check:** Component hierarchy, character matching logic, French diacritics support

---

#### `story/2-2-input-capture` (Story 2.2 — Input Capture with Dead Keys)
**Owner:** Frontend Developer  
**Tasks:**
- [ ] Task 2.2.1 — Implement robust input capture
- [ ] Task 2.2.2 — Handle dead keys and diacritics
- [ ] Task 2.2.3 — Handle backspace & special keys
**Effort:** 5 pts | **Status:** Not Started  
**Depends On:** story/2-1-typing-view (typing state)  
**PR Check:** Input handling logic, diacritics support, browser compatibility notes

---

#### `story/2-3-metrics` (Story 2.3 — Live Metrics Display)
**Owner:** Frontend Developer  
**Tasks:**
- [ ] Task 2.3.1 — Create metrics calculation logic
- [ ] Task 2.3.2 — Render metrics display component
- [ ] Task 2.3.3 — Implement timer loop
**Effort:** 4 pts | **Status:** Not Started  
**Depends On:** story/2-1-typing-view (typing state)  
**PR Check:** WPM/accuracy calculations verified, timer accuracy, performance

---

#### `story/2-4-keyboard` (Story 2.4 — German QWERTZ Keyboard Visualization)
**Owner:** Frontend Developer  
**Tasks:**
- [ ] Task 2.4.1 — Define keyboard layout data structure
- [ ] Task 2.4.2 — Render keyboard visualization
- [ ] Task 2.4.3 — Implement next-key highlighting
- [ ] Task 2.4.4 — Add error highlighting & animations
**Effort:** 6 pts | **Status:** Not Started  
**Depends On:** story/2-1-typing-view (typing state)  
**PR Check:** Layout correctness, highlighting accuracy, animations smooth

---

#### `story/2-5-controls` (Story 2.5 — Session Controls)
**Owner:** Frontend Developer  
**Tasks:**
- [ ] Task 2.5.1 — Add Restart button
- [ ] Task 2.5.2 — Add Pause/Resume button
- [ ] Task 2.5.3 — Add Return to Start button
**Effort:** 2 pts | **Status:** Not Started  
**Depends On:** story/2-1-typing-view (typing state)  
**PR Check:** State reset logic, button interactions, confirmation dialogs

---

#### `story/2-6-completion` (Story 2.6 — Completion Summary)
**Owner:** Frontend Developer  
**Tasks:**
- [ ] Task 2.6.1 — Detect lesson completion
- [ ] Task 2.6.2 — Render completion summary panel
- [ ] Task 2.6.3 — Add completion buttons
- [ ] Task 2.6.4 — Prepare for session persistence
**Effort:** 3 pts | **Status:** Not Started  
**Depends On:** story/2-3-metrics (metrics display), story/2-1-typing-view (completion detection)  
**PR Check:** Completion logic, summary layout, button routing

---

## Dependency Graph

```
main (base)
├── story/1-1-start-screen (frontend foundation)
├── story/1-2-db-api (backend foundation)
│   ├── story/1-3-seed-data
│   ├── story/5-6-docker
│   └── story/2-1-typing-view (requires API)
│       ├── story/2-2-input-capture
│       ├── story/2-3-metrics
│       │   └── story/2-6-completion
│       ├── story/2-4-keyboard
│       └── story/2-5-controls
```

**Critical Path:** story/1-2-db-api → story/2-1-typing-view → story/2-6-completion

---

## Merging Strategy

### Merge Order (Recommended)

1. **story/1-2-db-api** (Foundation: Backend API)
2. **story/1-3-seed-data** (Depends on #1)
3. **story/1-1-start-screen** (Frontend foundation, can work in parallel)
4. **story/5-6-docker** (Can work in parallel after #1)
5. **story/2-1-typing-view** (Depends on #1 & #3)
6. **story/2-2-input-capture** (Depends on #5)
7. **story/2-3-metrics** (Depends on #5)
8. **story/2-4-keyboard** (Depends on #5)
9. **story/2-5-controls** (Depends on #5)
10. **story/2-6-completion** (Depends on #7 & #5)

### Merge Conflicts Resolution

If conflicts arise during merge:
1. Pull latest `main`: `git fetch origin main`
2. Rebase branch: `git rebase main` (from story branch)
3. Resolve conflicts manually
4. Push: `git push origin story/xxx --force` (use with caution)
5. Re-request PR review

### Code Review Checklist (All PRs)

- [ ] Acceptance criteria met (100% from SPRINT_1_PLAN.md)
- [ ] Code passes peer review
- [ ] Tests added/updated (if applicable)
- [ ] No P0/P1 bugs (P2 acceptable if documented)
- [ ] Documentation updated (if applicable)
- [ ] Follows SAFE/F# conventions
- [ ] CI/CD pipeline passes (if configured)
- [ ] Responsive & accessible (frontend)

---

## Daily Workflow

### Start of Day (per developer)

```bash
cd KeyboardTrainer

# Pull latest changes
git fetch origin main

# Start working on your story branch
git checkout story/YOUR-STORY

# Create a local feature branch if working on specific task
git checkout -b task/YOUR-TASK

# Commit often with clear messages
git commit -m "task/1-2-1: add database migrations script"
```

### Before Creating PR

```bash
# Make sure you're up-to-date with main
git fetch origin main
git rebase main

# Push to origin
git push origin story/YOUR-STORY

# Test locally (run docker-compose if backend)
docker-compose up
npm run dev  # if frontend

# Verify acceptance criteria met
# (checklist in SPRINT_1_PLAN.md)
```

### Creating Pull Request

On GitHub/GitLab:
- **Title:** `[Story 1.2] Persist & Fetch Lessons from PostgreSQL`
- **Description:** Include:
  - Which tasks completed
  - How to test locally
  - Any known issues or limitations
  - Links to related issues/PRs
- **Reviewers:** Assign 1–2 team members
- **Labels:** `story/1-2`, `backend`, `ready-for-review`

---

## Tracking Progress

### Weekly Status (Every Wed & Fri)

```bash
# See which branches have changes since main
git fetch origin main
git branch -vv

# Example output:
# story/1-2-db-api          abc1234 [origin/main: ahead 3] Implement CRUD API
# story/1-3-seed-data       xyz7890 [origin/main: ahead 1] Add seed data
```

### Burndown Tracker

```
Sprint Capacity: 39 pts
Week 1 (Jan 27-31):
- Mon: 0 pts done
- Tue: 5 pts (story/1-2-db-api PR pending)
- Wed: 12 pts (story/1-1-start-screen, story/1-2-db-api merged)
- Thu: 20 pts (story/2-1-typing-view in progress)
- Fri: 27 pts (expected, end-of-week sync)

Week 2 (Feb 3-7):
- Mon: 30 pts (story/2-2-input-capture merged)
- Tue: 34 pts
- Wed: 37 pts
- Thu: 39 pts (SPRINT COMPLETE!)
```

---

## Getting Started

### Prerequisites (All Developers)

```bash
# 1. Ensure .NET SDK 8.0 is installed
dotnet --version

# 2. Install SAFE template
dotnet new --install SAFE.Template

# 3. Ensure Node.js 20+ is installed
node --version
npm --version

# 4. Ensure Docker is running
docker ps

# 5. Clone repo (if not already done)
git clone https://github.com/YOUR-REPO/KeyboardTrainer.git
cd KeyboardTrainer
```

### Setup for First Time

```bash
# Restore .NET dependencies
dotnet restore

# Install npm dependencies
cd src/Client
npm install
cd ../..

# Verify setup
dotnet build
```

---

## Common Git Commands (Reference)

```bash
# List all branches
git branch -a

# Switch to a branch
git checkout story/1-2-db-api

# Create and switch to new branch
git checkout -b task/1-2-1-schema

# Commit changes
git commit -m "task/1-2-1: create lessons table schema"

# Push to origin
git push origin story/1-2-db-api

# Create pull request (CLI, if using GitHub)
gh pr create --base main --title "[Story 1.2] DB & API" --body "Description here"

# Update branch from main (rebase)
git fetch origin main
git rebase main

# Squash commits before merge
git rebase -i main

# Merge locally and push (after PR approval)
git checkout main
git merge --squash story/1-2-db-api
git commit -m "Merge story/1-2-db-api: DB & Lesson API endpoints"
git push origin main
```

---

## Contact & Questions

- **Scrum Master / Questions:** Ask in sprint standup or Slack
- **Blocker Escalation:** DM Scrum Master immediately
- **Code Review Delays:** Ping reviewers in PR comments
- **Merge Conflicts:** Ask for help before force-pushing

---

**Branch Structure Created:** January 25, 2026  
**Sprint 1 Status:** ✓ Ready to Begin (branches initialized)  
**Next Step:** Developers check out their assigned story branches and begin Task 1.1.1, 1.2.1, etc.

