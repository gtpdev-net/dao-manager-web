namespace DAO.Manager.Data.Models.Entities;

/// <summary>
/// Represents a single scan operation performed on a source code repository.
/// </summary>
/// <remarks>
/// A scan captures a point-in-time snapshot of a repository's structure,
/// including solutions, projects, assemblies, packages, and their relationships.
/// Each scan is tied to a specific Git commit for traceability.
/// </remarks>
public class Scan
{
    /// <summary>
    /// Gets or sets the unique identifier for the scan.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the file system path to the repository that was scanned.
    /// </summary>
    public string RepositoryPath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Git commit SHA hash at the time of the scan.
    /// </summary>
    public string GitCommit { get; set; } = null!;

    /// <summary>
    /// Gets or sets the date and time when the scan was performed (UTC).
    /// </summary>
    public DateTime ScanDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the scan record was created in the database (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of events that occurred during the scan operation.
    /// </summary>
    public ICollection<ScanEvent> ScanEvents { get; set; } = new List<ScanEvent>();

    /// <summary>
    /// Gets or sets the collection of solution files discovered in the scan.
    /// </summary>
    public ICollection<Solution> Solutions { get; set; } = new List<Solution>();

    /// <summary>
    /// Gets or sets the collection of project files discovered in the scan.
    /// </summary>
    public ICollection<Project> Projects { get; set; } = new List<Project>();

    /// <summary>
    /// Gets or sets the collection of NuGet packages referenced in the scan.
    /// </summary>
    public ICollection<Package> Packages { get; set; } = new List<Package>();

    /// <summary>
    /// Gets or sets the collection of assemblies discovered in the scan.
    /// </summary>
    public ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>();
}
