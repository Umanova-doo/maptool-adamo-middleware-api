# MAP2ADAMOINT - Postman Testing Guide

Complete guide for testing the API using Postman.

---

## 🚀 Quick Setup

### Base URL
```
http://localhost:8085
```

### Required Headers
```
Content-Type: application/json
```

---

## 1️⃣ Health Check Endpoint

### GET /health

**No headers or body needed**

#### Postman Setup:
- **Method:** `GET`
- **URL:** `http://localhost:8085/health`
- **Headers:** None required
- **Body:** None

#### Expected Response:
```json
{
  "status": "OK",
  "service": "MAP2ADAMOINT",
  "timestamp": "2025-10-29T09:10:42.713Z"
}
```

#### Status Code: `200 OK`

---

## 2️⃣ Test MAP Tool → ADAMO Transformation

### POST /test/map-to-adamo

**Transforms MAP Tool data format to ADAMO format**

#### Postman Setup:
- **Method:** `POST`
- **URL:** `http://localhost:8085/test/map-to-adamo`
- **Headers:**
  ```
  Content-Type: application/json
  ```
- **Body (raw JSON):**

#### Option 1: Minimal Request
```json
{}
```
*Uses default sample data with GR number "GR-88-0681-1"*

#### Option 2: Custom GR Number
```json
{
  "grNumber": "GR-99-1234-5"
}
```

#### Expected Response (200 OK):
```json
{
  "status": "success",
  "message": "Successfully validated MAP Tool → ADAMO mapping",
  "source": {
    "type": "Molecule + Map1_1MoleculeEvaluation",
    "grNumber": "GR-99-1234-5",
    "chemist": "Dr. Smith",
    "status": "Map1"
  },
  "destination": {
    "type": "MapInitial",
    "grNumber": "GR-99-1234-5",
    "regNumber": "GR-88-0681",
    "chemist": "Dr. Smith",
    "evaluationDate": "2025-09-29T00:00:00Z",
    "odor0h": "Fruity, fresh, apple-like with green notes",
    "odor4h": "Softer, more floral with persistent fruity character",
    "odor24h": "Woody, dry-down with subtle fruit undertones",
    "dilution": "10% in DPG",
    "comments": "Synced from MAP Tool | Status: Map1 | Project: N/A",
    "createdBy": "TEST_USER"
  },
  "fieldsMapped": 13,
  "fieldsTotal": 16,
  "completeness": "81%"
}
```

#### Console Output (docker logs map2adamoint-api):
```
✓ Successfully mapped Molecule → MapInitial
  GR Number: GR-99-1234-5
  Chemist: Dr. Smith
  Odor 0h: Fruity, fresh, apple-like with green notes
  Odor 4h: Softer, more floral with persistent fruity character
  Odor 24h: Woody, dry-down with subtle fruit undertones
```

---

## 3️⃣ Test ADAMO → MAP Tool Transformation

### POST /test/adamo-to-map

**Transforms ADAMO data format to MAP Tool format**

#### Postman Setup:
- **Method:** `POST`
- **URL:** `http://localhost:8085/test/adamo-to-map`
- **Headers:**
  ```
  Content-Type: application/json
  ```
- **Body (raw JSON):**

#### Option 1: Minimal Request
```json
{}
```
*Uses default sample data with session ID 4111*

#### Option 2: Custom Session & GR Number
```json
{
  "sessionId": 9999,
  "grNumber": "GR-99-8888-2"
}
```

#### Expected Response (200 OK):
```json
{
  "status": "success",
  "message": "Successfully validated ADAMO → MAP Tool mapping",
  "source": {
    "type": "MapSession + MapResult",
    "sessionId": 9999,
    "stage": "MAP 3",
    "region": "US",
    "segment": "CP",
    "grNumber": "GR-99-8888-2",
    "odor": "Rosy, floral, peonile, geranium, interesting in DD but not powerful",
    "resultScore": 1
  },
  "destination": {
    "type": "Assessment",
    "sessionName": "ADAMO-9999",
    "dateTime": "2025-10-14T09:10:42.713Z",
    "stage": "MAP 3",
    "region": "US",
    "segment": "CP",
    "status": 1,
    "isClosed": false,
    "createdBy": "ADMIN"
  },
  "fieldsMapped": 10,
  "fieldsTotal": 13,
  "completeness": "77%"
}
```

