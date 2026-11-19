namespace Alis.Reactive.Conformance.World;

public sealed partial class MoveInChange
{
    public bool WithdrawAmoxicillinAlternative { get; set; }

    public bool RenewMedicationOrders { get; set; }
}

public static partial class MoveInChanges
{
    public static Seed ThePharmacyWithdrawsTheAmoxicillinAlternative() => new() { MoveIn = new MoveInChange { WithdrawAmoxicillinAlternative = true } };

    public static Seed ThePharmacyRenewsTheOrders() => new() { MoveIn = new MoveInChange { RenewMedicationOrders = true } };
}

public static partial class CareEndpoints
{
    public const string MoveInMedications = CarePages.MoveIn + "/medications";
    public const string MoveInPharmacy = CarePages.MoveIn + "/pharmacy-review";
    public const string MoveInDirections = CarePages.MoveIns + "pharmacy-instructions";
    public const string MoveInAlternatives = CarePages.MoveIns + "pharmacy-alternatives";
}
