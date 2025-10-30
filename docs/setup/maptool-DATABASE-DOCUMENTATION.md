# MAP2 (Molecule Assessment Program) Database Documentation

## Overview

This database is designed for the **MAP2 (Molecule Assessment Program)** system - a comprehensive platform for managing and evaluating fragrance molecules through multiple assessment stages. The system is used by perfumery professionals to track molecules from initial assessment through various evaluation levels (MAP 1, MAP 2, MAP 3) with detailed sensory analysis and panel-based evaluations.

### Primary Business Functions

- Track molecules with their chemical and structural information
- Manage multi-stage evaluation workflows (MAP 1.1, MAP 1.2, MAP 1.3, MAP 2.1, MAP 2.2, MAP 3.0)
- Record detailed odor characterizations with family and descriptor classifications
- Support both Consumer Preference (CP) and Fine Fragrance (FF) evaluation tracks
- Conduct panel-based smell evaluations with time-period assessments
- Manage trial compositions and benchmark comparisons
- Handle user permissions, teams, and panelist management
- Track evaluation sessions across multiple regions and sites

---

## Entity Relationship Overview

```
MOLECULE (1) ─────< (N) MAP1_MOLECULE_EVALUATION
                                   │
                                   └──< (N) ODOR_DETAIL ──> ODOR_DESCRIPTOR ──> ODOR_FAMILY

ASSESSMENT (1) ─────< (N) MAP1_1_EVALUATION
            │                      │
            │                      └──< (N) MAP1_1_MOLECULE_EVALUATION ──> MOLECULE
            │
            ├──< (N) MAP1_2CP_EVALUATION ──> PANELIST, BASES, TEST_CONDITION
            │              │
            │              └──< (N) MAP1_2CP_MOLECULE_EVALUATION ──> MOLECULE
            │
            ├──< (N) MAP1_3CP_EVALUATION
            │              │
            │              └──< (N) MAP1_3CP_MOLECULE_EVALUATION ──> MOLECULE
            │
            ├──< (N) MAP2_1CP_EVALUATION
            │              │
            │              ├──< (N) MAP2_1CP_PERFUME_CONFIGURATION
            │              │              │
            │              │              ├──< (N) MAP2_1CP_MOLECULE_EVALUATION
            │              │              ├──< (N) MAP2_1CP_EVALUATION_TIME_PERIOD
            │              │              └──< (N) MAP2_1CP_TRIAL_COMPOSITION
            │
            └──< (N) MAP2_2CP_EVALUATION, MAP2_2FF_EVALUATION, MAP3_0FF_EVALUATION

EVALUATION_SESSION (1) ─────< (N) SMELL_EVALUATION
                   │
                   └──< (1) SMELL_EVALUATION_SUMMARY

BUSINESS_LOGIC_USER ──> TEAM
                    │
                    └──> PERMISSION
```

---

## Database Schema

- **Schema Name**: `map_adm`
- **Database Type**: PostgreSQL
- **ORM Framework**: Entity Framework Core
- **Primary Key Strategy**: Auto-incrementing integers (`IDENTITY` generation)
- **Cascade Behavior**: Restrict (all foreign keys set to `DeleteBehavior.Restrict`)

---

## Core Entities

### 1. Molecule

**Purpose**: Stores chemical compound information for molecules being evaluated in the MAP system.

#### Table Structure

