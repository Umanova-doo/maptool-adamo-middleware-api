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
                // Test connection by executing a simple query
                var canConnect = await _mapToolContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    Console.WriteLine("✗ PostgreSQL: Cannot connect to database");
                    return StatusCode(500, new
                    {
                        status = "fail",
                        message = "Cannot connect to PostgreSQL database"
                    });
                }

                // Try to query molecules (may not exist yet)
                var molecules = new List<dynamic>();
                try
                {
                    var moleculeData = await _mapToolContext.Molecules
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
                    molecules = moleculeData.Cast<dynamic>().ToList();
                }
                catch (Exception tableEx)
                {
                    // Table doesn't exist - that's okay for demo
                    Console.WriteLine($"⚠ PostgreSQL connected but Molecule table not found (empty DB): {tableEx.Message}");
                }

                Console.WriteLine($"✓ PostgreSQL connection successful - Database exists, {molecules.Count} molecules found");

                return Ok(new
                {
                    status = "success",
                    message = "PostgreSQL connection working",
                    database = "MAP Tool (PostgreSQL)",
                    connection = "map2-postgres:5432/MAP23",
                    schema = "map_adm",
                    table = "Molecule",
                    connectionStatus = "CONNECTED ✓",
                    recordsFound = molecules.Count,
                    note = molecules.Count == 0 ? "Database connected successfully but table is empty or doesn't exist yet" : null,
                    sampleData = molecules.Take(3).ToList()
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
                // Test connection
                var canConnect = await _adamoContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    Console.WriteLine("✗ Oracle: Cannot connect to database");
                    return StatusCode(500, new
                    {
                        status = "fail",
                        message = "Cannot connect to Oracle database"
                    });
                }

                // Try to query sessions (may not exist yet)
                var sessions = new List<dynamic>();
                try
                {
                    var sessionData = await _adamoContext.MapSessions
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
                    sessions = sessionData.Cast<dynamic>().ToList();
                }
                catch (Exception tableEx)
                {
                    // Table doesn't exist - that's okay for demo
                    Console.WriteLine($"⚠ Oracle connected but MAP_SESSION table not found (empty DB): {tableEx.Message}");
                }

                Console.WriteLine($"✓ Oracle connection successful - Database exists, {sessions.Count} sessions found");

                return Ok(new
                {
                    status = "success",
                    message = "Oracle connection working",
                    database = "ADAMO (Oracle)",
                    connection = "oracle-map-db:1521/FREEPDB1",
                    schema = "GIV_MAP",
                    table = "MAP_SESSION",
                    connectionStatus = "CONNECTED ✓",
                    recordsFound = sessions.Count,
                    note = sessions.Count == 0 ? "Database connected successfully but table is empty or doesn't exist yet" : null,
                    sampleData = sessions.Take(3).ToList()
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

