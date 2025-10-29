using Microsoft.EntityFrameworkCore;
using MAP2ADAMOINT.Models.MapTool;

namespace MAP2ADAMOINT.Data
{
    public class MapToolContext : DbContext
    {
        public MapToolContext(DbContextOptions<MapToolContext> options) : base(options)
        {
        }

        public DbSet<Molecule> Molecules { get; set; } = null!;
        public DbSet<Assessment> Assessments { get; set; } = null!;
        public DbSet<Map1_1Evaluation> Map1_1Evaluations { get; set; } = null!;
        public DbSet<Map1_1MoleculeEvaluation> Map1_1MoleculeEvaluations { get; set; } = null!;
        public DbSet<OdorFamily> OdorFamilies { get; set; } = null!;
        public DbSet<OdorDescriptor> OdorDescriptors { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure schema
            modelBuilder.HasDefaultSchema("map_adm");

            // Set all foreign keys to Restrict delete behavior
            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys());

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            // Configure Molecule
            modelBuilder.Entity<Molecule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            // Configure Assessment
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasMany(e => e.Map1_1Evaluations)
                      .WithOne(e => e.Assessment)
                      .HasForeignKey(e => e.AssessmentId);
            });

            // Configure Map1_1Evaluation
            modelBuilder.Entity<Map1_1Evaluation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasMany(e => e.MoleculeEvaluations)
                      .WithOne(e => e.Map1_1Evaluation)
                      .HasForeignKey(e => e.Map1_1EvaluationId);
            });

            // Configure Map1_1MoleculeEvaluation
            modelBuilder.Entity<Map1_1MoleculeEvaluation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasOne(e => e.Molecule)
                      .WithMany()
                      .HasForeignKey(e => e.MoleculeId);
            });

            // Configure OdorFamily
            modelBuilder.Entity<OdorFamily>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasMany(e => e.OdorDescriptors)
                      .WithOne(e => e.OdorFamily)
                      .HasForeignKey(e => e.OdorFamilyId);
            });

            // Configure OdorDescriptor
            modelBuilder.Entity<OdorDescriptor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }
}

