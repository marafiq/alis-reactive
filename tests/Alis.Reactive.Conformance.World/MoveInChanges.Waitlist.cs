namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed TheWaitlistCloses(string communityId) => new() { MoveIn = new MoveInChange { ClosedWaitlistCommunity = communityId } };
}
