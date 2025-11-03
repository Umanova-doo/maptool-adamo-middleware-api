using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAP2ADAMOINT.Data;

namespace MAP2ADAMOINT.Controllers
{
    [ApiController]
    [Route("adamo")]
    public class AdamoController : ControllerBase
    {
        private readonly AdamoContext? _adamoContext;
        private readonly ILogger<AdamoController> _logger;

        public AdamoController(IServiceProvider serviceProvider, ILogger<AdamoController> logger)
        {
            _logger = logger;
            _adamoContext = serviceProvider.GetService<AdamoContext>();
        }

        /// <summary>
        /// Get MAP_INITIAL by MapInitialId
        /// </summary>
        [HttpGet("initial/{id}")]
        public async Task<IActionResult> GetMapInitial(long id)
        {
            if (_adamoContext == null) return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                var record = await _adamoContext.MapInitials.FindAsync(id);
                if (record == null) return NotFound(new { status = "not_found", message = $"MapInitial {id} not found" });

                Console.WriteLine($"✓ Found MAP_INITIAL: {record.GrNumber}");
                return Ok(new { status = "success", table = "MAP_INITIAL", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get MapInitial {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get MAP_INITIAL by GR_NUMBER
        /// </summary>
        [HttpGet("initial/gr/{grNumber}")]
        public async Task<IActionResult> GetMapInitialByGr(string grNumber)
        {
            if (_adamoContext == null) return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                var record = await _adamoContext.MapInitials.FirstOrDefaultAsync(m => m.GrNumber == grNumber);
                if (record == null) return NotFound(new { status = "not_found", message = $"GR_NUMBER {grNumber} not found in MAP_INITIAL" });

                Console.WriteLine($"✓ Found MAP_INITIAL: {record.GrNumber} (ID: {record.MapInitialId})");
                return Ok(new { status = "success", table = "MAP_INITIAL", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to lookup GR_NUMBER {GrNumber}", grNumber);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get MAP_SESSION by SessionId
        /// </summary>
        [HttpGet("session/{id}")]
        public async Task<IActionResult> GetMapSession(long id)
        {
            if (_adamoContext == null) return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                var record = await _adamoContext.MapSessions
                    .Include(s => s.Results)
                    .FirstOrDefaultAsync(s => s.SessionId == id);
                
                if (record == null) return NotFound(new { status = "not_found", message = $"Session {id} not found" });

                Console.WriteLine($"✓ Found MAP_SESSION: {id} - {record.Stage} ({record.Results?.Count ?? 0} results)");
                return Ok(new { status = "success", table = "MAP_SESSION", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Session {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get MAP_RESULT by ResultId
        /// </summary>
        [HttpGet("result/{id}")]
        public async Task<IActionResult> GetMapResult(long id)
        {
            if (_adamoContext == null) return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                var record = await _adamoContext.MapResults.FindAsync(id);
                if (record == null) return NotFound(new { status = "not_found", message = $"Result {id} not found" });

                Console.WriteLine($"✓ Found MAP_RESULT: {id} - {record.GrNumber}");
                return Ok(new { status = "success", table = "MAP_RESULT", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Result {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get ODOR_CHARACTERIZATION by OdorCharacterizationId
        /// </summary>
        [HttpGet("odor/{id}")]
        public async Task<IActionResult> GetOdorCharacterization(long id)
        {
            if (_adamoContext == null) return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                var record = await _adamoContext.OdorCharacterizations.FindAsync(id);
                if (record == null) return NotFound(new { status = "not_found", message = $"OdorCharacterization {id} not found" });

                Console.WriteLine($"✓ Found ODOR_CHARACTERIZATION: {record.GrNumber}");
                return Ok(new { status = "success", table = "ODOR_CHARACTERIZATION", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get OdorCharacterization {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get ODOR_CHARACTERIZATION by GR_NUMBER
        /// </summary>
        [HttpGet("odor/gr/{grNumber}")]
        public async Task<IActionResult> GetOdorCharacterizationByGr(string grNumber)
        {
            if (_adamoContext == null) return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                var record = await _adamoContext.OdorCharacterizations.FirstOrDefaultAsync(o => o.GrNumber == grNumber);
                if (record == null) return NotFound(new { status = "not_found", message = $"GR_NUMBER {grNumber} not found in ODOR_CHARACTERIZATION" });

                Console.WriteLine($"✓ Found ODOR_CHARACTERIZATION: {record.GrNumber} (ID: {record.OdorCharacterizationId})");
                return Ok(new { status = "success", table = "ODOR_CHARACTERIZATION", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to lookup OdorCharacterization by GR_NUMBER {GrNumber}", grNumber);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get MAP_ODOR_FAMILY by Id
        /// </summary>
        [HttpGet("odorfamily/{id}")]
        public async Task<IActionResult> GetOdorFamily(long id)
        {
            if (_adamoContext == null) return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                var record = await _adamoContext.OdorFamilies
                    .Include(f => f.Descriptors)
                    .FirstOrDefaultAsync(f => f.Id == id);
                
                if (record == null) return NotFound(new { status = "not_found", message = $"OdorFamily {id} not found" });

                Console.WriteLine($"✓ Found MAP_ODOR_FAMILY: {record.Name} ({record.Descriptors?.Count ?? 0} descriptors)");
                return Ok(new { status = "success", table = "MAP_ODOR_FAMILY", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get OdorFamily {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get MAP_ODOR_DESCRIPTOR by Id
        /// </summary>
        [HttpGet("odordescriptor/{id}")]
        public async Task<IActionResult> GetOdorDescriptor(long id)
        {
            if (_adamoContext == null) return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                var record = await _adamoContext.OdorDescriptors
                    .Include(d => d.Family)
                    .FirstOrDefaultAsync(d => d.Id == id);
                
                if (record == null) return NotFound(new { status = "not_found", message = $"OdorDescriptor {id} not found" });

                Console.WriteLine($"✓ Found MAP_ODOR_DESCRIPTOR: {record.Name} (Family: {record.Family?.Name ?? "N/A"})");
                return Ok(new { status = "success", table = "MAP_ODOR_DESCRIPTOR", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get OdorDescriptor {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get MAP1_SESSION_LINK by CP and FF session IDs
        /// </summary>
        [HttpGet("sessionlink/{cpSessionId}/{ffSessionId}")]
        public async Task<IActionResult> GetSessionLink(long cpSessionId, long ffSessionId)
        {
            if (_adamoContext == null) return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                var record = await _adamoContext.Map1SessionLinks
                    .FirstOrDefaultAsync(l => l.CpSessionId == cpSessionId && l.FfSessionId == ffSessionId);
                
                if (record == null) return NotFound(new { status = "not_found", message = $"SessionLink CP:{cpSessionId}/FF:{ffSessionId} not found" });

                Console.WriteLine($"✓ Found MAP1_SESSION_LINK: CP:{cpSessionId} ↔ FF:{ffSessionId}");
                return Ok(new { status = "success", table = "MAP1_SESSION_LINK", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get SessionLink");
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get SUBMITTING_IGNORED_MOLECULES by GR_NUMBER
        /// </summary>
        [HttpGet("ignored/{grNumber}")]
        public async Task<IActionResult> GetIgnoredMolecule(string grNumber)
        {
            if (_adamoContext == null) return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                var record = await _adamoContext.IgnoredMolecules.FindAsync(grNumber);
                if (record == null) return NotFound(new { status = "not_found", message = $"GR_NUMBER {grNumber} not in ignored list" });

                Console.WriteLine($"✓ Found SUBMITTING_IGNORED_MOLECULES: {record.GrNumber} (Ignored by: {record.EntryPerson})");
                return Ok(new { status = "success", table = "SUBMITTING_IGNORED_MOLECULES", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get IgnoredMolecule {GrNumber}", grNumber);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new MAP_INITIAL record
        /// POST /adamo/initial
        /// </summary>
        [HttpPost("initial")]
        public async Task<IActionResult> CreateMapInitial([FromBody] Models.DTOs.CreateMapInitialRequest request)
        {
            if (_adamoContext == null) 
                return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { status = "fail", message = "Validation failed", errors });
                }

                // Check if GR_NUMBER already exists
                var existing = await _adamoContext.MapInitials
                    .FirstOrDefaultAsync(m => m.GrNumber == request.GrNumber);
                
                if (existing != null)
                {
                    return Conflict(new 
                    { 
                        status = "fail", 
                        message = $"MAP_INITIAL record with GR_NUMBER '{request.GrNumber}' already exists",
                        existingId = existing.MapInitialId
                    });
                }

                // Create new MAP_INITIAL entity
                var mapInitial = new Models.Adamo.MapInitial
                {
                    GrNumber = request.GrNumber,
                    EvaluationDate = request.EvaluationDate,
                    Chemist = request.Chemist,
                    Assessor = request.Assessor,
                    Dilution = request.Dilution,
                    EvaluationSite = request.EvaluationSite,
                    Odor0H = request.Odor0H,
                    Odor4H = request.Odor4H,
                    Odor24H = request.Odor24H,
                    Comments = request.Comments,
                    CreatedBy = request.CreatedBy,
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = request.CreatedBy
                    // Note: MapInitialId, RegNumber, and Batch are auto-generated by database triggers
                };

                await _adamoContext.MapInitials.AddAsync(mapInitial);
                await _adamoContext.SaveChangesAsync();

                // Reload to get the auto-generated values
                await _adamoContext.Entry(mapInitial).ReloadAsync();

                _logger.LogInformation("Created MAP_INITIAL record for GR_NUMBER {GrNumber} with ID {Id}", 
                    mapInitial.GrNumber, mapInitial.MapInitialId);

                Console.WriteLine($"✓ Created MAP_INITIAL: {mapInitial.GrNumber} (ID: {mapInitial.MapInitialId})");

                return CreatedAtAction(
                    nameof(GetMapInitial), 
                    new { id = mapInitial.MapInitialId }, 
                    new 
                    { 
                        status = "success", 
                        message = "MAP_INITIAL record created successfully",
                        table = "MAP_INITIAL",
                        data = mapInitial 
                    }
                );
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating MAP_INITIAL for GR_NUMBER {GrNumber}", request.GrNumber);
                return StatusCode(500, new 
                { 
                    status = "fail", 
                    message = "Database error occurred while creating MAP_INITIAL record",
                    error = ex.InnerException?.Message ?? ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MAP_INITIAL for GR_NUMBER {GrNumber}", request.GrNumber);
                return StatusCode(500, new 
                { 
                    status = "fail", 
                    message = "An error occurred while creating MAP_INITIAL record",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Create a new MAP_SESSION record
        /// POST /adamo/session
        /// </summary>
        [HttpPost("session")]
        public async Task<IActionResult> CreateMapSession([FromBody] Models.DTOs.CreateMapSessionRequest request)
        {
            if (_adamoContext == null) 
                return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { status = "fail", message = "Validation failed", errors });
                }

                // Validate Stage if provided
                if (!string.IsNullOrWhiteSpace(request.Stage) && 
                    !Models.DTOs.MapStages.IsValidStage(request.Stage))
                {
                    return BadRequest(new 
                    { 
                        status = "fail", 
                        message = $"Invalid stage value: '{request.Stage}'",
                        validStages = Models.DTOs.MapStages.AllStages
                    });
                }

                // Create new MAP_SESSION entity
                var mapSession = new Models.Adamo.MapSession
                {
                    Stage = request.Stage,
                    EvaluationDate = request.EvaluationDate,
                    Region = request.Region,
                    Segment = request.Segment,
                    Participants = request.Participants,
                    ShowInTaskList = string.IsNullOrWhiteSpace(request.ShowInTaskList) 
                        ? "N" 
                        : request.ShowInTaskList.ToUpper(),
                    SubStage = request.SubStage,
                    Category = request.Category,
                    CreatedBy = request.CreatedBy,
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = request.CreatedBy
                    // Note: SessionId is auto-generated by database sequence
                };

                await _adamoContext.MapSessions.AddAsync(mapSession);
                await _adamoContext.SaveChangesAsync();

                // Reload to get the auto-generated values
                await _adamoContext.Entry(mapSession).ReloadAsync();

                _logger.LogInformation("Created MAP_SESSION with ID {SessionId}, Stage: {Stage}, Segment: {Segment}", 
                    mapSession.SessionId, mapSession.Stage, mapSession.Segment);

                Console.WriteLine($"✓ Created MAP_SESSION: ID {mapSession.SessionId} - {mapSession.Stage} - {mapSession.Segment}");

                return CreatedAtAction(
                    nameof(GetMapSession), 
                    new { id = mapSession.SessionId }, 
                    new 
                    { 
                        status = "success", 
                        message = "MAP_SESSION record created successfully",
                        table = "MAP_SESSION",
                        data = mapSession 
                    }
                );
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating MAP_SESSION");
                return StatusCode(500, new 
                { 
                    status = "fail", 
                    message = "Database error occurred while creating MAP_SESSION record",
                    error = ex.InnerException?.Message ?? ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MAP_SESSION");
                return StatusCode(500, new 
                { 
                    status = "fail", 
                    message = "An error occurred while creating MAP_SESSION record",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Create a new MAP_RESULT record
        /// POST /adamo/result
        /// </summary>
        [HttpPost("result")]
        public async Task<IActionResult> CreateMapResult([FromBody] Models.DTOs.CreateMapResultRequest request)
        {
            if (_adamoContext == null) 
                return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { status = "fail", message = "Validation failed", errors });
                }

                // Check if SESSION_ID exists
                var sessionExists = await _adamoContext.MapSessions
                    .AnyAsync(s => s.SessionId == request.SessionId);
                
                if (!sessionExists)
                {
                    return NotFound(new 
                    { 
                        status = "fail", 
                        message = $"MAP_SESSION with SESSION_ID {request.SessionId} not found",
                        hint = "Create the session first using POST /adamo/session or POST /adamo/session-with-results"
                    });
                }

                // Create new MAP_RESULT entity
                var mapResult = new Models.Adamo.MapResult
                {
                    SessionId = request.SessionId,
                    GrNumber = request.GrNumber,
                    Odor = request.Odor,
                    BenchmarkComments = request.BenchmarkComments,
                    Result = request.Result,
                    Dilution = request.Dilution,
                    Sponsor = request.Sponsor,
                    CreatedBy = request.CreatedBy,
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = request.CreatedBy
                    // Note: ResultId, RegNumber, and Batch are auto-generated by database
                };

                await _adamoContext.MapResults.AddAsync(mapResult);
                await _adamoContext.SaveChangesAsync();

                // Reload to get the auto-generated values
                await _adamoContext.Entry(mapResult).ReloadAsync();

                _logger.LogInformation("Created MAP_RESULT with ID {ResultId} for SESSION {SessionId}, GR_NUMBER {GrNumber}", 
                    mapResult.ResultId, mapResult.SessionId, mapResult.GrNumber);

                Console.WriteLine($"✓ Created MAP_RESULT: ID {mapResult.ResultId} - Session {mapResult.SessionId} - {mapResult.GrNumber}");

                return CreatedAtAction(
                    nameof(GetMapResult), 
                    new { id = mapResult.ResultId }, 
                    new 
                    { 
                        status = "success", 
                        message = "MAP_RESULT record created successfully",
                        table = "MAP_RESULT",
                        data = mapResult 
                    }
                );
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error creating MAP_RESULT for SESSION_ID {SessionId}", request.SessionId);
                return StatusCode(500, new 
                { 
                    status = "fail", 
                    message = "Database error occurred while creating MAP_RESULT record",
                    error = ex.InnerException?.Message ?? ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MAP_RESULT for SESSION_ID {SessionId}", request.SessionId);
                return StatusCode(500, new 
                { 
                    status = "fail", 
                    message = "An error occurred while creating MAP_RESULT record",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Create a MAP_SESSION with multiple MAP_RESULT records in one transaction
        /// POST /adamo/session-with-results
        /// </summary>
        [HttpPost("session-with-results")]
        public async Task<IActionResult> CreateSessionWithResults([FromBody] Models.DTOs.CreateSessionWithResultsRequest request)
        {
            if (_adamoContext == null) 
                return StatusCode(503, new { status = "fail", message = "Oracle not configured" });

            using var transaction = await _adamoContext.Database.BeginTransactionAsync();
            
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { status = "fail", message = "Validation failed", errors });
                }

                // Validate Stage if provided
                if (!string.IsNullOrWhiteSpace(request.Session.Stage) && 
                    !Models.DTOs.MapStages.IsValidStage(request.Session.Stage))
                {
                    return BadRequest(new 
                    { 
                        status = "fail", 
                        message = $"Invalid stage value: '{request.Session.Stage}'",
                        validStages = Models.DTOs.MapStages.AllStages
                    });
                }

                // Create MAP_SESSION
                var mapSession = new Models.Adamo.MapSession
                {
                    Stage = request.Session.Stage,
                    EvaluationDate = request.Session.EvaluationDate,
                    Region = request.Session.Region,
                    Segment = request.Session.Segment,
                    Participants = request.Session.Participants,
                    ShowInTaskList = string.IsNullOrWhiteSpace(request.Session.ShowInTaskList) 
                        ? "N" 
                        : request.Session.ShowInTaskList.ToUpper(),
                    SubStage = request.Session.SubStage,
                    Category = request.Session.Category,
                    CreatedBy = request.Session.CreatedBy,
                    CreationDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = request.Session.CreatedBy
                };

                await _adamoContext.MapSessions.AddAsync(mapSession);
                await _adamoContext.SaveChangesAsync();

                // Reload to get the auto-generated SESSION_ID
                await _adamoContext.Entry(mapSession).ReloadAsync();

                _logger.LogInformation("Created MAP_SESSION with ID {SessionId}", mapSession.SessionId);

                // Create MAP_RESULT records
                var mapResults = new List<Models.Adamo.MapResult>();
                foreach (var resultItem in request.Results)
                {
                    var mapResult = new Models.Adamo.MapResult
                    {
                        SessionId = mapSession.SessionId,
                        GrNumber = resultItem.GrNumber,
                        Odor = resultItem.Odor,
                        BenchmarkComments = resultItem.BenchmarkComments,
                        Result = resultItem.Result,
                        Dilution = resultItem.Dilution,
                        Sponsor = resultItem.Sponsor,
                        CreatedBy = request.Session.CreatedBy,
                        CreationDate = DateTime.UtcNow,
                        LastModifiedDate = DateTime.UtcNow,
                        LastModifiedBy = request.Session.CreatedBy
                    };

                    await _adamoContext.MapResults.AddAsync(mapResult);
                    mapResults.Add(mapResult);
                }

                await _adamoContext.SaveChangesAsync();

                // Reload all results to get auto-generated values
                foreach (var result in mapResults)
                {
                    await _adamoContext.Entry(result).ReloadAsync();
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Created MAP_SESSION {SessionId} with {Count} MAP_RESULT records", 
                    mapSession.SessionId, mapResults.Count);

                Console.WriteLine($"✓ Created MAP_SESSION: ID {mapSession.SessionId} with {mapResults.Count} results");

                return CreatedAtAction(
                    nameof(GetMapSession), 
                    new { id = mapSession.SessionId }, 
                    new 
                    { 
                        status = "success", 
                        message = $"MAP_SESSION created with {mapResults.Count} results successfully",
                        table = "MAP_SESSION + MAP_RESULT",
                        data = new
                        {
                            session = mapSession,
                            results = mapResults,
                            resultCount = mapResults.Count
                        }
                    }
                );
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database error creating MAP_SESSION with results");
                return StatusCode(500, new 
                { 
                    status = "fail", 
                    message = "Database error occurred while creating session with results. Transaction rolled back.",
                    error = ex.InnerException?.Message ?? ex.Message 
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating MAP_SESSION with results");
                return StatusCode(500, new 
                { 
                    status = "fail", 
                    message = "An error occurred while creating session with results. Transaction rolled back.",
                    error = ex.Message 
                });
            }
        }
    }
}

