using MAP2ADAMOINT.Data;
using MAP2ADAMOINT.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MAP2ADAMOINT.Services
{
    public class SyncService
    {
        private readonly MapToolContext _mapToolContext;
        private readonly AdamoContext _adamoContext;
        private readonly DataMapperService _mapper;
        private readonly ILogger<SyncService> _logger;

        public SyncService(
            MapToolContext mapToolContext,
            AdamoContext adamoContext,
            DataMapperService mapper,
            ILogger<SyncService> logger)
        {
            _mapToolContext = mapToolContext;
            _adamoContext = adamoContext;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Sync data from MAP Tool (PostgreSQL) to ADAMO (Oracle)
        /// </summary>
        public async Task<SyncResponse> SyncFromMapToAdamo(SyncFromMapRequest request)
        {
            _logger.LogInformation("Starting sync from MAP Tool to ADAMO - BatchSize: {BatchSize}, DryRun: {DryRun}", 
                request.BatchSize, request.DryRun);

            var response = new SyncResponse
            {
                WasDryRun = request.DryRun
            };

            var errors = new List<string>();

            try
            {
                // Build query based on request filters
                var query = _mapToolContext.Molecules.Where(m => !m.IsArchived);

                // Apply filters
                if (request.GrNumbers != null && request.GrNumbers.Any())
                {
                    query = query.Where(m => request.GrNumbers.Contains(m.GrNumber!));
                }

                if (request.MoleculeStatus.HasValue)
                {
                    query = query.Where(m => (int)m.Status == request.MoleculeStatus.Value);
                }

                if (request.CreatedAfter.HasValue)
                {
                    query = query.Where(m => m.CreatedAt >= request.CreatedAfter.Value);
                }

                if (request.ModifiedAfter.HasValue)
                {
                    query = query.Where(m => m.UpdatedAt >= request.ModifiedAfter.Value);
                }

                // Apply batch size limit
                var molecules = await query.Take(request.BatchSize).ToListAsync();

                response.RecordsProcessed = molecules.Count;
                _logger.LogInformation("Found {Count} molecules to sync", molecules.Count);

                foreach (var molecule in molecules)
                {
                    try
                    {
                        // Check if molecule already exists in ADAMO (if skip is enabled)
                        if (request.SkipExisting)
                        {
                            var existingInitial = await _adamoContext.MapInitials
                                .FirstOrDefaultAsync(mi => mi.GrNumber == molecule.GrNumber);

                            if (existingInitial != null)
                            {
                                _logger.LogInformation("Molecule {GrNumber} already exists in ADAMO, skipping", molecule.GrNumber);
                                response.RecordsSkipped++;
                                continue;
                            }
                        }

                        // Get evaluation data if requested
                        Models.MapTool.Map1_1MoleculeEvaluation? evaluation = null;
                        if (request.IncludeEvaluations)
                        {
                            evaluation = await _mapToolContext.Map1_1MoleculeEvaluations
                                .Include(me => me.Molecule)
                                .FirstOrDefaultAsync(me => me.MoleculeId == molecule.Id);
                        }

                        // Map to ADAMO entity
                        var mapInitial = _mapper.MapMoleculeToMapInitial(molecule, evaluation);

                        if (!request.DryRun)
                        {
                            // TODO: Uncomment when ready for production
                            // await _adamoContext.MapInitials.AddAsync(mapInitial);
                            _logger.LogInformation("[DRY RUN] Would insert MapInitial for {GrNumber} into ADAMO", molecule.GrNumber);
                        }
                        else
                        {
                            _logger.LogInformation("[DRY RUN] Would insert MapInitial for {GrNumber} into ADAMO", molecule.GrNumber);
                        }

                        response.RecordsSynced++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync molecule {GrNumber}", molecule.GrNumber);
                        errors.Add($"Failed to sync {molecule.GrNumber}: {ex.Message}");
                        response.RecordsFailed++;
                    }
                }

                if (!request.DryRun && response.RecordsSynced > 0)
                {
                    // TODO: Uncomment when ready for production
                    // await _adamoContext.SaveChangesAsync();
                    _logger.LogInformation("[DRY RUN] Would save {Count} records to ADAMO", response.RecordsSynced);
                }

                response.Status = response.RecordsFailed == 0 ? "success" : "fail";
                response.Message = request.DryRun 
                    ? $"[DRY RUN] Would sync {response.RecordsSynced} molecules to ADAMO"
                    : $"Successfully synced {response.RecordsSynced} molecules to ADAMO";
                response.Errors = errors.Any() ? errors : null;

                _logger.LogInformation("Sync from MAP to ADAMO completed - Synced: {Synced}, Skipped: {Skipped}, Failed: {Failed}", 
                    response.RecordsSynced, response.RecordsSkipped, response.RecordsFailed);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sync from MAP to ADAMO");
                response.Status = "fail";
                response.Message = $"Sync failed: {ex.Message}";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }

        /// <summary>
        /// Sync data from ADAMO (Oracle) to MAP Tool (PostgreSQL)
        /// </summary>
        public async Task<SyncResponse> SyncFromAdamoToMap(SyncFromAdamoRequest request)
        {
            _logger.LogInformation("Starting sync from ADAMO to MAP Tool - BatchSize: {BatchSize}, DryRun: {DryRun}", 
                request.BatchSize, request.DryRun);

            var response = new SyncResponse
            {
                WasDryRun = request.DryRun
            };

            var errors = new List<string>();

            try
            {
                // Build query based on request filters
                var query = _adamoContext.MapSessions.AsQueryable();

                // Apply filters
                if (request.SessionIds != null && request.SessionIds.Any())
                {
                    query = query.Where(s => request.SessionIds.Contains(s.SessionId));
                }

                if (!string.IsNullOrEmpty(request.Stage))
                {
                    query = query.Where(s => s.Stage == request.Stage);
                }

                if (!string.IsNullOrEmpty(request.Region))
                {
                    query = query.Where(s => s.Region == request.Region);
                }

                if (!string.IsNullOrEmpty(request.Segment))
                {
                    query = query.Where(s => s.Segment == request.Segment);
                }

                if (request.EvaluatedAfter.HasValue)
                {
                    query = query.Where(s => s.EvaluationDate >= request.EvaluatedAfter.Value);
                }

                // Apply includes if requested
                if (request.IncludeResults)
                {
                    query = query.Include(s => s.Results);
                }

                // Apply batch size and order
                var sessions = await query
                    .OrderByDescending(s => s.EvaluationDate)
                    .Take(request.BatchSize)
                    .ToListAsync();

                response.RecordsProcessed = sessions.Count;
                _logger.LogInformation("Found {Count} sessions to sync", sessions.Count);

                foreach (var session in sessions)
                {
                    try
                    {
                        // Check if assessment already exists (if skip is enabled)
                        if (request.SkipExisting)
                        {
                            var existingAssessment = await _mapToolContext.Assessments
                                .FirstOrDefaultAsync(a => a.SessionName == $"ADAMO Session {session.SessionId}");

                            if (existingAssessment != null)
                            {
                                _logger.LogInformation("Session {SessionId} already exists in MAP Tool, skipping", session.SessionId);
                                response.RecordsSkipped++;
                                continue;
                            }
                        }

                        // Map assessment (requires at least one result)
                        if (request.IncludeResults && session.Results != null && session.Results.Any())
                        {
                            var firstResult = session.Results.First();
                            var assessment = _mapper.MapResultToAssessment(firstResult, session);

                            if (!request.DryRun)
                            {
                                // TODO: Uncomment when ready for production
                                // await _mapToolContext.Assessments.AddAsync(assessment);
                                _logger.LogInformation("[DRY RUN] Would insert Assessment for session {SessionId} into MAP Tool", session.SessionId);
                            }
                            else
                            {
                                _logger.LogInformation("[DRY RUN] Would insert Assessment for session {SessionId} into MAP Tool", session.SessionId);
                            }

                            response.RecordsSynced++;
                        }
                        else if (!request.IncludeResults)
                        {
                            _logger.LogWarning("Session {SessionId} has no results to map, skipping", session.SessionId);
                            response.RecordsSkipped++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to sync session {SessionId}", session.SessionId);
                        errors.Add($"Failed to sync session {session.SessionId}: {ex.Message}");
                        response.RecordsFailed++;
                    }
                }

                if (!request.DryRun && response.RecordsSynced > 0)
                {
                    // TODO: Uncomment when ready for production
                    // await _mapToolContext.SaveChangesAsync();
                    _logger.LogInformation("[DRY RUN] Would save {Count} records to MAP Tool", response.RecordsSynced);
                }

                response.Status = response.RecordsFailed == 0 ? "success" : "fail";
                response.Message = request.DryRun 
                    ? $"[DRY RUN] Would sync {response.RecordsSynced} sessions to MAP Tool"
                    : $"Successfully synced {response.RecordsSynced} sessions to MAP Tool";
                response.Errors = errors.Any() ? errors : null;

                _logger.LogInformation("Sync from ADAMO to MAP completed - Synced: {Synced}, Skipped: {Skipped}, Failed: {Failed}", 
                    response.RecordsSynced, response.RecordsSkipped, response.RecordsFailed);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sync from ADAMO to MAP");
                response.Status = "fail";
                response.Message = $"Sync failed: {ex.Message}";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
    }
}

