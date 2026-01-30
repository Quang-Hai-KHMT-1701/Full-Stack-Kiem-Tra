#!/bin/bash

# PCM Deployment Script
# This script deploys the Pickleball Club Management application

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Functions
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."
    
    if ! command -v docker &> /dev/null; then
        log_error "Docker is not installed"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        log_error "Docker Compose is not installed"
        exit 1
    fi
    
    log_info "Prerequisites check passed"
}

# Load environment variables
load_env() {
    if [ -f .env ]; then
        log_info "Loading environment variables from .env"
        export $(cat .env | grep -v '^#' | xargs)
    else
        log_warn ".env file not found, using defaults"
        if [ ! -f .env.example ]; then
            log_error ".env.example not found"
            exit 1
        fi
        cp .env.example .env
        log_info "Created .env from .env.example. Please update it with your values."
        exit 0
    fi
}

# Build images
build_images() {
    log_info "Building Docker images..."
    
    if [ "$ENVIRONMENT" = "production" ]; then
        docker-compose -f docker-compose.prod.yml build --no-cache
    else
        docker-compose build --no-cache
    fi
    
    log_info "Images built successfully"
}

# Start services
start_services() {
    log_info "Starting services..."
    
    if [ "$ENVIRONMENT" = "production" ]; then
        docker-compose -f docker-compose.prod.yml up -d
    else
        docker-compose up -d
    fi
    
    log_info "Services started successfully"
}

# Stop services
stop_services() {
    log_info "Stopping services..."
    
    if [ "$ENVIRONMENT" = "production" ]; then
        docker-compose -f docker-compose.prod.yml down
    else
        docker-compose down
    fi
    
    log_info "Services stopped successfully"
}

# Check service health
check_health() {
    log_info "Checking service health..."
    
    # Wait for services to be ready
    sleep 10
    
    # Check API health
    if curl -f http://localhost:5211/health &> /dev/null; then
        log_info "API is healthy"
    else
        log_warn "API health check failed"
    fi
    
    # Check Frontend health
    if curl -f http://localhost:5173/health &> /dev/null; then
        log_info "Frontend is healthy"
    else
        log_warn "Frontend health check failed"
    fi
}

# View logs
view_logs() {
    if [ "$ENVIRONMENT" = "production" ]; then
        docker-compose -f docker-compose.prod.yml logs -f
    else
        docker-compose logs -f
    fi
}

# Backup database
backup_database() {
    log_info "Backing up database..."
    
    BACKUP_FILE="pcm-backup-$(date +%Y%m%d-%H%M%S).bak"
    
    docker exec pcm-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${DB_SA_PASSWORD}" \
        -Q "BACKUP DATABASE PCM TO DISK='/var/opt/mssql/backup/${BACKUP_FILE}'"
    
    docker cp pcm-db:/var/opt/mssql/backup/${BACKUP_FILE} ./backups/${BACKUP_FILE}
    
    log_info "Database backed up to ./backups/${BACKUP_FILE}"
}

# Main script
main() {
    # Default environment
    ENVIRONMENT=${ENVIRONMENT:-development}
    
    case "$1" in
        start)
            check_prerequisites
            load_env
            start_services
            check_health
            ;;
        stop)
            check_prerequisites
            stop_services
            ;;
        restart)
            check_prerequisites
            load_env
            stop_services
            start_services
            check_health
            ;;
        build)
            check_prerequisites
            load_env
            build_images
            ;;
        deploy)
            check_prerequisites
            load_env
            build_images
            stop_services
            start_services
            check_health
            ;;
        logs)
            check_prerequisites
            view_logs
            ;;
        backup)
            check_prerequisites
            load_env
            mkdir -p backups
            backup_database
            ;;
        health)
            check_health
            ;;
        *)
            echo "Usage: $0 {start|stop|restart|build|deploy|logs|backup|health}"
            echo ""
            echo "Commands:"
            echo "  start   - Start all services"
            echo "  stop    - Stop all services"
            echo "  restart - Restart all services"
            echo "  build   - Build Docker images"
            echo "  deploy  - Build and deploy (build + restart)"
            echo "  logs    - View service logs"
            echo "  backup  - Backup database"
            echo "  health  - Check service health"
            echo ""
            echo "Environment:"
            echo "  Set ENVIRONMENT=production for production deployment"
            exit 1
            ;;
    esac
}

main "$@"
