# PCM Deployment Script (PowerShell)
# This script deploys the Pickleball Club Management application on Windows

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('start','stop','restart','build','deploy','logs','backup','health')]
    [string]$Action,
    
    [string]$Environment = 'development'
)

# Colors for output
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warn {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Check prerequisites
function Check-Prerequisites {
    Write-Info "Checking prerequisites..."
    
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        Write-Error-Custom "Docker is not installed"
        exit 1
    }
    
    if (-not (Get-Command docker-compose -ErrorAction SilentlyContinue)) {
        Write-Error-Custom "Docker Compose is not installed"
        exit 1
    }
    
    Write-Info "Prerequisites check passed"
}

# Load environment variables
function Load-Env {
    if (Test-Path .env) {
        Write-Info "Loading environment variables from .env"
        Get-Content .env | ForEach-Object {
            if ($_ -match '^([^=]+)=(.*)$') {
                [System.Environment]::SetEnvironmentVariable($matches[1], $matches[2], 'Process')
            }
        }
    } else {
        Write-Warn ".env file not found"
        if (Test-Path .env.example) {
            Copy-Item .env.example .env
            Write-Info "Created .env from .env.example. Please update it with your values."
            exit 0
        } else {
            Write-Error-Custom ".env.example not found"
            exit 1
        }
    }
}

# Build images
function Build-Images {
    Write-Info "Building Docker images..."
    
    if ($Environment -eq 'production') {
        docker-compose -f docker-compose.prod.yml build --no-cache
    } else {
        docker-compose build --no-cache
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Info "Images built successfully"
    } else {
        Write-Error-Custom "Failed to build images"
        exit 1
    }
}

# Start services
function Start-Services {
    Write-Info "Starting services..."
    
    if ($Environment -eq 'production') {
        docker-compose -f docker-compose.prod.yml up -d
    } else {
        docker-compose up -d
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Info "Services started successfully"
    } else {
        Write-Error-Custom "Failed to start services"
        exit 1
    }
}

# Stop services
function Stop-Services {
    Write-Info "Stopping services..."
    
    if ($Environment -eq 'production') {
        docker-compose -f docker-compose.prod.yml down
    } else {
        docker-compose down
    }
    
    Write-Info "Services stopped successfully"
}

# Check service health
function Check-Health {
    Write-Info "Checking service health..."
    
    Start-Sleep -Seconds 10
    
    # Check API health
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5211/health" -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Info "API is healthy"
        }
    } catch {
        Write-Warn "API health check failed"
    }
    
    # Check Frontend health
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5173/health" -UseBasicParsing -TimeoutSec 5
        if ($response.StatusCode -eq 200) {
            Write-Info "Frontend is healthy"
        }
    } catch {
        Write-Warn "Frontend health check failed"
    }
}

# View logs
function View-Logs {
    if ($Environment -eq 'production') {
        docker-compose -f docker-compose.prod.yml logs -f
    } else {
        docker-compose logs -f
    }
}

# Backup database
function Backup-Database {
    Write-Info "Backing up database..."
    
    $backupFile = "pcm-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss').bak"
    $password = $env:DB_SA_PASSWORD
    
    if (-not (Test-Path backups)) {
        New-Item -ItemType Directory -Path backups | Out-Null
    }
    
    docker exec pcm-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $password `
        -Q "BACKUP DATABASE PCM TO DISK='/var/opt/mssql/backup/$backupFile'"
    
    docker cp "pcm-db:/var/opt/mssql/backup/$backupFile" ".\backups\$backupFile"
    
    Write-Info "Database backed up to .\backups\$backupFile"
}

# Main execution
Check-Prerequisites

switch ($Action) {
    'start' {
        Load-Env
        Start-Services
        Check-Health
    }
    'stop' {
        Stop-Services
    }
    'restart' {
        Load-Env
        Stop-Services
        Start-Services
        Check-Health
    }
    'build' {
        Load-Env
        Build-Images
    }
    'deploy' {
        Load-Env
        Build-Images
        Stop-Services
        Start-Services
        Check-Health
    }
    'logs' {
        View-Logs
    }
    'backup' {
        Load-Env
        Backup-Database
    }
    'health' {
        Check-Health
    }
}
