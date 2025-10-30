using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAP2ADAMOINT.Data;

namespace MAP2ADAMOINT.Controllers
{
    [ApiController]
    [Route("maptool")]
    public class MapToolController : ControllerBase
    {
        private readonly MapToolContext? _mapToolContext;
        private readonly ILogger<MapToolController> _logger;

        public MapToolController(IServiceProvider serviceProvider, ILogger<MapToolController> logger)
        {
            _logger = logger;
            _mapToolContext = serviceProvider.GetService<MapToolContext>();
        }

        /// <summary>
        /// Get Molecule by Id
        /// </summary>
        [HttpGet("molecule/{id}")]
        public async Task<IActionResult> GetMolecule(int id)
        {
            if (_mapToolContext == null) return StatusCode(503, new { status = "fail", message = "PostgreSQL not configured" });

            try
            {
                var record = await _mapToolContext.Molecules.FindAsync(id);
                if (record == null) return NotFound(new { status = "not_found", message = $"Molecule {id} not found" });

                Console.WriteLine($"✓ Found Molecule: {record.GrNumber} (ID: {id})");
                return Ok(new { status = "success", table = "Molecule", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Molecule {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get Molecule by GR_NUMBER
        /// </summary>
        [HttpGet("molecule/gr/{grNumber}")]
        public async Task<IActionResult> GetMoleculeByGr(string grNumber)
        {
            if (_mapToolContext == null) return StatusCode(503, new { status = "fail", message = "PostgreSQL not configured" });

            try
            {
                var record = await _mapToolContext.Molecules.FirstOrDefaultAsync(m => m.GrNumber == grNumber);
                if (record == null) return NotFound(new { status = "not_found", message = $"GR_NUMBER {grNumber} not found in Molecule" });

                Console.WriteLine($"✓ Found Molecule: {record.GrNumber} (ID: {record.Id})");
                return Ok(new { status = "success", table = "Molecule", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to lookup Molecule by GR_NUMBER {GrNumber}", grNumber);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get Assessment by Id
        /// </summary>
        [HttpGet("assessment/{id}")]
        public async Task<IActionResult> GetAssessment(int id)
        {
            if (_mapToolContext == null) return StatusCode(503, new { status = "fail", message = "PostgreSQL not configured" });

            try
            {
                var record = await _mapToolContext.Assessments
                    .Include(a => a.Map1_1Evaluations)
                    .FirstOrDefaultAsync(a => a.Id == id);
                
                if (record == null) return NotFound(new { status = "not_found", message = $"Assessment {id} not found" });

                Console.WriteLine($"✓ Found Assessment: {record.SessionName} (ID: {id})");
                return Ok(new { status = "success", table = "Assessment", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Assessment {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get Map1_1Evaluation by Id
        /// </summary>
        [HttpGet("evaluation/{id}")]
        public async Task<IActionResult> GetEvaluation(int id)
        {
            if (_mapToolContext == null) return StatusCode(503, new { status = "fail", message = "PostgreSQL not configured" });

            try
            {
                var record = await _mapToolContext.Map1_1Evaluations
                    .Include(e => e.Assessment)
                    .Include(e => e.MoleculeEvaluations)
                    .FirstOrDefaultAsync(e => e.Id == id);
                
                if (record == null) return NotFound(new { status = "not_found", message = $"Evaluation {id} not found" });

                Console.WriteLine($"✓ Found Map1_1Evaluation: {id} ({record.MoleculeEvaluations?.Count ?? 0} molecules)");
                return Ok(new { status = "success", table = "Map1_1Evaluation", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Evaluation {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get Map1_1MoleculeEvaluation by Id
        /// </summary>
        [HttpGet("moleculeevaluation/{id}")]
        public async Task<IActionResult> GetMoleculeEvaluation(int id)
        {
            if (_mapToolContext == null) return StatusCode(503, new { status = "fail", message = "PostgreSQL not configured" });

            try
            {
                var record = await _mapToolContext.Map1_1MoleculeEvaluations
                    .Include(me => me.Molecule)
                    .Include(me => me.Map1_1Evaluation)
                    .FirstOrDefaultAsync(me => me.Id == id);
                
                if (record == null) return NotFound(new { status = "not_found", message = $"MoleculeEvaluation {id} not found" });

                Console.WriteLine($"✓ Found Map1_1MoleculeEvaluation: {id} - {record.Molecule?.GrNumber}");
                return Ok(new { status = "success", table = "Map1_1MoleculeEvaluation", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get MoleculeEvaluation {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get OdorFamily by Id
        /// </summary>
        [HttpGet("odorfamily/{id}")]
        public async Task<IActionResult> GetOdorFamily(int id)
        {
            if (_mapToolContext == null) return StatusCode(503, new { status = "fail", message = "PostgreSQL not configured" });

            try
            {
                var record = await _mapToolContext.OdorFamilies
                    .Include(f => f.OdorDescriptors)
                    .FirstOrDefaultAsync(f => f.Id == id);
                
                if (record == null) return NotFound(new { status = "not_found", message = $"OdorFamily {id} not found" });

                Console.WriteLine($"✓ Found OdorFamily: {record.Name} ({record.OdorDescriptors?.Count ?? 0} descriptors)");
                return Ok(new { status = "success", table = "OdorFamily", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get OdorFamily {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }

        /// <summary>
        /// Get OdorDescriptor by Id
        /// </summary>
        [HttpGet("odordescriptor/{id}")]
        public async Task<IActionResult> GetOdorDescriptor(int id)
        {
            if (_mapToolContext == null) return StatusCode(503, new { status = "fail", message = "PostgreSQL not configured" });

            try
            {
                var record = await _mapToolContext.OdorDescriptors
                    .Include(d => d.OdorFamily)
                    .FirstOrDefaultAsync(d => d.Id == id);
                
                if (record == null) return NotFound(new { status = "not_found", message = $"OdorDescriptor {id} not found" });

                Console.WriteLine($"✓ Found OdorDescriptor: {record.Name} (Family: {record.OdorFamily?.Name ?? "N/A"})");
                return Ok(new { status = "success", table = "OdorDescriptor", data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get OdorDescriptor {Id}", id);
                return StatusCode(500, new { status = "fail", error = ex.Message });
            }
        }
    }
}

