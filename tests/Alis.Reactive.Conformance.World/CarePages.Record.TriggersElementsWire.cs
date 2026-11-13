namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    private const string RefusedOnTheRecord = RefusalPages.Area + "care/record/";

    public const string RecordCard = Record + "record-card";
    public const string RecordCardWithNoSummary = Record + "record-card/no-summary";
    public const string PinnedNote = Record + "pinned-note";
    public const string PartyBell = Record + "party-bell";
    public const string EscortDesk = Record + "escort-desk";
    public const string NotesDesk = Record + "notes-desk";
    public const string RoundNote = Record + "round-note";
    public const string PanelAcknowledgement = Record + "panel-acknowledgement";
    public const string AlarmPanels = Record + "alarm-panels";
    public const string FlaggedAlarm = Record + "flagged-alarm";
    public const string FlaggedAlarmWrittenLate = RefusedOnTheRecord + "flagged-alarm/written-late";
    public const string FlaggedAlarmWrittenFirst = Record + "flagged-alarm/written-first";
    public const string CountedAlarm = Record + "counted-alarm";
    public const string CountedAlarmWrittenAfterAnAwaitedCheck = RefusedOnTheRecord + "counted-alarm/written-after-an-awaited-check";
    public const string CountedAlarmWrittenAfterAFlagCheck = Record + "counted-alarm/written-after-a-flag-check";
    public const string VisitPlanner = Record + "visit-planner";
    public const string RoundBoard = Record + "round-board";
    public const string WardTransfer = Record + "ward-transfer";
    public const string VisitorList = Record + "visitor-list";
    public const string VisitorListOfASmallParty = Record + "visitor-list/small-party";
}
