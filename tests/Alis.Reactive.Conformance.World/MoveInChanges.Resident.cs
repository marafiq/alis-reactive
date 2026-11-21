namespace Alis.Reactive.Conformance.World;

public static partial class MoveInChanges
{
    public static Seed TheResidentHoldsAnAdmission(string resident) => new() { MoveIn = new MoveInChange { AdmittedResident = resident } };
}
