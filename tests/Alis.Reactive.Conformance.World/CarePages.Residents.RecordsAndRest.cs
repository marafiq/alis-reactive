namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string CensusDaysWithADayHeldAsSlashedText = Residents + "census/days/a-day-held-as-slashed-text";
    public const string CensusDaysWithEveryDayHeldAsADate = Residents + "census/days/every-day-held-as-a-date";
    public const string CensusDaysWithADayNoMonthHolds = Residents + "census/days/a-day-no-month-holds";
    public const string CensusItemPositions = Residents + "census/item-positions";
    public const string CensusFirstActiveItems = Residents + "census/first-active-items";
    public const string CensusActiveCounts = Residents + "census/active-counts";
    public const string CensusScoresWithAHeadCountOnEveryItem = Residents + "census/scores/a-head-count-on-every-item";
    public const string CensusScoresWithAnItemLackingItsHeadCount = Residents + "census/scores/an-item-lacking-its-head-count";
    public const string VisitCountsWithAVisitInTheNorthWard = Residents + "visit-counts/a-visit-in-the-north-ward";
    public const string VisitCountsWithEveryVisitInTheSouthWard = Residents + "visit-counts/every-visit-in-the-south-ward";
    public const string ResidentBoard = Residents + "board";
    public const string WardBeds = Residents + "ward-beds";
    public const string WardBedsAskedAsNoNumber = WardBeds + "?beds=abc";
    public const string WardBedsAskedAsSeven = WardBeds + "?beds=7";
    public const string WardsOnFile = Residents + "wards";
    public const string WardOnFile = WardsOnFile + "/{Ward}";
    public const string TheEastWardAskedAsWest = WardsOnFile + "/East?ward=West";
    public const string ResidentsOfTheEastWing = Residents + "by-wing/east";
    public const string ResidentsOfTheWestWing = Residents + "by-wing/west";
}
