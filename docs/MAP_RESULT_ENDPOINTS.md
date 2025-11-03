# MAP_RESULT Creation Endpoints

## Overview

These endpoints allow you to create MAP_RESULT records that link molecules to evaluation sessions. MAP_RESULT stores the actual evaluation outcomes for molecules tested in panel sessions.

**Two approaches available:**

1. **Separate calls:** Create session first, then add results one-by-one
2. **Combined call:** Create session with all results in one atomic transaction (RECOMMENDED)

---

## Endpoints

### 1. Create MAP_RESULT Record (Individual)

**Endpoint:** `POST /adamo/result`

**Purpose:** Creates a single MAP_RESULT record linked to an existing MAP_SESSION.

**Use Case:** When you need to add individual results to an already-created session.

**Content-Type:** `application/json`

#### Request Body

| Field               | Type    | Required | Max Length | Description                      | Example                        |
| ------------------- | ------- | -------- | ---------- | -------------------------------- | ------------------------------ |
| `sessionId`         | long    | ✅ Yes   | -          | Foreign key to MAP_SESSION       | `5001`                         |
| `grNumber`          | string  | ✅ Yes   | 14         | Molecule identifier              | `"GR-25-0003-1"`               |
| `odor`              | string  | No       | 1000       | Odor description from evaluation | `"Rosy, floral, peonile..."`   |
| `benchmarkComments` | string  | No       | 2000       | Comments comparing to benchmarks | `"CP: Status 1, FF: Status 2"` |
| `result`            | integer | No       | -          | Numeric score (1-5 scale)        | `4`                            |
| `dilution`          | string  | No       | 20         | Dilution used                    | `"10% in DPG"`                 |
| `sponsor`           | string  | No       | 255        | Sponsor information              | `"Project Phoenix Team"`       |
| `createdBy`         | string  | No       | 8          | User creating record             | `"APIUSER"`                    |

#### Result Score Scale

| Score | Meaning   |
| ----- | --------- |
| 1     | Poor      |
| 2     | Fair      |
| 3     | Good      |
| 4     | Very Good |
| 5     | Excellent |

#### Auto-Generated Fields

- `resultId` - Primary key (auto-generated via sequence)
- `regNumber` - Extracted from `grNumber`
- `batch` - Extracted from `grNumber`
- `creationDate` - Automatically set
- `lastModifiedDate` - Automatically set

#### Example Request

```json
{
  "sessionId": 5001,
  "grNumber": "GR-25-0003-1",
  "odor": "Rosy, floral, peonile, geranium, interesting in DD but not powerful",
  "benchmarkComments": "CP: Comparable to commercial standard. FF: Shows unique character worth exploring further.",
  "result": 4,
  "dilution": "10% in DPG",
  "sponsor": "Project Phoenix Team",
  "createdBy": "APIUSER"
}
```

#### Success Response (201 Created)

```json
{
  "status": "success",
  "message": "MAP_RESULT record created successfully",
  "table": "MAP_RESULT",
  "data": {
    "resultId": 98765,
    "sessionId": 5001,
    "grNumber": "GR-25-0003-1",
    "odor": "Rosy, floral, peonile, geranium, interesting in DD but not powerful",
    "benchmarkComments": "CP: Comparable to commercial standard. FF: Shows unique character worth exploring further.",
    "result": 4,
    "dilution": "10% in DPG",
    "sponsor": "Project Phoenix Team",
    "creationDate": "2025-11-03T15:00:00Z",
    "createdBy": "APIUSER",
    "lastModifiedDate": "2025-11-03T15:00:00Z",
    "lastModifiedBy": "APIUSER",
    "regNumber": "GR-25-0003",
    "batch": 1,
    "session": null
  }
}
```

#### Error Responses

**404 Not Found - Session doesn't exist**

```json
{
  "status": "fail",
  "message": "MAP_SESSION with SESSION_ID 5001 not found",
  "hint": "Create the session first using POST /adamo/session or POST /adamo/session-with-results"
}
```

**400 Bad Request - Validation Error**

```json
{
  "status": "fail",
  "message": "Validation failed",
  "errors": [
    "SESSION_ID is required",
    "GR_NUMBER is required",
    "Result must be between 1 and 5"
  ]
}
```

---

### 2. Create MAP_SESSION with MAP_RESULT Records (Combined) ⭐ RECOMMENDED

**Endpoint:** `POST /adamo/session-with-results`

