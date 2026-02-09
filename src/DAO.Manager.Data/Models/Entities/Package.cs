namespace DAO.Manager.Data.Models.Entities;

/// <summary>
/// Represents a NuGet package reference discovered during repository scanning.
/// </summary>
/// <remarks>
/// Packages are external dependencies obtained from NuGet repositories,
/// providing reusable functionality to .NET projects.
/// </remarks>
public class Package
{
    /// <summary>
    /// Gets or sets the unique identifier for the package.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key reference to the parent <see cref="Entities.Scan"/>.
    /// </summary>
    public int ScanId { get; set; }

    /// <summary>
    /// Gets or sets the name of the NuGet package (e.g., "Newtonsoft.Json").
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the version of the package (e.g., "13.0.1").
    /// </summary>
    public string Version { get; set; } = null!;

    /// <summary>
    /// Gets or sets the parent scan that discovered this package reference.
    /// </summary>
    public Scan Scan { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of projects that reference this package.
    /// </summary>
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
