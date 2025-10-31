using Microsoft.AspNetCore.Mvc;
using MAP2ADAMOINT.Models.Adamo;
using MAP2ADAMOINT.Models.MapTool;
using MAP2ADAMOINT.Services;
using MAP2ADAMOINT.Data;
using Microsoft.EntityFrameworkCore;

namespace MAP2ADAMOINT.Controllers
{
    [ApiController]
    [Route("transform")]
    public class TransformController : ControllerBase
    {
        private readonly DataMapperService _mapper;
        private readonly DatabaseService _dbService;
        private readonly FeatureFlags _features;
        private readonly AdamoContext? _adamoContext;
        private readonly MapToolContext? _mapToolContext;
        private readonly ILogger<TransformController> _logger;

        public TransformController(
            DataMapperService mapper,
            DatabaseService dbService,
            FeatureFlags features,
            IServiceProvider serviceProvider,
            ILogger<TransformController> logger)
        {
            _mapper = mapper;
            _dbService = dbService;
            _features = features;
            _adamoContext = serviceProvider.GetService<AdamoContext>();
            _mapToolContext = serviceProvider.GetService<MapToolContext>();
            _logger = logger;
        }

        #region Generic Transformations (Original)

        /// <summary>
        /// Generic: Transform MAP Tool Molecule + Evaluation to ADAMO MapInitial
        /// </summary>
        [HttpPost("map-to-adamo")]
        public async Task<IActionResult> MapToolToAdamo([FromBody] MapToolTransformRequest request)
        {
            _logger.LogInformation("Transform MAP Tool → ADAMO for GR: {GrNumber}", request.Molecule?.GrNumber);

            try
            {
                if (request.Molecule == null)
                {
                    return BadRequest(new { status = "fail", message = "Molecule data is required" });
                }

                var mapInitial = _mapper.MapMoleculeToMapInitial(request.Molecule, request.Evaluation);

                Console.WriteLine($"✓ Transformed Molecule → MapInitial: {mapInitial.GrNumber}");

                string? dbWriteMessage = null;
                if (_features.EnableDatabaseWrites && request.WriteToDatabase)
                {
                    var (success, message) = await _dbService.WriteToAdamo(mapInitial);
                    dbWriteMessage = message;

                    if (!success)
                    {
                        return StatusCode(500, new
                        {
                            status = "fail",
                            message = $"Transformation succeeded but database write failed: {message}",
                            transformed = mapInitial
                        });
                    }
                }

                return Ok(new
                {
                    status = "success",
                    message = $"Successfully transformed to ADAMO format for {mapInitial.GrNumber}",
                    transformed = mapInitial,
                    databaseWrite = _features.EnableDatabaseWrites && request.WriteToDatabase
                        ? dbWriteMessage
                        : "Database writes disabled"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed transformation: {ex.Message}");
                _logger.LogError(ex, "Transformation failed");
                return StatusCode(500, new { status = "fail", message = ex.Message });
            }
        }

        /// <summary>
        /// Generic: Transform ADAMO Session + Result to MAP Tool Assessment
        /// </summary>
        [HttpPost("adamo-to-map")]
        public async Task<IActionResult> AdamoToMapTool([FromBody] AdamoTransformRequest request)
        {
            _logger.LogInformation("Transform ADAMO → MAP Tool for Session: {SessionId}", request.Session?.SessionId);

            try
            {
                if (request.Session == null || request.Result == null)
                {
                    return BadRequest(new { status = "fail", message = "Both Session and Result data are required" });
                }

                var assessment = _mapper.MapResultToAssessment(request.Result, request.Session);

                Console.WriteLine($"✓ Transformed MapSession → Assessment: {assessment.SessionName}");

                string? dbWriteMessage = null;
                if (_features.EnableDatabaseWrites && request.WriteToDatabase)
                {
                    var (success, message) = await _dbService.WriteToMapTool(assessment);
                    dbWriteMessage = message;

                    if (!success)
                    {
                        return StatusCode(500, new
                        {
                            status = "fail",
                            message = $"Transformation succeeded but database write failed: {message}",
                            transformed = assessment
                        });
                    }
                }

                return Ok(new
                {
                    status = "success",
                    message = $"Successfully transformed to MAP Tool format for session {request.Session.SessionId}",
                    transformed = assessment,
                    databaseWrite = _features.EnableDatabaseWrites && request.WriteToDatabase
                        ? dbWriteMessage
                        : "Database writes disabled"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed transformation: {ex.Message}");
                _logger.LogError(ex, "Transformation failed");
                return StatusCode(500, new { status = "fail", message = ex.Message });
            }
        }

        #endregion

        #region Specific Entity Transformations

        /// <summary>
        /// Transform ADAMO OdorFamily to MAP Tool OdorFamily
        /// End-to-end: Fetch from ADAMO, transform, optionally write to MAP Tool
        /// </summary>
        [HttpPost("odorfamily/adamo-to-map/{familyId}")]
        public async Task<IActionResult> TransformOdorFamily(long familyId, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from ADAMO
                var adamoFamily = await _adamoContext.OdorFamilies.FindAsync(familyId);
                if (adamoFamily == null)
                {
                    return NotFound(new { status = "not_found", message = $"ADAMO OdorFamily {familyId} not found" });
                }

                // Step 2: Transform to MAP Tool format
                var mapToolFamily = new OdorFamily
                {
                    Name = adamoFamily.Name,
                    Color = adamoFamily.Color,
                    Code = adamoFamily.Code,
                    CreatedBy = "SYNC",
                    CreatedAt = DateTime.Now
                };

                Console.WriteLine($"✓ Transformed OdorFamily: {adamoFamily.Name}");

                // Step 3: Write to MAP Tool database (if enabled)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    // TODO: Uncomment when ready for production
                    // await _mapToolContext.OdorFamilies.AddAsync(mapToolFamily);
                    // await _mapToolContext.SaveChangesAsync();
                    Console.WriteLine($"[DRY RUN] Would insert OdorFamily '{mapToolFamily.Name}' to MAP Tool");
                }

                return Ok(new
                {
                    status = "success",
                    message = "OdorFamily transformed successfully",
                    source = new { database = "ADAMO", table = "MAP_ODOR_FAMILY", id = familyId },
                    transformed = mapToolFamily,
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform OdorFamily {Id}", familyId);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform ADAMO OdorDescriptor to MAP Tool OdorDescriptor
        /// End-to-end: Fetch from ADAMO, transform, optionally write to MAP Tool
        /// </summary>
        [HttpPost("odordescriptor/adamo-to-map/{descriptorId}")]
        public async Task<IActionResult> TransformOdorDescriptor(long descriptorId, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from ADAMO
                var adamoDescriptor = await _adamoContext.OdorDescriptors
                    .Include(d => d.Family)
                    .FirstOrDefaultAsync(d => d.Id == descriptorId);

                if (adamoDescriptor == null)
                {
                    return NotFound(new { status = "not_found", message = $"ADAMO OdorDescriptor {descriptorId} not found" });
                }

                // Step 2: Find or create matching family in MAP Tool
                // TODO: In production, lookup existing family by Code
                var mapToolFamilyId = adamoDescriptor.FamilyId ?? 1; // Placeholder

                // Step 3: Transform to MAP Tool format
                var mapToolDescriptor = new OdorDescriptor
                {
                    Name = adamoDescriptor.Name,
                    ProfileName = adamoDescriptor.ProfileName,
                    Code = adamoDescriptor.Code,
                    OdorFamilyId = (int)mapToolFamilyId, // TODO: Lookup actual MAP Tool family ID
                    CreatedBy = "SYNC",
                    CreatedAt = DateTime.Now
                };

                Console.WriteLine($"✓ Transformed OdorDescriptor: {adamoDescriptor.Name}");

                // Step 4: Write to MAP Tool database (if enabled)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    // TODO: Uncomment when ready for production
                    // await _mapToolContext.OdorDescriptors.AddAsync(mapToolDescriptor);
                    // await _mapToolContext.SaveChangesAsync();
                    Console.WriteLine($"[DRY RUN] Would insert OdorDescriptor '{mapToolDescriptor.Name}' to MAP Tool");
                }

                return Ok(new
                {
                    status = "success",
                    message = "OdorDescriptor transformed successfully",
                    source = new { database = "ADAMO", table = "MAP_ODOR_DESCRIPTOR", id = descriptorId },
                    transformed = mapToolDescriptor,
                    note = "TODO: FamilyId mapping requires lookup table",
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform OdorDescriptor {Id}", descriptorId);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform ADAMO MapInitial to MAP Tool Molecule (by ID)
        /// End-to-end: Fetch from ADAMO by MapInitialId, transform, optionally write to MAP Tool
        /// </summary>
        [HttpPost("initial-to-molecule/{id}")]
        public async Task<IActionResult> TransformInitialToMoleculeById(long id, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from ADAMO by ID
                var initial = await _adamoContext.MapInitials.FindAsync(id);
                if (initial == null)
                {
                    return NotFound(new { status = "not_found", message = $"MapInitial {id} not found in ADAMO" });
                }

                // Step 2: Transform
                var molecule = _mapper.MapInitialToMolecule(initial);

                Console.WriteLine($"✓ Transformed MAP_INITIAL → Molecule: {initial.GrNumber} (ID: {id})");

                // Step 3: Write to MAP Tool (if enabled)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    // TODO: Uncomment when ready
                    // Check if exists first
                    // var exists = await _mapToolContext.Molecules.AnyAsync(m => m.GrNumber == initial.GrNumber);
                    // if (!exists) {
                    //     await _mapToolContext.Molecules.AddAsync(molecule);
                    //     await _mapToolContext.SaveChangesAsync();
                    // }
                    Console.WriteLine($"[DRY RUN] Would insert Molecule '{initial.GrNumber}' to MAP Tool");
                }

                return Ok(new
                {
                    status = "success",
                    message = $"MAP_INITIAL → Molecule transformed for {initial.GrNumber}",
                    source = new { database = "ADAMO", table = "MAP_INITIAL", id, grNumber = initial.GrNumber },
                    transformed = molecule,
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform Initial→Molecule for ID {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform ADAMO MapInitial to MAP Tool Molecule (by GR_NUMBER)
        /// End-to-end: Fetch from ADAMO by GR_NUMBER, transform, optionally write to MAP Tool
        /// </summary>
        [HttpPost("initial-to-molecule/gr/{grNumber}")]
        public async Task<IActionResult> TransformInitialToMoleculeByGr(string grNumber, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from ADAMO
                var initial = await _adamoContext.MapInitials.FirstOrDefaultAsync(m => m.GrNumber == grNumber);
                if (initial == null)
                {
                    return NotFound(new { status = "not_found", message = $"GR_NUMBER {grNumber} not found in ADAMO MAP_INITIAL" });
                }

                // Step 2: Transform
                var molecule = _mapper.MapInitialToMolecule(initial);

                Console.WriteLine($"✓ Transformed MAP_INITIAL → Molecule: {grNumber}");

                // Step 3: Write to MAP Tool (if enabled)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    // TODO: Uncomment when ready
                    // Check if exists first
                    // var exists = await _mapToolContext.Molecules.AnyAsync(m => m.GrNumber == grNumber);
                    // if (!exists) {
                    //     await _mapToolContext.Molecules.AddAsync(molecule);
                    //     await _mapToolContext.SaveChangesAsync();
                    // }
                    Console.WriteLine($"[DRY RUN] Would insert Molecule '{grNumber}' to MAP Tool");
                }

                return Ok(new
                {
                    status = "success",
                    message = $"MAP_INITIAL → Molecule transformed for {grNumber}",
                    source = new { database = "ADAMO", table = "MAP_INITIAL", grNumber },
                    transformed = molecule,
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform Initial→Molecule for {GrNumber}", grNumber);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform ADAMO MapSession to MAP Tool Assessment
        /// End-to-end: Fetch from ADAMO by SessionId, transform, optionally write to MAP Tool
        /// </summary>
        [HttpPost("session-to-assessment/{sessionId}")]
        public async Task<IActionResult> TransformSessionToAssessment(long sessionId, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from ADAMO
                var session = await _adamoContext.MapSessions
                    .Include(s => s.Results)
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId);

                if (session == null)
                {
                    return NotFound(new { status = "not_found", message = $"Session {sessionId} not found in ADAMO" });
                }

                if (session.Results == null || !session.Results.Any())
                {
                    return BadRequest(new { status = "fail", message = "Session has no results - cannot transform without result data" });
                }

                // Step 2: Transform
                var firstResult = session.Results.First();
                var assessment = _mapper.MapResultToAssessment(firstResult, session);

                Console.WriteLine($"✓ Transformed MAP_SESSION → Assessment: {sessionId}");

                // Step 3: Write to MAP Tool (if enabled)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    // TODO: Uncomment when ready
                    // var exists = await _mapToolContext.Assessments.AnyAsync(a => a.SessionName == assessment.SessionName);
                    // if (!exists) {
                    //     await _mapToolContext.Assessments.AddAsync(assessment);
                    //     await _mapToolContext.SaveChangesAsync();
                    // }
                    Console.WriteLine($"[DRY RUN] Would insert Assessment '{assessment.SessionName}' to MAP Tool");
                }

                return Ok(new
                {
                    status = "success",
                    message = $"MAP_SESSION → Assessment transformed for session {sessionId}",
                    source = new { database = "ADAMO", table = "MAP_SESSION", sessionId },
                    transformed = assessment,
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform Session→Assessment for {SessionId}", sessionId);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform ADAMO MapResult to MAP Tool Map1_1MoleculeEvaluation
        /// End-to-end: Fetch from ADAMO by ResultId, transform, optionally write to MAP Tool
        /// </summary>
        [HttpPost("result-to-evaluation/{resultId}")]
        public async Task<IActionResult> TransformResultToEvaluation(long resultId, [FromQuery] bool writeToDb = false, [FromQuery] int? evaluationId = null, [FromQuery] int? moleculeId = null)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from ADAMO
                var result = await _adamoContext.MapResults.FindAsync(resultId);
                if (result == null)
                {
                    return NotFound(new { status = "not_found", message = $"Result {resultId} not found in ADAMO" });
                }

                // Step 2: Transform
                // TODO: In production, need to resolve evaluationId and moleculeId from GR_NUMBER
                var moleculeEvaluation = new Map1_1MoleculeEvaluation
                {
                    Map1_1EvaluationId = evaluationId ?? 0, // TODO: Lookup or create evaluation
                    MoleculeId = moleculeId ?? 0, // TODO: Lookup molecule by GR_NUMBER
                    Odor0h = result.Odor,
                    Benchmark = result.BenchmarkComments,
                    ResultCP = result.Result,
                    ResultFF = result.Result,
                    Comment = result.Sponsor,
                    CreatedAt = DateTime.Now,
                    SortOrder = 1
                };

                Console.WriteLine($"✓ Transformed MAP_RESULT → Map1_1MoleculeEvaluation: {result.GrNumber}");

                // Step 3: Write to MAP Tool (if enabled)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    // TODO: Uncomment when ready + implement molecule/evaluation ID resolution
                    // await _mapToolContext.Map1_1MoleculeEvaluations.AddAsync(moleculeEvaluation);
                    // await _mapToolContext.SaveChangesAsync();
                    Console.WriteLine($"[DRY RUN] Would insert MoleculeEvaluation for '{result.GrNumber}' to MAP Tool");
                }

                return Ok(new
                {
                    status = "success",
                    message = $"MAP_RESULT → MoleculeEvaluation transformed",
                    source = new { database = "ADAMO", table = "MAP_RESULT", resultId, grNumber = result.GrNumber },
                    transformed = moleculeEvaluation,
                    note = "TODO: Requires evaluationId and moleculeId resolution from GR_NUMBER",
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform Result→Evaluation for {ResultId}", resultId);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform ADAMO OdorCharacterization to MAP Tool (by ID)
        /// This requires creating multiple OdorDetail records - complex transformation
        /// </summary>
        [HttpPost("odorchar-to-details/{id}")]
        public async Task<IActionResult> TransformOdorCharToDetailsById(long id, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from ADAMO by ID
                var odorChar = await _adamoContext.OdorCharacterizations.FindAsync(id);

                if (odorChar == null)
                {
                    return NotFound(new { status = "not_found", message = $"OdorCharacterization {id} not found in ADAMO" });
                }

                // Step 2: Extract family scores
                var familyScores = _mapper.ExtractOdorFamilyScores(odorChar);

                Console.WriteLine($"✓ Extracted odor family scores for {odorChar.GrNumber} (ID: {id}): {familyScores.Count} families");

                var note = new
                {
                    message = "Complex transformation - requires OdorDetail record creation",
                    steps = new[]
                    {
                        "1. Find or create Map1MoleculeEvaluation for this GR_NUMBER",
                        "2. For each non-null descriptor score in ODOR_CHARACTERIZATION:",
                        "3. Lookup corresponding OdorFamily and OdorDescriptor in MAP Tool",
                        "4. Create OdorDetail record linking evaluation→family→descriptor with score",
                        "5. Repeat for all 100+ descriptor fields"
                    },
                    currentImplementation = "Family scores extracted - OdorDetail creation not yet implemented"
                };

                return Ok(new
                {
                    status = "success",
                    message = $"ODOR_CHARACTERIZATION family scores extracted for {odorChar.GrNumber}",
                    source = new { database = "ADAMO", table = "ODOR_CHARACTERIZATION", id, grNumber = odorChar.GrNumber },
                    familyScores = familyScores,
                    note = note,
                    databaseWrite = "Not implemented - complex transformation required"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform OdorChar for ID {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform ADAMO OdorCharacterization to MAP Tool (by GR_NUMBER)
        /// This requires creating multiple OdorDetail records - complex transformation
        /// </summary>
        [HttpPost("odorchar-to-details/gr/{grNumber}")]
        public async Task<IActionResult> TransformOdorCharToDetailsByGr(string grNumber, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from ADAMO
                var odorChar = await _adamoContext.OdorCharacterizations
                    .FirstOrDefaultAsync(o => o.GrNumber == grNumber);

                if (odorChar == null)
                {
                    return NotFound(new { status = "not_found", message = $"GR_NUMBER {grNumber} not found in ODOR_CHARACTERIZATION" });
                }

                // Step 2: Extract family scores
                var familyScores = _mapper.ExtractOdorFamilyScores(odorChar);

                Console.WriteLine($"✓ Extracted odor family scores for {grNumber}: {familyScores.Count} families");

                // Step 3: Transform to OdorDetail records (requires complex mapping)
                // TODO: Create OdorDetail records for each non-null descriptor
                // This requires:
                // 1. Lookup Map1MoleculeEvaluationId by GR_NUMBER
                // 2. Lookup OdorFamilyId by family code
                // 3. Lookup OdorDescriptorId by descriptor code
                // 4. Create OdorDetail record with Score
                
                var note = new
                {
                    message = "Complex transformation - requires OdorDetail record creation",
                    steps = new[]
                    {
                        "1. Find or create Map1MoleculeEvaluation for this GR_NUMBER",
                        "2. For each non-null descriptor score in ODOR_CHARACTERIZATION:",
                        "3. Lookup corresponding OdorFamily and OdorDescriptor in MAP Tool",
                        "4. Create OdorDetail record linking evaluation→family→descriptor with score",
                        "5. Repeat for all 100+ descriptor fields"
                    },
                    currentImplementation = "Family scores extracted - OdorDetail creation not yet implemented"
                };

                return Ok(new
                {
                    status = "success",
                    message = $"ODOR_CHARACTERIZATION family scores extracted for {grNumber}",
                    source = new { database = "ADAMO", table = "ODOR_CHARACTERIZATION", grNumber },
                    familyScores = familyScores,
                    note = note,
                    databaseWrite = "Not implemented - complex transformation required"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform OdorChar for {GrNumber}", grNumber);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        #endregion

        #region Reverse Transformations (MAP Tool → ADAMO)

        /// <summary>
        /// Transform MAP Tool Molecule to ADAMO MapInitial (by ID)
        /// End-to-end: Fetch from MAP Tool by Molecule Id, transform, optionally write to ADAMO
        /// </summary>
        [HttpPost("molecule-to-initial/{id}")]
        public async Task<IActionResult> TransformMoleculeToInitialById(int id, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from MAP Tool by ID
                var molecule = await _mapToolContext.Molecules.FindAsync(id);
                if (molecule == null)
                {
                    return NotFound(new { status = "not_found", message = $"Molecule {id} not found in MAP Tool" });
                }

                // Step 2: Get evaluation data if available
                var evaluation = await _mapToolContext.Map1_1MoleculeEvaluations
                    .FirstOrDefaultAsync(e => e.MoleculeId == molecule.Id);

                // Step 3: Transform
                var mapInitial = _mapper.MapMoleculeToMapInitial(molecule, evaluation);

                Console.WriteLine($"✓ Transformed Molecule → MAP_INITIAL: {molecule.GrNumber} (ID: {id})");

                // Step 4: Write to ADAMO (if enabled)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    var (success, message) = await _dbService.WriteToAdamo(mapInitial);
                    if (!success)
                    {
                        return StatusCode(500, new { status = "fail", message = $"Write failed: {message}" });
                    }
                }

                return Ok(new
                {
                    status = "success",
                    message = $"Molecule → MAP_INITIAL transformed for {molecule.GrNumber}",
                    source = new { database = "MAP Tool", table = "Molecule", id, grNumber = molecule.GrNumber },
                    transformed = mapInitial,
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform Molecule→Initial for ID {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform MAP Tool Molecule to ADAMO MapInitial (by GR_NUMBER)
        /// End-to-end: Fetch from MAP Tool by GR_NUMBER, transform, optionally write to ADAMO
        /// </summary>
        [HttpPost("molecule-to-initial/gr/{grNumber}")]
        public async Task<IActionResult> TransformMoleculeToInitialByGr(string grNumber, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from MAP Tool
                var molecule = await _mapToolContext.Molecules.FirstOrDefaultAsync(m => m.GrNumber == grNumber);
                if (molecule == null)
                {
                    return NotFound(new { status = "not_found", message = $"GR_NUMBER {grNumber} not found in MAP Tool Molecule" });
                }

                // Step 2: Get evaluation data if available
                var evaluation = await _mapToolContext.Map1_1MoleculeEvaluations
                    .FirstOrDefaultAsync(e => e.MoleculeId == molecule.Id);

                // Step 3: Transform
                var mapInitial = _mapper.MapMoleculeToMapInitial(molecule, evaluation);

                Console.WriteLine($"✓ Transformed Molecule → MAP_INITIAL: {grNumber}");

                // Step 4: Write to ADAMO (if enabled)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    var (success, message) = await _dbService.WriteToAdamo(mapInitial);
                    if (!success)
                    {
                        return StatusCode(500, new { status = "fail", message = $"Write failed: {message}" });
                    }
                }

                return Ok(new
                {
                    status = "success",
                    message = $"Molecule → MAP_INITIAL transformed for {grNumber}",
                    source = new { database = "MAP Tool", table = "Molecule", grNumber },
                    transformed = mapInitial,
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform Molecule→Initial for {GrNumber}", grNumber);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform MAP Tool Assessment to ADAMO MapSession
        /// End-to-end: Fetch from MAP Tool by AssessmentId, transform, optionally write to ADAMO
        /// </summary>
        [HttpPost("assessment-to-session/{assessmentId}")]
        public async Task<IActionResult> TransformAssessmentToSession(int assessmentId, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from MAP Tool
                var assessment = await _mapToolContext.Assessments.FindAsync(assessmentId);
                if (assessment == null)
                {
                    return NotFound(new { status = "not_found", message = $"Assessment {assessmentId} not found in MAP Tool" });
                }

                // Step 2: Transform to ADAMO format
                var session = new MapSession
                {
                    Stage = assessment.Stage,
                    EvaluationDate = assessment.DateTime.Date,
                    Region = assessment.Region,
                    Segment = assessment.Segment,
                    Participants = null, // TODO: Get from Map1_1Evaluation if linked
                    ShowInTaskList = assessment.IsClosed ? "N" : "Y",
                    CreatedBy = assessment.CreatedBy != null ? assessment.CreatedBy.Substring(0, Math.Min(8, assessment.CreatedBy.Length)) : "SYNC",
                    LastModifiedBy = assessment.UpdatedBy != null ? assessment.UpdatedBy.Substring(0, Math.Min(8, assessment.UpdatedBy.Length)) : "SYNC",
                    CreationDate = assessment.CreatedAt,
                    LastModifiedDate = assessment.UpdatedAt
                };

                Console.WriteLine($"✓ Transformed Assessment → MAP_SESSION: {assessment.SessionName}");

                // Step 3: Write to ADAMO (if enabled)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    // TODO: Uncomment when ready
                    // await _adamoContext.MapSessions.AddAsync(session);
                    // await _adamoContext.SaveChangesAsync();
                    Console.WriteLine($"[DRY RUN] Would insert MAP_SESSION for '{assessment.SessionName}' to ADAMO");
                }

                return Ok(new
                {
                    status = "success",
                    message = $"Assessment → MAP_SESSION transformed",
                    source = new { database = "MAP Tool", table = "Assessment", assessmentId },
                    transformed = session,
                    note = "TODO: Participants mapping requires Map1_1Evaluation lookup",
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform Assessment→Session for {AssessmentId}", assessmentId);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform MAP Tool Map1_1Evaluation to ADAMO MapSession
        /// End-to-end: Fetch from MAP Tool by EvaluationId, transform, optionally write to ADAMO
        /// </summary>
        [HttpPost("evaluation-to-session/{evaluationId}")]
        public async Task<IActionResult> TransformEvaluationToSession(int evaluationId, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from MAP Tool
                var evaluation = await _mapToolContext.Map1_1Evaluations
                    .Include(e => e.Assessment)
                    .FirstOrDefaultAsync(e => e.Id == evaluationId);

                if (evaluation == null)
                {
                    return NotFound(new { status = "not_found", message = $"Evaluation {evaluationId} not found in MAP Tool" });
                }

                // Step 2: Transform to ADAMO format
                var session = new MapSession
                {
                    Stage = evaluation.Assessment?.Stage ?? "Unknown",
                    EvaluationDate = evaluation.EvaluationDate,
                    Region = evaluation.Assessment?.Region,
                    Segment = evaluation.Assessment?.Segment,
                    Participants = evaluation.Participants,
                    ShowInTaskList = "N",
                    CreatedBy = evaluation.CreatedBy != null ? evaluation.CreatedBy.Substring(0, Math.Min(8, evaluation.CreatedBy.Length)) : "SYNC",
                    LastModifiedBy = evaluation.UpdatedBy != null ? evaluation.UpdatedBy.Substring(0, Math.Min(8, evaluation.UpdatedBy.Length)) : "SYNC",
                    CreationDate = evaluation.CreatedAt,
                    LastModifiedDate = evaluation.UpdatedAt
                };

                Console.WriteLine($"✓ Transformed Map1_1Evaluation → MAP_SESSION: {evaluationId}");

                // Step 3: Write to ADAMO (if enabled)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    // TODO: Uncomment when ready
                    // await _adamoContext.MapSessions.AddAsync(session);
                    // await _adamoContext.SaveChangesAsync();
                    Console.WriteLine($"[DRY RUN] Would insert MAP_SESSION for evaluation {evaluationId} to ADAMO");
                }

                return Ok(new
                {
                    status = "success",
                    message = $"Map1_1Evaluation → MAP_SESSION transformed",
                    source = new { database = "MAP Tool", table = "Map1_1Evaluation", evaluationId },
                    transformed = session,
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transform Evaluation→Session for {EvaluationId}", evaluationId);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform ADAMO Map1SessionLink to MAP Tool (informational only - no direct equivalent)
        /// Fetches the link data and returns information about linked CP/FF sessions
        /// </summary>
        [HttpPost("sessionlink/adamo/{cpSessionId}/{ffSessionId}")]
        public async Task<IActionResult> TransformSessionLink(long cpSessionId, long ffSessionId)
        {
            if (_adamoContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Oracle not configured" });
            }

            try
            {
                // Step 1: Fetch from ADAMO
                var link = await _adamoContext.Map1SessionLinks
                    .FirstOrDefaultAsync(l => l.CpSessionId == cpSessionId && l.FfSessionId == ffSessionId);

                if (link == null)
                {
                    return NotFound(new { status = "not_found", message = $"SessionLink CP:{cpSessionId}/FF:{ffSessionId} not found" });
                }

                // Step 2: This is a junction table - no direct MAP Tool equivalent
                // Information only - shows the relationship between CP and FF sessions
                Console.WriteLine($"✓ Found MAP1_SESSION_LINK: CP:{cpSessionId} ↔ FF:{ffSessionId}");

                return Ok(new
                {
                    status = "success",
                    message = "MAP1_SESSION_LINK retrieved (junction table - no MAP Tool equivalent)",
                    source = new { database = "ADAMO", table = "MAP1_SESSION_LINK" },
                    data = new
                    {
                        cpSessionId = link.CpSessionId,
                        ffSessionId = link.FfSessionId,
                        note = "This links Consumer Preference and Fine Fragrance sessions in ADAMO. MAP Tool uses different evaluation tracking structure."
                    },
                    transformation = "N/A - Junction table, no direct equivalent in MAP Tool"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get SessionLink");
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Transform ADAMO SubmittingIgnoredMolecules to MAP Tool Molecule status
        /// Fetches ignored molecule and optionally updates Molecule.Status = Ignore in MAP Tool
        /// </summary>
        [HttpPost("ignored-to-molecule/gr/{grNumber}")]
        public async Task<IActionResult> TransformIgnoredMolecule(string grNumber, [FromQuery] bool writeToDb = false)
        {
            if (_adamoContext == null || _mapToolContext == null)
            {
                return StatusCode(503, new { status = "fail", message = "Both databases must be configured" });
            }

            try
            {
                // Step 1: Fetch from ADAMO
                var ignored = await _adamoContext.IgnoredMolecules.FindAsync(grNumber);

                if (ignored == null)
                {
                    return NotFound(new { status = "not_found", message = $"GR_NUMBER {grNumber} not in ADAMO ignored list" });
                }

                // Step 2: Check if molecule exists in MAP Tool
                var molecule = await _mapToolContext.Molecules.FirstOrDefaultAsync(m => m.GrNumber == grNumber);

                Console.WriteLine($"✓ Found ignored molecule: {grNumber} (Entry by: {ignored.EntryPerson})");

                // Step 3: Update molecule status if exists (or create with Ignore status)
                if (_features.EnableDatabaseWrites && writeToDb)
                {
                    if (molecule != null)
                    {
                        // TODO: Uncomment when ready
                        // molecule.Status = MoleculeStatus.Ignore;
                        // await _mapToolContext.SaveChangesAsync();
                        Console.WriteLine($"[DRY RUN] Would set Molecule '{grNumber}' Status = Ignore in MAP Tool");
                    }
                    else
                    {
                        // TODO: Uncomment when ready
                        // Create molecule with Ignore status
                        // var newMolecule = new Molecule { GrNumber = grNumber, Status = MoleculeStatus.Ignore, ... };
                        // await _mapToolContext.Molecules.AddAsync(newMolecule);
                        // await _mapToolContext.SaveChangesAsync();
                        Console.WriteLine($"[DRY RUN] Would create Molecule '{grNumber}' with Status = Ignore in MAP Tool");
                    }
                }

                return Ok(new
                {
                    status = "success",
                    message = $"Ignored molecule {grNumber} processed",
                    source = new { database = "ADAMO", table = "SUBMITTING_IGNORED_MOLECULES" },
                    ignoredInfo = new
                    {
                        grNumber = ignored.GrNumber,
                        entryPerson = ignored.EntryPerson,
                        entryDate = ignored.EntryDate
                    },
                    mapToolMolecule = molecule != null ? "Exists - would update Status to Ignore" : "Not found - would create with Ignore status",
                    note = "MAP Tool has no IgnoredMolecules table - uses Molecule.Status = Ignore instead",
                    databaseWrite = _features.EnableDatabaseWrites && writeToDb ? "[DRY RUN]" : "Disabled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process ignored molecule {GrNumber}", grNumber);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        #endregion
    }

    public class MapToolTransformRequest
    {
        public Molecule? Molecule { get; set; }
        public Map1_1MoleculeEvaluation? Evaluation { get; set; }
        public bool WriteToDatabase { get; set; } = false;
    }

    public class AdamoTransformRequest
    {
        public MapSession? Session { get; set; }
        public MapResult? Result { get; set; }
        public bool WriteToDatabase { get; set; } = false;
    }
}