**Purpose:** Creates a MAP_SESSION and multiple MAP_RESULT records in ONE atomic transaction.

**Use Case:** When you have all session and molecule evaluation data ready (most common scenario).

**Content-Type:** `application/json`

**Key Benefit:** ✅ **Transaction safety** - If any part fails, everything rolls back. No orphaned sessions!

#### Request Body

The request has two main sections:

**1. `session` object** - Same structure as `POST /adamo/session`:

- `stage`, `evaluationDate`, `region`, `segment`, `participants`, etc.

**2. `results` array** - List of result items (minimum 1 required):

- Each item has: `grNumber`, `odor`, `benchmarkComments`, `result`, `dilution`, `sponsor`

#### Example Request

```json
{
  "session": {
    "stage": "MAP 1",
    "evaluationDate": "2025-11-03T14:00:00Z",
    "region": "US",
    "segment": "CP",
    "participants": "Johnson, Williams, Brown, Davis, Martinez",
    "showInTaskList": "Y",
    "subStage": 1,
    "category": "CP",
    "createdBy": "APIUSER"
  },
  "results": [
    {
      "grNumber": "GR-25-0010-1",
      "odor": "Fresh citrus top notes, bright and clean, with slight green undertones",
      "benchmarkComments": "Outperforms current standard in initial impact. Similar longevity.",
      "result": 5,
      "dilution": "10% in DPG",
      "sponsor": "Innovation Team A"
    },
    {
      "grNumber": "GR-25-0011-1",
      "odor": "Woody base with amber notes, warm and rounded character",
      "benchmarkComments": "Comparable to benchmark B. More natural feel.",
      "result": 4,
      "dilution": "10% in DPG",
      "sponsor": "Innovation Team A"
    },
    {
      "grNumber": "GR-25-0012-1",
      "odor": "Floral heart, rose and jasmine dominant, elegant and refined",
      "benchmarkComments": "Superior to current floral standard. Excellent balance.",
      "result": 5,
      "dilution": "5% in DPG",
      "sponsor": "Innovation Team B"
    }
  ]
}
```

#### Success Response (201 Created)

```json
{
  "status": "success",
  "message": "MAP_SESSION created with 3 results successfully",
  "table": "MAP_SESSION + MAP_RESULT",
  "data": {
    "session": {
      "sessionId": 5002,
      "stage": "MAP 1",
      "evaluationDate": "2025-11-03T14:00:00Z",
      "region": "US",
      "segment": "CP",
      "participants": "Johnson, Williams, Brown, Davis, Martinez",
      "showInTaskList": "Y",
      "creationDate": "2025-11-03T14:00:00Z",
      "createdBy": "APIUSER",
      "lastModifiedDate": "2025-11-03T14:00:00Z",
      "lastModifiedBy": "APIUSER",
      "subStage": 1,
      "category": "CP",
      "results": null
    },
    "results": [
      {
        "resultId": 98766,
        "sessionId": 5002,
        "grNumber": "GR-25-0010-1",
        "odor": "Fresh citrus top notes, bright and clean, with slight green undertones",
        "benchmarkComments": "Outperforms current standard in initial impact. Similar longevity.",
        "result": 5,
        "dilution": "10% in DPG",
        "sponsor": "Innovation Team A",
        "regNumber": "GR-25-0010",
        "batch": 1,
        "createdBy": "APIUSER",
        "creationDate": "2025-11-03T14:00:00Z"
      },
      {
        "resultId": 98767,
        "sessionId": 5002,
        "grNumber": "GR-25-0011-1",
        "odor": "Woody base with amber notes, warm and rounded character",
        "result": 4,
        "regNumber": "GR-25-0011",
        "batch": 1
      },
      {
        "resultId": 98768,
        "sessionId": 5002,
        "grNumber": "GR-25-0012-1",
        "odor": "Floral heart, rose and jasmine dominant, elegant and refined",
        "result": 5,
        "regNumber": "GR-25-0012",
        "batch": 1
      }
    ],
    "resultCount": 3
  }
}
```

#### Error Responses

**400 Bad Request - No Results Provided**

```json
{
  "status": "fail",
  "message": "Validation failed",
  "errors": ["At least one result is required"]
}
```

**400 Bad Request - Invalid Stage**

```json
{
  "status": "fail",
  "message": "Invalid stage value: 'MAP 5'",
  "validStages": [
    "MAP 0",
    "MAP 1",
    "MAP 2",
    "MAP 3",
    "ISC",
    "FIB",
    "FIM",
    "ISC (Quest)",
    "CARDEX",
    "RPMC"
  ]
}
```

