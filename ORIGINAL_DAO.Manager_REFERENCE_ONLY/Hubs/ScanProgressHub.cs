using Microsoft.AspNetCore.SignalR;

namespace ORIGINAL_DAO.Manager_REFERENCE_ONLY.Hubs;

public class ScanProgressHub : Hub
{
    public async Task JoinScanGroup(string scanId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"scan-{scanId}");
    }

    public async Task LeaveScanGroup(string scanId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"scan-{scanId}");
    }
}
