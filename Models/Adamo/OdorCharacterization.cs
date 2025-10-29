using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MAP2ADAMOINT.Models.Adamo
{
    [Table("ODOR_CHARACTERIZATION", Schema = "GIV_MAP")]
    public class OdorCharacterization
    {
        [Key]
        [Column("ODOR_CHARACTERIZATION_ID")]
        public long OdorCharacterizationId { get; set; }

        [Required]
        [StringLength(14)]
        [Column("GR_NUMBER")]
        public string GrNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        [Column("ODOR_SUMMARY")]
        public string? OdorSummary { get; set; }

        [Column("CREATION_DATE")]
        public DateTime? CreationDate { get; set; }

        [StringLength(20)]
        [Column("CREATED_BY")]
        public string? CreatedBy { get; set; }

        [Column("LAST_MODIFIED_DATE")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(20)]
        [Column("LAST_MODIFIED_BY")]
        public string? LastModifiedBy { get; set; }

        [StringLength(11)]
        [Column("REG_NUMBER")]
        public string? RegNumber { get; set; }

        [Column("BATCH")]
        public int? Batch { get; set; }

        // Overall metrics
        [Column("INTENSITY")]
        public int? Intensity { get; set; }

        [Column("TENACITY")]
        public int? Tenacity { get; set; }

        [Column("OVERALL_LIKING")]
        public int? OverallLiking { get; set; }

        [StringLength(2000)]
        [Column("FAMILY_PROFILE")]
        public string? FamilyProfile { get; set; }

        [StringLength(2000)]
        [Column("ODOR_PROFILE")]
        public string? OdorProfile { get; set; }

        // Family scores (12 families)
        [Column("AMBERGRIS_FAMILY")]
        public int? AmbergrisFamily { get; set; }

        [Column("AROMATIC_HERBAL_FAMILY")]
        public int? AromaticHerbalFamily { get; set; }

        [Column("CITRUS_FAMILY")]
        public int? CitrusFamily { get; set; }

        [Column("FLORAL_FAMILY")]
        public int? FloralFamily { get; set; }

        [Column("FRUITY_FAMILY")]
        public int? FruityFamily { get; set; }

        [Column("GREEN_FAMILY")]
        public int? GreenFamily { get; set; }

        [Column("MARINE_FAMILY")]
        public int? MarineFamily { get; set; }

        [Column("MUSKY_ANIMALIC_FAMILY")]
        public int? MuskyAnimalicFamily { get; set; }

        [Column("OFF_ODORS_FAMILY")]
        public int? OffOdorsFamily { get; set; }

        [Column("SPICY_FAMILY")]
        public int? SpicyFamily { get; set; }

        [Column("SWEET_GOURMAND_FAMILY")]
        public int? SweetGourmandFamily { get; set; }

        [Column("WOODY_FAMILY")]
        public int? WoodyFamily { get; set; }

        // Note: Individual descriptors (~100+ fields) can be added as needed
        // For brevity, showing just a few examples:

        [Column("APPLE")]
        public int? Apple { get; set; }

        [Column("ROSE")]
        public int? Rose { get; set; }

        [Column("CEDARWOOD")]
        public int? Cedarwood { get; set; }

        [Column("VANILLA")]
        public int? Vanilla { get; set; }

        [Column("LEMON")]
        public int? Lemon { get; set; }

        // TODO: Add remaining 80+ descriptor fields as needed
    }
}

