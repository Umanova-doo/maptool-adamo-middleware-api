# MAP2ADAMOINT - Quick Start Guide

Get the MAP2ADAMOINT integration API up and running in 5 minutes.

## Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) installed
- [Docker](https://www.docker.com/get-started) and Docker Compose (optional, for containerized setup)
- PostgreSQL 15+ (for local development without Docker)
- Oracle Database 21c+ (for local development without Docker)

## Option 1: Docker Compose (Recommended)

The fastest way to get everything running with all dependencies.

### Step 1: Build and Start Services

```bash
docker-compose up --build
```

This will:

- Start PostgreSQL on port 5432
- Start Oracle Database on port 1521
- Build and start MAP2ADAMOINT API on port 8085

### Step 2: Verify Health

```bash
curl http://localhost:8085/health
```

Expected output:

```json
{
  "status": "OK",
  "service": "MAP2ADAMOINT",
  "timestamp": "2025-10-29T12:34:56.789Z"
}
```

### Step 3: Test Sync Endpoints

**Sync from MAP Tool to ADAMO:**

```bash
curl -X POST http://localhost:8085/sync/from-map
```

**Sync from ADAMO to MAP Tool:**

```bash
curl -X POST http://localhost:8085/sync/from-adamo
```

### Step 4: Explore API with Swagger

Open in browser: http://localhost:8085/swagger

### Stop Services

```bash
docker-compose down
```

To remove volumes as well:

```bash
docker-compose down -v
```

---

## Option 2: Local Development (Without Docker)

For development with IDE debugging support.

### Step 1: Install Dependencies

```bash
dotnet restore
```

### Step 2: Configure Connection Strings

Edit `appsettings.Development.json` with your local database credentials:

```json
{
  "ConnectionStrings": {
    "MapToolDb": "Host=localhost;Port=5432;Database=MAP23;Username=your_user;Password=your_password",
    "AdamoDb": "User Id=your_user;Password=your_password;Data Source=localhost:1521/XE"
  }
}
```

### Step 3: Run the Application

```bash
dotnet run
```

Or press **F5** in Visual Studio / VS Code.

### Step 4: Access the API

- Swagger UI: http://localhost:8085/swagger
- Health check: http://localhost:8085/health

---

## Option 3: Docker Only (API Container)

Build and run just the API container, connecting to external databases.

### Step 1: Build Docker Image

```bash
docker build -t map2adamoint:latest .
```

### Step 2: Run Container

```bash
docker run -p 8085:8085 \
  -e MAPTOOL_CONNECTION_STRING="Host=host.docker.internal;Port=5432;Database=MAP23;Username=postgres;Password=postgresUser234" \
  -e ADAMO_CONNECTION_STRING="User Id=system;Password=oracle;Data Source=host.docker.internal:1521/XE" \
  map2adamoint:latest
```

**Note**: Use `host.docker.internal` to connect to databases on host machine from Docker container.

---

## Common Commands

### Development

```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Run with watch (auto-reload on file changes)
dotnet watch run

# Run tests (when tests are added)
dotnet test
```

### Docker

```bash
# Build image
docker build -t map2adamoint:latest .

# Run container
docker run -p 8085:8085 map2adamoint:latest

# View logs
docker logs -f <container-id>

# Stop container
docker stop <container-id>

# Remove container
docker rm <container-id>
```

### Docker Compose

```bash
# Start all services in background
docker-compose up -d

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f map2adamoint

# Restart a service
docker-compose restart map2adamoint

# Stop all services
docker-compose stop

# Remove all containers
docker-compose down

# Remove containers and volumes
docker-compose down -v

# Rebuild services
docker-compose up --build
```

---

## Testing the API

### Using cURL

**Health Check:**

```bash
curl http://localhost:8085/health
```

**Sync from MAP:**

```bash
curl -X POST http://localhost:8085/sync/from-map
```

**Sync from ADAMO:**

```bash
curl -X POST http://localhost:8085/sync/from-adamo
```

### Using PowerShell (Windows)

```powershell
# Health Check
Invoke-RestMethod -Uri "http://localhost:8085/health" -Method Get

# Sync from MAP
Invoke-RestMethod -Uri "http://localhost:8085/sync/from-map" -Method Post

# Sync from ADAMO
Invoke-RestMethod -Uri "http://localhost:8085/sync/from-adamo" -Method Post
```

### Using Swagger UI

1. Open http://localhost:8085/swagger
2. Expand endpoint sections
3. Click "Try it out"
4. Click "Execute"
5. View response

---

## Project Structure Overview

```
MAP2ADAMOINT/
├── Controllers/          → API endpoints
├── Data/                 → Database contexts
├── Models/
│   ├── Adamo/           → Oracle entities
│   └── MapTool/         → PostgreSQL entities
├── Services/            → Business logic
├── docs/                → Database documentation
├── Program.cs           → Application entry point
├── appsettings.json     → Configuration
├── Dockerfile           → Docker build
└── docker-compose.yml   → Multi-container setup
```

---

## Configuration Files

| File                           | Purpose                      |
| ------------------------------ | ---------------------------- |
| `appsettings.json`             | Production configuration     |
| `appsettings.Development.json` | Development overrides        |
| `docker-compose.yml`           | Container orchestration      |
| `Dockerfile`                   | Container build instructions |
| `launchSettings.json`          | IDE launch profiles          |

---

## Environment Variables

Override configuration with environment variables:

```bash
# PostgreSQL connection
export MAPTOOL_CONNECTION_STRING="Host=localhost;Port=5432;Database=MAP23;Username=user;Password=pass"

# Oracle connection
export ADAMO_CONNECTION_STRING="User Id=user;Password=pass;Data Source=localhost:1521/XE"

# Environment
export ASPNETCORE_ENVIRONMENT="Development"
```

---

## Troubleshooting

### Port Already in Use

**Error:**

```
Failed to bind to address http://0.0.0.0:8085
```

**Solutions:**

1. Change port in `appsettings.json`:

   ```json
   "Urls": "http://0.0.0.0:8086"
   ```

2. Or use environment variable:
   ```bash
   export ASPNETCORE_URLS="http://0.0.0.0:8086"
   ```

### Cannot Connect to Database

**Error:**

```
Npgsql.NpgsqlException: Connection refused
```

**Solutions:**

1. Verify database is running:

   ```bash
   # PostgreSQL
   docker ps | grep postgres

   # Oracle
   docker ps | grep oracle
   ```

2. Test connection manually:

   ```bash
   # PostgreSQL
   psql -h localhost -p 5432 -U postgres -d MAP23

   # Oracle
   sqlplus system/oracle@localhost:1521/XE
   ```

3. Check connection strings in configuration

### Swagger Not Loading

**Solution:**
Ensure you're accessing the correct URL: http://localhost:8085/swagger (not https)

### Oracle Container Slow to Start

Oracle Express Edition container takes ~2-3 minutes to fully initialize on first run.

**Check status:**

```bash
docker logs oracle
```

Wait for: `DATABASE IS READY TO USE!`

---

## Next Steps

After successful setup:

1. ✅ Verify health endpoint works
2. ✅ Test both sync endpoints
3. ✅ Explore Swagger documentation
4. 📖 Read [README.md](README.md) for detailed documentation
5. 📖 Review [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) for architecture
6. 🔧 Customize mapping logic in `Services/DataMapperService.cs`
7. 🔧 Uncomment actual database writes in `Services/SyncService.cs`
8. 🧪 Add your own endpoints as needed

---

## Additional Resources

- [.NET 6 Documentation](https://docs.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-6.0)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Oracle Database Documentation](https://docs.oracle.com/en/database/)
- [Docker Documentation](https://docs.docker.com/)

---

## Support

For issues or questions:

1. Check the [README.md](README.md)
2. Review [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)
3. Check database documentation in `docs/` folder
4. Contact the development team

---

**Happy Coding! 🚀**