| Column               | Type             | Nullable | Description                                           |
| -------------------- | ---------------- | -------- | ----------------------------------------------------- |
| `Id`                 | `int`            | NOT NULL | Primary key, auto-generated                           |
| `GrNumber`           | `string`         | NULL     | Unique molecule identifier (e.g., "GR-88-0681-1")     |
| `RegNumber`          | `string`         | NULL     | Registration number (base identifier without batch)   |
| `Structure`          | `string`         | NULL     | Chemical structure data                               |
| `Assessed`           | `bool`           | NOT NULL | Flag indicating if molecule has been assessed         |
| `ChemistName`        | `string`         | NULL     | Name of chemist associated with molecule              |
| `ChemicalName`       | `string`         | NULL     | IUPAC or common chemical name                         |
| `MolecularFormula`   | `string`         | NULL     | Molecular formula                                     |
| `ProjectName`        | `string`         | NULL     | Associated project name                               |
| `Status`             | `MoleculeStatus` | NOT NULL | Enum: None, Map1, Map0Weak, Map0Odorless, Ignore      |
| `Quantity`           | `decimal`        | NOT NULL | Available quantity                                    |
| `CreatedAt`          | `DateTime?`      | NULL     | Auto-set on insert                                    |
| `UpdatedAt`          | `DateTime?`      | NULL     | Auto-updated on changes                               |
| `CreatedBy`          | `string`         | NULL     | User who created record (max 50 chars)                |
| `UpdatedBy`          | `string`         | NULL     | User who last modified record (max 50 chars)          |
| `IsArchived`         | `bool`           | NOT NULL | Soft delete flag                                      |
| `IsManuallyArchived` | `bool`           | NOT NULL | Manual archive flag (preserves state during unarchive |

#### MoleculeStatus Enum

- `None = 0`: Not yet evaluated
- `Map1 = 1`: Approved for MAP 1 evaluation
- `Map0Weak = 2`: Weak odor detected in MAP 0
- `Map0Odorless = 3`: No significant odor detected
- `Ignore = 4`: Excluded from further evaluation

#### .NET Model

```csharp
public class Molecule : BaseBoObject
{
    public string GrNumber { get; set; }
    public string RegNumber { get; set; }
    public string Structure { get; set; }
    public bool Assessed { get; set; }
    public string ChemistName { get; set; }
    public string ChemicalName { get; set; }
    public string MolecularFormula { get; set; }
    public string ProjectName { get; set; }
    public MoleculeStatus Status { get; set; }
    public decimal Quantity { get; set; }
}
```

---

### 2. Assessment

**Purpose**: Represents an assessment session - a container for evaluations at a specific stage, region, and segment.

#### Table Structure

| Column               | Type        | Nullable | Description                               |
| -------------------- | ----------- | -------- | ----------------------------------------- |
| `Id`                 | `int`       | NOT NULL | Primary key, auto-generated               |
| `SessionName`        | `string`    | NULL     | Human-readable session name               |
| `DateTime`           | `DateTime`  | NOT NULL | Date and time of assessment               |
| `Stage`              | `string`    | NULL     | Assessment stage (e.g., "MAP 1", "MAP 2") |
| `Status`             | `int`       | NOT NULL | Status code                               |
| `Region`             | `string`    | NULL     | Geographic region                         |
| `Segment`            | `string`    | NULL     | Market segment (e.g., "CP", "FF")         |
| `IsClosed`           | `bool`      | NOT NULL | Whether assessment is closed/finalized    |
| `CreatedAt`          | `DateTime?` | NULL     | Auto-set on insert                        |
| `UpdatedAt`          | `DateTime?` | NULL     | Auto-updated on changes                   |
| `CreatedBy`          | `string`    | NULL     | User who created record                   |
| `UpdatedBy`          | `string`    | NULL     | User who last modified record             |
| `IsArchived`         | `bool`      | NOT NULL | Soft delete flag                          |
| `IsManuallyArchived` | `bool`      | NOT NULL | Manual archive flag                       |

#### Common Stage Values

- `"MAP 1"`: Initial evaluation stage
- `"MAP 1.1"`: Basic odor profiling
- `"MAP 1.2 CP"`: Consumer Preference track, stage 2
- `"MAP 1.2 FF"`: Fine Fragrance track, stage 2
- `"MAP 1.3 CP"`: Consumer Preference track, stage 3
- `"MAP 2.1 CP"`: Consumer Preference track, advanced stage 1
- `"MAP 2.1 FF"`: Fine Fragrance track, advanced stage 1
- `"MAP 2.2 CP"`: Consumer Preference track, advanced stage 2
- `"MAP 2.2 FF"`: Fine Fragrance track, advanced stage 2
- `"MAP 3.0 FF"`: Final Fine Fragrance evaluation

#### .NET Model

```csharp
public class Assessment : BaseBoObject
{
    public string SessionName { get; set; }
    public DateTime DateTime { get; set; }
    public string Stage { get; set; }
    public int Status { get; set; }
    public string Region { get; set; }
    public string Segment { get; set; }
    public bool IsClosed { get; set; }
}
```

---

### 3. Map1_1Evaluation

**Purpose**: MAP 1.1 evaluation session - basic initial odor profiling with time-based descriptions (0h, 4h, 24h).

#### Table Structure

| Column                | Type                                    | Nullable | Description                   |
| --------------------- | --------------------------------------- | -------- | ----------------------------- |
| `Id`                  | `int`                                   | NOT NULL | Primary key, auto-generated   |
| `AssessmentId`        | `int`                                   | NOT NULL | Foreign key to Assessment     |
| `Participants`        | `string`                                | NULL     | List of participants          |
| `EvaluationDate`      | `DateTime?`                             | NULL     | Date of evaluation            |
| `EvaluationSiteId`    | `int`                                   | NOT NULL | Foreign key to EvaluationSite |
| `MoleculeEvaluations` | `ICollection<Map1_1MoleculeEvaluation>` | NULL     | Navigation property           |
| `CreatedAt`           | `DateTime?`                             | NULL     | Auto-set on insert            |
| `UpdatedAt`           | `DateTime?`                             | NULL     | Auto-updated on changes       |
| `CreatedBy`           | `string`                                | NULL     | User who created record       |
| `UpdatedBy`           | `string`                                | NULL     | User who last modified record |

#### .NET Model

```csharp
public class Map1_1Evaluation : BaseBoObject
{
    public int AssessmentId { get; set; }
    public virtual Assessment Assessment { get; set; }
    public string Participants { get; set; }
    public DateTime? EvaluationDate { get; set; }
    public int EvaluationSiteId { get; set; }
    public ICollection<Map1_1MoleculeEvaluation> MoleculeEvaluations { get; set; }
}
```

---

### 4. Map1_1MoleculeEvaluation

**Purpose**: Individual molecule evaluation within MAP 1.1 session with time-based odor descriptions and initial scoring.

#### Table Structure

| Column                       | Type        | Nullable | Description                               |
| ---------------------------- | ----------- | -------- | ----------------------------------------- |
| `Id`                         | `int`       | NOT NULL | Primary key, auto-generated               |
| `Map1_1EvaluationId`         | `int`       | NOT NULL | Foreign key to Map1_1Evaluation           |
| `MoleculeId`                 | `int`       | NOT NULL | Foreign key to Molecule                   |
| `SortOrder`                  | `int`       | NOT NULL | Display order in evaluation               |
| `GrDilutionSolventId`        | `int?`      | NULL     | FK to DilutionSolvent for GR sample       |
| `BenchmarkDilutionSolventId` | `int?`      | NULL     | FK to DilutionSolvent for benchmark       |
| `Odor0h`                     | `string`    | NULL     | Odor description at 0 hours (initial)     |
| `Odor4h`                     | `string`    | NULL     | Odor description after 4 hours            |
| `Odor24h`                    | `string`    | NULL     | Odor description after 24 hours           |
| `Benchmark`                  | `string`    | NULL     | Benchmark comparison notes                |
| `Comment`                    | `string`    | NULL     | General comments                          |
| `FFNextSteps`                | `string`    | NULL     | Next steps for Fine Fragrance track       |
| `CPNextSteps`                | `string`    | NULL     | Next steps for Consumer Preference track  |
| `ResultCP`                   | `int?`      | NULL     | Consumer Preference score (typically 1-5) |
| `ResultFF`                   | `int?`      | NULL     | Fine Fragrance score (typically 1-5)      |
| `CreatedAt`                  | `DateTime?` | NULL     | Auto-set on insert                        |
| `UpdatedAt`                  | `DateTime?` | NULL     | Auto-updated on changes                   |

#### .NET Model

```csharp
public class Map1_1MoleculeEvaluation : BaseBoObject
{
    public int Map1_1EvaluationId { get; set; }
    public virtual Map1_1Evaluation Map1_1Evaluation { get; set; }
    public int MoleculeId { get; set; }
    public virtual Molecule Molecule { get; set; }
    public int SortOrder { get; set; }
    public int? GrDilutionSolventId { get; set; }
    public int? BenchmarkDilutionSolventId { get; set; }
    public string Odor0h { get; set; }
    public string Odor4h { get; set; }
    public string Odor24h { get; set; }
    public string Benchmark { get; set; }
    public string Comment { get; set; }
    public string FFNextSteps { get; set; }
    public string CPNextSteps { get; set; }
    public int? ResultCP { get; set; }
    public int? ResultFF { get; set; }
}
```

---

### 5. Map1MoleculeEvaluation

**Purpose**: Detailed molecule evaluation with odor characterization using descriptor families and individual descriptors.

#### Table Structure

| Column                    | Type               | Nullable | Description                      |
| ------------------------- | ------------------ | -------- | -------------------------------- |
| `Id`                      | `int`              | NOT NULL | Primary key, auto-generated      |
| `MoleculeId`              | `int`              | NOT NULL | Foreign key to Molecule          |
| `Map1EvaluationId`        | `int`              | NOT NULL | Foreign key to Map1Evaluation    |
| `SortOrder`               | `int`              | NOT NULL | Display order                    |
| `Odor0h`                  | `string`           | NULL     | Odor description at 0 hours      |
| `Odor4h`                  | `string`           | NULL     | Odor description at 4 hours      |
| `Odor24`                  | `string`           | NULL     | Odor description at 24 hours     |
| `Benchmark`               | `string`           | NULL     | Benchmark comparison             |
| `Comment`                 | `string`           | NULL     | General comments                 |
| `ResultCP`                | `int?`             | NULL     | Consumer Preference result (1-5) |
| `ResultFF`                | `int?`             | NULL     | Fine Fragrance result (1-5)      |
| `Tenacity`                | `int?`             | NULL     | Odor persistence score (1-10)    |
| `Liking`                  | `int?`             | NULL     | Overall liking score (1-10)      |
| `Intensity`               | `int?`             | NULL     | Odor intensity score (1-10)      |
| `DilutionSolventId`       | `int?`             | NULL     | Foreign key to DilutionSolvent   |
| `Quantity`                | `decimal`          | NOT NULL | Quantity used in evaluation      |
| `PreparationInstructions` | `string`           | NULL     | How to prepare sample            |
| `OdorsDetails`            | `List<OdorDetail>` | NULL     | Navigation to odor descriptors   |

#### .NET Model

```csharp
public class Map1MoleculeEvaluation : BaseBoObject
{
    public int MoleculeId { get; set; }
    public virtual Molecule Molecule { get; set; }
    public int Map1EvaluationId { get; set; }
    public virtual Map1Evaluation Map1Evaluation { get; set; }
    public int SortOrder { get; set; }
    public string Odor0h { get; set; }
    public string Odor4h { get; set; }
    public string Odor24 { get; set; }
    public string Benchmark { get; set; }
    public string Comment { get; set; }
    public int? ResultCP { get; set; }
    public int? ResultFF { get; set; }
    public int? Tenacity { get; set; }
    public int? Liking { get; set; }
    public int? Intensity { get; set; }
    public int? DilutionSolventId { get; set; }
    public virtual DilutionSolvent DilutionSolvent { get; set; }
    public decimal Quantity { get; set; }
    public string PreparationInstructions { get; set; }
    public List<OdorDetail> OdorsDetails { get; set; }
}
```

---

### 6. OdorDetail

**Purpose**: Links molecule evaluations to specific odor descriptors with intensity scores.

#### Table Structure

| Column                     | Type  | Nullable | Description                           |
| -------------------------- | ----- | -------- | ------------------------------------- |
| `Id`                       | `int` | NOT NULL | Primary key, auto-generated           |
| `Map1MoleculeEvaluationID` | `int` | NOT NULL | Foreign key to Map1MoleculeEvaluation |
| `OdorFamilyId`             | `int` | NOT NULL | Foreign key to OdorFamily             |
| `OdorDescriptorId`         | `int` | NOT NULL | Foreign key to OdorDescriptor         |
| `Score`                    | `int` | NOT NULL | Intensity score (typically 1-10)      |

#### .NET Model

```csharp
public class OdorDetail : BaseBoObject
{
    public virtual Map1MoleculeEvaluation Map1MoleculeEvaluation { get; set; }
    public int Map1MoleculeEvaluationID { get; set; }
    public virtual OdorFamily OdorFamily { get; set; }
    public int OdorFamilyId { get; set; }
    public virtual OdorDescriptor OdorDescriptor { get; set; }
    public int OdorDescriptorId { get; set; }
    public int Score { get; set; }
}
```

---

### 7. OdorFamily

**Purpose**: Reference table for odor family classifications with UI display colors.

#### Table Structure

| Column            | Type                          | Nullable | Description                            |
| ----------------- | ----------------------------- | -------- | -------------------------------------- |
| `Id`              | `int`                         | NOT NULL | Primary key, auto-generated            |
| `Name`            | `string`                      | NULL     | Display name (e.g., "Fruity", "Woody") |
| `Color`           | `string`                      | NULL     | Hex color code for UI display          |
| `Code`            | `string`                      | NULL     | Unique code identifier                 |
| `OdorDescriptors` | `ICollection<OdorDescriptor>` | NULL     | Navigation property                    |

#### Common Odor Families

Based on industry standards, typical families include:

- **Ambergris**: Amber, woody-ambery notes
- **Aromatic-Herbal**: Camphoraceous, minty, piney, resinous notes
- **Citrus**: Lemon, orange, grapefruit, lime notes
- **Floral**: Rose, jasmine, lilac, violet notes
- **Fruity**: Apple, strawberry, peach, tropical fruit notes
- **Green**: Grassy, foliage, cucumber, vegetable notes
- **Marine**: Ocean, ozonic, aquatic notes
- **Musky-Animalic**: Musk, leather, indolic notes
- **Off-Odors**: Solvent, sulfury, fishy, medicinal notes
- **Spicy**: Cinnamon, clove, pepper notes
- **Sweet-Gourmand**: Vanilla, caramel, honey, nutty notes
- **Woody**: Cedarwood, sandalwood, patchouli, vetiver notes

#### .NET Model

```csharp
public class OdorFamily : BaseBoObject
{
    public string Name { get; set; }
    public string Color { get; set; }
    public string Code { get; set; }
    public ICollection<OdorDescriptor> OdorDescriptors { get; set; }
}
```

---

### 8. OdorDescriptor

**Purpose**: Reference table for specific odor descriptors, each belonging to a family.

#### Table Structure

| Column         | Type         | Nullable | Description                             |
| -------------- | ------------ | -------- | --------------------------------------- |
| `Id`           | `int`        | NOT NULL | Primary key, auto-generated             |
| `Name`         | `string`     | NULL     | Descriptor name (e.g., "Apple", "Rose") |
| `ProfileName`  | `string`     | NULL     | Profile display name                    |
| `Code`         | `string`     | NULL     | Unique code identifier                  |
| `OdorFamilyId` | `int`        | NOT NULL | Foreign key to OdorFamily               |
| `OdorFamily`   | `OdorFamily` | NULL     | Navigation property                     |

#### Sample Descriptors by Family

**Fruity Family**: Apple, Banana, Strawberry, Peach, Pear, Melon, Raspberry  
**Floral Family**: Rose, Jasmine, Violet, Lily of the Valley, Freesia  
**Woody Family**: Cedarwood, Sandalwood, Patchouli, Vetiver  
**Citrus Family**: Lemon, Orange, Grapefruit, Lime, Mandarin  
**Green Family**: Green Grassy, Cucumber, Foliage, Violet Leaves

#### .NET Model

```csharp
public class OdorDescriptor : BaseBoObject
{
    public string Name { get; set; }
    public string ProfileName { get; set; }
    public string Code { get; set; }
    public virtual OdorFamily OdorFamily { get; set; }
    public int OdorFamilyId { get; set; }
}
```

---

## MAP 1.2 Evaluation System

### 9. Map1_2CPEvaluation

**Purpose**: MAP 1.2 Consumer Preference evaluation - panel-based assessment with test conditions.

#### Table Structure

| Column                          | Type                                        | Nullable | Description                        |
| ------------------------------- | ------------------------------------------- | -------- | ---------------------------------- |
| `Id`                            | `int`                                       | NOT NULL | Primary key                        |
| `AssessmentId`                  | `int`                                       | NOT NULL | Foreign key to Assessment          |
| `BaseId`                        | `int`                                       | NOT NULL | Foreign key to Bases               |
| `PanelistId`                    | `int`                                       | NOT NULL | Foreign key to Panelist            |
| `TestConditionId`               | `int`                                       | NOT NULL | Foreign key to TestCondition       |
| `RegionId`                      | `int`                                       | NOT NULL | Foreign key to Region              |
| `EvaluationSiteId`              | `int`                                       | NOT NULL | Foreign key to EvaluationSite      |
| `Instructions`                  | `string`                                    | NULL     | Instructions for panelists         |
| `DefaultWashingConditionId`     | `int?`                                      | NULL     | FK to DefaultWashingCondition      |
| `Conclusion`                    | `string`                                    | NULL     | Evaluation conclusion              |
| `NextSteps`                     | `string`                                    | NULL     | Recommended next steps             |
| `IsBlinded`                     | `bool`                                      | NOT NULL | Whether evaluation is blind-tested |
| `Map1_2CPMoleculeEvaluations`   | `ICollection<Map1_2CPMoleculeEvaluation>`   | NULL     | Navigation property                |
| `Map1_2CPEvaluationTimePeriods` | `ICollection<Map1_2CPEvaluationTimePeriod>` | NULL     | Navigation property                |

#### .NET Model

```csharp
public class Map1_2CPEvaluation : BaseBoObject
{
    public int AssessmentId { get; set; }
    public virtual Assessment Assessment { get; set; }
    public int BaseId { get; set; }
    public int PanelistId { get; set; }
    public int TestConditionId { get; set; }
    public int RegionId { get; set; }
    public int EvaluationSiteId { get; set; }
    public string Instructions { get; set; }
    public int? DefaultWashingConditionId { get; set; }
    public virtual DefaultWashingCondition DefaultWashingCondition { get; set; }
    public string Conclusion { get; set; }
    public string NextSteps { get; set; }
    public bool IsBlinded { get; set; }
    public ICollection<Map1_2CPMoleculeEvaluation> Map1_2CPMoleculeEvaluations { get; set; }
    public ICollection<Map1_2CPEvaluationTimePeriod> Map1_2CPEvaluationTimePeriods { get; set; }
}
```

---

### 10. Map1_2CPMoleculeEvaluation

**Purpose**: Individual molecule being evaluated in MAP 1.2 CP session, including benchmarks.

#### Table Structure

| Column                 | Type      | Nullable | Description                                 |
| ---------------------- | --------- | -------- | ------------------------------------------- |
| `Id`                   | `int`     | NOT NULL | Primary key                                 |
| `Map1_2CPEvaluationId` | `int`     | NOT NULL | Foreign key to Map1_2CPEvaluation           |
| `MoleculeId`           | `int?`    | NULL     | Foreign key to Molecule (null if benchmark) |
| `Dilution`             | `decimal` | NOT NULL | Dilution percentage                         |
| `RefPlace`             | `string`  | NULL     | Reference location/position                 |
| `Comment`              | `string`  | NULL     | Comments                                    |
| `IsBenchmark`          | `bool`    | NOT NULL | Whether this is a benchmark sample          |
| `BenchmarkName`        | `string`  | NULL     | Name of benchmark if applicable             |
| `ResultCP`             | `int?`    | NULL     | Consumer Preference result                  |
| `SortOrder`            | `int`     | NOT NULL | Display order                               |

#### .NET Model

```csharp
public class Map1_2CPMoleculeEvaluation : BaseBoObject
{
    public int Map1_2CPEvaluationId { get; set; }
    public virtual Map1_2CPEvaluation Map1_2CPEvaluation { get; set; }
    public int? MoleculeId { get; set; }
    public virtual Molecule Molecule { get; set; }
    public decimal Dilution { get; set; }
    public string RefPlace { get; set; }
    public string Comment { get; set; }
    public bool IsBenchmark { get; set; }
    public string BenchmarkName { get; set; }
    public int? ResultCP { get; set; }
    public int SortOrder { get; set; }
}
```

---

### 11. Map1_2CPEvaluationTimePeriod

**Purpose**: Defines time periods for panel evaluations (e.g., "Initial", "After 2h", "After 24h").

#### Table Structure

| Column                 | Type        | Nullable | Description                       |
| ---------------------- | ----------- | -------- | --------------------------------- |
| `Id`                   | `int`       | NOT NULL | Primary key                       |
| `Map1_2CPEvaluationId` | `int`       | NOT NULL | Foreign key to Map1_2CPEvaluation |
| `Text`                 | `string`    | NULL     | Time period label                 |
| `Date`                 | `DateTime?` | NULL     | Actual date/time of this period   |

#### .NET Model

```csharp
public class Map1_2CPEvaluationTimePeriod : BaseBoObject
{
    public int Map1_2CPEvaluationId { get; set; }
    public virtual Map1_2CPEvaluation Map1_2CPEvaluation { get; set; }
    public string Text { get; set; }
    public DateTime? Date { get; set; }
}
```

---

## MAP 2.1 Evaluation System

### 12. Map2_1CPEvaluation

**Purpose**: MAP 2.1 Consumer Preference evaluation - advanced stage with perfume configurations.

#### Table Structure

| Column                          | Type                                        | Nullable | Description                        |
| ------------------------------- | ------------------------------------------- | -------- | ---------------------------------- |
| `Id`                            | `int`                                       | NOT NULL | Primary key                        |
| `AssessmentId`                  | `int`                                       | NOT NULL | Foreign key to Assessment          |
| `PanelistId`                    | `int`                                       | NOT NULL | Foreign key to Panelist            |
| `RegionId`                      | `int`                                       | NOT NULL | Foreign key to Region              |
| `EvaluationSiteId`              | `int`                                       | NOT NULL | Foreign key to EvaluationSite      |
| `NextSteps`                     | `string`                                    | NULL     | Recommended next steps             |
| `Conclusion`                    | `string`                                    | NULL     | Evaluation conclusion              |
| `IsBlinded`                     | `bool`                                      | NOT NULL | Whether evaluation is blind-tested |
| `Map2_1CPPerfumeConfigurations` | `ICollection<Map2_1CPPerfumeConfiguration>` | NULL     | Navigation property                |

#### .NET Model

```csharp
public class Map2_1CPEvaluation : BaseBoObject
{
    public int AssessmentId { get; set; }
    public virtual Assessment Assessment { get; set; }
    public int PanelistId { get; set; }
    public int RegionId { get; set; }
    public int EvaluationSiteId { get; set; }
    public string NextSteps { get; set; }
    public string Conclusion { get; set; }
    public bool IsBlinded { get; set; }
    public ICollection<Map2_1CPPerfumeConfiguration> Map2_1CPPerfumeConfigurations { get; set; }
}
```

---

### 13. Map2_1CPPerfumeConfiguration

**Purpose**: Represents a perfume base configuration within which molecules are tested.

#### Table Structure

| Column                          | Type                                        | Nullable | Description                       |
| ------------------------------- | ------------------------------------------- | -------- | --------------------------------- |
| `Id`                            | `int`                                       | NOT NULL | Primary key                       |
| `Map2_1CPEvaluationId`          | `int`                                       | NOT NULL | Foreign key to Map2_1CPEvaluation |
| `PerfumeName`                   | `string`                                    | NULL     | Name of perfume base              |
| `DosagePercentage`              | `string`                                    | NULL     | Dosage percentage                 |
| `Base`                          | `string`                                    | NULL     | Base formulation                  |
| `SortOrder`                     | `int`                                       | NOT NULL | Display order                     |
| `Map2_1CPEvaluationTimePeriods` | `ICollection<Map2_1CPEvaluationTimePeriod>` | NULL     | Navigation property               |
| `Map2_1CPMoleculeEvaluations`   | `ICollection<Map2_1CPMoleculeEvaluation>`   | NULL     | Navigation property               |
| `Map2_1CPTrialCompositions`     | `ICollection<Map2_1CPTrialComposition>`     | NULL     | Navigation property               |

#### .NET Model

```csharp
public class Map2_1CPPerfumeConfiguration : BaseBoObject
{
    public int Map2_1CPEvaluationId { get; set; }
    public virtual Map2_1CPEvaluation Map2_1CPEvaluation { get; set; }
    public string PerfumeName { get; set; }
    public string DosagePercentage { get; set; }
    public string Base { get; set; }
    public int SortOrder { get; set; }
    public ICollection<Map2_1CPEvaluationTimePeriod> Map2_1CPEvaluationTimePeriods { get; set; }
    public ICollection<Map2_1CPMoleculeEvaluation> Map2_1CPMoleculeEvaluations { get; set; }
    public ICollection<Map2_1CPTrialComposition> Map2_1CPTrialCompositions { get; set; }
}
```

---

### 14. Map2_1CPMoleculeEvaluation

**Purpose**: Individual molecule tested within a perfume configuration.

#### Table Structure

| Column                           | Type                                          | Nullable | Description                        |
| -------------------------------- | --------------------------------------------- | -------- | ---------------------------------- |
| `Id`                             | `int`                                         | NOT NULL | Primary key                        |
| `Map2_1CPPerfumeConfigurationId` | `int`                                         | NOT NULL | FK to Map2_1CPPerfumeConfiguration |
| `MoleculeId`                     | `int?`                                        | NULL     | Foreign key to Molecule            |
| `IsBenchmark`                    | `bool`                                        | NOT NULL | Whether this is a benchmark        |
| `BenchmarkName`                  | `string`                                      | NULL     | Benchmark name if applicable       |
| `SortOrder`                      | `int`                                         | NOT NULL | Display order                      |
| `Comment`                        | `string`                                      | NULL     | Comments                           |
| `ResultCP`                       | `int?`                                        | NULL     | Consumer Preference result         |
| `Map2_1CPMoleculeEvaluationData` | `ICollection<Map2_1CPMoleculeEvaluationData>` | NULL     | Navigation property                |

#### .NET Model

```csharp
public class Map2_1CPMoleculeEvaluation : BaseBoObject
{
    public int Map2_1CPPerfumeConfigurationId { get; set; }
    public virtual Map2_1CPPerfumeConfiguration Map2_1CPPerfumeConfiguration { get; set; }
    public int? MoleculeId { get; set; }
    public virtual Molecule Molecule { get; set; }
    public bool IsBenchmark { get; set; }
    public string BenchmarkName { get; set; }
    public int SortOrder { get; set; }
    public ICollection<Map2_1CPMoleculeEvaluationData> Map2_1CPMoleculeEvaluationData { get; set; }
    public string Comment { get; set; }
    public int? ResultCP { get; set; }
}
```

---

### 15. Map2_1CPMoleculeEvaluationData

**Purpose**: Stores evaluation data for a molecule at specific time periods within trials.

#### Table Structure

| Column                           | Type     | Nullable | Description                        |
| -------------------------------- | -------- | -------- | ---------------------------------- |
| `Id`                             | `int`    | NOT NULL | Primary key                        |
| `Map2_1CPMoleculeEvaluationId`   | `int`    | NOT NULL | FK to Map2_1CPMoleculeEvaluation   |
| `Map2_1CPEvaluationTimePeriodId` | `int`    | NOT NULL | FK to Map2_1CPEvaluationTimePeriod |
| `TrialCompositionId`             | `int?`   | NULL     | Reference to trial composition     |
| `TrialComposition`               | `string` | NULL     | Copy of trial composition text     |
| `TrialDosage`                    | `string` | NULL     | Copy of trial dosage               |
| `Comment`                        | `string` | NULL     | Comments for this data point       |
| `Result`                         | `int?`   | NULL     | Evaluation result/score            |
| `RefPlace`                       | `string` | NULL     | Reference location                 |
| `Concentration`                  | `string` | NULL     | Concentration used                 |
| `Notes`                          | `string` | NULL     | Additional notes                   |

#### .NET Model

```csharp
public class Map2_1CPMoleculeEvaluationData : BaseBoObject
{
    public int Map2_1CPMoleculeEvaluationId { get; set; }
    public virtual Map2_1CPMoleculeEvaluation Map2_1CPMoleculeEvaluation { get; set; }
    public int Map2_1CPEvaluationTimePeriodId { get; set; }
    public virtual Map2_1CPEvaluationTimePeriod Map2_1CPEvaluationTimePeriod { get; set; }
    public int? TrialCompositionId { get; set; }
    public string TrialComposition { get; set; }
    public string TrialDosage { get; set; }
    public string Comment { get; set; }
    public int? Result { get; set; }
    public string RefPlace { get; set; }
    public string Concentration { get; set; }
    public string Notes { get; set; }
}
```

---

### 16. Map2_1CPTrialComposition

**Purpose**: Defines trial compositions (formulation variants) for perfume configurations.

#### Table Structure

| Column                           | Type     | Nullable | Description                        |
| -------------------------------- | -------- | -------- | ---------------------------------- |
| `Id`                             | `int`    | NOT NULL | Primary key                        |
| `Map2_1CPPerfumeConfigurationId` | `int`    | NOT NULL | FK to Map2_1CPPerfumeConfiguration |
| `TrialDosage`                    | `string` | NULL     | Dosage for this trial              |
| `TrialComposition`               | `string` | NULL     | Composition description            |
| `RefPlace`                       | `string` | NULL     | Reference location                 |

#### .NET Model

```csharp
public class Map2_1CPTrialComposition : BaseBoObject
{
    public int Map2_1CPPerfumeConfigurationId { get; set; }
    public virtual Map2_1CPPerfumeConfiguration Map2_1CPPerfumeConfiguration { get; set; }
    public string TrialDosage { get; set; }
    public string TrialComposition { get; set; }
    public string RefPlace { get; set; }
}
```

---

### 17. Map2_1CPEvaluationTimePeriod

**Purpose**: Time periods for MAP 2.1 CP evaluations with custom titles.

#### Table Structure

| Column                           | Type        | Nullable | Description                        |
| -------------------------------- | ----------- | -------- | ---------------------------------- |
| `Id`                             | `int`       | NOT NULL | Primary key                        |
| `Map2_1CPPerfumeConfigurationId` | `int`       | NOT NULL | FK to Map2_1CPPerfumeConfiguration |
| `Text`                           | `string`    | NULL     | Time period label                  |
| `Date`                           | `DateTime?` | NULL     | Actual date/time                   |
| `CustomTitle`                    | `string`    | NULL     | Custom display title               |

#### .NET Model

```csharp
public class Map2_1CPEvaluationTimePeriod : BaseBoObject
{
    public int Map2_1CPPerfumeConfigurationId { get; set; }
    public virtual Map2_1CPPerfumeConfiguration Map2_1CPPerfumeConfiguration { get; set; }
    public string Text { get; set; }
    public DateTime? Date { get; set; }
    public string CustomTitle { get; set; }
}
```

---

## Unified Evaluation Session System

### 18. EvaluationSession

**Purpose**: Unified container for panel-based evaluation sessions across multiple session types.

#### Table Structure

| Column                 | Type                           | Nullable | Description                                            |
| ---------------------- | ------------------------------ | -------- | ------------------------------------------------------ |
| `Id`                   | `int`                          | NOT NULL | Primary key                                            |
| `SessionType`          | `SessionType`                  | NOT NULL | Enum: Map1_2CP, Map1_3CP, Map2_1CP, Map2_2CP, Map2_2FF |
| `EvaluationId`         | `int`                          | NOT NULL | Generic evaluation ID                                  |
| `StartDate`            | `DateTime`                     | NOT NULL | Session start date                                     |
| `IsBlinded`            | `bool`                         | NOT NULL | Whether session is blind-tested                        |
| `Map1_2CPEvaluationId` | `int?`                         | NULL     | FK to Map1_2CPEvaluation (when applicable)             |
| `Map1_3CPEvaluationId` | `int?`                         | NULL     | FK to Map1_3CPEvaluation (when applicable)             |
| `Map2_1CPEvaluationId` | `int?`                         | NULL     | FK to Map2_1CPEvaluation (when applicable)             |
| `Map2_2CPEvaluationId` | `int?`                         | NULL     | FK to Map2_2CPEvaluation (when applicable)             |
| `Map2_2FFEvaluationId` | `int?`                         | NULL     | FK to Map2_2FFEvaluation (when applicable)             |
| `SmellEvaluations`     | `ICollection<SmellEvaluation>` | NULL     | Navigation property                                    |

**Note**: Only one of the evaluation FK fields will be populated based on `SessionType`.

#### SessionType Enum

```csharp
public enum SessionType
{
    Map1_2CP = 1,
    Map1_3CP = 2,
    Map2_1CP = 3,
    Map2_2CP = 4,
    Map2_2FF = 5
}
```

#### .NET Model

```csharp
public class EvaluationSession : BaseBoObject
{
    public SessionType SessionType { get; set; }
    public int EvaluationId { get; set; }
    public DateTime StartDate { get; set; }
    public bool IsBlinded { get; set; }

    // Type-specific foreign keys (only one will be set)
    public int? Map1_2CPEvaluationId { get; set; }
    public int? Map1_3CPEvaluationId { get; set; }
    public int? Map2_1CPEvaluationId { get; set; }
    public int? Map2_2CPEvaluationId { get; set; }
    public int? Map2_2FFEvaluationId { get; set; }

    // Navigation properties
    public virtual Map1_2CPEvaluation Map1_2CPEvaluation { get; set; }
    public virtual Map1_3CPEvaluation Map1_3CPEvaluation { get; set; }
    public virtual Map2_1CPEvaluation Map2_1CPEvaluation { get; set; }
    public virtual Map2_2CPEvaluation Map2_2CPEvaluation { get; set; }
    public virtual Map2_2FFEvaluation Map2_2FFEvaluation { get; set; }

    public virtual ICollection<SmellEvaluation> SmellEvaluations { get; set; }
}
```

---

### 19. SmellEvaluation

**Purpose**: Individual smell evaluation by a user during an evaluation session. Stores sensory data per time period and molecule.

#### Table Structure

| Column                | Type          | Nullable | Description                            |
| --------------------- | ------------- | -------- | -------------------------------------- |
| `Id`                  | `int`         | NOT NULL | Primary key                            |
| `SessionType`         | `SessionType` | NOT NULL | Type discriminator                     |
| `UserId`              | `string`      | NULL     | User/panelist who performed evaluation |
| `EvaluationSessionId` | `int`         | NOT NULL | Foreign key to EvaluationSession       |
| `Intensity`           | `decimal`     | NOT NULL | Intensity score                        |
| `Conclusion`          | `string`      | NULL     | Evaluation conclusion/notes            |

**Type-specific foreign keys** (nullable, based on `SessionType`):

- `Map1_2CPEvaluationTimePeriodId` / `Map1_2CPMoleculeEvaluationId`
- `Map1_3CPEvaluationTypeId` / `Map1_3CPMoleculeEvaluationId`
- `Map2_1CPEvaluationTimePeriodId` / `Map2_1CPMoleculeEvaluationId` / `Map2_1CPTrialCompositionId`
- `Map2_2CPEvaluationTimePeriodId` / `Map2_2CPMoleculeEvaluationId` / `Map2_2CPTrialCompositionId`
- `Map2_2FFEvaluationTimePeriodId` / `Map2_2FFMoleculeEvaluationId` / `Map2_2FFTrialCompositionId`

#### .NET Model

```csharp
public class SmellEvaluation : BaseBoObject
{
    public SessionType SessionType { get; set; }
    public string UserId { get; set; }
    public int EvaluationSessionId { get; set; }
    public virtual EvaluationSession EvaluationSession { get; set; }
    public decimal Intensity { get; set; }
    public string Conclusion { get; set; }

    // Type-specific foreign keys (only relevant ones populated based on SessionType)
    public int? Map1_2CPEvaluationTimePeriodId { get; set; }
    public virtual Map1_2CPEvaluationTimePeriod Map1_2CPEvaluationTimePeriod { get; set; }
    public int? Map1_2CPMoleculeEvaluationId { get; set; }
    public virtual Map1_2CPMoleculeEvaluation Map1_2CPMoleculeEvaluation { get; set; }
    // ... (additional type-specific properties)
}
```

---

### 20. SmellEvaluationSummary

**Purpose**: Aggregated summary data for evaluation sessions, including pre-calculated chart data and excluded users.

#### Table Structure

| Column                      | Type          | Nullable | Description                                       |
| --------------------------- | ------------- | -------- | ------------------------------------------------- |
| `Id`                        | `int`         | NOT NULL | Primary key                                       |
| `EvaluationSessionId`       | `int`         | NOT NULL | Foreign key to EvaluationSession                  |
| `ExcludedUserIds`           | `string`      | NOT NULL | JSON array of excluded user IDs (default "[]")    |
| `SummaryData`               | `string`      | NOT NULL | JSON object with pre-calculated chart data ("{}") |
| `IsFinal`                   | `bool`        | NOT NULL | True when evaluation is closed (default false)    |
| `Name`                      | `string`      | NULL     | Summary name                                      |
| `SessionType`               | `SessionType` | NOT NULL | Session type discriminator                        |
| `TimePeriodSummaryComments` | `string`      | NOT NULL | JSON object with comments by time period ("{}")   |

#### .NET Model

```csharp
public class SmellEvaluationSummary : BaseBoObject
{
    public int EvaluationSessionId { get; set; }
    public virtual EvaluationSession EvaluationSession { get; set; }
    public string ExcludedUserIds { get; set; } = "[]"; // JSON array
    public string SummaryData { get; set; } = "{}"; // JSON object
    public bool IsFinal { get; set; } = false;
    public string Name { get; set; }
    public SessionType SessionType { get; set; }
    public string TimePeriodSummaryComments { get; set; } = "{}"; // JSON object
}
```

---

## Supporting Reference Tables

### 21. Team

**Purpose**: Organizational teams for grouping users.

#### Table Structure

| Column        | Type                             | Nullable | Description         |
| ------------- | -------------------------------- | -------- | ------------------- |
| `Id`          | `int`                            | NOT NULL | Primary key         |
| `Name`        | `string`                         | NULL     | Team name           |
| `Description` | `string`                         | NULL     | Team description    |
| `Users`       | `ICollection<BusinessLogicUser>` | NULL     | Navigation property |

#### .NET Model

```csharp
public class Team : BaseBoObject
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<BusinessLogicUser> Users { get; set; }
}
```

---

### 22. Permission

**Purpose**: User permissions/roles.

#### Table Structure

| Column               | Type                             | Nullable | Description                           |
| -------------------- | -------------------------------- | -------- | ------------------------------------- |
| `Id`                 | `int`                            | NOT NULL | Primary key                           |
| `PermissionCode`     | `PermissionCode`                 | NOT NULL | Enum: PerfumeryLab, Chemist, Panelist |
| `Name`               | `string`                         | NULL     | Permission name                       |
| `BusinessLogicUsers` | `ICollection<BusinessLogicUser>` | NULL     | Navigation property                   |

#### PermissionCode Enum

```csharp
public enum PermissionCode
{
    PerfumeryLab = 1,
    Chemist = 2,
    Panelist = 3
}
```

#### .NET Model

```csharp
public class Permission : BaseBoObject
{
    public PermissionCode PermissionCode { get; set; }
    public string Name { get; set; }
    public ICollection<BusinessLogicUser> BusinessLogicUsers { get; set; }
}
```

---

### 23. BusinessLogicUser

**Purpose**: Extended user information linking to Teams and Permissions.

#### Table Structure

| Column         | Type     | Nullable | Description                        |
| -------------- | -------- | -------- | ---------------------------------- |
| `Id`           | `int`    | NOT NULL | Primary key                        |
| `PermissionId` | `int?`   | NULL     | Foreign key to Permission          |
| `TeamId`       | `int?`   | NULL     | Foreign key to Team                |
| `Fullname`     | `string` | NULL     | Full name (not mapped to database) |

#### .NET Model

```csharp
public class BusinessLogicUser : BaseBoObject
{
    public int? PermissionId { get; set; }
    public Permission Permission { get; set; }
    public int? TeamId { get; set; }
    public Team Team { get; set; }

    [NotMapped]
    public string Fullname { get; set; }
}
```

---

### 24. Panelist

**Purpose**: Information about panel members who evaluate molecules.

#### .NET Model

```csharp
public class Panelist : BaseBoObject
{
    public string Name { get; set; }
    public string Participants { get; set; }
}
```

---

### 25. EvaluationSite

**Purpose**: Physical locations where evaluations are conducted.

#### .NET Model

```csharp
public class EvaluationSite : BaseBoObject
{
    public string Name { get; set; }
}
```

---

### 26. Region

**Purpose**: Geographic regions for evaluation classification.

#### .NET Model

```csharp
public class Region : BaseBoObject
{
    public string Name { get; set; }
}
```

---

### 27. Country

**Purpose**: Country reference data.

#### .NET Model

```csharp
public class Country : BaseBoObject
{
    public string Name { get; set; }
    public string Code { get; set; } // ISO country code
}
```

---

### 28. DilutionSolvent

**Purpose**: Reference table for solvents used to dilute molecules.

#### .NET Model

```csharp
public class DilutionSolvent : BaseBoObject
{
    public string Name { get; set; }
    public int Order { get; set; }
}
```

---

### 29. TestCondition

**Purpose**: Reference table for test conditions (e.g., washing conditions, application methods).

#### .NET Model

```csharp
public class TestCondition : BaseBoObject
{
    public string Name { get; set; }
}
```

---

### 30. Bases

**Purpose**: Reference table for base formulations used in consumer preference testing.

#### .NET Model

```csharp
public class Bases : BaseBoObject
{
    public string Name { get; set; }
}
```

---

### 31. DefaultWashingCondition

**Purpose**: Reference table for default washing/testing conditions.

#### .NET Model

```csharp
public class DefaultWashingCondition : BaseBoObject
{
    public string Name { get; set; }
    public string Description { get; set; }
}
```

---

### 32. SessionStatus

**Purpose**: Reference table for session status values.

#### .NET Model

```csharp
public class SessionStatus : BaseBoObject
{
    public string Name { get; set; }
}
```

---

### 33. MoleculeTag

**Purpose**: Tags for categorizing molecules with color-coded labels.

#### .NET Model

```csharp
public class MoleculeTag : BaseBoObject
{
    public string Name { get; set; }
    public string Color { get; set; }
}
```

---

### 34. Compound

**Purpose**: Extended compound information with tags and structure data.

#### .NET Model

```csharp
public class Compound : BaseBoObject
{
    public string RegNumber { get; set; }
    public int Status { get; set; }
    public string Tags { get; set; }
    public string Structure { get; set; }
}
```

---

## BaseBoObject - Audit Trail Foundation

All domain entities inherit from `BaseBoObject`, which provides:

```csharp
public abstract class BaseBoObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [StringLength(50)]
    public string? CreatedBy { get; set; }

    [StringLength(50)]
    public string? UpdatedBy { get; set; }

    public bool IsArchived { get; set; }
    public bool IsManuallyArchived { get; set; }
}
```

### Audit Trail Behavior

The `Map2DbContext.SaveChanges()` method automatically:

1. Sets `CreatedAt` and `CreatedBy` on insert
2. Sets `UpdatedAt` and `UpdatedBy` on update
3. Logs deletions to `LogAction` table with JSON serialization of deleted object

---

## Key Business Concepts

### Evaluation Workflow Stages

1. **MAP 1.1**: Initial basic evaluation

   - Time-based odor descriptions (0h, 4h, 24h)
   - Initial CP/FF scoring
   - Next steps determination

2. **MAP 1 (Legacy)**: Detailed characterization

   - Odor descriptor profiling with families
   - Intensity, tenacity, liking scores
   - Benchmark comparisons

3. **MAP 1.2 CP**: Consumer Preference panel evaluation

   - Test conditions and bases
   - Time-period assessments
   - Multiple panelists

4. **MAP 1.2 FF**: Fine Fragrance panel evaluation

   - Similar structure to 1.2 CP but fragrance-focused

5. **MAP 1.3 CP**: Advanced Consumer Preference

   - Evaluation types instead of time periods
   - More complex trial structures

6. **MAP 2.1 CP/FF**: Multi-perfume configuration testing

   - Multiple perfume bases per evaluation
   - Trial compositions with dosage variations
   - Cross-matrix evaluations (molecules × perfumes × time periods × trials)

7. **MAP 2.2 CP/FF**: Advanced multi-perfume testing

   - Similar to 2.1 but with additional complexity
   - Includes "Minimap" sub-evaluations for quick assessments

8. **MAP 3.0 FF**: Final Fine Fragrance evaluation
   - Country-specific configurations
   - Regional preferences

### Unified Session Architecture

The system uses a unified architecture for managing panel-based evaluations:

**EvaluationSession** → Contains session metadata  
 ↓  
**SmellEvaluation** → Individual panelist smell assessments  
 ↓  
**SmellEvaluationSummary** → Aggregated results and charts

This architecture:

- Supports multiple session types through discriminator enum
- Uses type-specific foreign keys (only relevant ones populated)
- Enables consistent summary calculation across different evaluation types
- Stores pre-calculated chart data as JSON for performance

### Perfume Configuration Pattern (MAP 2.x)

For MAP 2.1 and 2.2 evaluations:

```
Evaluation
  └── PerfumeConfiguration(s)
       ├── Time Periods
       ├── Molecule Evaluations
       │    └── Molecule Evaluation Data (time × trial matrix)
       └── Trial Compositions
```

This pattern allows testing multiple molecules in multiple perfume bases with multiple trial variations over multiple time periods.

---

## Database Relationships Summary

### One-to-Many Relationships

```
Assessment → Map1_1Evaluation
Assessment → Map1_2CPEvaluation
Assessment → Map2_1CPEvaluation
Assessment → Map2_2CPEvaluation
Assessment → Map2_2FFEvaluation
Assessment → Map3_0FFEvaluation

Map1_1Evaluation → Map1_1MoleculeEvaluation
Map1Evaluation → Map1MoleculeEvaluation
Map1MoleculeEvaluation → OdorDetail

OdorFamily → OdorDescriptor
OdorFamily → OdorDetail
OdorDescriptor → OdorDetail

Map1_2CPEvaluation → Map1_2CPMoleculeEvaluation
Map1_2CPEvaluation → Map1_2CPEvaluationTimePeriod

Map2_1CPEvaluation → Map2_1CPPerfumeConfiguration
Map2_1CPPerfumeConfiguration → Map2_1CPMoleculeEvaluation
Map2_1CPPerfumeConfiguration → Map2_1CPEvaluationTimePeriod
Map2_1CPPerfumeConfiguration → Map2_1CPTrialComposition
Map2_1CPMoleculeEvaluation → Map2_1CPMoleculeEvaluationData

EvaluationSession → SmellEvaluation
EvaluationSession → SmellEvaluationSummary (1:1)

Team → BusinessLogicUser
Permission → BusinessLogicUser
```

### Foreign Key Behavior

- **All foreign keys**: `DeleteBehavior.Restrict` (set in `OnModelCreating`)
- This prevents accidental cascading deletes
- Explicit deletion logic required in application code

---

## .NET Entity Framework Configuration

### Connection String Format (PostgreSQL)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=map2_db;Username=map_adm;Password=yourpassword"
  }
}
```

### DbContext Configuration

```csharp
public class Map2DbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Set default schema
        builder.HasDefaultSchema("map_adm");

        // Set all foreign keys to Restrict delete behavior
        var cascadeFKs = builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys());

        foreach (var fk in cascadeFKs)
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        base.OnModelCreating(builder);
    }
}
```

---

## API Implementation Recommendations

### Controller Structure

```csharp
[ApiController]
[Route("api/[controller]")]
public class MoleculesController : ControllerBase
{
    // GET /api/molecules
    // GET /api/molecules/{id}
    // GET /api/molecules/by-gr/{grNumber}
    // POST /api/molecules
    // PUT /api/molecules/{id}
    // DELETE /api/molecules/{id}
}

