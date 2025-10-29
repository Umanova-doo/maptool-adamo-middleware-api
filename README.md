# MAP2ADAMOINT - Integration API

## Overview

**MAP2ADAMOINT** is a .NET 6 Web API that serves as a middleware layer between two database systems:

- **MAP Tool** - PostgreSQL database (schema: `map_adm`)
- **ADAMO** - Oracle database (schema: `GIV_MAP`)

The API reads data from both databases, performs data model mapping, and synchronizes information between the two systems.

## Architecture

```
┌─────────────┐         ┌──────────────────┐         ┌─────────────┐
│  MAP Tool   │◄────────│  MAP2ADAMOINT    │────────►│   ADAMO     │
│ PostgreSQL  │         │   .NET 6 API     │         │   Oracle    │
│  (map_adm)  │         │   Port: 8085     │         │  (GIV_MAP)  │
└─────────────┘         └──────────────────┘         └─────────────┘
```

## Project Structure

```
MAP2ADAMOINT/
│
├── Controllers/
│   ├── HealthController.cs        → GET /health endpoint
│   └── SyncController.cs          → POST /sync/* endpoints
│
├── Models/
│   ├── MapTool/                   → EF models for PostgreSQL tables
│   │   ├── Molecule.cs
│   │   ├── Assessment.cs
│   │   ├── Map1_1Evaluation.cs
│   │   ├── Map1_1MoleculeEvaluation.cs
│   │   ├── OdorFamily.cs
│   │   └── OdorDescriptor.cs
│   │
│   └── Adamo/                     → EF models for Oracle tables
│       ├── MapInitial.cs
│       ├── MapSession.cs
│       ├── MapResult.cs
│       └── OdorCharacterization.cs
│
├── Data/
│   ├── MapToolContext.cs          → DbContext for PostgreSQL
│   └── AdamoContext.cs            → DbContext for Oracle
│
├── Services/
│   ├── DataMapperService.cs       → Mapping logic between models
│   └── SyncService.cs             → Data synchronization logic
│
├── Program.cs
├── appsettings.json
├── Dockerfile
└── README.md
```

## API Endpoints

| Endpoint           | Method | Purpose                                       |
| ------------------ | ------ | --------------------------------------------- |
| `/health`          | GET    | Returns "OK" to confirm service is running    |
| `/sync/from-map`   | POST   | Pull data from MAP Tool → map → send to ADAMO |
| `/sync/from-adamo` | POST   | Pull data from ADAMO → map → send to MAP Tool |

### Example Response

```json
{
  "status": "success",
  "recordsProcessed": 24,
  "message": "Successfully processed 24 records"
}
```

## Configuration

### Connection Strings

Connection strings can be configured in `appsettings.json` or via environment variables:

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "MapToolDb": "Host=postgres;Port=5432;Database=MAP23;Username=postgres;Password=postgresUser234",
    "AdamoDb": "User Id=system;Password=oracle;Data Source=oracle:1521/XE"
  }
}
```

**Environment Variables (production):**

- `MAPTOOL_CONNECTION_STRING` - PostgreSQL connection string
- `ADAMO_CONNECTION_STRING` - Oracle connection string

## Building and Running

### Local Development

1. **Restore dependencies:**

   ```bash
   dotnet restore
   ```

2. **Run the application:**

   ```bash
   dotnet run
   ```

3. **Access Swagger UI:**
   ```
   http://localhost:8085/swagger
   ```

### Docker Build

1. **Build Docker image:**

   ```bash
   docker build -t map2adamoint:latest .
   ```

2. **Run container:**
   ```bash
   docker run -p 8085:8085 \
     -e MAPTOOL_CONNECTION_STRING="Host=postgres;Port=5432;Database=MAP23;Username=postgres;Password=postgresUser234" \
     -e ADAMO_CONNECTION_STRING="User Id=system;Password=oracle;Data Source=oracle:1521/XE" \
     map2adamoint:latest
   ```

### Docker Compose

Create a `docker-compose.yml` file:

```yaml
version: "3.8"

