# MAP2ADAMOINT - Project Structure

## Generated Files Overview

This document provides a complete overview of the generated .NET 6 Web API project.

## Directory Structure

```
MAP2ADAMOINT/
│
├── Controllers/
│   ├── HealthController.cs           # GET /health - Health check endpoint
│   └── SyncController.cs             # POST /sync/from-map, /sync/from-adamo
│
├── Data/
│   ├── AdamoContext.cs               # EF Core DbContext for Oracle (ADAMO)
│   └── MapToolContext.cs             # EF Core DbContext for PostgreSQL (MAP Tool)
│
├── Models/
│   ├── Adamo/
│   │   ├── MapInitial.cs             # Initial molecule evaluation (ADAMO)
│   │   ├── MapSession.cs             # Evaluation sessions (ADAMO)
│   │   ├── MapResult.cs              # Session results (ADAMO)
│   │   └── OdorCharacterization.cs   # Detailed odor profiling (ADAMO)
│   │
│   └── MapTool/
│       ├── Molecule.cs               # Molecule entity (MAP Tool)
│       ├── Assessment.cs             # Assessment sessions (MAP Tool)
│       ├── Map1_1Evaluation.cs       # MAP 1.1 evaluation (MAP Tool)
│       ├── Map1_1MoleculeEvaluation.cs # Molecule evaluation details (MAP Tool)
│       ├── OdorFamily.cs             # Odor family reference (MAP Tool)
│       └── OdorDescriptor.cs         # Odor descriptor reference (MAP Tool)
│
├── Services/
│   ├── DataMapperService.cs          # Data transformation and mapping logic
│   └── SyncService.cs                # Synchronization orchestration
│
├── Properties/
│   └── launchSettings.json           # Launch profiles for development
│
├── docs/
│   ├── adamo-DATABASE_STRUCTURE.md   # Oracle database documentation
│   ├── maptool-DATABASE-DOCUMENTATION.md # PostgreSQL database documentation
│   └── MAP2-ADAMO-Integration-API-Specification.md # API specification
│
├── .dockerignore                     # Docker ignore patterns
├── .gitignore                        # Git ignore patterns
├── appsettings.json                  # Application configuration (production)
├── appsettings.Development.json      # Development configuration
├── docker-compose.yml                # Docker Compose orchestration
├── Dockerfile                        # Docker build instructions
├── GlobalUsings.cs                   # Global using directives
├── MAP2ADAMOINT.csproj               # Project file with dependencies
├── Program.cs                        # Application entry point
├── PROJECT_STRUCTURE.md              # This file
└── README.md                         # Comprehensive documentation

```

## Key Components

### 1. Controllers

#### HealthController.cs

- **Route**: `/health`
- **Method**: GET
- **Purpose**: Returns service health status
- **Response**: `{ "status": "OK", "service": "MAP2ADAMOINT", "timestamp": "..." }`

#### SyncController.cs

- **Route**: `/sync/from-map`
  - **Method**: POST
  - **Purpose**: Sync data from MAP Tool (PostgreSQL) → ADAMO (Oracle)
- **Route**: `/sync/from-adamo`
  - **Method**: POST
  - **Purpose**: Sync data from ADAMO (Oracle) → MAP Tool (PostgreSQL)

### 2. Data Contexts

#### AdamoContext.cs

- **Database**: Oracle
- **Schema**: GIV_MAP
- **Tables**: MAP_INITIAL, MAP_SESSION, MAP_RESULT, ODOR_CHARACTERIZATION
- **Connection String**: `User Id=system;Password=oracle;Data Source=oracle:1521/XE`

#### MapToolContext.cs

- **Database**: PostgreSQL
- **Schema**: map_adm
- **Tables**: Molecule, Assessment, Map1_1Evaluation, Map1_1MoleculeEvaluation, OdorFamily, OdorDescriptor
- **Connection String**: `Host=postgres;Port=5432;Database=MAP23;Username=postgres;Password=postgresUser234`

### 3. Models

#### ADAMO Models (Oracle)

| Model                | Table                 | Key Fields                                      |
| -------------------- | --------------------- | ----------------------------------------------- |
| MapInitial           | MAP_INITIAL           | MapInitialId, GrNumber, Chemist, Odor0h/4h/24h  |
| MapSession           | MAP_SESSION           | SessionId, Stage, Region, Segment               |
| MapResult            | MAP_RESULT            | ResultId, SessionId, GrNumber, Odor, Result     |
| OdorCharacterization | ODOR_CHARACTERIZATION | OdorCharacterizationId, GrNumber, Family scores |

#### MAP Tool Models (PostgreSQL)