**500 Internal Server Error - Rollback**

```json
{
  "status": "fail",
  "message": "Database error occurred while creating session with results. Transaction rolled back.",
  "error": "Detailed error message..."
}
```

---

## Which Endpoint Should I Use?

### Use `POST /adamo/session-with-results` when:

✅ You have complete session data with all molecule results ready  
✅ You want atomic transaction safety (all-or-nothing)  
✅ You're syncing from MapTool evaluations (most common use case)  
✅ You want to minimize round-trips

### Use `POST /adamo/session` + `POST /adamo/result` when:

✅ You need to add results incrementally over time  
✅ Results come from different sources/times  
✅ You want fine-grained control over each result  
✅ You're handling errors per-result

---

## Workflow Examples

### Workflow 1: Complete Evaluation (RECOMMENDED)

```javascript
// You have a complete MapTool evaluation ready
const payload = {
  session: {
    stage: mapToolAssessment.stage,
    evaluationDate: mapToolAssessment.dateTime,
    region: mapToolAssessment.region,
    segment: mapToolAssessment.segment,
    participants: mapToolEvaluation.participants,
    createdBy: "MAPTOOL",
  },
  results: mapToolMoleculeEvaluations.map((molEval) => ({
    grNumber: molEval.molecule.grNumber,
    odor: molEval.odor0h,
    result: molEval.resultCP || molEval.resultFF,
    dilution: molEval.dilution,
    benchmarkComments: molEval.benchmark,
  })),
};

const response = await axios.post("/adamo/session-with-results", payload);
console.log(
  `Created session ${response.data.data.session.sessionId} with ${response.data.data.resultCount} results`
);
```

### Workflow 2: Incremental Results

```javascript
// Step 1: Create session
const sessionResponse = await axios.post("/adamo/session", {
  stage: "MAP 1",
  segment: "CP",
  region: "US",
  createdBy: "APIUSER",
});

const sessionId = sessionResponse.data.data.sessionId;

// Step 2: Add results as they become available
for (const molecule of molecules) {
  await axios.post("/adamo/result", {
    sessionId: sessionId,
    grNumber: molecule.grNumber,
    odor: molecule.odorDescription,
    result: molecule.score,
    createdBy: "APIUSER",
  });
}
```

---

## Integration Mapping from MapTool

### Map1_1Evaluation → MAP_SESSION + MAP_RESULT

**MapTool Structure:**

```json
{
  "assessment": { "stage": "MAP 1.1", "region": "US", "segment": "CP" },
  "evaluation": { "participants": "Smith, Jones", "evaluationDate": "..." },
  "moleculeEvaluations": [
    {
      "molecule": { "grNumber": "GR-25-0001-1" },
      "odor0h": "Fresh citrus",
      "resultCP": 5,
      "dilution": "10%"
    }
  ]
}
```

**Maps to Adamo:**

```json
{
  "session": {
    "stage": "MAP 1",
    "region": "US",
    "segment": "CP",
    "participants": "Smith, Jones",
    "evaluationDate": "...",
    "subStage": 1
  },
  "results": [
    {
      "grNumber": "GR-25-0001-1",
      "odor": "Fresh citrus",
      "result": 5,
      "dilution": "10%"
    }
  ]
}
```

### Map1_2CPEvaluation → MAP_SESSION + MAP_RESULT

**MapTool Structure:**

```json
{
  "assessment": { "stage": "MAP 1.2 CP", "region": "US" },
  "evaluation": { "participants": "...", "baseId": 3 },
  "moleculeEvaluations": [
    {
      "moleculeId": 456,
      "grNumber": "GR-25-0001-1",
      "resultCP": 4,
      "comment": "Strong lasting effect"
    }
  ]
}
```

**Maps to Adamo:**

```json
{
  "session": {
    "stage": "MAP 1",
    "segment": "CP",
    "region": "US",
    "subStage": 2
  },
  "results": [
    {
      "grNumber": "GR-25-0001-1",
      "result": 4,
      "odor": "Strong lasting effect"
    }
  ]
}
```

---

## Important Considerations

### 1. Foreign Key Relationship

MAP_RESULT has a **required foreign key** to MAP_SESSION. You cannot create a result without a valid session.

The `POST /adamo/result` endpoint validates that the SESSION_ID exists before creating the result.

### 2. Transaction Safety

The `POST /adamo/session-with-results` endpoint uses database transactions:

