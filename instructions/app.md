You are a senior F# SAFE Stack engineer and product designer. Produce a complete, implementation-ready specification and starter scaffold plan for a full-stack web app.

Project
Build a Typing Learning Web App that:
- Uses a German keyboard layout (QWERTZ, German key labels) for the on-screen keyboard visualization.
- Uses French words and sentences as typing content to learn touch typing.
- Provides three primary views:
  1) Start Screen
  2) Typing View with keyboard visualization
  3) Lesson Editor (create/edit lessons)

Tech stack and constraints
- Use SAFE Stack (Saturn server on ASP.NET Core/Giraffe, Fable client, Elmish MVU).
- Use PostgreSQL for persistence.
- Use Docker Compose to run frontend, backend, and database locally.
- Provide a production-minded structure, but focus first on a working MVP.

Functional requirements
1) Start Screen
   - Show app title and short instructions.
   - Show CTA buttons: “Start typing”, “Create lesson”, “Continue last session” (if a session exists).
   - Allow selecting a lesson from a list (title, difficulty, length, language tag “FR”).
   - Optional filters: difficulty (A1/A2/B1/B2/C1), content type (words vs sentences).

2) Typing View
   - Display the active lesson text (French words/sentences) with:
     - Current character highlighted
     - Correct/incorrect styling for typed characters
     - Support for French diacritics (é, è, ê, ë, à, â, î, ï, ô, ù, û, ü, ç) and punctuation (’ « » …).
   - Capture typing input robustly (handle dead keys, IME edge cases if feasible).
   - Display live metrics:
     - WPM (or CPM), accuracy %, error count, time elapsed
     - Optional: streak, per-key error heatmap
   - Keyboard visualization:
     - Render a German QWERTZ keyboard with correct key positions and labels.
     - Highlight the next required key (and optionally show shift state).
     - Highlight last pressed key; show error state on wrong key.
     - Provide a toggle for showing finger hints (optional).
   - Session controls:
     - Restart lesson
     - Pause/resume
     - Return to start screen
   - At completion: summary panel (time, accuracy, WPM/CPM, error hotspots) and “Try again” / “Next lesson”.

3) Lesson Editor View
   - Create a new lesson with fields:
     - title (string), difficulty (enum), contentType (words|sentences), language fixed to FR
     - textContent (multi-line), optional tags
   - Validation:
     - Must not be empty; enforce max length; normalize line endings; reject disallowed control chars.
   - CRUD:
     - Create, list, edit, delete lessons.
   - Preview mode: show the lesson as it will appear in Typing View (including special characters).

Data requirements (PostgreSQL)
- Persist lessons in Postgres:
  - lessons table with id (UUID), title, difficulty, content_type, language, content, created_at, updated_at
- (Optional but recommended) Persist typing sessions:
  - sessions table with id, lesson_id, started_at, ended_at, wpm/cpm, accuracy, error_count, per_key_errors JSONB
- Provide SQL migrations (e.g., via a migration tool or SQL scripts run on startup) and seed data with a few French lessons.

API requirements
- Define typed DTOs in Shared and expose APIs from Server:
  - GET /api/lessons
  - GET /api/lessons/{id}
  - POST /api/lessons
  - PUT /api/lessons/{id}
  - DELETE /api/lessons/{id}
  - (Optional) POST /api/sessions to store results
- Provide consistent error schema (problem details style) and validation responses.

Frontend (Fable + Elmish)
- Use MVU architecture:
  - Model: current route, selected lesson, typing state (cursor position, typed text, errors), lesson editor state, async loading states
  - Msg: navigation, fetch lessons, create/update/delete lesson, key press events, start/stop session, etc.
  - Update: pure state transitions; side effects via Cmd
  - View: Start, Typing, LessonEditor components
- Routing between the three views.
- Handle French characters input correctly; document any browser limitations.
- Keyboard visualization should be data-driven (layout map) rather than hard-coded DOM per key.

Docker Compose (required)
- Provide docker-compose.yml with services:
  1) db: postgres (with volume, env vars, healthcheck)
  2) server: SAFE server container, depends_on db
  3) client: SAFE client dev server (Vite) container or a combined web service depending on approach
- Provide a clear local dev workflow:
  - one command to start everything
  - ports and URLs
  - environment variables for DB connection
- Include a Dockerfile (or multiple) appropriate for local dev and a production build note.

Quality and non-functional requirements
- Accessibility: keyboard-only, ARIA for key highlights, readable contrast
- Internationalization: UI language can be EN or DE, but typed content is FR
- Security basics: input validation, safe SQL, least-privileged DB user
- Observability: structured logs in server; simple client error reporting
- Testing:
  - unit tests for text processing/typing logic (client and/or shared)
  - server API tests for lesson CRUD
  - (Optional) integration test with Postgres container

Deliverables you must output
1) A concise architecture overview aligned with SAFE Stack conventions (Server/Client/Shared).
2) Database schema (SQL) + migration/seed strategy.
3) API contract: endpoints, request/response DTOs, error schema.
4) Elmish design: key Model/Msg/Update breakdown and routing approach.
5) Keyboard visualization design: German QWERTZ layout mapping (data structure) and highlight rules including shift/altgr handling.
6) Docker Compose + Dockerfiles: describe the service topology, ports, env vars, volumes, and how to run.
7) Implementation plan: milestones (MVP first), ordered backlog with user stories + acceptance criteria.
8) “Getting started” steps for a new repo using SAFE template, including the exact commands at a high level (do not invent version numbers; use placeholders).

Output format rules
- Use headings and bullet lists.
- Provide code blocks for SQL and docker-compose.yml skeletons (not full production-hardening unless needed).
- If any detail is uncertain (e.g., template defaults), label it “uncertain” and give a safe alternative.
- Do not include external links unless explicitly requested.

Inputs (fill in if missing; otherwise assume sensible defaults)
- Target users/age group: <...>
- Preferred UI language: <EN/DE>
- Lesson difficulty scale: <A1–C1 or simple 1–5>
- Any hosting target beyond local Docker: <...>
