namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed TheCoordinatorMayNotAdmit(string coordinator) => new() { MoveIn = new MoveInChange { CoordinatorWithoutAdmission = coordinator } };

    public static Seed TheBedIsTaken() => new() { MoveIn = new MoveInChange { BedTaken = true } };
}
