# New MAP_INITIAL and MAP_SESSION Endpoints - Summary

## What Was Created

**Four new POST endpoints** have been added to the API for creating records in the Oracle Adamo database. These are the **most important endpoints** for the MapTool → Adamo integration direction.

**⭐ RECOMMENDED:** Use `POST /adamo/session-with-results` for complete evaluations!

---

## Quick Reference

### 1. Create MAP_INITIAL (Initial Molecule Evaluation)

**Endpoint:** `POST /adamo/initial`

**Purpose:** Create a new molecule evaluation record with odor descriptions at 0h, 4h, and 24h.

**Minimal Request:**

```json
{
  "grNumber": "GR-25-0001-1"
}
```

**Full Request Example:** See `test-create-map-initial.json`

**Key Fields:**

- `grNumber` (REQUIRED) - Molecule identifier
- `evaluationDate` - When evaluated
- `chemist` - Who made it
- `assessor` - Who evaluated it
- `odor0H`, `odor4H`, `odor24H` - Odor descriptions at different time points
- `dilution` - Dilution used (e.g., "10% in DPG")
- `evaluationSite` - Site code (e.g., "US", "ZH")
- `comments` - Additional notes
- `createdBy` - Your username for audit trail

**Returns:** 201 Created with full record including auto-generated ID, REG_NUMBER, and BATCH

---

### 2. Create MAP_SESSION (Evaluation Session)

**Endpoint:** `POST /adamo/session`

**Purpose:** Create a new evaluation session for panel assessments.

**Minimal Request:**

```json
{
  "stage": "MAP 1",
  "segment": "CP"
}
```

**Full Request Example:** See `test-create-map-session.json`

**Key Fields:**

- `stage` - Session stage (see valid values below)
- `segment` - "CP" (Consumer Preference) or "FF" (Fine Fragrance)
- `region` - Geographic region (e.g., "US", "EU")
- `evaluationDate` - When session occurs
- `participants` - Comma-separated list of participants
- `subStage` - Sub-stage number (0-9)
- `category` - Session category
- `showInTaskList` - "Y" or "N" (default: "N")
- `createdBy` - Your username for audit trail

**Valid Stages:**

- `"MAP 0"` - Initial screening
- `"MAP 1"` - Initial evaluation
- `"MAP 2"` - Second evaluation
- `"MAP 3"` - Third evaluation
- `"ISC"` - International Sensory Committee
- `"FIB"` - Fine Ingredients Bench
- `"FIM"` - Fine Ingredients Meeting
- `"ISC (Quest)"` - ISC Quest evaluation
- `"CARDEX"` - Cardex evaluation
- `"RPMC"` - Regional Product Management Committee

**Returns:** 201 Created with full record including auto-generated SESSION_ID

---

### 3. Create MAP_RESULT (Individual Result)

**Endpoint:** `POST /adamo/result`

**Purpose:** Create a single result record linked to an existing session.

**Minimal Request:**

```json
{
  "sessionId": 5001,
  "grNumber": "GR-25-0003-1"
}
```

**Full Request Example:** See `test-create-map-result.json`

**Key Fields:**

- `sessionId` (REQUIRED) - Foreign key to MAP_SESSION
- `grNumber` (REQUIRED) - Molecule identifier
- `odor` - Odor description (max 1000 chars)
- `benchmarkComments` - Benchmark comparison (max 2000 chars)
- `result` - Numeric score (1-5 scale)
- `dilution` - Dilution used (e.g., "10% in DPG")
- `sponsor` - Sponsor information
- `createdBy` - Username for audit

**Returns:** 201 Created with full record including auto-generated RESULT_ID

---

### 4. Create SESSION with RESULTS (Combined) ⭐ RECOMMENDED

**Endpoint:** `POST /adamo/session-with-results`

**Purpose:** Create session AND multiple results in ONE atomic transaction.

**Example Request:** See `test-create-session-with-results.json`

**Structure:**

```json
{
  "session": {
    /* CreateMapSessionRequest */
  },
  "results": [
    /* Array of MapResultItem */
  ]
}
```

**Key Benefits:**

- ✅ **Atomic transaction** - All-or-nothing (if any fails, everything rolls back)
- ✅ **Single API call** - No need to manage SESSION_ID manually
- ✅ **Faster** - Reduces round-trips
- ✅ **Safer** - No orphaned sessions

