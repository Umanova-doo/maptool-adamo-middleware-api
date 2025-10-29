# MAP (Molecule Assessment Program) Database Documentation

## Overview

This database is designed for a **Molecule Assessment Program (MAP)** system that manages odor evaluations, characterizations, and assessment sessions for chemical compounds. The system appears to be used by a perfumery or fragrance company (Givaudan - indicated by schema name `GIV_MAP`).

### Primary Business Functions

- Track initial molecule evaluations with odor descriptions at different time intervals
- Manage assessment sessions across different stages (MAP 0, MAP 1, MAP 2, MAP 3, ISC, FIB, FIM, CARDEX, RPMC)
- Record detailed odor characterization profiles with multiple descriptor families
- Store evaluation results from panel sessions
- Maintain reference data for odor families and descriptors
- Track molecules that are deliberately ignored in submissions

---

## Entity Relationship Overview

```
MAP_ODOR_FAMILY (1) ─────< (N) MAP_ODOR_DESCRIPTOR

MAP_SESSION (1) ─────< (N) MAP_RESULT
     │                           │
     │                           │ (references GR_NUMBER)
     └──────< MAP1_SESSION_LINK >──────┘
          (CP ↔ FF session links)

MAP_INITIAL (1:1 unique GR_NUMBER)
     │
     └─── (shares GR_NUMBER with) ──> ODOR_CHARACTERIZATION

SUBMITTING_IGNORED_MOLECULES (standalone tracking)
```

---

## Core Entities

### 1. MAP_INITIAL

**Purpose**: Stores initial evaluation data for molecules including preliminary odor assessments at different time intervals.

#### Table Structure

| Column               | Type             | Nullable | Description                                                       |
| -------------------- | ---------------- | -------- | ----------------------------------------------------------------- |
| `MAP_INITIAL_ID`     | `NUMBER(38,0)`   | NOT NULL | Primary key, auto-generated via sequence                          |
| `GR_NUMBER`          | `VARCHAR2(14)`   | NOT NULL | Unique molecule identifier (format: GR-YY-NNNNN-B or SL-NNNNNN-B) |
| `EVALUATION_DATE`    | `DATE`           | NULL     | Date of evaluation                                                |
| `CHEMIST`            | `VARCHAR2(50)`   | NULL     | Chemist who performed evaluation                                  |
| `ASSESSOR`           | `VARCHAR2(255)`  | NULL     | Person(s) who assessed the molecule                               |
| `DILUTION`           | `VARCHAR2(30)`   | NULL     | Dilution used (e.g., "10% in DPG")                                |
| `EVALUATION_SITE`    | `VARCHAR2(2)`    | NULL     | Site code where evaluation occurred (e.g., "ZH")                  |
| `ODOR0H`             | `VARCHAR2(500)`  | NULL     | Odor description at 0 hours (initial)                             |
| `ODOR4H`             | `VARCHAR2(255)`  | NULL     | Odor description after 4 hours                                    |
| `ODOR24H`            | `VARCHAR2(255)`  | NULL     | Odor description after 24 hours                                   |
| `CREATION_DATE`      | `TIMESTAMP(6)`   | NULL     | Auto-set on insert                                                |
| `CREATED_BY`         | `VARCHAR2(8)`    | NULL     | User who created record                                           |
| `LAST_MODIFIED_DATE` | `TIMESTAMP(6)`   | NULL     | Auto-updated on changes                                           |
| `LAST_MODIFIED_BY`   | `VARCHAR2(8)`    | NULL     | User who last modified record                                     |
| `COMMENTS`           | `VARCHAR2(1000)` | NULL     | Additional comments                                               |
| `REG_NUMBER`         | `VARCHAR2(11)`   | NULL     | Auto-extracted registration number from GR_NUMBER                 |
| `BATCH`              | `NUMBER(*,0)`    | NULL     | Auto-extracted batch number from GR_NUMBER                        |

#### Business Rules (Triggers)

- `MAP_INITIAL_ID`: Auto-generated using sequence `seq_map_initial_id`
- `REG_NUMBER`: Auto-extracted via regex: `^GR-\d\d-\d{4,5}|^SL-\d{6}`
- `BATCH`: Auto-extracted as trailing digits from `GR_NUMBER`
- `CREATION_DATE` and `LAST_MODIFIED_DATE`: Auto-managed
- `GR_NUMBER` must be unique across all records

#### Sample Data

```
GR_NUMBER: "GR-88-0681-1"
EVALUATION_DATE: 2012-11-22
CHEMIST: "Kraft"
DILUTION: "10% in DPG"
ODOR0H: "resinous cypress, natural, pine needles, slightly fruity"
ODOR4H: "linear"
ODOR24H: "woody cedarwood"
REG_NUMBER: "GR-88-0681"
BATCH: 1
```

#### .NET Model Recommendation

```csharp
public class MapInitial
{
    public long MapInitialId { get; set; }

    [Required]
    [StringLength(14)]
    public string GrNumber { get; set; }

    public DateTime? EvaluationDate { get; set; }

    [StringLength(50)]
    public string Chemist { get; set; }

    [StringLength(255)]
    public string Assessor { get; set; }

    [StringLength(30)]
    public string Dilution { get; set; }

    [StringLength(2)]
    public string EvaluationSite { get; set; }

    [StringLength(500)]
    public string Odor0H { get; set; }

    [StringLength(255)]
    public string Odor4H { get; set; }

    [StringLength(255)]
    public string Odor24H { get; set; }

    public DateTime? CreationDate { get; set; }

    [StringLength(8)]
    public string CreatedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    [StringLength(8)]
    public string LastModifiedBy { get; set; }

    [StringLength(1000)]
    public string Comments { get; set; }

    [StringLength(11)]
    public string RegNumber { get; set; }

    public int? Batch { get; set; }
}
```