#### Console Output (docker logs map2adamoint-api):
```
✓ Successfully mapped MapSession + MapResult → Assessment
  Session: ADAMO-9999
  Stage: MAP 3
  Region: US
  Segment: CP
```

---

## 📝 Postman Collection JSON

Copy and paste this into Postman (Import → Raw Text):

```json
{
  "info": {
    "name": "MAP2ADAMOINT API",
    "description": "Data transformation middleware between ADAMO and MAP Tool",
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
      "name": "Transform MAP Tool to ADAMO (Default)",
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
          "raw": "{}"
        },
        "url": {
          "raw": "http://localhost:8085/test/map-to-adamo",
          "protocol": "http",
          "host": ["localhost"],
          "port": "8085",
          "path": ["test", "map-to-adamo"]
        }
      }
    },
    {
      "name": "Transform MAP Tool to ADAMO (Custom GR)",
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
          "raw": "{\n  \"grNumber\": \"GR-99-1234-5\"\n}"
        },
        "url": {
          "raw": "http://localhost:8085/test/map-to-adamo",
          "protocol": "http",
          "host": ["localhost"],
          "port": "8085",
          "path": ["test", "map-to-adamo"]
        }
      }
    },
    {
      "name": "Transform ADAMO to MAP Tool (Default)",
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
          "raw": "{}"
        },
        "url": {
          "raw": "http://localhost:8085/test/adamo-to-map",
          "protocol": "http",
          "host": ["localhost"],
          "port": "8085",
          "path": ["test", "adamo-to-map"]
        }
      }
    },
    {
      "name": "Transform ADAMO to MAP Tool (Custom)",
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
          "raw": "{\n  \"sessionId\": 9999,\n  \"grNumber\": \"GR-99-8888-2\"\n}"
        },
        "url": {
          "raw": "http://localhost:8085/test/adamo-to-map",
          "protocol": "http",
          "host": ["localhost"],
          "port": "8085",
          "path": ["test", "adamo-to-map"]
        }
      }
    }
  ]
}
```

---

## 📸 Postman Screenshots Guide

### Setup for POST /test/map-to-adamo

1. **Method:** Select `POST` from dropdown
2. **URL:** Enter `http://localhost:8085/test/map-to-adamo`
3. **Headers Tab:**
   - Click "Headers"
   - Add: `Content-Type` = `application/json`
4. **Body Tab:**
   - Click "Body"
   - Select "raw"
   - Select "JSON" from dropdown (right side)
   - Paste:
   ```json
   {
     "grNumber": "GR-99-1234-5"
   }
   ```
5. **Click "Send"**
6. **View response** in the bottom panel

### Setup for POST /test/adamo-to-map

1. **Method:** Select `POST` from dropdown
2. **URL:** Enter `http://localhost:8085/test/adamo-to-map`
3. **Headers Tab:**
   - Click "Headers"
   - Add: `Content-Type` = `application/json`
4. **Body Tab:**
   - Click "Body"
   - Select "raw"
   - Select "JSON" from dropdown
   - Paste:
   ```json
   {
     "sessionId": 9999,
     "grNumber": "GR-99-8888-2"
   }
   ```
5. **Click "Send"**
6. **View response** in the bottom panel

---

## 🧪 Test Scenarios

### Scenario 1: Basic Transformation Test
```
GET  /health
     → Verify API is running

POST /test/map-to-adamo
     Body: {}
     → Should return success with sample MAP data transformed to ADAMO format

POST /test/adamo-to-map
     Body: {}
     → Should return success with sample ADAMO data transformed to MAP format
```

### Scenario 2: Custom Data Test
```
POST /test/map-to-adamo
     Body: {"grNumber": "GR-25-9999-1"}
     → Should transform with your custom GR number

POST /test/adamo-to-map
     Body: {"sessionId": 12345, "grNumber": "GR-25-9999-1"}
     → Should transform with your custom session and GR number
```

### Scenario 3: Error Handling Test
```
POST /test/map-to-adamo
     Body: {"grNumber": null}
     → Should still succeed (uses default or handles null)

POST /test/map-to-adamo
     Body: invalid json
     → Should return 400 Bad Request
```

---

## 📊 Response Field Meanings

