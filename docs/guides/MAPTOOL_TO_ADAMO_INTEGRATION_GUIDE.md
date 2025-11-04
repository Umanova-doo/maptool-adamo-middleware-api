# MapTool to Adamo Integration - Complete Guide

## Overview

This guide shows how to map **all 9 MapTool evaluation types** to the Adamo database using the new creation endpoints.

**Key Insight:** All MapTool evaluations map to the same Adamo tables (MAP_SESSION + MAP_RESULT), just with different `stage`, `subStage`, and `segment` values!

---

## The Complete Solution: 4 Endpoints

### Endpoints Created

1. **`POST /adamo/initial`** - Create MAP_INITIAL (molecule info)
2. **`POST /adamo/session`** - Create MAP_SESSION (session metadata)
3. **`POST /adamo/result`** - Create MAP_RESULT (individual molecule result)
4. **`POST /adamo/session-with-results`** ⭐ - **RECOMMENDED** (session + results in one transaction)

### Why the Combined Endpoint?

You identified the exact problem: **MAP_RESULT needs SESSION_ID**, which you only get after creating the session!

The **`POST /adamo/session-with-results`** endpoint solves this by:

- ✅ Creating the session
- ✅ Getting the SESSION_ID
- ✅ Creating all results with that SESSION_ID
- ✅ All in ONE atomic transaction
- ✅ If anything fails, EVERYTHING rolls back (no orphaned sessions!)

---

## MapTool Evaluation Type Mapping

### Master Mapping Table

| MapTool Evaluation | Adamo STAGE | Adamo SUB_STAGE | Adamo SEGMENT    | Notes                        |
| ------------------ | ----------- | --------------- | ---------------- | ---------------------------- |
| **MAP 1.1**        | `"MAP 1"`   | `1`             | `"CP"` or `"FF"` | Initial basic evaluation     |
| **MAP 1.2 CP**     | `"MAP 1"`   | `2`             | `"CP"`           | CP panel evaluation          |
| **MAP 1.2 FF**     | `"MAP 1"`   | `2`             | `"FF"`           | FF panel evaluation          |
| **MAP 1.3 CP**     | `"MAP 1"`   | `3`             | `"CP"`           | Advanced CP evaluation       |
| **MAP 2.1 CP**     | `"MAP 2"`   | `1`             | `"CP"`           | Multi-perfume CP testing     |
| **MAP 2.1 FF**     | `"MAP 2"`   | `1`             | `"FF"`           | Trial composition FF testing |
| **MAP 2.2 CP**     | `"MAP 2"`   | `2`             | `"CP"`           | Complex CP with minimaps     |
| **MAP 2.2 FF**     | `"MAP 2"`   | `2`             | `"FF"`           | Complex FF with minimaps     |
| **MAP 3.0 FF**     | `"MAP 3"`   | `0`             | `"FF"`           | Final FF regional evaluation |

**Pattern:** Use `SUB_STAGE` to differentiate between evaluation subtypes (1.1, 1.2, 1.3, etc.) within the same `STAGE`.

---

## Integration Examples for Each MapTool Evaluation Type

### 1. MAP 1.1 Evaluation → Adamo

**MapTool Data:**

```csharp
var assessment = { Stage: "MAP 1.1", Region: "US", Segment: "CP" };
var evaluation = { Participants: "Smith, Jones", EvaluationDate: "2025-11-03" };
var moleculeEvals = [
  { GrNumber: "GR-25-0001-1", Odor0h: "Fresh citrus", ResultCP: 5 },
  { GrNumber: "GR-25-0002-1", Odor0h: "Woody base", ResultCP: 4 }
];
```

**Adamo Payload:**

```json
{
  "session": {
    "stage": "MAP 1",
    "subStage": 1,
    "segment": "CP",
    "region": "US",
    "evaluationDate": "2025-11-03T00:00:00Z",
    "participants": "Smith, Jones",
    "createdBy": "MAPTOOL"
  },
  "results": [
    {
      "grNumber": "GR-25-0001-1",
      "odor": "Fresh citrus",
      "result": 5,
      "dilution": "10% in DPG"
    },
    {
      "grNumber": "GR-25-0002-1",
      "odor": "Woody base",
      "result": 4,
      "dilution": "10% in DPG"
    }
  ]
}
```

**Endpoint:** `POST /adamo/session-with-results`

---

### 2. MAP 1.2 CP Evaluation → Adamo

