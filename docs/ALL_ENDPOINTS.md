# Complete API Endpoint Reference

All available endpoints in the MAP2ADAMOINT middleware API.

---

## 🏥 Health & Debug (5 endpoints)

### Health Check
```
GET /health
```
Returns API status

### Database Connectivity Tests
```
GET /debug/test-postgres    → Test PostgreSQL connection
GET /debug/test-oracle       → Test Oracle connection
GET /debug/test-both         → Test both databases
```

---

## 🔍 ADAMO Lookups (8 endpoints)

**Base Route:** `/adamo/`

| Endpoint | Lookup By | Example |
|----------|-----------|---------|
| `GET /adamo/initial/{id}` | MapInitialId | `/adamo/initial/12345` |
| `GET /adamo/initial/gr/{grNumber}` | GR_NUMBER | `/adamo/initial/gr/GR-88-0681-1` |
| `GET /adamo/session/{id}` | SessionId | `/adamo/session/4111` |
| `GET /adamo/result/{id}` | ResultId | `/adamo/result/207` |
| `GET /adamo/odor/{id}` | OdorCharacterizationId | `/adamo/odor/5000` |
| `GET /adamo/odor/gr/{grNumber}` | GR_NUMBER | `/adamo/odor/gr/GR-88-0681-1` |
| `GET /adamo/odorfamily/{id}` | FamilyId | `/adamo/odorfamily/5` |
| `GET /adamo/odordescriptor/{id}` | DescriptorId | `/adamo/odordescriptor/63` |
| `GET /adamo/sessionlink/{cpId}/{ffId}` | CP+FF Session IDs | `/adamo/sessionlink/100/200` |
| `GET /adamo/ignored/{grNumber}` | GR_NUMBER | `/adamo/ignored/GR-99-9999-9` |

**Database:** Oracle (ADAMO)  
**Schema:** GIV_MAP  
**Tables:** MAP_INITIAL, MAP_SESSION, MAP_RESULT, ODOR_CHARACTERIZATION, MAP_ODOR_FAMILY, MAP_ODOR_DESCRIPTOR, MAP1_SESSION_LINK, SUBMITTING_IGNORED_MOLECULES

---

## 🔍 MAP Tool Lookups (6 endpoints)

**Base Route:** `/maptool/`

| Endpoint | Lookup By | Example |
|----------|-----------|---------|
| `GET /maptool/molecule/{id}` | Molecule Id | `/maptool/molecule/123` |
| `GET /maptool/molecule/gr/{grNumber}` | GR_NUMBER | `/maptool/molecule/gr/GR-88-0681-1` |
| `GET /maptool/assessment/{id}` | Assessment Id | `/maptool/assessment/456` |
| `GET /maptool/evaluation/{id}` | Evaluation Id | `/maptool/evaluation/789` |
| `GET /maptool/moleculeevaluation/{id}` | MoleculeEvaluation Id | `/maptool/moleculeevaluation/1011` |
| `GET /maptool/odorfamily/{id}` | OdorFamily Id | `/maptool/odorfamily/5` |
| `GET /maptool/odordescriptor/{id}` | OdorDescriptor Id | `/maptool/odordescriptor/25` |

**Database:** PostgreSQL (MAP Tool)  
**Schema:** map_adm  
**Tables:** Molecule, Assessment, Map1_1Evaluation, Map1_1MoleculeEvaluation, OdorFamily, OdorDescriptor

---

## 🔄 Transformation (2 endpoints)

```
POST /transform/map-to-adamo     → Transform MAP Tool → ADAMO format
POST /transform/adamo-to-map     → Transform ADAMO → MAP Tool format
```

---

## 📦 Migration (1 endpoint)

```
POST /migration/adamo-to-maptool → Bulk migration ADAMO → MAP Tool
```
(Requires `EnableMigration: true`)

---

## 📊 Complete Endpoint Count

| Category | Count |
|----------|-------|
| Health & Debug | 4 |
| ADAMO Lookups | 10 |
| MAP Tool Lookups | 7 |
| Transformation | 2 |
| Migration | 1 |
| **TOTAL** | **24 endpoints** |

---

## 🧪 Testing Examples

### ADAMO Lookups

```bash
# Lookup MAP_INITIAL by ID
curl http://localhost:8085/adamo/initial/12345

# Lookup MAP_INITIAL by GR_NUMBER
curl http://localhost:8085/adamo/initial/gr/GR-88-0681-1

# Lookup Session
curl http://localhost:8085/adamo/session/4111

# Lookup Odor Characterization by GR_NUMBER
curl http://localhost:8085/adamo/odor/gr/GR-88-0681-1
```

### MAP Tool Lookups

```bash
# Lookup Molecule by ID
curl http://localhost:8085/maptool/molecule/123

# Lookup Molecule by GR_NUMBER
curl http://localhost:8085/maptool/molecule/gr/GR-88-0681-1

# Lookup Assessment
curl http://localhost:8085/maptool/assessment/456
```

---

## 📋 Response Formats

### Success (200 OK)
```json
{
  "status": "success",
  "table": "MAP_INITIAL",
  "data": {
    "mapInitialId": 12345,
    "grNumber": "GR-88-0681-1",
    "chemist": "Dr. Kraft",
    ...
  }
}
```

### Not Found (404)
```json
{
  "status": "not_found",
  "message": "GR_NUMBER 'GR-99-9999-9' not found in MAP_INITIAL",
  "grNumber": "GR-99-9999-9",
  "table": "GIV_MAP.MAP_INITIAL"
}
```

### Error (500)
```json
{
  "status": "fail",
  "message": "Database query failed",
  "error": "Connection timeout"
}
```

---

## 🎯 Models Coverage

### ADAMO (Oracle) - 8/8 Models ✓

- ✅ MapInitial
- ✅ MapSession
- ✅ MapResult
- ✅ OdorCharacterization
- ✅ MapOdorFamily
- ✅ MapOdorDescriptor
- ✅ Map1SessionLink
- ✅ SubmittingIgnoredMolecules

### MAP Tool (PostgreSQL) - 6/6 Core Models ✓

- ✅ Molecule
- ✅ Assessment
- ✅ Map1_1Evaluation
- ✅ Map1_1MoleculeEvaluation
- ✅ OdorFamily
- ✅ OdorDescriptor

---

## 🚀 All Credentials from appsettings.json

**NO hardcoded credentials anywhere!**

All database connections use:
- `builder.Configuration.GetConnectionString("MapToolDb")`
- `builder.Configuration.GetConnectionString("AdamoDb")`
- Proper Dependency Injection

---

**24 endpoints ready for your demo!** 🎉

