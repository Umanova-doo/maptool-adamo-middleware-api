# MapTool → Adamo Endpoints - Visual Guide

## 🎯 The 4 New Endpoints

```
┌─────────────────────────────────────────────────────────────────┐
│                    MapTool → Adamo Integration                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  1. POST /adamo/initial                                         │
│     Creates: MAP_INITIAL (molecule info)                        │
│     Input:   grNumber (required) + odor descriptions            │
│     Returns: mapInitialId, regNumber, batch                     │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  2. POST /adamo/session                                         │
│     Creates: MAP_SESSION (evaluation session)                   │
│     Input:   stage, segment, region, participants               │
│     Returns: sessionId                                          │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  3. POST /adamo/result                                          │
│     Creates: MAP_RESULT (single molecule result)                │
│     Input:   sessionId (required), grNumber, odor, result       │
│     Returns: resultId                                           │
│     Note:    Requires existing sessionId from step 2            │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  4. POST /adamo/session-with-results  ⭐ RECOMMENDED            │
│     Creates: MAP_SESSION + multiple MAP_RESULT records          │
│     Input:   { session: {...}, results: [{...}, {...}] }        │
│     Returns: session + all results + count                      │
│     Feature: Atomic transaction (all-or-nothing)                │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Two Workflows

### Workflow A: Separate Calls (Manual Control)

```
Step 1: POST /adamo/initial
        ↓
   Returns: mapInitialId
        ↓
Step 2: POST /adamo/session
        ↓
   Returns: sessionId = 5001
        ↓
Step 3: POST /adamo/result (sessionId: 5001, grNumber: "GR-25-0001-1")
        ↓
   Returns: resultId = 98765
        ↓
Step 4: POST /adamo/result (sessionId: 5001, grNumber: "GR-25-0002-1")
        ↓
   Returns: resultId = 98766
        ↓
Step 5: POST /adamo/result (sessionId: 5001, grNumber: "GR-25-0003-1")
        ↓
   Returns: resultId = 98767

✅ Done: 1 session + 3 results (5 API calls)
❌ Risk: If step 4 fails, you have orphaned results
```

### Workflow B: Combined Call (⭐ RECOMMENDED)

```
Step 1: POST /adamo/initial
        ↓
   Returns: mapInitialId
        ↓
Step 2: POST /adamo/session-with-results
        {
          session: { stage, segment, region, ... },
          results: [
            { grNumber: "GR-25-0001-1", ... },
            { grNumber: "GR-25-0002-1", ... },
            { grNumber: "GR-25-0003-1", ... }
          ]
        }
        ↓
   Returns: {
     session: { sessionId: 5001 },
     results: [
       { resultId: 98765, ... },
       { resultId: 98766, ... },
       { resultId: 98767, ... }
     ],
     resultCount: 3
   }

✅ Done: 1 session + 3 results (2 API calls)
✅ Safe: Atomic transaction - all or nothing
```

---

## 📋 MapTool Evaluation Types → Adamo Parameters

```
┌──────────────────┬────────────┬───────────┬──────────┐
│ MapTool Type     │ STAGE      │ SUB_STAGE │ SEGMENT  │
├──────────────────┼────────────┼───────────┼──────────┤
│ MAP 1.1          │ "MAP 1"    │ 1         │ CP/FF    │
│ MAP 1.2 CP       │ "MAP 1"    │ 2         │ CP       │
│ MAP 1.2 FF       │ "MAP 1"    │ 2         │ FF       │
│ MAP 1.3 CP       │ "MAP 1"    │ 3         │ CP       │
│ MAP 2.1 CP       │ "MAP 2"    │ 1         │ CP       │
│ MAP 2.1 FF       │ "MAP 2"    │ 1         │ FF       │
│ MAP 2.2 CP       │ "MAP 2"    │ 2         │ CP       │
│ MAP 2.2 FF       │ "MAP 2"    │ 2         │ FF       │
│ MAP 3.0 FF       │ "MAP 3"    │ 0         │ FF       │
└──────────────────┴────────────┴───────────┴──────────┘

Pattern: All evaluations use the SAME endpoint!
         Just change these 3 values ↑↑↑
```

---

## 🎨 Visual Request Structure

### POST /adamo/session-with-results

```
┌─────────────────────────────────────────────────────┐
│ Request Body                                        │
├─────────────────────────────────────────────────────┤
│                                                     │
│  "session": {                                       │
│    ┌─────────────────────────────────────────┐     │
│    │ "stage": "MAP 1"         ← Change this  │     │
│    │ "subStage": 1            ← Change this  │     │
│    │ "segment": "CP"          ← Change this  │     │
│    │ "region": "US"                          │     │
│    │ "evaluationDate": "2025-11-03T...",     │     │
│    │ "participants": "Smith, Jones",         │     │
│    │ "createdBy": "MAPTOOL"                  │     │
│    └─────────────────────────────────────────┘     │
│  },                                                 │
│                                                     │
│  "results": [                                       │
│    ┌─────────────────────────────────────────┐     │
│    │ {                                       │     │
│    │   "grNumber": "GR-25-0010-1",          │     │
│    │   "odor": "Fresh citrus",              │     │
│    │   "result": 5,                         │     │
│    │   "dilution": "10% in DPG"             │     │
│    │ },                                      │     │
│    │ {                                       │     │
│    │   "grNumber": "GR-25-0011-1",          │     │
│    │   "odor": "Woody base",                │     │
│    │   "result": 4                          │     │
│    │ }                                       │     │
│    └─────────────────────────────────────────┘     │
│  ]                                                  │
│                                                     │
└─────────────────────────────────────────────────────┘
        ↓
    API processes
        ↓
