# MAP2ADAMOINT - API Usage Examples

Complete guide with request/response examples for all endpoints.

---

## 🏥 Health Check

### Request

```bash
GET http://localhost:8085/health
```

### Response

```json
{
  "status": "OK",
  "service": "MAP2ADAMOINT",
  "timestamp": "2025-10-29T14:32:15.789Z"
}
```

---

## 🔄 Sync from MAP Tool to ADAMO

Syncs molecules and evaluations from PostgreSQL (MAP Tool) to Oracle (ADAMO).

### Endpoint

```
POST http://localhost:8085/sync/from-map
Content-Type: application/json
```

### Request Body Options

#### Option 1: Simple Sync (No Body - Uses Defaults)

```bash
curl -X POST http://localhost:8085/sync/from-map
```

Uses default settings:
- BatchSize: 100
- SkipExisting: true
- IncludeEvaluations: true

#### Option 2: Sync Specific Molecules

```json
{
  "grNumbers": ["GR-88-0681-1", "GR-88-0808-1", "GR-86-6561-0"],
  "includeEvaluations": true,
  "skipExisting": true,
  "dryRun": false
}
```

```bash
curl -X POST http://localhost:8085/sync/from-map \
  -H "Content-Type: application/json" \
  -d '{
    "grNumbers": ["GR-88-0681-1", "GR-88-0808-1"],
    "includeEvaluations": true
  }'
```

#### Option 3: Sync by Date Range

```json
{
  "createdAfter": "2025-01-01T00:00:00Z",
  "batchSize": 50,
  "moleculeStatus": 1,
  "includeEvaluations": true,
  "skipExisting": true,
  "dryRun": false
}
```

```bash
curl -X POST http://localhost:8085/sync/from-map \
  -H "Content-Type: application/json" \
  -d '{
    "createdAfter": "2025-01-01T00:00:00Z",
    "batchSize": 50,
    "moleculeStatus": 1
  }'
```

#### Option 4: Dry Run (Test Without Writing)

```json
{
  "batchSize": 10,
  "dryRun": true,
  "skipExisting": false
}
```

```bash
curl -X POST http://localhost:8085/sync/from-map \
  -H "Content-Type: application/json" \
  -d '{
    "batchSize": 10,
    "dryRun": true
  }'
```

### Request Body Parameters

| Parameter           | Type       | Required | Default | Description                                           |
| ------------------- | ---------- | -------- | ------- | ----------------------------------------------------- |
| `grNumbers`         | string[]   | No       | null    | Specific GR numbers to sync                           |
| `moleculeStatus`    | int        | No       | null    | Filter by status (0=None, 1=Map1, 2=Weak, 3=Odorless, 4=Ignore) |
| `createdAfter`      | datetime   | No       | null    | Only sync molecules created after this date           |
| `modifiedAfter`     | datetime   | No       | null    | Only sync molecules modified after this date          |
| `batchSize`         | int        | No       | 100     | Max records to sync (1-1000)                          |
| `dryRun`            | bool       | No       | false   | Test without writing to database                      |
| `skipExisting`      | bool       | No       | true    | Skip molecules already in ADAMO                       |
| `includeEvaluations`| bool       | No       | true    | Include MAP 1.1 evaluation data                       |

### Response

```json
{
  "status": "success",
  "recordsProcessed": 25,
  "recordsSynced": 20,
  "recordsSkipped": 5,
  "recordsFailed": 0,
  "message": "Successfully synced 20 molecules from MAP Tool to ADAMO",
  "errors": null,
  "wasDryRun": false,
  "completedAt": "2025-10-29T14:35:22.456Z"
}
```

### Error Response

```json
{
  "status": "fail",
  "recordsProcessed": 10,
  "recordsSynced": 5,
  "recordsSkipped": 2,
  "recordsFailed": 3,
  "message": "Sync completed with errors",
  "errors": [
    "Failed to sync GR-88-0681-1: Duplicate key violation",
    "Failed to sync GR-88-0808-1: Connection timeout",
    "Failed to sync GR-86-6561-0: Invalid data format"
  ],
  "wasDryRun": false,
  "completedAt": "2025-10-29T14:35:22.456Z"
}
```

---

## 🔄 Sync from ADAMO to MAP Tool

Syncs sessions and results from Oracle (ADAMO) to PostgreSQL (MAP Tool).

### Endpoint

```
POST http://localhost:8085/sync/from-adamo
Content-Type: application/json
```

### Request Body Options

#### Option 1: Simple Sync (No Body - Uses Defaults)

```bash
curl -X POST http://localhost:8085/sync/from-adamo
```

#### Option 2: Sync Specific Sessions

```json
{
  "sessionIds": [4005, 4111, 4112],
  "includeResults": true,
  "skipExisting": true,
  "dryRun": false
}
```

```bash
curl -X POST http://localhost:8085/sync/from-adamo \
  -H "Content-Type: application/json" \
  -d '{
    "sessionIds": [4005, 4111, 4112],
    "includeResults": true
  }'
```

#### Option 3: Sync by Stage and Region

