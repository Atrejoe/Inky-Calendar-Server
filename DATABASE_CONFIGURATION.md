# Database Configuration Guide

This document explains how to configure InkyCal Server to run with or without a database.

## Overview

InkyCal Server can operate in two modes:

1. **With Database** (default): Full functionality with persistent storage, user authentication, and panel management
2. **Without Database**: Lightweight mode using in-memory storage, suitable for testing or rendering-only deployments

## Configuration

### Using appsettings.json

Set the `DatabaseEnabled` property in `appsettings.json`:

```json
{
  "DatabaseEnabled": true,  // Set to false to disable database
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InkyCal;..."
  }
}
```

### Using Environment Variables

Set the `DatabaseEnabled` environment variable:

```bash
# Enable database (default)
export DatabaseEnabled=true

# Disable database
export DatabaseEnabled=false
```

## Docker Compose Usage

### Option 1: Run with Database (Default)

```bash
# Set database password
export DB_SA_PASSWORD=YourStrongPassword123!

# Start with database
docker-compose --profile with-database up
```

Or explicitly set in docker-compose:

```bash
DATABASE_ENABLED=true docker-compose up
```

### Option 2: Run without Database

```bash
# Start without database
docker-compose --profile no-database up
```

This will:
- Skip starting the SQL Server container
- Use in-memory storage for panels
- Disable user authentication

### Option 3: Selective Database Usage

You can also run the main service with database disabled while keeping the database container for other purposes:

```bash
export DATABASE_ENABLED=false
export DB_SA_PASSWORD=YourStrongPassword123!
docker-compose up
```

## Features by Mode

### With Database Enabled

? User authentication and authorization  
? Panel persistence across restarts  
? Panel ownership and management  
? Access tracking and statistics  
? Google OAuth integration  
? Full CRUD operations  

### With Database Disabled

? Panel rendering  
? Test panel generation  
? API endpoints for rendering  
? Swagger documentation  
? User authentication  
? Panel persistence  
? Panel ownership  
? Access tracking  

## Use Cases

### Database-Less Mode is Ideal For:

- **Testing and development**: Quick startup without database setup
- **Rendering workers**: Dedicated workers that only render panels
- **Docker containers**: Lightweight containers focused on rendering
- **CI/CD pipelines**: Fast testing without database dependencies
- **Stateless deployments**: Kubernetes pods that scale horizontally

### Database Mode is Required For:

- **Production deployments**: With user management
- **Panel management**: Creating, updating, and deleting panels
- **Multi-user environments**: With authentication and authorization
- **Data persistence**: Panels must survive restarts
- **Analytics**: Tracking panel access and usage

## Environment Variables Reference

| Variable | Default | Description |
|----------|---------|-------------|
| `DatabaseEnabled` | `true` | Enable/disable database support |
| `DATABASE_ENABLED` | `true` | Alternative format for docker-compose |
| `ConnectionStrings__DefaultConnection` | - | SQL Server connection string |
| `DB_SA_PASSWORD` | - | SQL Server SA password (for docker-compose) |

## Migration Strategy

### From Database to Database-Less

1. Export any critical panel configurations
2. Set `DatabaseEnabled=false`
3. Restart the application
4. Re-create panels programmatically or via API

### From Database-Less to Database

1. Set up SQL Server
2. Configure connection string
3. Set `DatabaseEnabled=true`
4. Restart the application (migrations run automatically)
5. Create panels through UI or API

## Health Checks

The `/health` endpoint reflects database status:

- **With Database**: Checks SQL Server connectivity
- **Without Database**: Basic application health only

## Troubleshooting

### Database Connection Issues

If `DatabaseEnabled=true` but the database is unavailable:

1. The application logs a warning
2. Continues without database support
3. Falls back to in-memory storage
4. Some features will be unavailable

Check the logs for:
```
WARNING: Failed to connect to database or run migrations: [error message]
The application will continue without database support.
```

### In-Memory Storage Limitations

When running without a database:
- All data is lost on restart
- No user authentication available
- Panel configurations are temporary
- Consider using configuration files or environment variables for panel definitions

## Examples

### Docker Compose - Full Stack with Database

```yaml
version: '3'
services:
  web:
    build: .
    environment:
      - DatabaseEnabled=true
      - ConnectionStrings__DefaultConnection=Server=db;Database=InkyCal;...
    depends_on:
      - db
  db:
    image: "mcr.microsoft.com/mssql/server"
    environment:
      - SA_PASSWORD=${DB_SA_PASSWORD}
      - ACCEPT_EULA=Y
```

### Docker Compose - Rendering Worker (No Database)

```yaml
version: '3'
services:
  renderer:
    build: .
    environment:
      - DatabaseEnabled=false
      - ASPNETCORE_URLS=http://0.0.0.0:5000/
    # No database dependency
```

### Kubernetes Deployment (No Database)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: inkycal-renderer
spec:
  replicas: 3
  template:
    spec:
      containers:
      - name: inkycal
        image: inkycal:latest
        env:
        - name: DatabaseEnabled
          value: "false"
```

## Performance Considerations

### Database Mode
- Slower startup (migrations)
- Persistent storage overhead
- Connection pool management
- Suitable for 1-10 instances

### Database-Less Mode
- Fast startup (< 1 second)
- No I/O overhead
- No connection pooling needed
- Suitable for 10-100+ instances

## Security

### Database Mode
- User authentication via ASP.NET Identity
- SQL Server security
- Connection string security (use secrets)

### Database-Less Mode
- No authentication (public access)
- Suitable for internal networks only
- Consider reverse proxy with auth
- Use firewall rules for protection

## Monitoring

Both modes support:
- `/health` endpoint
- MiniProfiler (at `/profiler`)
- Application logging
- Swagger UI (at `/swagger`)

Database mode additionally provides:
- SQL query profiling
- Database health metrics
- User activity tracking
