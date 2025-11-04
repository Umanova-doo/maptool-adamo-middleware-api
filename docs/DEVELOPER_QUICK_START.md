# Developer Quick Start - MapTool → Adamo Integration

**For MapTool Developers Integrating with the Adamo API**

---

## 🎯 What This API Does

This middleware API allows your MapTool application to insert evaluation data into the Adamo (Oracle) database.

**Direction:** MapTool (PostgreSQL) → **This API** → Adamo (Oracle)

---

## 🚀 Getting Started (5 Steps)

### Step 1: Get the API Running

**Clone the repository:**

```bash
git clone <your-repo-url>
cd MAP2ADAMOINT
```

**Configure Oracle connection:**

Edit `appsettings.Docker.json` (or `appsettings.json` if not using Docker):

```json
{
  "ConnectionStrings": {
    "AdamoDb": "User Id=GIV_MAP;Password=YOUR_PASSWORD;Data Source=YOUR_ORACLE_HOST:1521/FREEPDB1;Connection Timeout=30"
  }
}
```

**Replace:**

- `YOUR_PASSWORD` → Your Oracle GIV_MAP user password
- `YOUR_ORACLE_HOST` → Your Oracle server hostname/IP

**Start the API:**

**If using Docker:**

```bash
docker-compose build --no-cache map2adamoint
docker-compose up -d map2adamoint
```

**If running locally:**

```bash
dotnet run
```

**Verify it's running:**

```
GET http://localhost:8085/debug/test-oracle
```

Should return: `{ "status": "success", "connectionStatus": "CONNECTED ✓" }`

---

### Step 2: Understand the 4 Endpoints

You have **4 endpoints** available:

| Endpoint                              | Purpose                  | When to Use                                               |
| ------------------------------------- | ------------------------ | --------------------------------------------------------- |
| `POST /adamo/initial`                 | Create MAP_INITIAL       | When you have basic molecule info (optional)              |
| `POST /adamo/session`                 | Create MAP_SESSION       | When creating session without results (rare)              |
| `POST /adamo/result`                  | Create MAP_RESULT        | When adding individual results to existing session (rare) |
| `POST /adamo/session-with-results` ⭐ | Create session + results | **USE THIS 90% OF THE TIME**                              |

**⭐ RECOMMENDED:** Use `POST /adamo/session-with-results` for complete evaluations.

---

### Step 3: Map Your MapTool Evaluation Types

All 9 MapTool evaluation types use the **SAME endpoint** - just change these 3 parameters:

| MapTool Evaluation | `stage` | `subStage` | `segment`    |
| ------------------ | ------- | ---------- | ------------ |
| MAP 1.1            | "MAP 1" | 1          | "CP" or "FF" |
| MAP 1.2 CP         | "MAP 1" | 2          | "CP"         |
| MAP 1.2 FF         | "MAP 1" | 2          | "FF"         |
| MAP 1.3 CP         | "MAP 1" | 3          | "CP"         |
| MAP 2.1 CP         | "MAP 2" | 1          | "CP"         |
| MAP 2.1 FF         | "MAP 2" | 1          | "FF"         |
| MAP 2.2 CP         | "MAP 2" | 2          | "CP"         |
| MAP 2.2 FF         | "MAP 2" | 2          | "FF"         |
| MAP 3.0 FF         | "MAP 3" | 0          | "FF"         |

---

### Step 4: Integration Code Example

**C# Example - Sync MAP 1.1 Evaluation:**

```csharp
public async Task SyncEvaluationToAdamo(int assessmentId)
{
    // 1. Fetch from your MapTool database
    var assessment = await _mapToolContext.Assessments
        .Include(a => a.Map1_1Evaluations)
            .ThenInclude(e => e.MoleculeEvaluations)
                .ThenInclude(me => me.Molecule)
        .FirstOrDefaultAsync(a => a.Id == assessmentId);

    var evaluation = assessment.Map1_1Evaluations.FirstOrDefault();

    // 2. Build the payload
    var payload = new
    {
        session = new
        {
            stage = "MAP 1",              // ← Change based on eval type
            subStage = 1,                 // ← Change based on eval type
            segment = assessment.Segment, // ← "CP" or "FF"
            region = assessment.Region,
            evaluationDate = assessment.DateTime,
            participants = evaluation.Participants
        },
        results = evaluation.MoleculeEvaluations.Select(me => new
        {
            grNumber = me.Molecule.GrNumber,
            odor = me.Odor0h,
            result = me.ResultCP ?? me.ResultFF,
            dilution = "10% in DPG",
            benchmarkComments = me.Benchmark
        }).ToList()
    };

    // 3. Send to Adamo API
    using var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri("http://localhost:8085");

    var response = await httpClient.PostAsJsonAsync(
        "/adamo/session-with-results",
        payload
    );

    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Console.WriteLine($"✓ Synced to Adamo: Session ID {result.data.session.sessionId}");
        return true;
    }

    return false;
}
```

**For other evaluation types:** Just change `stage`, `subStage`, and `segment` values!

---

### Step 5: Test with Postman First

**Before integrating into MapTool, test with Postman:**

1. Import: `MAP2ADAMOINT-Creation-Endpoints.postman_collection.json`
2. Set `baseUrl` = `http://localhost:8085`
3. Run: "Create SESSION with RESULTS (Combined)"
4. Verify data appears in Oracle

**Once working in Postman, you know the API works!**

---

## 📡 API Endpoint Details

### POST /adamo/session-with-results ⭐ (MOST IMPORTANT)

**URL:** `http://localhost:8085/adamo/session-with-results`

**Request Format:**

