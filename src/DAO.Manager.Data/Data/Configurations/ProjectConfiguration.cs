using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAO.Manager.Data.Models.Entities;

namespace DAO.Manager.Data.Data.Configurations;

/// <summary>
/// Entity configuration for the <see cref="Project"/> entity.
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <summary>
    /// Configures the Project entity properties, indexes, and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        // Table configuration
        builder.ToTable("Projects");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        // Property configuration
        builder.Property(e => e.ScanId).IsRequired();
        builder.Property(e => e.UniqueIdentifier).IsRequired().HasMaxLength(100);
        builder.Property(e => e.VisualStudioGuid).HasMaxLength(100);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.FilePath).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.TargetFramework).HasMaxLength(100);

        // Indexes
        builder.HasIndex(e => e.ScanId);
        builder.HasIndex(e => new { e.ScanId, e.UniqueIdentifier }).IsUnique();
        builder.HasIndex(e => e.FilePath);
        builder.HasIndex(e => e.Name);

        // Many-to-many relationship: Project <-> Project (ProjectReferences junction table - self-referencing)
        builder.HasMany(p => p.ReferencedProjects)
            .WithMany(p => p.ReferencingProjects)
            .UsingEntity<Dictionary<string, object>>(
                "ProjectReferences",
                j => j
                    .HasOne<Project>()
                    .WithMany()
                    .HasForeignKey("ReferencedProjectId")
                    .OnDelete(DeleteBehavior.NoAction), // NO ACTION to avoid circular cascade
                j => j
                    .HasOne<Project>()
                    .WithMany()
                    .HasForeignKey("ReferencingProjectId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("ReferencingProjectId", "ReferencedProjectId");
                    j.HasIndex("ReferencedProjectId");
                });

        // Many-to-many relationship: Project <-> Package (ProjectPackageReferences junction table)
        builder.HasMany(p => p.Packages)
            .WithMany(pkg => pkg.Projects)
            .UsingEntity<Dictionary<string, object>>(
                "ProjectPackageReferences",
                j => j
                    .HasOne<Package>()
                    .WithMany()
                    .HasForeignKey("PackageId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Project>()
                    .WithMany()
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("ProjectId", "PackageId");
                    j.HasIndex("PackageId");
                });

        // Many-to-many relationship: Project <-> Assembly (ProjectAssemblyReferences junction table)
        builder.HasMany(p => p.Assemblies)
            .WithMany(a => a.Projects)
            .UsingEntity<Dictionary<string, object>>(
                "ProjectAssemblyReferences",
                j => j
                    .HasOne<Assembly>()
                    .WithMany()
                    .HasForeignKey("AssemblyId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Project>()
                    .WithMany()
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("ProjectId", "AssemblyId");
                    j.HasIndex("AssemblyId");
                });
    }
}
