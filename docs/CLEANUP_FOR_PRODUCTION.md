# Production Cleanup Checklist

**For:** Removing debug, test, and unused endpoints before production deployment  
**Status:** 📋 To be executed when ready for production handover

---

## 🎯 Endpoints to Remove

### Debug & Health Endpoints (4 endpoints)

**File:** `Controllers/DebugController.cs`

| Endpoint                   | Purpose                    | Remove?                             |
| -------------------------- | -------------------------- | ----------------------------------- |
| `GET /debug/test-postgres` | Test PostgreSQL connection | ✅ Remove                           |
| `GET /debug/test-oracle`   | Test Oracle connection     | ✅ Remove                           |
| `GET /debug/test-both`     | Test both connections      | ✅ Remove                           |
| `GET /health`              | Health check               | ⚠️ **KEEP** (useful for monitoring) |

**Action:**

- ✅ Delete entire `Controllers/DebugController.cs` file
- ⚠️ Keep `Controllers/HealthController.cs` for production monitoring

---

### MapTool Lookup Endpoints (6 endpoints) - **Adamo → MapTool Direction**

**File:** `Controllers/MapToolController.cs`

**These read FROM MapTool (PostgreSQL) - not needed for MapTool → Adamo direction:**

| Endpoint                               | Purpose                      | Remove?   |
| -------------------------------------- | ---------------------------- | --------- |
| `GET /maptool/molecule/{id}`           | Get Molecule by ID           | ✅ Remove |
| `GET /maptool/molecule/gr/{grNumber}`  | Get Molecule by GR_NUMBER    | ✅ Remove |
| `GET /maptool/assessment/{id}`         | Get Assessment by ID         | ✅ Remove |
| `GET /maptool/evaluation/{id}`         | Get Evaluation by ID         | ✅ Remove |
| `GET /maptool/moleculeevaluation/{id}` | Get MoleculeEvaluation by ID | ✅ Remove |
| `GET /maptool/odorfamily/{id}`         | Get OdorFamily by ID         | ✅ Remove |
| `GET /maptool/odordescriptor/{id}`     | Get OdorDescriptor by ID     | ✅ Remove |

**Action:**

- ✅ Delete entire `Controllers/MapToolController.cs` file
- ✅ Can also remove `Data/MapToolContext.cs` if not used elsewhere
- ✅ Can remove MapTool models in `Models/MapTool/` if not used

**⚠️ NOTE:** Only remove if you're **100% certain** you won't need Adamo → MapTool direction!

---

### Transformation Endpoints (19 endpoints) - **Testing/Development Only**

**File:** `Controllers/TransformController.cs`

**These were for testing transformations - not needed in production:**

| Endpoint Category           | Count | Remove?   |
| --------------------------- | ----- | --------- |
| Generic transformations     | 2     | ✅ Remove |
| ADAMO → MAP Tool transforms | 10    | ✅ Remove |
| MAP Tool → ADAMO transforms | 7     | ✅ Remove |

**All 19 transformation endpoints can be removed.**

**Action:**

- ✅ Delete entire `Controllers/TransformController.cs` file
- ✅ Can remove `Services/DataMapperService.cs` if only used by TransformController

---

### Sync Endpoints (2 endpoints) - **Development/Testing**

**File:** `Controllers/SyncController.cs`

| Endpoint                  | Purpose                    | Remove?                                              |
| ------------------------- | -------------------------- | ---------------------------------------------------- |
| `POST /sync/map-to-adamo` | Batch sync MapTool → Adamo | ⚠️ **MAYBE KEEP** (if you want scheduled batch sync) |
| `POST /sync/adamo-to-map` | Batch sync Adamo → MapTool | ✅ Remove                                            |

**Action:**

- ✅ Remove `POST /sync/adamo-to-map` (Adamo → MapTool direction)
- ⚠️ Consider keeping `POST /sync/map-to-adamo` for batch operations
- Or ✅ Delete entire `Controllers/SyncController.cs` if you don't need batch sync
- ✅ Can remove `Services/SyncService.cs` if SyncController is deleted

---

### Migration Endpoints (1 endpoint) - **One-Time Use Only**

**File:** `Controllers/MigrationController.cs`

| Endpoint                | Purpose                     | Remove?                                  |
| ----------------------- | --------------------------- | ---------------------------------------- |
| `POST /migrate/execute` | Bulk migration all entities | ✅ Remove (after initial migration done) |

**Action:**

- ✅ Delete entire `Controllers/MigrationController.cs` after initial data migration is complete
- ✅ Can remove `Services/MigrationService.cs` after migration

---

## 📁 Files to Delete

### Controllers (5 files)

```
Controllers/
├── DebugController.cs              ← ✅ DELETE
├── MapToolController.cs            ← ✅ DELETE (if not using Adamo → MapTool)
├── TransformController.cs          ← ✅ DELETE
├── SyncController.cs               ← ⚠️ DELETE or KEEP (if using batch sync)
├── MigrationController.cs          ← ✅ DELETE (after migration complete)
├── HealthController.cs             ← ⚠️ KEEP (for monitoring)
└── AdamoController.cs              ← ⚠️ KEEP (your production endpoints!)
```

