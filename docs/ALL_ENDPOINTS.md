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

| Endpoint                               | Lookup By              | Example                          |
| -------------------------------------- | ---------------------- | -------------------------------- |
| `GET /adamo/initial/{id}`              | MapInitialId           | `/adamo/initial/12345`           |
| `GET /adamo/initial/gr/{grNumber}`     | GR_NUMBER              | `/adamo/initial/gr/GR-88-0681-1` |
| `GET /adamo/session/{id}`              | SessionId              | `/adamo/session/4111`            |
| `GET /adamo/result/{id}`               | ResultId               | `/adamo/result/207`              |
| `GET /adamo/odor/{id}`                 | OdorCharacterizationId | `/adamo/odor/5000`               |
| `GET /adamo/odor/gr/{grNumber}`        | GR_NUMBER              | `/adamo/odor/gr/GR-88-0681-1`    |
| `GET /adamo/odorfamily/{id}`           | FamilyId               | `/adamo/odorfamily/5`            |
| `GET /adamo/odordescriptor/{id}`       | DescriptorId           | `/adamo/odordescriptor/63`       |
| `GET /adamo/sessionlink/{cpId}/{ffId}` | CP+FF Session IDs      | `/adamo/sessionlink/100/200`     |
| `GET /adamo/ignored/{grNumber}`        | GR_NUMBER              | `/adamo/ignored/GR-99-9999-9`    |

**Database:** Oracle (ADAMO)  
**Schema:** GIV_MAP  
**Tables:** MAP_INITIAL, MAP_SESSION, MAP_RESULT, ODOR_CHARACTERIZATION, MAP_ODOR_FAMILY, MAP_ODOR_DESCRIPTOR, MAP1_SESSION_LINK, SUBMITTING_IGNORED_MOLECULES

---

## 🔍 MAP Tool Lookups (6 endpoints)

**Base Route:** `/maptool/`

| Endpoint                               | Lookup By             | Example                             |
| -------------------------------------- | --------------------- | ----------------------------------- |
| `GET /maptool/molecule/{id}`           | Molecule Id           | `/maptool/molecule/123`             |
| `GET /maptool/molecule/gr/{grNumber}`  | GR_NUMBER             | `/maptool/molecule/gr/GR-88-0681-1` |
| `GET /maptool/assessment/{id}`         | Assessment Id         | `/maptool/assessment/456`           |
| `GET /maptool/evaluation/{id}`         | Evaluation Id         | `/maptool/evaluation/789`           |
| `GET /maptool/moleculeevaluation/{id}` | MoleculeEvaluation Id | `/maptool/moleculeevaluation/1011`  |
| `GET /maptool/odorfamily/{id}`         | OdorFamily Id         | `/maptool/odorfamily/5`             |
| `GET /maptool/odordescriptor/{id}`     | OdorDescriptor Id     | `/maptool/odordescriptor/25`        |

**Database:** PostgreSQL (MAP Tool)  
**Schema:** map_adm  
**Tables:** Molecule, Assessment, Map1_1Evaluation, Map1_1MoleculeEvaluation, OdorFamily, OdorDescriptor

---

## 🔄 Transformation (13 endpoints)

### Generic Transformations (2)

```
POST /transform/map-to-adamo     → Generic: Transform MAP Tool Molecule+Eval → ADAMO MapInitial
POST /transform/adamo-to-map     → Generic: Transform ADAMO Session+Result → MAP Tool Assessment
```

### Entity-Specific End-to-End Transformations (11)

**ADAMO → MAP Tool (8 endpoints):**

| #   | Endpoint                                            | Fetch From                       | Transform To                      | Example                                          |
| --- | --------------------------------------------------- | -------------------------------- | --------------------------------- | ------------------------------------------------ |
| 1   | `POST /transform/odorfamily/adamo-to-map/{id}`      | ADAMO MAP_ODOR_FAMILY            | MAP Tool OdorFamily               | `/transform/odorfamily/adamo-to-map/1`           |
| 2   | `POST /transform/odordescriptor/adamo-to-map/{id}`  | ADAMO MAP_ODOR_DESCRIPTOR        | MAP Tool OdorDescriptor           | `/transform/odordescriptor/adamo-to-map/63`      |
| 3   | `POST /transform/initial-to-molecule/gr/{grNumber}` | ADAMO MAP_INITIAL                | MAP Tool Molecule                 | `/transform/initial-to-molecule/gr/GR-50-0789-0` |
| 4   | `POST /transform/session-to-assessment/{sessionId}` | ADAMO MAP_SESSION                | MAP Tool Assessment               | `/transform/session-to-assessment/4111`          |
| 5   | `POST /transform/result-to-evaluation/{resultId}`   | ADAMO MAP_RESULT                 | MAP Tool Map1_1MoleculeEvaluation | `/transform/result-to-evaluation/207`            |
| 6   | `POST /transform/odorchar-to-details/gr/{grNumber}` | ADAMO ODOR_CHARACTERIZATION      | MAP Tool OdorDetails (complex)    | `/transform/odorchar-to-details/gr/GR-50-0789-0` |
| 7   | `POST /transform/sessionlink/adamo/{cpId}/{ffId}`   | ADAMO MAP1_SESSION_LINK          | Info only (no MAP Tool equivalent)| `/transform/sessionlink/adamo/100/200`           |
| 8   | `POST /transform/ignored-to-molecule/gr/{grNumber}` | ADAMO SUBMITTING_IGNORED_MOLECULES| MAP Tool Molecule (Status=Ignore) | `/transform/ignored-to-molecule/gr/GR-99-9999-9` |

