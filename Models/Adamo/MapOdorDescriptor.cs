using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.Adamo
{
    [Table("MAP_ODOR_DESCRIPTOR", Schema = "GIV_MAP")]
    public class MapOdorDescriptor
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [StringLength(255)]
        [Column("CODE")]
        public string? Code { get; set; }

        [StringLength(255)]
        [Column("NAME")]
        public string? Name { get; set; }

        [Column("FAMILY_ID")]
        public long? FamilyId { get; set; }

        [StringLength(255)]
        [Column("PROFILE_NAME")]
        public string? ProfileName { get; set; }

        // Navigation property
        [ForeignKey("FamilyId")]
        public virtual MapOdorFamily? Family { get; set; }
    }
}

