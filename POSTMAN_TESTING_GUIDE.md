# MAP2ADAMOINT - Postman Testing Guide

Complete guide for testing the transformation middleware API.

**CRITICAL:** This API does NOT create any data. It ONLY transforms data you send to it.

---

## 🎯 API Endpoints

| Endpoint | Input | Output | Purpose |
|----------|-------|--------|---------|
| `GET /health` | None | Status | Health check |
| `POST /transform/map-to-adamo` | MAP Tool data (JSON) | ADAMO format (JSON) | Transform MAP → ADAMO |
| `POST /transform/adamo-to-map` | ADAMO data (JSON) | MAP Tool format (JSON) | Transform ADAMO → MAP |

---

## 1️⃣ Health Check

### GET /health

**Method:** GET  
**URL:** `http://localhost:8085/health`  
**Headers:** None  
**Body:** None

**Response:**
```json
{
  "status": "OK",
  "service": "MAP2ADAMOINT",
  "timestamp": "2025-10-29T09:21:22.665Z"
}
```

---

## 2️⃣ Transform MAP Tool → ADAMO

### POST /transform/map-to-adamo

**Transforms MAP Tool Molecule data to ADAMO MapInitial format**

### Postman Setup

**Method:** POST  
**URL:** `http://localhost:8085/transform/map-to-adamo`  
**Headers:**
```
Content-Type: application/json
```

### Request Body

#### Minimum Required:
```json
{
  "molecule": {
    "grNumber": "GR-88-0681-1",
    "regNumber": "GR-88-0681",
    "chemistName": "Dr. Kraft",
    "status": 1,
    "assessed": true,
    "quantity": 100
  }
}
```

#### Full Example (with Evaluation):
```json
{
  "molecule": {
    "id": 123,
    "grNumber": "GR-88-0681-1",
    "regNumber": "GR-88-0681",
    "structure": "MOLECULAR_STRUCTURE_DATA",
    "assessed": true,
    "chemistName": "Dr. Jane Kraft",
    "chemicalName": "3-Phenylpropanol",
    "molecularFormula": "C9H12O",
    "projectName": "Project Alpha",
    "status": 1,
    "quantity": 250.75,
    "createdAt": "2025-09-15T10:30:00Z",
    "updatedAt": "2025-10-20T14:15:00Z",
    "createdBy": "jkraft",
    "updatedBy": "jsmith",
    "isArchived": false,
    "isManuallyArchived": false
  },
  "evaluation": {
    "id": 456,
    "map1_1EvaluationId": 789,
    "moleculeId": 123,
    "sortOrder": 1,
    "grDilutionSolventId": 5,
    "benchmarkDilutionSolventId": 5,
    "odor0h": "Resinous cypress, natural, pine needles, slightly fruity",
    "odor4h": "Linear, more woody character",
    "odor24h": "Woody cedarwood, dry-down",
    "benchmark": "Similar to Cedryl Acetate but more natural",
    "comment": "Promising for masculine fragrances",
    "ffNextSteps": "Test in woody fougere base at 5%",
    "cpNextSteps": "Evaluate in fabric softener at 0.5%",
    "resultCP": 4,
    "resultFF": 3,
    "createdAt": "2025-09-15T12:00:00Z",
    "updatedAt": "2025-09-15T16:00:00Z"
  }
}
```

### Response (200 OK)

```json
{
  "status": "success",
  "message": "Successfully transformed MAP Tool data to ADAMO format for GR-88-0681-1",
  "transformed": {
    "mapInitialId": 0,
    "grNumber": "GR-88-0681-1",
    "evaluationDate": null,
    "chemist": "Dr. Jane Kraft",
    "assessor": null,
    "dilution": "10% in DPG",
    "evaluationSite": null,
    "odor0H": "Resinous cypress, natural, pine needles, slightly fruity",
    "odor4H": "Linear, more woody character",
    "odor24H": "Woody cedarwood, dry-down",
    "creationDate": "2025-10-29T09:21:49.992Z",
    "createdBy": "jkraft",
    "lastModifiedDate": "2025-10-29T09:21:49.996Z",
    "lastModifiedBy": "jsmith",
    "comments": "Synced from MAP Tool | Status: Map1 | Project: Project Alpha",
    "regNumber": "GR-88-0681",
    "batch": null
  }
}
```

