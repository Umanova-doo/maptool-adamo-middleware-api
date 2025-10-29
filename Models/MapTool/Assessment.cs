using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.MapTool
{
    [Table("Assessment", Schema = "map_adm")]
    public class Assessment
    {
        [Key]
        public int Id { get; set; }

        public string? SessionName { get; set; }
        public DateTime DateTime { get; set; }
        public string? Stage { get; set; }
        public int Status { get; set; }
        public string? Region { get; set; }
        public string? Segment { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [StringLength(50)]
        public string? UpdatedBy { get; set; }

        public bool IsArchived { get; set; }
        public bool IsManuallyArchived { get; set; }

        // Navigation properties
        public virtual ICollection<Map1_1Evaluation>? Map1_1Evaluations { get; set; }
    }
}

