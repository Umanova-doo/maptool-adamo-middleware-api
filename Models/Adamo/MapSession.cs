using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.Adamo
{
    [Table("MAP_SESSION", Schema = "GIV_MAP")]
    public class MapSession
    {
        [Key]
        [Column("SESSION_ID")]
        public long SessionId { get; set; }

        [StringLength(20)]
        [Column("STAGE")]
        public string? Stage { get; set; }

        [Column("EVALUATION_DATE")]
        public DateTime? EvaluationDate { get; set; }

        [StringLength(2)]
        [Column("REGION")]
        public string? Region { get; set; }

        [StringLength(2)]
        [Column("SEGMENT")]
        public string? Segment { get; set; }

        [StringLength(1000)]
        [Column("PARTICIPANTS")]
        public string? Participants { get; set; }

        [StringLength(1)]
        [Column("SHOW_IN_TASK_LIST")]
        public string? ShowInTaskList { get; set; } = "N";

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

        [Column("SUB_STAGE")]
        public int? SubStage { get; set; }

        [StringLength(2)]
        [Column("CATEGORY")]
        public string? Category { get; set; }

        [StringLength(50)]
        [Column("SESSION_LINK")]
        public string? SessionLink { get; set; }

        // Navigation properties
        public virtual ICollection<MapResult>? Results { get; set; }
    }
}

