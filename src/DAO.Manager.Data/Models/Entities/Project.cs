namespace DAO.Manager.Data.Models.Entities;

/// <summary>
/// Represents a .NET project file (.csproj, .vbproj, etc.) discovered during repository scanning.
/// </summary>
/// <remarks>
/// Projects contain the source code, references, and build configuration for a specific component.
/// They can reference other projects, NuGet packages, and external assemblies.
/// </remarks>
public class Project
{
    /// <summary>
    /// Gets or sets the unique identifier for the project.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key reference to the parent <see cref="Entities.Scan"/>.
    /// </summary>
    public int ScanId { get; set; }

    /// <summary>
    /// Gets or sets a unique identifier calculated from the project's path and scan,
    /// used for deduplication and reference tracking.
    /// </summary>
    public string UniqueIdentifier { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Visual Studio project GUID from the solution file, if present.
    /// </summary>
    public string? VisualStudioGuid { get; set; }

    /// <summary>
    /// Gets or sets the name of the project (typically derived from the file name).
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the full file system path to the project file.
    /// </summary>
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the target framework for the project (e.g., "net8.0", "net48").
    /// </summary>
    public string? TargetFramework { get; set; }

    /// <summary>
    /// Gets or sets the parent scan that discovered this project.
    /// </summary>
    public Scan Scan { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of solutions that contain this project.
    /// </summary>
    public ICollection<Solution> Solutions { get; set; } = new List<Solution>();

    /// <summary>
    /// Gets or sets the collection of other projects that this project references.
    /// </summary>
    public ICollection<Project> ReferencedProjects { get; set; } = new List<Project>();

    /// <summary>
    /// Gets or sets the collection of other projects that reference this project.
    /// </summary>
    public ICollection<Project> ReferencingProjects { get; set; } = new List<Project>();

    /// <summary>
    /// Gets or sets the collection of NuGet packages referenced by this project.
    /// </summary>
    public ICollection<Package> Packages { get; set; } = new List<Package>();

    /// <summary>
    /// Gets or sets the collection of assemblies referenced by this project.
    /// </summary>
    public ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>();
}