**Returns:** 201 Created with session + all results + result count

---

## Files Created

### API Implementation

1. **Controllers/AdamoController.cs** (modified)

   - Added `POST /adamo/initial` endpoint
   - Added `POST /adamo/session` endpoint
   - Added `POST /adamo/result` endpoint
   - Added `POST /adamo/session-with-results` endpoint (atomic transaction)

2. **Models/DTOs/CreateMapInitialRequest.cs** (new)

   - Request DTO for MAP_INITIAL creation
   - Full validation attributes
   - Field length limits

3. **Models/DTOs/CreateMapSessionRequest.cs** (new)

   - Request DTO for MAP_SESSION creation
   - Stage validation helper class
   - Segment constants

4. **Models/DTOs/CreateMapResultRequest.cs** (new)

   - Request DTO for MAP_RESULT creation
   - Session validation

5. **Models/DTOs/CreateSessionWithResultsRequest.cs** (new)
   - Request DTO for combined session+results creation
   - Nested structure with session and results array

### Testing Files

6. **test-create-map-initial.json** (new)

   - Example request body for creating MAP_INITIAL
   - Ready to use in Postman

7. **test-create-map-session.json** (new)

   - Example request body for creating MAP_SESSION
   - Ready to use in Postman

8. **test-create-map-result.json** (new)

   - Example request body for creating MAP_RESULT
   - Ready to use in Postman

9. **test-create-session-with-results.json** (new)
   - Example request for combined session+results (RECOMMENDED)
   - Shows session with 3 molecule results

### Documentation

10. **docs/MAP_INITIAL_SESSION_ENDPOINTS.md** (new)

    - Comprehensive documentation for MAP_INITIAL and MAP_SESSION endpoints
    - Field descriptions, examples, error handling
    - Postman testing guide
    - Integration notes and use cases

11. **docs/MAP_RESULT_ENDPOINTS.md** (new)

    - Complete guide for MAP_RESULT endpoints
    - Combined session-with-results endpoint details
    - Workflow examples and integration mapping
    - MapTool evaluation type to Adamo stage mapping

12. **docs/ALL_ENDPOINTS.md** (updated)

    - Added new section for creation endpoints
    - Updated endpoint counts (4 creation endpoints)

13. **docs/NEW_ENDPOINTS_SUMMARY.md** (this file)
    - Quick reference guide for all 4 endpoints

---

## How to Test in Postman

### Test 1: Create MAP_INITIAL

1. Create a new **POST** request
2. URL: `http://localhost:5000/adamo/initial`
3. Headers: `Content-Type: application/json`
4. Body → Raw → JSON → Copy from `test-create-map-initial.json`
5. Click **Send**

**Expected Result:**

```json
{
  "status": "success",
  "message": "MAP_INITIAL record created successfully",
  "data": {
    "mapInitialId": 123456,
    "grNumber": "GR-25-0001-1",
    "regNumber": "GR-25-0001",
    "batch": 1,
    ...
  }
}
```

### Test 2: Create MAP_SESSION

1. Create a new **POST** request
2. URL: `http://localhost:5000/adamo/session`
3. Headers: `Content-Type: application/json`
4. Body → Raw → JSON → Copy from `test-create-map-session.json`
5. Click **Send**

**Expected Result:**

```json
{
  "status": "success",
  "message": "MAP_SESSION record created successfully",
  "data": {
    "sessionId": 5001,
    "stage": "MAP 1",
    "segment": "CP",
    ...
  }
}
```

### Test 3: Create Individual Result

1. Create a new **POST** request
2. URL: `http://localhost:5000/adamo/result`
3. Headers: `Content-Type: application/json`
4. Body → Raw → JSON → Copy from `test-create-map-result.json`
5. **Important:** Update `sessionId` to match a session you created
6. Click **Send**

**Expected Result:**

```json
{
  "status": "success",
  "message": "MAP_RESULT record created successfully",
  "data": {
    "resultId": 98765,
    "sessionId": 5001,
    "grNumber": "GR-25-0003-1",
    "regNumber": "GR-25-0003",
    "batch": 1,
    ...
  }
}
```

### Test 4: Create Session with Results (⭐ RECOMMENDED)

