using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.MapTool
{
    [Table("OdorFamily", Schema = "map_adm")]
    public class OdorFamily
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Color { get; set; }
        public string? Code { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [StringLength(50)]
        public string? UpdatedBy { get; set; }

        public bool IsArchived { get; set; }
        public bool IsManuallyArchived { get; set; }

        // Navigation properties
        public virtual ICollection<OdorDescriptor>? OdorDescriptors { get; set; }
    }
}

