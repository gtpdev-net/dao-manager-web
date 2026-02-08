namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Models;

public class Project
{
    public int Id { get; set; }
    public int ScanId { get; set; }
    public string UniqueIdentifier { get; set; } = string.Empty;
    public string? VisualStudioGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string GuidDeterminationMethod { get; set; } = string.Empty;
    public string? TargetFramework { get; set; }
    public string ProjectStyle { get; set; } = string.Empty;
    
    // Navigation properties
    public Scan Scan { get; set; } = null!;
    public ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>();
    public ICollection<PackageReference> PackageReferences { get; set; } = new List<PackageReference>();
    public ICollection<AssemblyReference> AssemblyReferences { get; set; } = new List<AssemblyReference>();
}
