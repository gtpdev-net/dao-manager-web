using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAO.Manager.Data.Models.Entities;

namespace DAO.Manager.Data.Data.Configurations;

/// <summary>
/// Entity configuration for the <see cref="Solution"/> entity.
/// </summary>
public class SolutionConfiguration : IEntityTypeConfiguration<Solution>
{
    /// <summary>
    /// Configures the Solution entity properties, indexes, and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Solution> builder)
    {
        // Table configuration
        builder.ToTable("Solutions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        // Property configuration
        builder.Property(e => e.ScanId).IsRequired();
        builder.Property(e => e.UniqueIdentifier).IsRequired().HasMaxLength(100);
        builder.Property(e => e.VisualStudioGuid).HasMaxLength(100);
        builder.Property(e => e.FilePath).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);

        // Indexes
        builder.HasIndex(e => e.ScanId);
        builder.HasIndex(e => new { e.ScanId, e.UniqueIdentifier }).IsUnique();
        builder.HasIndex(e => e.FilePath);

        // Many-to-many relationship: Solution <-> Project (SolutionProjects junction table)
        builder.HasMany(s => s.Projects)
            .WithMany(p => p.Solutions)
            .UsingEntity<Dictionary<string, object>>(
                "SolutionProjects",
                j => j
                    .HasOne<Project>()
                    .WithMany()
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Solution>()
                    .WithMany()
                    .HasForeignKey("SolutionId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("SolutionId", "ProjectId");
                    j.HasIndex("ProjectId");
                });
    }
}
