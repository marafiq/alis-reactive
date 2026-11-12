namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    private const string Admission = Area + "admission/";

    public const string RoomsOfTheFloor = Admission + "rooms";
    public const string OpenRoom = Admission + "open-room";
    public const string IntakeContact = Admission + "intake-contact";
    public const string BadgeDesk = Admission + "badge-desk";
    public const string BedCommit = Admission + "bed-commit";
    public const string BedCommitWithAConflictRoute = Admission + "bed-commit/conflict-route";
}
