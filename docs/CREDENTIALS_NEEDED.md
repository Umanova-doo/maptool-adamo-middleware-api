# 🔑 Credentials Required

## What I Need From You

To enable database features, I need the following information:

---

## 1️⃣ PostgreSQL (MAP Tool Database)

Please provide these details:

```
Host/IP:     ________________________________
Port:        ________________________________ (usually 5432)
Database:    ________________________________ (usually MAP23)
Username:    ________________________________
Password:    ________________________________
Schema:      map_adm (already configured ✓)
```

### Example Connection String:
```
Host=192.168.1.100;Port=5432;Database=MAP23;Username=map_admin;Password=SecurePass123!
```

---

## 2️⃣ Oracle (ADAMO Database)

Please provide these details:

```
Host/IP:          ________________________________
Port:             ________________________________ (usually 1521)
Service Name/SID: ________________________________ (e.g., XE, ORCL)
Username:         ________________________________ (usually system or giv_map)
Password:         ________________________________
Schema:           GIV_MAP (already configured ✓)
```

### Example Connection String:
```
User Id=system;Password=OracleSecure456!;Data Source=192.168.1.200:1521/XE
```

---

## 📁 Where to Put Credentials

### Option A: .env File (Recommended for Docker)

**Step 1:** Copy the example:
```bash
cp .env.example .env
```

**Step 2:** Edit `.env` with your credentials:
```bash
# .env file
POSTGRES_PASSWORD=YOUR_REAL_POSTGRES_PASSWORD
ORACLE_PASSWORD=YOUR_REAL_ORACLE_PASSWORD
ENABLE_DATABASE_WRITES=false
ENABLE_MIGRATION=false
```

**Step 3:** Use docker-compose.full.yml:
```bash
docker-compose -f docker-compose.full.yml up -d
```

### Option B: appsettings.json (For Local Development)

**Step 1:** Copy the example:
```bash
cp config.example.json appsettings.json
```

**Step 2:** Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "MapToolDb": "Host=YOUR_HOST;Port=5432;Database=MAP23;Username=YOUR_USER;Password=YOUR_PASSWORD",
    "AdamoDb": "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=YOUR_HOST:1521/XE"
  },
  "DatabaseFeatures": {
    "EnableDatabaseWrites": false,
    "EnableMigration": false
  }
}
```

**Step 3:** Run locally:
```bash
dotnet run
```

---

## ✅ What Happens When You Provide Credentials

### With Credentials Configured:

```
═══════════════════════════════════════════════════════
  MAP2ADAMOINT Middleware API
  Data transformation between ADAMO and MAP Tool
═══════════════════════════════════════════════════════
  ✓ PostgreSQL context configured
  ✓ Oracle context configured
  Database Writes: DISABLED (transformation only)
  Migration: DISABLED
═══════════════════════════════════════════════════════
```

### Without Credentials (Current State):

```
═══════════════════════════════════════════════════════
  MAP2ADAMOINT Middleware API
  Data transformation between ADAMO and MAP Tool
═══════════════════════════════════════════════════════
  ⚠ PostgreSQL not configured (transformation-only mode)
  ⚠ Oracle not configured (transformation-only mode)
  Database Writes: DISABLED (transformation only)
  Migration: DISABLED
═══════════════════════════════════════════════════════
```

---

## 🎯 Current Status

| Feature | Status | Notes |
|---------|--------|-------|
| Transformation endpoints | ✅ Working | No credentials needed |
| Database reading | ⏸️ Ready | Needs credentials |
| Database writing | ⏸️ Ready (disabled) | Needs credentials + feature flag |
| Migration endpoint | ⏸️ Ready (disabled) | Needs credentials + feature flag |

---

## 🚀 Enabling Features (After Providing Credentials)

### Stage 1: Test Connection Only
```json
{
  "DatabaseFeatures": {
    "EnableDatabaseWrites": false,  // ← Read-only mode
    "EnableMigration": false
  }
}
```

### Stage 2: Enable Transformation + Write
```json
{
  "DatabaseFeatures": {
    "EnableDatabaseWrites": true,   // ← Can write from transform endpoints
    "EnableMigration": false
  }
}
```

### Stage 3: Enable Migration (One-Time Bulk Transfer)
```json
{
  "DatabaseFeatures": {
    "EnableDatabaseWrites": true,
    "EnableMigration": true          // ← Enables /migration/adamo-to-maptool
  }
}
```

---

## 📧 Send Me These Details

### For PostgreSQL:
1. Host/IP address
2. Port (if not 5432)
3. Database name (if not MAP23)
4. Username
5. Password

### For Oracle:
1. Host/IP address
2. Port (if not 1521)
3. Service name/SID
4. Username
5. Password

**Once you provide these, I'll tell you EXACTLY where to put them!** 🚀

---

## 🔒 Security Notes

- ⚠️ Never share passwords in chat - I'll show you where to put them in files
- ⚠️ `.env` and `appsettings.json` are already in `.gitignore`
- ⚠️ Use `.env.example` and `config.example.json` as templates
- ⚠️ Real credentials only go in files that are gitignored

---

**Ready when you are!** 📋

