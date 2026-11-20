using Alis.Reactive.Conformance.World;
using Alis.Reactive.Declarations;
using Alis.Reactive.Realtime;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CoordinatorLinePage
{
    public string DraftId { get; set; } = string.Empty;
    public string CoordinatorId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int DisplayedSequence { get; set; }
}

public sealed class CoordinatorHub : SignalRHub
{
    public static readonly SignalRSource<CoordinatorHub> Line = At<CoordinatorHub>(CareEndpoints.MoveInCoordinatorLine);

    public HubPush<CoordinatorLineReply> Changed => Push<CoordinatorLineReply>();

    public HubPush<CoordinatorLineRefusal> Refused => Push<CoordinatorLineRefusal>();

    public void Join(Arg<string> draftId, Arg<string> coordinatorId) => Send([draftId, coordinatorId]);

    public MethodResult<CoordinatorLineReply> Snapshot(Arg<string> draftId) => Invoke<CoordinatorLineReply>([draftId]);

    public MethodResult<CoordinatorMessage> Post(Arg<string> draftId, Arg<string> coordinatorId, Arg<string> text)
        => Invoke<CoordinatorMessage>([draftId, coordinatorId, text]);
}

public sealed record CoordinatorLineRefusal(string Reason);

public sealed class WardPreparation
{
    public string DraftId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string BedId { get; set; } = string.Empty;
    public string CoordinatorId { get; set; } = string.Empty;
}