[ApiController]
[Route("api/[controller]")]
public class AssessmentsController : ControllerBase
{
    // GET /api/assessments
    // GET /api/assessments/{id}
    // GET /api/assessments/by-stage/{stage}
    // POST /api/assessments
    // PUT /api/assessments/{id}
}

[ApiController]
[Route("api/map1-1/[controller]")]
public class EvaluationsController : ControllerBase
{
    // GET /api/map1-1/evaluations
    // GET /api/map1-1/evaluations/{id}
    // POST /api/map1-1/evaluations
    // POST /api/map1-1/evaluations/{id}/molecules
}

[ApiController]
[Route("api/map2-1-cp/[controller]")]
public class EvaluationsController : ControllerBase
{
    // Nested routes for complex structure
    // POST /api/map2-1-cp/evaluations/{id}/perfume-configurations
    // POST /api/map2-1-cp/evaluations/{evalId}/perfumes/{perfumeId}/molecules
}

[ApiController]
[Route("api/[controller]")]
public class OdorProfilesController : ControllerBase
{
    // GET /api/odorprofiles/families
    // GET /api/odorprofiles/descriptors
    // GET /api/odorprofiles/descriptors/by-family/{familyId}
}

[ApiController]
[Route("api/[controller]")]
public class EvaluationSessionsController : ControllerBase
{
    // POST /api/evaluationsessions
    // POST /api/evaluationsessions/{id}/smell-evaluations
    // GET /api/evaluationsessions/{id}/summary
    // PUT /api/evaluationsessions/{id}/summary
}
```

### Key Implementation Patterns

1. **Eager Loading**: Use `.Include()` for related entities

   ```csharp
   var evaluation = await _context.Map1_1Evaluations
       .Include(e => e.Assessment)
       .Include(e => e.MoleculeEvaluations)
           .ThenInclude(me => me.Molecule)
       .FirstOrDefaultAsync(e => e.Id == id);
   ```

2. **Projection for Performance**: Use DTOs for large result sets

   ```csharp
   var molecules = await _context.Molecules
       .Where(m => !m.IsArchived)
       .Select(m => new MoleculeListDto
       {
           Id = m.Id,
           GrNumber = m.GrNumber,
           Status = m.Status
       })
       .ToListAsync();
   ```

3. **Transaction Management**: Use transactions for complex operations

   ```csharp
   using var transaction = await _context.Database.BeginTransactionAsync();
   try
   {
       // Multiple operations
       await _context.SaveChanges(userName);
       await transaction.CommitAsync();
   }
   catch
   {
       await transaction.RollbackAsync();
       throw;
   }
   ```

4. **Soft Delete**: Use `IsArchived` flag

   ```csharp
   molecule.IsArchived = true;
   await _context.SaveChanges(userName);
   ```

5. **Custom SaveChanges**: Always use `SaveChanges(string userName)` overload to ensure audit trail

---

## Sample Queries for Common Use Cases

### 1. Get Molecule with All Evaluations

```csharp
var molecule = await _context.Molecules
    .Include(m => m.Map1_1MoleculeEvaluations)
        .ThenInclude(me => me.Map1_1Evaluation)
            .ThenInclude(e => e.Assessment)
    .FirstOrDefaultAsync(m => m.GrNumber == grNumber);
