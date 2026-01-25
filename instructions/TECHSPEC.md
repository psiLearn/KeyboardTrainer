# KeyboardTrainer — Technical Specification

**Project:** KeyboardTrainer (F# SAFE Stack Typing Learning App)  
**Date:** January 25, 2026  
**Status:** Phase II Architecture Complete  
**Version:** 1.0

---

## Executive Summary

**Problem:** Language learners need an engaging, keyboard-focused typing tutor that combines German keyboard layout visualization (for muscle memory) with French practice content (for language acquisition).

**Solution:** A full-stack web application (SAFE Stack) featuring:
- Desktop/browser-based typing practice with live metrics (WPM, accuracy)
- German QWERTZ keyboard visualization with real-time key highlighting
- Lesson management (create/edit/delete French lessons)
- PostgreSQL persistence for lessons and optional session analytics
- Docker Compose local dev environment for rapid iteration

**Target Users:** Language learners (A1–C1 French), touch-typing practitioners (all ages).

**Key Success Metrics:**
- ✓ Users can complete a 5-minute typing lesson without friction
- ✓ Keyboard visualization is responsive and intuitive (< 2 sec to understand key highlights)
- ✓ Lesson creation takes < 2 minutes
- ✓ App is accessible via keyboard-only; no mouse required
- ✓ All functionality runs in Docker locally (no external dependencies)

**MVP Scope:** Start Screen + Typing View + Keyboard Visualization + Lesson Selector  
**Timeline:** 3 weeks (Sprint 1–3)  
**Team:** 2–3 full-stack developers, 1 QA engineer (part-time)

---

## 1. SAFE-Aligned Architecture Overview

### Solution Structure

```
KeyboardTrainer/
├── src/
│   ├── Server/
│   │   ├── Server.fs               # Saturn app entry point
│   │   ├── Api/
│   │   │   ├── Lessons.fs          # GET/POST/PUT/DELETE /api/lessons
│   │   │   ├── Sessions.fs         # (Optional) POST /api/sessions
│   │   │   └── ErrorHandler.fs     # Consistent error responses
│   │   ├── Domain/
│   │   │   ├── Lesson.fs           # Lesson type, validation
│   │   │   ├── Session.fs          # Session type (optional)
│   │   │   └── Errors.fs           # ApiError discriminated union
│   │   ├── Data/
│   │   │   ├── Db.fs               # PostgreSQL connection & queries (Dapper or SqlHydra)
│   │   │   ├── Migrations/         # SQL migration scripts (or FluentMigrator)
│   │   │   └── Seeds.fs            # Sample lesson data
│   │   └── Program.fs              # Configuration, middleware, app builder
│   │
│   ├── Client/
│   │   ├── Index.html              # Entry point
│   │   ├── App.fs                  # Root Elmish component (Model/Msg/Update/View)
│   │   ├── Pages/
│   │   │   ├── Start.fs            # Start Screen (lesson list, CTAs)
│   │   │   ├── Typing.fs           # Typing View (input capture, metrics, keyboard)
│   │   │   └── Editor.fs           # Lesson Editor (CRUD form)
│   │   ├── Components/
│   │   │   ├── Keyboard.fs         # Keyboard visualization component
│   │   │   ├── Metrics.fs          # Live metrics display (WPM, accuracy, timer)
│   │   │   ├── LessonText.fs       # Lesson text with char highlighting
│   │   │   └── Forms.fs            # Reusable form components
│   │   ├── Services/
│   │   │   ├── Api.fs              # HTTP client for /api/lessons, /api/sessions
│   │   │   ├── Keyboard.fs         # Keyboard layout data & highlighting logic
│   │   │   └── Typing.fs           # Typing logic (WPM calc, accuracy, error tracking)
│   │   ├── i18n/
│   │   │   └── Translations.fs     # EN/DE strings
│   │   ├── Style/
│   │   │   └── App.css             # Global styles (+ Tailwind if used)
│   │   └── Index.fs                # Fable entry point
│   │
│   ├── Shared/
│   │   ├── Dtos.fs                 # Lesson, Session, Error DTOs (shared with API)
│   │   ├── Validation.fs           # Validation rules (used by server & client)
│   │   └── Constants.fs            # Shared enums (Difficulty, ContentType, Language)
│   │
│   └── App.fsproj                  # Fake build script references, etc.
│
├── docker-compose.yml
├── Dockerfile.server
├── Dockerfile.client
├── global.json                     # .NET SDK version pinning
│
└── instructions/
    ├── AGENT_INSTRUCTIONS.md
    ├── app.md
    ├── Roles.md
    ├── workflow.md
    ├── techstack.md
    ├── BACKLOG.md
    └── TECHSPEC.md (this file)
```

### Technology Choices

| Component | Technology | Rationale |
|-----------|-----------|-----------|
| **Server Framework** | Saturn (ASP.NET Core + Giraffe) | Type-safe routing, strongly-typed handlers, SAFE ecosystem |
| **Client Framework** | Fable (F# → JavaScript) + Elmish | Pure functional MVU, immutable state, predictable app |
| **UI Rendering** | React (via Fable) | Well-tested component model, large ecosystem |
| **Styling** | CSS + optional Tailwind | Keep simple for MVP; Tailwind optional for rapid iteration |
| **Database** | PostgreSQL 15+ | Robust, open-source, good F# support, jsonb for session data |
| **ORM/Query** | Dapper or SqlHydra (F#) | Type-safe, lightweight, SAFE-aligned |
| **Client Build** | Vite + npm | Fast dev server, modern bundler, SAFE template default |
| **Routing** | Feliz Router (client) + Saturn routes (server) | Declarative, type-safe, SAFE conventions |
| **Local Dev** | Docker Compose (3 services) | Reproducible, no local postgres install needed |
| **Migrations** | SQL scripts (manual) or FluentMigrator | Simple for MVP |

---

## 2. Database Schema & Migrations

### Schema (PostgreSQL)

```sql
-- Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Lessons table
CREATE TABLE lessons (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  title VARCHAR(100) NOT NULL,
  difficulty VARCHAR(5) NOT NULL, -- A1, A2, B1, B2, C1
  content_type VARCHAR(20) NOT NULL, -- words, sentences
  language VARCHAR(5) NOT NULL DEFAULT 'FR', -- Fixed to FR
  content TEXT NOT NULL,
  tags VARCHAR(500),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Index on common queries
CREATE INDEX idx_lessons_difficulty ON lessons(difficulty);
CREATE INDEX idx_lessons_language ON lessons(language);

-- Sessions table (optional, for analytics)
CREATE TABLE sessions (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  lesson_id UUID NOT NULL,
  started_at TIMESTAMP NOT NULL,
  ended_at TIMESTAMP NOT NULL,
  wpm NUMERIC(5,2),
  cpm NUMERIC(5,2),
  accuracy NUMERIC(5,2) NOT NULL, -- Percentage 0-100
  error_count INT NOT NULL DEFAULT 0,
  per_key_errors JSONB, -- { "q": 2, "w": 1, ... }
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_lesson FOREIGN KEY (lesson_id) REFERENCES lessons(id) ON DELETE CASCADE
);

CREATE INDEX idx_sessions_lesson_id ON sessions(lesson_id);
CREATE INDEX idx_sessions_created_at ON sessions(created_at DESC);
```

### Migration Strategy

**For MVP:** Simple SQL script approach:
1. Create `src/Server/Data/Migrations/001_init_schema.sql` with above schema
2. On app startup: check if `lessons` table exists; if not, run migrations
3. Seed data: `src/Server/Data/Seeds.fs` inserts 5–7 sample French lessons on first run

**Alternative (if desired):** FluentMigrator for version-controlled migrations.

### Sample Seed Data

```fsharp
// src/Server/Data/Seeds.fs
let seedLessons (conn: IDbConnection) =
  let lessons = [
    {
      Id = Guid.NewGuid()
      Title = "Bonjour - French Greetings (Words)"
      Difficulty = A1
      ContentType = Words
      Language = FR
      Content = "bonjour bonsoir bonne nuit au revoir salut"
      Tags = "greetings,a1"
      CreatedAt = DateTime.Now
      UpdatedAt = DateTime.Now
    }
    // ... 4–6 more lessons (A1, A2, B1 difficulties)
  ]
  lessons |> List.iter (fun lesson -> insertLesson conn lesson)
```

---

## 3. API Contract

### Base URL
```
http://localhost:5000/api
```

### Endpoints

#### GET /api/lessons
Retrieve all lessons.

**Response (200 OK):**
```json
{
  "lessons": [
    {
      "id": "uuid",
      "title": "Bonjour - French Greetings",
      "difficulty": "A1",
      "contentType": "words",
      "language": "FR",
      "content": "bonjour bonsoir bonne nuit",
      "tags": "greetings,a1",
      "createdAt": "2026-01-25T10:00:00Z",
      "updatedAt": "2026-01-25T10:00:00Z"
    }
  ]
}
```

**Errors (500 Internal Server Error):**
```json
{
  "message": "Database connection failed",
  "statusCode": 500,
  "errors": {}
}
```

---

#### GET /api/lessons/{id}
Retrieve a single lesson.

**Path Parameter:** `id` (UUID)

**Response (200 OK):**
```json
{
  "id": "uuid",
  "title": "Bonjour - French Greetings",
  "difficulty": "A1",
  "contentType": "words",
  "language": "FR",
  "content": "bonjour bonsoir bonne nuit",
  "tags": "greetings,a1",
  "createdAt": "2026-01-25T10:00:00Z",
  "updatedAt": "2026-01-25T10:00:00Z"
}
```

**Error (404 Not Found):**
```json
{
  "message": "Lesson not found",
  "statusCode": 404,
  "errors": {}
}
```

---

#### POST /api/lessons
Create a new lesson.

**Request Body:**
```json
{
  "title": "French Foods (Vocabulary)",
  "difficulty": "A2",
  "contentType": "words",
  "content": "pain fromage vin pomme croissant café",
  "tags": "food,vocabulary"
}
```

**Validation (server-side):**
- `title`: 1–100 characters, required
- `difficulty`: must be A1, A2, B1, B2, or C1
- `contentType`: must be "words" or "sentences"
- `content`: 1–5000 characters, required, no control chars except newline/tab
- `tags`: optional

**Response (201 Created):**
```json
{
  "id": "new-uuid",
  "title": "French Foods (Vocabulary)",
  "difficulty": "A2",
  "contentType": "words",
  "language": "FR",
  "content": "pain fromage vin pomme croissant café",
  "tags": "food,vocabulary",
  "createdAt": "2026-01-25T11:00:00Z",
  "updatedAt": "2026-01-25T11:00:00Z"
}
```

**Location Header:** `Location: /api/lessons/{id}`

**Error (400 Bad Request):**
```json
{
  "message": "Validation failed",
  "statusCode": 400,
  "errors": {
    "title": ["Title is required"],
    "content": ["Content must not exceed 5000 characters"]
  }
}
```

---

#### PUT /api/lessons/{id}
Update a lesson.

**Path Parameter:** `id` (UUID)

**Request Body:** (same as POST)

**Response (200 OK):** (updated lesson object)

**Error (404 Not Found):** Lesson not found  
**Error (400 Bad Request):** Validation failed

---

#### DELETE /api/lessons/{id}
Delete a lesson.

**Path Parameter:** `id` (UUID)

**Response (204 No Content):** (empty)

**Error (404 Not Found):** Lesson not found

---

#### POST /api/sessions (Optional, Phase 2)
Create a session record.

**Request Body:**
```json
{
  "lessonId": "uuid",
  "startedAt": "2026-01-25T11:00:00Z",
  "endedAt": "2026-01-25T11:05:30Z",
  "wpm": 45.5,
  "cpm": 227.5,
  "accuracy": 98.5,
  "errorCount": 2,
  "perKeyErrors": { "q": 1, "w": 1 }
}
```

**Response (201 Created):** (session object with id)

---

### Error Schema

All error responses follow this structure:

```json
{
  "message": "Human-readable error message",
  "statusCode": 400,
  "errors": {
    "fieldName": ["Error detail 1", "Error detail 2"]
  }
}
```

---

## 4. Elmish MVU Design

### Model

```fsharp
type Page =
  | Start
  | Typing of lessonId: Guid
  | LessonEditor of mode: EditorMode * lessonId: Guid option

and EditorMode = Create | Edit

type TypingState = {
  LessonId: Guid
  Lesson: Lesson
  TypingText: string
  CursorPosition: int
  StartTime: System.DateTime option
  ElapsedSeconds: int
  PauseState: bool
  Errors: Map<int, char> // position -> wrong character typed
  SessionCompleted: bool
  LastKeyPressed: string option
  ShiftActive: bool
}

type EditorState = {
  Mode: EditorMode
  LessonId: Guid option
  Title: string
  Difficulty: Difficulty
  ContentType: ContentType
  TextContent: string
  Tags: string
  ValidationErrors: Map<string, string list>
  IsSubmitting: bool
}

type Model = {
  CurrentPage: Page
  AllLessons: Lesson list
  SelectedLessonId: Guid option
  TypingState: TypingState option
  EditorState: EditorState option
  UILanguage: Language // EN or DE
  Loading: Set<string> // Track async operations
  Error: string option
}

and Language = EN | DE
```

### Messages

```fsharp
type Msg =
  // Navigation
  | GoToStart
  | GoToTyping of lessonId: Guid
  | GoToEditor of EditorMode * lessonId: Guid option
  
  // Lesson list
  | FetchLessons
  | LessonsLoaded of Result<Lesson list, string>
  | SelectLesson of lessonId: Guid
  
  // Typing
  | StartTyping of Lesson
  | TypeKey of char
  | Backspace
  | TypingTimerTick
  | PauseTyping
  | ResumeTyping
  | RestartLesson
  | CompleteLesson
  
  // Keyboard interaction
  | KeyPressed of keyCode: string
  | KeyReleased of keyCode: string
  | ShiftStateChanged of bool
  
  // Editor
  | UpdateTitle of string
  | UpdateDifficulty of Difficulty
  | UpdateContentType of ContentType
  | UpdateTextContent of string
  | UpdateTags of string
  | ValidateField of fieldName: string
  | SubmitLesson
  | LessonSaved of Result<Lesson, string>
  | DeleteLesson of lessonId: Guid
  | LessonDeleted of Result<unit, string>
  | PreviewLesson
  | ClosePreview
  
  // Global
  | SwitchLanguage of Language
  | ClearError
```

### Update Function (High-Level Logic)

```fsharp
let update msg model : Model * Cmd<Msg> =
  match msg with
  | FetchLessons ->
      { model with Loading = Set.add "lessons" model.Loading },
      Cmd.ofPromise Api.getLessons () LessonsLoaded (fun ex -> LessonsLoaded (Error ex.Message))
  
  | LessonsLoaded (Ok lessons) ->
      { model with AllLessons = lessons; Loading = Set.remove "lessons" model.Loading },
      Cmd.none
  
  | StartTyping lesson ->
      let typingState = {
        LessonId = lesson.Id
        Lesson = lesson
        TypingText = ""
        CursorPosition = 0
        StartTime = None // Start on first keystroke
        ElapsedSeconds = 0
        PauseState = false
        Errors = Map.empty
        SessionCompleted = false
        LastKeyPressed = None
        ShiftActive = false
      }
      { model with CurrentPage = Typing lesson.Id; TypingState = Some typingState },
      Cmd.none
  
  | TypeKey c ->
      match model.TypingState with
      | Some state ->
          if state.SessionCompleted then model, Cmd.none
          else
            let newState = {
              state with
                TypingText = state.TypingText + string c
                CursorPosition = state.CursorPosition + 1
                StartTime = match state.StartTime with None -> Some System.DateTime.Now | t -> t
            }
            let isCorrect = state.Lesson.Content.[state.CursorPosition] = c
            let newState = 
              if isCorrect then newState
              else { newState with Errors = Map.add state.CursorPosition c newState.Errors }
            
            let isComplete = newState.TypingText = state.Lesson.Content
            let newModel = { model with TypingState = Some newState }
            if isComplete then
              newModel, Cmd.ofMsg CompleteLesson
            else
              newModel, Cmd.none
      | None -> model, Cmd.none
  
  | TypingTimerTick ->
      match model.TypingState with
      | Some state when Option.isSome state.StartTime && not state.PauseState ->
          let newState = { state with ElapsedSeconds = state.ElapsedSeconds + 1 }
          { model with TypingState = Some newState }, Cmd.none
      | _ -> model, Cmd.none
  
  | CompleteLesson ->
      match model.TypingState with
      | Some state ->
          let newState = { state with SessionCompleted = true }
          { model with TypingState = Some newState },
          Cmd.none // Optional: POST to /api/sessions
      | None -> model, Cmd.none
  
  | SwitchLanguage lang ->
      { model with UILanguage = lang },
      Cmd.none
  
  // ... more cases ...
  | _ -> model, Cmd.none
```

### Routing

**Routes:**
- `/` → Start page (lesson list)
- `/typing/:lessonId` → Typing view
- `/lessons/new` → Create lesson editor
- `/lessons/:lessonId/edit` → Edit lesson editor

**Implementation:** Feliz Router or Elmish.UrlParser

---

## 5. Keyboard Visualization Design

### German QWERTZ Layout Data Structure

```fsharp
type KeyLayout = {
  Label: string        // Display label (e.g., "Q" or "Ä")
  Code: string         // Keyboard code (e.g., "KeyQ")
  ShiftLabel: string option // Label when Shift held (e.g., Some "q")
  Row: int
  Col: int
  Width: float         // Relative width (1.0 = standard key, 1.5 = 1.5x, etc.)
}

let germanQwertzLayout : KeyLayout array = [|
  // Row 0: Numbers + special chars
  { Label = "1"; Code = "Digit1"; ShiftLabel = Some "!"; Row = 0; Col = 0; Width = 1.0 }
  { Label = "2"; Code = "Digit2"; ShiftLabel = Some "\""; Row = 0; Col = 1; Width = 1.0 }
  // ... Digit3-9, Digit0
  { Label = "ß"; Code = "Minus"; ShiftLabel = Some "?"; Row = 0; Col = 10; Width = 1.0 }
  { Label = "´"; Code = "Equal"; ShiftLabel = Some "`"; Row = 0; Col = 11; Width = 1.0 }
  
  // Row 1: QWERTZ row
  { Label = "Q"; Code = "KeyQ"; ShiftLabel = None; Row = 1; Col = 0; Width = 1.0 }
  { Label = "W"; Code = "KeyW"; ShiftLabel = None; Row = 1; Col = 1; Width = 1.0 }
  { Label = "E"; Code = "KeyE"; ShiftLabel = None; Row = 1; Col = 2; Width = 1.0 }
  { Label = "R"; Code = "KeyR"; ShiftLabel = None; Row = 1; Col = 3; Width = 1.0 }
  { Label = "T"; Code = "KeyT"; ShiftLabel = None; Row = 1; Col = 4; Width = 1.0 }
  { Label = "Z"; Code = "KeyY"; ShiftLabel = None; Row = 1; Col = 5; Width = 1.0 } // Note: Z in QWERTZ
  { Label = "U"; Code = "KeyU"; ShiftLabel = None; Row = 1; Col = 6; Width = 1.0 }
  { Label = "I"; Code = "KeyI"; ShiftLabel = None; Row = 1; Col = 7; Width = 1.0 }
  { Label = "O"; Code = "KeyO"; ShiftLabel = None; Row = 1; Col = 8; Width = 1.0 }
  { Label = "P"; Code = "KeyP"; ShiftLabel = None; Row = 1; Col = 9; Width = 1.0 }
  { Label = "Ü"; Code = "BracketLeft"; ShiftLabel = None; Row = 1; Col = 10; Width = 1.0 }
  
  // Row 2: ASDFGH row
  { Label = "A"; Code = "KeyA"; ShiftLabel = None; Row = 2; Col = 0; Width = 1.0 }
  { Label = "S"; Code = "KeyS"; ShiftLabel = None; Row = 2; Col = 1; Width = 1.0 }
  { Label = "D"; Code = "KeyD"; ShiftLabel = None; Row = 2; Col = 2; Width = 1.0 }
  { Label = "F"; Code = "KeyF"; ShiftLabel = None; Row = 2; Col = 3; Width = 1.0 }
  { Label = "G"; Code = "KeyG"; ShiftLabel = None; Row = 2; Col = 4; Width = 1.0 }
  { Label = "H"; Code = "KeyH"; ShiftLabel = None; Row = 2; Col = 5; Width = 1.0 }
  { Label = "J"; Code = "KeyJ"; ShiftLabel = None; Row = 2; Col = 6; Width = 1.0 }
  { Label = "K"; Code = "KeyK"; ShiftLabel = None; Row = 2; Col = 7; Width = 1.0 }
  { Label = "L"; Code = "KeyL"; ShiftLabel = None; Row = 2; Col = 8; Width = 1.0 }
  { Label = "Ö"; Code = "Semicolon"; ShiftLabel = None; Row = 2; Col = 9; Width = 1.0 }
  { Label = "Ä"; Code = "Quote"; ShiftLabel = None; Row = 2; Col = 10; Width = 1.0 }
  
  // Row 3: YXCVBN row (Y at start in QWERTZ)
  { Label = "Y"; Code = "KeyZ"; ShiftLabel = None; Row = 3; Col = 0; Width = 1.0 } // Y is at Z position in QWERTZ
  { Label = "X"; Code = "KeyX"; ShiftLabel = None; Row = 3; Col = 1; Width = 1.0 }
  { Label = "C"; Code = "KeyC"; ShiftLabel = None; Row = 3; Col = 2; Width = 1.0 }
  { Label = "V"; Code = "KeyV"; ShiftLabel = None; Row = 3; Col = 3; Width = 1.0 }
  { Label = "B"; Code = "KeyB"; ShiftLabel = None; Row = 3; Col = 4; Width = 1.0 }
  { Label = "N"; Code = "KeyN"; ShiftLabel = None; Row = 3; Col = 5; Width = 1.0 }
  { Label = "M"; Code = "KeyM"; ShiftLabel = None; Row = 3; Col = 6; Width = 1.0 }
  { Label = ","; Code = "Comma"; ShiftLabel = Some ";"; Row = 3; Col = 7; Width = 1.0 }
  { Label = "."; Code = "Period"; ShiftLabel = Some ":"; Row = 3; Col = 8; Width = 1.0 }
  { Label = "-"; Code = "Slash"; ShiftLabel = Some "_"; Row = 3; Col = 9; Width = 1.0 }
  
  // Space bar
  { Label = "Space"; Code = "Space"; ShiftLabel = None; Row = 4; Col = 3; Width = 6.0 }
|]
```

### Highlighting Rules

```fsharp
type KeyHighlight =
  | Normal
  | NextKey         // Bright cyan/green bg
  | LastPressed     // Flash animation
  | Error           // Red bg + shake animation
  | Correct         // Green checkmark overlay

let getKeyHighlight (keyCode: string) (nextKeyCode: string) (lastKeyCode: string option) (isError: bool) : KeyHighlight =
  if isError then KeyHighlight.Error
  elif keyCode = nextKeyCode then KeyHighlight.NextKey
  elif lastKeyCode = Some keyCode then KeyHighlight.LastPressed
  else KeyHighlight.Normal
```

### Rendering Component (Fable React)

```fsharp
let keyboardVisualization (props: KeyboardProps) : ReactElement =
  let groupedByRow = germanQwertzLayout |> Array.groupBy (fun k -> k.Row)
  
  div [ Class "keyboard-container" ] [
    for (row, keys) in groupedByRow do
      div [ Class "keyboard-row" ] [
        for key in keys do
          let highlight = getKeyHighlight key.Code props.NextKeyCode props.LastKeyCode props.IsError
          let highlightClass = match highlight with
            | NextKey -> "key-highlight-next"
            | Error -> "key-highlight-error"
            | _ -> ""
          
          div [
            Class $"key {highlightClass}"
            Style [ Width $"{key.Width * 40}px" ] // 40px base width per key
            Attr.ariaLabel $"Key {key.Label}"
          ] [
            str key.Label
          ]
      ]
  ]
```

---

## 6. Docker Compose & Dockerfiles

### docker-compose.yml

```yaml
version: '3.8'

services:
  db:
    image: postgres:15-alpine
    container_name: keyboard-trainer-db
    environment:
      POSTGRES_DB: keyboardtrainer
      POSTGRES_USER: trainer
      POSTGRES_PASSWORD: trainer123 # Change in production
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U trainer -d keyboardtrainer"]
      interval: 10s
      timeout: 5s
      retries: 5

  server:
    build:
      context: .
      dockerfile: Dockerfile.server
    container_name: keyboard-trainer-server
    environment:
      DATABASE_URL: "postgresql://trainer:trainer123@db:5432/keyboardtrainer"
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: "http://0.0.0.0:5000"
    ports:
      - "5000:5000"
    depends_on:
      db:
        condition: service_healthy
    volumes:
      - .:/app
    command: dotnet watch run

  client:
    build:
      context: .
      dockerfile: Dockerfile.client
    container_name: keyboard-trainer-client
    ports:
      - "3000:3000"
    volumes:
      - ./src/Client:/app/src/Client
      - ./src/Shared:/app/src/Shared
    command: npm run dev

volumes:
  postgres_data:

networks:
  default:
    name: keyboard-trainer-net
```

### Dockerfile.server

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 as builder
WORKDIR /build

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=builder /publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "Server.dll"]
```

### Dockerfile.client

```dockerfile
# Build stage
FROM node:20-alpine as builder
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# Runtime stage (Nginx)
FROM nginx:alpine
COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 3000
CMD ["nginx", "-g", "daemon off;"]
```

### nginx.conf (for Client SPA)

```nginx
events {}
http {
  server {
    listen 3000;
    root /usr/share/nginx/html;
    
    location / {
      try_files $uri $uri/ /index.html;
    }
    
    location ~* \.(js|css|png|jpg|gif|ico|woff|woff2)$ {
      expires 1d;
      add_header Cache-Control "public, immutable";
    }
  }
}
```

### Local Development Workflow

```bash
# Start all services
docker-compose up -d

# Watch logs
docker-compose logs -f

# Rebuild services (after code changes)
docker-compose up --build

# Access app
# Client: http://localhost:3000
# Server: http://localhost:5000
# Database: localhost:5432

# Stop services
docker-compose down

# Clean up (remove volumes)
docker-compose down -v
```

---

## 7. Implementation Plan & Milestones

### MVP Scope (3 weeks, 2–3 developers)

**Milestone 1 (Week 1): Foundation**
- Set up SAFE project scaffold
- Database schema + migrations
- Lesson CRUD API endpoints
- Start Screen page (lesson list, selection)

**Milestone 2 (Week 2): Typing Core**
- Typing View with lesson text display
- Input capture + character matching logic
- Live metrics (WPM, accuracy, time)
- German keyboard visualization

**Milestone 3 (Week 3): Polish & Deploy**
- Lesson Editor (create/edit/delete) + validation
- Session controls (pause, restart, return)
- Completion summary
- Docker Compose setup
- Basic tests (unit + API)

### Task Breakdown (see SPRINT_1_PLAN.md)

**Backend Tasks:**
1. Project setup + DB migrations (2d)
2. Lesson CRUD API (2d)
3. Seed data (0.5d)
4. Error handling middleware (1d)
5. Unit tests for validation (1d)
6. API tests for endpoints (1.5d)

**Frontend Tasks:**
1. Elmish project setup + routing (1d)
2. Start Screen page (1d)
3. Typing View skeleton (0.5d)
4. Input capture + metrics logic (2d)
5. Keyboard visualization component (1.5d)
6. Lesson Editor form (1.5d)
7. Session completion summary (0.5d)
8. i18n setup (0.5d)

**Infrastructure:**
1. Docker Compose + Dockerfiles (1d)
2. Local dev workflow docs (0.5d)

---

## 8. Getting Started Guide

### Prerequisites

- **Node.js** 20+ (npm or yarn)
- **Docker** + Docker Compose
- **.NET 8 SDK** (or pinned in global.json)
- **Git**

### Step 1: Clone & Setup SAFE Template

```bash
# Navigate to project root
cd KeyboardTrainer

# Install .NET SAFE template (if not already done)
dotnet new --install SAFE.Template

# OR: Create new SAFE project (if starting fresh)
dotnet new SAFE -o . --layout web --communication remoting
```

### Step 2: Restore & Build

```bash
# Restore dependencies
dotnet restore

# Verify build
dotnet build
```

### Step 3: Set Up Database (Docker Compose)

```bash
# Start Docker Compose services
docker-compose up -d

# Verify DB is running
docker-compose logs db

# (If using migrations tool, run migrations here)
# dotnet run -- migrate
```

### Step 4: Run Locally

```bash
# Option A: Development (watch mode)
dotnet watch run  # Runs server on :5000

# In another terminal:
cd src/Client
npm install
npm run dev      # Runs client on :3000

# Option B: Docker Compose (all-in-one)
docker-compose up

# Access app
# Client: http://localhost:3000
# Server API: http://localhost:5000/api/lessons
```

### Step 5: Verify Setup

1. Open http://localhost:3000 in browser
2. See Start Screen with lesson list (from seeded data)
3. Click "Start typing" to enter Typing View
4. Type a few characters and verify keyboard visualization + metrics appear
5. Check server logs: `docker-compose logs server`

### Troubleshooting

| Issue | Solution |
|-------|----------|
| Port 5000 or 3000 already in use | Change `ports` in docker-compose.yml or kill existing process |
| Database connection refused | Verify `docker-compose logs db`; check DATABASE_URL env var |
| "SAFE template not found" | Run `dotnet new --install SAFE.Template` first |
| npm install fails | Delete `package-lock.json`, try again |
| Hot reload not working | Ensure volumes are mounted in docker-compose.yml |

### Development Tips

- **Server changes:** Auto-reload via `dotnet watch run`
- **Client changes:** Auto-reload via Vite dev server
- **Database changes:** Restart docker-compose to re-run migrations
- **Reset DB:** `docker-compose down -v && docker-compose up -d` (deletes data)

---

## Technical Decisions (ADR-Style)

### Decision 1: Dapper for Data Access (vs. Entity Framework Core)

**Context:** Need lightweight, type-safe DB queries for SAFE app.

**Options:**
- A) Entity Framework Core (ORM, full-featured)
- B) Dapper (micro-ORM, lightweight, explicit SQL)
- C) SqlHydra (F#-native, type-safe SQL generation)

**Decision:** Dapper (Option B)

**Consequences:**
- ✓ Lightweight, performance-friendly
- ✓ Explicit SQL control
- ✓ Easy to map F# records to DB rows
- ✗ Manual query writing (more verbose than ORM)
- ✗ No automatic migrations (must manage SQL scripts)

---

### Decision 2: Vite for Client Build (vs. Webpack)

**Context:** SAFE Stack uses various bundlers; need modern, fast dev server.

**Options:**
- A) Webpack (industry standard, complex config)
- B) Vite (modern, fast, simple config)