---

### 2. MAP_SESSION

**Purpose**: Represents evaluation sessions where molecules are assessed by panels at different stages of the MAP process.

#### Table Structure

| Column               | Type             | Nullable | Description                                      |
| -------------------- | ---------------- | -------- | ------------------------------------------------ |
| `SESSION_ID`         | `NUMBER(*,0)`    | NOT NULL | Primary key, auto-generated via sequence         |
| `STAGE`              | `VARCHAR2(20)`   | NULL     | Session stage (constrained values)               |
| `EVALUATION_DATE`    | `DATE`           | NULL     | Date of session                                  |
| `REGION`             | `VARCHAR2(2)`    | NULL     | Geographic region (e.g., "EU", "US")             |
| `SEGMENT`            | `VARCHAR2(2)`    | NULL     | Market segment (e.g., "CP", "FF")                |
| `PARTICIPANTS`       | `VARCHAR2(1000)` | NULL     | List of session participants                     |
| `SHOW_IN_TASK_LIST`  | `CHAR(1)`        | NULL     | Flag to show in task list ('Y'/'N'), default 'N' |
| `CREATION_DATE`      | `TIMESTAMP(6)`   | NULL     | Auto-set on insert                               |
| `CREATED_BY`         | `VARCHAR2(8)`    | NULL     | User who created record                          |
| `LAST_MODIFIED_DATE` | `TIMESTAMP(6)`   | NULL     | Auto-updated on changes                          |
| `LAST_MODIFIED_BY`   | `VARCHAR2(8)`    | NULL     | User who last modified record                    |
| `SUB_STAGE`          | `NUMBER(1,0)`    | NULL     | Sub-stage identifier                             |
| `CATEGORY`           | `VARCHAR2(2)`    | NULL     | Session category                                 |

#### Business Rules

- `SESSION_ID`: Auto-generated using sequence `seq_map_session_id`
- `STAGE`: Must be one of: `'MAP 0'`, `'MAP 1'`, `'MAP 2'`, `'MAP 3'`, `'ISC'`, `'FIB'`, `'FIM'`, `'ISC (Quest)'`, `'CARDEX'`, `'RPMC'`
- `SHOW_IN_TASK_LIST`: Defaults to 'N'
- Timestamps auto-managed

#### Sample Data

```
SESSION_ID: 4005
STAGE: "MAP 3"
EVALUATION_DATE: 2004-06-18
REGION: "US"
SEGMENT: "CP"
SHOW_IN_TASK_LIST: "N"
```

#### .NET Model Recommendation

```csharp
public class MapSession
{
    public long SessionId { get; set; }

    [StringLength(20)]
    public string Stage { get; set; }

    public DateTime? EvaluationDate { get; set; }

    [StringLength(2)]
    public string Region { get; set; }

    [StringLength(2)]
    public string Segment { get; set; }

    [StringLength(1000)]
    public string Participants { get; set; }

    [StringLength(1)]
    public string ShowInTaskList { get; set; } = "N";

    public DateTime? CreationDate { get; set; }

    [StringLength(8)]
    public string CreatedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    [StringLength(8)]
    public string LastModifiedBy { get; set; }

    public int? SubStage { get; set; }

    [StringLength(2)]
    public string Category { get; set; }

    // Navigation properties
    public ICollection<MapResult> Results { get; set; }
}

public enum SessionStage
{
    MAP0,
    MAP1,
    MAP2,
    MAP3,
    ISC,
    FIB,
    FIM,
    ISCQuest,
    CARDEX,
    RPMC
}
```

---

### 3. MAP_RESULT

**Purpose**: Stores individual evaluation results for molecules within assessment sessions.

#### Table Structure

| Column               | Type             | Nullable | Description                                |
| -------------------- | ---------------- | -------- | ------------------------------------------ |
| `RESULT_ID`          | `NUMBER(*,0)`    | NOT NULL | Primary key, auto-generated                |
| `SESSION_ID`         | `NUMBER(*,0)`    | NOT NULL | Foreign key to MAP_SESSION                 |
| `GR_NUMBER`          | `VARCHAR2(14)`   | NOT NULL | Molecule identifier                        |
| `ODOR`               | `VARCHAR2(1000)` | NULL     | Odor description                           |
| `BENCHMARK_COMMENTS` | `VARCHAR2(2000)` | NULL     | Comments comparing to benchmarks           |
| `RESULT`             | `NUMBER(6,0)`    | NULL     | Numeric result/score (1-5 scale typically) |
| `DILUTION`           | `VARCHAR2(20)`   | NULL     | Dilution used                              |
| `SPONSOR`            | `VARCHAR2(255)`  | NULL     | Sponsor information                        |
| `CREATION_DATE`      | `TIMESTAMP(6)`   | NULL     | Auto-set on insert                         |
| `CREATED_BY`         | `VARCHAR2(8)`    | NULL     | User who created record                    |
| `LAST_MODIFIED_DATE` | `TIMESTAMP(6)`   | NULL     | Auto-updated on changes                    |
| `LAST_MODIFIED_BY`   | `VARCHAR2(8)`    | NULL     | User who last modified record              |
| `REG_NUMBER`         | `VARCHAR2(11)`   | NULL     | Auto-extracted from GR_NUMBER              |
| `BATCH`              | `NUMBER(*,0)`    | NULL     | Auto-extracted from GR_NUMBER              |

#### Business Rules

- `RESULT_ID`: Auto-generated using sequence `seq_map_result_id`
- Foreign key to `MAP_SESSION` with CASCADE DELETE
- `REG_NUMBER` and `BATCH`: Auto-extracted like in MAP_INITIAL
- Timestamps auto-managed