### Console Output:
```
✓ Transformed Molecule → MapInitial: GR-88-0681-1
  Chemist: Dr. Jane Kraft
  Odor 0h: Resinous cypress, natural, pine needles, slightly fruity
```

---

## 3️⃣ Transform ADAMO → MAP Tool

### POST /transform/adamo-to-map

**Transforms ADAMO MapSession + MapResult to MAP Tool Assessment format**

### Postman Setup

**Method:** POST  
**URL:** `http://localhost:8085/transform/adamo-to-map`  
**Headers:**
```
Content-Type: application/json
```

### Request Body

#### Minimum Required:
```json
{
  "session": {
    "sessionId": 4111,
    "stage": "MAP 3",
    "evaluationDate": "2004-06-18",
    "region": "US",
    "segment": "CP"
  },
  "result": {
    "resultId": 207,
    "sessionId": 4111,
    "grNumber": "GR-86-6561-0",
    "odor": "Rosy, floral, peonile, geranium"
  }
}
```

#### Full Example:
```json
{
  "session": {
    "sessionId": 4111,
    "stage": "MAP 3",
    "evaluationDate": "2004-06-18T00:00:00Z",
    "region": "US",
    "segment": "CP",
    "participants": "Panel A: John Smith, Mary Johnson, Bob Williams",
    "showInTaskList": "N",
    "creationDate": "2004-06-15T10:00:00Z",
    "createdBy": "ADMIN",
    "lastModifiedDate": "2004-06-18T14:30:00Z",
    "lastModifiedBy": "EVAL123",
    "subStage": 1,
    "category": "A"
  },
  "result": {
    "resultId": 207,
    "sessionId": 4111,
    "grNumber": "GR-86-6561-0",
    "odor": "Rosy, floral, peonile, geranium, interesting in DD but not powerful",
    "benchmarkComments": "CP: 02/09/2005, Status 1, FF: 04/15/2005, Status 1",
    "result": 1,
    "dilution": "10% in DPG",
    "sponsor": "Perfumery Lab Team - Region US",
    "creationDate": "2004-06-18T10:00:00Z",
    "createdBy": "EVAL123",
    "lastModifiedDate": "2004-06-18T14:00:00Z",
    "lastModifiedBy": "EVAL123",
    "regNumber": "GR-86-6561",
    "batch": 0
  }
}
```

### Response (200 OK)

```json
{
  "status": "success",
  "message": "Successfully transformed ADAMO data to MAP Tool format for session 4111",
  "transformed": {
    "id": 0,
    "sessionName": "ADAMO-4111",
    "dateTime": "2004-06-18T00:00:00",
    "stage": "MAP 3",
    "status": 1,
    "region": "US",
    "segment": "CP",
    "isClosed": false,
    "createdAt": "2025-10-29T09:22:14.016Z",
    "updatedAt": "2025-10-29T09:22:14.016Z",
    "createdBy": "ADMIN",
    "updatedBy": "EVAL123",
    "isArchived": false,
    "isManuallyArchived": false,
    "map1_1Evaluations": null
  }
}
```

### Console Output:
```
✓ Transformed MapSession → Assessment: ADAMO-4111
  Stage: MAP 3
  Region: US
  Segment: CP
```

---

## 📋 Field Requirements

### MAP Tool Molecule Fields

| Field | Type | Required | Max Length | Notes |
|-------|------|----------|------------|-------|
| `grNumber` | string | ✅ Yes | 14 | GR-YY-NNNNN-B format |
| `regNumber` | string | No | 11 | Base GR number |
| `chemistName` | string | No | 50 | Chemist name |
| `status` | int | No | - | 0=None, 1=Map1, 2=Weak, 3=Odorless, 4=Ignore |
| `assessed` | bool | No | - | - |
| `quantity` | decimal | No | - | - |
| `createdBy` | string | No | 50 | - |
| `updatedBy` | string | No | 50 | - |

### ADAMO Session Fields

