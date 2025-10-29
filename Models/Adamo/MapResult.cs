using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.Adamo
{
    [Table("MAP_RESULT", Schema = "GIV_MAP")]
    public class MapResult
    {
        [Key]
        [Column("RESULT_ID")]
        public long ResultId { get; set; }

        [Required]
        [Column("SESSION_ID")]
        public long SessionId { get; set; }

        [Required]
        [StringLength(14)]
        [Column("GR_NUMBER")]
        public string GrNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        [Column("ODOR")]
        public string? Odor { get; set; }

        [StringLength(2000)]
        [Column("BENCHMARK_COMMENTS")]
        public string? BenchmarkComments { get; set; }

        [Column("RESULT")]
        public int? Result { get; set; }

        [StringLength(20)]
        [Column("DILUTION")]
        public string? Dilution { get; set; }

        [StringLength(255)]
        [Column("SPONSOR")]
        public string? Sponsor { get; set; }

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

        [StringLength(11)]
        [Column("REG_NUMBER")]
        public string? RegNumber { get; set; }

        [Column("BATCH")]
        public int? Batch { get; set; }

        // Navigation property
        [ForeignKey("SessionId")]
        public virtual MapSession? Session { get; set; }
    }
}

