# MAP2ADAMOINT - Configuration Guide

Complete guide for configuring database connections and features.

---

## 🔑 Required Credentials

You need credentials for TWO databases:

### 1. PostgreSQL (MAP Tool Database)
- **Host/IP address** of your PostgreSQL server
- **Port** (usually 5432)
- **Database name** (usually MAP23)
- **Username**
- **Password**

### 2. Oracle (ADAMO Database)
- **Host/IP address** of your Oracle server
- **Port** (usually 1521)
- **Service name or SID** (e.g., XE)
- **Username** (usually system or a specific user)
- **Password**

---

## ⚙️ Configuration Methods

### Option 1: Environment Variables (Recommended for Docker)

Set these environment variables:

**PostgreSQL:**
```bash
MAPTOOL_CONNECTION_STRING="Host=YOUR_HOST;Port=5432;Database=MAP23;Username=YOUR_USER;Password=YOUR_PASSWORD"
```

**Oracle:**
```bash
ADAMO_CONNECTION_STRING="User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=YOUR_HOST:1521/XE"
```

**Feature Flags:**
```bash
ENABLE_DATABASE_WRITES="false"
ENABLE_MIGRATION="false"
```

### Option 2: appsettings.json (For Local Development)

**Step 1:** Copy `config.example.json` to `appsettings.json`:
```bash
cp config.example.json appsettings.json
```

**Step 2:** Edit `appsettings.json` with your credentials:

```json
{
  "ConnectionStrings": {
    "MapToolDb": "Host=192.168.1.100;Port=5432;Database=MAP23;Username=map_user;Password=SecurePass123!",
    "AdamoDb": "User Id=system;Password=OraclePass456!;Data Source=192.168.1.200:1521/XE"
  },
  "DatabaseFeatures": {
    "EnableDatabaseWrites": false,
    "EnableMigration": false
  }
}
```

⚠️ **NEVER commit `appsettings.json` with real credentials to git!**

---

## 🔒 Connection String Formats

### PostgreSQL Connection String

**Format:**
```
Host={hostname};Port={port};Database={database};Username={username};Password={password}
```

**Examples:**

**Localhost:**
```
Host=localhost;Port=5432;Database=MAP23;Username=postgres;Password=mypassword
```

**Remote Server:**
```
Host=192.168.1.100;Port=5432;Database=MAP23;Username=map_admin;Password=SecurePass123!
```

**Docker Internal (from another container):**
```
Host=postgres-container;Port=5432;Database=MAP23;Username=postgres;Password=mypassword
```

**With SSL:**
```
Host=secure-host.com;Port=5432;Database=MAP23;Username=map_user;Password=pass;SSL Mode=Require
```

### Oracle Connection String

**Format:**
```
User Id={username};Password={password};Data Source={host}:{port}/{service_name}
```

**Examples:**

**Localhost:**
```
User Id=system;Password=oracle;Data Source=localhost:1521/XE
```

**Remote Server:**
```
User Id=giv_map;Password=SecureOraclePass!;Data Source=192.168.1.200:1521/ORCL
```

**Docker Internal:**
```
User Id=system;Password=oracle;Data Source=oracle-container:1521/XE
```

**With TNS Names:**
```
User Id=giv_map;Password=pass;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=host)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)))
```

---

## 🎛️ Feature Flags

### EnableDatabaseWrites

**Values:** `true` | `false`  
**Default:** `false`

**When `false`:**
- API transforms data and returns it
- NO data is written to databases
- Logs show "[DRY RUN]" messages

**When `true`:**
- API transforms data AND writes to database
- Actual INSERT/UPDATE operations occur
- Logs show actual database operations

### EnableMigration

**Values:** `true` | `false`  
**Default:** `false`

**When `false`:**
- Migration endpoint returns error

**When `true`:**
- Enables `/migrate/adamo-to-maptool` endpoint
- Allows bulk data transfer from ADAMO to MAP Tool

---

## 🐳 Docker Configuration

### docker-compose.yml with Databases

Create `docker-compose.full.yml`:

