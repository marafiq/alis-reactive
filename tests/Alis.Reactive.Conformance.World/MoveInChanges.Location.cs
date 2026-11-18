namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed TheBuildingIsClosed(string building) => new() { MoveIn = new MoveInChange { ClosedBuilding = building } };
}
