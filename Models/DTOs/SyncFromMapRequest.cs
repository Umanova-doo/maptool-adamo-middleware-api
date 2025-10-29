using System.ComponentModel.DataAnnotations;

namespace MAP2ADAMOINT.Models.DTOs
{
    /// <summary>
    /// Request body for syncing data from MAP Tool to ADAMO
    /// </summary>
    public class SyncFromMapRequest
    {
        /// <summary>
        /// Specific GR numbers to sync (optional - if null, syncs based on other filters)
        /// </summary>
        public List<string>? GrNumbers { get; set; }

        /// <summary>
        /// Only sync molecules with this status (optional)
        /// Values: None=0, Map1=1, Map0Weak=2, Map0Odorless=3, Ignore=4
        /// </summary>
        public int? MoleculeStatus { get; set; }

        /// <summary>
        /// Only sync molecules created after this date (optional)
        /// </summary>
        public DateTime? CreatedAfter { get; set; }

        /// <summary>
        /// Only sync molecules modified after this date (optional)
        /// </summary>
        public DateTime? ModifiedAfter { get; set; }

        /// <summary>
        /// Maximum number of records to sync in this batch (default: 100, max: 1000)
        /// </summary>
        [Range(1, 1000)]
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Whether to perform a dry run (validate without writing to database)
        /// </summary>
        public bool DryRun { get; set; } = false;

        /// <summary>
        /// Skip records that already exist in ADAMO (based on GR_NUMBER)
        /// </summary>
        public bool SkipExisting { get; set; } = true;

        /// <summary>
        /// Include evaluation data in sync (MAP 1.1 evaluations)
        /// </summary>
        public bool IncludeEvaluations { get; set; } = true;
    }
}

