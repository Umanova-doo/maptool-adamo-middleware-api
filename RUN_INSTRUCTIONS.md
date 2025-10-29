# How to Run MAP2ADAMOINT

---

## 🎯 Choose Your Method

### Method 1: Local Development (Recommended - Direct localhost access)

**Use this when:** Databases are on localhost (your current setup)

```bash
dotnet run
```

**Credentials:** Configured in `appsettings.json` or `appsettings.Development.json`

**Database Hosts:**
- PostgreSQL: `localhost:5433`
- Oracle: `localhost:4040`

---

### Method 2: Docker Container

**Use this when:** Deploying to server or want containerization

#### Step 1: Update appsettings for Docker

The credentials are already in `appsettings.Docker.json` - it uses `host.docker.internal` instead of `localhost` to access your host machine's databases from inside the container.

#### Step 2: Copy Docker settings

```bash
copy appsettings.Docker.json appsettings.json
```

#### Step 3: Build and run

```bash
docker-compose up --build -d
```

**Database Hosts (from container):**
- PostgreSQL: `host.docker.internal:5433`
- Oracle: `host.docker.internal:4040`

---

### Method 3: Docker with Embedded Test Databases

**Use this when:** Testing on a system without existing databases

```bash
docker-compose -f docker-compose.full.yml up -d
```

This starts:
- PostgreSQL container (port 5432)
- Oracle container (port 1521)
- MAP2ADAMOINT API (port 8085)

---

## ⚙️ Current Configuration

### ✅ Configured (Your Local Setup)

**PostgreSQL (MAP Tool):**
```
Host: localhost
Port: 5433
Database: MAP23
Username: postgres
Password: postgresUser234
```

**Oracle (ADAMO):**
```
Host: localhost
Port: 4040
Service: FREEPDB1
Username: GIV_MAP
Password: MapPassword123
```

**Feature Flags:**
```
EnableDatabaseWrites: false (transformation only)
EnableMigration: false (not enabled yet)
```

---

## 🧪 Test Connection

### Local (dotnet run):

**Terminal 1 - Start API:**
```bash
dotnet run
```

**Terminal 2 - Test:**
```bash
curl http://localhost:8085/health
```

### Docker:

**Start:**
```bash
# Copy Docker-specific settings first
copy appsettings.Docker.json appsettings.json

# Build and run
docker-compose up --build -d

# Check logs
docker logs map2adamoint-api

# Test
curl http://localhost:8085/health
```

---

## 📊 Startup Output

You should see:

```
═══════════════════════════════════════════════════════
  MAP2ADAMOINT Middleware API
  Data transformation between ADAMO and MAP Tool
═══════════════════════════════════════════════════════
  ✓ PostgreSQL configured: localhost
  ✓ Oracle configured: localhost:4040/FREEPDB1
  Database Writes: DISABLED (transformation only)
  Migration: DISABLED
═══════════════════════════════════════════════════════
```

If you see ✓ for both databases, connections are configured!

---

## 🔧 Switching Between Local and Docker

### For Local Development:

`appsettings.json` or `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "MapToolDb": "Host=localhost;Port=5433;...",
    "AdamoDb": "...Data Source=localhost:4040/FREEPDB1"
  }
}
```

### For Docker:

`appsettings.Docker.json` → copy to `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "MapToolDb": "Host=host.docker.internal;Port=5433;...",
    "AdamoDb": "...Data Source=host.docker.internal:4040/FREEPDB1"
  }
}
```

---

## 🚀 Current Recommended Setup

**For your local machine testing:**

```bash
# Just run locally
dotnet run
```

**Why?**
- ✅ Direct access to localhost:5433 and localhost:4040
- ✅ Faster development/testing
- ✅ No Docker networking complexity
- ✅ Easy debugging with VS Code / Visual Studio

---

## ⚡ Quick Commands

```bash
# Build
dotnet build

# Run locally
dotnet run

# Run and watch for changes
dotnet watch run

# Stop (Ctrl+C)

# Or with Docker
docker-compose up -d
docker logs -f map2adamoint-api
docker-compose down
```

---

## 📝 Next Steps

1. ✅ Credentials configured in appsettings.json
2. ✅ Run `dotnet run` to start
3. ✅ Test endpoints
4. 🔧 When ready, enable database writes
5. 🚀 When ready, enable migration

---

**Use `dotnet run` for now - it's configured and ready!** 🎉