| Model                    | Table                    | Key Fields                                   |
| ------------------------ | ------------------------ | -------------------------------------------- |
| Molecule                 | Molecule                 | Id, GrNumber, RegNumber, Status, ChemistName |
| Assessment               | Assessment               | Id, SessionName, Stage, Region, Segment      |
| Map1_1Evaluation         | Map1_1Evaluation         | Id, AssessmentId, Participants               |
| Map1_1MoleculeEvaluation | Map1_1MoleculeEvaluation | Id, MoleculeId, Odor0h/4h/24h, ResultCP/FF   |
| OdorFamily               | OdorFamily               | Id, Name, Code, Color                        |
| OdorDescriptor           | OdorDescriptor           | Id, Name, Code, OdorFamilyId                 |

### 4. Services

#### DataMapperService.cs

Handles data transformation between the two database schemas:

**Key Methods:**

- `MapMoleculeToMapInitial()` - Converts MAP Tool Molecule → ADAMO MapInitial
- `MapInitialToMolecule()` - Converts ADAMO MapInitial → MAP Tool Molecule
- `MapResultToAssessment()` - Converts ADAMO MapResult → MAP Tool Assessment
- `MapMoleculeEvaluationToResult()` - Converts MAP Tool evaluation → ADAMO Result
- `ExtractOdorFamilyScores()` - Extracts odor family scores from ADAMO

#### SyncService.cs

Orchestrates synchronization between databases:

**Key Methods:**

- `SyncFromMapToAdamo()` - Pulls molecules from MAP Tool and pushes to ADAMO
- `SyncFromAdamoToMap()` - Pulls sessions from ADAMO and pushes to MAP Tool

### 5. Configuration Files

#### appsettings.json

- Connection strings for both databases
- Logging configuration
- Application URL: `http://0.0.0.0:8085`

#### docker-compose.yml

Defines three services:

1. **postgres** - PostgreSQL 15 database (port 5432)
2. **oracle** - Oracle Express Edition 21c (port 1521)
3. **map2adamoint** - .NET 6 API (port 8085)

## Data Flow

### Sync from MAP Tool to ADAMO

```
┌──────────────────────────────────────────────────────────┐
│ POST /sync/from-map                                      │
└────────────────────┬─────────────────────────────────────┘
                     │
                     ▼
┌──────────────────────────────────────────────────────────┐
│ SyncService.SyncFromMapToAdamo()                         │
├──────────────────────────────────────────────────────────┤
│ 1. Query Molecule from MapToolContext (PostgreSQL)      │
│ 2. Query Map1_1MoleculeEvaluation if available          │
│ 3. DataMapperService.MapMoleculeToMapInitial()          │
│ 4. Check if GrNumber exists in AdamoContext (Oracle)    │
│ 5. Insert MapInitial into AdamoContext                  │
│ 6. SaveChanges to Oracle database                       │
└──────────────────────────────────────────────────────────┘
```

### Sync from ADAMO to MAP Tool

```
┌──────────────────────────────────────────────────────────┐
│ POST /sync/from-adamo                                    │
└────────────────────┬─────────────────────────────────────┘
                     │
                     ▼
┌──────────────────────────────────────────────────────────┐
│ SyncService.SyncFromAdamoToMap()                         │
├──────────────────────────────────────────────────────────┤
│ 1. Query MapSession + MapResults from AdamoContext      │
│ 2. DataMapperService.MapResultToAssessment()            │
│ 3. Check if Assessment exists in MapToolContext         │
│ 4. Insert Assessment into MapToolContext                │
│ 5. SaveChanges to PostgreSQL database                   │
└──────────────────────────────────────────────────────────┘
```

## NuGet Dependencies

| Package                               | Version  | Purpose                    |
| ------------------------------------- | -------- | -------------------------- |
| Microsoft.EntityFrameworkCore         | 6.0.25   | ORM framework              |
| Microsoft.EntityFrameworkCore.Design  | 6.0.25   | Design-time tools          |
| Npgsql.EntityFrameworkCore.PostgreSQL | 6.0.22   | PostgreSQL provider        |
| Oracle.EntityFrameworkCore            | 6.21.170 | Oracle provider            |
| Dapper                                | 2.1.24   | Lightweight ORM (optional) |
| Swashbuckle.AspNetCore                | 6.5.0    | Swagger/OpenAPI            |

## Build Commands

### Local Development

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Access Swagger UI
# http://localhost:8085/swagger
```

### Docker

```bash
# Build image
docker build -t map2adamoint:latest .

# Run with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f map2adamoint

