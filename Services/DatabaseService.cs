using MAP2ADAMOINT.Data;
using MAP2ADAMOINT.Models.Adamo;
using MAP2ADAMOINT.Models.MapTool;
using Microsoft.EntityFrameworkCore;

namespace MAP2ADAMOINT.Services
{
    /// <summary>
    /// Handles actual database write operations
    /// Only used when EnableDatabaseWrites = true
    /// </summary>
    public class DatabaseService
    {
        private readonly MapToolContext? _mapToolContext;
        private readonly AdamoContext? _adamoContext;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(
            IServiceProvider serviceProvider,
            ILogger<DatabaseService> logger)
        {
            _logger = logger;
            
            // Try to get contexts if they're registered
            _mapToolContext = serviceProvider.GetService<MapToolContext>();
            _adamoContext = serviceProvider.GetService<AdamoContext>();
        }

        /// <summary>
        /// Write MapInitial to ADAMO Oracle database
        /// </summary>
        public async Task<(bool Success, string Message)> WriteToAdamo(MapInitial mapInitial)
        {
            if (_adamoContext == null)
            {
                return (false, "Oracle database not configured");
            }

            try
            {
                // Check if already exists
                var exists = await _adamoContext.MapInitials
                    .AnyAsync(m => m.GrNumber == mapInitial.GrNumber);

                if (exists)
                {
                    _logger.LogWarning("MapInitial {GrNumber} already exists in ADAMO", mapInitial.GrNumber);
                    return (false, $"GR Number {mapInitial.GrNumber} already exists in ADAMO");
                }

                // TODO: Uncomment when ready for production writes
                // await _adamoContext.MapInitials.AddAsync(mapInitial);
                // await _adamoContext.SaveChangesAsync();
                
                _logger.LogInformation("[DRY RUN] Would insert MapInitial {GrNumber} to ADAMO", mapInitial.GrNumber);
                return (true, $"[DRY RUN] Would insert {mapInitial.GrNumber} to ADAMO Oracle database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write to ADAMO");
                return (false, $"Database error: {ex.Message}");
            }
        }

        /// <summary>
        /// Write Assessment to MAP Tool PostgreSQL database
        /// </summary>
        public async Task<(bool Success, string Message)> WriteToMapTool(Assessment assessment)
        {
            if (_mapToolContext == null)
            {
                return (false, "PostgreSQL database not configured");
            }

            try
            {
                // Check if already exists
                var exists = await _mapToolContext.Assessments
                    .AnyAsync(a => a.SessionName == assessment.SessionName);

                if (exists)
                {
                    _logger.LogWarning("Assessment {SessionName} already exists in MAP Tool", assessment.SessionName);
                    return (false, $"Session {assessment.SessionName} already exists in MAP Tool");
                }

                // TODO: Uncomment when ready for production writes
                // await _mapToolContext.Assessments.AddAsync(assessment);
                // await _mapToolContext.SaveChangesAsync();
                
                _logger.LogInformation("[DRY RUN] Would insert Assessment {SessionName} to MAP Tool", assessment.SessionName);
                return (true, $"[DRY RUN] Would insert {assessment.SessionName} to MAP Tool PostgreSQL database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write to MAP Tool");
                return (false, $"Database error: {ex.Message}");
            }
        }
    }
}

