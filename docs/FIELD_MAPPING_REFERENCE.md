# Field Mapping Reference

Complete field-by-field mapping documentation between MAP Tool (PostgreSQL) and ADAMO (Oracle) databases.

---

## 📋 Table of Contents

1. [MAP Tool → ADAMO Mappings](#map-tool--adamo-mappings)
2. [ADAMO → MAP Tool Mappings](#adamo--map-tool-mappings)
3. [Missing Fields & TODOs](#missing-fields--todos)
4. [Data Transformation Notes](#data-transformation-notes)

---

## MAP Tool → ADAMO Mappings

### Molecule → MAP_INITIAL

Mapping handled in: `DataMapperService.MapMoleculeToMapInitial()`

| MAP Tool Field | ADAMO Field | Status | Notes |
|----------------|-------------|--------|-------|
| `Molecule.GrNumber` | `MAP_INITIAL.GR_NUMBER` | ✅ Mapped | Direct mapping |
| `Molecule.RegNumber` | `MAP_INITIAL.REG_NUMBER` | ✅ Mapped | Direct mapping |
| `Molecule.ChemistName` | `MAP_INITIAL.CHEMIST` | ✅ Mapped | Direct mapping |
| `Map1_1MoleculeEvaluation.EvaluationDate` | `MAP_INITIAL.EVALUATION_DATE` | ✅ Mapped | Falls back to CreatedAt |
| `Map1_1MoleculeEvaluation.Odor0h` | `MAP_INITIAL.ODOR0H` | ✅ Mapped | 0 hour odor description |
| `Map1_1MoleculeEvaluation.Odor4h` | `MAP_INITIAL.ODOR4H` | ✅ Mapped | 4 hour odor description |
| `Map1_1MoleculeEvaluation.Odor24h` | `MAP_INITIAL.ODOR24H` | ✅ Mapped | 24 hour odor description |
| `Molecule.CreatedBy` | `MAP_INITIAL.CREATED_BY` | ✅ Mapped | Audit trail |
| `Molecule.UpdatedBy` | `MAP_INITIAL.LAST_MODIFIED_BY` | ✅ Mapped | Audit trail |
| (Constructed) | `MAP_INITIAL.COMMENTS` | ✅ Mapped | Combines status & project |
| (Default) | `MAP_INITIAL.DILUTION` | ⚠️ Hardcoded | "10% in DPG" - needs lookup |
| - | `MAP_INITIAL.ASSESSOR` | ❌ Not Mapped | Not available in Molecule |
| - | `MAP_INITIAL.EVALUATION_SITE` | ❌ Not Mapped | Needs EvaluationSite lookup |
| - | `MAP_INITIAL.BATCH` | ⚙️ Auto-extracted | Oracle trigger extracts from GR_NUMBER |
| - | `MAP_INITIAL.MAP_INITIAL_ID` | ⚙️ Auto-generated | Oracle sequence |
| - | `MAP_INITIAL.CREATION_DATE` | ⚙️ Auto-set | Set by sync process |
| - | `MAP_INITIAL.LAST_MODIFIED_DATE` | ⚙️ Auto-set | Set by sync process |

**Fields NOT mapped from Molecule:**
- `Structure` - Not in ADAMO MAP_INITIAL
- `ChemicalName` - Not in ADAMO MAP_INITIAL
- `MolecularFormula` - Not in ADAMO MAP_INITIAL
- `ProjectName` - Included in Comments field only
- `Status` - Included in Comments field only
- `Quantity` - Not in ADAMO MAP_INITIAL
- `Assessed` - Not in ADAMO MAP_INITIAL
- `IsArchived` - Not in ADAMO MAP_INITIAL
- `IsManuallyArchived` - Not in ADAMO MAP_INITIAL

### Map1_1MoleculeEvaluation → MAP_RESULT

Mapping handled in: `DataMapperService.MapMoleculeEvaluationToResult()`

| MAP Tool Field | ADAMO Field | Status | Notes |
|----------------|-------------|--------|-------|
| (Parameter) | `MAP_RESULT.SESSION_ID` | ✅ Mapped | Passed as parameter |
| `Molecule.GrNumber` | `MAP_RESULT.GR_NUMBER` | ✅ Mapped | Via Molecule navigation |
| `Molecule.RegNumber` | `MAP_RESULT.REG_NUMBER` | ✅ Mapped | Via Molecule navigation |
| `Odor0h` (fallback 4h/24h) | `MAP_RESULT.ODOR` | ✅ Mapped | Priority: 0h → 4h → 24h |
| `Benchmark` | `MAP_RESULT.BENCHMARK_COMMENTS` | ✅ Mapped | Direct mapping |
| `ResultCP` OR `ResultFF` | `MAP_RESULT.RESULT` | ✅ Mapped | First non-null value |
| `Map1_1Evaluation.Participants` | `MAP_RESULT.SPONSOR` | ✅ Mapped | Via evaluation |
| `CreatedBy` | `MAP_RESULT.CREATED_BY` | ✅ Mapped | Audit trail |
| `UpdatedBy` | `MAP_RESULT.LAST_MODIFIED_BY` | ✅ Mapped | Audit trail |
| (Default) | `MAP_RESULT.DILUTION` | ⚠️ Hardcoded | "10%" - needs DilutionSolvent lookup |
| - | `MAP_RESULT.BATCH` | ⚙️ Auto-extracted | Oracle trigger |
| - | `MAP_RESULT.RESULT_ID` | ⚙️ Auto-generated | Oracle sequence |
| - | `MAP_RESULT.CREATION_DATE` | ⚙️ Auto-set | Set by sync |
| - | `MAP_RESULT.LAST_MODIFIED_DATE` | ⚙️ Auto-set | Set by sync |

**Fields NOT mapped:**
- `GrDilutionSolventId` - Stored as string in ADAMO
- `BenchmarkDilutionSolventId` - Not in ADAMO
- `Comment` - Not directly in ADAMO MAP_RESULT
- `FFNextSteps` - Not in ADAMO
- `CPNextSteps` - Not in ADAMO
- `SortOrder` - Not in ADAMO MAP_RESULT

---

## ADAMO → MAP Tool Mappings

### MAP_INITIAL → Molecule

Mapping handled in: `DataMapperService.MapInitialToMolecule()`

| ADAMO Field | MAP Tool Field | Status | Notes |
|-------------|----------------|--------|-------|
| `MAP_INITIAL.GR_NUMBER` | `Molecule.GrNumber` | ✅ Mapped | Direct mapping |
| `MAP_INITIAL.REG_NUMBER` | `Molecule.RegNumber` | ✅ Mapped | Direct mapping |
| `MAP_INITIAL.CHEMIST` | `Molecule.ChemistName` | ✅ Mapped | Direct mapping |
| `MAP_INITIAL.CREATED_BY` | `Molecule.CreatedBy` | ✅ Mapped | Audit trail |
| `MAP_INITIAL.LAST_MODIFIED_BY` | `Molecule.UpdatedBy` | ✅ Mapped | Audit trail |
| (Default: `Map1`) | `Molecule.Status` | ✅ Mapped | Default to Map1 |
| (Default: `true`) | `Molecule.Assessed` | ✅ Mapped | True if in ADAMO |
| (Default: `0`) | `Molecule.Quantity` | ⚠️ Default | Not tracked in ADAMO |
| (Default: `false`) | `Molecule.IsArchived` | ✅ Mapped | Default false |
| (Default: `false`) | `Molecule.IsManuallyArchived` | ✅ Mapped | Default false |
| - | `Molecule.Structure` | ❌ Not Mapped | Not in ADAMO |
| - | `Molecule.ChemicalName` | ❌ Not Mapped | Not in ADAMO |
| - | `Molecule.MolecularFormula` | ❌ Not Mapped | Not in ADAMO |
| - | `Molecule.ProjectName` | ❌ Not Mapped | Not in ADAMO |

**ADAMO Fields NOT mapped to Molecule:**
- `EVALUATION_DATE` - Would go to evaluation record
- `ASSESSOR` - Would go to evaluation record
- `DILUTION` - Would go to evaluation record
- `EVALUATION_SITE` - Would go to evaluation record
- `ODOR0H`, `ODOR4H`, `ODOR24H` - Would go to Map1_1MoleculeEvaluation
- `COMMENTS` - Could extract status/project info
- `BATCH` - Could be stored but not currently

### MAP_SESSION + MAP_RESULT → Assessment

Mapping handled in: `DataMapperService.MapResultToAssessment()`

| ADAMO Field | MAP Tool Field | Status | Notes |
|-------------|----------------|--------|-------|
| `MAP_SESSION.SESSION_ID` | `Assessment.SessionName` | ✅ Mapped | Formatted as "ADAMO-{id}" |
| `MAP_SESSION.EVALUATION_DATE` | `Assessment.DateTime` | ✅ Mapped | Direct mapping |
| `MAP_SESSION.STAGE` | `Assessment.Stage` | ✅ Mapped | MAP 0/1/2/3, ISC, etc. |
| `MAP_SESSION.REGION` | `Assessment.Region` | ✅ Mapped | Direct mapping |
| `MAP_SESSION.SEGMENT` | `Assessment.Segment` | ✅ Mapped | CP or FF |
| `MAP_SESSION.CREATED_BY` | `Assessment.CreatedBy` | ✅ Mapped | Audit trail |
| `MAP_SESSION.LAST_MODIFIED_BY` | `Assessment.UpdatedBy` | ✅ Mapped | Audit trail |
| (Default: `1`) | `Assessment.Status` | ⚠️ Default | Needs status code mapping |
| (Default: `false`) | `Assessment.IsClosed` | ⚠️ Default | Could use ShowInTaskList |
| (Default: `false`) | `Assessment.IsArchived` | ✅ Mapped | Default false |
| (Default: `false`) | `Assessment.IsManuallyArchived` | ✅ Mapped | Default false |

**ADAMO Fields NOT mapped to Assessment:**
- `PARTICIPANTS` - Could go to Map1_1Evaluation
- `SHOW_IN_TASK_LIST` - Could influence IsClosed
- `SUB_STAGE` - Not tracked in Assessment
- `CATEGORY` - Not tracked in Assessment

### MAP_RESULT → Map1_1MoleculeEvaluation

Mapping handled in: `DataMapperService.MapResultToMoleculeEvaluation()`

| ADAMO Field | MAP Tool Field | Status | Notes |
|-------------|----------------|--------|-------|
| (Parameter) | `Map1_1EvaluationId` | ✅ Mapped | Passed as parameter |
| (Parameter) | `MoleculeId` | ✅ Mapped | Passed as parameter |
| (Parameter) | `SortOrder` | ✅ Mapped | Passed as parameter |
| `MAP_RESULT.ODOR` | `Odor0h` | ✅ Mapped | Single field → 0h |
| `MAP_RESULT.BENCHMARK_COMMENTS` | `Benchmark` | ✅ Mapped | Direct mapping |
| `MAP_RESULT.RESULT` | `ResultCP` | ✅ Mapped | Same value for both |
| `MAP_RESULT.RESULT` | `ResultFF` | ✅ Mapped | Same value for both |
| `MAP_RESULT.SPONSOR` | `Comment` | ✅ Mapped | Formatted as "Sponsor: X" |
| - | `Odor4h` | ❌ Not Mapped | ADAMO has single odor field |
| - | `Odor24h` | ❌ Not Mapped | ADAMO has single odor field |
| - | `GrDilutionSolventId` | ❌ Not Mapped | ADAMO stores as string |
| - | `BenchmarkDilutionSolventId` | ❌ Not Mapped | Not in ADAMO |
| - | `FFNextSteps` | ❌ Not Mapped | Not in ADAMO |
| - | `CPNextSteps` | ❌ Not Mapped | Not in ADAMO |

**ADAMO Fields NOT mapped:**
- `DILUTION` - String format, not FK to lookup table

### MAP_SESSION → Map1_1Evaluation

Mapping handled in: `DataMapperService.MapSessionToEvaluation()`

| ADAMO Field | MAP Tool Field | Status | Notes |
|-------------|----------------|--------|-------|
| (Parameter) | `AssessmentId` | ✅ Mapped | Passed as parameter |
| (Parameter) | `EvaluationSiteId` | ⚠️ Parameter | Needs site code → ID lookup |
| `MAP_SESSION.PARTICIPANTS` | `Participants` | ✅ Mapped | Direct mapping |
| `MAP_SESSION.EVALUATION_DATE` | `EvaluationDate` | ✅ Mapped | Direct mapping |
| `MAP_SESSION.CREATED_BY` | `CreatedBy` | ✅ Mapped | Audit trail |
| `MAP_SESSION.LAST_MODIFIED_BY` | `UpdatedBy` | ✅ Mapped | Audit trail |

---

## Missing Fields & TODOs

### Critical TODOs (High Priority)

1. **Dilution Solvent Mapping**
   - **Issue**: ADAMO stores dilution as string ("10% in DPG"), MAP Tool uses FK to `DilutionSolvent` table
   - **Current**: Hardcoded to "10% in DPG" or "10%"
   - **Solution Needed**: Parse dilution string or create lookup table

2. **Evaluation Site Mapping**
   - **Issue**: ADAMO uses site codes (e.g., "ZH"), MAP Tool uses `EvaluationSite.Id`
   - **Current**: Set to null or requires parameter
   - **Solution Needed**: Create site code → site ID lookup table

3. **Status Code Mapping**
   - **Issue**: ADAMO and MAP Tool may use different status code systems
   - **Current**: Hardcoded to `1`
   - **Solution Needed**: Verify and map status codes between systems

4. **Assessor Field**
   - **Issue**: ADAMO tracks assessor, MAP Tool doesn't have equivalent in Molecule
   - **Current**: Not mapped
   - **Solution Needed**: Store in evaluation record or create new field

### Non-Critical Missing Fields

**In MAP Tool but not ADAMO:**
- `Molecule.Structure` - Chemical structure data
- `Molecule.ChemicalName` - IUPAC/common name
- `Molecule.MolecularFormula` - Chemical formula
- `Molecule.ProjectName` - Project association
- `Molecule.Quantity` - Inventory tracking
- `Map1_1MoleculeEvaluation.FFNextSteps` - Next steps for FF
- `Map1_1MoleculeEvaluation.CPNextSteps` - Next steps for CP

**In ADAMO but not MAP Tool:**
- `MAP_SESSION.SUB_STAGE` - Session sub-stage
- `MAP_SESSION.CATEGORY` - Session category
- `MAP_SESSION.SHOW_IN_TASK_LIST` - UI display flag
- `MAP_INITIAL.ASSESSOR` - Person who assessed
- `MAP_INITIAL.BATCH` - Batch number (could be added)

### Odor Characterization Fields

**Status**: ⚠️ **Minimal implementation**

- ADAMO has 100+ descriptor fields in `ODOR_CHARACTERIZATION`
- MAP Tool has `OdorFamily` and `OdorDescriptor` reference tables
- Current mapping only extracts 12 family-level scores
- Individual descriptor scores (88 fields) not yet mapped

**To fully implement**:
1. Map all 88+ descriptors from ADAMO to MAP Tool
2. Create `OdorDetail` records for each non-null descriptor
3. Link to appropriate `OdorDescriptor` via code matching

---

## Data Transformation Notes

### GR Number Format
- **Format**: `GR-YY-NNNNN-B` (e.g., "GR-88-0681-1")
- **Components**:
  - `GR`: Prefix
  - `YY`: Year (2 digits)
  - `NNNNN`: Sequential number (4-5 digits)
  - `B`: Batch number
- **Extraction**:
  - `REG_NUMBER`: Oracle trigger extracts `GR-YY-NNNNN`
  - `BATCH`: Oracle trigger extracts trailing digits

### Auto-Generated Fields

**ADAMO (Oracle)**:
- Primary keys via sequences
- Audit timestamps via triggers
- `REG_NUMBER` and `BATCH` via triggers

**MAP Tool (PostgreSQL)**:
- Primary keys via IDENTITY
- Audit timestamps via `SaveChanges()` override

### Data Type Conversions

| ADAMO Type | MAP Tool Type | Notes |
|------------|---------------|-------|
| `NUMBER(38,0)` | `long` | For large IDs |
| `NUMBER(*,0)` | `int` or `long` | Based on expected range |
| `NUMBER(2,0)` | `int?` | For scores (1-10) |
| `NUMBER(6,0)` | `int?` | For results (1-5) |
| `VARCHAR2(n)` | `string` | Direct mapping |
| `DATE` | `DateTime?` | Nullable in both |
| `TIMESTAMP(6)` | `DateTime?` | Nullable in both |
| `CHAR(1)` | `string` | Y/N flags |

---

## Mapping Completeness Summary

| Direction | Entity | Fields Mapped | Fields Missing | Completeness |
|-----------|--------|---------------|----------------|--------------|
| MAP → ADAMO | Molecule → MapInitial | 13/16 | 3 | 81% |
| MAP → ADAMO | Map1_1MoleculeEvaluation → MapResult | 10/12 | 2 | 83% |
| ADAMO → MAP | MapInitial → Molecule | 9/18 | 9 | 50% |
| ADAMO → MAP | MapSession → Assessment | 8/11 | 3 | 73% |
| ADAMO → MAP | MapResult → Map1_1MoleculeEvaluation | 7/14 | 7 | 50% |
| ADAMO → MAP | MapSession → Map1_1Evaluation | 5/6 | 1 | 83% |

**Overall**: ~70% field coverage with critical fields mapped

---

## References

- **Database Documentation**:
  - `docs/adamo-DATABASE_STRUCTURE.md` - Oracle schema
  - `docs/maptool-DATABASE-DOCUMENTATION.md` - PostgreSQL schema
  
- **Implementation**:
  - `Services/DataMapperService.cs` - All mapping methods
  - `Models/Adamo/` - Oracle entity models
  - `Models/MapTool/` - PostgreSQL entity models

---

**Last Updated**: October 29, 2025  
**Mapping Version**: 1.0