```yaml
version: "3.8"

services:
  # PostgreSQL for MAP Tool
  postgres:
    image: postgres:15
    container_name: maptool-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: YOUR_POSTGRES_PASSWORD
      POSTGRES_DB: MAP23
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - middleware-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5

  # Oracle for ADAMO
  oracle:
    image: container-registry.oracle.com/database/express:21.3.0-xe
    container_name: adamo-oracle
    environment:
      ORACLE_PWD: YOUR_ORACLE_PASSWORD
      ORACLE_CHARACTERSET: AL32UTF8
    ports:
      - "1521:1521"
    volumes:
      - oracle-data:/opt/oracle/oradata
    networks:
      - middleware-network
    shm_size: 1g
    healthcheck:
      test: ["CMD-SHELL", "healthcheck.sh"]
      interval: 30s
      timeout: 10s
      retries: 10
      start_period: 120s

  # Middleware API
  map2adamoint:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: map2adamoint-api
    ports:
      - "8085:8085"
    depends_on:
      postgres:
        condition: service_healthy
      oracle:
        condition: service_healthy
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - MAPTOOL_CONNECTION_STRING=Host=postgres;Port=5432;Database=MAP23;Username=postgres;Password=YOUR_POSTGRES_PASSWORD
      - ADAMO_CONNECTION_STRING=User Id=system;Password=YOUR_ORACLE_PASSWORD;Data Source=oracle:1521/XE
      - ENABLE_DATABASE_WRITES=false
      - ENABLE_MIGRATION=false
    networks:
      - middleware-network
    restart: unless-stopped

volumes:
  postgres-data:
  oracle-data:

networks:
  middleware-network:
    driver: bridge
```

**To use:**
```bash
# Replace YOUR_POSTGRES_PASSWORD and YOUR_ORACLE_PASSWORD
# Then run:
docker-compose -f docker-compose.full.yml up -d
```

---

## 🔐 Security Best Practices

### DO ✅
- ✅ Use environment variables for production
- ✅ Use strong passwords (16+ characters, mixed case, numbers, symbols)
- ✅ Keep `appsettings.json` in `.gitignore`
- ✅ Use `config.example.json` as a template (no real passwords)
- ✅ Rotate passwords regularly
- ✅ Use separate users for read vs write operations
- ✅ Enable SSL/TLS for database connections

### DON'T ❌
- ❌ Commit real passwords to git
- ❌ Use default passwords (like "postgres" or "oracle")
- ❌ Share connection strings in chat/email
- ❌ Use same password for multiple databases
- ❌ Store passwords in plain text files

---

## 📋 Configuration Checklist

Before enabling database features:

- [ ] Obtain PostgreSQL credentials from MAP Tool team
- [ ] Obtain Oracle credentials from ADAMO team
- [ ] Test PostgreSQL connection manually:
  ```bash
  psql -h YOUR_HOST -p 5432 -U YOUR_USER -d MAP23
  ```
- [ ] Test Oracle connection manually:
  ```bash
  sqlplus YOUR_USER/YOUR_PASSWORD@YOUR_HOST:1521/XE
  ```
- [ ] Create `appsettings.json` with real credentials
- [ ] Set `EnableDatabaseWrites: false` initially
- [ ] Test transformation endpoints first
- [ ] Backup databases before enabling writes
- [ ] Set `EnableDatabaseWrites: true` only when ready
- [ ] Monitor logs during first writes

---

## 🧪 Testing Database Connection

### Test PostgreSQL

**Without starting the API, test connection:**

```bash
# Windows PowerShell
docker run --rm postgres:15 psql -h YOUR_HOST -p 5432 -U YOUR_USER -d MAP23 -c "SELECT version();"
```

**Expected output:** PostgreSQL version information

### Test Oracle

```bash
# Windows PowerShell (with Oracle client)
sqlplus YOUR_USER/YOUR_PASSWORD@YOUR_HOST:1521/XE
```

**Expected:** SQL> prompt

**In SQL:**
```sql
SELECT * FROM GIV_MAP.MAP_SESSION WHERE ROWNUM <= 1;
```

---

## ⚙️ What I Need From You

### For PostgreSQL (MAP Tool Database)

Please provide:

