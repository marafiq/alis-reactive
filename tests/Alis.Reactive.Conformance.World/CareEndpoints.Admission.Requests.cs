namespace Alis.Reactive.Conformance.World;

public static partial class CareEndpoints
{
    public const string CommunicationInterpreters = CarePages.CommunicationPreferences + "/interpreters";
    public const string Room = "/Rooms/{code}";
    public const string Badge = CarePages.Area + "admission/badge-desk/badge";
    public const string AdmissionWards = CarePages.AdmissionWard + "/wards";
    public const string QuickAdmissionAdmissions = CarePages.QuickAdmission + "/admissions";
    public const string FreeBedNorthWardsFreeBeds = CarePages.Area + "admission/free-bed/north/beds/free";
    public const string FreeBedBeds = CarePages.Area + "admission/free-bed/beds/";
    public const string BedNotesOfNorth = CarePages.BedNotes + "/wards/North/notes";
    public const string BedPhotosOfNorth = CarePages.BedNotes + "/wards/North/photos";
}