### /test/map-to-adamo Response

| Field | Description |
|-------|-------------|
| `status` | "success" or "fail" |
| `message` | Human-readable result |
| `source.type` | Input data type (MAP Tool models) |
| `source.grNumber` | Input GR number |
| `destination.type` | Output data type (ADAMO model) |
| `destination.grNumber` | Transformed GR number |
| `destination.odor0h/4h/24h` | Odor descriptions at 0h, 4h, 24h |
| `fieldsMapped` | Number of fields successfully mapped |
| `fieldsTotal` | Total fields in destination model |
| `completeness` | Percentage of fields mapped |

### /test/adamo-to-map Response

| Field | Description |
|-------|-------------|
| `status` | "success" or "fail" |
| `message` | Human-readable result |
| `source.sessionId` | Input session ID from ADAMO |
| `source.stage` | MAP stage (MAP 0/1/2/3, ISC, etc.) |
| `destination.sessionName` | Transformed session name |
| `destination.stage` | Copied stage value |
| `fieldsMapped` | Number of fields successfully mapped |
| `completeness` | Percentage of fields mapped |

---

## 🔍 Viewing Console Logs

To see the detailed console output:

```bash
# View live logs
docker logs -f map2adamoint-api

# View last 50 lines
docker logs map2adamoint-api --tail 50
```

Expected console output when you call endpoints:
```
✓ Successfully mapped Molecule → MapInitial
  GR Number: GR-99-1234-5
  Chemist: Dr. Smith
  Odor 0h: Fruity, fresh, apple-like with green notes
  Odor 4h: Softer, more floral with persistent fruity character
  Odor 24h: Woody, dry-down with subtle fruit undertones
```

---

## 📦 Postman Collection Import Steps

### Method 1: Import JSON

1. Open Postman
2. Click **Import** button (top left)
3. Select **Raw text** tab
4. Copy the entire JSON collection from above
5. Click **Continue** → **Import**
6. Collection appears in left sidebar

### Method 2: Manual Setup

1. Create new Collection: **"MAP2ADAMOINT API"**
2. Add Request 1:
   - Name: "Health Check"
   - Method: GET
   - URL: `http://localhost:8085/health`
   
3. Add Request 2:
   - Name: "MAP to ADAMO"
   - Method: POST
   - URL: `http://localhost:8085/test/map-to-adamo`
   - Headers: `Content-Type: application/json`
   - Body (raw, JSON):
   ```json
   {
     "grNumber": "GR-99-1234-5"
   }
   ```

4. Add Request 3:
   - Name: "ADAMO to MAP"
   - Method: POST
   - URL: `http://localhost:8085/test/adamo-to-map`
   - Headers: `Content-Type: application/json`
   - Body (raw, JSON):
   ```json
   {
     "sessionId": 9999,
     "grNumber": "GR-99-8888-2"
   }
   ```

---

## 🎯 Complete Test Examples

### Test 1: Health Check

**Request:**
```http
GET http://localhost:8085/health HTTP/1.1
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "status": "OK",
  "service": "MAP2ADAMOINT",
  "timestamp": "2025-10-29T09:10:42.713Z"
}
```

---

### Test 2: MAP Tool → ADAMO (Minimal)

**Request:**
```http
POST http://localhost:8085/test/map-to-adamo HTTP/1.1
Content-Type: application/json

{}
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "status": "success",
  "message": "Successfully validated MAP Tool → ADAMO mapping",
  "source": {
    "type": "Molecule + Map1_1MoleculeEvaluation",
    "grNumber": "GR-88-0681-1",
    "chemist": "Dr. Smith",
    "status": "Map1"
  },
  "destination": {
    "type": "MapInitial",
    "grNumber": "GR-88-0681-1",
    "regNumber": "GR-88-0681",
    "chemist": "Dr. Smith",
    "evaluationDate": "2025-09-29T00:00:00Z",
    "odor0h": "Fruity, fresh, apple-like with green notes",
    "odor4h": "Softer, more floral with persistent fruity character",
    "odor24h": "Woody, dry-down with subtle fruit undertones",
    "dilution": "10% in DPG",
    "comments": "Synced from MAP Tool | Status: Map1 | Project: N/A",
    "createdBy": "TEST_USER"
  },
  "fieldsMapped": 13,
  "fieldsTotal": 16,
  "completeness": "81%"
}
```

