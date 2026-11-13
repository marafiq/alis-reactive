namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    private const string Round = Area + "round/";

    public const string WardBoard = Round + "ward-board";
    public const string MedicationChartWithAspirinGiven = Round + "medication-chart/aspirin-given";
    public const string MedicationChartWithAspirinDue = Round + "medication-chart/aspirin-due";
    public const string MedicationChartWithNoAspirin = Round + "medication-chart/no-aspirin";
    public const string DoseCartWithACopayOfOneTenth = Round + "dose-cart/copay-of-one-tenth";
    public const string DoseCartWithACopayOfOneEighth = Round + "dose-cart/copay-of-one-eighth";
    public const string DoseCartWithACopayOfTenHundredths = Round + "dose-cart/copay-of-ten-hundredths";
    public const string DoseRoundWithAWardlessDose = Round + "dose-round/a-wardless-dose";
    public const string DoseRoundWithEveryDoseInAWard = Round + "dose-round/every-dose-in-a-ward";
    public const string StaffSearch = Round + "staff-search";
    public const string StaffSearchLogged = Round + "staff-search/logged";
    public const string StaffRosterWithAnOpenShift = Round + "staff-roster/an-open-shift";
    public const string StaffRosterWithEveryShiftFilled = Round + "staff-roster/every-shift-filled";
    public const string SupplyRound = Round + "supplies";
    public const string NurseCall = Round + "nurse-call";
    public const string NurseCallCopiedFirst = Round + "nurse-call/copied-first";
    public const string CallDesk = Round + "call-desk";
    public const string CallDeskWithNoLogHandler = Round + "call-desk/no-log-handler";
    public const string RoundStaffWithAChargeNurse = Round + "staff/charge-nurse";
    public const string RoundStaffWithAStaffNurse = Round + "staff/staff-nurse";
    public const string MedicationCheckOfAResidentWithNoChart = Round + "medication-check/no-chart";
    public const string MedicationCheckOfAResidentWhoseChartSaysMissed = Round + "medication-check/missed";
    public const string MedicationTestCheck = Round + "medication-check/test";
    public const string SupplyScanOfFiveVials = Round + "supply-scan/five-vials";
    public const string SupplyScanOfAnEmptyBox = Round + "supply-scan/empty-box";
}
