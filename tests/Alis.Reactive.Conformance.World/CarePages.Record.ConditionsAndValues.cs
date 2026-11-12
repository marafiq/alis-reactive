namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    private const string Record = Area + "record/";

    public const string RoomBooking = Record + "room-booking";
    public const string FluidBalanceMeasuredBeforeItsWrite = Record + "fluid-balance/measured-before-its-write";
    public const string FluidBalanceMeasuredAfterItsWrite = Record + "fluid-balance/measured-after-its-write";
    public const string VitalSigns = Record + "vital-signs";
    public const string FallNote = Record + "fall-note";
    public const string AlarmNote = Record + "alarm-note";
    public const string PumpLogWithNoAlarm = Record + "pump-log/no-alarm";
    public const string PumpLogWithAnAlarm = Record + "pump-log/an-alarm";
    public const string WeightCheckOfAnUnweighedResident = Record + "weight-check/unweighed";
    public const string WeightCheckOfAWeighedResident = Record + "weight-check/weighed";
    public const string FluidOrders = Record + "fluid-orders";
    public const string PumpCaptionOfANamedRound = Record + "pump-caption/named-round";
    public const string PumpCaptionOfAnUnnamedRound = Record + "pump-caption/unnamed-round";
    public const string Chart = Record + "chart";
    public const string ChartAtTheMedicationsTab = Chart + "?TAB=medications";
}
