namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Models;

public class Solution
{
    public int Id { get; set; }
    public int ScanId { get; set; }
    public string UniqueIdentifier { get; set; } = string.Empty;
    public string? VisualStudioGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string GuidDeterminationMethod { get; set; } = string.Empty;
    public bool IsSingleProjectSolution { get; set; }
    
    // Navigation properties
    public Scan Scan { get; set; } = null!;
}