#### Sample Data

```
RESULT_ID: 207
SESSION_ID: 4111
GR_NUMBER: "GR-86-6561-0"
ODOR: "Rosy, floral, peonile, geranium, interesting in DD but not powerful"
BENCHMARK_COMMENTS: "CP: 02/09/2005, Status 1, FF: 04/15/2005, Status 1"
RESULT: 1
```

#### .NET Model Recommendation

```csharp
public class MapResult
{
    public long ResultId { get; set; }

    [Required]
    public long SessionId { get; set; }

    [Required]
    [StringLength(14)]
    public string GrNumber { get; set; }

    [StringLength(1000)]
    public string Odor { get; set; }

    [StringLength(2000)]
    public string BenchmarkComments { get; set; }

    public int? Result { get; set; }

    [StringLength(20)]
    public string Dilution { get; set; }

    [StringLength(255)]
    public string Sponsor { get; set; }

    public DateTime? CreationDate { get; set; }

    [StringLength(8)]
    public string CreatedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    [StringLength(8)]
    public string LastModifiedBy { get; set; }

    [StringLength(11)]
    public string RegNumber { get; set; }

    public int? Batch { get; set; }

    // Navigation property
    [ForeignKey("SessionId")]
    public MapSession Session { get; set; }
}
```

---

### 4. ODOR_CHARACTERIZATION

**Purpose**: Stores detailed odor profiling with intensity scores across multiple odor families and descriptors (100+ descriptor fields).

#### Table Structure (Key Fields)

| Column                     | Type             | Nullable | Description                                          |
| -------------------------- | ---------------- | -------- | ---------------------------------------------------- |
| `ODOR_CHARACTERIZATION_ID` | `NUMBER(38,0)`   | NOT NULL | Primary key, auto-generated                          |
| `GR_NUMBER`                | `VARCHAR2(14)`   | NOT NULL | Unique molecule identifier                           |
| `ODOR_SUMMARY`             | `VARCHAR2(1000)` | NULL     | Text summary of odor                                 |
| `CREATION_DATE`            | `TIMESTAMP(6)`   | NULL     | Auto-set on insert                                   |
| `CREATED_BY`               | `VARCHAR2(20)`   | NULL     | User who created record                              |
| `LAST_MODIFIED_DATE`       | `TIMESTAMP(6)`   | NULL     | Auto-updated on changes                              |
| `LAST_MODIFIED_BY`         | `VARCHAR2(20)`   | NULL     | User who last modified record                        |
| `REG_NUMBER`               | `VARCHAR2(11)`   | NULL     | Auto-extracted from GR_NUMBER                        |
| `BATCH`                    | `NUMBER(*,0)`    | NULL     | Auto-extracted from GR_NUMBER                        |
| `INTENSITY`                | `NUMBER(2,0)`    | NULL     | Overall intensity score (typically 1-10)             |
| `TENACITY`                 | `NUMBER(2,0)`    | NULL     | How long the odor lasts (typically 1-10)             |
| `OVERALL_LIKING`           | `NUMBER(2,0)`    | NULL     | Overall preference score (typically 1-10)            |
| `FAMILY_PROFILE`           | `VARCHAR2(2000)` | NULL     | Text profile of families (e.g., "Fruity 2, Green 2") |
| `ODOR_PROFILE`             | `VARCHAR2(2000)` | NULL     | Text profile of descriptors                          |

**Plus 100+ descriptor fields for 12 odor families:**

#### Odor Families and Their Descriptors

1. **AMBERGRIS_FAMILY**

   - `AMBERGRIS`, `WOODY_AMBERY`

2. **AROMATIC_HERBAL_FAMILY**

   - `AGRESTIC`, `CAMPHORACEOUS`, `INCENSE`, `MINTY_BLUE`, `MINTY_GREEN`, `PEPPERY_TERPENIC`, `PINEY_CONIFEROUS_RESINOUS`, `RESINOUS_BALSAMIC`, `THYME`

3. **CITRUS_FAMILY**

   - `ALDEHYDIC`, `CITRONELLA`, `GRAPEFRUIT`, `LEMON`, `LIME`, `MANDARIN`, `ORANGE`

4. **FLORAL_FAMILY**

   - `FLORAL_BALSAMIC`, `FREESIA`, `JASMINE`, `LILAC`, `LILY_OF_THE_VALLEY`, `ROSE`, `VIOLET`, `YLANG_YLANG_TUBERSOE`

5. **FRUITY_FAMILY**

   - `APPLE`, `BANANA`, `CASSIS`, `COCONUT_PRUNE_MILKY_LACTONIC`, `DRIED_FRUITS_WINEY`, `MELON`, `PEACH_APRICOT`, `PEAR`, `PINEAPPLE`, `RASPBERRY`, `STRAWBERRY_RED_BERRIES`

6. **GREEN_FAMILY**

   - `CUCUMBER_VEGETABLE`, `FOLIAGE`, `FRUITY_GREEN_RHUBARB`, `GALBANUM`, `GREEN_APPLE`, `GREEN_GRASSY`, `GREEN_METALLIC`, `IVY_LEAVES`, `MOLDY_EARTHY`, `MUSHROOMY`, `VIOLET_LEAVES`

7. **MARINE_FAMILY**

   - `MARINE`, `OZONIC`

8. **MUSKY_ANIMALIC_FAMILY**

   - `ANIMALIC`, `FECAL`, `INDOLIC_NAPHTHALINIC`, `LEATHERY`, `MUSKY`

