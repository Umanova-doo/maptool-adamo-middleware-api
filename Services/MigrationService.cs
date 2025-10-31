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
        /// ONE-TIME bulk transfer operation - processes ALL entity types
        /// </summary>
        public async Task<MigrationResult> MigrateAdamoToMapTool(MigrationOptions options)
        {
            _logger.LogInformation("Starting COMPREHENSIVE migration from ADAMO to MAP Tool");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  BULK MIGRATION: ADAMO (Oracle) → MAP Tool (PostgreSQL)");
            Console.WriteLine("═══════════════════════════════════════════════════════");

            var result = new MigrationResult { StartTime = DateTime.UtcNow };

            if (_adamoContext == null || _mapToolContext == null)
            {
                result.Success = false;
                result.ErrorMessage = "Both databases must be configured";
                return result;
            }

            try
            {
                // STEP 1: Migrate Reference Data First (Order matters!)
                Console.WriteLine("\n[1/6] Migrating Odor Families...");
                await MigrateOdorFamilies(options, result);

                Console.WriteLine("\n[2/6] Migrating Odor Descriptors...");
                await MigrateOdorDescriptors(options, result);

                // STEP 2: Migrate Core Molecule Data
                Console.WriteLine("\n[3/6] Migrating MAP_INITIAL → Molecules...");
                await MigrateInitialData(options, result);

                // STEP 3: Migrate Session Data
                Console.WriteLine("\n[4/6] Migrating MAP_SESSION → Assessments...");
                await MigrateSessions(options, result);

                // STEP 4: Migrate Odor Characterizations (complex)
                Console.WriteLine("\n[5/6] Migrating Odor Characterizations...");
                await MigrateOdorCharacterizations(options, result);

                // STEP 5: Migrate Ignored Molecules List
                Console.WriteLine("\n[6/6] Migrating Ignored Molecules...");
                await MigrateIgnoredMolecules(options, result);

                result.Success = result.Errors.Count == 0;
                result.EndTime = DateTime.UtcNow;
                result.Duration = result.EndTime - result.StartTime;

                Console.WriteLine("\n═══════════════════════════════════════════════════════");
                Console.WriteLine($"  MIGRATION COMPLETE - Duration: {result.Duration.TotalSeconds:F2}s");
                Console.WriteLine($"  OdorFamilies: {result.OdorFamiliesMigrated}");
                Console.WriteLine($"  OdorDescriptors: {result.OdorDescriptorsMigrated}");
                Console.WriteLine($"  Molecules: {result.MoleculesMigrated}");
                Console.WriteLine($"  Assessments: {result.SessionsMigrated}");
                Console.WriteLine($"  OdorCharacterizations: {result.OdorCharsMigrated}");
                Console.WriteLine($"  Errors: {result.Errors.Count}");
                Console.WriteLine("═══════════════════════════════════════════════════════");

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

        #region Migration Methods for Each Entity Type

        private async Task MigrateOdorFamilies(MigrationOptions options, MigrationResult result)
        {
            try
            {
                var families = await _adamoContext!.OdorFamilies.Take(options.BatchSize).ToListAsync();
                result.OdorFamiliesFound = families.Count;

                foreach (var family in families)
                {
                    try
                    {
                        // Check if exists
                        var exists = await _mapToolContext!.OdorFamilies.AnyAsync(f => f.Code == family.Code);
                        if (exists)
                        {
                            result.OdorFamiliesSkipped++;
                            continue;
                        }

                        var mapToolFamily = new Models.MapTool.OdorFamily
                        {
                            Name = family.Name,
                            Color = family.Color,
                            Code = family.Code,
                            CreatedBy = "MIGRATION",
                            CreatedAt = DateTime.Now
                        };

                        // TODO: Uncomment when ready for production
                        // await _mapToolContext.OdorFamilies.AddAsync(mapToolFamily);
                        
                        result.OdorFamiliesMigrated++;
                        _logger.LogInformation("[DRY RUN] Would migrate OdorFamily: {Name}", family.Name);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"OdorFamily {family.Name}: {ex.Message}");
                    }
                }

                // TODO: Uncomment when ready
                // await _mapToolContext.SaveChangesAsync();
                Console.WriteLine($"  Migrated {result.OdorFamiliesMigrated}/{result.OdorFamiliesFound} families");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate OdorFamilies");
                result.Errors.Add($"OdorFamilies migration: {ex.Message}");
            }
        }

        private async Task MigrateOdorDescriptors(MigrationOptions options, MigrationResult result)
        {
            try
            {
                var descriptors = await _adamoContext!.OdorDescriptors
                    .Include(d => d.Family)
                    .Take(options.BatchSize)
                    .ToListAsync();
                
                result.OdorDescriptorsFound = descriptors.Count;

                foreach (var descriptor in descriptors)
                {
                    try
                    {
                        var exists = await _mapToolContext!.OdorDescriptors.AnyAsync(d => d.Code == descriptor.Code);
                        if (exists)
                        {
                            result.OdorDescriptorsSkipped++;
                            continue;
                        }

                        // TODO: Lookup MAP Tool OdorFamilyId by family code
                        var mapToolFamilyId = 1; // Placeholder - needs family code lookup

                        var mapToolDescriptor = new Models.MapTool.OdorDescriptor
                        {
                            Name = descriptor.Name,
                            ProfileName = descriptor.ProfileName,
                            Code = descriptor.Code,
                            OdorFamilyId = mapToolFamilyId, // TODO: Lookup by family code
                            CreatedBy = "MIGRATION",
                            CreatedAt = DateTime.Now
                        };

                        // TODO: Uncomment when ready + implement family ID resolution
                        // await _mapToolContext.OdorDescriptors.AddAsync(mapToolDescriptor);
                        
                        result.OdorDescriptorsMigrated++;
                        _logger.LogInformation("[DRY RUN] Would migrate OdorDescriptor: {Name}", descriptor.Name);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"OdorDescriptor {descriptor.Name}: {ex.Message}");
                    }
                }

                // TODO: Uncomment when ready
                // await _mapToolContext.SaveChangesAsync();
                Console.WriteLine($"  Migrated {result.OdorDescriptorsMigrated}/{result.OdorDescriptorsFound} descriptors");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate OdorDescriptors");
                result.Errors.Add($"OdorDescriptors migration: {ex.Message}");
            }
        }

        private async Task MigrateOdorCharacterizations(MigrationOptions options, MigrationResult result)
        {
            try
            {
                var query = _adamoContext!.OdorCharacterizations.AsQueryable();

                if (options.AfterDate.HasValue)
                {
                    query = query.Where(o => o.CreationDate >= options.AfterDate.Value);
                }

                var odorChars = await query.Take(options.BatchSize).ToListAsync();
                result.OdorCharsFound = odorChars.Count;

                foreach (var odorChar in odorChars)
                {
                    try
                    {
                        // TODO: Complex transformation
                        // For each ODOR_CHARACTERIZATION record:
                        // 1. Find or create Molecule in MAP Tool
                        // 2. Find or create Map1MoleculeEvaluation
                        // 3. Create OdorDetail records for each non-null descriptor score
                        // 4. Link OdorDetail → OdorDescriptor → OdorFamily
                        
                        // This requires:
                        // - Molecule lookup/creation by GR_NUMBER
                        // - Evaluation creation
                        // - Up to 100+ OdorDetail records per characterization
                        
                        _logger.LogInformation("[DRY RUN] Would migrate OdorCharacterization for {GrNumber} (complex - 100+ descriptor fields)", odorChar.GrNumber);
                        result.OdorCharsMigrated++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"OdorChar {odorChar.GrNumber}: {ex.Message}");
                    }
                }

                Console.WriteLine($"  Migrated {result.OdorCharsMigrated}/{result.OdorCharsFound} odor characterizations");
                Console.WriteLine($"  Note: Each requires 100+ OdorDetail records (not yet implemented)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate OdorCharacterizations");
                result.Errors.Add($"OdorCharacterizations migration: {ex.Message}");
            }
        }

        private async Task MigrateIgnoredMolecules(MigrationOptions options, MigrationResult result)
        {
            try
            {
                var ignored = await _adamoContext!.IgnoredMolecules.Take(options.BatchSize).ToListAsync();
                result.IgnoredMoleculesFound = ignored.Count;

                foreach (var molecule in ignored)
                {
                    try
                    {
                        // TODO: MAP Tool doesn't have equivalent "IgnoredMolecules" table
                        // Options:
                        // 1. Create new table in MAP Tool
                        // 2. Set Molecule.Status = Ignore (if molecule exists)
                        // 3. Add to comments field
                        // 4. Skip this data (ADAMO-specific)

                        _logger.LogInformation("[DRY RUN] Would handle ignored molecule: {GrNumber} (no equivalent MAP Tool table)", molecule.GrNumber);
                        result.IgnoredMoleculesMigrated++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"IgnoredMolecule {molecule.GrNumber}: {ex.Message}");
                    }
                }

                Console.WriteLine($"  Migrated {result.IgnoredMoleculesMigrated}/{result.IgnoredMoleculesFound} ignored molecules");
                Console.WriteLine($"  Note: MAP Tool has no equivalent table - may need to create or set Molecule.Status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate IgnoredMolecules");
                result.Errors.Add($"IgnoredMolecules migration: {ex.Message}");
            }
        }

        #endregion
    }

    public class MigrationOptions
    {
        public int BatchSize { get; set; } = 1000;
        public string? StageFilter { get; set; }
        public DateTime? AfterDate { get; set; }
        public bool MigrateInitialData { get; set; } = true;
        public bool MigrateOdorFamilies { get; set; } = true;
        public bool MigrateOdorDescriptors { get; set; } = true;
        public bool MigrateOdorCharacterizations { get; set; } = true;
        public bool MigrateIgnoredMolecules { get; set; } = false;
    }

    public class MigrationResult
    {
        public bool Success { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        
        // Reference data
        public int OdorFamiliesFound { get; set; }
        public int OdorFamiliesMigrated { get; set; }
        public int OdorFamiliesSkipped { get; set; }
        
        public int OdorDescriptorsFound { get; set; }
        public int OdorDescriptorsMigrated { get; set; }
        public int OdorDescriptorsSkipped { get; set; }
        
        // Core data
        public int SessionsFound { get; set; }
        public int SessionsMigrated { get; set; }
        public int SessionsSkipped { get; set; }
        
        public int MoleculesFound { get; set; }
        public int MoleculesMigrated { get; set; }
        public int MoleculesSkipped { get; set; }
        
        // Complex data
        public int OdorCharsFound { get; set; }
        public int OdorCharsMigrated { get; set; }
        public int OdorCharsSkipped { get; set; }
        
        public int IgnoredMoleculesFound { get; set; }
        public int IgnoredMoleculesMigrated { get; set; }
        public int IgnoredMoleculesSkipped { get; set; }
        
        public List<string> Errors { get; set; } = new List<string>();
        public string? ErrorMessage { get; set; }
    }
}

