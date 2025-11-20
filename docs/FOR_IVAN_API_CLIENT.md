# API Client for Ivan - Insert Data to Oracle

## 📄 One File

Copy `Services/MapToOracleApiClient.cs` into your MapTool project.

---

## ⚙️ Setup

### 1. Update API URL

Change line 15 in `MapToOracleApiClient.cs`:

```csharp
private const string API_BASE_URL = "https://your-hosted-api-url.com";
```

### 2. Register in Program.cs

```csharp
builder.Services.AddHttpClient<MapToOracleApiClient>();
builder.Services.AddScoped<MapToOracleApiClient>();
```

---

## 🎯 The 4 Key Endpoints

### 1. Create MAP_INITIAL (Molecule Evaluation)

Inserts molecule initial evaluation data into Oracle.

```csharp
var request = new CreateMapInitialRequest
{
    GrNumber = "GR-88-0681-1",
    Chemist = "John Doe",
    EvaluationDate = DateTime.Now,
    Dilution = "10% in DPG",
    Odor0H = "Fresh, citrus, green",
    Odor4H = "Floral, sweet",
    Odor24H = "Woody, musky",
    Comments = "Promising molecule",
    CreatedBy = "IVAN"
};

var response = await apiClient.CreateMapInitial(request);
```

---

### 2. Create MAP_SESSION (Evaluation Session)

Creates an evaluation session in Oracle. **Now supports UPSERT** - if you provide a `SessionLink` and it already exists, it will update the existing session instead of creating a duplicate.

```csharp
var request = new CreateMapSessionRequest
{
    Stage = "MAP 1",
    EvaluationDate = DateTime.Now,
    Region = "NA",
    Segment = "CP",
    Participants = "Ivan, Sarah, Mike",
    SessionLink = "MAPTOOL_SESSION_12345",  // Optional: Use to prevent duplicates
    CreatedBy = "IVAN"
};

var response = await apiClient.CreateMapSession(request);
// Response will include the auto-generated SessionId
// Response also includes isUpdate flag to indicate if session was updated or created
```

**How SessionLink works:**
- If `SessionLink` is provided and exists → **Updates** existing session
- If `SessionLink` is not provided or doesn't exist → **Creates** new session
- Use your MapTool session ID or unique identifier as the SessionLink value

---

### 3. Create MAP_RESULT (Result for Existing Session)

Adds a molecule result to an existing session.

```csharp
var request = new CreateMapResultRequest
{
    SessionId = 4111,  // Use SessionId from CreateMapSession response
    GrNumber = "GR-88-0681-1",
    Odor = "Fresh citrus with floral notes",
    BenchmarkComments = "Similar to benchmark A",
    Result = 4,  // Score 1-5
    Dilution = "10%",
    Sponsor = "Ivan",
    CreatedBy = "IVAN"
};

var response = await apiClient.CreateMapResult(request);
```

---

### 4. Create SESSION + RESULTS (All-in-One)

Creates a session and multiple results in one call (recommended). **Supports UPSERT** - use `SessionLink` to update existing sessions instead of creating duplicates.

```csharp
var request = new CreateSessionWithResultsRequest
{
    Session = new CreateMapSessionRequest
    {
        Stage = "MAP 1",
        EvaluationDate = DateTime.Now,
        Region = "NA",
        Segment = "CP",
        Participants = "Ivan, Sarah",
        SessionLink = "MAPTOOL_SESSION_12345",  // Prevents duplicate sessions
        CreatedBy = "IVAN"
    },
    Results = new List<CreateMapResultRequest>
    {
        new CreateMapResultRequest
        {
            GrNumber = "GR-88-0681-1",
            Odor = "Fresh citrus",
            Result = 4,
            Dilution = "10%",
            CreatedBy = "IVAN"
        },
        new CreateMapResultRequest
        {
            GrNumber = "GR-88-0682-1",
            Odor = "Floral sweet",
            Result = 5,
            Dilution = "10%",
            CreatedBy = "IVAN"
        }
    }
};

var response = await apiClient.CreateSessionWithResults(request);
```

---

## 🔧 Complete Example

