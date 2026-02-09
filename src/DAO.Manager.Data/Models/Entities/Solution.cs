namespace DAO.Manager.Data.Models.Entities;

/// <summary>
/// Represents a Visual Studio solution (.sln) file discovered during repository scanning.
/// </summary>
/// <remarks>
/// Solutions are container files that group related projects and define their build configurations.
/// </remarks>
public class Solution
{
    /// <summary>
    /// Gets or sets the unique identifier for the solution.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key reference to the parent <see cref="Entities.Scan"/>.
    /// </summary>
    public int ScanId { get; set; }

    /// <summary>
    /// Gets or sets a unique identifier calculated from the solution's path and scan,
    /// used for deduplication and reference tracking.
    /// </summary>
    public string UniqueIdentifier { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Visual Studio solution GUID from the .sln file, if present.
    /// </summary>
    public string? VisualStudioGuid { get; set; }

    /// <summary>
    /// Gets or sets the full file system path to the solution file.
    /// </summary>
    public string FilePath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the solution (typically derived from the file name).
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the parent scan that discovered this solution.
    /// </summary>
    public Scan Scan { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of projects contained within this solution.
    /// </summary>
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
