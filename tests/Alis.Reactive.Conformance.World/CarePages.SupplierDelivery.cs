namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string SupplierDelivery = AdmissionClinical + "supplier-delivery";
}

public static partial class CareEndpoints
{
    public const string SupplierCatalogue = CarePages.SupplierDelivery + "/catalogue";
}