┌─────────────────────────────────────────────────────┐
│ Response (201 Created)                              │
├─────────────────────────────────────────────────────┤
│                                                     │
│  "session": {                                       │
│    "sessionId": 5002,        ← Auto-generated      │
│    "stage": "MAP 1",                                │
│    "subStage": 1,                                   │
│    ...                                              │
│  },                                                 │
│  "results": [                                       │
│    {                                                │
│      "resultId": 98766,      ← Auto-generated      │
│      "sessionId": 5002,      ← Links to session    │
│      "grNumber": "GR-25-0010-1",                   │
│      "regNumber": "GR-25-0010", ← Auto-extracted   │
│      "batch": 1,             ← Auto-extracted      │
│      "result": 5                                    │
│    },                                               │
│    {                                                │
│      "resultId": 98767,      ← Auto-generated      │
│      "sessionId": 5002,      ← Links to session    │
│      "grNumber": "GR-25-0011-1",                   │
│      "result": 4                                    │
│    }                                                │
│  ],                                                 │
│  "resultCount": 2                                   │
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## 📊 Database Tables Created

```
Oracle Database (GIV_MAP schema)
─────────────────────────────────

┌──────────────────┐
│   MAP_INITIAL    │  ← Endpoint 1: POST /adamo/initial
├──────────────────┤
│ MAP_INITIAL_ID ◄─┼─ Auto-generated (sequence)
│ GR_NUMBER        │  Required
│ REG_NUMBER     ◄─┼─ Auto-extracted from GR_NUMBER
│ BATCH          ◄─┼─ Auto-extracted from GR_NUMBER
│ CHEMIST          │  Optional
│ ODOR0H           │  Optional
│ ODOR4H           │  Optional
│ ODOR24H          │  Optional
│ ...              │
└──────────────────┘

┌──────────────────┐
│   MAP_SESSION    │  ← Endpoint 2: POST /adamo/session
├──────────────────┤     Endpoint 4: POST /adamo/session-with-results
│ SESSION_ID     ◄─┼─ Auto-generated (sequence)
│ STAGE            │  "MAP 1", "MAP 2", "MAP 3"
│ SUB_STAGE        │  0, 1, 2, 3
│ SEGMENT          │  "CP" or "FF"
│ REGION           │  "US", "EU", "AS", etc.
│ PARTICIPANTS     │
│ ...              │
└────────┬─────────┘
         │
         │ 1:N relationship
         ↓
┌──────────────────┐
│   MAP_RESULT     │  ← Endpoint 3: POST /adamo/result
├──────────────────┤     Endpoint 4: POST /adamo/session-with-results
│ RESULT_ID      ◄─┼─ Auto-generated (sequence)
│ SESSION_ID ─────►│  Foreign key to MAP_SESSION
│ GR_NUMBER        │  Required
│ REG_NUMBER     ◄─┼─ Auto-extracted from GR_NUMBER
│ BATCH          ◄─┼─ Auto-extracted from GR_NUMBER
│ ODOR             │  Optional
│ RESULT           │  1-5 score
│ DILUTION         │  Optional
│ ...              │
└──────────────────┘
```

---

## 🎓 Learning Path

1. **Read:** [QUICK_START_NEW_ENDPOINTS.md](../QUICK_START_NEW_ENDPOINTS.md)
2. **Test:** Import Postman collection, run "Create SESSION with RESULTS"
3. **Integrate:** Read [MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md](./MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md)
4. **Build:** Implement sync logic in your MapTool application
5. **Deploy:** Add to production with error handling

---

## ⚡ Quick Reference

| What You Want to Do                            | Use This Endpoint                     |
| ---------------------------------------------- | ------------------------------------- |
| Create molecule info                           | `POST /adamo/initial`                 |
| Create complete evaluation (session + results) | `POST /adamo/session-with-results` ⭐ |
| Create session only                            | `POST /adamo/session`                 |
| Add result to existing session                 | `POST /adamo/result`                  |

**90% of the time:** Use `POST /adamo/session-with-results`

---

## 📱 Postman Quick Actions

Import collection → Set `baseUrl` → Run these:

1. **"Create SESSION with RESULTS (Combined)"** ⭐ - Most common
2. **"Create MAP_INITIAL"** - For molecule info
3. **"Get MAP_SESSION by ID"** - Verify session
4. **"Get MAP_RESULT by ID"** - Verify result

---

**Ready to integrate!** 🚀

For questions, see [FINAL_IMPLEMENTATION_SUMMARY.md](docs/FINAL_IMPLEMENTATION_SUMMARY.md)
