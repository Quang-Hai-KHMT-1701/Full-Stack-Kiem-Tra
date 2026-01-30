#!/bin/bash

# ===========================================
# RESTORE DATABASE FROM BACPAC
# ===========================================

set -e

echo "📦 Database Restore Script"

# Load environment variables
source .env

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${YELLOW}⏳ Waiting for SQL Server to be ready...${NC}"
sleep 10

# Check if sqlpackage is available, if not install it
if ! command -v sqlpackage &> /dev/null; then
    echo -e "${YELLOW}📥 Installing sqlpackage...${NC}"
    
    # Download and install sqlpackage
    wget -q https://aka.ms/sqlpackage-linux -O sqlpackage.zip
    unzip -q sqlpackage.zip -d sqlpackage
    chmod +x sqlpackage/sqlpackage
    sudo mv sqlpackage /opt/
    sudo ln -sf /opt/sqlpackage/sqlpackage /usr/local/bin/sqlpackage
    rm sqlpackage.zip
fi

# Wait for SQL Server to be fully ready
echo -e "${YELLOW}⏳ Checking SQL Server connection...${NC}"
max_attempts=30
attempt=1
while [ $attempt -le $max_attempts ]; do
    if docker exec pcm-db-prod /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$DB_SA_PASSWORD" -C -Q "SELECT 1" &> /dev/null; then
        echo -e "${GREEN}✅ SQL Server is ready!${NC}"
        break
    fi
    echo "Attempt $attempt/$max_attempts - SQL Server not ready yet..."
    sleep 5
    ((attempt++))
done

if [ $attempt -gt $max_attempts ]; then
    echo "❌ SQL Server failed to start"
    exit 1
fi

# Option 1: Restore from BACPAC using sqlpackage
if [ -f "PickleballDb.bacpac" ]; then
    echo -e "${YELLOW}📦 Restoring database from BACPAC...${NC}"
    
    # Copy bacpac to container
    docker cp PickleballDb.bacpac pcm-db-prod:/backup/
    
    # Use sqlpackage to import
    sqlpackage /Action:Import \
        /SourceFile:PickleballDb.bacpac \
        /TargetServerName:localhost,1433 \
        /TargetDatabaseName:PickleballDB \
        /TargetUser:sa \
        /TargetPassword:"$DB_SA_PASSWORD" \
        /TargetTrustServerCertificate:True
    
    echo -e "${GREEN}✅ Database restored successfully from BACPAC!${NC}"
else
    echo -e "${YELLOW}⚠️ No BACPAC file found. Running EF migrations instead...${NC}"
    
    # Run migrations via API container
    docker exec pcm-api-prod dotnet ef database update 2>/dev/null || echo "Migrations will run on first API start"
fi

# Verify database
echo -e "${YELLOW}📊 Verifying database...${NC}"
docker exec pcm-db-prod /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$DB_SA_PASSWORD" -C -Q "SELECT name FROM sys.databases"

echo -e "${GREEN}✅ Database setup completed!${NC}"