| Field | Type | Required | Max Length | Notes |
|-------|------|----------|------------|-------|
| `sessionId` | long | ✅ Yes | - | - |
| `stage` | string | No | 20 | MAP 0/1/2/3, ISC, FIB, FIM, CARDEX, RPMC |
| `evaluationDate` | datetime | No | - | ISO 8601 format |
| `region` | string | No | 2 | e.g., "US", "EU" |
| `segment` | string | No | 2 | "CP" or "FF" |
| `participants` | string | No | 1000 | - |
| `createdBy` | string | No | **8** | ⚠️ Max 8 chars! |
| `lastModifiedBy` | string | No | **8** | ⚠️ Max 8 chars! |

### ADAMO Result Fields

| Field | Type | Required | Max Length | Notes |
|-------|------|----------|------------|-------|
| `resultId` | long | No | - | - |
| `sessionId` | long | ✅ Yes | - | Must match session |
| `grNumber` | string | ✅ Yes | 14 | - |
| `odor` | string | No | 1000 | - |
| `benchmarkComments` | string | No | 2000 | - |
| `result` | int | No | - | Score (typically 1-5) |
| `dilution` | string | No | 20 | - |
| `sponsor` | string | No | 255 | - |
| `createdBy` | string | No | **8** | ⚠️ Max 8 chars! |
| `lastModifiedBy` | string | No | **8** | ⚠️ Max 8 chars! |

---

## 🧪 Quick Test Commands

### Using cURL (Windows PowerShell)

**Health Check:**
```bash
curl http://localhost:8085/health
```

**MAP Tool → ADAMO:**
```bash
curl -X POST http://localhost:8085/transform/map-to-adamo `
  -H "Content-Type: application/json" `
  -d '@test-map-to-adamo.json'
```

**ADAMO → MAP Tool:**
```bash
curl -X POST http://localhost:8085/transform/adamo-to-map `
  -H "Content-Type: application/json" `
  -d '@test-adamo-to-map.json'
```

### Using PowerShell Invoke-RestMethod

**MAP Tool → ADAMO:**
```powershell
$body = Get-Content test-map-to-adamo.json -Raw
Invoke-RestMethod -Uri "http://localhost:8085/transform/map-to-adamo" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body | ConvertTo-Json -Depth 10
```

**ADAMO → MAP Tool:**
```powershell
$body = Get-Content test-adamo-to-map.json -Raw
Invoke-RestMethod -Uri "http://localhost:8085/transform/adamo-to-map" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body | ConvertTo-Json -Depth 10
```

---

## 📦 Postman Collection JSON

Import this into Postman:

```json
{
  "info": {
    "name": "MAP2ADAMOINT Middleware API",
    "description": "Data transformation between ADAMO and MAP Tool",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Health Check",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "http://localhost:8085/health",
          "protocol": "http",
          "host": ["localhost"],
          "port": "8085",
          "path": ["health"]
        }
      }
    },
    {
      "name": "Transform MAP Tool to ADAMO",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"molecule\": {\n    \"grNumber\": \"GR-88-0681-1\",\n    \"regNumber\": \"GR-88-0681\",\n    \"chemistName\": \"Dr. Kraft\",\n    \"status\": 1,\n    \"assessed\": true,\n    \"quantity\": 100\n  },\n  \"evaluation\": {\n    \"odor0h\": \"Resinous cypress, natural\",\n    \"odor4h\": \"Linear\",\n    \"odor24h\": \"Woody cedarwood\",\n    \"resultCP\": 4,\n    \"resultFF\": 3\n  }\n}"
        },
        "url": {
          "raw": "http://localhost:8085/transform/map-to-adamo",
          "protocol": "http",
          "host": ["localhost"],
          "port": "8085",
          "path": ["transform", "map-to-adamo"]
        }
      }
    },
    {
      "name": "Transform ADAMO to MAP Tool",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"session\": {\n    \"sessionId\": 4111,\n    \"stage\": \"MAP 3\",\n    \"evaluationDate\": \"2004-06-18\",\n    \"region\": \"US\",\n    \"segment\": \"CP\",\n    \"participants\": \"Panel A\",\n    \"createdBy\": \"ADMIN\"\n  },\n  \"result\": {\n    \"resultId\": 207,\n    \"sessionId\": 4111,\n    \"grNumber\": \"GR-86-6561-0\",\n    \"odor\": \"Rosy, floral\",\n    \"result\": 1,\n    \"createdBy\": \"EVAL\"\n  }\n}"
        },
        "url": {
          "raw": "http://localhost:8085/transform/adamo-to-map",
          "protocol": "http",
          "host": ["localhost"],
          "port": "8085",
          "path": ["transform", "adamo-to-map"]
        }
      }
    }
  ]
}
```

