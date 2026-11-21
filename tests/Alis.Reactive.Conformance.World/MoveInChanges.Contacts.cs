namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed TheDraftHoldsFourContacts() => new() { MoveIn = new MoveInChange { FourContacts = true } };

    public static Seed TheContactHoldsNoId(string contact) => new() { MoveIn = new MoveInChange { ContactWithNoId = contact } };

    public static Seed ThePayersContactIs(string contact) => new() { MoveIn = new MoveInChange { PayersContact = contact } };
}