9. **OFF_ODORS_FAMILY**

   - `ACIDIC_CHEESY`, `BLEACHY_CHLORINE`, `CELERY_FENUGREEK`, `FISHY_AMMONIACAL`, `MEDICINAL`, `MUSTY`, `OILY_FATTY_RANCID`, `SOLVENT_LIKE`, `SULFURY_MEATY_EGGY_BURNT`, `SWEATY`, `URINOUS`, `WAXY`

10. **SPICY_FAMILY**

    - `ANISIC`, `CARNATION_CLOVE`, `CINNAMON`, `CUMINIC`, `SAFFRON`

11. **SWEET_GOURMAND_FAMILY**

    - `ALMOND`, `CARAMEL`, `HONEY`, `LICORICE_TARRAGON_HAWTHORN`, `MARZIPAN`, `NUTTY`, `POWDERY_SWEET`, `VANILLA`

12. **WOODY_FAMILY**
    - `CEDARWOOD`, `GUIAC_WOOD`, `OAKMOSS`, `PATCHOULI`, `SANDALWOOD`, `VETIVER`

Each descriptor field is `NUMBER(2,0)` and typically contains intensity scores from 1-10.

#### Business Rules

- `ODOR_CHARACTERIZATION_ID`: Auto-generated using sequence `seq_odor_char_id`
- `GR_NUMBER` must be unique
- `CREATED_BY` and `LAST_MODIFIED_BY`: Default to database user if not provided
- `REG_NUMBER` and `BATCH`: Auto-extracted from GR_NUMBER

#### Sample Data

```
GR_NUMBER: "GR-88-0808-1"
ODOR_SUMMARY: "resinous cypress, natural, pine needles, slightly fruity"
INTENSITY: 6
TENACITY: 4
OVERALL_LIKING: 6
FAMILY_PROFILE: "Fruity 2, Green 2, Woody 2"
ODOR_PROFILE: "Apple 2, Green Grassy 2, Cedarwood 2"
APPLE: 2
GREEN_GRASSY: 2
CEDARWOOD: 2
(all other descriptors: null)
```

#### .NET Model Recommendation

```csharp
public class OdorCharacterization
{
    public long OdorCharacterizationId { get; set; }

    [Required]
    [StringLength(14)]
    public string GrNumber { get; set; }

    [StringLength(1000)]
    public string OdorSummary { get; set; }

    public DateTime? CreationDate { get; set; }

    [StringLength(20)]
    public string CreatedBy { get; set; }

    public DateTime? LastModifiedDate { get; set; }

    [StringLength(20)]
    public string LastModifiedBy { get; set; }

    [StringLength(11)]
    public string RegNumber { get; set; }

    public int? Batch { get; set; }

    // Overall metrics
    public int? Intensity { get; set; }
    public int? Tenacity { get; set; }
    public int? OverallLiking { get; set; }

    [StringLength(2000)]
    public string FamilyProfile { get; set; }

    [StringLength(2000)]
    public string OdorProfile { get; set; }

    // Family scores (12 families)
    public int? AmbergrisFamily { get; set; }
    public int? AromaticHerbalFamily { get; set; }
    public int? CitrusFamily { get; set; }
    public int? FloralFamily { get; set; }
    public int? FruityFamily { get; set; }
    public int? GreenFamily { get; set; }
    public int? MarineFamily { get; set; }
    public int? MuskyAnimalicFamily { get; set; }
    public int? OffOdorsFamily { get; set; }
    public int? SpicyFamily { get; set; }
    public int? SweetGourmandFamily { get; set; }
    public int? WoodyFamily { get; set; }

    // Ambergris descriptors
    public int? Ambergris { get; set; }
    public int? WoodyAmbery { get; set; }

    // Aromatic-Herbal descriptors
    public int? Agrestic { get; set; }
    public int? Camphoraceous { get; set; }
    public int? Incense { get; set; }
    public int? MintyBlue { get; set; }
    public int? MintyGreen { get; set; }
    public int? PepperyTerpenic { get; set; }
    public int? PineyConiferous { get; set; }
    public int? ResinousBalsamic { get; set; }
    public int? Thyme { get; set; }

    // Citrus descriptors
    public int? Aldehydic { get; set; }
    public int? Citronella { get; set; }
    public int? Grapefruit { get; set; }
    public int? Lemon { get; set; }
    public int? Lime { get; set; }
    public int? Mandarin { get; set; }
    public int? Orange { get; set; }

    // Floral descriptors
    public int? FloralBalsamic { get; set; }
    public int? Freesia { get; set; }
    public int? Jasmine { get; set; }
    public int? Lilac { get; set; }
    public int? LilyOfTheValley { get; set; }
    public int? Rose { get; set; }
    public int? Violet { get; set; }
    public int? YlangYlangTubersoe { get; set; }

    // Fruity descriptors
    public int? Apple { get; set; }
    public int? Banana { get; set; }
    public int? Cassis { get; set; }
    public int? CoconutPruneMilkyLactonic { get; set; }
    public int? DriedFruitsWiney { get; set; }
    public int? Melon { get; set; }
    public int? PeachApricot { get; set; }
    public int? Pear { get; set; }
    public int? Pineapple { get; set; }
    public int? Raspberry { get; set; }
    public int? StrawberryRedBerries { get; set; }

    // Green descriptors
    public int? CucumberVegetable { get; set; }
    public int? Foliage { get; set; }
    public int? FruityGreenRhubarb { get; set; }
    public int? Galbanum { get; set; }
    public int? GreenApple { get; set; }
    public int? GreenGrassy { get; set; }
    public int? GreenMetallic { get; set; }
    public int? IvyLeaves { get; set; }
    public int? MoldyEarthy { get; set; }
    public int? Mushroomy { get; set; }
    public int? VioletLeaves { get; set; }

    // Marine descriptors
    public int? Marine { get; set; }
    public int? Ozonic { get; set; }

    // Musky-Animalic descriptors
    public int? Animalic { get; set; }
    public int? Fecal { get; set; }
    public int? IndolicNaphthalinic { get; set; }
    public int? Leathery { get; set; }
    public int? Musky { get; set; }

    // Off-Odors descriptors
    public int? AcidicCheesy { get; set; }
    public int? BleachyChlorine { get; set; }
    public int? CeleryFenugreek { get; set; }
    public int? FishyAmmoniacal { get; set; }
    public int? Medicinal { get; set; }
    public int? Musty { get; set; }
    public int? OilyFattyRancid { get; set; }
    public int? SolventLike { get; set; }
    public int? SulfuryMeatyEggyBurnt { get; set; }
    public int? Sweaty { get; set; }
    public int? Urinous { get; set; }
    public int? Waxy { get; set; }

    // Spicy descriptors
    public int? Anisic { get; set; }
    public int? CarnationClove { get; set; }
    public int? Cinnamon { get; set; }
    public int? Cuminic { get; set; }
    public int? Saffron { get; set; }

    // Sweet-Gourmand descriptors
    public int? Almond { get; set; }
    public int? Caramel { get; set; }
    public int? Honey { get; set; }
    public int? LicoriceTarragonHawthorn { get; set; }
    public int? Marzipan { get; set; }
    public int? Nutty { get; set; }
    public int? PowderySweet { get; set; }
    public int? Vanilla { get; set; }

    // Woody descriptors
    public int? Cedarwood { get; set; }
    public int? GuiacWood { get; set; }
    public int? Oakmoss { get; set; }
    public int? Patchouli { get; set; }
    public int? Sandalwood { get; set; }
    public int? Vetiver { get; set; }
}
```

