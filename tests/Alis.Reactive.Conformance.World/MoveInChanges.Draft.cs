namespace Alis.Reactive.Conformance.World;

public sealed partial class MoveInChange
{
    public bool LuisUpdatesContact { get; set; }

    public bool LuisSavesTransport { get; set; }
}

public static partial class MoveInChanges
{
    public static Seed LuisUpdatesTheEmergencyContact() => new() { MoveIn = new MoveInChange { LuisUpdatesContact = true } };

    public static Seed LuisSavesTheTransportPlan() => new() { MoveIn = new MoveInChange { LuisSavesTransport = true } };
}

public static partial class CareEndpoints
{
    public const string MoveInAdmissions = CarePages.MoveIn + "/admissions";
    public const string MoveInComparisons = CarePages.MoveIns + "comparisons";
    public const string MoveInArrivalChecklist = CarePages.MoveIns + "arrival-checklists";
}

public static partial class CarePages
{
    public const string MoveInFromReferral = MoveIn + "?Referral=HOSP-42";
}
