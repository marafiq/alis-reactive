using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed class MoveInCoordinatorHub : Hub
{
    private WorldData World => Context.GetHttpContext()!.RequestServices.GetRequiredService<CurrentWorld>().Data;
    private MoveInDesk Desk => World.Owns(MoveInDesk.Opened);

    public override Task OnConnectedAsync()
    {
        var reader = World.Streams.Of(CareEndpoints.MoveInCoordinatorLine).Opens(Context.ConnectionAborted);
        _ = DisconnectWhenClosed(reader.Completion, Context.GetHttpContext()!);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Desk.LeaveLine(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public void Join(string draftId, string coordinatorId)
    {
        if (!Desk.JoinLine(Context.ConnectionId, draftId, coordinatorId)) throw new HubException("Membership revoked.");
    }

    public CoordinatorLineReply Snapshot(string draftId)
        => Desk.ReadLine(Context.ConnectionId, draftId) ?? throw new HubException("Membership revoked.");

    public async Task<CoordinatorMessage> Post(string draftId, string coordinatorId, string text)
    {
        var result = Desk.PostToLine(Context.ConnectionId, draftId, coordinatorId, text);
        await World.Reach(GateName.Of(RunGate.Settle), CareEndpoints.MoveInCoordinatorLine);
        if (result.Message is null || result.Snapshot is null)
        {
            await new CoordinatorHub().Refused.SendAsync(Clients.Caller, new CoordinatorLineRefusal(result.Error));
            throw new HubException(result.Error);
        }
        await new CoordinatorHub().Changed.SendAsync(Clients.Clients(result.Recipients), result.Snapshot);
        return result.Message;
    }

    private static async Task DisconnectWhenClosed(Task closed, HttpContext transport)
    {
        await closed;
        transport.Abort();
    }
}
