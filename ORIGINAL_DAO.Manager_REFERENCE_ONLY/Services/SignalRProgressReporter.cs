using Microsoft.AspNetCore.SignalR;
using ORIGINAL_DAO.Manager_REFERENCE_ONLY.Hubs;

namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Services;

public class SignalRProgressReporter : IProgressReporter
{
    private readonly IHubContext<ScanProgressHub> _hubContext;
    private readonly string _scanId;

    public SignalRProgressReporter(IHubContext<ScanProgressHub> hubContext, string scanId)
    {
        _hubContext = hubContext;
        _scanId = scanId;
    }

    public async Task ReportProgress(string phase, string message, int percentComplete)
    {
        await _hubContext.Clients.Group($"scan-{_scanId}").SendAsync(
            "ReceiveProgress",
            new
            {
                phase,
                message,
                percentComplete,
                timestamp = DateTime.UtcNow
            });
    }

    public async Task ReportComplete(bool success, string message)
    {
        await _hubContext.Clients.Group($"scan-{_scanId}").SendAsync(
            "ReceiveComplete",
            new
            {
                success,
                message,
                timestamp = DateTime.UtcNow
            });
    }

    public async Task ReportError(string message)
    {
        await _hubContext.Clients.Group($"scan-{_scanId}").SendAsync(
            "ReceiveError",
            new
            {
                message,
                timestamp = DateTime.UtcNow
            });
    }
}