---

### 5. MAP_ODOR_FAMILY

**Purpose**: Reference/lookup table for odor family categorizations with associated display colors.

#### Table Structure

| Column  | Type           | Nullable | Description                                       |
| ------- | -------------- | -------- | ------------------------------------------------- |
| `ID`    | `NUMBER(38,0)` | NOT NULL | Primary key                                       |
| `CODE`  | `VARCHAR2(50)` | NULL     | Unique code identifier (e.g., "AMBERGRIS_FAMILY") |
| `NAME`  | `VARCHAR2(50)` | NULL     | Display name (e.g., "Ambergris")                  |
| `COLOR` | `VARCHAR2(10)` | NULL     | Hex color code for UI display                     |

#### Complete Data Set (12 Families)

| ID  | CODE                   | NAME            | COLOR   |
| --- | ---------------------- | --------------- | ------- |
| 1   | AMBERGRIS_FAMILY       | Ambergris       | #dadbdc |
| 2   | AROMATIC_HERBAL_FAMILY | Aromatic-Herbal | #7fd6f7 |
| 3   | CITRUS_FAMILY          | Citrus          | #fff87f |
| 4   | FLORAL_FAMILY          | Floral          | #f39db4 |
| 5   | FRUITY_FAMILY          | Fruity          | #fedb8c |
| 6   | GREEN_FAMILY           | Green           | #a1d5a4 |
| 7   | MARINE_FAMILY          | Marine          | #94a0c6 |
| 8   | MUSKY_ANIMALIC_FAMILY  | Musky-Animalic  | #f57fc5 |
| 9   | OFF_ODORS_FAMILY       | Off-Odors       | #999999 |
| 10  | SPICY_FAMILY           | Spicy           | #e59a9a |
| 11  | SWEET_GOURMAND_FAMILY  | Sweet-Gourmand  | #ebbd90 |
| 12  | WOODY_FAMILY           | Woody           | #a89791 |

#### .NET Model Recommendation

```csharp
public class MapOdorFamily
{
    public long Id { get; set; }

    [StringLength(50)]
    public string Code { get; set; }

    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(10)]
    public string Color { get; set; }

    // Navigation property
    public ICollection<MapOdorDescriptor> Descriptors { get; set; }
}
```

---

### 6. MAP_ODOR_DESCRIPTOR

**Purpose**: Reference/lookup table for specific odor descriptors, each belonging to a family. Contains ~88 descriptors.

#### Table Structure

| Column         | Type            | Nullable | Description                                                            |
| -------------- | --------------- | -------- | ---------------------------------------------------------------------- |
| `ID`           | `NUMBER(38,0)`  | NOT NULL | Primary key                                                            |
| `CODE`         | `VARCHAR2(255)` | NULL     | Unique code identifier (matches column names in ODOR_CHARACTERIZATION) |
| `NAME`         | `VARCHAR2(255)` | NULL     | Display name                                                           |
| `FAMILY_ID`    | `NUMBER(38,0)`  | NULL     | Foreign key to MAP_ODOR_FAMILY                                         |
| `PROFILE_NAME` | `VARCHAR2(255)` | NULL     | Profile display name                                                   |

#### Sample Data

| ID  | CODE            | NAME                         | FAMILY_ID | PROFILE_NAME    |
| --- | --------------- | ---------------------------- | --------- | --------------- |
| 63  | SOLVENT_LIKE    | Solvent-Like (tert-Butanol)  | 9         | Solvent-Like    |
| 68  | ANISIC          | Anisic (Anethole)            | 10        | Anisic          |
| 69  | CARNATION_CLOVE | Carnation/Clove (Isoeugenol) | 10        | Carnation/Clove |

#### .NET Model Recommendation

