# Keyboard Trainer

Keyboard Trainer is a small full-stack typing practice app built with F#, Saturn/Giraffe, and a Fable/Elmish client.
It includes training aids (letter colors + on-screen keyboard with next-key and last-key highlights), Docker Compose for local dev, and a Playwright E2E suite.

## Quick Start (Docker)

1. Build client assets:

```powershell
npm install
npm run build:client
```

2. Start the stack:

```powershell
docker-compose --env-file .env.docker.dev up -d
```

3. Open:
- Web UI: `http://localhost`
- API: `http://localhost:5000`
- DB: `localhost:5434`

## Quick Start (Local Server)

Prereqs:
- .NET 8 SDK
- Node.js 18+ (for the client build)
- PostgreSQL (or use Docker)

```powershell
npm install
npm run build:client:dev

dotnet run --project src/Server/KeyboardTrainer.Server.fsproj
```

Then open `http://localhost:5000` for the API or `http://localhost` if you are serving the client via Docker/Nginx.

## Tests

Server unit tests:

```powershell
dotnet test tests/KeyboardTrainer.Server.Tests/KeyboardTrainer.Server.Tests.fsproj
```

E2E tests (includes offline + perf/load coverage):

```powershell
npm run test:e2e
```

## Lecture Creator CLI

Create lessons ("lectures") directly in PostgreSQL from the command line:

```powershell
dotnet run --project src/Tools/KeyboardTrainer.LectureCreator/KeyboardTrainer.LectureCreator.fsproj -- `
  --title "L50 Home Row Drill" `
  --difficulty A1 `
  --content-type words `
  --language French `
  --content "asdf jkl; asdf jkl;"
```

Interactive mode:

```powershell
dotnet run --project src/Tools/KeyboardTrainer.LectureCreator/KeyboardTrainer.LectureCreator.fsproj -- --interactive
```

Probability lecture from text files:

```powershell
dotnet run --project src/Tools/KeyboardTrainer.LectureCreator/KeyboardTrainer.LectureCreator.fsproj -- `
  --title "QWER Probability Drill" `
  --difficulty A1 `
  --content-type probability `
  --source-files "WordLists/french_words_1.txt,WordLists/french_words_2.txt" `
  --merge-groups "[È,E,è,e]=e|[É,é]=e" `
  --alphabet "q,w,e,r" `
  --generated-length 320 `
  --word-length 4
```

Probability lessons store the probability model JSON (`content_type = probability`).
The backend generates fresh exercise text each time a probability lesson is started (`GET /api/lessons/{id}/exercise`).

Vocabulary lesson from CSV files:

```powershell
dotnet run --project src/Tools/KeyboardTrainer.LectureCreator/KeyboardTrainer.LectureCreator.fsproj -- `
  --title "German/French Vocabulary Drill" `
  --difficulty A1 `
  --content-type words `
  --vocab-csv-files "data\\WordLists\\german_french_vocab_second_pass.csv" `
  --vocab-csv-columns "German,French"
```

Example using files in `data\texts`, with alphabet `[q,w,e,r, ]` and merges:
- `Q,q -> q`
- `W,w -> w`
- `E,é,è,ê,É,È,Ê -> e`

```powershell
dotnet run --project src/Tools/KeyboardTrainer.LectureCreator/KeyboardTrainer.LectureCreator.fsproj -- `
  --title "QWER + Space Probability (data\\texts)" `
  --difficulty A1 `
  --content-type probability `
  --source-files "data\\texts\\AmongUs.txt,data\\texts\\ClashOfClans.txt,data\\texts\\ClashRoyale.txt,data\\texts\\Fortnite.txt,data\\texts\\Minecraft.txt,data\\texts\\RocketLeague.txt" `
  --merge-groups "[Q,q]=q|[W,w]=w|[E,é,è,ê,É,È,Ê]=e" `
  --alphabet "q,w,e,r,space" `
  --generated-length 320 `
  --word-length 4
```

## Analyze text files (no lecture)

Use `--analyze` when you only need the letter distribution that feeds a probability lesson. The CLI still accepts `--source-files`, `--merge-groups`, and `--alphabet` and prints the counts instead of writing a lecture.

```powershell
dotnet run --project src/Tools/KeyboardTrainer.LectureCreator/KeyboardTrainer.LectureCreator.fsproj -- `
  --analyze `
  --source-files "data\texts\Fortnite.txt,data\texts\Minecraft.txt" `
  --merge-groups "[Q,q]=q|[W,w]=w|[E,é,è,ê,É,È,Ê]=e" `
  --alphabet "q,w,e,r,space"
```

You will see `Letter counts (after merge/alphabet rules):` followed by each letter (spaces are reported as `space`), the raw count, and its percent share.

Connection uses `DATABASE_URL` or `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`.
Defaults match local Docker setup (`localhost:5434`, `keyboardtrainer`, `keyboardtrainer_dev`).

## Docs

- `PHASE_1_DELIVERY_SUMMARY.md` - Phase 1 summary
- `PHASE_2_BACKLOG.md` - Phase 2 tasks
- `DOCKER_DEPLOYMENT.md` - Docker quick start
- `openapi.yaml` - API contract
- `USER_GUIDE.md` - End-user guide

## API Notes

The server uses F# record/union serialization. Some responses may appear in PascalCase or camelCase.
The client is tolerant to both.

Probability exercise endpoints:
- `GET /api/lessons/{id}/exercise` resolves a lesson into concrete exercise text.
- `POST /api/exercises/probability` generates text from a probability JSON payload.