1. Create a new **POST** request
2. URL: `http://localhost:5000/adamo/session-with-results`
3. Headers: `Content-Type: application/json`
4. Body → Raw → JSON → Copy from `test-create-session-with-results.json`
5. Click **Send**

**Expected Result:**

```json
{
  "status": "success",
  "message": "MAP_SESSION created with 3 results successfully",
  "data": {
    "session": { "sessionId": 5002, ... },
    "results": [ /* 3 results with IDs */ ],
    "resultCount": 3
  }
}
```

### Test 5: Verify Created Records

Use existing GET endpoints:

- `GET /adamo/initial/{id}` - Get MAP_INITIAL by ID
- `GET /adamo/initial/gr/GR-25-0001-1` - Get MAP_INITIAL by GR_NUMBER
- `GET /adamo/session/{id}` - Get MAP_SESSION by ID (includes results)
- `GET /adamo/result/{id}` - Get MAP_RESULT by ID

---

## Features & Validation

### MAP_INITIAL Endpoint Features

✅ **Duplicate Detection** - Returns 409 Conflict if GR_NUMBER already exists  
✅ **Auto-Generation** - ID, REG_NUMBER, and BATCH generated by database  
✅ **Field Validation** - Length limits, format validation  
✅ **Audit Trail** - CreatedBy, CreatedDate tracked  
✅ **Complete Response** - Returns full record with auto-generated fields

### MAP_SESSION Endpoint Features

✅ **Stage Validation** - Validates against allowed stage values  
✅ **Helpful Errors** - Returns list of valid stages if invalid value provided  
✅ **Auto-Generation** - SESSION_ID generated by database sequence  
✅ **Field Validation** - Length limits, format validation  
✅ **Audit Trail** - CreatedBy, CreatedDate tracked  
✅ **Complete Response** - Returns full record with auto-generated ID

### MAP_RESULT Endpoint Features

✅ **Foreign Key Validation** - Checks SESSION_ID exists before creating  
✅ **Auto-Generation** - RESULT_ID, REG_NUMBER, BATCH generated by database  
✅ **Field Validation** - Length limits, result score range (1-5)  
✅ **Audit Trail** - CreatedBy, CreatedDate tracked  
✅ **Complete Response** - Returns full record with auto-generated fields

### SESSION-WITH-RESULTS Endpoint Features (⭐ RECOMMENDED)

✅ **Atomic Transaction** - All-or-nothing safety (rollback on any error)  
✅ **Single API Call** - No need to manage SESSION_ID manually  
✅ **Batch Processing** - Create multiple results efficiently  
✅ **Complete Validation** - Validates session and all results  
✅ **Full Response** - Returns session, all results, and count  
✅ **Error Safety** - Transaction rollback prevents orphaned sessions

---

## Common Error Responses

### 400 Bad Request - Validation Error

```json
{
  "status": "fail",
  "message": "Validation failed",
  "errors": [
    "GR_NUMBER is required",
    "Chemist name must be 50 characters or less"
  ]
}
```

### 409 Conflict - Duplicate GR_NUMBER (MAP_INITIAL only)

```json
{
  "status": "fail",
  "message": "MAP_INITIAL record with GR_NUMBER 'GR-25-0001-1' already exists",
  "existingId": 123456
}
```

### 400 Bad Request - Invalid Stage (MAP_SESSION only)

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

### 404 Not Found - Session Not Found (MAP_RESULT only)

```json
{
  "status": "fail",
  "message": "MAP_SESSION with SESSION_ID 5001 not found",
  "hint": "Create the session first using POST /adamo/session or POST /adamo/session-with-results"
}
```

### 503 Service Unavailable - Oracle Not Configured

```json
{
  "status": "fail",
  "message": "Oracle not configured"
}
```

---

## Integration Mapping

### From MapTool Molecule to MAP_INITIAL

| MapTool Source                     | ADAMO Target                  |
| ---------------------------------- | ----------------------------- |
| `Molecule.GrNumber`                | `MAP_INITIAL.GR_NUMBER`       |
| `Molecule.ChemistName`             | `MAP_INITIAL.CHEMIST`         |
| `Map1_1Evaluation.EvaluationDate`  | `MAP_INITIAL.EVALUATION_DATE` |
| `Map1_1MoleculeEvaluation.Odor0h`  | `MAP_INITIAL.ODOR0H`          |
| `Map1_1MoleculeEvaluation.Odor4h`  | `MAP_INITIAL.ODOR4H`          |
| `Map1_1MoleculeEvaluation.Odor24h` | `MAP_INITIAL.ODOR24H`         |
| `Map1_1MoleculeEvaluation.Comment` | `MAP_INITIAL.COMMENTS`        |

