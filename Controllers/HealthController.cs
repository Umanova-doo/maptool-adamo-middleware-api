using Microsoft.AspNetCore.Mvc;

namespace MAP2ADAMOINT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint - returns OK to confirm service is running
        /// </summary>
        /// <returns>OK status</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Health check requested");
            return Ok(new { status = "OK", service = "MAP2ADAMOINT", timestamp = DateTime.UtcNow });
        }
    }
}

