namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string AdmissionHandover = MoveIns + "D100/handover";
    public const string AdmissionReception = MoveIns + "D100/reception-desk";
    public const string AdmissionInformation = MoveIns + "information";
}

public static partial class CareEndpoints
{
    public const string AdmissionContactAuthority = CarePages.MoveIns + "authority-on-file";
    public const string AdmissionFilingTray = CarePages.MoveIns + "filing-tray";
    public const string AdmissionDeskStatus = CarePages.MoveIns + "desk-status";
}

public static partial class MoveInChanges
{
    public static Seed TheCoordinatorSignsInAgain() => new() { SessionSignedOut = false };
}
