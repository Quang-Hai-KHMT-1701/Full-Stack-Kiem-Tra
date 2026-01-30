#!/bin/bash

# ===========================================
# DEPLOY SCRIPT FOR TIEMCAMDO.LINKPC.NET
# VPS: 143.198.88.205 (DigitalOcean)
# ===========================================

set -e

echo "🚀 Starting deployment for tiemcamdo.linkpc.net"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
DOMAIN="tiemcamdo.linkpc.net"
EMAIL="your-email@example.com"  # ⚠️ THAY ĐỔI EMAIL CỦA BẠN

# Step 1: Create necessary directories
echo -e "${YELLOW}📁 Creating directories...${NC}"
mkdir -p certbot/conf certbot/www backup

# Step 2: Copy environment file
echo -e "${YELLOW}📝 Setting up environment...${NC}"
cp .env.production .env

# Step 3: Start nginx with init config for SSL
echo -e "${YELLOW}🔧 Starting nginx for SSL verification...${NC}"
cp nginx/nginx.init.conf nginx/nginx.temp.conf

# Create temporary docker-compose for SSL init
cat > docker-compose.ssl-init.yml << 'EOF'
version: '3.8'
services:
  nginx-init:
    image: nginx:alpine
    container_name: pcm-nginx-init
    ports:
      - "80:80"
    volumes:
      - ./nginx/nginx.init.conf:/etc/nginx/nginx.conf:ro
      - ./certbot/www:/var/www/certbot:ro
    restart: unless-stopped
EOF

docker compose -f docker-compose.ssl-init.yml up -d

# Wait for nginx to start
sleep 5

# Step 4: Get SSL Certificate
echo -e "${YELLOW}🔐 Obtaining SSL certificate from Let's Encrypt...${NC}"
docker run --rm \
  -v $(pwd)/certbot/conf:/etc/letsencrypt \
  -v $(pwd)/certbot/www:/var/www/certbot \
  certbot/certbot certonly \
  --webroot \
  --webroot-path=/var/www/certbot \
  --email $EMAIL \
  --agree-tos \
  --no-eff-email \
  -d $DOMAIN \
  -d www.$DOMAIN \
  -d api.$DOMAIN

# Step 5: Stop temporary nginx
echo -e "${YELLOW}🛑 Stopping temporary nginx...${NC}"
docker compose -f docker-compose.ssl-init.yml down
rm docker-compose.ssl-init.yml

# Step 6: Start all services with production config
echo -e "${YELLOW}🚀 Starting all services...${NC}"
docker compose -f docker-compose.production.yml up -d --build

# Step 7: Wait for services to be healthy
echo -e "${YELLOW}⏳ Waiting for services to start...${NC}"
sleep 30

# Step 8: Check service status
echo -e "${GREEN}✅ Deployment completed!${NC}"
echo ""
echo "📊 Service Status:"
docker compose -f docker-compose.production.yml ps

echo ""
echo -e "${GREEN}🎉 Your application is now live at:${NC}"
echo "   Frontend: https://$DOMAIN"
echo "   API:      https://api.$DOMAIN"
echo "   Swagger:  https://api.$DOMAIN/swagger"
echo ""
echo -e "${YELLOW}📋 Useful commands:${NC}"
echo "   View logs:     docker compose -f docker-compose.production.yml logs -f"
echo "   Restart:       docker compose -f docker-compose.production.yml restart"
echo "   Stop:          docker compose -f docker-compose.production.yml down"
echo "   DB Shell:      docker exec -it pcm-db-prod /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P '\$DB_SA_PASSWORD' -C"
