namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Models;

public class Scan
{
    public int Id { get; set; }
    public DateTime ScanDate { get; set; }
    public string GitCommitHash { get; set; } = string.Empty;
    public string ShortCommitHash { get; set; } = string.Empty;
    public string RepositoryPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Solution> Solutions { get; set; } = new List<Solution>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
