using Microsoft.AspNetCore.Mvc;
using MAP2ADAMOINT.Services;
using MAP2ADAMOINT.Models.DTOs;

namespace MAP2ADAMOINT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly SyncService _syncService;
        private readonly ILogger<SyncController> _logger;

        public SyncController(SyncService syncService, ILogger<SyncController> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        /// <summary>
        /// Pull data from MAP Tool (Postgres) → map → send to ADAMO (Oracle)
        /// </summary>
        /// <param name="request">Sync configuration and filters</param>
        /// <returns>Sync result with status and records processed</returns>
        [HttpPost("from-map")]
        [ProducesResponseType(typeof(SyncResponse), 200)]
        [ProducesResponseType(typeof(SyncResponse), 500)]
        public async Task<IActionResult> SyncFromMap([FromBody] SyncFromMapRequest? request = null)
        {
            // Use default request if none provided (backwards compatibility)
            request ??= new SyncFromMapRequest();

            _logger.LogInformation("POST /sync/from-map requested - BatchSize: {BatchSize}, DryRun: {DryRun}", 
                request.BatchSize, request.DryRun);

            var response = await _syncService.SyncFromMapToAdamo(request);

            if (response.Status == "success")
            {
                Console.WriteLine($"✓ Sync from MAP to ADAMO completed. Synced: {response.RecordsSynced}, Skipped: {response.RecordsSkipped}");
                return Ok(response);
            }
            else
            {
                Console.WriteLine($"✗ Sync from MAP to ADAMO failed: {response.Message}");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Pull data from ADAMO (Oracle) → map → send to MAP Tool (Postgres)
        /// </summary>
        /// <param name="request">Sync configuration and filters</param>
        /// <returns>Sync result with status and records processed</returns>
        [HttpPost("from-adamo")]
        [ProducesResponseType(typeof(SyncResponse), 200)]
        [ProducesResponseType(typeof(SyncResponse), 500)]
        public async Task<IActionResult> SyncFromAdamo([FromBody] SyncFromAdamoRequest? request = null)
        {
            // Use default request if none provided (backwards compatibility)
            request ??= new SyncFromAdamoRequest();

            _logger.LogInformation("POST /sync/from-adamo requested - BatchSize: {BatchSize}, DryRun: {DryRun}", 
                request.BatchSize, request.DryRun);

            var response = await _syncService.SyncFromAdamoToMap(request);

            if (response.Status == "success")
            {
                Console.WriteLine($"✓ Sync from ADAMO to MAP completed. Synced: {response.RecordsSynced}, Skipped: {response.RecordsSkipped}");
                return Ok(response);
            }
            else
            {
                Console.WriteLine($"✗ Sync from ADAMO to MAP failed: {response.Message}");
                return StatusCode(500, response);
            }
        }
    }
}

