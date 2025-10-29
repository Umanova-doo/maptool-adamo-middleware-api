# ✅ MAP2ADAMOINT - Setup Complete!

## 🎉 Everything is Configured and Working

---

## 🔑 Credentials Configured

### PostgreSQL (MAP Tool Database)
```
✅ Host: localhost (Docker: host.docker.internal)
✅ Port: 5433
✅ Database: MAP23
✅ Username: postgres
✅ Password: postgresUser234
✅ Schema: map_adm
```

### Oracle (ADAMO Database)
```
✅ Host: localhost (Docker: host.docker.internal)
✅ Port: 4040
✅ Service: FREEPDB1
✅ Username: GIV_MAP
✅ Password: MapPassword123
✅ Schema: GIV_MAP
```

### Configured In:
- `appsettings.json` - For local development (localhost)
- `appsettings.Docker.json` - For Docker (host.docker.internal)
- Dockerfile automatically uses Docker version when building

---

## 🚀 How to Run

### Running in Docker (Your Preferred Method):

```bash
docker-compose up -d
```

**What happens:**
1. Builds .NET 6 API
2. Copies `appsettings.Docker.json` as `appsettings.json` in container
3. Container uses `host.docker.internal` to access YOUR localhost databases
4. Starts on port 8085

**Verification:**
```bash
# Check logs
docker logs map2adamoint-api

# Should see:
#   ✓ PostgreSQL configured: localhost
#   ✓ Oracle configured: localhost:4040/FREEPDB1
#   Database Writes: DISABLED (transformation only)
```

### Running Locally (Alternative):

```bash
dotnet run
```

Uses `appsettings.json` or `appsettings.Development.json` with direct `localhost` access.

---

## 📡 API Endpoints (All Working)

| Endpoint | Status | Database Access |
|----------|--------|-----------------|
| `GET /health` | ✅ Working | None |
| `POST /transform/map-to-adamo` | ✅ Working | Read-only (when writes disabled) |
| `POST /transform/adamo-to-map` | ✅ Working | Read-only (when writes disabled) |
| `POST /migration/adamo-to-maptool` | ⏸️ Ready | Disabled (returns 403) |

---

## 🎛️ Current Mode

```
Database Reads: ✅ ENABLED (can query both databases)
Database Writes: ❌ DISABLED (transformation only)
Migration: ❌ DISABLED (returns 403)
```

### How Data Flows Now:

```
ADAMO Tool                MAP2ADAMOINT API              MAP Tool
    │                            │                         │
    │──── Sends Session JSON ────▶│                         │
    │                            │                         │
    │                            │ Transforms to           │
    │                            │ Assessment format       │
    │                            │                         │
    │◀─── Returns Assessment ────│                         │
    │                            │                         │
    │                            │                         │
    │     (ADAMO stores it)      │    (No database write)  │
```

---

## 🔓 To Enable Database Writes

### Step 1: Edit appsettings files

**Both `appsettings.json` AND `appsettings.Docker.json`:**

```json
{
  "DatabaseFeatures": {
    "EnableDatabaseWrites": true,   // ← Change to true
    "EnableMigration": false
  }
}
```

### Step 2: Uncomment database write code

**In `Services/DatabaseService.cs`:**

**Line ~36-37:**
```csharp
// await _adamoContext.MapInitials.AddAsync(mapInitial);
// await _adamoContext.SaveChangesAsync();
```
**Remove the `//` to uncomment**

**Line ~69-70:**
```csharp
// await _mapToolContext.Assessments.AddAsync(assessment);
// await _mapToolContext.SaveChangesAsync();
```
**Remove the `//` to uncomment**

### Step 3: Rebuild and test

```bash
docker-compose down
docker-compose up --build -d

# Test with writeToDatabase flag
curl -X POST http://localhost:8085/transform/map-to-adamo \
  -H "Content-Type: application/json" \
  -d '{
    "molecule": {...},
    "writeToDatabase": true
  }'
```

---

## 🔄 To Enable Migration

### Step 1: Enable feature

**In both appsettings files:**
```json
{
  "DatabaseFeatures": {
    "EnableDatabaseWrites": true,
    "EnableMigration": true        // ← Change to true
  }
}
```

### Step 2: Uncomment migration code

**In `Services/MigrationService.cs`:**

**Line ~105-106:**
```csharp
// await _mapToolContext.Assessments.AddAsync(assessment);
// await _mapToolContext.SaveChangesAsync();
```

**Line ~145-146:**
```csharp
// await _mapToolContext.Molecules.AddAsync(molecule);
// await _mapToolContext.SaveChangesAsync();
```

**Remove the `//` to uncomment all 4 lines**

### Step 3: Run migration

```bash
curl -X POST http://localhost:8085/migration/adamo-to-maptool \
  -H "Content-Type: application/json" \
  -d '{
    "batchSize": 100,
    "stageFilter": "MAP 3",
    "migrateInitialData": true
  }'
```

---

## 📊 Current Status Summary

| Component | Status | Notes |
|-----------|--------|-------|
| API Container | ✅ Running | Port 8085 |
| PostgreSQL Connection | ✅ Configured | Can read (writes disabled) |
| Oracle Connection | ✅ Configured | Can read (writes disabled) |
| Transformation | ✅ Working | Pure transformation |
| Database Writes | ⏸️ Ready | Needs uncommenting |
| Migration | ⏸️ Ready | Needs feature flag + uncommenting |

---

## 🧪 Test Right Now

```bash
# 1. Health check
curl http://localhost:8085/health

# 2. Transform MAP → ADAMO
curl -X POST http://localhost:8085/transform/map-to-adamo \
  -H "Content-Type: application/json" \
  -d '@test-map-to-adamo.json'

# 3. Transform ADAMO → MAP
curl -X POST http://localhost:8085/transform/adamo-to-map \
  -H "Content-Type: application/json" \
  -d '@test-adamo-to-map.json'
```

All three should work! ✅

---

## 📋 Quick Commands

```bash
# Start API in Docker
docker-compose up -d

# View logs
docker logs -f map2adamoint-api

# Stop API
docker-compose down

# Rebuild after changes
docker-compose up --build -d

# Check what's running
docker ps
```

---

## 🎯 Summary

✅ **Docker configured** with your database credentials  
✅ **API running** on port 8085  
✅ **Both databases** connected (read-only mode)  
✅ **Transformations** working  
✅ **Write functions** ready (commented out)  
✅ **Migration endpoint** ready (disabled)  
✅ **All in appsettings.json** (proper .NET way)  
✅ **Pushed to GitHub**

---

## 📞 Next Steps

1. ✅ **Test transformations** - Working now
2. 🔧 **When ready for writes** - Uncomment 4 lines in DatabaseService.cs, set EnableDatabaseWrites=true
3. 🔧 **When ready for migration** - Uncomment 4 lines in MigrationService.cs, set EnableMigration=true

---

**Everything is configured and ready to go with Docker!** 🚀

**Start command:** `docker-compose up -d`  
**Test command:** `curl http://localhost:8085/health`

