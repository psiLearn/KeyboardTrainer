@echo off
REM Docker Compose Helper Script - Development (Windows)
REM Usage: docker-dev.bat [up|down|logs|clean|rebuild|shell]

setlocal enabledelayedexpansion

set PROJECT_NAME=keyboard-trainer-dev
set ENV_FILE=.env.docker.dev

if "%1"=="" (
    set COMMAND=up
) else (
    set COMMAND=%1
)

if "%COMMAND%"=="up" (
    echo 🚀 Starting Keyboard Trainer (Development)...
    docker-compose --project-name %PROJECT_NAME% --env-file %ENV_FILE% up -d
    echo ✅ Services started!
    echo.
    echo 🔗 Access points:
    echo   - Web:    http://localhost:80
    echo   - API:    http://localhost:5000/api/lessons
    echo   - DB:     localhost:5432
    echo.
    docker-compose --project-name %PROJECT_NAME% --env-file %ENV_FILE% ps
    goto :end
)

if "%COMMAND%"=="down" (
    echo 🛑 Stopping services...
    docker-compose --project-name %PROJECT_NAME% --env-file %ENV_FILE% down
    echo ✅ Services stopped
    goto :end
)

if "%COMMAND%"=="logs" (
    echo 📋 Showing logs...
    docker-compose --project-name %PROJECT_NAME% --env-file %ENV_FILE% logs -f %2
    goto :end
)

if "%COMMAND%"=="clean" (
    echo 🧹 Cleaning up (removing volumes)...
    docker-compose --project-name %PROJECT_NAME% --env-file %ENV_FILE% down -v
    echo ✅ Cleanup complete - database reset
    goto :end
)

if "%COMMAND%"=="rebuild" (
    echo 🔨 Rebuilding images...
    docker-compose --project-name %PROJECT_NAME% --env-file %ENV_FILE% build --no-cache
    echo ✅ Images rebuilt
    goto :end
)

if "%COMMAND%"=="shell" (
    if "%2"=="" (
        set SERVICE=server
    ) else (
        set SERVICE=%2
    )
    echo 🔧 Opening shell in !SERVICE! container...
    docker-compose --project-name %PROJECT_NAME% --env-file %ENV_FILE% exec !SERVICE! cmd
    goto :end
)

if "%COMMAND%"=="health" (
    echo ❤️  Checking service health...
    docker-compose --project-name %PROJECT_NAME% --env-file %ENV_FILE% ps
    echo.
    curl -s http://localhost:5000/health && echo ✅ API is healthy || echo ❌ API is not responding
    goto :end
)

echo Usage: docker-dev.bat [up^|down^|logs^|clean^|rebuild^|shell^|health]
echo.
echo Commands:
echo   up       - Start all services
echo   down     - Stop services (keeps data)
echo   logs     - Show logs
echo   clean    - Stop services and reset database
echo   rebuild  - Rebuild Docker images
echo   shell    - Open shell in container
echo   health   - Check service health

:end
endlocal
