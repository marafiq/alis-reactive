namespace Alis.Reactive.Conformance.World;

public static partial class CareEndpoints
{
    public const string RoundCensusCount = "/care/round/census/count";
    public const string FreeBeds = "/care/round/bed-finder/free-beds";
    public const string WardStock = "/care/round/pharmacy-check/ward-stock";
    public const string WardDoses = "/care/round/wards/{Ward}/doses";
    public const string WardCensus = "/care/round/wards/{Ward}/census";
    public const string OrderCheckOrders = "/care/round/order-check/orders";
    public const string DoseIntakeSaves = "/care/round/dose-intake/saves";
}
