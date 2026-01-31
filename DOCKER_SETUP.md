# Docker Compose Setup Guide

## Overview

This Docker Compose configuration orchestrates the complete Keyboard Trainer application stack:
- **PostgreSQL 16** - Database
- **F# Server (ASP.NET Core 8)** - API backend
- **Nginx** - Reverse proxy & static file server

## Prerequisites

- Docker 20.10+
- Docker Compose 2.0+
- Git

## Quick Start

### 1. Development Environment

```bash
# Build client assets (required for UI)
npm install
npm run build:client

# Start all services
docker-compose --env-file .env.docker.dev up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Reset database (delete volumes)
docker-compose down -v
```

### 2. Production Environment

```bash
# Build client assets (required for UI)
npm install
npm run build:client

# Configure secrets first
cp .env.docker.prod .env.docker.prod.local
# Edit .env.docker.prod.local with production values
nano .env.docker.prod.local

# Start services
docker-compose --env-file .env.docker.prod.local up -d

# View logs
docker-compose logs -f server

# Backup database
docker exec keyboard-trainer-db pg_dump -U keyboardtrainer -p 5434 keyboardtrainer > backup.sql
```

## Service Details

### PostgreSQL Database

- **Container**: `keyboard-trainer-db`
- **Port**: `5434`
- **User**: `keyboardtrainer`
- **Password**: Set via `DB_PASSWORD` env var
- **Database**: `keyboardtrainer`
- **Volume**: `postgres_data` (persistent storage)
- **Migrations**: Auto-run from `src/Server/Database/Migrations/`

**Health Check**:
```bash
docker exec keyboard-trainer-db pg_isready -U keyboardtrainer -p 5434
```

**Connect Directly**:
```bash
psql -h localhost -p 5434 -U keyboardtrainer -d keyboardtrainer
```

### F# Server

- **Container**: `keyboard-trainer-server`
- **Port**: `5000` (internal), `5000` (host)
- **Framework**: ASP.NET Core 8
- **Health Endpoint**: `GET /health`
- **Logs**: `/app/logs`

**Health Check**:
```bash
curl http://localhost:5000/health
```

**View Logs**:
```bash
docker logs keyboard-trainer-server
docker logs -f keyboard-trainer-server
```

**API Endpoints**:
- `GET /api/lessons` - List all lessons
- `POST /api/lessons` - Create lesson
- `GET /api/lessons/{id}` - Get lesson
- `PUT /api/lessons/{id}` - Update lesson
- `DELETE /api/lessons/{id}` - Delete lesson
- `POST /api/sessions` - Submit session
- `GET /api/lessons/{id}/sessions` - Get sessions for lesson

### Nginx Reverse Proxy

- **Container**: `keyboard-trainer-nginx`
- **Port**: `80` (HTTP), `443` (HTTPS)
- **Static Files**: From `src/Client/public/`
- **API Proxy**: Routes `/api/*` to server:5000

**Configuration Files**:
- `nginx.conf` - Main Nginx configuration
- `nginx-default.conf` - Server block configuration

## Environment Variables

### Development

```env
ASPNETCORE_ENVIRONMENT=Development
DB_USER=keyboardtrainer
DB_PASSWORD=keyboardtrainer_dev
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5000
SERVER_PORT=5000
NGINX_PORT=80
```

### Production

```env
ASPNETCORE_ENVIRONMENT=Production
DB_USER=keyboardtrainer
DB_PASSWORD=<STRONG_PASSWORD>
CORS_ALLOWED_ORIGINS=https://keyboard-trainer.example.com
SERVER_PORT=5000
NGINX_PORT=80
```

## Building Images Locally

### Build All Images

```bash
docker-compose build
```

### Build Specific Service

```bash
docker-compose build server
docker-compose build nginx
```

### Build with BuildKit (faster, better caching)

```bash
DOCKER_BUILDKIT=1 docker-compose build
```

## Network Configuration

All services communicate via `keyboard-trainer-network` bridge network:

```
┌─────────────┐
│   Client    │ (Nginx - port 80)
└──────┬──────┘
       │
       └──────────────┬──────────────────┐
                      │                  │
            ┌─────────▼─────┐   ┌────────▼──────┐
            │  Server       │   │  PostgreSQL   │
            │  (port 5000)  │   │  (port 5434)  │
            └───────────────┘   └───────────────┘
```

## Health Checks

Services include health checks that Docker monitors:

```bash
# View service status
docker-compose ps

# Check specific service health
docker inspect keyboard-trainer-server --format='{{json .State.Health}}'
```

## Volumes

### Persistent Data

- `postgres_data` - PostgreSQL database files
- `./logs` - Server application logs
- `./ssl` - SSL certificates for HTTPS

### Temporary

- Nginx cache
- Build artifacts (not persisted)

## Database Migrations

Migrations run automatically on container startup:

```bash
# Files in src/Server/Database/Migrations/
001_CreateLessonsAndSessionsTables.sql
002_SeedFrenchLessons.sql
```

**Manual Migration**:
```bash
docker exec keyboard-trainer-db psql -U keyboardtrainer -p 5434 -d keyboardtrainer -f /docker-entrypoint-initdb.d/001_CreateLessonsAndSessionsTables.sql
```

## Troubleshooting

### Containers won't start

```bash
# Check logs
docker-compose logs

# Verify Docker daemon
docker ps

# Rebuild images
docker-compose build --no-cache
```

### Database connection error

```bash
# Check PostgreSQL is healthy
docker-compose ps

# Verify network connectivity
docker exec keyboard-trainer-server curl -v http://postgres:5434

# Check credentials in .env file
cat .env.docker.dev
```

