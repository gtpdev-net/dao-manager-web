using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using ORIGINAL_DAO.Manager_REFERENCE_ONLY.Data;
using ORIGINAL_DAO.Manager_REFERENCE_ONLY.Models;

namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Services;

public class RepositoryScannerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RepositoryScannerService> _logger;

    public RepositoryScannerService(ApplicationDbContext context, ILogger<RepositoryScannerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Scan> ScanRepositoryAsync(string repositoryPath, IProgressReporter? progressReporter = null)
    {
        try
        {
            _logger.LogInformation("Starting repository scan at {Path}", repositoryPath);
            await progressReporter?.ReportProgress("Initializing", "Starting repository scan...", 0)!;

            // Validate repository path
            if (!Directory.Exists(repositoryPath))
            {
                throw new DirectoryNotFoundException($"Repository path not found: {repositoryPath}");
            }

            // Get git commit hash
            await progressReporter?.ReportProgress("Git", "Reading git commit information...", 5)!;
            var (commitHash, shortHash) = await GetGitCommitHashAsync(repositoryPath);

            // Create scan
            await progressReporter?.ReportProgress("Database", "Creating scan record...", 10)!;
            var scan = new Scan
            {
                ScanDate = DateTime.UtcNow,
                GitCommitHash = commitHash,
                ShortCommitHash = shortHash,
                RepositoryPath = repositoryPath
            };

            _context.Scans.Add(scan);
            await _context.SaveChangesAsync();

            // Find solutions
            await progressReporter?.ReportProgress("Solutions", "Scanning for solution files...", 20)!;
            await FindSolutionsAsync(scan, repositoryPath);
            await progressReporter?.ReportProgress("Solutions", "Solution files processed", 35)!;

            // Find projects
            await progressReporter?.ReportProgress("Projects", "Scanning for project files...", 40)!;
            await FindProjectsAsync(scan, repositoryPath);
            await progressReporter?.ReportProgress("Projects", "Project files processed", 60)!;

            // Extract assemblies
            await progressReporter?.ReportProgress("Assemblies", "Extracting assembly information...", 70)!;
            await ExtractAssembliesAsync(scan);
            await progressReporter?.ReportProgress("Assemblies", "Assembly information extracted", 95)!;

            _logger.LogInformation("Repository scan completed. Scan ID: {ScanId}", scan.Id);
            await progressReporter?.ReportProgress("Complete", "Scan completed successfully!", 100)!;
            await progressReporter?.ReportComplete(true, $"Scan completed successfully. Scan ID: {scan.Id}")!;

            return scan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during repository scan");
            await progressReporter?.ReportError($"Scan failed: {ex.Message}")!;
            throw;
        }
    }

    private async Task<(string commitHash, string shortHash)> GetGitCommitHashAsync(string repositoryPath)
    {
        try
        {
            var gitDir = Path.Combine(repositoryPath, ".git");
            if (!Directory.Exists(gitDir))
            {
                _logger.LogWarning("Not a git repository: {Path}", repositoryPath);
                return ("N/A", "N/A");
            }

            var headFile = Path.Combine(gitDir, "HEAD");
            if (!File.Exists(headFile))
            {
                return ("N/A", "N/A");
            }

            var headContent = await File.ReadAllTextAsync(headFile);
            headContent = headContent.Trim();

            if (headContent.StartsWith("ref:"))
            {
                // HEAD points to a reference
                var refPath = headContent.Substring(5).Trim();
                var refFile = Path.Combine(gitDir, refPath.Replace('/', Path.DirectorySeparatorChar));
                
                if (File.Exists(refFile))
                {
                    var commitHash = (await File.ReadAllTextAsync(refFile)).Trim();
                    return (commitHash, commitHash.Substring(0, Math.Min(8, commitHash.Length)));
                }
            }
            else
            {
                // HEAD contains the commit hash directly
                var commitHash = headContent;
                return (commitHash, commitHash.Substring(0, Math.Min(8, commitHash.Length)));
            }

            return ("N/A", "N/A");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting git commit hash");
            return ("N/A", "N/A");
        }
    }

    private async Task FindSolutionsAsync(Scan scan, string repositoryPath)
    {
        _logger.LogInformation("Finding solution files...");

        var solutionFiles = Directory.GetFiles(repositoryPath, "*.sln", SearchOption.AllDirectories);
        
        _logger.LogInformation("Found {Count} solution files", solutionFiles.Length);

        foreach (var filePath in solutionFiles)
        {
            try
            {
                var solution = await ExtractSolutionInfoAsync(scan.Id, filePath);
                if (solution != null)
                {
                    _context.Solutions.Add(solution);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing solution file: {FilePath}", filePath);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<Solution?> ExtractSolutionInfoAsync(int scanId, string filePath)
    {
        var content = await File.ReadAllTextAsync(filePath);
        var uniqueId = GenerateGuidFromPath(filePath);
        var name = Path.GetFileNameWithoutExtension(filePath);

        // Extract solution GUID
        string? visualStudioGuid = null;
        string guidMethod = "Not found";

        var solutionGuidMatch = System.Text.RegularExpressions.Regex.Match(content, 
            @"SolutionGuid\s*=\s*\{([0-9A-Fa-f\-]+)\}");

        if (solutionGuidMatch.Success)
        {
            visualStudioGuid = solutionGuidMatch.Groups[1].Value;
            guidMethod = "SolutionGuid";
        }

        // Count referenced projects
        var projectMatches = System.Text.RegularExpressions.Regex.Matches(content,
            @"Project\(""[^""]+""\)\s*=\s*""[^""]+""\s*,\s*""([^""]+)""\s*,\s*""\{([0-9A-Fa-f\-]+)\}""");

        var projectCount = 0;
        string? singleProjectGuid = null;
        var solutionDir = Path.GetDirectoryName(filePath);

        foreach (System.Text.RegularExpressions.Match match in projectMatches)
        {
            var projectPath = match.Groups[1].Value;
            if (projectPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                var absolutePath = Path.GetFullPath(Path.Combine(solutionDir!, projectPath));
                if (File.Exists(absolutePath))
                {
                    projectCount++;
                    singleProjectGuid = match.Groups[2].Value;
                }
            }
        }

        var isSingleProject = projectCount == 1;
        if (isSingleProject && singleProjectGuid != null)
        {
            visualStudioGuid = singleProjectGuid;
            guidMethod = "From referenced project";
        }

        return new Solution
        {
            ScanId = scanId,
            UniqueIdentifier = uniqueId,
            VisualStudioGuid = visualStudioGuid,
            Name = name,
            FilePath = filePath,
            GuidDeterminationMethod = guidMethod,
            IsSingleProjectSolution = isSingleProject
        };
    }

    private async Task FindProjectsAsync(Scan scan, string repositoryPath)
    {
        _logger.LogInformation("Finding project files...");

        var projectFiles = Directory.GetFiles(repositoryPath, "*.csproj", SearchOption.AllDirectories);
        
        _logger.LogInformation("Found {Count} project files", projectFiles.Length);

        // First pass: create map of project paths to GUIDs from solution files
        var projectGuidMap = await BuildProjectGuidMapAsync(repositoryPath);

        foreach (var filePath in projectFiles)
        {
            try
            {
                var project = await ExtractProjectInfoAsync(scan.Id, filePath, projectGuidMap);
                if (project != null)
                {
                    _context.Projects.Add(project);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing project file: {FilePath}", filePath);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<Dictionary<string, string>> BuildProjectGuidMapAsync(string repositoryPath)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var solutionFiles = Directory.GetFiles(repositoryPath, "*.sln", SearchOption.AllDirectories);

        foreach (var solutionPath in solutionFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(solutionPath);
                var solutionDir = Path.GetDirectoryName(solutionPath);

                var projectMatches = System.Text.RegularExpressions.Regex.Matches(content,
                    @"Project\(""[^""]+""\)\s*=\s*""[^""]+""\s*,\s*""([^""]+)""\s*,\s*""\{([0-9A-Fa-f\-]+)\}""");

                foreach (System.Text.RegularExpressions.Match match in projectMatches)
                {
                    var relativePath = match.Groups[1].Value;
                    var projectGuid = match.Groups[2].Value;

                    if (relativePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    {
                        var absolutePath = Path.GetFullPath(Path.Combine(solutionDir!, relativePath));
                        map[absolutePath] = projectGuid;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing solution for project GUIDs: {SolutionPath}", solutionPath);
            }
        }

        return map;
    }

    private async Task<Project?> ExtractProjectInfoAsync(int scanId, string filePath, Dictionary<string, string> projectGuidMap)
    {
        var content = await File.ReadAllTextAsync(filePath);
        var uniqueId = GenerateGuidFromPath(filePath);
        var name = Path.GetFileNameWithoutExtension(filePath);

        XDocument projectXml;
        try
        {
            projectXml = XDocument.Parse(content);
        }
        catch
        {
            _logger.LogWarning("Failed to parse project XML: {FilePath}", filePath);
            return null;
        }

        var root = projectXml.Root;
        if (root == null) return null;

        // Determine if SDK-style
        var isSdkStyle = root.Attribute("Sdk") != null;
        var projectStyle = isSdkStyle ? "SDK-style" : "Legacy";

        // Extract Visual Studio GUID
        string? visualStudioGuid = null;
        string guidMethod = "Not found";

        // Try to get from ProjectGuid element (legacy projects)
        var guidElement = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "ProjectGuid");
        if (guidElement != null && !string.IsNullOrWhiteSpace(guidElement.Value))
        {
            visualStudioGuid = guidElement.Value.Trim('{', '}');
            guidMethod = "ProjectGuid element";
        }
        else if (projectGuidMap.TryGetValue(filePath, out var guid))
        {
            visualStudioGuid = guid;
            guidMethod = "From solution file";
        }

        // Extract target framework
        string? targetFramework = null;
        var tfElement = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "TargetFramework");
        var tfsElement = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "TargetFrameworks");
        
        if (tfElement != null)
        {
            targetFramework = tfElement.Value;
        }
        else if (tfsElement != null)
        {
            targetFramework = tfsElement.Value;
        }
        else
        {
            var tfvElement = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "TargetFrameworkVersion");
            if (tfvElement != null)
            {
                targetFramework = tfvElement.Value;
            }
        }

        // Count project references
        var projectReferences = root.Descendants()
            .Where(e => e.Name.LocalName == "ProjectReference")
            .Count();

        return new Project
        {
            ScanId = scanId,
            UniqueIdentifier = uniqueId,
            VisualStudioGuid = visualStudioGuid,
            Name = name,
            FilePath = filePath,
            GuidDeterminationMethod = guidMethod,
            TargetFramework = targetFramework ?? "N/A",
            ProjectStyle = projectStyle
        };
    }

    private async Task ExtractAssembliesAsync(Scan scan)
    {
        _logger.LogInformation("Extracting assembly information...");

        var projects = await _context.Projects.Where(p => p.ScanId == scan.Id).ToListAsync();

        foreach (var project in projects)
        {
            try
            {
                var assembly = await ExtractAssemblyInfoAsync(scan.Id, project);
                if (assembly != null)
                {
                    _context.Assemblies.Add(assembly);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting assembly from project: {FilePath}", project.FilePath);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<Assembly?> ExtractAssemblyInfoAsync(int scanId, Project project)
    {
        var content = await File.ReadAllTextAsync(project.FilePath);
        
        XDocument projectXml;
        try
        {
            projectXml = XDocument.Parse(content);
        }
        catch
        {
            return null;
        }

        var root = projectXml.Root;
        if (root == null) return null;

        // Determine OutputType
        var outputType = "Library";
        var outputTypeElement = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "OutputType");
        if (outputTypeElement != null && !string.IsNullOrWhiteSpace(outputTypeElement.Value))
        {
            outputType = outputTypeElement.Value;
        }

        // Get AssemblyName
        var assemblyName = project.Name;
        var assemblyNameElement = root.Descendants().FirstOrDefault(e => e.Name.LocalName == "AssemblyName");
        if (assemblyNameElement != null && !string.IsNullOrWhiteSpace(assemblyNameElement.Value))
        {
            assemblyName = assemblyNameElement.Value;
        }

        // Determine assembly extension
        var extension = outputType.ToLower() switch
        {
            "exe" => ".exe",
            "winexe" => ".exe",
            "module" => ".netmodule",
            _ => ".dll"
        };

        var assemblyFileName = assemblyName + extension;
        
        // Generate unique identifier based on project path and assembly name
        var uniqueId = GenerateGuidFromString($"{project.FilePath.ToLower()}|{assemblyName.ToLower()}");

        return new Assembly
        {
            ProjectId = project.Id,
            UniqueIdentifier = uniqueId,
            Name = assemblyName,
            AssemblyFileName = assemblyFileName,
            OutputType = outputType,
            ProjectStyle = project.ProjectStyle,
            TargetFramework = project.TargetFramework ?? "N/A",
            ProjectFilePath = project.FilePath
        };
    }

    private string GenerateGuidFromPath(string path)
    {
        var normalizedPath = Path.GetFullPath(path).ToLower();
        return GenerateGuidFromString(normalizedPath);
    }

    private string GenerateGuidFromString(string input)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        
        return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}-{4:X2}{5:X2}-{6:X2}{7:X2}-{8:X2}{9:X2}-{10:X2}{11:X2}{12:X2}{13:X2}{14:X2}{15:X2}",
            hashBytes[0], hashBytes[1], hashBytes[2], hashBytes[3],
            hashBytes[4], hashBytes[5],
            hashBytes[6], hashBytes[7],
            hashBytes[8], hashBytes[9],
            hashBytes[10], hashBytes[11], hashBytes[12], hashBytes[13], hashBytes[14], hashBytes[15]);
    }
}
