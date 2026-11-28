namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed TheEndpointDropsItsConnection(string endpoint) => new() { MoveIn = new MoveInChange { DroppedEndpoint = endpoint } };
}
