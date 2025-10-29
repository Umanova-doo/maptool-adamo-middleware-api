using Microsoft.AspNetCore.Mvc;
using MAP2ADAMOINT.Models.Adamo;
using MAP2ADAMOINT.Models.MapTool;
using MAP2ADAMOINT.Services;
using System.Text.Json;

namespace MAP2ADAMOINT.Controllers
{
    [ApiController]
    [Route("transform")]
    public class TransformController : ControllerBase
    {
        private readonly DataMapperService _mapper;
        private readonly DatabaseService _dbService;
        private readonly FeatureFlags _features;
        private readonly ILogger<TransformController> _logger;

        public TransformController(
            DataMapperService mapper, 
            DatabaseService dbService,
            FeatureFlags features,
            ILogger<TransformController> logger)
        {
            _mapper = mapper;
            _dbService = dbService;
            _features = features;
            _logger = logger;
        }

        /// <summary>
        /// Transform MAP Tool Molecule data to ADAMO MapInitial format
        /// Optionally writes to Oracle database if EnableDatabaseWrites = true
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

                // Perform transformation
                var mapInitial = _mapper.MapMoleculeToMapInitial(request.Molecule, request.Evaluation);

                Console.WriteLine($"✓ Transformed Molecule → MapInitial: {mapInitial.GrNumber}");
                Console.WriteLine($"  Chemist: {mapInitial.Chemist ?? "N/A"}");
                Console.WriteLine($"  Odor 0h: {mapInitial.Odor0H ?? "N/A"}");

                // Optionally write to database if enabled
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
                    message = $"Successfully transformed MAP Tool data to ADAMO format for {mapInitial.GrNumber}",
                    transformed = mapInitial,
                    databaseWrite = _features.EnableDatabaseWrites && request.WriteToDatabase 
                        ? dbWriteMessage 
                        : "Database writes disabled"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed transformation MAP → ADAMO: {ex.Message}");
                _logger.LogError(ex, "Transformation failed");
                return StatusCode(500, new
                {
                    status = "fail",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Transform ADAMO MapSession + MapResult data to MAP Tool Assessment format
        /// Optionally writes to PostgreSQL database if EnableDatabaseWrites = true
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

                // Perform transformation
                var assessment = _mapper.MapResultToAssessment(request.Result, request.Session);

                Console.WriteLine($"✓ Transformed MapSession → Assessment: {assessment.SessionName}");
                Console.WriteLine($"  Stage: {assessment.Stage}");
                Console.WriteLine($"  Region: {assessment.Region}");
                Console.WriteLine($"  Segment: {assessment.Segment}");

                // Optionally write to database if enabled
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
                    message = $"Successfully transformed ADAMO data to MAP Tool format for session {request.Session.SessionId}",
                    transformed = assessment,
                    databaseWrite = _features.EnableDatabaseWrites && request.WriteToDatabase 
                        ? dbWriteMessage 
                        : "Database writes disabled"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed transformation ADAMO → MAP: {ex.Message}");
                _logger.LogError(ex, "Transformation failed");
                return StatusCode(500, new
                {
                    status = "fail",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Request for transforming MAP Tool data to ADAMO format
    /// </summary>
    public class MapToolTransformRequest
    {
        /// <summary>
        /// Molecule data from MAP Tool (REQUIRED)
        /// </summary>
        public Molecule? Molecule { get; set; }

        /// <summary>
        /// Optional evaluation data from MAP Tool
        /// </summary>
        public Map1_1MoleculeEvaluation? Evaluation { get; set; }

        /// <summary>
        /// Whether to write transformed data to Oracle database (requires EnableDatabaseWrites = true)
        /// </summary>
        public bool WriteToDatabase { get; set; } = false;
    }

    /// <summary>
    /// Request for transforming ADAMO data to MAP Tool format
    /// </summary>
    public class AdamoTransformRequest
    {
        /// <summary>
        /// Session data from ADAMO (REQUIRED)
        /// </summary>
        public MapSession? Session { get; set; }

        /// <summary>
        /// Result data from ADAMO (REQUIRED)
        /// </summary>
        public MapResult? Result { get; set; }

        /// <summary>
        /// Whether to write transformed data to PostgreSQL database (requires EnableDatabaseWrites = true)
        /// </summary>
        public bool WriteToDatabase { get; set; } = false;
    }
}

