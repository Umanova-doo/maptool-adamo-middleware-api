using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MAP2ADAMOINT.Data;

namespace MAP2ADAMOINT.Controllers
{
    [ApiController]
    [Route("lookup")]
    public class LookupController : ControllerBase
    {
        private readonly AdamoContext? _adamoContext;
        private readonly MapToolContext? _mapToolContext;
        private readonly ILogger<LookupController> _logger;

        public LookupController(
            IServiceProvider serviceProvider,
            ILogger<LookupController> logger)
        {
            _logger = logger;
            _adamoContext = serviceProvider.GetService<AdamoContext>();
            _mapToolContext = serviceProvider.GetService<MapToolContext>();
        }

        /// <summary>
        /// Lookup molecule by GR_NUMBER in ADAMO MAP_INITIAL table
        /// </summary>
        /// <param name="grNumber">GR Number to search for (e.g., GR-88-0681-1)</param>
        /// <returns>MapInitial record if found</returns>
        [HttpGet("adamo/gr/{grNumber}")]
        public async Task<IActionResult> LookupAdamoByGrNumber(string grNumber)
        {
            _logger.LogInformation("Lookup ADAMO MAP_INITIAL by GR_NUMBER: {GrNumber}", grNumber);

            if (_adamoContext == null)
            {
                return StatusCode(503, new
                {
                    status = "fail",
                    message = "Oracle (ADAMO) database not configured"
                });
            }

            try
            {
                var mapInitial = await _adamoContext.MapInitials
                    .FirstOrDefaultAsync(m => m.GrNumber == grNumber);

                if (mapInitial == null)
                {
                    Console.WriteLine($"⚠ GR_NUMBER '{grNumber}' not found in ADAMO MAP_INITIAL");
                    return NotFound(new
                    {
                        status = "not_found",
                        message = $"GR_NUMBER '{grNumber}' not found in ADAMO database",
                        grNumber = grNumber,
                        table = "GIV_MAP.MAP_INITIAL"
                    });
                }

                Console.WriteLine($"✓ Found MAP_INITIAL record for GR_NUMBER: {grNumber}");
                Console.WriteLine($"  Chemist: {mapInitial.Chemist ?? "N/A"}");
                Console.WriteLine($"  Evaluation Date: {mapInitial.EvaluationDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
                Console.WriteLine($"  Odor 0h: {mapInitial.Odor0H ?? "N/A"}");

                return Ok(new
                {
                    status = "success",
                    message = $"Found MAP_INITIAL record for {grNumber}",
                    grNumber = grNumber,
                    database = "ADAMO (Oracle)",
                    table = "GIV_MAP.MAP_INITIAL",
                    data = new
                    {
                        mapInitial.MapInitialId,
                        mapInitial.GrNumber,
                        mapInitial.RegNumber,
                        mapInitial.Batch,
                        mapInitial.EvaluationDate,
                        mapInitial.Chemist,
                        mapInitial.Assessor,
                        mapInitial.Dilution,
                        mapInitial.EvaluationSite,
                        mapInitial.Odor0H,
                        mapInitial.Odor4H,
                        mapInitial.Odor24H,
                        mapInitial.Comments,
                        mapInitial.CreationDate,
                        mapInitial.CreatedBy,
                        mapInitial.LastModifiedDate,
                        mapInitial.LastModifiedBy
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to lookup GR_NUMBER '{grNumber}': {ex.Message}");
                _logger.LogError(ex, "Database lookup failed for GR_NUMBER: {GrNumber}", grNumber);

                return StatusCode(500, new
                {
                    status = "fail",
                    message = "Database query failed",
                    grNumber = grNumber,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lookup molecule by GR_NUMBER in MAP Tool Molecule table
        /// </summary>
        /// <param name="grNumber">GR Number to search for</param>
        /// <returns>Molecule record if found</returns>
        [HttpGet("maptool/gr/{grNumber}")]
        public async Task<IActionResult> LookupMapToolByGrNumber(string grNumber)
        {
            _logger.LogInformation("Lookup MAP Tool Molecule by GR_NUMBER: {GrNumber}", grNumber);

            if (_mapToolContext == null)
            {
                return StatusCode(503, new
                {
                    status = "fail",
                    message = "PostgreSQL (MAP Tool) database not configured"
                });
            }

            try
            {
                var molecule = await _mapToolContext.Molecules
                    .FirstOrDefaultAsync(m => m.GrNumber == grNumber);

                if (molecule == null)
                {
                    Console.WriteLine($"⚠ GR_NUMBER '{grNumber}' not found in MAP Tool Molecule table");
                    return NotFound(new
                    {
                        status = "not_found",
                        message = $"GR_NUMBER '{grNumber}' not found in MAP Tool database",
                        grNumber = grNumber,
                        table = "map_adm.Molecule"
                    });
                }

                Console.WriteLine($"✓ Found Molecule record for GR_NUMBER: {grNumber}");
                Console.WriteLine($"  Chemist: {molecule.ChemistName ?? "N/A"}");
                Console.WriteLine($"  Status: {molecule.Status}");

                return Ok(new
                {
                    status = "success",
                    message = $"Found Molecule record for {grNumber}",
                    grNumber = grNumber,
                    database = "MAP Tool (PostgreSQL)",
                    table = "map_adm.Molecule",
                    data = new
                    {
                        molecule.Id,
                        molecule.GrNumber,
                        molecule.RegNumber,
                        molecule.Structure,
                        molecule.Assessed,
                        molecule.ChemistName,
                        molecule.ChemicalName,
                        molecule.MolecularFormula,
                        molecule.ProjectName,
                        molecule.Status,
                        molecule.Quantity,
                        molecule.CreatedAt,
                        molecule.CreatedBy,
                        molecule.UpdatedAt,
                        molecule.UpdatedBy,
                        molecule.IsArchived
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to lookup GR_NUMBER '{grNumber}': {ex.Message}");
                _logger.LogError(ex, "Database lookup failed for GR_NUMBER: {GrNumber}", grNumber);

                return StatusCode(500, new
                {
                    status = "fail",
                    message = "Database query failed",
                    grNumber = grNumber,
                    error = ex.Message
                });
            }
        }
    }
}