**MAP Tool → ADAMO (3 endpoints):**

| #   | Endpoint                                               | Fetch From            | Transform To      | Example                                          |
| --- | ------------------------------------------------------ | --------------------- | ----------------- | ------------------------------------------------ |
| 9   | `POST /transform/molecule-to-initial/gr/{grNumber}`    | MAP Tool Molecule     | ADAMO MAP_INITIAL | `/transform/molecule-to-initial/gr/GR-50-0789-0` |
| 10  | `POST /transform/assessment-to-session/{assessmentId}` | MAP Tool Assessment   | ADAMO MAP_SESSION | `/transform/assessment-to-session/456`           |
| 11  | `POST /transform/evaluation-to-session/{evaluationId}` | MAP Tool Map1_1Evaluation | ADAMO MAP_SESSION | `/transform/evaluation-to-session/789`           |

**Query Parameters:**

- `?writeToDb=true` - Write transformed data to target database (requires EnableDatabaseWrites=true)

**Features:**

- Fetches data from source database
- Transforms to target format
- Optionally writes to target database (commented out, dry-run mode)
- All TODO items documented in code

---

## 📦 Migration (2 endpoints - same functionality)

### Simple GET (Recommended for One-Time Use)

```
GET /migration/adamo-to-maptool → Just trigger migration with defaults
```

**Use Case:** Someone just wants to click a link or hit URL once  
**Settings:** Uses defaults (batch: 1000, all entity types enabled)  
**Example:**

```bash
# Simple - just GET the URL
curl http://localhost:8085/migration/adamo-to-maptool

# Or just paste in browser
http://localhost:8085/migration/adamo-to-maptool
```

### Advanced POST (For Custom Options)

```
POST /migration/adamo-to-maptool → Bulk migration with custom settings
```

**Use Case:** Need to customize batch size, filter by stage, or disable certain entity types  
**Request Body:**

```json
{
  "batchSize": 500,
  "stageFilter": "MAP 3",
  "afterDate": "2024-01-01",
  "migrateInitialData": true,
  "migrateOdorFamilies": true,
  "migrateOdorDescriptors": true,
  "migrateOdorCharacterizations": false,
  "migrateIgnoredMolecules": false
}
```

**Both require:** `EnableMigration: true` in configuration

---

### What Migration Does (6 Steps)

1. **OdorFamilies** - ADAMO MAP_ODOR_FAMILY (12) → MAP Tool OdorFamily
2. **OdorDescriptors** - ADAMO MAP_ODOR_DESCRIPTOR (88) → MAP Tool OdorDescriptor
3. **Molecules** - ADAMO MAP_INITIAL → MAP Tool Molecule
4. **Assessments** - ADAMO MAP_SESSION → MAP Tool Assessment
5. **OdorCharacterizations** - ADAMO ODOR_CHARACTERIZATION → MAP Tool OdorDetails (complex)
6. **IgnoredMolecules** - ADAMO SUBMITTING_IGNORED_MOLECULES (optional - no MAP Tool equivalent)

---

## 📊 Complete Endpoint Count

| Category                        | Count            |
| ------------------------------- | ---------------- |
| Health & Debug                  | 4                |
| ADAMO Lookups                   | 10               |
| MAP Tool Lookups                | 7                |
| Generic Transformations         | 2                |
| Entity-Specific Transformations | 11               |
| Migration (GET + POST)          | 2                |
| **TOTAL**                       | **36 endpoints** |

---

## 🧪 Testing Examples

### ADAMO Lookups

```bash
# Lookup MAP_INITIAL by GR_NUMBER (VERIFIED WORKING ✓)
curl http://localhost:8085/adamo/initial/gr/GR-50-0789-0

# Lookup Session
curl http://localhost:8085/adamo/session/5824

# Lookup Odor Characterization by GR_NUMBER
curl http://localhost:8085/adamo/odor/gr/GR-50-0789-0

# Lookup Odor Family
curl http://localhost:8085/adamo/odorfamily/1
```

### MAP Tool Lookups

```bash
# Lookup Molecule by GR_NUMBER
curl http://localhost:8085/maptool/molecule/gr/GR-88-0681-1

# Lookup Molecule by ID
curl http://localhost:8085/maptool/molecule/123

# Lookup Assessment
curl http://localhost:8085/maptool/assessment/456
```

### Entity-Specific Transformations

```bash
# Fetch from ADAMO, transform OdorFamily, return MAP Tool format (VERIFIED WORKING ✓)
curl -X POST http://localhost:8085/transform/odorfamily/adamo-to-map/1

# Fetch ADAMO MAP_INITIAL, transform to Molecule (VERIFIED WORKING ✓)
curl -X POST http://localhost:8085/transform/initial-to-molecule/gr/GR-50-0789-0

# Fetch ADAMO Session, transform to Assessment
curl -X POST http://localhost:8085/transform/session-to-assessment/5824

# With database write enabled (dry-run by default)
curl -X POST "http://localhost:8085/transform/initial-to-molecule/gr/GR-50-0789-0?writeToDb=true"
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

---

## ✅ Verified Working Endpoints

**Tested with Real Data:**

- ✅ GET /adamo/initial/gr/GR-50-0789-0 → Found real molecule from Oracle
- ✅ GET /debug/test-oracle → Connected, found 5 sessions
- ✅ POST /transform/odorfamily/adamo-to-map/1 → Transformed "Ambergris" family
- ✅ POST /transform/initial-to-molecule/gr/GR-50-0789-0 → Fetched & transformed

---

**31 endpoints ready for your demo!** 🎉
