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
    }
}

