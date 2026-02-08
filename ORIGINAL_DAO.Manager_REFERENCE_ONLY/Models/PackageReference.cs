namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Models;

public class PackageReference
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsDevelopmentDependency { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
}
