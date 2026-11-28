namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string WardAvailabilityBulletin = AdmissionClinical + "ward-availability-bulletin";
    public const string PersonalWardBulletin = WardAvailabilityBulletin + "?personal=true";
}

public static partial class CareEndpoints
{
    public const string WardAvailability = CarePages.WardAvailabilityBulletin + "/wards";
}
