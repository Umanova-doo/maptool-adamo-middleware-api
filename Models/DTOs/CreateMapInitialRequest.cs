using System.ComponentModel.DataAnnotations;

namespace MAP2ADAMOINT.Models.DTOs
{
    /// <summary>
    /// Request body for creating a new MAP_INITIAL record
    /// </summary>
    public class CreateMapInitialRequest
    {
        /// <summary>
        /// GR Number - Unique molecule identifier (REQUIRED)
        /// Format: GR-YY-NNNNN-B or SL-NNNNNN-B
        /// Example: "GR-88-0681-1"
        /// </summary>
        [Required(ErrorMessage = "GR_NUMBER is required")]
        [StringLength(14, ErrorMessage = "GR_NUMBER must be 14 characters or less")]
        public string GrNumber { get; set; } = string.Empty;

        /// <summary>
        /// Date of evaluation
        /// </summary>
        public DateTime? EvaluationDate { get; set; }

        /// <summary>
        /// Chemist who performed evaluation
        /// Max length: 50 characters
        /// </summary>
        [StringLength(50, ErrorMessage = "Chemist name must be 50 characters or less")]
        public string? Chemist { get; set; }

        /// <summary>
        /// Person(s) who assessed the molecule
        /// Max length: 255 characters
        /// </summary>
        [StringLength(255, ErrorMessage = "Assessor must be 255 characters or less")]
        public string? Assessor { get; set; }

        /// <summary>
        /// Dilution used (e.g., "10% in DPG")
        /// Max length: 30 characters
        /// </summary>
        [StringLength(30, ErrorMessage = "Dilution must be 30 characters or less")]
        public string? Dilution { get; set; }

        /// <summary>
        /// Site code where evaluation occurred (e.g., "ZH", "US")
        /// Max length: 2 characters
        /// </summary>
        [StringLength(2, ErrorMessage = "Evaluation site must be 2 characters")]
        public string? EvaluationSite { get; set; }

        /// <summary>
        /// Odor description at 0 hours (initial)
        /// Max length: 500 characters
        /// </summary>
        [StringLength(500, ErrorMessage = "Odor0H must be 500 characters or less")]
        public string? Odor0H { get; set; }

        /// <summary>
        /// Odor description after 4 hours
        /// Max length: 255 characters
        /// </summary>
        [StringLength(255, ErrorMessage = "Odor4H must be 255 characters or less")]
        public string? Odor4H { get; set; }

        /// <summary>
        /// Odor description after 24 hours
        /// Max length: 255 characters
        /// </summary>
        [StringLength(255, ErrorMessage = "Odor24H must be 255 characters or less")]
        public string? Odor24H { get; set; }

        /// <summary>
        /// Additional comments
        /// Max length: 1000 characters
        /// </summary>
        [StringLength(1000, ErrorMessage = "Comments must be 1000 characters or less")]
        public string? Comments { get; set; }

        /// <summary>
        /// User who is creating this record (for audit trail)
        /// Max length: 8 characters
        /// </summary>
        [StringLength(8, ErrorMessage = "CreatedBy must be 8 characters or less")]
        public string? CreatedBy { get; set; }
    }
}