```json
{
  "stage": "MAP 3",
  "region": "US",
  "segment": "CP",
  "evaluatedAfter": "2025-01-01T00:00:00Z",
  "batchSize": 25,
  "includeResults": true,
  "includeOdorCharacterization": false,
  "skipExisting": true,
  "dryRun": false
}
```

```bash
curl -X POST http://localhost:8085/sync/from-adamo \
  -H "Content-Type: application/json" \
  -d '{
    "stage": "MAP 3",
    "region": "US",
    "segment": "CP",
    "batchSize": 25
  }'
```

#### Option 4: Sync with Odor Characterization

```json
{
  "stage": "MAP 2",
  "batchSize": 10,
  "includeResults": true,
  "includeOdorCharacterization": true,
  "dryRun": false
}
```

```bash
curl -X POST http://localhost:8085/sync/from-adamo \
  -H "Content-Type: application/json" \
  -d '{
    "includeOdorCharacterization": true,
    "batchSize": 10
  }'
```

### Request Body Parameters

| Parameter                    | Type       | Required | Default | Description                                    |
| ---------------------------- | ---------- | -------- | ------- | ---------------------------------------------- |
| `sessionIds`                 | long[]     | No       | null    | Specific session IDs to sync                   |
| `stage`                      | string     | No       | null    | Filter by stage (MAP 0/1/2/3, ISC, FIB, etc.)  |
| `region`                     | string     | No       | null    | Filter by region                               |
| `segment`                    | string     | No       | null    | Filter by segment (CP or FF)                   |
| `evaluatedAfter`             | datetime   | No       | null    | Only sync sessions after this date             |
| `batchSize`                  | int        | No       | 50      | Max sessions to sync (1-500)                   |
| `dryRun`                     | bool       | No       | false   | Test without writing to database               |
| `skipExisting`               | bool       | No       | true    | Skip sessions already in MAP Tool              |
| `includeResults`             | bool       | No       | true    | Include MAP_RESULT records                     |
| `includeOdorCharacterization`| bool       | No       | false   | Include odor characterization data             |

### Response

```json
{
  "status": "success",
  "recordsProcessed": 15,
  "recordsSynced": 12,
  "recordsSkipped": 3,
  "recordsFailed": 0,
  "message": "Successfully synced 12 sessions from ADAMO to MAP Tool",
  "errors": null,
  "wasDryRun": false,
  "completedAt": "2025-10-29T14:40:18.123Z"
}
```

---

## 📋 Complete PowerShell Examples

### Sync from MAP Tool (PowerShell)

```powershell
# Simple sync
Invoke-RestMethod -Uri "http://localhost:8085/sync/from-map" -Method Post

# With parameters
$body = @{
    grNumbers = @("GR-88-0681-1", "GR-88-0808-1")
    batchSize = 50
    dryRun = $false
    includeEvaluations = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8085/sync/from-map" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

### Sync from ADAMO (PowerShell)

```powershell
# Simple sync
Invoke-RestMethod -Uri "http://localhost:8085/sync/from-adamo" -Method Post

# With parameters
$body = @{
    stage = "MAP 3"
    region = "US"
    batchSize = 25
    includeResults = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8085/sync/from-adamo" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

---

## 🧪 Testing Workflow

### 1. Start with Dry Run

Test your sync without actually writing to the database:

```bash
# Test MAP to ADAMO
curl -X POST http://localhost:8085/sync/from-map \
  -H "Content-Type: application/json" \
  -d '{"batchSize": 5, "dryRun": true}'

# Test ADAMO to MAP
curl -X POST http://localhost:8085/sync/from-adamo \
  -H "Content-Type: application/json" \
  -d '{"batchSize": 5, "dryRun": true}'
```

### 2. Sync Small Batch

After validating dry run, sync a small batch:

```bash
curl -X POST http://localhost:8085/sync/from-map \
  -H "Content-Type: application/json" \
  -d '{"batchSize": 10, "dryRun": false}'
```

### 3. Verify Results

Check the response for `recordsSynced`, `recordsSkipped`, and `recordsFailed`.

### 4. Scale Up

Once confident, increase batch size:

```bash
curl -X POST http://localhost:8085/sync/from-map \
  -H "Content-Type: application/json" \
  -d '{"batchSize": 100}'
```

---

## ⚠️ Important Notes

### Current Implementation Status

**⚠️ As of initial generation, the enhanced request body functionality is scaffolded but needs full implementation in `SyncService.cs`.**

To enable full functionality:
1. Update `SyncService.cs` to accept and use the new request DTOs
2. Implement filtering logic based on request parameters
3. Uncomment database write operations
4. Add proper transaction handling

### Backwards Compatibility

The API maintains backwards compatibility:
- Endpoints work **without** a request body (uses defaults)
- Empty POST requests will use default batch sizes and settings

### Error Handling

The API returns detailed error information:
- `errors` array contains specific failure messages
- `recordsFailed` indicates number of failures
- HTTP 500 returned if sync fails completely

---

## 📚 See Also

- [README.md](README.md) - Main documentation
- [QUICKSTART.md](QUICKSTART.md) - Quick start guide  
- [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) - Architecture details
- [Swagger UI](http://localhost:8085/swagger) - Interactive API documentation

---

**Last Updated**: October 29, 2025

