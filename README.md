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

## Docs

- `PHASE_1_DELIVERY_SUMMARY.md` - Phase 1 summary
- `PHASE_2_BACKLOG.md` - Phase 2 tasks
- `DOCKER_DEPLOYMENT.md` - Docker quick start
- `openapi.yaml` - API contract
- `USER_GUIDE.md` - End-user guide

## API Notes

The server uses F# record/union serialization. Some responses may appear in PascalCase or camelCase.
The client is tolerant to both.
