using System.ComponentModel.DataAnnotations;

namespace MAP2ADAMOINT.Models.DTOs
{
    /// <summary>
    /// Request body for syncing data from ADAMO to MAP Tool
    /// </summary>
    public class SyncFromAdamoRequest
    {
        /// <summary>
        /// Specific session IDs to sync (optional)
        /// </summary>
        public List<long>? SessionIds { get; set; }

        /// <summary>
        /// Only sync sessions of this stage (optional)
        /// Values: "MAP 0", "MAP 1", "MAP 2", "MAP 3", "ISC", "FIB", "FIM", "CARDEX", "RPMC"
        /// </summary>
        public string? Stage { get; set; }

        /// <summary>
        /// Only sync sessions from this region (optional)
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Only sync sessions from this segment (optional)
        /// Values: "CP", "FF"
        /// </summary>
        public string? Segment { get; set; }

        /// <summary>
        /// Only sync sessions evaluated after this date (optional)
        /// </summary>
        public DateTime? EvaluatedAfter { get; set; }

        /// <summary>
        /// Maximum number of sessions to sync in this batch (default: 50, max: 500)
        /// </summary>
        [Range(1, 500)]
        public int BatchSize { get; set; } = 50;

        /// <summary>
        /// Whether to perform a dry run (validate without writing to database)
        /// </summary>
        public bool DryRun { get; set; } = false;

        /// <summary>
        /// Skip sessions that already exist in MAP Tool
        /// </summary>
        public bool SkipExisting { get; set; } = true;

        /// <summary>
        /// Include results in sync (MAP_RESULT records)
        /// </summary>
        public bool IncludeResults { get; set; } = true;

        /// <summary>
        /// Include odor characterization data
        /// </summary>
        public bool IncludeOdorCharacterization { get; set; } = false;
    }
}

