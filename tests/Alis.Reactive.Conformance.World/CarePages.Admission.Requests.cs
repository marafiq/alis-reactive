namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    private const string Admission = Area + "admission/";

    public const string RoomsOfTheFloor = Admission + "rooms";
    public const string OpenRoom = Admission + "open-room";
    public const string IntakeContact = Admission + "intake-contact";
    public const string IntakeAdmit = Admission + "intake-admit";
    public const string IntakeAdmitWardApart = Admission + "intake-admit/ward-apart";
    public const string VisitAdmit = Admission + "visit-admit";
    public const string VisitAdmitOwnSection = Admission + "visit-admit/own-section";
    public const string BadgeDesk = Admission + "badge-desk";
    public const string BedCommit = Admission + "bed-commit";
    public const string BedCommitWithAConflictRoute = Admission + "bed-commit/conflict-route";
    public const string AdmissionApplication = Admission + "admission-application";
    public const string IntakeDocuments = Admission + "intake-documents";
    public const string QuickAdmission = Admission + "quick-admission";
}
