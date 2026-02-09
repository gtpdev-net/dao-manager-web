namespace DAO.Manager.Data.Models.Entities;

/// <summary>
/// Represents a .NET assembly reference or binary file discovered during repository scanning.
/// </summary>
/// <remarks>
/// This entity tracks both project-generated assemblies and external assembly references,
/// including their relationships and dependencies within the scanned solution.
/// </remarks>
public class Assembly
{
    /// <summary>
    /// Gets or sets the unique identifier for the assembly.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key reference to the parent <see cref="Entities.Scan"/>.
    /// </summary>
    public int ScanId { get; set; }

    /// <summary>
    /// Gets or sets the name of the assembly (e.g., "System.Core", "MyProject.dll").
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type of assembly (e.g., "GAC", "Project", "External", "NuGet").
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Gets or sets the full file system path to the assembly file.
    /// </summary>
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the version of the assembly, if available.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the parent scan that discovered this assembly.
    /// </summary>
    public Scan Scan { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of projects that reference this assembly.
    /// </summary>
    public ICollection<Project> Projects { get; set; } = new List<Project>();

    /// <summary>
    /// Gets or sets the collection of assemblies that this assembly depends on.
    /// </summary>
    public ICollection<Assembly> ReferencedAssemblies { get; set; } = new List<Assembly>();

    /// <summary>
    /// Gets or sets the collection of assemblies that depend on this assembly.
    /// </summary>
    public ICollection<Assembly> ReferencingAssemblies { get; set; } = new List<Assembly>();
}