```json
{
  "session": {
    "stage": "MAP 1", // Required: "MAP 1", "MAP 2", "MAP 3", etc.
    "subStage": 1, // Optional: 0, 1, 2, or 3
    "segment": "CP", // Required: "CP" or "FF"
    "region": "US", // Optional: "US", "EU", "AS", etc.
    "evaluationDate": "2025-11-04T14:00:00Z", // Optional
    "participants": "Smith, Jones, Williams" // Optional
  },
  "results": [
    {
      "grNumber": "GR-87-1010-1", // Required: Must match GR-YY-NNNN-B format
      "odor": "Fresh citrus", // Optional
      "result": 5, // Optional: 1-5 score
      "dilution": "10% in DPG", // Optional
      "benchmarkComments": "..." // Optional
    }
    // Add as many results as you need
  ]
}
```

**Response (201 Created):**

```json
{
  "status": "success",
  "message": "MAP_SESSION created with 2 results successfully",
  "data": {
    "session": { "sessionId": 13602, ... },
    "results": [ ... ],
    "resultCount": 2
  }
}
```

**What it does:**

1. ✅ Creates 1 MAP_SESSION in Oracle
2. ✅ Creates N MAP_RESULT records in Oracle
3. ✅ All in ONE atomic transaction (all-or-nothing)
4. ✅ Auto-generates all IDs
5. ✅ Auto-populates `createdBy` as "MAPTOOL"

---

## 🔧 Configuration

### Required Oracle Connection

**File:** `appsettings.Docker.json` (if using Docker) or `appsettings.json`

```json
{
  "ConnectionStrings": {
    "AdamoDb": "User Id=GIV_MAP;Password=<password>;Data Source=<oracle-host>:1521/FREEPDB1"
  }
}
```

**For Docker:** Use container name if Oracle is also in Docker (e.g., `oracle-map-db:1521`)  
**For Local:** Use `localhost:PORT` or IP address

### No PostgreSQL/MapTool Configuration Needed

This API only **writes to Oracle**. You don't need to configure the MapTool database connection unless you're using other endpoints.

---

## 📋 Integration Checklist

- [ ] Clone repository
- [ ] Configure Oracle connection in `appsettings.Docker.json`
- [ ] Build and start API (`docker-compose up -d`)
- [ ] Test with Postman using provided collection
- [ ] Verify data in Oracle
- [ ] Implement integration code in MapTool
- [ ] Test with MapTool data
- [ ] Deploy to production

---

## ⚠️ Important Notes

### GR_NUMBER Format

**Valid:** `GR-87-1234-1` (GR-YY-NNNN-B or GR-YY-NNNNN-B)  
**Invalid:** `INVALID-123` → Returns 400 validation error

### UPSERT Behavior (MAP_INITIAL)

- First call with GR_NUMBER → Creates new record
- Second call with same GR_NUMBER → **Updates existing record**
- No duplicate errors!

### Auto-Population

`createdBy` is automatically set to `"MAPTOOL"` if not provided in request.

### Field Mappings

**See:** [guides/MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md](./guides/MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md) for complete field mapping between MapTool and Adamo.

---

## 🆘 Troubleshooting

### Issue: Connection refused

**Solution:** Check Oracle connection string, verify Oracle is running

### Issue: 404 Not Found

**Solution:** Rebuild Docker container: `docker-compose build --no-cache map2adamoint`

### Issue: 400 Validation Error

**Solution:** Check error message - usually GR_NUMBER format or required field missing

### Issue: Sequence does not exist

**Solution:** Create Oracle sequences (see [setup/adamo-DATABASE_STRUCTURE.md](./setup/adamo-DATABASE_STRUCTURE.md))

---

## 📚 Complete Documentation

**For detailed integration:**

- **[guides/MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md](./guides/MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md)** - Complete integration guide with helper methods
- **[endpoints/MAP_RESULT_ENDPOINTS.md](./endpoints/MAP_RESULT_ENDPOINTS.md)** - Full endpoint documentation
- **[reference/ALL_ENDPOINTS.md](./reference/ALL_ENDPOINTS.md)** - All 46 endpoints reference

---

## 🎯 Quick Test

**1. Test endpoint is alive:**

```bash
curl http://localhost:8085/health
```

**2. Test Oracle connection:**

```bash
curl http://localhost:8085/debug/test-oracle
```

**3. Create test data:**

```bash
curl -X POST http://localhost:8085/adamo/session-with-results \
  -H "Content-Type: application/json" \
  -d @test-create-session-with-results.json
```

**4. Verify in Oracle:**

```sql
SELECT * FROM GIV_MAP.MAP_SESSION WHERE CREATED_BY = 'MAPTOOL';
SELECT * FROM GIV_MAP.MAP_RESULT WHERE CREATED_BY = 'MAPTOOL';
```

---

## 🐳 Docker Deployment

**On developer's machine:**

1. Clone repo
2. Update `appsettings.Docker.json` with their Oracle connection
3. Build: `docker-compose build map2adamoint`
4. Run: `docker-compose up -d map2adamoint`
5. API available at: `http://localhost:8085`

**Network requirements:**

- API needs network access to Oracle database
- If Oracle is in Docker on same host, use Docker network
- If Oracle is remote, ensure firewall allows connection

---

## 🎉 You're Ready!

**Give this document to your developer.** They should be able to:

- ✅ Get the API running
- ✅ Configure Oracle connection
- ✅ Integrate with MapTool
- ✅ Deploy to production

**Questions?** See complete docs in `/docs` folder or contact the API development team.

---

**Last Updated:** November 4, 2025  
**API Version:** 1.0  
**Status:** Production Ready
