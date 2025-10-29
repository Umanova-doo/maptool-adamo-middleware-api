using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.Adamo
{
    [Table("MAP_INITIAL", Schema = "GIV_MAP")]
    public class MapInitial
    {
        [Key]
        [Column("MAP_INITIAL_ID")]
        public long MapInitialId { get; set; }

        [Required]
        [StringLength(14)]
        [Column("GR_NUMBER")]
        public string GrNumber { get; set; } = string.Empty;

        [Column("EVALUATION_DATE")]
        public DateTime? EvaluationDate { get; set; }

        [StringLength(50)]
        [Column("CHEMIST")]
        public string? Chemist { get; set; }

        [StringLength(255)]
        [Column("ASSESSOR")]
        public string? Assessor { get; set; }

        [StringLength(30)]
        [Column("DILUTION")]
        public string? Dilution { get; set; }

        [StringLength(2)]
        [Column("EVALUATION_SITE")]
        public string? EvaluationSite { get; set; }

        [StringLength(500)]
        [Column("ODOR0H")]
        public string? Odor0H { get; set; }

        [StringLength(255)]
        [Column("ODOR4H")]
        public string? Odor4H { get; set; }

        [StringLength(255)]
        [Column("ODOR24H")]
        public string? Odor24H { get; set; }

        [Column("CREATION_DATE")]
        public DateTime? CreationDate { get; set; }

        [StringLength(8)]
        [Column("CREATED_BY")]
        public string? CreatedBy { get; set; }

        [Column("LAST_MODIFIED_DATE")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(8)]
        [Column("LAST_MODIFIED_BY")]
        public string? LastModifiedBy { get; set; }

        [StringLength(1000)]
        [Column("COMMENTS")]
        public string? Comments { get; set; }

        [StringLength(11)]
        [Column("REG_NUMBER")]
        public string? RegNumber { get; set; }

        [Column("BATCH")]
        public int? Batch { get; set; }
    }
}

