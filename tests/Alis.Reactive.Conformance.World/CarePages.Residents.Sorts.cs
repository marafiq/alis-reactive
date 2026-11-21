namespace Alis.Reactive.Conformance.World;

public static partial class CarePages
{
    public const string CarersOnDuty = Residents + "carers-on-duty";
    public const string CarersOnDutyInTurkish = CarersOnDuty + "?culture=tr-TR";
    public const string CarersOnDutyInEnglish = CarersOnDuty + "?culture=en-US";
    public const string CarersOnDutyInHawaiian = CarersOnDuty + "?culture=haw-US";
    public const string ResidentTimeline = Residents + "resident-timeline";
    public const string ResidentTimelineInNewYork = ResidentTimeline + "?zone=America/New_York";
    public const string ResidentTimelineLatestVisitFirst = ResidentTimeline + "/latest-visit-first";
    public const string ResidentTimelineLatestVisitFirstInNewYork = ResidentTimelineLatestVisitFirst + "?zone=America/New_York";
}