```csharp
public class MapOdorDescriptor
{
    public long Id { get; set; }

    [StringLength(255)]
    public string Code { get; set; }

    [StringLength(255)]
    public string Name { get; set; }

    public long? FamilyId { get; set; }

    [StringLength(255)]
    public string ProfileName { get; set; }

    // Navigation property
    [ForeignKey("FamilyId")]
    public MapOdorFamily Family { get; set; }
}
```

---

### 7. MAP1_SESSION_LINK

**Purpose**: Links MAP 1 sessions between Consumer Preference (CP) and Fine Fragrance (FF) evaluations for the same set of molecules.

#### Table Structure

| Column          | Type          | Nullable | Description                              |
| --------------- | ------------- | -------- | ---------------------------------------- |
| `CP_SESSION_ID` | `NUMBER(*,0)` | NOT NULL | Session ID for Consumer Preference panel |
| `FF_SESSION_ID` | `NUMBER(*,0)` | NOT NULL | Session ID for Fine Fragrance panel      |

#### Business Rules

- Composite primary key on both fields
- Both fields have foreign keys to `MAP_SESSION` with CASCADE DELETE
- Creates a many-to-many relationship between CP and FF sessions

#### .NET Model Recommendation

```csharp
public class Map1SessionLink
{
    [Key, Column(Order = 0)]
    public long CpSessionId { get; set; }

    [Key, Column(Order = 1)]
    public long FfSessionId { get; set; }

    // Navigation properties
    [ForeignKey("CpSessionId")]
    public MapSession CpSession { get; set; }

    [ForeignKey("FfSessionId")]
    public MapSession FfSession { get; set; }
}
```

---

### 8. SUBMITTING_IGNORED_MOLECULES

**Purpose**: Tracks molecules that have been deliberately excluded or ignored during submission processes.

#### Table Structure

| Column         | Type           | Nullable | Description                      |
| -------------- | -------------- | -------- | -------------------------------- |
| `GR_NUMBER`    | `VARCHAR2(20)` | NOT NULL | Primary key, molecule identifier |
| `ENTRY_PERSON` | `VARCHAR2(20)` | NOT NULL | Person who added to ignore list  |
| `ENTRY_DATE`   | `TIMESTAMP(6)` | NOT NULL | Auto-set on insert               |

#### Business Rules

- `ENTRY_DATE`: Auto-set to current timestamp on insert
- Primary key on `GR_NUMBER` ensures each molecule can only be ignored once

#### .NET Model Recommendation

```csharp
public class SubmittingIgnoredMolecules
{
    [Key]
    [StringLength(20)]
    public string GrNumber { get; set; }

    [Required]
    [StringLength(20)]
    public string EntryPerson { get; set; }

    [Required]
    public DateTime EntryDate { get; set; }
}
```

---

## Key Business Concepts

### GR Number Format

The `GR_NUMBER` is the primary molecule identifier with specific formats:

- **Standard format**: `GR-YY-NNNNN-B` (e.g., "GR-88-0681-1")
  - `GR`: Prefix
  - `YY`: Year (2 digits)
  - `NNNNN`: Sequential number (4-5 digits)
  - `B`: Batch number (1-3 digits)
- **SL format**: `SL-NNNNNN-B` (e.g., "SL-123456-1")

### Derived Fields

Several tables auto-extract:

- **REG_NUMBER**: Base identifier without batch (e.g., "GR-88-0681")
- **BATCH**: Trailing batch number extracted as integer

### Evaluation Workflow

1. **Initial Assessment** (`MAP_INITIAL`)

   - Chemist creates initial evaluation
   - Odor described at 0h, 4h, 24h intervals
   - Basic metadata captured

2. **Detailed Characterization** (`ODOR_CHARACTERIZATION`)

   - Comprehensive odor profiling
   - 100+ descriptors scored
   - Intensity, tenacity, overall liking recorded

3. **Panel Sessions** (`MAP_SESSION`)

   - Different stages: MAP 0 → MAP 1 → MAP 2 → MAP 3 → ISC/FIB/FIM/CARDEX/RPMC
   - Regional and segment-specific evaluations
   - Multiple participants assess molecules

4. **Results Recording** (`MAP_RESULT`)
   - Individual assessments within sessions
   - Linked to sessions via `SESSION_ID`
   - Benchmark comparisons
   - Numeric scoring (typically 1-5)

### Audit Trail

All primary tables include:

- `CREATION_DATE` / `CREATED_BY`
- `LAST_MODIFIED_DATE` / `LAST_MODIFIED_BY`

Auto-managed by database triggers for consistent audit trail.

---

## Database Schema Details

### Schema Name

`GIV_MAP` (Givaudan MAP system)

### Sequences

- `seq_map_initial_id` → `MAP_INITIAL.MAP_INITIAL_ID`
- `seq_map_session_id` → `MAP_SESSION.SESSION_ID`
- `seq_map_result_id` → `MAP_RESULT.RESULT_ID`
- `seq_odor_char_id` → `ODOR_CHARACTERIZATION.ODOR_CHARACTERIZATION_ID`

### Foreign Key Relationships

```
MAP_ODOR_DESCRIPTOR.FAMILY_ID → MAP_ODOR_FAMILY.ID
MAP_RESULT.SESSION_ID → MAP_SESSION.SESSION_ID (ON DELETE CASCADE)
MAP1_SESSION_LINK.CP_SESSION_ID → MAP_SESSION.SESSION_ID (ON DELETE CASCADE)
MAP1_SESSION_LINK.FF_SESSION_ID → MAP_SESSION.SESSION_ID (ON DELETE CASCADE)
```

### Indexes

#### MAP_INITIAL

