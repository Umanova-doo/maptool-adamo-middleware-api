# Quick Start: New MAP_INITIAL & MAP_SESSION Endpoints

## 🎯 What's New

**Four new POST endpoints** for creating records in Oracle Adamo database:

1. **`POST /adamo/initial`** - Create MAP_INITIAL records (molecule evaluations)
2. **`POST /adamo/session`** - Create MAP_SESSION records (evaluation sessions)
3. **`POST /adamo/result`** - Create MAP_RESULT records (individual results)
4. **`POST /adamo/session-with-results`** ⭐ - Create session + results in ONE transaction (RECOMMENDED)

## 🚀 Test in 3 Steps

### Step 1: Start the API

```bash
dotnet run
```

### Step 2: Import Postman Collection

1. Open Postman
2. Click **Import**
3. Select `MAP2ADAMOINT-Creation-Endpoints.postman_collection.json`
4. Set `baseUrl` variable to `http://localhost:5000`

### Step 3: Run the Requests

1. Open the **ADAMO Creation** folder
2. Click **"Create MAP_INITIAL"** → Send
3. Click **"Create SESSION with RESULTS (Combined)"** ⭐ → Send (RECOMMENDED)
   - Or use separate endpoints:
     - Click **"Create MAP_SESSION"** → Send → Note the `sessionId`
     - Update `sessionId` in **"Create MAP_RESULT"** → Send

✅ Done! Check the responses for your new record IDs.

## 📋 Or Use cURL

**Create MAP_INITIAL:**

```bash
curl -X POST http://localhost:5000/adamo/initial \
  -H "Content-Type: application/json" \
  -d @test-create-map-initial.json
```

**Create SESSION with RESULTS (⭐ RECOMMENDED):**

```bash
curl -X POST http://localhost:5000/adamo/session-with-results \
  -H "Content-Type: application/json" \
  -d @test-create-session-with-results.json
```

**OR create separately:**

```bash
# Create MAP_SESSION
curl -X POST http://localhost:5000/adamo/session \
  -H "Content-Type: application/json" \
  -d @test-create-map-session.json

# Then create MAP_RESULT (update sessionId in JSON first)
curl -X POST http://localhost:5000/adamo/result \
  -H "Content-Type: application/json" \
  -d @test-create-map-result.json
```

## 📖 Example Request Bodies

### SESSION with RESULTS (⭐ RECOMMENDED)

```json
{
  "session": {
    "stage": "MAP 1",
    "segment": "CP",
    "region": "US"
  },
  "results": [
    {
      "grNumber": "GR-25-0010-1",
      "odor": "Fresh citrus",
      "result": 5
    },
    {
      "grNumber": "GR-25-0011-1",
      "odor": "Woody base",
      "result": 4
    }
  ]
}
```

### MAP_INITIAL (Minimal)

```json
{
  "grNumber": "GR-25-0001-1"
}
```

### MAP_SESSION (Minimal)

```json
{
  "stage": "MAP 1",
  "segment": "CP"
}
```

### MAP_RESULT (Minimal)

```json
{
  "sessionId": 5001,
  "grNumber": "GR-25-0003-1"
}
```

## 📚 Documentation

| Document                                                                      | What's Inside                          |
| ----------------------------------------------------------------------------- | -------------------------------------- |
| **[IMPLEMENTATION_COMPLETE.md](docs/IMPLEMENTATION_COMPLETE.md)**             | Full summary, checklist, Q&A           |
| **[MAP_INITIAL_SESSION_ENDPOINTS.md](docs/MAP_INITIAL_SESSION_ENDPOINTS.md)** | MAP_INITIAL and MAP_SESSION guide      |
| **[MAP_RESULT_ENDPOINTS.md](docs/MAP_RESULT_ENDPOINTS.md)**                   | MAP_RESULT and combined endpoint guide |
| **[NEW_ENDPOINTS_SUMMARY.md](docs/NEW_ENDPOINTS_SUMMARY.md)**                 | Quick reference for all 4 endpoints    |

## ✅ What You Get

- ✅ Full validation with helpful errors
- ✅ Duplicate detection (MAP_INITIAL)
- ✅ Foreign key validation (MAP_RESULT)
- ✅ Auto-generated IDs, REG_NUMBER, BATCH
- ✅ Stage validation (MAP_SESSION)
- ✅ **Atomic transactions** (session-with-results)
- ✅ Complete audit trail

## 🎉 Ready to Test!

**Status:** Build successful ✅  
**Endpoints:** 4 new POST endpoints  
**Files:** 13 new files created  
**Tests:** Postman collection with 10 requests  
**Docs:** 2500+ lines of documentation

**⭐ TIP:** Use `POST /adamo/session-with-results` for complete evaluations - it's atomic and safer!
