using System.ComponentModel.DataAnnotations;

namespace MAP2ADAMOINT.Models.DTOs
{
    /// <summary>
    /// Request body for creating a MAP_SESSION with multiple MAP_RESULT records in one transaction
    /// </summary>
    public class CreateSessionWithResultsRequest
    {
        /// <summary>
        /// Session information (will create MAP_SESSION record)
        /// </summary>
        [Required(ErrorMessage = "Session information is required")]
        public CreateMapSessionRequest Session { get; set; } = new CreateMapSessionRequest();

        /// <summary>
        /// List of results to create for this session (will create MAP_RESULT records)
        /// Each result will be linked to the newly created session
        /// </summary>
        [Required(ErrorMessage = "Results list is required")]
        [MinLength(1, ErrorMessage = "At least one result is required")]
        public List<MapResultItem> Results { get; set; } = new List<MapResultItem>();
    }

    /// <summary>
    /// Individual result item (without sessionId since it will be auto-populated)
    /// </summary>
    public class MapResultItem
    {
        /// <summary>
        /// GR Number - Molecule identifier (REQUIRED)
        /// Format: GR-YY-NNNN-B or GR-YY-NNNNN-B
        /// </summary>
        [Required(ErrorMessage = "GR_NUMBER is required")]
        [StringLength(14, ErrorMessage = "GR_NUMBER must be 14 characters or less")]
        [RegularExpression(@"^GR-\d{2}-\d{4,5}-\d{1}$|^SL-\d{6}-\d{1}$", 
            ErrorMessage = "GR_NUMBER must be in format GR-YY-NNNN-B or GR-YY-NNNNN-B (e.g., 'GR-87-0857-0')")]
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
        /// </summary>
        [Range(1, 5, ErrorMessage = "Result must be between 1 and 5")]
        public int? Result { get; set; }

        /// <summary>
        /// Dilution used for this result
        /// Max length: 20 characters
        /// </summary>
        [StringLength(20, ErrorMessage = "Dilution must be 20 characters or less")]
        public string? Dilution { get; set; }

        /// <summary>
        /// Sponsor information
        /// Max length: 255 characters
        /// </summary>
        [StringLength(255, ErrorMessage = "Sponsor must be 255 characters or less")]
        public string? Sponsor { get; set; }
    }
}

