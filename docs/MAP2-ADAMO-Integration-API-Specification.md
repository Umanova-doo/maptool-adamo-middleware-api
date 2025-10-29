# MAP2 ↔ ADAMO Integration API Specification

## 🧩 Project Overview

Create a new **.NET 6 Web API** project named **MAP2ADAMOINT**.
This API acts as a middleware layer between two existing systems:

- **MAP Tool** → runs on PostgreSQL
- **ADAMO** → runs on Oracle

The API should be designed to read/write data between both databases, map data models where possible, and eventually forward transformed data to MAP Tool or ADAMO once endpoints are defined.

For now, simply log “success” or “fail” when mock transfer functions are called.

The API should be containerized (Docker), similar in structure to the MAP2.Web.UI example provided, but listening on a different port (e.g. 8085) and using its own DLL (MAP2ADAMOINT.dll).

---

## ⚙️ Technical Requirements

### Framework & Language
- .NET 6 Web API
- C#
- Use Entity Framework Core for data access
- Optionally use Dapper for direct SQL queries

### Database Backends
1. PostgreSQL
   - Represents the MAP Tool side
   - Schema documented in: maptool-DATABASE-DOCUMENTATION.md

2. Oracle
   - Represents the ADAMO side
   - Schema documented in: adamo-DATABASE_STRUCTURE.md

Each database will have its own DbContext and connection string:

Example connection strings (in JSON format):

    {
      "ConnectionStrings": {
        "MapToolDb": "Host=postgres;Port=5432;Database=MAP23;Username=postgres;Password=postgresUser234",
        "AdamoDb": "User Id=system;Password=oracle;Data Source=oracle:1521/XE"
      }
    }

Environment variables should override these in production.

---

## 🧱 Project Structure

    MAP2ADAMOINT/
    │
    ├── Controllers/
    │   ├── HealthController.cs        → simple GET /health endpoint
    │   └── SyncController.cs          → endpoints to sync data between systems
    │
    ├── Models/
    │   ├── MapTool/                   → EF models for MAP Tool tables
    │   └── Adamo/                     → EF models for ADAMO tables
    │
    ├── Data/
    │   ├── MapToolContext.cs          → DbContext for PostgreSQL
    │   └── AdamoContext.cs            → DbContext for Oracle
    │
    ├── Services/
    │   ├── DataMapperService.cs       → handles mapping between different models
    │   ├── SyncService.cs             → contains logic for pulling/pushing data
    │
    ├── Program.cs
    ├── appsettings.json
    └── Dockerfile

---

## 🧠 Mapping & Logic

The DataMapperService should read structures from both DbContexts and map corresponding entities.

Where fields differ in name or type:
- Apply reasonable defaults (e.g. ToString(), DateTime.Parse()).
- Leave a “TODO: verify mapping for <field>” comment.
- Do not assume identical schema — map using best guess.

Example:

    public AdamoResult MapToAdamo(MapResult src)
    {
        return new AdamoResult {
            GrNumber = src.GR_NUMBER,
            Description = src.Description ?? "",
            CreatedAt = DateTime.Now // TODO: verify correct timestamp mapping
        };
    }

The SyncService should:
1. Retrieve data from PostgreSQL
2. Map to Oracle model via DataMapperService
3. Attempt insert/update into Oracle
4. Log success or error (Console.WriteLine for now)

---

## 🌐 API Endpoints

| Endpoint | Method | Purpose | Notes |
|-----------|--------|----------|-------|
| /health | GET | Returns “OK” to confirm service is running | |
| /sync/from-map | POST | Pull data from MAP (Postgres) → map → send to ADAMO (Oracle) | Logs success/fail |
| /sync/from-adamo | POST | Pull data from ADAMO (Oracle) → map → send to MAP (Postgres) | Logs success/fail |

Response Example:

    { "status": "success", "recordsProcessed": 24 }

---

## 🐳 Docker Requirements

Use the same logic as the MAP Tool Dockerfile but update port and DLL names.

Example Dockerfile:

    ARG BASE_IMAGE=mcr.microsoft.com/dotnet/aspnet:6.0
    FROM $BASE_IMAGE AS base
    USER root

    RUN apt-get update && apt-get install -y         fontconfig         fonts-dejavu-core         && rm -rf /var/lib/apt/lists/*         && fc-cache -fv

    WORKDIR /app
    COPY publish/ .

    EXPOSE 8085
    ENTRYPOINT ["dotnet", "MAP2ADAMOINT.dll"]

The container should run on port 8085 (or 8086 if 8085 is in use).

---

## 🧪 Testing

- Run locally using docker compose up (the API + databases)
- Verify /health endpoint returns HTTP 200
- Call /sync/from-map or /sync/from-adamo  
  → Should print “Sync completed successfully.” to console

---

## 📄 Deliverables

The LLM (Cursor or similar) should generate:
1. A complete .NET 6 Web API project scaffold named MAP2ADAMOINT
2. Entity models inferred from both schema markdown files
3. Basic mapping logic between entities
4. Controllers and Services as described
5. A working Dockerfile exposing port 8085
6. Build and run instructions in README.md

---

## 🧭 Next Steps (Manual Work After Generation)

- Review entity mappings for accuracy.
- Replace dummy console logging with actual network calls when real endpoints are known.
- Connect to the real production Oracle instance (replace XE connection).
- Add authentication, logging, and error handling later.
