using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAP2ADAMOINT.Data;

namespace MAP2ADAMOINT.Controllers
{
    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly MapToolContext? _mapToolContext;
        private readonly AdamoContext? _adamoContext;
        private readonly ILogger<DebugController> _logger;

        public DebugController(
            IServiceProvider serviceProvider,
            ILogger<DebugController> logger)
        {
            _logger = logger;
            _mapToolContext = serviceProvider.GetService<MapToolContext>();
            _adamoContext = serviceProvider.GetService<AdamoContext>();
        }

        /// <summary>
        /// Test PostgreSQL connection - Get sample molecules from MAP Tool
        /// </summary>
        [HttpGet("test-postgres")]
        public async Task<IActionResult> TestPostgres()
        {
            _logger.LogInformation("Testing PostgreSQL connection");

            if (_mapToolContext == null)
            {
                return StatusCode(503, new 
                { 
                    status = "fail", 
                    message = "PostgreSQL not configured. Check appsettings.json ConnectionStrings:MapToolDb" 
                });
            }

            try
            {
                // Query first 5 molecules
                var molecules = await _mapToolContext.Molecules
                    .Where(m => !m.IsArchived)
                    .OrderByDescending(m => m.Id)
                    .Take(5)
                    .Select(m => new
                    {
                        m.Id,
                        m.GrNumber,
                        m.RegNumber,
                        m.ChemistName,
                        m.Status,
                        m.Assessed,
                        m.CreatedAt
                    })
                    .ToListAsync();

                Console.WriteLine($"✓ PostgreSQL connection successful - Found {molecules.Count} molecules");

                return Ok(new
                {
                    status = "success",
                    message = "PostgreSQL connection working",
                    database = "MAP Tool (PostgreSQL)",
                    connection = "host.docker.internal:5433/MAP23",
                    table = "Molecule",
                    recordsFound = molecules.Count,
                    sampleData = molecules
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ PostgreSQL connection failed: {ex.Message}");
                _logger.LogError(ex, "PostgreSQL connection test failed");
                
                return StatusCode(500, new
                {
                    status = "fail",
                    message = "PostgreSQL connection failed",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Test Oracle connection - Get sample sessions from ADAMO
        /// </summary>
        [HttpGet("test-oracle")]
        public async Task<IActionResult> TestOracle()
        {
            _logger.LogInformation("Testing Oracle connection");

            if (_adamoContext == null)
            {
                return StatusCode(503, new 
                { 
                    status = "fail", 
                    message = "Oracle not configured. Check appsettings.json ConnectionStrings:AdamoDb" 
                });
            }

            try
            {
                // Query first 5 sessions
                var sessions = await _adamoContext.MapSessions
                    .OrderByDescending(s => s.SessionId)
                    .Take(5)
                    .Select(s => new
                    {
                        s.SessionId,
                        s.Stage,
                        s.EvaluationDate,
                        s.Region,
                        s.Segment,
                        s.Participants
                    })
                    .ToListAsync();

                Console.WriteLine($"✓ Oracle connection successful - Found {sessions.Count} sessions");

                return Ok(new
                {
                    status = "success",
                    message = "Oracle connection working",
                    database = "ADAMO (Oracle)",
                    connection = "host.docker.internal:4040/FREEPDB1",
                    schema = "GIV_MAP",
                    table = "MAP_SESSION",
                    recordsFound = sessions.Count,
                    sampleData = sessions
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Oracle connection failed: {ex.Message}");
                _logger.LogError(ex, "Oracle connection test failed");
                
                return StatusCode(500, new
                {
                    status = "fail",
                    message = "Oracle connection failed",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Test both database connections at once
        /// </summary>
        [HttpGet("test-both")]
        public async Task<IActionResult> TestBoth()
        {
            _logger.LogInformation("Testing both database connections");

            var results = new
            {
                postgres = new { status = "not_configured", message = "", recordCount = 0 },
                oracle = new { status = "not_configured", message = "", recordCount = 0 }
            };

            // Test PostgreSQL
            if (_mapToolContext != null)
            {
                try
                {
                    var count = await _mapToolContext.Molecules.CountAsync();
                    results = results with 
                    { 
                        postgres = new 
                        { 
                            status = "connected", 
                            message = "PostgreSQL working", 
                            recordCount = count 
                        } 
                    };
                    Console.WriteLine($"✓ PostgreSQL: {count} molecules");
                }
                catch (Exception ex)
                {
                    results = results with 
                    { 
                        postgres = new 
                        { 
                            status = "error", 
                            message = ex.Message, 
                            recordCount = 0 
                        } 
                    };
                    Console.WriteLine($"✗ PostgreSQL failed: {ex.Message}");
                }
            }

            // Test Oracle
            if (_adamoContext != null)
            {
                try
                {
                    var count = await _adamoContext.MapSessions.CountAsync();
                    results = results with 
                    { 
                        oracle = new 
                        { 
                            status = "connected", 
                            message = "Oracle working", 
                            recordCount = count 
                        } 
                    };
                    Console.WriteLine($"✓ Oracle: {count} sessions");
                }
                catch (Exception ex)
                {
                    results = results with 
                    { 
                        oracle = new 
                        { 
                            status = "error", 
                            message = ex.Message, 
                            recordCount = 0 
                        } 
                    };
                    Console.WriteLine($"✗ Oracle failed: {ex.Message}");
                }
            }

            var bothWorking = results.postgres.status == "connected" && results.oracle.status == "connected";

            return Ok(new
            {
                status = bothWorking ? "success" : "partial",
                message = bothWorking ? "Both databases connected successfully" : "Check individual database status",
                databases = results
            });
        }
    }
}

