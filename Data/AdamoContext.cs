using Microsoft.EntityFrameworkCore;
using MAP2ADAMOINT.Models.Adamo;

namespace MAP2ADAMOINT.Data
{
    public class AdamoContext : DbContext
    {
        public AdamoContext(DbContextOptions<AdamoContext> options) : base(options)
        {
        }

        public DbSet<MapInitial> MapInitials { get; set; } = null!;
        public DbSet<MapSession> MapSessions { get; set; } = null!;
        public DbSet<MapResult> MapResults { get; set; } = null!;
        public DbSet<OdorCharacterization> OdorCharacterizations { get; set; } = null!;
        public DbSet<MapOdorFamily> OdorFamilies { get; set; } = null!;
        public DbSet<MapOdorDescriptor> OdorDescriptors { get; set; } = null!;
        public DbSet<Map1SessionLink> Map1SessionLinks { get; set; } = null!;
        public DbSet<SubmittingIgnoredMolecules> IgnoredMolecules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure schema
            modelBuilder.HasDefaultSchema("GIV_MAP");

            // Configure MapSession
            modelBuilder.Entity<MapSession>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.Property(e => e.ShowInTaskList).HasDefaultValue("N");
            });

            // Configure MapResult
            modelBuilder.Entity<MapResult>(entity =>
            {
                entity.HasKey(e => e.ResultId);
                entity.HasOne(e => e.Session)
                      .WithMany(s => s.Results)
                      .HasForeignKey(e => e.SessionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MapInitial
            modelBuilder.Entity<MapInitial>(entity =>
            {
                entity.HasKey(e => e.MapInitialId);
                entity.HasIndex(e => e.GrNumber).IsUnique();
            });

            // Configure OdorCharacterization
            modelBuilder.Entity<OdorCharacterization>(entity =>
            {
                entity.HasKey(e => e.OdorCharacterizationId);
                entity.HasIndex(e => e.GrNumber).IsUnique();
            });

            // Configure MapOdorFamily
            modelBuilder.Entity<MapOdorFamily>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasMany(e => e.Descriptors)
                      .WithOne(d => d.Family)
                      .HasForeignKey(d => d.FamilyId);
            });

            // Configure MapOdorDescriptor
            modelBuilder.Entity<MapOdorDescriptor>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // Configure Map1SessionLink (composite key)
            modelBuilder.Entity<Map1SessionLink>(entity =>
            {
                entity.HasKey(e => new { e.CpSessionId, e.FfSessionId });
            });

            // Configure SubmittingIgnoredMolecules
            modelBuilder.Entity<SubmittingIgnoredMolecules>(entity =>
            {
                entity.HasKey(e => e.GrNumber);
            });
        }
    }
}

