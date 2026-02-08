namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Models;

public class Assembly
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string UniqueIdentifier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AssemblyFileName { get; set; } = string.Empty;
    public string OutputType { get; set; } = string.Empty;
    public string ProjectStyle { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = string.Empty;
    public string ProjectFilePath { get; set; } = string.Empty;
    
    // Navigation properties
    public Project Project { get; set; } = null!;
}