- PK: `MAP_INITIAL_PK` on `MAP_INITIAL_ID`
- Unique: `MAP_INITIAL_UNIQUE_GR_NUMBER` on `GR_NUMBER`
- Index: `MAP_INITIAL_REG_NUMBER` on `REG_NUMBER`

#### MAP_SESSION

- PK: `MAP_SESSION_PK` on `SESSION_ID`
- Index: `IDX_SESSION_STAGE` on `STAGE`
- Index: `IDX_SHOW_IN_TASK_LIST` on `SHOW_IN_TASK_LIST`

#### MAP_RESULT

- PK: `MAP_RESULT_PK` on `RESULT_ID`
- Index: `MAP_RESULT_REG_NUMBER` on `REG_NUMBER`
- Index: `MAP_RESULT_GR_NUMBER` on `GR_NUMBER`
- Index: `IDX_MAP_RESULT` on `RESULT`

#### ODOR_CHARACTERIZATION

- PK: `ODOR_CHARACTERIZATION_PK` on `ODOR_CHARACTERIZATION_ID`
- Unique: `ODOR_CHAR_UNIQUE_GR_NUMBER` on `GR_NUMBER`
- Index: `IDX_REG_NUMBER_ODOR_CHARACT` on `REG_NUMBER`

#### MAP_ODOR_FAMILY

- PK: `MAP_ODOR_FAMILY_PK` on `ID`

#### MAP_ODOR_DESCRIPTOR

- PK: `MAP_ODOR_DESCRIPTOR_PK` on `ID`

#### MAP1_SESSION_LINK

- PK: `MAP1_SESSION_LINK_PK` on composite `(CP_SESSION_ID, FF_SESSION_ID)`

#### SUBMITTING_IGNORED_MOLECULES

- PK: `MAP_TRACKING_IGNORED_MOLEC_PK` on `GR_NUMBER`

---

## .NET Web API Implementation Recommendations

### DbContext Example

```csharp
public class MapDbContext : DbContext
{
    public DbSet<MapInitial> MapInitials { get; set; }
    public DbSet<MapSession> MapSessions { get; set; }
    public DbSet<MapResult> MapResults { get; set; }
    public DbSet<OdorCharacterization> OdorCharacterizations { get; set; }
    public DbSet<MapOdorFamily> OdorFamilies { get; set; }
    public DbSet<MapOdorDescriptor> OdorDescriptors { get; set; }
    public DbSet<Map1SessionLink> Map1SessionLinks { get; set; }
    public DbSet<SubmittingIgnoredMolecules> IgnoredMolecules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure composite key for Map1SessionLink
        modelBuilder.Entity<Map1SessionLink>()
            .HasKey(m => new { m.CpSessionId, m.FfSessionId });

        // Configure relationships
        modelBuilder.Entity<MapResult>()
            .HasOne(r => r.Session)
            .WithMany(s => s.Results)
            .HasForeignKey(r => r.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MapOdorDescriptor>()
            .HasOne(d => d.Family)
            .WithMany(f => f.Descriptors)
            .HasForeignKey(d => d.FamilyId);

        // Configure table names to match Oracle
        modelBuilder.Entity<MapInitial>().ToTable("MAP_INITIAL", "GIV_MAP");
        modelBuilder.Entity<MapSession>().ToTable("MAP_SESSION", "GIV_MAP");
        modelBuilder.Entity<MapResult>().ToTable("MAP_RESULT", "GIV_MAP");
        modelBuilder.Entity<OdorCharacterization>().ToTable("ODOR_CHARACTERIZATION", "GIV_MAP");
        modelBuilder.Entity<MapOdorFamily>().ToTable("MAP_ODOR_FAMILY", "GIV_MAP");
        modelBuilder.Entity<MapOdorDescriptor>().ToTable("MAP_ODOR_DESCRIPTOR");
        modelBuilder.Entity<Map1SessionLink>().ToTable("MAP1_SESSION_LINK", "GIV_MAP");
        modelBuilder.Entity<SubmittingIgnoredMolecules>().ToTable("SUBMITTING_IGNORED_MOLECULES", "GIV_MAP");

        // Configure sequences
        modelBuilder.Entity<MapInitial>()
            .Property(m => m.MapInitialId)
            .HasDefaultValueSql("seq_map_initial_id.nextval");

        modelBuilder.Entity<MapSession>()
            .Property(m => m.SessionId)
            .HasDefaultValueSql("seq_map_session_id.nextval");

        modelBuilder.Entity<MapResult>()
            .Property(m => m.ResultId)
            .HasDefaultValueSql("seq_map_result_id.nextval");

        modelBuilder.Entity<OdorCharacterization>()
            .Property(o => o.OdorCharacterizationId)
            .HasDefaultValueSql("seq_odor_char_id.nextval");
    }
}
```

### Connection String Example

```json
{
  "ConnectionStrings": {
    "MapDatabase": "User Id=giv_map;Password=yourpassword;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=your-oracle-host)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=your-service)))"
  }
}
```

### Controller Structure Suggestions

```csharp
[ApiController]
[Route("api/[controller]")]
public class MoleculesController : ControllerBase
{
    // GET /api/molecules/{grNumber}
    // GET /api/molecules/{grNumber}/initial
    // GET /api/molecules/{grNumber}/characterization
    // POST /api/molecules/initial
    // PUT /api/molecules/initial/{id}
}

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    // GET /api/sessions
    // GET /api/sessions/{id}
    // GET /api/sessions/{id}/results
    // POST /api/sessions
    // POST /api/sessions/{id}/results
}

[ApiController]
[Route("api/[controller]")]
public class OdorProfilesController : ControllerBase
{
    // GET /api/odorprofiles/families
    // GET /api/odorprofiles/descriptors
    // GET /api/odorprofiles/descriptors/family/{familyId}
}
```

