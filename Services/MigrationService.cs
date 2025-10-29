using MAP2ADAMOINT.Data;
using MAP2ADAMOINT.Models.Adamo;
using MAP2ADAMOINT.Models.MapTool;
using Microsoft.EntityFrameworkCore;

namespace MAP2ADAMOINT.Services
{
    /// <summary>
    /// Handles one-time bulk data migration from ADAMO to MAP Tool
    /// Only enabled when EnableMigration = true
    /// </summary>
    public class MigrationService
    {
        private readonly MapToolContext? _mapToolContext;
        private readonly AdamoContext? _adamoContext;
        private readonly DataMapperService _mapper;
        private readonly ILogger<MigrationService> _logger;

        public MigrationService(
            IServiceProvider serviceProvider,
            DataMapperService mapper,
            ILogger<MigrationService> logger)
        {
            _mapper = mapper;
            _logger = logger;
            
            // Try to get contexts if they're registered
            _mapToolContext = serviceProvider.GetService<MapToolContext>();
            _adamoContext = serviceProvider.GetService<AdamoContext>();
        }

        /// <summary>
        /// Migrate all data from ADAMO (Oracle) to MAP Tool (PostgreSQL)
        /// ONE-TIME bulk transfer operation
        /// </summary>
        public async Task<MigrationResult> MigrateAdamoToMapTool(MigrationOptions options)
        {
            _logger.LogInformation("Starting migration from ADAMO to MAP Tool - BatchSize: {BatchSize}", options.BatchSize);

            var result = new MigrationResult
            {
                StartTime = DateTime.UtcNow
            };

            if (_adamoContext == null)
            {
                result.Success = false;
                result.ErrorMessage = "Oracle (ADAMO) database not configured";
                return result;
            }

            if (_mapToolContext == null)
            {
                result.Success = false;
                result.ErrorMessage = "PostgreSQL (MAP Tool) database not configured";
                return result;
            }

            try
            {
                // Step 1: Migrate Sessions → Assessments
                await MigrateSessions(options, result);

                // Step 2: Migrate MAP_INITIAL → Molecules (if requested)
                if (options.MigrateInitialData)
                {
                    await MigrateInitialData(options, result);
                }

                result.Success = result.Errors.Count == 0;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;

                _logger.LogInformation("Migration completed - Success: {Success}, Sessions: {Sessions}, Molecules: {Molecules}, Errors: {Errors}",
                    result.Success, result.SessionsMigrated, result.MoleculesMigrated, result.Errors.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration failed");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;
                return result;
            }
        }

        private async Task MigrateSessions(MigrationOptions options, MigrationResult result)
        {
            _logger.LogInformation("Migrating sessions from ADAMO...");

            var query = _adamoContext!.MapSessions.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(options.StageFilter))
            {
                query = query.Where(s => s.Stage == options.StageFilter);
            }

            if (options.AfterDate.HasValue)
            {
                query = query.Where(s => s.EvaluationDate >= options.AfterDate.Value);
            }

            var sessions = await query
                .Include(s => s.Results)
                .OrderBy(s => s.SessionId)
                .Take(options.BatchSize)
                .ToListAsync();

            result.SessionsFound = sessions.Count;

            foreach (var session in sessions)
            {
                try
                {
                    // Check if already migrated
                    var exists = await _mapToolContext!.Assessments
                        .AnyAsync(a => a.SessionName == $"ADAMO-{session.SessionId}");

                    if (exists)
                    {
                        result.SessionsSkipped++;
                        continue;
                    }

                    if (session.Results != null && session.Results.Any())
                    {
                        var firstResult = session.Results.First();
                        var assessment = _mapper.MapResultToAssessment(firstResult, session);

                        // TODO: Uncomment when ready for production
                        // await _mapToolContext.Assessments.AddAsync(assessment);
                        // await _mapToolContext.SaveChangesAsync();

                        result.SessionsMigrated++;
                        _logger.LogInformation("[DRY RUN] Would migrate session {SessionId}", session.SessionId);
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Session {session.SessionId}: {ex.Message}");
                    _logger.LogError(ex, "Failed to migrate session {SessionId}", session.SessionId);
                }
            }
        }

        private async Task MigrateInitialData(MigrationOptions options, MigrationResult result)
        {
            _logger.LogInformation("Migrating initial data from ADAMO...");

            var query = _adamoContext!.MapInitials.AsQueryable();

            if (options.AfterDate.HasValue)
            {
                query = query.Where(m => m.EvaluationDate >= options.AfterDate.Value);
            }

            var initials = await query
                .OrderBy(m => m.MapInitialId)
                .Take(options.BatchSize)
                .ToListAsync();

            result.MoleculesFound = initials.Count;

            foreach (var initial in initials)
            {
                try
                {
                    // Check if already exists
                    var exists = await _mapToolContext!.Molecules
                        .AnyAsync(m => m.GrNumber == initial.GrNumber);

                    if (exists)
                    {
                        result.MoleculesSkipped++;
                        continue;
                    }

                    var molecule = _mapper.MapInitialToMolecule(initial);

                    // TODO: Uncomment when ready for production
                    // await _mapToolContext.Molecules.AddAsync(molecule);
                    // await _mapToolContext.SaveChangesAsync();

                    result.MoleculesMigrated++;
                    _logger.LogInformation("[DRY RUN] Would migrate molecule {GrNumber}", initial.GrNumber);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Molecule {initial.GrNumber}: {ex.Message}");
                    _logger.LogError(ex, "Failed to migrate molecule {GrNumber}", initial.GrNumber);
                }
            }
        }
    }

    public class MigrationOptions
    {
        public int BatchSize { get; set; } = 1000;
        public string? StageFilter { get; set; }
        public DateTime? AfterDate { get; set; }
        public bool MigrateInitialData { get; set; } = true;
    }

    public class MigrationResult
    {
        public bool Success { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int SessionsFound { get; set; }
        public int SessionsMigrated { get; set; }
        public int SessionsSkipped { get; set; }
        public int MoleculesFound { get; set; }
        public int MoleculesMigrated { get; set; }
        public int MoleculesSkipped { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? ErrorMessage { get; set; }
    }
}

