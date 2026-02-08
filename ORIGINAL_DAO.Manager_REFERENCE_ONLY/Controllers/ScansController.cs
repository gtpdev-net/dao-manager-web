using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ORIGINAL_DAO.Manager_REFERENCE_ONLY.Data;
using ORIGINAL_DAO.Manager_REFERENCE_ONLY.Services;
using ORIGINAL_DAO.Manager_REFERENCE_ONLY.Hubs;
using ORIGINAL_DAO.Manager_REFERENCE_ONLY.Models;

namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Controllers;

public class ScansController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly RepositoryScannerService _scannerService;
    private readonly ILogger<ScansController> _logger;
    private readonly IHubContext<ScanProgressHub> _hubContext;

    public ScansController(
        ApplicationDbContext context,
        RepositoryScannerService scannerService,
        ILogger<ScansController> logger,
        IHubContext<ScanProgressHub> hubContext)
    {
        _context = context;
        _scannerService = scannerService;
        _logger = logger;
        _hubContext = hubContext;
    }

    // GET: Scans
    public async Task<IActionResult> Index()
    {
        var scans = await _context.Scans
            .OrderByDescending(s => s.ScanDate)
            .ToListAsync();
        return View(scans);
    }

    // GET: Scans/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Scans/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("RepositoryPath")] string repositoryPath)
    {
        if (!string.IsNullOrWhiteSpace(repositoryPath))
        {
            try
            {
                // Validate path exists
                if (!Directory.Exists(repositoryPath))
                {
                    ModelState.AddModelError("", $"Directory not found: {repositoryPath}");
                    return View();
                }

                // Create a placeholder scan record to get an ID
                var scan = new Scan
                {
                    ScanDate = DateTime.UtcNow,
                    GitCommitHash = "Scanning...",
                    ShortCommitHash = "Scanning...",
                    RepositoryPath = repositoryPath
                };
                
                _context.Scans.Add(scan);
                await _context.SaveChangesAsync();

                var scanId = scan.Id;

                // Start scan in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var progressReporter = new SignalRProgressReporter(_hubContext, scanId.ToString());
                        
                        // Remove the placeholder scan
                        using var scope = HttpContext.RequestServices.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var scannerService = scope.ServiceProvider.GetRequiredService<RepositoryScannerService>();
                        
                        var scanToRemove = await dbContext.Scans.FindAsync(scanId);
                        if (scanToRemove != null)
                        {
                            dbContext.Scans.Remove(scanToRemove);
                            await dbContext.SaveChangesAsync();
                        }

                        // Perform the actual scan
                        await scannerService.ScanRepositoryAsync(repositoryPath, progressReporter);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in background scan for {Path}", repositoryPath);
                        var progressReporter = new SignalRProgressReporter(_hubContext, scanId.ToString());
                        await progressReporter.ReportError($"Scan failed: {ex.Message}");
                    }
                });

                // Return scan progress view
                return View("ScanProgress", scanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting repository scan: {Path}", repositoryPath);
                ModelState.AddModelError("", $"Error starting scan: {ex.Message}");
            }
        }
        else
        {
            ModelState.AddModelError("", "Repository path is required");
        }

        return View();
    }
}
