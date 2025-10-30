using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.Adamo
{
    [Table("SUBMITTING_IGNORED_MOLECULES", Schema = "GIV_MAP")]
    public class SubmittingIgnoredMolecules
    {
        [Key]
        [StringLength(20)]
        [Column("GR_NUMBER")]
        public string GrNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column("ENTRY_PERSON")]
        public string EntryPerson { get; set; } = string.Empty;

        [Required]
        [Column("ENTRY_DATE")]
        public DateTime EntryDate { get; set; }
    }
}
