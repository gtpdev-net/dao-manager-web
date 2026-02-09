using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAO.Manager.Data.Models.Entities;

namespace DAO.Manager.Data.Data.Configurations;

/// <summary>
/// Entity configuration for the <see cref="Assembly"/> entity.
/// </summary>
public class AssemblyConfiguration : IEntityTypeConfiguration<Assembly>
{
    /// <summary>
    /// Configures the Assembly entity properties, indexes, and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Assembly> builder)
    {
        // Table configuration
        builder.ToTable("Assemblies");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        // Property configuration
        builder.Property(e => e.ScanId).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Type).IsRequired().HasMaxLength(20);
        builder.Property(e => e.FilePath).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Version).HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => e.ScanId);
        builder.HasIndex(e => new { e.ScanId, e.FilePath }).IsUnique();
        builder.HasIndex(e => e.Name);

        // Many-to-many relationship: Assembly <-> Assembly (AssemblyDependencies junction table - self-referencing)
        builder.HasMany(a => a.ReferencedAssemblies)
            .WithMany(a => a.ReferencingAssemblies)
            .UsingEntity<Dictionary<string, object>>(
                "AssemblyDependencies",
                j => j
                    .HasOne<Assembly>()
                    .WithMany()
                    .HasForeignKey("ReferencedAssemblyId")
                    .OnDelete(DeleteBehavior.NoAction), // NO ACTION to avoid circular cascade
                j => j
                    .HasOne<Assembly>()
                    .WithMany()
                    .HasForeignKey("ReferencingAssemblyId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("ReferencingAssemblyId", "ReferencedAssemblyId");
                    j.HasIndex("ReferencedAssemblyId");
                });
    }
}