**MapTool Data:**

```csharp
var assessment = { Stage: "MAP 1.2 CP", Region: "US", Segment: "CP" };
var evaluation = { PanelistId: 5, BaseId: 3 };
var moleculeEvals = [
  { MoleculeId: 456, ResultCP: 4, Comment: "Strong lasting effect" }
];
```

**Adamo Payload:**

```json
{
  "session": {
    "stage": "MAP 1",
    "subStage": 2,
    "segment": "CP",
    "region": "US",
    "participants": "Panel A Members",
    "createdBy": "MAPTOOL"
  },
  "results": [
    {
      "grNumber": "GR-25-0001-1",
      "odor": "Strong lasting effect",
      "result": 4,
      "dilution": "2.5%"
    }
  ]
}
```

**Endpoint:** `POST /adamo/session-with-results`

---

### 3. MAP 1.2 FF Evaluation → Adamo

**MapTool Data:**

```csharp
var assessment = { Stage: "1.2FF", Region: "EU", Segment: "FF" };
var evaluation = { Dilution: 10.0, Participants: "Perfumer Panel A" };
var moleculeEvals = [
  { Odor0h: "Fresh citrus top note", ResultFF: 5 }
];
```

**Adamo Payload:**

```json
{
  "session": {
    "stage": "MAP 1",
    "subStage": 2,
    "segment": "FF",
    "region": "EU",
    "participants": "Perfumer Panel A",
    "createdBy": "MAPTOOL"
  },
  "results": [
    {
      "grNumber": "GR-25-0001-1",
      "odor": "Fresh citrus top note",
      "result": 5,
      "dilution": "10.0%"
    }
  ]
}
```

**Endpoint:** `POST /adamo/session-with-results`

---

### 4. MAP 2.1 CP Evaluation → Adamo

**MapTool Data:**

```csharp
var assessment = { Stage: "MAP 2.1 CP", Region: "US", Segment: "CP" };
var perfumeConfigs = [
  {
    PerfumeName: "Floral Base A",
    MoleculeEvaluations: [
      { GrNumber: "GR-25-0001-1", ResultCP: 5, Comment: "Excellent in floral base" }
    ]
  }
];
```

**Adamo Payload:**

```json
{
  "session": {
    "stage": "MAP 2",
    "subStage": 1,
    "segment": "CP",
    "region": "US",
    "createdBy": "MAPTOOL"
  },
  "results": [
    {
      "grNumber": "GR-25-0001-1",
      "odor": "Excellent in floral base",
      "result": 5,
      "sponsor": "Floral Base A"
    }
  ]
}
```

**Note:** For MAP 2.1+ with multiple perfume configurations, you can use the `sponsor` field to store the perfume base name.

**Endpoint:** `POST /adamo/session-with-results`

---

### 5. MAP 3.0 FF Evaluation → Adamo

**MapTool Data:**

```csharp
var assessment = { Stage: "MAP 3.0 FF", Region: "Global", Segment: "FF" };
var countryConfigs = [
  {
    CountryName: "United States",
    MoleculeEvals: [
      { GrNumber: "GR-25-0001-1", Result: 5, Comment: "Excellent reception in US" }
    ]
  }
];
```

**Adamo Payload:**

```json
{
  "session": {
    "stage": "MAP 3",
    "subStage": 0,
    "segment": "FF",
    "region": "US",
    "createdBy": "MAPTOOL"
  },
  "results": [
    {
      "grNumber": "GR-25-0001-1",
      "odor": "Excellent reception in US",
      "result": 5,
      "sponsor": "United States Market"
    }
  ]
}
```

**Endpoint:** `POST /adamo/session-with-results`

---

## Universal Integration Pattern

### For ANY MapTool Evaluation Type

**Step 1: Determine Stage/SubStage/Segment**

Use the mapping table above to convert MapTool's evaluation type to Adamo's fields.

**Step 2: Extract Session Data**

```csharp
var sessionData = new CreateMapSessionRequest
{
    Stage = DetermineAdamoStage(assessment.Stage),
    SubStage = DetermineSubStage(assessment.Stage),
    Segment = assessment.Segment,
    Region = assessment.Region,
    EvaluationDate = assessment.DateTime,
    Participants = GetParticipants(evaluation),
    CreatedBy = "MAPTOOL"
};
```

**Step 3: Extract Results Data**

