# MAP2ADAMOINT - Middleware Clarification

## 🎯 What This API Is

**MAP2ADAMOINT is a DATA TRANSFORMATION MIDDLEWARE ONLY**

```
┌────────────────┐                    ┌────────────────┐
│   ADAMO Tool   │                    │  MAP Tool      │
│  (has Oracle)  │                    │ (has Postgres) │
└────────┬───────┘                    └───────┬────────┘
         │                                    │
         │ Sends ADAMO format JSON            │ Sends MAP format JSON
         │                                    │
         ▼                                    ▼
    ┌────────────────────────────────────────────┐
    │       MAP2ADAMOINT Middleware API          │
    │                                            │
    │   ┌──────────────────────────────────┐    │
    │   │   DataMapperService              │    │
    │   │   - Transforms data formats      │    │
    │   │   - Validates models             │    │
    │   │   - Logs success/fail/errors     │    │
    │   └──────────────────────────────────┘    │
    └────────────────────────────────────────────┘
         │                                    │
         │ Returns MAP format JSON            │ Returns ADAMO format JSON
         │                                    │
         ▼                                    ▼
┌────────────────┐                    ┌────────────────┐
│   ADAMO Tool   │                    │  MAP Tool      │
│   (stores it)  │                    │  (stores it)   │
└────────────────┘                    └────────────────┘
```

---

## ❌ What This API Is NOT

- ❌ NOT a database client
- ❌ NOT ADAMO itself
- ❌ NOT MAP Tool itself
- ❌ Does NOT connect to Oracle
- ❌ Does NOT connect to PostgreSQL
- ❌ Does NOT store any data

---

## ✅ What This API DOES

1. **Receives** data in one format (JSON)
2. **Transforms** it to the other format
3. **Returns** the transformed data (JSON)
4. **Logs** success/failure/errors

---

## 📡 API Endpoints

### Test Endpoints (No External Tools Needed)

| Endpoint | Input | Output | Purpose |
|----------|-------|--------|---------|
| `POST /test/map-to-adamo` | MAP Tool format JSON | ADAMO format JSON | Test transformation |
| `POST /test/adamo-to-map` | ADAMO format JSON | MAP Tool format JSON | Test transformation |

### Example: Transform MAP Tool → ADAMO

**Request:**
```bash
POST /test/map-to-adamo
Content-Type: application/json

{
  "grNumber": "GR-99-1234-5"
}
```

**Response:**
```json
{
  "status": "success",
  "message": "Successfully validated MAP Tool → ADAMO mapping",
  "source": {
    "type": "Molecule + Map1_1MoleculeEvaluation",
    "grNumber": "GR-99-1234-5",
    "chemist": "Dr. Smith",
    "status": "Map1"
  },
  "destination": {
    "type": "MapInitial",
    "grNumber": "GR-99-1234-5",
    "regNumber": "GR-88-0681",
    "chemist": "Dr. Smith",
    "odor0h": "Fruity, fresh, apple-like with green notes",
    "odor4h": "Softer, more floral with persistent fruity character",
    "odor24h": "Woody, dry-down with subtle fruit undertones",
    "dilution": "10% in DPG",
    "comments": "Synced from MAP Tool | Status: Map1 | Project: N/A"
  },
  "fieldsMapped": 13,
  "fieldsTotal": 16,
  "completeness": "81%"
}
```

**Console Output:**
```
✓ Successfully mapped Molecule → MapInitial
  GR Number: GR-99-1234-5
  Chemist: Dr. Smith
  Odor 0h: Fruity, fresh, apple-like with green notes
  Odor 4h: Softer, more floral with persistent fruity character
  Odor 24h: Woody, dry-down with subtle fruit undertones
```

---

## 🔄 Integration Flow

### Scenario 1: ADAMO → MAP Tool

1. **ADAMO tool** queries its Oracle database
2. **ADAMO tool** sends data to `POST /test/adamo-to-map`
3. **MAP2ADAMOINT** transforms ADAMO format → MAP format
4. **MAP2ADAMOINT** returns transformed JSON
5. **MAP tool** receives the data and stores it in PostgreSQL

### Scenario 2: MAP Tool → ADAMO

1. **MAP tool** queries its PostgreSQL database
2. **MAP tool** sends data to `POST /test/map-to-adamo`
3. **MAP2ADAMOINT** transforms MAP format → ADAMO format
4. **MAP2ADAMOINT** returns transformed JSON
5. **ADAMO tool** receives the data and stores it in Oracle

---

## 🏗️ Architecture

### Current Implementation

```
MAP2ADAMOINT/
├── Controllers/
│   ├── HealthController.cs       → GET /health
│   └── TestController.cs         → POST /test/* (transformation endpoints)
├── Models/
│   ├── Adamo/                    → ADAMO format models (Oracle schema)
│   └── MapTool/                  → MAP Tool format models (PostgreSQL schema)
├── Services/
│   └── DataMapperService.cs      → Transformation logic
└── Program.cs                    → API startup (NO database connections)
```

### What Was Removed

- ❌ `Data/AdamoContext.cs` - Not used (no database access)
- ❌ `Data/MapToolContext.cs` - Not used (no database access)
- ❌ `Services/SyncService.cs` - Not used (no database queries)
- ❌ Database connection strings - Not needed
- ❌ Entity Framework DbContext registration - Not needed
- ❌ PostgreSQL/Oracle containers in docker-compose - Not needed

---

## 📦 Dependencies

Only these NuGet packages are ACTUALLY used:

- `Microsoft.EntityFrameworkCore` - For model annotations only
- `Swashbuckle.AspNetCore` - For Swagger UI
- ~~Npgsql~~ - NOT USED (no PostgreSQL connection)
- ~~Oracle.EntityFrameworkCore~~ - NOT USED (no Oracle connection)
- ~~Dapper~~ - NOT USED (no SQL queries)

**Note:** The EF Core packages can be removed entirely if you don't want the annotations.

---

## 🚀 Running the API

### With Docker Compose (Simplified)

```bash
docker-compose up --build -d
```

This starts ONLY the middleware API on port 8085. No databases.

### Testing

```bash
# Health check
curl http://localhost:8085/health

# Transform MAP Tool data to ADAMO format
curl -X POST http://localhost:8085/test/map-to-adamo \
  -H "Content-Type: application/json" \
  -d '{"grNumber":"GR-99-1234-5"}'

# Transform ADAMO data to MAP Tool format
curl -X POST http://localhost:8085/test/adamo-to-map \
  -H "Content-Type: application/json" \
  -d '{"sessionId":9999,"grNumber":"GR-99-8888-2"}'
```

---

## 💡 Future Enhancement: Add Database Support

**IF** you want to add database connections later:

1. The models are already defined (in `Models/Adamo/` and `Models/MapTool/`)
2. Create DbContext classes (examples in git history)
3. Add connection string configuration
4. Uncomment database code in `SyncService.cs` (in git history)
5. Change endpoints from `/test/*` to `/sync/*`

But for now, this is a **pure transformation layer** with NO database dependencies.

---

## 📖 Key Takeaways

✅ **Middleware only** - transforms data between formats  
✅ **Stateless** - no data storage  
✅ **Lightweight** - single container, ~50MB image  
✅ **Fast** - pure in-memory transformations  
✅ **Testable** - no external dependencies  
✅ **Portable** - runs anywhere Docker runs

---

**Last Updated:** October 29, 2025  
**Status:** ✅ Working and tested

