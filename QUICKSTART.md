# 🚀 Quick Start Guide

## Development (Local)

### Option 1: Using Docker (Recommended)

**Windows:**
```powershell
# Start all services
.\deploy.ps1 -Action start

# View logs
.\deploy.ps1 -Action logs

# Stop services
.\deploy.ps1 -Action stop
```

**Linux/macOS:**
```bash
# Make script executable
chmod +x deploy.sh

# Start all services
./deploy.sh start

# View logs
./deploy.sh logs

# Stop services
./deploy.sh stop
```

### Option 2: Manual Setup

**Backend:**
```bash
cd PCM.Api
dotnet restore
dotnet ef database update
dotnet run
```

**Frontend:**
```bash
cd PCM.FE
npm install
npm run dev
```

## Access Points

- **Frontend:** http://localhost:5173
- **Backend API:** http://localhost:5211
- **Swagger:** http://localhost:5211/swagger
- **Database:** localhost:1433

## Default Credentials

- **Email:** admin@pcm.com
- **Password:** Admin@123

## Common Commands

### Docker
```bash
# Build images
docker-compose build

# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Database
```bash
# Run migrations
cd PCM.Api
dotnet ef database update

# Create new migration
dotnet ef migrations add MigrationName
```

## Troubleshooting

**Port already in use:**
```bash
# Windows
netstat -ano | findstr :5173
netstat -ano | findstr :5211
netstat -ano | findstr :1433

# Linux/macOS
lsof -i :5173
lsof -i :5211
lsof -i :1433
```

**Database connection failed:**
- Ensure SQL Server container is running
- Check connection string in `.env`
- Wait 30 seconds for SQL Server to initialize

**API not responding:**
- Check logs: `docker-compose logs api`
- Verify database is ready
- Restart API: `docker-compose restart api`

## Next Steps

- 📖 See [DEPLOYMENT.md](DEPLOYMENT.md) for production deployment
- 🐛 Report issues on GitHub
- 📝 Read full documentation in [README.md](README.md)
