namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed TheAuthorityIsRevoked(string reference) => new() { MoveIn = new MoveInChange { RevokedAuthority = reference } };
}
