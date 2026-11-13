namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    private const string Handover = Area + "handover/";

    public const string MedicationAssistant = Handover + "medication-assistant";

    public const string MedicationAssistantOnCall = Handover + "medication-assistant/on-call";

    public const string RoundResidents = Handover + "round-residents";

    public const string RoundResidentsWithLena = Handover + "round-residents/with-lena";

    public const string MedicationCabinet = Handover + "medication-cabinet";

    public const string MedicationCabinetUnset = Handover + "medication-cabinet/unset";

    public const string ShiftNoteWritten = Handover + "shift-note/written";

    public const string ShiftNoteBlank = Handover + "shift-note/blank";

    public const string DoseCardToGive = Handover + "dose-card/give";

    public const string DoseCardToSkip = Handover + "dose-card/skip";

    public const string DischargeChecklist = Handover + "discharge-checklist";

    public const string DischargeChecklistShown = Handover + "discharge-checklist/shown";

    public const string DoseRow = Handover + "dose-row";

    public const string DoseBoard = Handover + "dose-board";

    public const string DoseRecordGiven = Handover + "dose-record/given";

    public const string DoseRecordBlank = Handover + "dose-record/blank";

    public const string NextDoseSet = Handover + "next-dose/set";

    public const string NextDoseOpen = Handover + "next-dose/open";

    public const string ReportSendingReport = Handover + "send-report/report";

    public const string ReportSendingNightReport = Handover + "send-report/night-report";

    public const string HandoverSummaryWritten = Handover + "handover-summary/written";

    public const string HandoverSummaryBlank = Handover + "handover-summary/blank";
}
