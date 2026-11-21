namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string EquipmentAvailability = AdmissionClinical + "equipment-availability";
}

public static partial class CareEndpoints
{
    public const string TransportStock = CarePages.EquipmentAvailability + "/stock";
}