---

### Test 3: MAP Tool → ADAMO (Custom GR Number)

**Request:**
```http
POST http://localhost:8085/test/map-to-adamo HTTP/1.1
Content-Type: application/json

{
  "grNumber": "GR-25-7890-3"
}
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "status": "success",
  "message": "Successfully validated MAP Tool → ADAMO mapping",
  "source": {
    "type": "Molecule + Map1_1MoleculeEvaluation",
    "grNumber": "GR-25-7890-3",
    "chemist": "Dr. Smith",
    "status": "Map1"
  },
  "destination": {
    "type": "MapInitial",
    "grNumber": "GR-25-7890-3",
    "regNumber": "GR-88-0681",
    "chemist": "Dr. Smith",
    "evaluationDate": "2025-09-29T00:00:00Z",
    "odor0h": "Fruity, fresh, apple-like with green notes",
    "odor4h": "Softer, more floral with persistent fruity character",
    "odor24h": "Woody, dry-down with subtle fruit undertones",
    "dilution": "10% in DPG",
    "comments": "Synced from MAP Tool | Status: Map1 | Project: N/A",
    "createdBy": "TEST_USER"
  },
  "fieldsMapped": 13,
  "fieldsTotal": 16,
  "completeness": "81%"
}
```

---

### Test 4: ADAMO → MAP Tool (Minimal)

**Request:**
```http
POST http://localhost:8085/test/adamo-to-map HTTP/1.1
Content-Type: application/json

{}
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "status": "success",
  "message": "Successfully validated ADAMO → MAP Tool mapping",
  "source": {
    "type": "MapSession + MapResult",
    "sessionId": 4111,
    "stage": "MAP 3",
    "region": "US",
    "segment": "CP",
    "grNumber": "GR-86-6561-0",
    "odor": "Rosy, floral, peonile, geranium, interesting in DD but not powerful",
    "resultScore": 1
  },
  "destination": {
    "type": "Assessment",
    "sessionName": "ADAMO-4111",
    "dateTime": "2025-10-14T09:10:42.713Z",
    "stage": "MAP 3",
    "region": "US",
    "segment": "CP",
    "status": 1,
    "isClosed": false,
    "createdBy": "ADMIN"
  },
  "fieldsMapped": 10,
  "fieldsTotal": 13,
  "completeness": "77%"
}
```

---

### Test 5: ADAMO → MAP Tool (Custom Values)

**Request:**
```http
POST http://localhost:8085/test/adamo-to-map HTTP/1.1
Content-Type: application/json

{
  "sessionId": 12345,
  "grNumber": "GR-25-7890-3"
}
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "status": "success",
  "message": "Successfully validated ADAMO → MAP Tool mapping",
  "source": {
    "type": "MapSession + MapResult",
    "sessionId": 12345,
    "stage": "MAP 3",
    "region": "US",
    "segment": "CP",
    "grNumber": "GR-25-7890-3",
    "odor": "Rosy, floral, peonile, geranium, interesting in DD but not powerful",
    "resultScore": 1
  },
  "destination": {
    "type": "Assessment",
    "sessionName": "ADAMO-12345",
    "dateTime": "2025-10-14T09:10:42.713Z",
    "stage": "MAP 3",
    "region": "US",
    "segment": "CP",
    "status": 1,
    "isClosed": false,
    "createdBy": "ADMIN"
  },
  "fieldsMapped": 10,
  "fieldsTotal": 13,
  "completeness": "77%"
}
```

---

## ⚠️ Error Responses

### 400 Bad Request (Invalid JSON)

**Request:**
```http
POST http://localhost:8085/test/map-to-adamo HTTP/1.1
Content-Type: application/json

{invalid json}
```

**Response:**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "$": ["'i' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 1."]
  }
}
```

### 500 Internal Server Error

**Request:**
```http
POST http://localhost:8085/test/map-to-adamo HTTP/1.1
Content-Type: application/json

{
  "grNumber": "INVALID"
}
```

**Response (if mapping fails):**
```http
HTTP/1.1 500 Internal Server Error
Content-Type: application/json

