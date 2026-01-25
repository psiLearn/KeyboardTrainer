# Docker Deployment Guide

## Quick Start (5 minutes)

### Prerequisites
- Docker 20.10+
- Docker Compose 2.0+

### Start the Application

**Linux/Mac**:
```bash
chmod +x docker-dev.sh
./docker-dev.sh up
```

**Windows**:
```bash
docker-dev.bat up
```

**Manual (any OS)**:
```bash
docker-compose --env-file .env.docker.dev up -d
```

### Access the Application

- **Web Application**: http://localhost:80
- **API Documentation**: http://localhost:5000/health
- **Database**: localhost:5434

### View Logs

```bash
./docker-dev.sh logs server
# or
docker-compose logs -f server postgres nginx
```

---

## Architecture

```
Internet
   ↓
┌─────────────────────┐
│   Nginx (Port 80)   │
│  - Static files     │
│  - API proxy        │
└──────────┬──────────┘
           ↓
┌─────────────────────────┐
│  F# Server (Port 5000)  │
│  - REST API             │
│  - Business logic       │
└──────────┬──────────────┘
           ↓
┌──────────────────────────┐
│ PostgreSQL (Port 5434)   │
│ - Data persistence       │
│ - Lesson management      │
│ - Session tracking       │
└──────────────────────────┘
```

---

## File Structure

```
KeyboardTrainer/
├── docker-compose.yml          # Main orchestration
├── .env.docker.dev             # Dev environment vars
├── .env.docker.prod            # Prod environment template
├── Dockerfile.server           # Server image definition
├── Dockerfile.client           # Client image definition
├── nginx.conf                  # Nginx main config
├── nginx-default.conf          # Nginx server block
├── .dockerignore               # Build exclusions
├── docker-dev.sh              # Dev helper script (Linux/Mac)
├── docker-dev.bat             # Dev helper script (Windows)
├── docker-prod.sh             # Prod helper script
├── DOCKER_SETUP.md            # Detailed setup guide
└── src/
    ├── Server/                # F# backend
    ├── Client/                # Fable frontend
    └── Shared/                # Shared domain types
```

---

## Environment Configuration

### Development

Edit `.env.docker.dev`:
```env
ASPNETCORE_ENVIRONMENT=Development
DB_PASSWORD=keyboardtrainer_dev
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5000
```

### Production

1. Copy template:
   ```bash
   cp .env.docker.prod .env.docker.prod.local
   ```

2. Edit `.env.docker.prod.local`:
   ```env
   ASPNETCORE_ENVIRONMENT=Production
   DB_PASSWORD=<GENERATE_STRONG_PASSWORD>
   CORS_ALLOWED_ORIGINS=https://keyboard-trainer.example.com
   ```

