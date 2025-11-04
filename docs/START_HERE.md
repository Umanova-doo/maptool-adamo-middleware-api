# ⭐ START HERE - MAP2ADAMOINT API

**Welcome to the MAP2ADAMOINT Integration API!**

This is your entry point to understanding and using the API.

---

## 🎯 What is This?

MAP2ADAMOINT is a middleware API that connects:

- **MapTool** (PostgreSQL) - Molecule evaluation system
- **Adamo** (Oracle) - Assessment database

**You can:**

- ✅ Query data from both databases
- ✅ Transform data between systems
- ✅ Create new records in Adamo from MapTool
- ✅ Migrate data in bulk

---

## 🚀 Quick Start (3 Steps)

### 1. Test the API

```bash
dotnet run
```

### 2. Import Postman Collection

File: `MAP2ADAMOINT-Creation-Endpoints.postman_collection.json`  
Set `baseUrl` = `http://localhost:5000`

### 3. Run Your First Request

Run: **"Create SESSION with RESULTS (Combined)"**

---

## 📖 Documentation

### New Developer Path

1. **Read:** [README.md](./README.md) - Documentation index
2. **Test:** [guides/QUICK_START_NEW_ENDPOINTS.md](./guides/QUICK_START_NEW_ENDPOINTS.md)
3. **Integrate:** [guides/MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md](./guides/MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md)

### Most Important Endpoints (NEW - Nov 2025)

**⭐ RECOMMENDED:** `POST /adamo/session-with-results`

Creates evaluation session + all molecule results in ONE atomic transaction.

**Documentation:** [endpoints/MAP_RESULT_ENDPOINTS.md](./endpoints/MAP_RESULT_ENDPOINTS.md)

---

## 📁 Documentation Structure

```
docs/
├── README.md                    ← Documentation index
├── START_HERE.md               ← This file
│
├── endpoints/                  ← API Endpoint Docs
│   ├── MAP_INITIAL_SESSION_ENDPOINTS.md
│   ├── MAP_RESULT_ENDPOINTS.md
│   └── DEBUG_ENDPOINTS.md
│
├── guides/                     ← How-To Guides
│   ├── MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md ⭐
│   ├── QUICK_START_NEW_ENDPOINTS.md
│   ├── API_USAGE_EXAMPLES.md
│   └── POSTMAN_TESTING_GUIDE.md
│
├── reference/                  ← Quick References
│   ├── ALL_ENDPOINTS.md
│   ├── NEW_ENDPOINTS_SUMMARY.md
│   └── ENDPOINTS_VISUAL_GUIDE.md
│
└── setup/                      ← Database & Setup
    ├── adamo-DATABASE_STRUCTURE.md
    └── maptool-DATABASE-DOCUMENTATION.md
```

---

## 💡 Key Insight

All 9 MapTool evaluation types use the **SAME endpoint** - just change 3 parameters:

- `stage` → "MAP 1", "MAP 2", or "MAP 3"
- `subStage` → 0, 1, 2, or 3
- `segment` → "CP" or "FF"

---

## 🆘 Quick Help

| I want to...        | Go to...                                                                                       |
| ------------------- | ---------------------------------------------------------------------------------------------- |
| Test new endpoints  | [guides/QUICK_START_NEW_ENDPOINTS.md](./guides/QUICK_START_NEW_ENDPOINTS.md)                   |
| Integrate MapTool   | [guides/MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md](./guides/MAPTOOL_TO_ADAMO_INTEGRATION_GUIDE.md) |
| See all endpoints   | [reference/ALL_ENDPOINTS.md](./reference/ALL_ENDPOINTS.md)                                     |
| Understand database | [setup/adamo-DATABASE_STRUCTURE.md](./setup/adamo-DATABASE_STRUCTURE.md)                       |

---

**Next:** Read [README.md](./README.md) for complete navigation

**Ready to integrate!** 🚀