### Services (4 files)

```
Services/
├── DatabaseService.cs              ← ⚠️ CHECK (might be used by kept controllers)
├── DataMapperService.cs            ← ✅ DELETE (if TransformController deleted)
├── MigrationService.cs             ← ✅ DELETE (if MigrationController deleted)
├── SyncService.cs                  ← ⚠️ DELETE or KEEP (if SyncController kept)
└── FeatureFlags.cs                 ← ⚠️ KEEP (might be needed)
```

### Data Contexts (1 file - optional)

```
Data/
├── MapToolContext.cs               ← ✅ DELETE (if MapToolController deleted)
└── AdamoContext.cs                 ← ⚠️ KEEP (required for production!)
```

### Models (optional)

```
Models/
├── MapTool/                        ← ✅ DELETE entire folder (if MapToolController deleted)
├── Adamo/                          ← ⚠️ KEEP (required!)
└── DTOs/                           ← ⚠️ KEEP (required!)
```

---

## 🎯 What to KEEP for Production

### ✅ MUST KEEP (Production Endpoints)

**Controllers:**

- ✅ `AdamoController.cs` - Your 4 production endpoints
- ✅ `HealthController.cs` - For monitoring

**Services:**

- ✅ `DatabaseService.cs` - Database connectivity
- ✅ `FeatureFlags.cs` - Feature toggles
- ⚠️ `SyncService.cs` - Only if using batch sync

**Data:**

- ✅ `AdamoContext.cs` - Oracle EF context

**Models:**

- ✅ `Models/Adamo/*` - All Adamo entity models
- ✅ `Models/DTOs/*` - All request/response DTOs

---

## 📊 Before vs After Cleanup

### Before Cleanup (Development)

```
46 total endpoints:
- 4 Creation endpoints (production) ✅
- 10 ADAMO lookups (production) ✅
- 6 MapTool lookups ❌
- 19 Transformation endpoints ❌
- 4 Debug/Health ❌ (3 debug, 1 health)
- 2 Sync endpoints ❌
- 1 Migration endpoint ❌
```

### After Cleanup (Production)

```
15 total endpoints:
- 4 Creation endpoints (production) ✅
- 10 ADAMO lookups (production) ✅
- 1 Health check ✅
```

**Reduces from 46 → 15 endpoints** (clean, production-ready!)

---

## 🔒 Security Considerations

### Before Production:

1. **Remove debug endpoints** - They expose database connection info
2. **Add authentication** - Protect endpoints with API keys or JWT
3. **Add rate limiting** - Prevent abuse
4. **Review error messages** - Don't expose sensitive info
5. **Enable HTTPS** - Encrypt traffic
6. **Add logging** - Track API usage
7. **Remove test data** - Clean test records from Oracle

---

## 📝 Cleanup Script (Future)

**When ready, give me this document and say:**

> "Remove these endpoints and references from the code"

**I will:**

1. ✅ Delete all files marked for deletion
2. ✅ Remove unused using statements
3. ✅ Clean up dependency injection
4. ✅ Update documentation
5. ✅ Verify build succeeds
6. ✅ Create clean production-ready API

---

## 🎯 Production Endpoint Summary

**After cleanup, you'll have:**

### ADAMO Creation (4 endpoints) - **PRIMARY ENDPOINTS**

- `POST /adamo/initial`
- `POST /adamo/session`
- `POST /adamo/result`
- `POST /adamo/session-with-results` ⭐

### ADAMO Lookups (10 endpoints) - **READ OPERATIONS**

- `GET /adamo/initial/{id}`
- `GET /adamo/initial/gr/{grNumber}`
- `GET /adamo/session/{id}`
- `GET /adamo/result/{id}`
- `GET /adamo/odor/{id}`
- `GET /adamo/odor/gr/{grNumber}`
- `GET /adamo/odorfamily/{id}`
- `GET /adamo/odordescriptor/{id}`
- `GET /adamo/sessionlink/{cpId}/{ffId}`
- `GET /adamo/ignored/{grNumber}`

### Monitoring (1 endpoint)

- `GET /health`

**Total: 15 clean, production-ready endpoints**

---

## 🚀 When to Execute Cleanup

**Timing:**

- ✅ After successful testing
- ✅ After initial data migration complete
- ✅ Before production deployment
- ✅ Before handover to client/team

**Don't cleanup if:**

- ❌ Still testing
- ❌ Might need Adamo → MapTool direction later
- ❌ Haven't completed initial migration

---

## 📋 Cleanup Execution Plan

**When you're ready:**

1. **Backup your code** (git commit)
2. **Give me this document**
3. **Say:** "Remove these endpoints - execute cleanup"
4. **I will:**
   - Delete marked files
   - Clean up references
   - Update docs
   - Verify build
   - Provide clean production-ready API

---

**Status:** 📋 **Ready to Execute When You Decide**

**Current:** Development/Testing API (46 endpoints)  
**After Cleanup:** Production API (15 endpoints)

---

**Save this document for later when ready to clean up for production!**