```csharp
var resultsData = GetAllMoleculeEvaluations(evaluation)
    .Select(me => new MapResultItem
    {
        GrNumber = me.Molecule.GrNumber,
        Odor = GetOdorDescription(me),
        Result = me.ResultCP ?? me.ResultFF,
        Dilution = GetDilution(me),
        BenchmarkComments = me.Benchmark ?? me.Comment,
        Sponsor = GetSponsor(me) // Perfume name, country, etc.
    })
    .ToList();
```

**Step 4: Send Combined Request**

```csharp
var payload = new CreateSessionWithResultsRequest
{
    Session = sessionData,
    Results = resultsData
};

var response = await httpClient.PostAsJsonAsync(
    "http://adamo-api/adamo/session-with-results",
    payload
);
```

---

## Helper Method Examples

### Determine Adamo Stage from MapTool Stage

```csharp
public static string DetermineAdamoStage(string mapToolStage)
{
    if (mapToolStage.StartsWith("MAP 1")) return "MAP 1";
    if (mapToolStage.StartsWith("MAP 2")) return "MAP 2";
    if (mapToolStage.StartsWith("MAP 3")) return "MAP 3";

    // Default
    return "MAP 1";
}
```

### Determine SubStage from MapTool Stage

```csharp
public static int? DetermineSubStage(string mapToolStage)
{
    var mapping = new Dictionary<string, int>
    {
        { "MAP 1.1", 1 },
        { "MAP 1.2 CP", 2 },
        { "MAP 1.2 FF", 2 },
        { "MAP 1.3 CP", 3 },
        { "MAP 2.1 CP", 1 },
        { "MAP 2.1 FF", 1 },
        { "MAP 2.2 CP", 2 },
        { "MAP 2.2 FF", 2 },
        { "MAP 3.0 FF", 0 }
    };

    return mapping.TryGetValue(mapToolStage, out var subStage)
        ? subStage
        : (int?)null;
}
```

### Get Participants from Different Evaluation Types

```csharp
public static string GetParticipants(object evaluation)
{
    return evaluation switch
    {
        Map1_1Evaluation map11 => map11.Participants,
        Map1_2CPEvaluation map12cp => map12cp.Panelist?.Name ?? "Panel",
        Map1_2FFEvaluation map12ff => map12ff.Participants,
        Map2_1CPEvaluation map21cp => map21cp.Panelist?.Name ?? "Panel",
        _ => "Unknown"
    };
}
```

### Get Odor Description from Different Evaluation Types

```csharp
public static string GetOdorDescription(object moleculeEvaluation)
{
    return moleculeEvaluation switch
    {
        Map1_1MoleculeEvaluation map11me => map11me.Odor0h,
        Map1_2CPMoleculeEvaluation map12cpme => map12cpme.Comment,
        Map1_2FFMoleculeEvaluation map12ffme => map12ffme.Odor0h,
        Map2_1CPMoleculeEvaluationData map21data => map21data.Comment,
        _ => string.Empty
    };
}
```

---

## Complete Integration Example

### Sync Any MapTool Evaluation to Adamo