# Stop services
docker-compose down
```

## Testing Endpoints

### Health Check

```bash
curl http://localhost:8085/health
```

Expected response:

```json
{
  "status": "OK",
  "service": "MAP2ADAMOINT",
  "timestamp": "2025-10-29T12:34:56.789Z"
}
```

### Sync from MAP Tool

```bash
curl -X POST http://localhost:8085/sync/from-map
```

Expected response:

```json
{
  "status": "success",
  "recordsProcessed": 10,
  "message": "Successfully processed 10 records"
}
```

### Sync from ADAMO

```bash
curl -X POST http://localhost:8085/sync/from-adamo
```

Expected response:

```json
{
  "status": "success",
  "recordsProcessed": 5,
  "message": "Successfully processed 5 records"
}
```

## Environment Variables

The application supports environment variable overrides for connection strings:

- `MAPTOOL_CONNECTION_STRING` - Overrides PostgreSQL connection string
- `ADAMO_CONNECTION_STRING` - Overrides Oracle connection string
- `ASPNETCORE_ENVIRONMENT` - Sets environment (Development/Production)

## Logging

Logging is configured at multiple levels:

- **Console**: All logs output to console
- **Debug**: Available in development
- **Information**: Default level for application logs
- **Warning**: EF Core and ASP.NET Core warnings

Example log output:

```
info: MAP2ADAMOINT.Controllers.SyncController[0]
      POST /sync/from-map requested
info: MAP2ADAMOINT.Services.SyncService[0]
      Starting sync from MAP Tool to ADAMO
info: MAP2ADAMOINT.Services.SyncService[0]
      Found 10 molecules to sync
✓ Sync from MAP to ADAMO completed successfully. Records: 10
```

## Next Steps

### Immediate Tasks

1. ✅ Project structure created
2. ✅ Models defined for both databases
3. ✅ DbContexts configured
4. ✅ Data mapping service implemented
5. ✅ Sync service implemented
6. ✅ Controllers created
7. ✅ Dockerfile and docker-compose ready

### Production Readiness Checklist

- [ ] Uncomment database write operations in SyncService.cs
- [ ] Test with real database connections
- [ ] Add authentication and authorization
- [ ] Implement comprehensive error handling
- [ ] Add retry logic for failed syncs
- [ ] Add transaction management for atomic operations
- [ ] Create database migration scripts
- [ ] Add integration tests
- [ ] Add unit tests for mapping logic
- [ ] Configure production connection strings
- [ ] Set up monitoring and alerting
- [ ] Add rate limiting
- [ ] Implement request/response logging
- [ ] Add API versioning
- [ ] Create deployment pipeline
- [ ] Document API with full examples

### Schema Extension Ideas

- [ ] Add remaining ODOR_CHARACTERIZATION descriptor fields (~100+ fields)
- [ ] Implement Map1_2CP, Map2_1CP evaluation mappings
- [ ] Add MAP_ODOR_FAMILY and MAP_ODOR_DESCRIPTOR sync
- [ ] Implement bidirectional sync conflict resolution
- [ ] Add incremental sync based on timestamps
- [ ] Create sync history/audit table

## Architecture Notes

### Design Patterns Used

- **Repository Pattern**: DbContext acts as repository
- **Service Layer**: Business logic separated in services
- **DTO/Entity Pattern**: Models represent database entities
- **Dependency Injection**: Services injected via constructor
- **Factory Pattern**: DbContext factory for multiple databases

### Why Two Separate DbContexts?

- Different database providers (PostgreSQL vs Oracle)
- Different schemas and naming conventions
- Independent connection management
- Simplified transaction boundaries
- Better separation of concerns

### Mapping Strategy

The DataMapperService uses a **Best-Effort Mapping** approach:

- Maps fields with similar semantics
- Applies reasonable defaults for missing data
- Logs TODO comments for manual verification
- Preserves data lineage (e.g., "Mapped from MAP Tool")

## Troubleshooting

### Common Issues

#### Connection Refused

```
Npgsql.NpgsqlException: Connection refused
```

**Solution**: Ensure PostgreSQL is running and accessible on specified port.

#### Oracle Client Not Found

```
ORA-12154: TNS:could not resolve the connect identifier specified
```

**Solution**: Verify Oracle connection string format and ensure Oracle service is running.

#### EF Core Migrations

To create migrations:

```bash
# For MAP Tool (PostgreSQL)
dotnet ef migrations add InitialCreate --context MapToolContext

# For ADAMO (Oracle)
dotnet ef migrations add InitialCreate --context AdamoContext
```

#### Port Already in Use

```
Failed to bind to address http://0.0.0.0:8085
```

**Solution**: Change port in appsettings.json or use environment variable.

## Contributing

When extending this project:

1. Follow existing naming conventions
2. Add XML documentation comments to public methods
3. Update this PROJECT_STRUCTURE.md with new components
4. Add unit tests for mapping logic
5. Update README.md with new endpoints
6. Keep models in sync with database schema documentation

## License

Internal use only - Proprietary software

---

**Generated**: October 29, 2025  
**Version**: 1.0  
**Framework**: .NET 6.0  
**Author**: Auto-generated by Cursor AI
