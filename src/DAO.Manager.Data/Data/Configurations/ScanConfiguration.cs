using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAO.Manager.Data.Models.Entities;

namespace DAO.Manager.Data.Data.Configurations;

/// <summary>
/// Entity configuration for the <see cref="Scan"/> entity.
/// </summary>
public class ScanConfiguration : IEntityTypeConfiguration<Scan>
{
    /// <summary>
    /// Configures the Scan entity properties, indexes, and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Scan> builder)
    {
        // Table configuration
        builder.ToTable("Scans");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        // Property configuration
        builder.Property(e => e.RepositoryPath).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.GitCommit).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ScanDate).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(e => e.ScanDate);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.GitCommit);

        // Relationships
        builder.HasMany(e => e.ScanEvents)
            .WithOne(e => e.Scan)
            .HasForeignKey(e => e.ScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Solutions)
            .WithOne(e => e.Scan)
            .HasForeignKey(e => e.ScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Projects)
            .WithOne(e => e.Scan)
            .HasForeignKey(e => e.ScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Packages)
            .WithOne(e => e.Scan)
            .HasForeignKey(e => e.ScanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Assemblies)
            .WithOne(e => e.Scan)
            .HasForeignKey(e => e.ScanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