3. Never commit `.local` files (they're in .gitignore)

---

## Common Commands

### Development Workflow

```bash
# Start all services
./docker-dev.sh up

# View logs
./docker-dev.sh logs server

# Connect to database
./docker-dev.sh psql

# Open shell in server
./docker-dev.sh shell server

# Check health
./docker-dev.sh health

# Reset everything
./docker-dev.sh clean
```

### Production Deployment

```bash
# Start services
./docker-prod.sh up

# Backup database
./docker-prod.sh backup

# Restore from backup
./docker-prod.sh restore backups/keyboard_trainer_20260125_120000.sql.gz

# Update code and services
./docker-prod.sh update

# View logs
./docker-prod.sh logs
```

---

## Troubleshooting

### Services Won't Start

```bash
# Check Docker daemon
docker ps

# View full logs
docker-compose logs

# Rebuild images
docker-compose build --no-cache

# Restart
docker-compose up -d
```

### Database Connection Failed

```bash
# Verify PostgreSQL is running
docker-compose ps postgres

# Check database logs
docker-compose logs postgres

# Try connecting directly
docker-compose exec postgres psql -U keyboardtrainer -d keyboardtrainer

# Reset database
docker-compose down -v
docker-compose up -d
```

### Port Already in Use

Edit `.env.docker.dev` or `.env.docker.prod.local`:
```env
NGINX_PORT=8080        # Use 8080 instead of 80
SERVER_PORT=5001       # Use 5001 instead of 5000
DB_PORT=5433           # Use 5433 instead of 5434
```

Then restart:
```bash
docker-compose up -d
```

### API Not Responding

```bash
# Check server health
curl http://localhost:5000/health

# View server logs
docker-compose logs -f server

# Verify network connectivity
docker exec keyboard-trainer-server ping postgres

# Check environment variables
docker inspect keyboard-trainer-server | grep -A 20 '"Env"'
```

---

## Performance Tuning

### Database Performance

For production, optimize PostgreSQL:

```bash
# Connect to database
docker-compose exec postgres psql -U keyboardtrainer -d keyboardtrainer

# View connection count
SELECT datname, count(*) FROM pg_stat_activity GROUP BY datname;

# View slow queries (if enabled)
SELECT query, mean_time FROM pg_stat_statements ORDER BY mean_time DESC;
```

### Server Scaling

Run multiple server instances (requires load balancing):

```bash
# Scale to 3 instances
docker-compose up -d --scale server=3

# Nginx will load balance between them
```

### Memory Limits

Edit `docker-compose.yml`:
```yaml
services:
  server:
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M
        reservations:
          cpus: '0.25'
          memory: 256M
```

---

## Security Best Practices

### 1. Secrets Management

```bash
# Generate strong database password
openssl rand -base64 32

# Use in .env.docker.prod.local
DB_PASSWORD=<GENERATED_PASSWORD>
```

### 2. SSL/TLS Configuration

Uncomment HTTPS in `nginx-default.conf`:
```nginx
server {
    listen 443 ssl http2;
    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;
}
```

Copy certificates:
```bash
mkdir -p ssl
cp /path/to/cert.pem ssl/
cp /path/to/key.pem ssl/
```

### 3. Network Security

- Use private networks (already configured)
- Restrict database access to server only
- Use firewall rules for port access

### 4. Regular Updates

```bash
# Update base images
docker pull postgres:16-alpine
docker pull mcr.microsoft.com/dotnet/aspnet:8.0-alpine
docker pull nginx:alpine

# Rebuild and restart
docker-compose build --no-cache
docker-compose up -d
```

---

## Backup & Recovery

### Automated Daily Backups

Create `backup.sh`:
```bash
#!/bin/bash
BACKUP_DIR="/backups"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR

# Backup database
docker exec keyboard-trainer-db pg_dump \
  -U keyboardtrainer keyboardtrainer | \
  gzip > $BACKUP_DIR/db_$DATE.sql.gz

# Keep 30 days
find $BACKUP_DIR -name "db_*.sql.gz" -mtime +30 -delete
```

Add to crontab:
```bash
crontab -e
# Add: 0 2 * * * /path/to/backup.sh
```

### Restore from Backup

```bash
# Stop application
./docker-prod.sh down

# Restore database
gunzip < backups/db_20260125_020000.sql.gz | \
  docker exec -i keyboard-trainer-db psql \
  -U keyboardtrainer keyboardtrainer

# Restart
./docker-prod.sh up
```

---

## Monitoring & Logging

### View Logs

```bash
# All services
docker-compose logs

# Specific service
docker-compose logs -f server

# Last 100 lines
docker-compose logs --tail 100
```

### Container Resource Usage

```bash
# Real-time stats
docker stats

# Specific container
docker stats keyboard-trainer-server
```

### Health Checks

```bash
# API health
curl http://localhost:5000/health

# Database health
docker-compose exec postgres pg_isready -U keyboardtrainer

# Service status
docker-compose ps
```

---

## Production Checklist

- [ ] Create `.env.docker.prod.local` with production secrets
- [ ] Generate strong database password
- [ ] Configure SSL certificates
- [ ] Update CORS_ALLOWED_ORIGINS
- [ ] Enable HTTPS in nginx-default.conf
- [ ] Set up automated backups
- [ ] Configure log aggregation
- [ ] Test failover procedures
- [ ] Set resource limits
- [ ] Configure monitoring/alerting
- [ ] Document runbook for ops team

---

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Build Docker images
        run: docker-compose build
      
      - name: Push to registry
        run: docker push keyboard-trainer:latest
      
      - name: Deploy to server
        run: |
          ssh deploy@prod-server "cd /app && \
            docker pull keyboard-trainer:latest && \
            docker-compose up -d"
```

---

## Support & Resources

- **Docker Docs**: https://docs.docker.com/
- **Docker Compose**: https://docs.docker.com/compose/
- **PostgreSQL**: https://www.postgresql.org/docs/
- **Nginx**: https://nginx.org/en/docs/
- **Project Repo**: [Your repo URL]

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-25 | Initial Docker Compose setup |

---

**Last Updated**: 2026-01-25  
**Docker Compose Version**: 3.8  
**Tested With**: Docker 20.10+, Docker Compose 2.0+