- ✅ If session creation fails → nothing is saved
- ✅ If any result fails → entire operation rolls back
- ✅ All records get the same CreatedBy and CreationDate

### 3. Auto-Generated Fields

Like MAP_INITIAL, the database automatically:

- Generates `RESULT_ID` via sequence
- Extracts `REG_NUMBER` and `BATCH` from `GR_NUMBER`
- Sets audit timestamps

### 4. Cascade Delete

When a MAP_SESSION is deleted, all its MAP_RESULT records are automatically deleted (CASCADE DELETE configured in database).

---

## Postman Testing Guide

### Test 1: Create Individual Result

**Prerequisites:** You must have a valid SESSION_ID (create a session first)

1. Create session: `POST /adamo/session`
2. Note the `sessionId` from response
3. Create new POST request in Postman
4. URL: `http://localhost:5000/adamo/result`
5. Body → Raw → JSON → Copy from `test-create-map-result.json`
6. **Update `sessionId` field** with your actual session ID
7. Click **Send**

Expected: 201 Created with the new result including auto-generated `resultId`

### Test 2: Create Session with Results (RECOMMENDED)

1. Create new POST request in Postman
2. URL: `http://localhost:5000/adamo/session-with-results`
3. Body → Raw → JSON → Copy from `test-create-session-with-results.json`
4. Click **Send**

Expected: 201 Created with session + all results

### Test 3: Verify Created Results

```
GET http://localhost:5000/adamo/result/{resultId}
GET http://localhost:5000/adamo/session/{sessionId}  (includes results in response)
```

---

## Common Use Cases

### Use Case 1: Sync MapTool MAP 1.1 Evaluation

```json
{
  "session": {
    "stage": "MAP 1",
    "evaluationDate": "{{map11Evaluation.evaluationDate}}",
    "region": "{{assessment.region}}",
    "segment": "{{assessment.segment}}",
    "participants": "{{map11Evaluation.participants}}",
    "subStage": 1,
    "createdBy": "MAPTOOL"
  },
  "results": [
    {
      "grNumber": "{{moleculeEval1.molecule.grNumber}}",
      "odor": "{{moleculeEval1.odor0h}}",
      "result": "{{moleculeEval1.resultCP}}",
      "dilution": "{{moleculeEval1.grDilutionSolvent}}",
      "benchmarkComments": "{{moleculeEval1.benchmark}}"
    }
  ]
}
```

### Use Case 2: Sync MapTool MAP 1.2 CP Evaluation

```json
{
  "session": {
    "stage": "MAP 1",
    "evaluationDate": "{{evaluation.evaluationDate}}",
    "region": "{{assessment.region}}",
    "segment": "CP",
    "participants": "{{evaluation.panelist.name}}",
    "subStage": 2,
    "createdBy": "MAPTOOL"
  },
  "results": [
    {
      "grNumber": "{{moleculeEval.molecule.grNumber}}",
      "result": "{{moleculeEval.resultCP}}",
      "dilution": "{{moleculeEval.dilution}}%",
      "benchmarkComments": "{{moleculeEval.comment}}"
    }
  ]
}
```

### Use Case 3: Sync MapTool MAP 2.1 CP Evaluation

```json
{
  "session": {
    "stage": "MAP 2",
    "evaluationDate": "{{evaluation.dateTime}}",
    "region": "{{assessment.region}}",
    "segment": "CP",
    "subStage": 1,
    "createdBy": "MAPTOOL"
  },
  "results": [
    // Flatten all molecules from all perfume configurations
    {
      "grNumber": "{{perfumeConfig1.moleculeEval1.grNumber}}",
      "odor": "{{perfumeConfig1.moleculeEval1.comment}}",
      "result": "{{perfumeConfig1.moleculeEval1.resultCP}}",
      "sponsor": "{{perfumeConfig1.perfumeName}}"
    }
  ]
}
```

---

## Stage and SubStage Mapping from MapTool

| MapTool Evaluation | Adamo STAGE | Adamo SUB_STAGE | Adamo SEGMENT |
| ------------------ | ----------- | --------------- | ------------- |
| MAP 1.1            | "MAP 1"     | 1               | "CP" or "FF"  |
| MAP 1.2 CP         | "MAP 1"     | 2               | "CP"          |
| MAP 1.2 FF         | "MAP 1"     | 2               | "FF"          |
| MAP 1.3 CP         | "MAP 1"     | 3               | "CP"          |
| MAP 2.1 CP         | "MAP 2"     | 1               | "CP"          |
| MAP 2.1 FF         | "MAP 2"     | 1               | "FF"          |
| MAP 2.2 CP         | "MAP 2"     | 2               | "CP"          |
| MAP 2.2 FF         | "MAP 2"     | 2               | "FF"          |
| MAP 3.0 FF         | "MAP 3"     | 0               | "FF"          |

