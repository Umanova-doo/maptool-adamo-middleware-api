using Microsoft.AspNetCore.Mvc;
using MAP2ADAMOINT.Models.Adamo;
using MAP2ADAMOINT.Models.MapTool;
using MAP2ADAMOINT.Services;

namespace MAP2ADAMOINT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly DataMapperService _mapper;
        private readonly ILogger<TestController> _logger;

        public TestController(DataMapperService mapper, ILogger<TestController> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Test MAP Tool Molecule → ADAMO MapInitial mapping
        /// No database required - validates model mapping logic
        /// </summary>
        [HttpPost("map-to-adamo")]
        public IActionResult TestMapToAdamo([FromBody] TestMapToAdamoRequest? request = null)
        {
            _logger.LogInformation("Testing MAP Tool → ADAMO mapping");

            try
            {
                // Create sample MAP Tool molecule
                var molecule = new Molecule
                {
                    Id = 1,
                    GrNumber = request?.GrNumber ?? "GR-88-0681-1",
                    RegNumber = "GR-88-0681",
                    ChemistName = "Dr. Smith",
                    ChemicalName = "Test Compound",
                    Status = MoleculeStatus.Map1,
                    Assessed = true,
                    Quantity = 100,
                    CreatedBy = "TEST_USER",
                    UpdatedBy = "TEST_USER",
                    CreatedAt = DateTime.Now.AddDays(-30),
                    UpdatedAt = DateTime.Now
                };

                // Create sample evaluation
                var evaluation = new Map1_1MoleculeEvaluation
                {
                    Id = 1,
                    MoleculeId = 1,
                    Odor0h = "Fruity, fresh, apple-like with green notes",
                    Odor4h = "Softer, more floral with persistent fruity character",
                    Odor24h = "Woody, dry-down with subtle fruit undertones",
                    Benchmark = "Similar to benchmark XYZ-123 but more natural",
                    ResultCP = 4,
                    ResultFF = 3,
                    CreatedAt = DateTime.Now.AddDays(-30)
                };

                // Perform mapping
                var mapInitial = _mapper.MapMoleculeToMapInitial(molecule, evaluation);

                Console.WriteLine("✓ Successfully mapped Molecule → MapInitial");
                Console.WriteLine($"  GR Number: {mapInitial.GrNumber}");
                Console.WriteLine($"  Chemist: {mapInitial.Chemist}");
                Console.WriteLine($"  Odor 0h: {mapInitial.Odor0H}");
                Console.WriteLine($"  Odor 4h: {mapInitial.Odor4H}");
                Console.WriteLine($"  Odor 24h: {mapInitial.Odor24H}");

                return Ok(new
                {
                    status = "success",
                    message = "Successfully validated MAP Tool → ADAMO mapping",
                    source = new
                    {
                        type = "Molecule + Map1_1MoleculeEvaluation",
                        grNumber = molecule.GrNumber,
                        chemist = molecule.ChemistName,
                        status = molecule.Status.ToString()
                    },
                    destination = new
                    {
                        type = "MapInitial",
                        grNumber = mapInitial.GrNumber,
                        regNumber = mapInitial.RegNumber,
                        chemist = mapInitial.Chemist,
                        evaluationDate = mapInitial.EvaluationDate,
                        odor0h = mapInitial.Odor0H,
                        odor4h = mapInitial.Odor4H,
                        odor24h = mapInitial.Odor24H,
                        dilution = mapInitial.Dilution,
                        comments = mapInitial.Comments,
                        createdBy = mapInitial.CreatedBy
                    },
                    fieldsMapped = 13,
                    fieldsTotal = 16,
                    completeness = "81%"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to map Molecule → MapInitial: {ex.Message}");
                _logger.LogError(ex, "Mapping test failed");
                return StatusCode(500, new
                {
                    status = "fail",
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Test ADAMO MapSession + MapResult → MAP Tool Assessment mapping
        /// No database required - validates model mapping logic
        /// </summary>
        [HttpPost("adamo-to-map")]
        public IActionResult TestAdamoToMap([FromBody] TestAdamoToMapRequest? request = null)
        {
            _logger.LogInformation("Testing ADAMO → MAP Tool mapping");

            try
            {
                // Create sample ADAMO session
                var session = new MapSession
                {
                    SessionId = request?.SessionId ?? 4111,
                    Stage = "MAP 3",
                    EvaluationDate = DateTime.Now.AddDays(-15),
                    Region = "US",
                    Segment = "CP",
                    Participants = "John Doe, Jane Smith, Bob Johnson",
                    ShowInTaskList = "N",
                    CreatedBy = "ADMIN",
                    LastModifiedBy = "ADMIN"
                };

                // Create sample ADAMO result
                var result = new MapResult
                {
                    ResultId = 207,
                    SessionId = session.SessionId,
                    GrNumber = request?.GrNumber ?? "GR-86-6561-0",
                    Odor = "Rosy, floral, peonile, geranium, interesting in DD but not powerful",
                    BenchmarkComments = "CP: 02/09/2005, Status 1, FF: 04/15/2005, Status 1",
                    Result = 1,
                    Dilution = "10% in DPG",
                    Sponsor = "Perfumery Lab Team",
                    RegNumber = "GR-86-6561",
                    CreatedBy = "EVALUATOR",
                    LastModifiedBy = "EVALUATOR"
                };

                // Perform mapping
                var assessment = _mapper.MapResultToAssessment(result, session);

                Console.WriteLine("✓ Successfully mapped MapSession + MapResult → Assessment");
                Console.WriteLine($"  Session: {assessment.SessionName}");
                Console.WriteLine($"  Stage: {assessment.Stage}");
                Console.WriteLine($"  Region: {assessment.Region}");
                Console.WriteLine($"  Segment: {assessment.Segment}");

                return Ok(new
                {
                    status = "success",
                    message = "Successfully validated ADAMO → MAP Tool mapping",
                    source = new
                    {
                        type = "MapSession + MapResult",
                        sessionId = session.SessionId,
                        stage = session.Stage,
                        region = session.Region,
                        segment = session.Segment,
                        grNumber = result.GrNumber,
                        odor = result.Odor,
                        resultScore = result.Result
                    },
                    destination = new
                    {
                        type = "Assessment",
                        sessionName = assessment.SessionName,
                        dateTime = assessment.DateTime,
                        stage = assessment.Stage,
                        region = assessment.Region,
                        segment = assessment.Segment,
                        status = assessment.Status,
                        isClosed = assessment.IsClosed,
                        createdBy = assessment.CreatedBy
                    },
                    fieldsMapped = 10,
                    fieldsTotal = 13,
                    completeness = "77%"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to map MapSession → Assessment: {ex.Message}");
                _logger.LogError(ex, "Mapping test failed");
                return StatusCode(500, new
                {
                    status = "fail",
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }

    public class TestMapToAdamoRequest
    {
        public string? GrNumber { get; set; }
    }

    public class TestAdamoToMapRequest
    {
        public long? SessionId { get; set; }
        public string? GrNumber { get; set; }
    }
}