```

### 2. Get Assessment with All Evaluation Types

```csharp
var assessment = await _context.Assessments
    .Include(a => a.Map1_1Evaluations)
    .Include(a => a.Map1_2CPEvaluations)
    .Include(a => a.Map2_1CPEvaluations)
    .FirstOrDefaultAsync(a => a.Id == id);
```

### 3. Get MAP 2.1 CP Evaluation with Full Hierarchy

```csharp
var evaluation = await _context.Map2_1CPEvaluations
    .Include(e => e.Assessment)
    .Include(e => e.Map2_1CPPerfumeConfigurations)
        .ThenInclude(pc => pc.Map2_1CPMoleculeEvaluations)
            .ThenInclude(me => me.Molecule)
    .Include(e => e.Map2_1CPPerfumeConfigurations)
        .ThenInclude(pc => pc.Map2_1CPEvaluationTimePeriods)
    .Include(e => e.Map2_1CPPerfumeConfigurations)
        .ThenInclude(pc => pc.Map2_1CPTrialCompositions)
    .FirstOrDefaultAsync(e => e.Id == id);
```

### 4. Get Odor Descriptors by Family

```csharp
var descriptors = await _context.OdorDescriptors
    .Include(d => d.OdorFamily)
    .Where(d => d.OdorFamily.Code == "FRUITY_FAMILY")
    .OrderBy(d => d.Name)
    .ToListAsync();