```csharp
public async Task<IActionResult> SyncMapToolEvaluationToAdamo(
    int assessmentId,
    string evaluationType)
{
    // 1. Fetch MapTool data
    var assessment = await _mapToolContext.Assessments
        .Include(a => a.Map1_1Evaluations)
            .ThenInclude(e => e.MoleculeEvaluations)
                .ThenInclude(me => me.Molecule)
        .FirstOrDefaultAsync(a => a.Id == assessmentId);

    if (assessment == null)
        return NotFound("Assessment not found");

    // 2. Determine Adamo stage parameters
    var adamoStage = DetermineAdamoStage(assessment.Stage);
    var adamoSubStage = DetermineSubStage(assessment.Stage);

    // 3. Build session data
    var sessionRequest = new CreateMapSessionRequest
    {
        Stage = adamoStage,
        SubStage = adamoSubStage,
        Segment = assessment.Segment,
        Region = assessment.Region,
        EvaluationDate = assessment.DateTime,
        Participants = GetParticipantsForEvaluation(assessment, evaluationType),
        Category = assessment.Segment,
        CreatedBy = "MAPTOOL"
    };

    // 4. Build results data
    var resultsData = new List<MapResultItem>();

    switch (evaluationType)
    {
        case "Map1_1":
            var map11Eval = assessment.Map1_1Evaluations?.FirstOrDefault();
            if (map11Eval != null)
            {
                resultsData = map11Eval.MoleculeEvaluations
                    .Select(me => new MapResultItem
                    {
                        GrNumber = me.Molecule.GrNumber,
                        Odor = me.Odor0h,
                        Result = me.ResultCP ?? me.ResultFF,
                        Dilution = GetDilutionName(me.GrDilutionSolventId),
                        BenchmarkComments = me.Benchmark
                    })
                    .ToList();
            }
            break;

        case "Map1_2CP":
            // Similar extraction for Map1_2CP
            break;

        // Add cases for other evaluation types...
    }

    // 5. Send to Adamo
    var payload = new CreateSessionWithResultsRequest
    {
        Session = sessionRequest,
        Results = resultsData
    };

    using var httpClient = new HttpClient();
    var response = await httpClient.PostAsJsonAsync(
        "http://adamo-api/adamo/session-with-results",
        payload
    );

    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<dynamic>();

        _logger.LogInformation(
            "Synced MapTool Assessment {AssessmentId} to Adamo Session {SessionId} with {Count} results",
            assessmentId,
            result.data.session.sessionId,
            result.data.resultCount
        );

        return Ok(new
        {
            message = "Synced to Adamo successfully",
            adamoSessionId = result.data.session.sessionId,
            resultCount = result.data.resultCount,
            adamoStage = adamoStage,
            adamoSubStage = adamoSubStage
        });
    }

    return StatusCode((int)response.StatusCode, "Failed to sync to Adamo");
}
```

---

## Field Mapping Reference

### Session Fields

| MapTool Field             | Adamo Field                   | Source                      |
| ------------------------- | ----------------------------- | --------------------------- |
| `Assessment.Stage`        | `MAP_SESSION.STAGE`           | Mapped via lookup table     |
| `Assessment.Stage`        | `MAP_SESSION.SUB_STAGE`       | Extracted from stage string |
| `Assessment.Segment`      | `MAP_SESSION.SEGMENT`         | Direct copy                 |
| `Assessment.Region`       | `MAP_SESSION.REGION`          | Direct copy                 |
| `Assessment.DateTime`     | `MAP_SESSION.EVALUATION_DATE` | Direct copy                 |
| `Evaluation.Participants` | `MAP_SESSION.PARTICIPANTS`    | Varies by evaluation type   |
| `Assessment.Segment`      | `MAP_SESSION.CATEGORY`        | Direct copy (optional)      |

### Result Fields

| MapTool Field                            | Adamo Field                     | Source                            |
| ---------------------------------------- | ------------------------------- | --------------------------------- |
| _(auto)_                                 | `MAP_RESULT.SESSION_ID`         | Generated from session creation   |
| `Molecule.GrNumber`                      | `MAP_RESULT.GR_NUMBER`          | Direct copy                       |
| `MoleculeEvaluation.Odor0h` (or similar) | `MAP_RESULT.ODOR`               | Varies by evaluation type         |
| `MoleculeEvaluation.ResultCP/ResultFF`   | `MAP_RESULT.RESULT`             | Use CP or FF depending on segment |
| `MoleculeEvaluation.Benchmark/Comment`   | `MAP_RESULT.BENCHMARK_COMMENTS` | Direct copy                       |
| `DilutionSolvent.Name` or Dilution       | `MAP_RESULT.DILUTION`           | Lookup or direct value            |
| `PerfumeName` or `CountryName`           | `MAP_RESULT.SPONSOR`            | Context-dependent                 |

---

## Handling Different Evaluation Complexities

### Simple Evaluations (MAP 1.1, 1.2)

**Characteristics:**

- One evaluation per assessment
- Direct molecule evaluations
- Simple one-to-one mapping

**Strategy:** Straightforward conversion as shown above

---

### Complex Evaluations (MAP 2.1, 2.2)

**Characteristics:**

- Multiple perfume configurations
- Trial compositions
- Nested data structures

**Strategy:** Flatten the hierarchy

