namespace Alis.Reactive.Conformance.World;

public sealed partial class MoveInChange
{
    public bool PharmacyCapacityExhausted { get; set; }

    public bool RefuseServiceRecord { get; set; }

    public bool PharmacyPriceChanged { get; set; }

    public bool AcceptanceConnectionRestored { get; set; }
}

public static partial class MoveInChanges
{
    public static Seed PharmacyCapacityIsExhausted() => new() { MoveIn = new MoveInChange { PharmacyCapacityExhausted = true } };

    public static Seed TheServiceRecordIsUnavailable() => new() { MoveIn = new MoveInChange { RefuseServiceRecord = true } };

    public static Seed ThePharmacyPriceChanges() => new() { MoveIn = new MoveInChange { PharmacyPriceChanged = true } };

    public static Seed TheAcceptanceConnectionIsRestored() => new() { MoveIn = new MoveInChange { AcceptanceConnectionRestored = true } };
}

public static partial class CareEndpoints
{
    public const string MoveInServiceCount = CarePages.MoveIn + "/review/visits";
    public const string MoveInServiceRecord = CarePages.MoveIn + "/review/services";
    public const string MoveInCurrentOffer = CarePages.MoveIns + "current-quote";
    public const string MoveInAcceptance = CarePages.MoveIn + "/quote-acceptances";
}
