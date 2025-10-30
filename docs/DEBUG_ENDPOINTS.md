# Debug Endpoints - Database Connectivity Testing

Quick reference for testing database connections.

---

## 🔍 Debug Endpoints

### 1. Test PostgreSQL Connection

```
GET http://localhost:8085/debug/test-postgres
```

**What it does:**
- Connects to MAP Tool PostgreSQL database
- Queries the `Molecule` table
- Returns first 5 molecules

**Success Response:**
```json
{
  "status": "success",
  "message": "PostgreSQL connection working",
  "database": "MAP Tool (PostgreSQL)",
  "connection": "localhost:5433/MAP23",
  "table": "Molecule",
  "recordsFound": 5,
  "sampleData": [
    {
      "id": 123,
      "grNumber": "GR-88-0681-1",
      "regNumber": "GR-88-0681",
      "chemistName": "Dr. Smith",
      "status": 1,
      "assessed": true,
      "createdAt": "2025-09-15T10:30:00Z"
    },
    ...
  ]
}
```

**Error Response:**
```json
{
  "status": "fail",
  "message": "PostgreSQL connection failed",
  "error": "Failed to connect to 127.0.0.1:5433"
}
```

---

### 2. Test Oracle Connection

```
GET http://localhost:8085/debug/test-oracle
```

**What it does:**
- Connects to ADAMO Oracle database
- Queries the `MAP_SESSION` table (GIV_MAP schema)
- Returns first 5 sessions

**Success Response:**
```json
{
  "status": "success",
  "message": "Oracle connection working",
  "database": "ADAMO (Oracle)",
  "connection": "localhost:4040/FREEPDB1",
  "schema": "GIV_MAP",
  "table": "MAP_SESSION",
  "recordsFound": 5,
  "sampleData": [
    {
      "sessionId": 4111,
      "stage": "MAP 3",
      "evaluationDate": "2004-06-18",
      "region": "US",
      "segment": "CP",
      "participants": "Panel A"
    },
    ...
  ]
}
```

**Error Response:**
```json
{
  "status": "fail",
  "message": "Oracle connection failed",
  "error": "Failed to connect to server"
}
```

---

### 3. Test Both Databases

```
GET http://localhost:8085/debug/test-both
```

**What it does:**
- Tests both PostgreSQL and Oracle connections
- Returns COUNT of records in each database
- Quick health check for both databases

**Success Response:**
```json
{
  "status": "success",
  "message": "Both databases connected successfully",
  "databases": {
    "postgres": {
      "status": "connected",
      "message": "PostgreSQL working",
      "recordCount": 1523
    },
    "oracle": {
      "status": "connected",
      "message": "Oracle working",
      "recordCount": 4285
    }
  }
}
```

---

## 📋 Postman Collection

```json
{
  "info": {
    "name": "MAP2ADAMOINT - Debug Endpoints",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Test PostgreSQL Connection",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "http://localhost:8085/debug/test-postgres",
          "protocol": "http",
          "host": ["localhost"],
          "port": "8085",
          "path": ["debug", "test-postgres"]
        }
      }
    },
    {
      "name": "Test Oracle Connection",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "http://localhost:8085/debug/test-oracle",
          "protocol": "http",
          "host": ["localhost"],
          "port": "8085",
          "path": ["debug", "test-oracle"]
        }
      }
    },
    {
      "name": "Test Both Databases",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "http://localhost:8085/debug/test-both",
          "protocol": "http",
          "host": ["localhost"],
          "port": "8085",
          "path": ["debug", "test-both"]
        }
      }
    }
  ]
}
```

---

## 🧪 cURL Commands

```bash
# Test PostgreSQL
curl http://localhost:8085/debug/test-postgres

# Test Oracle
curl http://localhost:8085/debug/test-oracle

# Test both
curl http://localhost:8085/debug/test-both
```

---

## ✅ What Success Looks Like

**Console Output:**
```
✓ PostgreSQL connection successful - Found 5 molecules
✓ Oracle connection successful - Found 5 sessions
```

**HTTP Response:** Status 200 OK with sample data

---

## ❌ Common Issues

### "Failed to connect to 127.0.0.1:5433"

**Cause:** Docker can't reach host databases

**Solutions:**
1. Run API locally: `dotnet run` instead of Docker
2. Check if databases allow external connections
3. Verify ports are open in firewall

### "Database not configured"

**Cause:** Connection string is "CONFIGURE_ME"

**Solution:** Edit `appsettings.json` with real credentials

---

**Test these endpoints in Postman to verify database connectivity!** 🚀

