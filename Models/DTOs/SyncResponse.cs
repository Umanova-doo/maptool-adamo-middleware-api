namespace MAP2ADAMOINT.Models.DTOs
{
    /// <summary>
    /// Standard response for sync operations
    /// </summary>
    public class SyncResponse
    {
        /// <summary>
        /// Status of sync operation: "success" or "fail"
        /// </summary>
        public string Status { get; set; } = "success";

        /// <summary>
        /// Number of records processed (read from source)
        /// </summary>
        public int RecordsProcessed { get; set; }

        /// <summary>
        /// Number of records successfully synced to destination
        /// </summary>
        public int RecordsSynced { get; set; }

        /// <summary>
        /// Number of records skipped (already exist)
        /// </summary>
        public int RecordsSkipped { get; set; }

        /// <summary>
        /// Number of records that failed to sync
        /// </summary>
        public int RecordsFailed { get; set; }

        /// <summary>
        /// Detailed message about the sync operation
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// List of errors encountered during sync (if any)
        /// </summary>
        public List<string>? Errors { get; set; }

        /// <summary>
        /// Whether this was a dry run (no actual writes)
        /// </summary>
        public bool WasDryRun { get; set; }

        /// <summary>
        /// Timestamp when sync completed
        /// </summary>
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}