```

### 5. Get Evaluation Sessions with Smell Evaluations

```csharp
var session = await _context.EvaluationSessions
    .Include(es => es.SmellEvaluations)
    .Include(es => es.SmellEvaluationSummaries)
    .Where(es => es.SessionType == SessionType.Map2_1CP)
    .FirstOrDefaultAsync(es => es.Id == sessionId);
```

### 6. Search Molecules by Status

```csharp
var molecules = await _context.Molecules
    .Where(m => m.Status == MoleculeStatus.Map1 && !m.IsArchived)
    .OrderByDescending(m => m.CreatedAt)
    .ToListAsync();
```

---

## Migration and Deployment

### Initial Setup

1. **Create Database**

   ```bash
   createdb -U postgres map2_db
   ```

2. **Run Migrations**

   ```bash
   cd MAP2.DB
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

3. **Seed Reference Data**
   - OdorFamilies and OdorDescriptors
   - EvaluationSites
   - Regions
   - TestConditions
   - Bases
   - DilutionSolvents
   - Permissions

### Database Versioning

The project uses database patch folders (`db/patch_0.0.0.X/`) for version-specific SQL scripts.

Apply patches in order:

```bash
psql -U map_adm -d map2_db -f db/patch_0.0.0.1/env/script.sql
psql -U map_adm -d map2_db -f db/patch_0.0.0.2/env/script.sql
# etc.
```