```csharp
// MAP 2.1 CP has nested structure:
// Evaluation → PerfumeConfigurations → MoleculeEvaluations → Data

var allResults = new List<MapResultItem>();

foreach (var perfumeConfig in evaluation.Map2_1CPPerfumeConfigurations)
{
    foreach (var moleculeEval in perfumeConfig.Map2_1CPMoleculeEvaluations)
    {
        var molecule = moleculeEval.Molecule;

        // Get aggregated or representative data
        var representativeData = moleculeEval.Map2_1CPMoleculeEvaluationData.FirstOrDefault();

        allResults.Add(new MapResultItem
        {
            GrNumber = molecule.GrNumber,
            Odor = representativeData?.Comment ?? moleculeEval.Comment,
            Result = moleculeEval.ResultCP,
            Sponsor = perfumeConfig.PerfumeName, // Store perfume base name
            BenchmarkComments = $"Perfume: {perfumeConfig.PerfumeName}, Dosage: {perfumeConfig.DosagePercentage}"
        });
    }
}
```

---

### Evaluations with Minimaps (MAP 2.2)

**Characteristics:**

- Has a "Minimap" quick assessment tab
- Regular detailed evaluation PLUS minimap

**Strategy:** Choose which to sync

**Option A:** Sync only detailed evaluation (ignore minimap)  
**Option B:** Sync minimap as separate session with different SubStage  
**Option C:** Combine both in BENCHMARK_COMMENTS

---

## Reusability: Same Endpoints for All Evaluation Types

**YES!** You're absolutely correct. You'll use the **same `POST /adamo/session-with-results` endpoint** for all MapTool evaluation types, just changing:

1. **`stage`** - "MAP 1", "MAP 2", or "MAP 3"
2. **`subStage`** - 0, 1, 2, or 3 (to differentiate 1.1, 1.2, 1.3, etc.)
3. **`segment`** - "CP" or "FF"
4. **`region`** - "US", "EU", "AS", etc.

**The rest of the payload structure stays the same!**

---

## Common Scenarios

### Scenario 1: Sync All Assessments

```csharp
public async Task SyncAllAssessmentsToAdamo()
{
    var assessments = await _mapToolContext.Assessments
        .Where(a => !a.IsArchived && !a.IsClosed)
        .ToListAsync();

    foreach (var assessment in assessments)
    {
        var evaluationType = DetermineEvaluationType(assessment.Stage);
        await SyncMapToolEvaluationToAdamo(assessment.Id, evaluationType);
    }
}
```

### Scenario 2: Sync Only New/Modified Assessments

```csharp
public async Task SyncRecentAssessmentsToAdamo(DateTime since)
{
    var assessments = await _mapToolContext.Assessments
        .Where(a => !a.IsArchived && a.UpdatedAt >= since)
        .ToListAsync();

    foreach (var assessment in assessments)
    {
        // Check if already synced (store mapping in a tracking table)
        var alreadySynced = await _trackingContext.SyncRecords
            .AnyAsync(sr => sr.MapToolAssessmentId == assessment.Id);

        if (!alreadySynced)
        {
            var result = await SyncMapToolEvaluationToAdamo(
                assessment.Id,
                DetermineEvaluationType(assessment.Stage)
            );

            if (result.IsSuccess)
            {
                // Track the sync
                await _trackingContext.SyncRecords.AddAsync(new SyncRecord
                {
                    MapToolAssessmentId = assessment.Id,
                    AdamoSessionId = result.AdamoSessionId,
                    SyncedAt = DateTime.UtcNow
                });
                await _trackingContext.SaveChangesAsync();
            }
        }
    }
}
```

### Scenario 3: Real-Time Sync on Save

Add webhook to MapTool save operations:

```csharp
[HttpPost("SaveMap1_1Evaluation")]
public async Task<IActionResult> SaveMap1_1Evaluation(
    [FromBody] Map1_1EvaluationIM evaluationIM)
{
    // Save to MapTool database
    var response = Map1_1EvaluationLoader.SaveMap1_1Evaluation(
        _dbContext,
        evaluationIM,
        User.Identity.Name
    );

    if (response.Success)
    {
        // Sync to Adamo immediately
        try
        {
            await SyncToAdamoAsync(response.ObjectId, "Map1_1");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync to Adamo, but MapTool save succeeded");
            // Don't fail the request - log for retry later
        }

        return Json(JsonResponseOb.GetSuccess(response.ObjectId, null));
    }

    return Json(JsonResponseOb.GetCustomError(response.ErrorMessages.FirstOrDefault()));
}
```

---

## Batch Processing Example

### Sync Multiple Evaluations Efficiently

