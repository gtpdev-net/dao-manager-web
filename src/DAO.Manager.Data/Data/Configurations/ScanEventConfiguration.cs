using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAO.Manager.Data.Models.Entities;

namespace DAO.Manager.Data.Data.Configurations;

/// <summary>
/// Entity configuration for the <see cref="ScanEvent"/> entity.
/// </summary>
public class ScanEventConfiguration : IEntityTypeConfiguration<ScanEvent>
{
    /// <summary>
    /// Configures the ScanEvent entity properties, indexes, and relationships.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<ScanEvent> builder)
    {
        // Table configuration
        builder.ToTable("ScanEvents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        
        // Property configuration
        builder.Property(e => e.ScanId).IsRequired();
        builder.Property(e => e.OccurredAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(e => e.Phase).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(2000);

        // Indexes
        builder.HasIndex(e => e.ScanId);
        builder.HasIndex(e => new { e.ScanId, e.OccurredAt });
    }
}
