using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HallRing
{
    public int Count { get; init; }
}

public static class HallBells
{
    public static readonly AppEvent RingTheBell = AppEvent.Declare();

    public static readonly AppEvent<HallRing> Rang = AppEvent.Declare<HallRing>();
}