1. **Host:** `____________` (IP address or hostname)
2. **Port:** `____________` (usually 5432)
3. **Database:** `____________` (usually MAP23)
4. **Username:** `____________`
5. **Password:** `____________`
6. **Schema:** `map_adm` (already configured)

### For Oracle (ADAMO Database)

Please provide:

1. **Host:** `____________` (IP address or hostname)
2. **Port:** `____________` (usually 1521)
3. **Service Name/SID:** `____________` (e.g., XE, ORCL)
4. **Username:** `____________` (usually system or giv_map)
5. **Password:** `____________`
6. **Schema:** `GIV_MAP` (already configured)

---

## 🚀 Quick Configuration Steps

### Step 1: Create Configuration File

```bash
# Copy example
cp config.example.json appsettings.json

# Edit with your credentials (use VS Code, Notepad++, etc.)
notepad appsettings.json
```

### Step 2: Update Connection Strings

Replace `CONFIGURE_ME` with your actual connection strings.

### Step 3: Keep Writes Disabled Initially

```json
{
  "DatabaseFeatures": {
    "EnableDatabaseWrites": false,  // ← Keep this false!
    "EnableMigration": false         // ← Keep this false!
  }
}
```

### Step 4: Test Without Writes

```bash
# Rebuild
docker-compose down
docker-compose up --build -d

# Test health
curl http://localhost:8085/health

# Test transformation (no database write)
curl -X POST http://localhost:8085/transform/map-to-adamo \
  -H "Content-Type: application/json" \
  -d '@test-map-to-adamo.json'
```

### Step 5: When Ready, Enable Writes

Edit `appsettings.json`:
```json
{
  "DatabaseFeatures": {
    "EnableDatabaseWrites": true,   // ← Enable writes
    "EnableMigration": false         // ← Keep migration disabled for now
  }
}
```

Rebuild:
```bash
docker-compose down
docker-compose up --build -d
```

### Step 6: When Ready for Migration

```json
{
  "DatabaseFeatures": {
    "EnableDatabaseWrites": true,
    "EnableMigration": true          // ← Enable migration
  }
}
```

---

## 📁 Where to Put Credentials

### Recommended Approach:

Create a `.env` file (automatically ignored by git):

```bash
# .env file
MAPTOOL_HOST=192.168.1.100
MAPTOOL_PORT=5432
MAPTOOL_DATABASE=MAP23
MAPTOOL_USER=map_admin
MAPTOOL_PASSWORD=SecurePassword123!

ADAMO_HOST=192.168.1.200
ADAMO_PORT=1521
ADAMO_SERVICE=XE
ADAMO_USER=system
ADAMO_PASSWORD=OracleSecure456!

ENABLE_DATABASE_WRITES=false
ENABLE_MIGRATION=false
```

Then reference in `docker-compose.yml`:

```yaml
environment:
  - MAPTOOL_CONNECTION_STRING=Host=${MAPTOOL_HOST};Port=${MAPTOOL_PORT};Database=${MAPTOOL_DATABASE};Username=${MAPTOOL_USER};Password=${MAPTOOL_PASSWORD}
  - ADAMO_CONNECTION_STRING=User Id=${ADAMO_USER};Password=${ADAMO_PASSWORD};Data Source=${ADAMO_HOST}:${ADAMO_PORT}/${ADAMO_SERVICE}
  - ENABLE_DATABASE_WRITES=${ENABLE_DATABASE_WRITES}
  - ENABLE_MIGRATION=${ENABLE_MIGRATION}
```

---

## 🎯 Summary

**What you need to provide:**

1. ✅ PostgreSQL connection details (6 items)
2. ✅ Oracle connection details (6 items)
3. ✅ Decide: Use `.env` file OR `appsettings.json`

**What I'll build:**

1. ✅ Database write functions (ready but disabled by default)
2. ✅ Migration endpoint for one-time bulk transfer
3. ✅ Configuration system with feature flags
4. ✅ Docker compose with optional database containers
5. ✅ Full documentation

**Where to configure:**

- **Development:** `appsettings.json` (gitignored)
- **Docker/Production:** `.env` file or environment variables
- **Example template:** `config.example.json` (committed to git)

---

**Once you provide the credentials, I'll show you exactly where to put them! 🚀**