---

## ⚠️ Common Errors

### Error 1: Missing Required Field

**Request:**
```json
{
  "molecule": {}
}
```

**Response (400):**
```json
{
  "status": "fail",
  "message": "Molecule data is required"
}
```

### Error 2: Field Too Long

**Request:**
```json
{
  "session": {...},
  "result": {
    "createdBy": "EVALUATOR123"  // Too long! Max 8 chars
  }
}
```

**Response (400):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Result.CreatedBy": ["The field CreatedBy must be a string with a maximum length of 8."]
  }
}
```

**Fix:** Use max 8 characters: `"createdBy": "EVAL123"`

### Error 3: Invalid JSON

**Request:**
```json
{
  "molecule": {
    "grNumber": "GR-88-0681-1"  // Missing closing brace
  }
```

**Response (400):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "$": ["'}' is invalid after a value. Expected either ',', '}', or ']'. Path: $ | LineNumber: 3 | BytePositionInLine: 2."]
  }
}
```

---

## ✅ Test Files Included

The project includes sample test files:

- **`test-map-to-adamo.json`** - Sample MAP Tool data
- **`test-adamo-to-map.json`** - Sample ADAMO data

Use these for quick testing:

```bash
# Test MAP → ADAMO
curl -X POST http://localhost:8085/transform/map-to-adamo \
  -H "Content-Type: application/json" \
  -d '@test-map-to-adamo.json'

# Test ADAMO → MAP
curl -X POST http://localhost:8085/transform/adamo-to-map \
  -H "Content-Type: application/json" \
  -d '@test-adamo-to-map.json'
```

---

## 📊 What Gets Transformed

### MAP Tool → ADAMO Mapping

| From (MAP Tool) | To (ADAMO) |
|-----------------|------------|
| `molecule.grNumber` | `grNumber` |
| `molecule.regNumber` | `regNumber` |
| `molecule.chemistName` | `chemist` |
| `evaluation.odor0h` | `odor0H` |
| `evaluation.odor4h` | `odor4H` |
| `evaluation.odor24h` | `odor24H` |
| `molecule.createdBy` | `createdBy` |
| `molecule.updatedBy` | `lastModifiedBy` |
| (Constructed) | `comments` |

### ADAMO → MAP Tool Mapping

| From (ADAMO) | To (MAP Tool) |
|--------------|---------------|
| `session.sessionId` | `sessionName` (formatted) |
| `session.evaluationDate` | `dateTime` |
| `session.stage` | `stage` |
| `session.region` | `region` |
| `session.segment` | `segment` |
| `session.createdBy` | `createdBy` |
| `session.lastModifiedBy` | `updatedBy` |

---

## 🎓 How to Use in Production

### Step 1: ADAMO Tool Sends Data

```http
POST http://your-middleware:8085/transform/adamo-to-map
Content-Type: application/json

{
  "session": { /* session data from Oracle */ },
  "result": { /* result data from Oracle */ }
}
```

### Step 2: Middleware Transforms

The API receives ADAMO format → transforms to MAP format → returns

### Step 3: MAP Tool Receives & Stores

MAP Tool receives the response and stores it in PostgreSQL

---

## 🔍 Viewing Logs

```bash
# Watch live logs
docker logs -f map2adamoint-api

# Last 50 lines
docker logs map2adamoint-api --tail 50
```

---

## 🚀 Quick Start

1. Start API:
   ```bash
   docker-compose up -d
   ```

2. Verify health:
   ```bash
   curl http://localhost:8085/health
   ```

3. Test transformations:
   ```bash
   curl -X POST http://localhost:8085/transform/map-to-adamo \
     -H "Content-Type: application/json" \
     -d '@test-map-to-adamo.json'
   ```

4. View logs:
   ```bash
   docker logs map2adamoint-api
   ```

---

## ⚡ No Dummy Data!

**Important:** The API does NOT create or prefill ANY data. It ONLY:
- ✅ Accepts your data
- ✅ Transforms the format
- ✅ Returns transformed data
- ✅ Logs success/fail

---

**Ready for Postman testing! 🚀**
