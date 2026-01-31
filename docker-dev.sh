#!/bin/bash

# Docker Compose Helper Script - Development
# Usage: ./docker-dev.sh [up|down|logs|clean|rebuild|shell]

set -e

PROJECT_NAME="keyboard-trainer-dev"
ENV_FILE=".env.docker.dev"

case "${1:-up}" in
    up)
        echo "🚀 Starting Keyboard Trainer (Development)..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" up -d
        echo "✅ Services started!"
        echo ""
        echo "🔗 Access points:"
        echo "  - Web:    http://localhost:80"
        echo "  - API:    http://localhost:5000/api/lessons"
        echo "  - DB:     localhost:5434"
        echo ""
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" ps
        ;;

    down)
        echo "🛑 Stopping services..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" down
        echo "✅ Services stopped"
        ;;

    logs)
        echo "📋 Showing logs (Ctrl+C to exit)..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" logs -f "${2:-}"
        ;;

    clean)
        echo "🧹 Cleaning up (removing volumes)..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" down -v
        echo "✅ Cleanup complete - database reset"
        ;;

    rebuild)
        echo "🔨 Rebuilding images..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" build --no-cache
        echo "✅ Images rebuilt"
        ;;

    shell)
        SERVICE="${2:-server}"
        echo "🔧 Opening shell in $SERVICE container..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" exec "$SERVICE" /bin/sh
        ;;

    psql)
        echo "🐘 Connecting to PostgreSQL..."
        docker exec -it keyboard-trainer-db psql -U keyboardtrainer -d keyboardtrainer -p 5434
        ;;

    health)
        echo "❤️  Checking service health..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" ps
        echo ""
        curl -s http://localhost:5000/health && echo "✅ API is healthy" || echo "❌ API is not responding"
        ;;

    *)
        echo "Usage: $0 {up|down|logs|clean|rebuild|shell|psql|health} [service]"
        echo ""
        echo "Commands:"
        echo "  up       - Start all services"
        echo "  down     - Stop services (keeps data)"
        echo "  logs     - Show logs (e.g., './docker-dev.sh logs server')"
        echo "  clean    - Stop services and reset database"
        echo "  rebuild  - Rebuild Docker images"
        echo "  shell    - Open shell in container (e.g., './docker-dev.sh shell server')"
        echo "  psql     - Connect to PostgreSQL"
        echo "  health   - Check service health"
        exit 1
        ;;
esac

