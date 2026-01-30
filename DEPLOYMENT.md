# 🚀 Deployment Guide

## Prerequisites

- Docker & Docker Compose installed
- Git installed
- (Production) Server with SSH access

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────┐
│                   Nginx (Reverse Proxy)          │
│                   Port 80/443                    │
└────────────┬──────────────────┬─────────────────┘
             │                  │
    ┌────────▼─────────┐  ┌────▼──────────┐
    │   Frontend       │  │   Backend API  │
    │   (Vue 3)        │  │   (.NET 8)     │
    │   Port 80        │  │   Port 8080    │
    └──────────────────┘  └────┬───────────┘
                               │
                          ┌────▼──────────┐
                          │  SQL Server   │
                          │  Port 1433    │
                          └───────────────┘
```

## 📦 Local Development

### Using Docker Compose

1. **Clone the repository**
```bash
git clone <your-repo-url>
cd Tuan_7
```

2. **Create environment file**
```bash
cp .env.example .env
```

3. **Start all services**
```bash
docker-compose up -d
```

4. **Access the application**
- Frontend: http://localhost:5173
- Backend API: http://localhost:5211
- Swagger: http://localhost:5211/swagger

5. **View logs**
```bash
docker-compose logs -f
```

6. **Stop services**
```bash
docker-compose down
```

### Manual Setup (Without Docker)

#### Backend
```bash
cd PCM.Api
dotnet restore
dotnet ef database update
dotnet run
```

#### Frontend
```bash
cd PCM.FE
npm install
npm run dev
```

## 🌐 Production Deployment

### Option 1: Docker Compose (Recommended)

1. **Prepare the server**
```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

2. **Configure environment**
```bash
# Create .env file with production values
cp .env.example .env
nano .env
```

3. **Deploy**
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### Option 2: GitHub Actions CI/CD

#### Setup GitHub Secrets

Navigate to Repository Settings → Secrets and add:

**Required Secrets:**
- `DEPLOY_HOST`: Your server IP/hostname
- `DEPLOY_USER`: SSH username
- `DEPLOY_SSH_KEY`: Private SSH key
- `DEPLOY_PATH`: Deployment directory path (e.g., /var/www/pcm)
- `DB_SA_PASSWORD`: SQL Server SA password
- `JWT_SECRET`: JWT signing secret

**Optional Secrets:**
- `DEPLOY_PORT`: SSH port (default: 22)

**Variables:**
- `VITE_API_URL`: Frontend API URL
- `PRODUCTION_URL`: Production website URL

#### Deployment Workflow

The CI/CD pipeline automatically:

1. **On Push to `main` or `develop`:**
   - Builds backend (.NET)
   - Builds frontend (Vue 3)
   - Runs tests
   - Builds Docker images
   - Pushes to GitHub Container Registry

2. **On Push to `main` (Production):**
   - Deploys to production server via SSH
   - Pulls latest Docker images
   - Restarts services
   - Cleans up old images

#### Manual Trigger

```bash
# From GitHub UI: Actions → CI/CD Pipeline → Run workflow
```

### Option 3: Manual Production Deployment

1. **Build Backend**
```bash
cd PCM.Api
dotnet publish -c Release -o ./publish
```

2. **Build Frontend**
```bash
cd PCM.FE
npm install
npm run build
```

3. **Deploy to Server**
```bash
# Copy files to server
scp -r PCM.Api/publish user@server:/var/www/pcm/api
scp -r PCM.FE/dist user@server:/var/www/pcm/frontend
```

4. **Configure Reverse Proxy (Nginx)**
```nginx
server {
    listen 80;
    server_name your-domain.com;

    # Frontend
    location / {
        root /var/www/pcm/frontend;
        try_files $uri $uri/ /index.html;
    }

    # Backend API
    location /api {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## 🔒 Security Checklist

- [ ] Change default database passwords
- [ ] Use strong JWT secret (min 32 characters)
- [ ] Enable HTTPS with SSL certificates
- [ ] Configure CORS properly
- [ ] Set up firewall rules
- [ ] Enable database backups
- [ ] Configure log rotation
- [ ] Use environment variables for secrets
- [ ] Enable rate limiting
- [ ] Regular security updates

## 🔍 Monitoring & Maintenance

### Health Checks

- API: `http://your-domain/health`
- Frontend: `http://your-domain/health`

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f frontend
docker-compose logs -f db
```

### Database Backup

```bash
# Backup
docker exec pcm-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourPassword' -Q "BACKUP DATABASE PCM TO DISK='/var/opt/mssql/backup/pcm.bak'"

# Copy from container
docker cp pcm-db:/var/opt/mssql/backup/pcm.bak ./pcm-backup-$(date +%Y%m%d).bak
```

### Update Application

```bash
# Pull latest code
git pull origin main

# Rebuild and restart
docker-compose -f docker-compose.prod.yml up -d --build

# Or using CI/CD
git push origin main  # Triggers automatic deployment
```

## 🐛 Troubleshooting

### Database connection issues
```bash
# Check database is running
docker-compose ps db

# Test connection
docker exec -it pcm-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourPassword'
```

### API not responding
```bash
# Check API logs
docker-compose logs api

# Restart API
docker-compose restart api
```

### Frontend not loading
```bash
# Check Nginx configuration
docker-compose exec frontend nginx -t

# Restart frontend
docker-compose restart frontend
```

## 📊 Performance Optimization

### Database
- Enable query caching
- Create proper indexes
- Regular VACUUM/maintenance
- Monitor slow queries

### Backend
- Enable response compression
- Use caching (Redis/Memory)
- Optimize Entity Framework queries
- Enable async operations

### Frontend
- Enable gzip compression (✓ configured)
- Use CDN for static assets
- Implement lazy loading
- Optimize bundle size

## 🔄 Rollback

```bash
# Rollback to previous version
docker-compose down
git checkout <previous-commit>
docker-compose up -d --build
```

## 📞 Support

For issues or questions:
- Check logs: `docker-compose logs`
- GitHub Issues: [Repository Issues]
- Documentation: See README.md

---

**Last Updated:** 2026-01-30