**Pattern:** Use SUB_STAGE to differentiate between evaluation subtypes within the same STAGE.

---

## Error Handling Best Practices

### For Individual Result Creation

```javascript
try {
  const result = await createMapResult({
    sessionId: sessionId,
    grNumber: "GR-25-0001-1",
    result: 5,
  });

  console.log("Result created:", result.data.resultId);
} catch (error) {
  if (error.response?.status === 404) {
    console.error("Session not found. Create session first.");
  } else if (error.response?.status === 400) {
    console.error("Validation error:", error.response.data.errors);
  } else {
    console.error("Unexpected error:", error.message);
  }
}
```

### For Combined Creation

```javascript
try {
  const response = await createSessionWithResults(fullPayload);

  console.log(`Session ${response.data.data.session.sessionId} created`);
  console.log(`${response.data.data.resultCount} results saved`);

  // All results have IDs
  response.data.data.results.forEach((result) => {
    console.log(
      `Result ID ${result.resultId}: ${result.grNumber} = ${result.result}`
    );
  });
} catch (error) {
  if (error.response?.status === 400) {
    console.error("Validation failed:", error.response.data.errors);
  } else {
    console.error("Transaction rolled back:", error.response.data.message);
  }
}
```

---

## Complete Integration Example

### Sync Entire MapTool Evaluation to Adamo

```csharp
public async Task<IActionResult> SyncMapToolEvaluationToAdamo(int assessmentId)
{
    // Fetch MapTool data
    var assessment = await _mapToolContext.Assessments
        .Include(a => a.Map1_1Evaluations)
            .ThenInclude(e => e.MoleculeEvaluations)
                .ThenInclude(me => me.Molecule)
        .FirstOrDefaultAsync(a => a.Id == assessmentId);

    if (assessment == null)
        return NotFound("Assessment not found");

    var evaluation = assessment.Map1_1Evaluations?.FirstOrDefault();
    if (evaluation == null)
        return NotFound("No evaluation found");

    // Build Adamo payload
    var payload = new CreateSessionWithResultsRequest
    {
        Session = new CreateMapSessionRequest
        {
            Stage = "MAP 1",
            EvaluationDate = evaluation.EvaluationDate,
            Region = assessment.Region,
            Segment = assessment.Segment,
            Participants = evaluation.Participants,
            SubStage = 1,
            Category = assessment.Segment,
            CreatedBy = "MAPTOOL"
        },
        Results = evaluation.MoleculeEvaluations
            .Select(me => new MapResultItem
            {
                GrNumber = me.Molecule.GrNumber,
                Odor = me.Odor0h,
                Result = me.ResultCP ?? me.ResultFF,
                Dilution = "10% in DPG", // From dilution solvent
                BenchmarkComments = me.Benchmark
            })
            .ToList()
    };

    // Send to Adamo
    using var httpClient = new HttpClient();
    var response = await httpClient.PostAsJsonAsync(
        "http://adamo-api/adamo/session-with-results",
        payload
    );

    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        return Ok(new
        {
            message = "Synced to Adamo successfully",
            adamoSessionId = result.data.session.sessionId,
            resultCount = result.data.resultCount
        });
    }

    return StatusCode((int)response.StatusCode, "Failed to sync to Adamo");
}
```

---

## Next Steps

After successfully creating sessions and results:

1. **Query results by session** - `GET /adamo/session/{id}` (includes results)
2. **Create ODOR_CHARACTERIZATION** - For detailed odor profiling
3. **Link CP/FF sessions** - Using MAP1_SESSION_LINK
4. **Build automation** - Periodic sync from MapTool to Adamo

---

## Support

- **Main Guide:** [MAP_INITIAL_SESSION_ENDPOINTS.md](./MAP_INITIAL_SESSION_ENDPOINTS.md)
- **All Endpoints:** [ALL_ENDPOINTS.md](./ALL_ENDPOINTS.md)
- **Database Schema:** [setup/adamo-DATABASE_STRUCTURE.md](./setup/adamo-DATABASE_STRUCTURE.md)
