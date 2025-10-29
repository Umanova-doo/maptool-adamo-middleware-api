using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.MapTool
{
    [Table("Molecule", Schema = "map_adm")]
    public class Molecule
    {
        [Key]
        public int Id { get; set; }

        public string? GrNumber { get; set; }
        public string? RegNumber { get; set; }
        public string? Structure { get; set; }
        public bool Assessed { get; set; }
        public string? ChemistName { get; set; }
        public string? ChemicalName { get; set; }
        public string? MolecularFormula { get; set; }
        public string? ProjectName { get; set; }
        public MoleculeStatus Status { get; set; }
        public decimal Quantity { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [StringLength(50)]
        public string? UpdatedBy { get; set; }

        public bool IsArchived { get; set; }
        public bool IsManuallyArchived { get; set; }
    }

    public enum MoleculeStatus
    {
        None = 0,
        Map1 = 1,
        Map0Weak = 2,
        Map0Odorless = 3,
        Ignore = 4
    }
}

