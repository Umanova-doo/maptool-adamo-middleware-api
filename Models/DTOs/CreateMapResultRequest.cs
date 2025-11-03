using System.ComponentModel.DataAnnotations;

namespace MAP2ADAMOINT.Models.DTOs
{
    /// <summary>
    /// Request body for creating a new MAP_RESULT record
    /// </summary>
    public class CreateMapResultRequest
    {
        /// <summary>
        /// Session ID - Foreign key to MAP_SESSION (REQUIRED)
        /// This is the SESSION_ID returned when creating a MAP_SESSION
        /// </summary>
        [Required(ErrorMessage = "SESSION_ID is required")]
        public long SessionId { get; set; }

        /// <summary>
        /// GR Number - Molecule identifier (REQUIRED)
        /// Format: GR-YY-NNNNN-B or SL-NNNNNN-B
        /// Example: "GR-88-0681-1"
        /// </summary>
        [Required(ErrorMessage = "GR_NUMBER is required")]
        [StringLength(14, ErrorMessage = "GR_NUMBER must be 14 characters or less")]
        public string GrNumber { get; set; } = string.Empty;

        /// <summary>
        /// Odor description from evaluation
        /// Max length: 1000 characters
        /// </summary>
        [StringLength(1000, ErrorMessage = "Odor must be 1000 characters or less")]
        public string? Odor { get; set; }

        /// <summary>
        /// Comments comparing to benchmark molecules
        /// Max length: 2000 characters
        /// </summary>
        [StringLength(2000, ErrorMessage = "BenchmarkComments must be 2000 characters or less")]
        public string? BenchmarkComments { get; set; }

        /// <summary>
        /// Numeric result/score (typically 1-5 scale)
        /// 1 = Poor, 2 = Fair, 3 = Good, 4 = Very Good, 5 = Excellent
        /// </summary>
        [Range(1, 5, ErrorMessage = "Result must be between 1 and 5")]
        public int? Result { get; set; }

        /// <summary>
        /// Dilution used for this result
        /// Max length: 20 characters
        /// Example: "10% in DPG", "2.5%"
        /// </summary>
        [StringLength(20, ErrorMessage = "Dilution must be 20 characters or less")]
        public string? Dilution { get; set; }

        /// <summary>
        /// Sponsor information
        /// Max length: 255 characters
        /// </summary>
        [StringLength(255, ErrorMessage = "Sponsor must be 255 characters or less")]
        public string? Sponsor { get; set; }

        /// <summary>
        /// User who is creating this record (for audit trail)
        /// Max length: 8 characters
        /// </summary>
        [StringLength(8, ErrorMessage = "CreatedBy must be 8 characters or less")]
        public string? CreatedBy { get; set; }
    }
}

