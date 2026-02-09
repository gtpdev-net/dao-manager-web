using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAO.Manager.Data.Models.Entities;

namespace DAO.Manager.Data.Data.Configurations;

/// <summary>
/// Entity configuration for the <see cref="Package"/> entity.
/// </summary>
public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    /// <summary>
    /// Configures the Package entity properties, indexes, and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        // Table configuration
        builder.ToTable("Packages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        // Property configuration
        builder.Property(e => e.ScanId).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Version).IsRequired().HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => e.ScanId);
        builder.HasIndex(e => new { e.ScanId, e.Name, e.Version }).IsUnique();
        builder.HasIndex(e => e.Name);
    }
}
