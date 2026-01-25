#!/bin/bash

# Docker Compose Helper Script - Production
# Usage: ./docker-prod.sh [up|down|logs|backup|restore|shell]

set -e

PROJECT_NAME="keyboard-trainer-prod"
ENV_FILE=".env.docker.prod.local"

# Check if .env file exists
if [ ! -f "$ENV_FILE" ]; then
    echo "❌ Error: $ENV_FILE not found!"
    echo "Please copy .env.docker.prod and create $ENV_FILE with production values"
    exit 1
fi

case "${1:-help}" in
    up)
        echo "🚀 Starting Keyboard Trainer (Production)..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" up -d
        echo "✅ Services started!"
        echo ""
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" ps
        ;;

    down)
        read -p "⚠️  WARNING: Stop production services? (yes/no): " confirm
        if [ "$confirm" = "yes" ]; then
            echo "🛑 Stopping services..."
            docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" down
            echo "✅ Services stopped"
        else
            echo "❌ Cancelled"
        fi
        ;;

    logs)
        SERVICE="${2:-}"
        echo "📋 Showing logs (Ctrl+C to exit)..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" logs -f $SERVICE
        ;;

    backup)
        BACKUP_DIR="./backups"
        DATE=$(date +%Y%m%d_%H%M%S)
        BACKUP_FILE="$BACKUP_DIR/keyboard_trainer_$DATE.sql.gz"
        
        mkdir -p "$BACKUP_DIR"
        
        echo "💾 Backing up database..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" exec -T postgres \
            pg_dump -U keyboardtrainer keyboardtrainer | gzip > "$BACKUP_FILE"
        
        echo "✅ Backup created: $BACKUP_FILE"
        ls -lh "$BACKUP_FILE"
        ;;

    restore)
        if [ -z "$2" ]; then
            echo "❌ Please specify backup file to restore"
            echo "Usage: $0 restore <backup_file.sql.gz>"
            exit 1
        fi
        
        if [ ! -f "$2" ]; then
            echo "❌ Backup file not found: $2"
            exit 1
        fi
        
        read -p "⚠️  WARNING: Restore database from $2? This will overwrite existing data! (yes/no): " confirm
        if [ "$confirm" = "yes" ]; then
            echo "📥 Restoring database..."
            gunzip < "$2" | docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" exec -T postgres \
                psql -U keyboardtrainer keyboardtrainer
            echo "✅ Restore complete"
        else
            echo "❌ Cancelled"
        fi
        ;;

    shell)
        SERVICE="${2:-server}"
        echo "🔧 Opening shell in $SERVICE container..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" exec "$SERVICE" /bin/sh
        ;;

    update)
        echo "🔄 Updating to latest code..."
        git pull origin main
        echo "🔨 Rebuilding images..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" build
        echo "🚀 Restarting services..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" up -d
        echo "✅ Update complete"
        ;;

    health)
        echo "❤️  Checking service health..."
        docker-compose --project-name "$PROJECT_NAME" --env-file "$ENV_FILE" ps
        echo ""
        curl -s https://keyboard-trainer.example.com/health && echo "✅ API is healthy" || echo "❌ API is not responding"
        ;;

    *)
        echo "Usage: $0 {up|down|logs|backup|restore|shell|update|health} [args]"
        echo ""
        echo "Commands:"
        echo "  up       - Start all services"
        echo "  down     - Stop services (requires confirmation)"
        echo "  logs     - Show logs (e.g., './docker-prod.sh logs server')"
        echo "  backup   - Backup database to ./backups/"
        echo "  restore  - Restore database (e.g., './docker-prod.sh restore backups/file.sql.gz')"
        echo "  shell    - Open shell in container (e.g., './docker-prod.sh shell server')"
        echo "  update   - Pull latest code and update services"
        echo "  health   - Check service health"
        echo ""
        echo "⚠️  This script requires .env.docker.prod.local to exist"
        exit 1
        ;;
esac
