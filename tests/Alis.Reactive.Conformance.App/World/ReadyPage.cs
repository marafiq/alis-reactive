using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

public sealed record ReadyPage(string Ward, bool SendsRequests);

public sealed record ReadyDose(string Ward, bool Given);

public static class ReadyEndpoints
{
    public const string Doses = WorldEndpoints.Ready + "/doses";
}
