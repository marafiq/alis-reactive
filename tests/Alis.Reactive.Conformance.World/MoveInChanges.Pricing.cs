namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed TheDraftIsReadyForAdmission() => new() { MoveIn = new MoveInChange { ReadyForAdmission = true } };

    public static Seed TheDraftAwaitsItsFirstQuote() => new() { MoveIn = new MoveInChange { AwaitsFirstQuote = true } };
}
