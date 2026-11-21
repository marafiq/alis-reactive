using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Care;

public sealed record VisitBoard(string Desk);

public sealed record VisitBoardDesk(string Visit);

public sealed record BoardVisit(DateTimeOffset Starts);

public static class VisitBoardEndpoints
{
    public const string DeskRoute = CarePages.VisitBoard + "/desk";
    public const string KolkataDesk = DeskRoute + "?zone=Asia/Kolkata";
    public const string NewYorkDesk = DeskRoute + "?zone=America/New_York";
    public const string NextVisit = CarePages.VisitBoard + "/next-visit";
}

public static class VisitBoardButtons
{
    public static readonly AppEvent ShowVisit = AppEvent.Declare();

    public static readonly AppEvent OpenKolkataDesk = AppEvent.Declare();

    public static readonly AppEvent ShowDeskVisit = AppEvent.Declare();
}