```csharp
public class TestOracleInsert
{
    private readonly MapToOracleApiClient _apiClient;
    private readonly ILogger<TestOracleInsert> _logger;

    public TestOracleInsert(MapToOracleApiClient apiClient, ILogger<TestOracleInsert> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task RunTest()
    {
        // 1. Check API health
        if (!await _apiClient.TestHealthCheck())
        {
            _logger.LogError("API not responding");
            return;
        }

        // 2. Create MAP_INITIAL
        var initialRequest = new CreateMapInitialRequest
        {
            GrNumber = "GR-88-0681-1",
            Chemist = "Ivan",
            EvaluationDate = DateTime.Now,
            Odor0H = "Fresh citrus",
            CreatedBy = "IVAN"
        };
        var initialResponse = await _apiClient.CreateMapInitial(initialRequest);

        // 3. Create SESSION with RESULTS
        var sessionRequest = new CreateSessionWithResultsRequest
        {
            Session = new CreateMapSessionRequest
            {
                Stage = "MAP 1",
                Segment = "CP",
                EvaluationDate = DateTime.Now,
                CreatedBy = "IVAN"
            },
            Results = new List<CreateMapResultRequest>
            {
                new CreateMapResultRequest
                {
                    GrNumber = "GR-88-0681-1",
                    Odor = "Fresh citrus",
                    Result = 4,
                    CreatedBy = "IVAN"
                }
            }
        };
        var sessionResponse = await _apiClient.CreateSessionWithResults(sessionRequest);

        _logger.LogInformation("Test complete!");
    }
}
```

---

## ⚠️ Important Notes

### Session Link (Prevents Duplicates) 🆕

**NEW:** Use `SessionLink` to prevent duplicate session creation!

When creating sessions, provide a unique `SessionLink` (e.g., your MapTool session ID):

```csharp
Session = new CreateMapSessionRequest
{
    SessionLink = "MAPTOOL_SESSION_12345",  // Use your internal session ID
    Stage = "MAP 1",
    Segment = "CP",
    // ... other fields
}
```

**How it works:**
- First call with `SessionLink = "MAPTOOL_SESSION_12345"` → **Creates new** session
- Second call with same `SessionLink` → **Updates existing** session (no duplicate!)
- Response includes `isUpdate` or `isSessionUpdate` flag

**Benefits:**
- ✅ No more duplicate sessions when API is called multiple times
- ✅ Safe to retry failed requests
- ✅ Can update session details later
- ✅ Links MapTool sessions to ADAMO sessions

---

### GR Number Format

The API accepts multiple molecule number formats:

- **GR format**: `GR-YY-NNNN-B` (e.g., `GR-87-0857-0`)
- **TS format**: `TS-YY-NNNN-B` (e.g., `TS-87-0857-0`)
- **SL format**: `SL-NNNNNN-B` (e.g., `SL-123456-1`)

Examples:

- ✅ `GR-87-0857-0`
- ✅ `TS-87-0857-0`
- ✅ `GR-88-06811-1` (5 digits also supported)
- ✅ `SL-123456-1`

### Database Configuration

**The hosted API currently connects to a local Docker Oracle instance.**

Before you can insert data, you need to update the API's `appsettings.json` with your actual Oracle credentials:

```json
{
  "ConnectionStrings": {
    "AdamoDatabase": "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=YOUR_ORACLE_TNS"
  }
}
```

Ask your team for the production Oracle connection string and update the hosted API configuration.

### Valid Stage Values

- "MAP 0", "MAP 1", "MAP 2", "MAP 3"
- "ISC", "FIB", "FIM", "CARDEX", "RPMC"

### Valid Segment Values

- "CP" (Consumer Preference)
- "FF" (Fine Fragrance)

---

## 📊 Response Format

All endpoints return:

```csharp
{
    "status": "success",           // or "fail"
    "message": "Record created",
    "data": { /* created record */ }
}
```

On error:

```csharp
{
    "status": "fail",
    "message": "Error description",
    "errors": ["Detailed error 1", "Detailed error 2"]
}
```

---

## 🐛 Troubleshooting

**"Oracle not configured"**

- API needs Oracle connection string updated

**"Session not found"**

- When creating a result, make sure SessionId exists
- Use endpoint #4 (session-with-results) to create both together

**"Invalid stage value"**

- Check valid stage values above
- Stage is case-sensitive

**"503 Service Unavailable"**

- API cannot connect to Oracle database
- Check Oracle connection string in API configuration

---

That's it! 4 endpoints, simple usage. 🚀