**Decision:** Vite (Option B, default in SAFE template)

**Consequences:**
- ✓ Fast HMR (hot module replacement)
- ✓ Minimal config
- ✓ Native ES modules
- ✗ Slightly newer ecosystem (less battle-tested than Webpack)

---

### Decision 3: PostgreSQL for Database (vs. SQLite)

**Context:** Choose persistence layer for lessons and sessions.

**Options:**
- A) SQLite (simple, single-file)
- B) PostgreSQL (robust, JSONB support, scales)

**Decision:** PostgreSQL (Option B)

**Consequences:**
- ✓ JSONB for per_key_errors (structured data)
- ✓ Production-ready
- ✓ Scales if app grows
- ✗ Requires Docker or separate install
- ✗ Slightly more ops overhead

---

### Decision 4: Elmish for State Management (vs. Custom Redux-like)

**Context:** Need predictable, functional state management on client.

**Options:**
- A) Elmish (Elm-inspired, pure functional MVU)
- B) Custom React hooks + Context
- C) Redux-like reducer pattern

**Decision:** Elmish (Option A)

**Consequences:**
- ✓ Pure functional architecture
- ✓ Immutable state
- ✓ Easy to test & reason about
- ✓ Large SAFE ecosystem
- ✗ Learning curve (if new to Elm model)

