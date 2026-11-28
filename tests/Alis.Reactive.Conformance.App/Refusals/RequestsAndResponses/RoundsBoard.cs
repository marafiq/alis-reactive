using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed record RoundsBoard(string Ward);

public static class RoundsBoards
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
