# Nginx SSL Configuration

This directory contains SSL certificates for HTTPS configuration.

## Quick Setup with Let's Encrypt

### Using Certbot

```bash
# Install Certbot
sudo apt-get update
sudo apt-get install certbot python3-certbot-nginx

# Get certificate
sudo certbot --nginx -d your-domain.com -d www.your-domain.com
```

### Manual Certificate Setup

1. **Place your certificates:**
   ```bash
   cp your-cert.pem nginx/ssl/cert.pem
   cp your-key.pem nginx/ssl/key.pem
   ```

2. **Update nginx.conf:**
   - Uncomment the HTTPS server block
   - Update `server_name` with your domain
   - Restart nginx

### Self-Signed Certificates (Development Only)

```bash
# Generate self-signed certificate
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout nginx/ssl/key.pem \
  -out nginx/ssl/cert.pem \
  -subj "/C=US/ST=State/L=City/O=Organization/CN=localhost"
```

## Security Best Practices

- Never commit private keys to version control
- Use strong encryption (TLSv1.2+)
- Renew certificates before expiration
- Enable HSTS for production
- Use OCSP stapling

## File Structure

```
nginx/ssl/
├── cert.pem          # SSL certificate
├── key.pem           # Private key
└── README.md         # This file
```
