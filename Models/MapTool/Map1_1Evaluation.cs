using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.MapTool
{
    [Table("Map1_1Evaluation", Schema = "map_adm")]
    public class Map1_1Evaluation
    {
        [Key]
        public int Id { get; set; }

        public int AssessmentId { get; set; }
        public string? Participants { get; set; }
        public DateTime? EvaluationDate { get; set; }
        public int EvaluationSiteId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [StringLength(50)]
        public string? UpdatedBy { get; set; }

        // Navigation property
        [ForeignKey("AssessmentId")]
        public virtual Assessment? Assessment { get; set; }

        public virtual ICollection<Map1_1MoleculeEvaluation>? MoleculeEvaluations { get; set; }
    }
}