### Important Implementation Notes

1. **Trigger Logic**: Many fields are auto-populated by database triggers. Your API should:

   - Not send values for auto-generated fields (IDs, timestamps, REG_NUMBER, BATCH)
   - Or read them back after insert to get trigger-populated values

2. **Batch Operations**: The system handles large datasets (60K+ molecules, 13K+ sessions)

   - Implement pagination for list endpoints
   - Consider async operations for bulk operations
   - Add filtering by REG_NUMBER, STAGE, REGION, SEGMENT

3. **Validation**:

   - GR_NUMBER format validation (regex patterns from triggers)
   - STAGE enum validation against allowed values
   - Odor descriptor scores should be 1-10 (or nullable)

4. **Performance**:

   - Use indexed columns (REG_NUMBER, GR_NUMBER, STAGE) in WHERE clauses
   - Consider DTOs that don't include all 100+ descriptor fields unless needed
   - Implement projection queries when only specific fields are needed

5. **Data Relationships**:
   - A molecule (GR_NUMBER) can have:
     - One MAP_INITIAL record
     - One ODOR_CHARACTERIZATION record
     - Multiple MAP_RESULT records across different sessions

---

## Sample Queries for Common Use Cases

### 1. Get Complete Molecule Profile

```sql
SELECT
    mi.*,
    oc.INTENSITY,
    oc.TENACITY,
    oc.OVERALL_LIKING,
    oc.ODOR_SUMMARY
FROM MAP_INITIAL mi
LEFT JOIN ODOR_CHARACTERIZATION oc ON mi.GR_NUMBER = oc.GR_NUMBER
WHERE mi.GR_NUMBER = 'GR-88-0681-1';
```

### 2. Get Session Results with Molecules

```sql
SELECT
    ms.SESSION_ID,
    ms.STAGE,
    ms.EVALUATION_DATE,
    mr.GR_NUMBER,
    mr.ODOR,
    mr.RESULT
FROM MAP_SESSION ms
INNER JOIN MAP_RESULT mr ON ms.SESSION_ID = mr.SESSION_ID
WHERE ms.SESSION_ID = 4111
ORDER BY mr.RESULT;
```

### 3. Find Molecules by Odor Family

```sql
SELECT
    oc.GR_NUMBER,
    oc.ODOR_SUMMARY,
    oc.FRUITY_FAMILY,
    oc.APPLE,
    oc.STRAWBERRY_RED_BERRIES
FROM ODOR_CHARACTERIZATION oc
WHERE oc.FRUITY_FAMILY > 0
ORDER BY oc.FRUITY_FAMILY DESC;
```

### 4. Get All Results for a Registration Number (All Batches)

```sql
SELECT
    mr.GR_NUMBER,
    mr.BATCH,
    ms.STAGE,
    ms.EVALUATION_DATE,
    mr.RESULT,
    mr.ODOR
FROM MAP_RESULT mr
INNER JOIN MAP_SESSION ms ON mr.SESSION_ID = ms.SESSION_ID
WHERE mr.REG_NUMBER = 'GR-88-0681'
ORDER BY mr.BATCH, ms.EVALUATION_DATE;
```

### 5. Get Linked MAP1 Sessions (CP and FF)

```sql
SELECT
    cp.SESSION_ID as CP_SESSION_ID,
    cp.EVALUATION_DATE as CP_DATE,
    ff.SESSION_ID as FF_SESSION_ID,
    ff.EVALUATION_DATE as FF_DATE
FROM MAP1_SESSION_LINK link
INNER JOIN MAP_SESSION cp ON link.CP_SESSION_ID = cp.SESSION_ID
INNER JOIN MAP_SESSION ff ON link.FF_SESSION_ID = ff.SESSION_ID;
```

---

## Data Volume Estimates (Based on CSV Files)

- **MAP_INITIAL**: ~70,000+ records
- **MAP_SESSION**: ~13,600+ records
- **MAP_RESULT**: ~97,500+ records
- **ODOR_CHARACTERIZATION**: ~61,000+ records
- **MAP_ODOR_FAMILY**: 12 records (static reference)
- **MAP_ODOR_DESCRIPTOR**: ~88 records (static reference)
- **MAP1_SESSION_LINK**: ~600+ records
- **SUBMITTING_IGNORED_MOLECULES**: Variable

---

## Migration Considerations

### Data Import Strategy

1. **Load reference tables first**:

   - `MAP_ODOR_FAMILY` (12 records)
   - `MAP_ODOR_DESCRIPTOR` (88 records)

2. **Load core molecule data**:

   - `MAP_INITIAL` (70K records)
   - `ODOR_CHARACTERIZATION` (61K records)
   - **Note**: Many GR_NUMBERs may exist in one table but not the other

3. **Load session data**:

   - `MAP_SESSION` (13.6K records)
   - `MAP_RESULT` (97.5K records) - requires existing sessions
   - `MAP1_SESSION_LINK` (600 records) - requires existing sessions

4. **Load auxiliary data**:
   - `SUBMITTING_IGNORED_MOLECULES`

### Sequence Initialization

After data import, update sequences to start above maximum existing IDs:

```sql
-- Example for MAP_INITIAL
SELECT MAX(MAP_INITIAL_ID) + 1 FROM MAP_INITIAL;
ALTER SEQUENCE seq_map_initial_id RESTART WITH [max_value];
```

---

## Change Log & Versioning

- **Export Date**: October 28, 2025
- **Source System**: Oracle Database (GIV_MAP schema)
- **Documentation Version**: 1.0

---

## Contact & Support

For questions about this data structure or integration support, contact your database administrator or system architect familiar with the GIV_MAP system.
