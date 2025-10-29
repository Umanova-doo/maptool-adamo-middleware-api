using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.MapTool
{
    [Table("Map1_1MoleculeEvaluation", Schema = "map_adm")]
    public class Map1_1MoleculeEvaluation
    {
        [Key]
        public int Id { get; set; }

        public int Map1_1EvaluationId { get; set; }
        public int MoleculeId { get; set; }
        public int SortOrder { get; set; }
        public int? GrDilutionSolventId { get; set; }
        public int? BenchmarkDilutionSolventId { get; set; }
        public string? Odor0h { get; set; }
        public string? Odor4h { get; set; }
        public string? Odor24h { get; set; }
        public string? Benchmark { get; set; }
        public string? Comment { get; set; }
        public string? FFNextSteps { get; set; }
        public string? CPNextSteps { get; set; }
        public int? ResultCP { get; set; }
        public int? ResultFF { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("Map1_1EvaluationId")]
        public virtual Map1_1Evaluation? Map1_1Evaluation { get; set; }

        [ForeignKey("MoleculeId")]
        public virtual Molecule? Molecule { get; set; }
    }
}

