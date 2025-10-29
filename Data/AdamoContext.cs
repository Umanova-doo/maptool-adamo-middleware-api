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
        }
    }
}

