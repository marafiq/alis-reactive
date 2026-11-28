namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed TheWindowBedIsReleased() => new() { MoveIn = new MoveInChange { FreeWindowBed = true } };

    public static Seed TheCoordinatorServesHarbor(bool serves) => new() { MoveIn = new MoveInChange { HarborAccess = serves } };

    public static Seed TheAvailabilityConnectionIsRestored() => new() { MoveIn = new MoveInChange { AvailabilityConnectionRestored = true } };
}