---

## Non-Functional Requirements Checklist

| Requirement | MVP Status | Implementation |
|-------------|------------|-----------------|
| **Accessibility** | Phase 2 | Keyboard-only navigation, ARIA labels, screenreader support (Story 5.1–5.2) |
| **Performance** | MVP | WPM calculation should be real-time (<100ms latency); keyboard rendering <60fps |
| **Security** | MVP | Input validation, SQL injection prevention (Dapper parameterized queries), no secrets in code |
| **Logging** | MVP | Structured logs (serilog) on server; optional client error reporting |
| **Internationalization** | Phase 2 | UI language toggle (EN/DE); lesson content fixed to FR |
| **Testability** | MVP | 80%+ coverage on critical paths (WPM, accuracy, validation) |
| **Deployability** | Phase 2 | Docker images, CI/CD pipeline (GitHub Actions), deployment guide |

---

## Risk Register

| Risk | Severity | Mitigation |
|------|----------|-----------|
| Dead key input handling (French diacritics) | Medium | Test with French keyboard layout early; document browser limitations; provide paste fallback |
| Keyboard layout mapping mistakes | Medium | Create unit tests for layout; visual regression tests for keyboard viz |
| Database migration failures in prod | Medium | Automated migrations on startup; test migration rollback; maintain manual script fallback |
| Docker networking issues (db ↔ server) | Low | Use docker-compose healthchecks; clear error messages in logs |
| Scope creep (analytics, leaderboards, etc.) | Medium | Strict backlog prioritization; defer Phase 2+ features; track story points vs. velocity |

---

## Sign-Off

**Prepared by:** AI Agent (Phase II Architecture)  
**Date:** January 25, 2026  
**Status:** Ready for Sprint Planning (Phase III)  
**Next Step:** Execute SPRINT_1_PLAN.md for week 1–3 development

---

**References:**
- [SAFE Docs](https://safe-stack.github.io/)
- [Elmish Docs](https://elmish.github.io/)
- [Saturn Docs](https://saturnframework.org/)
- [Fable Docs](https://fable.io/)