### From MapTool Assessment to MAP_SESSION

| MapTool Source                  | ADAMO Target                  |
| ------------------------------- | ----------------------------- |
| `Assessment.Stage`              | `MAP_SESSION.STAGE`           |
| `Assessment.DateTime`           | `MAP_SESSION.EVALUATION_DATE` |
| `Assessment.Region`             | `MAP_SESSION.REGION`          |
| `Assessment.Segment`            | `MAP_SESSION.SEGMENT`         |
| `Map1_1Evaluation.Participants` | `MAP_SESSION.PARTICIPANTS`    |

### From MapTool MoleculeEvaluation to MAP_RESULT

| MapTool Source                               | ADAMO Target                    |
| -------------------------------------------- | ------------------------------- |
| `Map1_1MoleculeEvaluation.Molecule.GrNumber` | `MAP_RESULT.GR_NUMBER`          |
| `Map1_1MoleculeEvaluation.Odor0h`            | `MAP_RESULT.ODOR`               |
| `Map1_1MoleculeEvaluation.ResultCP/ResultFF` | `MAP_RESULT.RESULT`             |
| `Map1_1MoleculeEvaluation.Benchmark`         | `MAP_RESULT.BENCHMARK_COMMENTS` |
| `DilutionSolvent.Name`                       | `MAP_RESULT.DILUTION`           |
| _(generated)_                                | `MAP_RESULT.SESSION_ID`         |

### MapTool Evaluation Types to Adamo Stage/SubStage

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

---

## What Fields Are Auto-Generated?

### MAP_INITIAL

- ❌ Don't send: `mapInitialId` (auto-generated by sequence)
- ❌ Don't send: `regNumber` (extracted from `grNumber`)
- ❌ Don't send: `batch` (extracted from `grNumber`)
- ✅ Optional: `creationDate`, `lastModifiedDate` (set by API, can be overridden)

### MAP_SESSION

- ❌ Don't send: `sessionId` (auto-generated by sequence)
- ✅ Optional: `creationDate`, `lastModifiedDate` (set by API, can be overridden)

### MAP_RESULT

- ❌ Don't send: `resultId` (auto-generated by sequence)
- ❌ Don't send: `regNumber` (extracted from `grNumber`)
- ❌ Don't send: `batch` (extracted from `grNumber`)
- ✅ Required: `sessionId` (must be valid SESSION_ID from MAP_SESSION)
- ✅ Required: `grNumber`
- ✅ Optional: `creationDate`, `lastModifiedDate` (set by API)

---

## Next Steps

After successfully creating these records:

1. **Verify the inserts** using the GET endpoints
2. **Create MAP_RESULT records** linked to the session
3. **Create ODOR_CHARACTERIZATION records** for detailed profiling
4. **Link sessions** using MAP1_SESSION_LINK for CP/FF relationships
5. **Build automation** to sync data from MapTool to Adamo periodically

---

## Need Help?

- **Detailed Docs:** [../endpoints/MAP_INITIAL_SESSION_ENDPOINTS.md](../endpoints/MAP_INITIAL_SESSION_ENDPOINTS.md)
- **All Endpoints:** [ALL_ENDPOINTS.md](./ALL_ENDPOINTS.md)
- **Database Structure:** [../setup/adamo-DATABASE_STRUCTURE.md](../setup/adamo-DATABASE_STRUCTURE.md)
- **API Examples:** [../guides/API_USAGE_EXAMPLES.md](../guides/API_USAGE_EXAMPLES.md)

---

## Notes

1. **GR_NUMBER Format:** Must follow `GR-YY-NNNNN-B` or `SL-NNNNNN-B` format
2. **Date Format:** Use ISO 8601 format: `YYYY-MM-DDTHH:mm:ssZ`
3. **Character Limits:** Pay attention to max field lengths to avoid validation errors
4. **Stage Case:** Stage values are case-insensitive but will be stored as provided
5. **ShowInTaskList:** Automatically converted to uppercase ('Y' or 'N')

---

**Created:** November 3, 2025  
**Status:** ✅ Ready for Testing