services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgresUser234
      POSTGRES_DB: MAP23
    ports:
      - "5432:5432"

  oracle:
    image: container-registry.oracle.com/database/express:21.3.0-xe
    environment:
      ORACLE_PWD: oracle
    ports:
      - "1521:1521"

  map2adamoint:
    build: .
    ports:
      - "8085:8085"
    depends_on:
      - postgres
      - oracle
    environment:
      MAPTOOL_CONNECTION_STRING: "Host=postgres;Port=5432;Database=MAP23;Username=postgres;Password=postgresUser234"
      ADAMO_CONNECTION_STRING: "User Id=system;Password=oracle;Data Source=oracle:1521/XE"
```

Run with:

```bash
docker-compose up
```

## Testing

### 1. Health Check

```bash
curl http://localhost:8085/health
```

Expected response:

```json
{
  "status": "OK",
  "service": "MAP2ADAMOINT",
  "timestamp": "2025-10-29T12:34:56Z"
}
```

### 2. Sync from MAP Tool to ADAMO

```bash
curl -X POST http://localhost:8085/sync/from-map
```

Expected console output:

```
✓ Sync from MAP to ADAMO completed successfully. Records: 24
```

### 3. Sync from ADAMO to MAP Tool

```bash
curl -X POST http://localhost:8085/sync/from-adamo
```

Expected console output:

```
✓ Sync from ADAMO to MAP completed successfully. Records: 15
```

## Data Mapping Logic

The `DataMapperService` handles transformations between different data models:

### MAP Tool → ADAMO

| MAP Tool (PostgreSQL)             | ADAMO (Oracle)        | Notes                           |
| --------------------------------- | --------------------- | ------------------------------- |
| `Molecule.GrNumber`               | `MapInitial.GrNumber` | Direct mapping                  |
| `Molecule.ChemistName`            | `MapInitial.Chemist`  | Direct mapping                  |
| `Map1_1MoleculeEvaluation.Odor0h` | `MapInitial.Odor0h`   | Direct mapping                  |
| `Molecule.Status`                 | `MapInitial.Comments` | Converted to string in comments |

### ADAMO → MAP Tool

| ADAMO (Oracle)         | MAP Tool (PostgreSQL)               | Notes                             |
| ---------------------- | ----------------------------------- | --------------------------------- |
| `MapSession.SessionId` | `Assessment.SessionName`            | Formatted as "ADAMO Session {id}" |
| `MapSession.Stage`     | `Assessment.Stage`                  | Direct mapping                    |
| `MapResult.Odor`       | `Map1_1MoleculeEvaluation.Odor0h`   | Direct mapping                    |
| `MapResult.Result`     | `Map1_1MoleculeEvaluation.ResultCP` | Numeric score mapping             |

## Known Limitations (Development Phase)

1. **Mock Operations**: Currently logs success/fail messages without actual database writes
2. **Limited Entity Coverage**: Only core entities are modeled (not all 8+ tables)
3. **Simplified Mapping**: Complex nested relationships require additional mapping logic
4. **No Authentication**: Production deployment requires authentication middleware
5. **No Error Recovery**: Transaction rollback and retry logic to be implemented

## Next Steps (Post-Generation)

1. ✅ Review entity mappings for accuracy
2. 🔲 Test connection to real Oracle instance (replace XE connection)
3. 🔲 Uncomment database write operations in `SyncService.cs`
4. 🔲 Add authentication and authorization
5. 🔲 Implement comprehensive error handling and logging
6. 🔲 Add integration tests
7. 🔲 Configure CI/CD pipeline
8. 🔲 Add remaining ODOR_CHARACTERIZATION descriptor fields (~100+ fields)

## Technology Stack

- **.NET 6.0** - Runtime framework
- **Entity Framework Core 6.0** - ORM
- **Npgsql** - PostgreSQL provider
- **Oracle.EntityFrameworkCore** - Oracle provider
- **Dapper** (optional) - Direct SQL queries
- **Swashbuckle** - Swagger/OpenAPI documentation

## Support

For questions or issues, contact the development team or refer to:

- `docs/MAP2-ADAMO-Integration-API-Specification.md`
- `docs/adamo-DATABASE_STRUCTURE.md`
- `docs/maptool-DATABASE-DOCUMENTATION.md`

## License

Internal use only - Proprietary software
