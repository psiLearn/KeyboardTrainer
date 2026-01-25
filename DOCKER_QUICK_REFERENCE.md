# Docker Quick Reference Card

## 🚀 Quick Start (30 seconds)

```bash
# Development
docker-compose --env-file .env.docker.dev up -d

# Production (requires .env.docker.prod.local)
docker-compose --env-file .env.docker.prod.local up -d
```

---

## 📋 Helper Scripts

### Windows
```batch
docker-dev.bat up        :: Start
docker-dev.bat logs      :: View logs
docker-dev.bat down      :: Stop
docker-dev.bat clean     :: Reset (delete data)
docker-dev.bat shell     :: SSH to container
docker-dev.bat health    :: Check health
```

### Linux/Mac
```bash
chmod +x docker-dev.sh docker-prod.sh
./docker-dev.sh up
./docker-dev.sh logs server
./docker-dev.sh shell server
./docker-dev.sh psql
./docker-prod.sh backup
./docker-prod.sh restore backups/file.sql.gz
```

---

## 🌐 Access Points

| Service | URL | Port | User |
|---------|-----|------|------|
| Web App | http://localhost | 80 | - |
| API | http://localhost:5000 | 5000 | - |
| Health | http://localhost:5000/health | 5000 | - |
| Database | localhost | 5432 | keyboardtrainer |

---

## 📊 Common Commands

### View Status & Logs
```bash
docker-compose ps                    # Service status
docker-compose logs                  # All logs
docker-compose logs -f server        # Follow server logs
docker-compose logs --tail 100       # Last 100 lines
docker stats                         # Resource usage
```

### Database Operations
```bash
# Connect to database
docker-compose exec postgres psql -U keyboardtrainer -d keyboardtrainer

# Backup
docker-compose exec postgres pg_dump -U keyboardtrainer keyboardtrainer > backup.sql

# Restore
cat backup.sql | docker-compose exec -T postgres psql -U keyboardtrainer keyboardtrainer
```

### Container Access
```bash
docker-compose exec server /bin/sh              # Shell access
docker-compose exec postgres psql -U keyboardtrainer   # DB shell
docker logs keyboard-trainer-server             # View logs
docker inspect keyboard-trainer-server          # Show config
```

### Cleanup
```bash
docker-compose down              # Stop containers
docker-compose down -v           # Stop + delete volumes (data loss!)
docker-compose build --no-cache  # Rebuild images
docker system prune -a           # Clean up all unused
```

---

## ⚙️ Environment Variables

### Required for Development
- `ASPNETCORE_ENVIRONMENT=Development`
- `DB_PASSWORD=keyboardtrainer_dev`

### Required for Production
- `ASPNETCORE_ENVIRONMENT=Production`
- `DB_PASSWORD=<strong_password>`
- `CORS_ALLOWED_ORIGINS=https://your-domain.com`

---

## 🔧 Troubleshooting

| Problem | Solution |
|---------|----------|
| Port 80 in use | Change `NGINX_PORT=8080` in .env file |
| Port 5432 in use | Change `DB_PORT=5433` in .env file |
| Database won't connect | `docker-compose logs postgres` |
| API not responding | `curl http://localhost:5000/health` |
| Containers won't start | `docker-compose build --no-cache && docker-compose up -d` |
| Docker daemon not running | Start Docker Desktop or Docker daemon |

---

## 📁 Important Files

| File | Purpose |
|------|---------|
| `docker-compose.yml` | Main orchestration |
| `.env.docker.dev` | Development config |
| `.env.docker.prod` | Production template |
| `Dockerfile.server` | .NET server image |
| `nginx-default.conf` | Nginx routing |
| `DOCKER_SETUP.md` | Full guide |
| `DOCKER_DEPLOYMENT.md` | Quick start |

---

## 🔒 Security Checklist

- [ ] Set strong `DB_PASSWORD` in `.env.docker.prod.local`
- [ ] Don't commit `.env.docker.prod.local` (it's in .gitignore)
- [ ] Enable HTTPS in nginx-default.conf for production
- [ ] Update `CORS_ALLOWED_ORIGINS` for your domain
- [ ] Use firewall to restrict port access
- [ ] Regular backups: `./docker-prod.sh backup`
- [ ] Review logs regularly: `docker-compose logs`

---

## 📈 Performance Tips

```bash
# Run multiple server instances (behind Nginx)
docker-compose up -d --scale server=3

# Limit resource usage
# Edit docker-compose.yml:
# deploy:
#   resources:
#     limits:
#       memory: 512M

# Monitor in real-time
docker stats --no-stream=false

# Check database connections
docker exec keyboard-trainer-db psql -U keyboardtrainer -c \
  "SELECT datname, count(*) FROM pg_stat_activity GROUP BY datname"
```

---

## 📚 Documentation

- **Full Setup Guide**: `DOCKER_SETUP.md`
- **Deployment Guide**: `DOCKER_DEPLOYMENT.md`
- **Task Summary**: `TASK_2_7_DOCKER_COMPLETE.md`

---

## 🆘 Getting Help

```bash
# Show available docker-compose commands
docker-compose help

# Show help for specific command
docker-compose help up

# View service logs for debugging
docker-compose logs [service_name]

# Check Docker system info
docker info

# Verify Docker is installed
docker --version
docker-compose --version
```

---

## ⏱️ Service Startup Times

| Service | Time | Status |
|---------|------|--------|
| Nginx | ~2 sec | Fast |
| PostgreSQL | ~5 sec | Medium (depends on volume) |
| Server (ASP.NET) | ~10 sec | Slow (JIT compilation) |
| **Total** | **~15 sec** | - |

---

## 💾 Backup Schedule

```bash
# Daily backup at 2 AM
0 2 * * * /path/to/docker-prod.sh backup

# Test restore weekly
0 3 * * 0 /path/to/docker-prod.sh restore /backups/latest.sql.gz

# Keep 30 days of backups
find /backups -name "*.sql.gz" -mtime +30 -delete
```

---

## 🎯 Next Steps

1. **Start services**: `docker-compose up -d`
2. **Check health**: `docker-compose ps`
3. **View logs**: `docker-compose logs -f`
4. **Access app**: http://localhost
5. **Read full guide**: `DOCKER_SETUP.md`

---

**Quick Reference Version**: 1.0  
**Updated**: 2026-01-25  
**Format**: Copy & paste ready commands
