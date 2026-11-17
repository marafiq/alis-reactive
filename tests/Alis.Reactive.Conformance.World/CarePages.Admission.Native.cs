namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string VisitSchedule = Admission + "visit-schedule";
    public const string VisitWeek = Admission + "visit-week";
    public const string IntakeBed = Admission + "intake-bed";
    public const string WingMove = Admission + "wing-move";
    public const string AddressedMoveIn = Admission + "addressed-move-in";
    public const string WingTransfer = Admission + "wing-transfer";
    public const string WingChange = Admission + "wing-change";
    public const string WingChangeWithNoWing = WingChange + "/no-wing";
    public const string VisitDaysChange = Admission + "visit-days-change";
    public const string VisitRoutine = Admission + "visit-routine";
}
