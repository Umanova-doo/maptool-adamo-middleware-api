using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.Adamo
{
    [Table("MAP_ODOR_FAMILY", Schema = "GIV_MAP")]
    public class MapOdorFamily
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [StringLength(50)]
        [Column("CODE")]
        public string? Code { get; set; }

        [StringLength(50)]
        [Column("NAME")]
        public string? Name { get; set; }

        [StringLength(10)]
        [Column("COLOR")]
        public string? Color { get; set; }

        // Navigation properties
        public virtual ICollection<MapOdorDescriptor>? Descriptors { get; set; }
    }
}