```csharp
public async Task<BatchSyncResult> SyncBatchToAdamo(List<int> assessmentIds)
{
    var results = new BatchSyncResult();
    var httpClient = new HttpClient();

    foreach (var assessmentId in assessmentIds)
    {
        try
        {
            var payload = await BuildAdamoPayload(assessmentId);
            var response = await httpClient.PostAsJsonAsync(
                "http://adamo-api/adamo/session-with-results",
                payload
            );

            if (response.IsSuccessStatusCode)
            {
                results.SuccessCount++;
                var data = await response.Content.ReadFromJsonAsync<dynamic>();
                results.SyncedSessionIds.Add(data.data.session.sessionId);
            }
            else
            {
                results.FailCount++;
                results.Errors.Add($"Assessment {assessmentId}: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            results.FailCount++;
            results.Errors.Add($"Assessment {assessmentId}: {ex.Message}");
        }
    }

    return results;
}
```

---

## Best Practices

### 1. Always Use the Combined Endpoint for New Evaluations

```csharp
// ✅ GOOD - Atomic transaction
await CreateSessionWithResults(payload);

// ❌ AVOID - Multiple calls, no transaction safety
var session = await CreateSession(sessionData);
foreach (var result in results) {
    await CreateResult(session.SessionId, result); // If one fails, orphaned session!
}
```

### 2. Store Sync Mappings

Create a tracking table to avoid duplicate syncs:

```sql
CREATE TABLE SyncTracking (
    Id SERIAL PRIMARY KEY,
    MapToolAssessmentId INT NOT NULL,
    AdamoSessionId BIGINT NOT NULL,
    SyncedAt TIMESTAMP NOT NULL,
    UNIQUE (MapToolAssessmentId)
);
```

### 3. Handle Errors Gracefully

```csharp
try
{
    await SyncToAdamo(assessmentId);
}
catch (AdamoApiException ex) when (ex.StatusCode == 409)
{
    _logger.LogWarning("Assessment {Id} already synced to Adamo", assessmentId);
    // Continue - not a fatal error
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to sync assessment {Id}", assessmentId);
    // Queue for retry
    await _retryQueue.EnqueueAsync(assessmentId);
}
```

### 4. Validate Before Sending

```csharp
// Ensure required data exists
if (string.IsNullOrEmpty(molecule.GrNumber))
{
    _logger.LogWarning("Skipping molecule {Id} - no GR_NUMBER", molecule.Id);
    continue;
}

// Ensure results have scores
if (!moleculeEval.ResultCP.HasValue && !moleculeEval.ResultFF.HasValue)
{
    _logger.LogWarning("Skipping molecule {GrNumber} - no result score", molecule.GrNumber);
    continue;
}
```

---

## FAQ

**Q: Do I need to create MAP_INITIAL separately?**  
A: MAP_INITIAL is for basic molecule info. MAP_SESSION+RESULT is for evaluation sessions. They're independent - create both if you have the data.

**Q: Can I use the same GR_NUMBER in multiple sessions?**  
A: Yes! A molecule can have multiple MAP_RESULT records across different sessions. That's the whole point - tracking a molecule through different evaluation stages.

**Q: What if my MapTool evaluation has 50 molecules?**  
A: Perfect! Put all 50 in the `results` array. The combined endpoint handles bulk efficiently.

**Q: Can I update results after creation?**  
A: Not yet. PUT endpoints are planned for the next phase. For now, you'd need to delete and recreate (or contact DBA).

**Q: What about ODOR_CHARACTERIZATION?**  
A: That's a separate table for detailed odor profiling (100+ descriptor fields). Separate endpoints will be added for that.

**Q: Should I sync in real-time or batch?**  
A: Depends on your needs:

- **Real-time:** Sync immediately when evaluation is saved in MapTool
- **Batch:** Run periodic sync job (hourly, daily) for all new/updated evaluations

---

## Summary

✅ **One endpoint, all evaluation types:** Use `POST /adamo/session-with-results`  
✅ **Change only 3 fields:** `stage`, `subStage`, `segment`  
✅ **Everything else the same:** Session and result structure stays consistent  
✅ **Atomic and safe:** Transaction ensures all-or-nothing  
✅ **Efficient:** One API call instead of N+1

---

## Next Steps

1. ✅ Test the endpoint with sample data
2. ✅ Build helper methods for stage/substage mapping
3. ✅ Implement sync logic in MapTool
4. ✅ Add error handling and retry logic
5. ✅ Create sync tracking table
6. ✅ Test with all 9 evaluation types

**You're ready to integrate!** 🚀
