using System.ComponentModel.DataAnnotations;

namespace MAP2ADAMOINT.Models.DTOs
{
    /// <summary>
    /// Request body for creating a new MAP_SESSION record
    /// </summary>
    public class CreateMapSessionRequest
    {
        /// <summary>
        /// Session stage - Must be one of the valid stages
        /// Valid values: "MAP 0", "MAP 1", "MAP 2", "MAP 3", "ISC", "FIB", "FIM", "ISC (Quest)", "CARDEX", "RPMC"
        /// Max length: 20 characters
        /// </summary>
        [StringLength(20, ErrorMessage = "Stage must be 20 characters or less")]
        public string? Stage { get; set; }

        /// <summary>
        /// Date of session evaluation
        /// </summary>
        public DateTime? EvaluationDate { get; set; }

        /// <summary>
        /// Geographic region (e.g., "EU", "US", "AS")
        /// Max length: 2 characters
        /// </summary>
        [StringLength(2, ErrorMessage = "Region must be 2 characters")]
        public string? Region { get; set; }

        /// <summary>
        /// Market segment (e.g., "CP" for Consumer Preference, "FF" for Fine Fragrance)
        /// Max length: 2 characters
        /// </summary>
        [StringLength(2, ErrorMessage = "Segment must be 2 characters")]
        public string? Segment { get; set; }

        /// <summary>
        /// List of session participants (comma-separated or formatted as needed)
        /// Max length: 1000 characters
        /// </summary>
        [StringLength(1000, ErrorMessage = "Participants must be 1000 characters or less")]
        public string? Participants { get; set; }

        /// <summary>
        /// Flag to show in task list ('Y' or 'N')
        /// Default: 'N'
        /// </summary>
        [StringLength(1, ErrorMessage = "ShowInTaskList must be 'Y' or 'N'")]
        [RegularExpression("^[YNyn]$", ErrorMessage = "ShowInTaskList must be 'Y' or 'N'")]
        public string? ShowInTaskList { get; set; } = "N";

        /// <summary>
        /// Sub-stage identifier (numeric value 0-9)
        /// </summary>
        [Range(0, 9, ErrorMessage = "SubStage must be between 0 and 9")]
        public int? SubStage { get; set; }

        /// <summary>
        /// Session category
        /// Max length: 2 characters
        /// </summary>
        [StringLength(2, ErrorMessage = "Category must be 2 characters")]
        public string? Category { get; set; }

        /// <summary>
        /// Session link - unique identifier from MapTool to prevent duplicate session creation
        /// Use this as a unique reference to link MapTool sessions to ADAMO sessions
        /// Max length: 50 characters
        /// </summary>
        [StringLength(50, ErrorMessage = "SessionLink must be 50 characters or less")]
        public string? SessionLink { get; set; }

        /// <summary>
        /// User who is creating this record (for audit trail)
        /// Max length: 8 characters
        /// </summary>
        [StringLength(8, ErrorMessage = "CreatedBy must be 8 characters or less")]
        public string? CreatedBy { get; set; }
    }

    /// <summary>
    /// Valid stage values for MAP_SESSION
    /// </summary>
    public static class MapStages
    {
        public const string MAP0 = "MAP 0";
        public const string MAP1 = "MAP 1";
        public const string MAP2 = "MAP 2";
        public const string MAP3 = "MAP 3";
        public const string ISC = "ISC";
        public const string FIB = "FIB";
        public const string FIM = "FIM";
        public const string ISC_QUEST = "ISC (Quest)";
        public const string CARDEX = "CARDEX";
        public const string RPMC = "RPMC";

        public static readonly string[] AllStages = new[]
        {
            MAP0, MAP1, MAP2, MAP3, ISC, FIB, FIM, ISC_QUEST, CARDEX, RPMC
        };

        public static bool IsValidStage(string? stage)
        {
            return stage != null && AllStages.Contains(stage, StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Common segment values
    /// </summary>
    public static class MapSegments
    {
        public const string CP = "CP"; // Consumer Preference
        public const string FF = "FF"; // Fine Fragrance
    }
}

