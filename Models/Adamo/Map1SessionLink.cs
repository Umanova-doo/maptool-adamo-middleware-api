using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.Adamo
{
    [Table("MAP1_SESSION_LINK", Schema = "GIV_MAP")]
    public class Map1SessionLink
    {
        [Column("CP_SESSION_ID")]
        public long CpSessionId { get; set; }

        [Column("FF_SESSION_ID")]
        public long FfSessionId { get; set; }

        // Navigation properties
        [ForeignKey("CpSessionId")]
        public virtual MapSession? CpSession { get; set; }

        [ForeignKey("FfSessionId")]
        public virtual MapSession? FfSession { get; set; }
    }
}