### Port already in use

```bash
# Change in docker-compose.yml or .env file
NGINX_PORT=8080        # Use 8080 instead of 80
SERVER_PORT=5001       # Use 5001 instead of 5000
DB_PORT=5435           # Use 5435 instead of 5434

docker-compose up
```

### API not responding

```bash
# Check server logs
docker logs keyboard-trainer-server

# Test health endpoint
curl http://localhost:5000/health

# Check Nginx proxy config
docker exec keyboard-trainer-nginx cat /etc/nginx/conf.d/default.conf
```

### Database locked or corrupted

```bash
# Backup and reset
docker-compose down -v
docker volume rm keyboard-trainer-dev_postgres_data  # Or prod
docker-compose up -d
```

## Production Deployment

### Pre-Deployment Checklist

- [ ] Update `.env.docker.prod.local` with production secrets
- [ ] Configure SSL certificates in `./ssl/` directory
- [ ] Set strong database password
- [ ] Configure CORS_ALLOWED_ORIGINS
- [ ] Enable SSL in nginx-default.conf
- [ ] Set up log rotation
- [ ] Configure backup strategy
- [ ] Test failover procedures

### Deploy Stack

```bash
# Pull latest code
git pull origin main

# Rebuild images
docker-compose -f docker-compose.yml \
  --env-file .env.docker.prod.local \
  build

# Start services (use -d for detached)
docker-compose -f docker-compose.yml \
  --env-file .env.docker.prod.local \
  up -d

# Verify health
docker-compose -f docker-compose.yml \
  --env-file .env.docker.prod.local \
  ps

# Check logs
docker-compose -f docker-compose.yml \
  --env-file .env.docker.prod.local \
  logs -f
```

### Backup Strategy

```bash
# Daily backup script
#!/bin/bash
BACKUP_DIR="/backups/keyboard-trainer"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR

# Backup database
docker exec keyboard-trainer-db pg_dump \
  -U keyboardtrainer -p 5434 keyboardtrainer \
  > $BACKUP_DIR/db_backup_$DATE.sql

# Compress
gzip $BACKUP_DIR/db_backup_$DATE.sql

# Keep only 30 days
find $BACKUP_DIR -name "*.gz" -mtime +30 -delete
```

### Zero-Downtime Updates

```bash
# Pull latest code
git pull origin main

# Rebuild only changed service
docker-compose build --no-cache server

# Update running service (no restart needed for some changes)
docker-compose up -d server

# Verify new version
curl http://localhost:5000/health
```

## Security Considerations

### Running in Production

1. **Never commit secrets** - Use `.env.docker.prod.local` (in .gitignore)
2. **Use strong passwords** - Generate with: `openssl rand -base64 32`
3. **Enable SSL/TLS** - Uncomment HTTPS config in nginx-default.conf
4. **Restrict network access** - Use firewall rules, VPC settings
5. **Regular backups** - Automated daily backups to secure storage
6. **Monitor logs** - Centralize logs to ELK, DataDog, etc.
7. **Keep images updated** - Regular security patches

### Database Security

```bash
# Create restricted user for backups
docker exec keyboard-trainer-db createuser -U keyboardtrainer -p 5434 -l backup_user
docker exec keyboard-trainer-db psql -U keyboardtrainer -p 5434 -c "ALTER ROLE backup_user ENCRYPTED PASSWORD 'backup_pass';"
docker exec keyboard-trainer-db psql -U keyboardtrainer -p 5434 -c "GRANT CONNECT ON DATABASE keyboardtrainer TO backup_user;"
```

## Monitoring & Logging

### View Logs

```bash
# All services
docker-compose logs

# Specific service
docker-compose logs server
docker-compose logs postgres
docker-compose logs nginx

# Follow logs
docker-compose logs -f

# Last 100 lines
docker-compose logs --tail=100
```

### Container Stats

```bash
docker stats keyboard-trainer-server
docker stats keyboard-trainer-db
```

### Inside Container

```bash
# Shell into container
docker exec -it keyboard-trainer-server /bin/sh
docker exec -it keyboard-trainer-db psql -U keyboardtrainer -p 5434

# View server logs
docker exec keyboard-trainer-server tail -f /app/logs/*.log
```

## Common Tasks

### Add New Environment Variable

1. Update `.env.docker.dev` and `.env.docker.prod`
2. Reference in `docker-compose.yml` under `environment:`
3. Rebuild: `docker-compose build server`
4. Restart: `docker-compose up -d`

### Update Database Schema

1. Create migration SQL in `src/Server/Database/Migrations/`
2. Docker will auto-run on restart
3. Or manually: `docker exec keyboard-trainer-db psql -U keyboardtrainer -p 5434 -d keyboardtrainer < migration.sql`

### Scale Services

```bash
# Run multiple server instances (load balanced by Nginx)
docker-compose up -d --scale server=3

# Not recommended for PostgreSQL
```

### Test API Endpoint

```bash
# Health
curl http://localhost:80/health

# Get lessons
curl -H "Content-Type: application/json" http://localhost:80/api/lessons

# Create lesson (from server)
curl -X POST http://localhost:80/api/lessons \
  -H "Content-Type: application/json" \
  -d '{"Title":"Test","Content":"Test content","Difficulty":"Easy","ContentType":"Paragraph"}'
```

## Cleanup

```bash
# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# Remove images
docker-compose down --rmi all

# Prune unused Docker resources
docker system prune -a
```

---

**Last Updated**: 2026-01-25
**Docker Compose Version**: 3.8
**Architecture**: Multi-container orchestration with Nginx, ASP.NET Core, PostgreSQL

