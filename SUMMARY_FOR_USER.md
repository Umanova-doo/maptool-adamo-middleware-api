# 🎯 MAP2ADAMOINT - What's Ready & What You Need to Provide

---

## ✅ **What's WORKING Right Now (No Credentials Needed)**

### Transformation Endpoints

| Endpoint | Status | Purpose |
|----------|--------|---------|
| `GET /health` | ✅ Working | Health check |
| `POST /transform/map-to-adamo` | ✅ Working | Transform MAP Tool data → ADAMO format |
| `POST /transform/adamo-to-map` | ✅ Working | Transform ADAMO data → MAP Tool format |

**Current Mode:** Pure transformation - receives JSON, transforms it, returns JSON

**Test Now:**
```bash
curl -X POST http://localhost:8085/transform/map-to-adamo \
  -H "Content-Type: application/json" \
  -d '@test-map-to-adamo.json'
```

---

## ⏸️ **What's READY But Disabled (Waiting for Your Credentials)**

### 1. Database Write Functions

**Status:** Built and ready, but commented out  
**Location:** `Services/DatabaseService.cs`  
**Methods:**
- `WriteToAdamo(mapInitial)` - Writes to Oracle
- `WriteToMapTool(assessment)` - Writes to PostgreSQL

**To Enable:**
1. Provide database credentials
2. Set `EnableDatabaseWrites: true`
3. Uncomment 4 lines in `DatabaseService.cs`

### 2. Migration Endpoint

**Status:** Built and ready  
**Endpoint:** `POST /migration/adamo-to-maptool`  
**Purpose:** ONE-TIME bulk transfer from ADAMO (Oracle) to MAP Tool (PostgreSQL)

**To Enable:**
1. Provide database credentials
2. Set `EnableMigration: true`

### 3. Database Contexts

**Status:** Configured and ready  
**PostgreSQL Context:** `Data/MapToolContext.cs` ✓  
**Oracle Context:** `Data/AdamoContext.cs` ✓

**Waiting For:** Your database credentials

---

## 📋 What I Need From You

See **`CREDENTIALS_NEEDED.md`** for the fill-in form.

### Summary:

**For PostgreSQL (MAP Tool):**
- Host, Port, Database, Username, Password

**For Oracle (ADAMO):**
- Host, Port, Service Name, Username, Password

---

## 📄 Where to Put Credentials

### Method 1: .env File (Recommended for Docker)

1. Copy `.env.example` to `.env`
2. Fill in your credentials
3. Run: `docker-compose -f docker-compose.full.yml up -d`

### Method 2: appsettings.json (For Local Development)

1. Copy `config.example.json` to `appsettings.json`
2. Fill in your credentials
3. Run: `dotnet run`

**Both files are in `.gitignore` - safe from accidental commits!** ✅

---

## 🎮 Current API Endpoints

### Transformation (Always Available)

```http
POST /transform/map-to-adamo
Content-Type: application/json

{
  "molecule": { /* YOUR Molecule data */ },
  "evaluation": { /* YOUR Evaluation data */ },
  "writeToDatabase": false  // ← Set to true to write (when enabled)
}
```

```http
POST /transform/adamo-to-map
Content-Type: application/json

{
  "session": { /* YOUR Session data */ },
  "result": { /* YOUR Result data */ },
  "writeToDatabase": false  // ← Set to true to write (when enabled)
}
```

### Migration (Requires Configuration)

```http
POST /migration/adamo-to-maptool
Content-Type: application/json

{
  "batchSize": 1000,
  "stageFilter": "MAP 3",  // Optional
  "afterDate": "2024-01-01",  // Optional
  "migrateInitialData": true
}
```

**Currently Returns:** 403 Forbidden (migration disabled)  
**After You Enable:** Performs bulk transfer

---

## 🚀 Deployment Options

### Option 1: Transformation Only (Current - NO Databases)

```bash
docker-compose up -d
```

- ✅ Transforms data
- ❌ No database connections
- ❌ No writes
- ❌ No migration

### Option 2: With Your Existing Databases

1. Provide credentials (see CREDENTIALS_NEEDED.md)
2. Edit `.env` or `appsettings.json`
3. Run:
```bash
docker-compose -f docker-compose.full.yml up -d
# OR
dotnet run
```

- ✅ Transforms data
- ✅ Connects to your databases
- ⏸️ Writes disabled by default
- ⏸️ Migration disabled by default

### Option 3: With Test Databases (Included in docker-compose.full.yml)

```bash
# Set passwords in .env
echo "POSTGRES_PASSWORD=testpass123" > .env
echo "ORACLE_PASSWORD=testpass456" >> .env

# Start everything
docker-compose -f docker-compose.full.yml up -d
```

- ✅ Spins up test Postgres + Oracle
- ✅ Connects API to test databases
- ⏸️ Writes still disabled (safe)

---

## ⚙️ Feature Flag Control

| Flag | Default | Purpose |
|------|---------|---------|
| `EnableDatabaseWrites` | `false` | Allows writing to databases |
| `EnableMigration` | `false` | Enables migration endpoint |

**How to Enable:**

**Via .env:**
```bash
ENABLE_DATABASE_WRITES=true
ENABLE_MIGRATION=true
```

**Via appsettings.json:**
```json
{
  "DatabaseFeatures": {
    "EnableDatabaseWrites": true,
    "EnableMigration": true
  }
}
```

---

## 📝 Next Steps

### Immediate (Works Now):

1. ✅ Test transformation endpoints (no credentials needed)
2. ✅ Use Postman guide to test  
3. ✅ Verify transformations are correct

### When Ready for Database Operations:

1. 📋 Fill out `CREDENTIALS_NEEDED.md`
2. 📁 Put credentials in `.env` or `appsettings.json`
3. 🧪 Test with `EnableDatabaseWrites: false` first
4. ✅ Verify connections work
5. 🔓 Enable writes when ready
6. 🚀 Run migration if needed

---

## 📚 Documentation

| File | Purpose |
|------|---------|
| `CREDENTIALS_NEEDED.md` | ← Fill this out and send to me |
| `CONFIGURATION_GUIDE.md` | Complete config instructions |
| `POSTMAN_TESTING_GUIDE.md` | How to test all endpoints |
| `docker-compose.yml` | Transformation-only (no DB) |
| `docker-compose.full.yml` | With database containers |
| `.env.example` | Template for credentials |
| `config.example.json` | Template for appsettings |

---

## ✨ Summary

**Right Now:**
- ✅ API running on port 8085
- ✅ Transformations working
- ✅ No credentials needed
- ✅ No databases connected
- ✅ Safe to test

**When You Provide Credentials:**
- ✅ Can connect to databases
- ✅ Can enable writes
- ✅ Can run migration
- ✅ Full functionality unlocked

**Pushed to GitHub:** https://github.com/Umanova-doo/maptool-adamo-middleware-api

---

**Next: Send me your database credentials and I'll show you where to configure them! 🚀**

