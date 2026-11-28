namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string ArrivalBooking = AdmissionClinical + "arrival-scheduling/booking";

    public const string ArrivalTransport = AdmissionClinical + "arrival-scheduling/transport";

    public const string ArrivalCalendarReview = AdmissionClinical + "arrival-scheduling/calendar";

    public const string ArrivalCalendarSigned = AdmissionClinical + "arrival-scheduling/calendar/signed";

    public const string ArrivalCalendarHeld = AdmissionClinical + "arrival-scheduling/calendar/review";

    public const string ArrivalInterview = AdmissionClinical + "arrival-scheduling/interview";

    public const string ArrivalWindow = AdmissionClinical + "arrival-scheduling/window";

    public const string FileArrival = ArrivalBooking + "/file";

    public const string FileArrivalTransport = ArrivalTransport + "/file";
}
