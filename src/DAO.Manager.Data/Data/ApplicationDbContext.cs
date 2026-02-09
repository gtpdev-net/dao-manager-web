using Microsoft.EntityFrameworkCore;
using DAO.Manager.Data.Models.Entities;

namespace DAO.Manager.Data.Data;

/// <summary>
/// Entity Framework Core database context for the DAO Manager application.
/// </summary>
/// <remarks>
/// This context manages the data model for repository scanning operations,
/// including scans, solutions, projects, packages, assemblies, and their relationships.
/// It configures entity relationships, indexes, and database constraints.
/// </remarks>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by this DbContext.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the DbSet for <see cref="Scan"/> entities.
    /// </summary>
    public DbSet<Scan> Scans { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for <see cref="ScanEvent"/> entities.
    /// </summary>
    public DbSet<ScanEvent> ScanEvents { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for <see cref="Solution"/> entities.
    /// </summary>
    public DbSet<Solution> Solutions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for <see cref="Project"/> entities.
    /// </summary>
    public DbSet<Project> Projects { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for <see cref="Package"/> entities.
    /// </summary>
    public DbSet<Package> Packages { get; set; } = null!;

    /// <summary>
    /// Gets or sets the DbSet for <see cref="Assembly"/> entities.
    /// </summary>
    public DbSet<Assembly> Assemblies { get; set; } = null!;

    /// <summary>
    /// Configures the database schema and entity relationships using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The builder used to construct the model for this context.</param>
    /// <remarks>
    /// Entity configurations are applied from separate IEntityTypeConfiguration classes
    /// located in the Data/Configurations folder. This approach:
    /// - Separates concerns by placing each entity's configuration in its own class
    /// - Improves maintainability and readability
    /// - Follows EF Core best practices for complex data models
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration implementations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