{
  "status": "fail",
  "message": "Mapping error details...",
  "stackTrace": "..."
}
```

---

## 🎨 Postman Environment Variables (Optional)

Create an environment for easy switching between dev/prod:

**Environment Name:** MAP2ADAMOINT Local

| Variable | Initial Value | Current Value |
|----------|---------------|---------------|
| `base_url` | `http://localhost:8085` | `http://localhost:8085` |
| `gr_number` | `GR-99-1234-5` | `GR-99-1234-5` |
| `session_id` | `9999` | `9999` |

Then in your requests, use:
- URL: `{{base_url}}/test/map-to-adamo`
- Body: `{"grNumber": "{{gr_number}}"}`

---

## ✅ Success Criteria Checklist

After testing in Postman, verify:

- ✅ Health endpoint returns 200 OK
- ✅ MAP → ADAMO returns transformed data with all fields
- ✅ ADAMO → MAP returns transformed data with all fields
- ✅ Console shows "✓ Successfully mapped..." messages
- ✅ Response includes completeness percentage
- ✅ Custom GR numbers are used when provided
- ✅ Custom session IDs are used when provided
- ✅ Response time < 100ms (should be very fast)

---

## 🔧 Troubleshooting

### Issue: Cannot connect to localhost:8085

**Solution:**
```bash
# Check if container is running
docker ps | findstr map2adamoint

# If not running, start it
docker-compose up -d

# Check logs
docker logs map2adamoint-api
```

### Issue: 404 Not Found

**Solution:**
- Verify URL spelling: `/test/map-to-adamo` (not `/test/mapToAdamo`)
- Ensure you're using POST method (not GET)

### Issue: 400 Bad Request

**Solution:**
- Verify `Content-Type: application/json` header is set
- Ensure JSON body is valid (use Postman's "Beautify" button)
- Try empty body `{}` first

---

## 📱 Alternative: Using Swagger UI

Instead of Postman, you can use the built-in Swagger UI:

**URL:** http://localhost:8085/swagger

1. Open in browser
2. Expand `/test/map-to-adamo` section
3. Click **"Try it out"**
4. Edit the request body
5. Click **"Execute"**
6. View response below

---

## 🚀 Quick Copy-Paste Commands

### cURL Commands

```bash
# Health check
curl http://localhost:8085/health

# MAP to ADAMO (default)
curl -X POST http://localhost:8085/test/map-to-adamo \
  -H "Content-Type: application/json" \
  -d '{}'

# MAP to ADAMO (custom GR)
curl -X POST http://localhost:8085/test/map-to-adamo \
  -H "Content-Type: application/json" \
  -d '{"grNumber":"GR-99-1234-5"}'

# ADAMO to MAP (default)
curl -X POST http://localhost:8085/test/adamo-to-map \
  -H "Content-Type: application/json" \
  -d '{}'

# ADAMO to MAP (custom)
curl -X POST http://localhost:8085/test/adamo-to-map \
  -H "Content-Type: application/json" \
  -d '{"sessionId":9999,"grNumber":"GR-99-8888-2"}'
```

### PowerShell Commands

```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:8085/health" -Method Get

# MAP to ADAMO
$body = '{"grNumber":"GR-99-1234-5"}' 
Invoke-RestMethod -Uri "http://localhost:8085/test/map-to-adamo" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body

# ADAMO to MAP
$body = '{"sessionId":9999,"grNumber":"GR-99-8888-2"}' 
Invoke-RestMethod -Uri "http://localhost:8085/test/adamo-to-map" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

---

## 📋 Summary

### Endpoint Summary

| Endpoint | Method | Headers | Body Required | Purpose |
|----------|--------|---------|---------------|---------|
| `/health` | GET | None | No | Check if API is running |
| `/test/map-to-adamo` | POST | `Content-Type: application/json` | Optional | Transform MAP → ADAMO |
| `/test/adamo-to-map` | POST | `Content-Type: application/json` | Optional | Transform ADAMO → MAP |

### Body Parameters

**POST /test/map-to-adamo:**
- `grNumber` (string, optional) - Custom GR number to use

**POST /test/adamo-to-map:**
- `sessionId` (number, optional) - Custom session ID to use
- `grNumber` (string, optional) - Custom GR number to use

---

**Ready to test! 🚀**

Open Postman and start with the Health Check endpoint, then try both transformation endpoints.

