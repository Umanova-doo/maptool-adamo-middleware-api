using Microsoft.AspNetCore.Mvc;
using MAP2ADAMOINT.Services;

namespace MAP2ADAMOINT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MigrationController : ControllerBase
    {
        private readonly MigrationService _migrationService;
        private readonly FeatureFlags _features;
        private readonly ILogger<MigrationController> _logger;

        public MigrationController(
            MigrationService migrationService,
            FeatureFlags features,
            ILogger<MigrationController> logger)
        {
            _migrationService = migrationService;
            _features = features;
            _logger = logger;
        }

        /// <summary>
        /// ONE-TIME bulk migration from ADAMO (Oracle) to MAP Tool (PostgreSQL)
        /// Simple GET endpoint - just trigger the migration with default settings
        /// Requires EnableMigration = true in configuration
        /// </summary>
        [HttpGet("adamo-to-maptool")]
        public async Task<IActionResult> MigrateAdamoToMapToolGet()
        {
            if (!_features.EnableMigration)
            {
                _logger.LogWarning("Migration endpoint called but feature is disabled");
                return StatusCode(403, new
                {
                    status = "fail",
                    message = "Migration feature is disabled. Set 'DatabaseFeatures:EnableMigration' to true in configuration."
                });
            }

            // Use default options for simple GET
            var options = new MigrationOptions
            {
                BatchSize = 1000,
                MigrateOdorFamilies = true,
                MigrateOdorDescriptors = true,
                MigrateInitialData = true,
                MigrateOdorCharacterizations = true,
                MigrateIgnoredMolecules = false
            };

            _logger.LogInformation("GET Migration triggered with default settings");

            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  MIGRATION TRIGGERED VIA GET");
            Console.WriteLine("  Using default settings (batch: 1000, all entity types)");
            Console.WriteLine("═══════════════════════════════════════════════════════");

            return await ExecuteMigration(options);
        }

        /// <summary>
        /// ONE-TIME bulk migration from ADAMO (Oracle) to MAP Tool (PostgreSQL)
        /// POST endpoint with custom options
        /// Requires EnableMigration = true in configuration
        /// </summary>
        [HttpPost("adamo-to-maptool")]
        public async Task<IActionResult> MigrateAdamoToMapToolPost([FromBody] MigrationOptions? options = null)
        {
            if (!_features.EnableMigration)
            {
                _logger.LogWarning("Migration endpoint called but feature is disabled");
                return StatusCode(403, new
                {
                    status = "fail",
                    message = "Migration feature is disabled. Set 'DatabaseFeatures:EnableMigration' to true in configuration."
                });
            }

            options ??= new MigrationOptions();

            _logger.LogInformation("POST Migration request - BatchSize: {BatchSize}, StageFilter: {Stage}", 
                options.BatchSize, options.StageFilter ?? "ALL");

            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  MIGRATION TRIGGERED VIA POST");
            Console.WriteLine($"  Batch Size: {options.BatchSize}");
            Console.WriteLine($"  Stage Filter: {options.StageFilter ?? "ALL"}");
            Console.WriteLine($"  Migrate Molecules: {options.MigrateInitialData}");
            Console.WriteLine("═══════════════════════════════════════════════════════");

            return await ExecuteMigration(options);
        }

        /// <summary>
        /// Shared migration execution logic for both GET and POST endpoints
        /// </summary>
        private async Task<IActionResult> ExecuteMigration(MigrationOptions options)
        {
            var result = await _migrationService.MigrateAdamoToMapTool(options);

            if (result.Success)
            {
                Console.WriteLine("✓ Migration completed successfully");
                Console.WriteLine($"  Sessions migrated: {result.SessionsMigrated}/{result.SessionsFound}");
                Console.WriteLine($"  Molecules migrated: {result.MoleculesMigrated}/{result.MoleculesFound}");
                Console.WriteLine($"  Duration: {result.Duration.TotalSeconds:F2}s");

                return Ok(new
                {
                    status = "success",
                    message = "Migration completed successfully",
                    duration = $"{result.Duration.TotalSeconds:F2}s",
                    statistics = new
                    {
                        odorFamilies = new
                        {
                            found = result.OdorFamiliesFound,
                            migrated = result.OdorFamiliesMigrated,
                            skipped = result.OdorFamiliesSkipped
                        },
                        odorDescriptors = new
                        {
                            found = result.OdorDescriptorsFound,
                            migrated = result.OdorDescriptorsMigrated,
                            skipped = result.OdorDescriptorsSkipped
                        },
                        molecules = new
                        {
                            found = result.MoleculesFound,
                            migrated = result.MoleculesMigrated,
                            skipped = result.MoleculesSkipped
                        },
                        assessments = new
                        {
                            found = result.SessionsFound,
                            migrated = result.SessionsMigrated,
                            skipped = result.SessionsSkipped
                        },
                        odorCharacterizations = new
                        {
                            found = result.OdorCharsFound,
                            migrated = result.OdorCharsMigrated,
                            skipped = result.OdorCharsSkipped,
                            note = "Complex - requires 100+ OdorDetail records per characterization"
                        },
                        ignoredMolecules = new
                        {
                            found = result.IgnoredMoleculesFound,
                            migrated = result.IgnoredMoleculesMigrated,
                            skipped = result.IgnoredMoleculesSkipped
                        },
                        totalErrors = result.Errors.Count
                    },
                    errors = result.Errors.Any() ? result.Errors : null
                });
            }
            else
            {
                Console.WriteLine($"✗ Migration failed: {result.ErrorMessage}");
                Console.WriteLine($"  Errors encountered: {result.Errors.Count}");

                return StatusCode(500, new
                {
                    status = "fail",
                    message = result.ErrorMessage ?? "Migration failed",
                    statistics = new
                    {
                        sessionsMigrated = result.SessionsMigrated,
                        moleculesMigrated = result.MoleculesMigrated,
                        errorsCount = result.Errors.Count
                    },
                    errors = result.Errors
                });
            }
        }
    }
}

