namespace Alis.Reactive.Conformance.World;

public static partial class CareEndpoints
{
    public const string MoveInAvailability = CarePages.MoveIns + "availability";

    public const string MoveInServices = CarePages.MoveIns + "services";

    public const string MoveInBedFeed = CarePages.MoveIns + "availability-stream";

    public const string MoveInAvailabilityStream = MoveInBedFeed + "?DraftId=D100&CommunityId=C1";

    public const string MoveInCardStream = MoveInAvailabilityStream + "&Card=true";
}
