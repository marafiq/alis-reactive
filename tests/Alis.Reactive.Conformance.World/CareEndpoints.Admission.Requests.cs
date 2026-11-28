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
    public const string BedNotesOfNortheast = CarePages.BedNotes + "/wards/Northeast/notes";
    public const string BedPhotosOfNortheast = CarePages.BedNotes + "/wards/Northeast/photos";
    public const string SavedBedNotesOfNorth = CarePages.BedNotes + "/wards/North/saved-notes";
    public const string EveningSavedBedNotesOfNorth = CarePages.BedNotesOnTheEveningShift + "/wards/North/saved-notes";
    public const string BedHandoversOfNorth = CarePages.BedNotes + "/wards/North/handovers";
}
