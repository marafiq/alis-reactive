using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.RealTime;

/// <summary>
/// Strongly-typed hub for resident status broadcasts.
/// Client method: StatusChanged — matches JS connection.on("StatusChanged", handler).
/// </summary>
public class ResidentStatusHub : Hub<IResidentStatusClient>
{
    /// <summary>
    /// Broadcasts a resident status update to all connected clients.
    /// </summary>
    public async Task SendStatusUpdate(ResidentStatusPayload payload)
        => await Clients.All.StatusChanged(payload);
}