---

## Performance Considerations

1. **Indexes**: Entity Framework will create indexes for:

   - Primary keys (automatic)
   - Foreign keys (automatic)
   - Consider adding indexes on frequently queried columns:
     - `Molecule.GrNumber`
     - `Molecule.RegNumber`
     - `Assessment.Stage`
     - `Assessment.DateTime`

2. **Pagination**: Always paginate large result sets

   ```csharp
   var results = await _context.Molecules
       .OrderBy(m => m.Id)
       .Skip((page - 1) * pageSize)
       .Take(pageSize)
       .ToListAsync();
   ```

3. **Lazy Loading**: Disabled by default (use explicit `.Include()`)

4. **Batch Operations**: Use `AddRange()`, `UpdateRange()`, `RemoveRange()` for bulk operations

5. **JSON Fields**: `SmellEvaluationSummary` stores pre-calculated JSON to avoid expensive aggregations on read

---

## Security Considerations

1. **Permission-based Access**: Use `PermissionCode` enum to control access
2. **Audit Trail**: All changes tracked via `CreatedBy`, `UpdatedBy`, `CreatedAt`, `UpdatedAt`
3. **Soft Deletes**: Use `IsArchived` flag to preserve data integrity
4. **Cascade Prevention**: All FK deletes restricted to prevent accidental data loss

---

## Data Volume Estimates

Based on typical usage patterns:

- **Molecules**: 1,000 - 10,000+ records
- **Assessments**: 100 - 1,000+ records
- **Evaluations** (all types): 500 - 5,000+ records
- **Molecule Evaluations** (all types): 5,000 - 50,000+ records
- **Odor Families**: ~12 records (relatively static)
- **Odor Descriptors**: ~100 records (relatively static)
- **Odor Details**: 10,000 - 100,000+ records
- **Smell Evaluations**: 10,000 - 100,000+ records (high volume)
- **Users/Teams/Permissions**: 10 - 100 records

---

## Change Log & Versioning

- **Documentation Version**: 1.0
- **Generated**: October 29, 2025
- **Source System**: MAP2 (Molecule Assessment Program 2)
- **Database**: PostgreSQL
- **Schema**: `map_adm`
- **ORM**: Entity Framework Core

---

## Contact & Support

For questions about this database structure or integration support, contact the MAP2 development team or system administrator.
