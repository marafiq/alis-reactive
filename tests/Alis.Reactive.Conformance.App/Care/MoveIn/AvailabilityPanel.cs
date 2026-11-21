using Alis.Reactive.Realtime;

namespace Alis.Reactive.Conformance.App.Care;

public sealed record AvailabilityPanel(string DraftId, string CommunityId, string BedId, PayerKind PayerKind)
{
    public int DisplayedRevision { get; set; }
}

public sealed record AvailabilityCard(string DraftId, string CommunityId, string BedId)
{
    public int Revision { get; set; }
}

public sealed class AvailabilityQuery
{
    public string DraftId { get; set; } = string.Empty;

    public string CommunityId { get; set; } = string.Empty;

    public string BedId { get; set; } = string.Empty;

    public int SinceRevision { get; set; }
}

public sealed class ServiceCatalogueQuery
{
    public string DraftId { get; set; } = string.Empty;

    public CareLevel CareLevel { get; set; }

    public PayerKind PayerKind { get; set; }

    public string Culture { get; set; } = string.Empty;
}

public sealed class BedAvailabilityFeed : ServerPushStream
{
    public ServerEvent<BedFeedOpened> Opened => Message<BedFeedOpened>();

    public ServerEvent<BedChanged> BedChanged => Message<BedChanged>();

    internal static string Address(string draftId, string communityId, bool card)
        => Conformance.World.CareEndpoints.MoveInBedFeed + "?DraftId=" + Uri.EscapeDataString(draftId) + "&CommunityId=" + Uri.EscapeDataString(communityId) + (card ? "&Card=true" : string.Empty);
}

public sealed record BedFeedOpened(int Connection);
