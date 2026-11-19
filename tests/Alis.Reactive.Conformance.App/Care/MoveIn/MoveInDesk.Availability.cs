using System.Text.Json;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private readonly HashSet<(string Draft, string Community, WorldStream Stream)> _availabilityStreams = [];

    internal int AvailabilityAccess(string draftId, string communityId)
    {
        lock (_gate)
        {
            var draft = DraftOf(draftId);
            if (draft is null || CommunityOf(communityId) is null) return 404;
            return CoordinatorServes(draft.Data.CoordinatorId, communityId) ? 200 : 403;
        }
    }

    internal void PublishAvailabilityTo(string draftId, string communityId, WorldStream stream)
    {
        lock (_gate) _availabilityStreams.Add((draftId, communityId, stream));
    }

    internal AvailabilitySnapshot? AvailabilityOf(string communityId, string bedId)
    {
        lock (_gate) return SnapshotOf(communityId, bedId);
    }

    internal (int Status, List<ServiceDefinition> Catalogue) ServiceCatalogueOf(string draftId)
    {
        lock (_gate)
        {
            var draft = DraftOf(draftId);
            if (draft is null) return (404, []);
            if (!CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return (403, []);
            return (200, ServiceChoices());
        }
    }

    internal List<ServiceDefinition> ServiceChoices()
    {
        lock (_gate)
        {
            return [.. _world.Services.Where(service => service.Available).Select(service => new ServiceDefinition
            {
                Code = service.Code,
                Name = service.Name,
                MonthlyPrice = service.MonthlyPrice,
                Revision = service.Revision,
                Available = service.Available,
                Capacity = service.Capacity,
            })];
        }
    }

    private AvailabilitySnapshot? SnapshotOf(string communityId, string bedId)
    {
        if (CommunityOf(communityId) is not { } community) return null;
        var home = new CommunitySummary(community.Id, community.Name);
        foreach (var building in community.Buildings)
        {
            foreach (var apartment in building.Apartments)
            {
                if (apartment.Beds.FirstOrDefault(bed => bed.Id == bedId) is not { } bed) continue;
                var room = new ApartmentSummary(apartment.Id, apartment.Number, new BuildingSummary(building.Id, building.Name, home));
                var summary = new BedSummary(bed.Id, room, bed.Revision, bed.State == BedState.Available);
                return new AvailabilitySnapshot(new AvailabilityReply(home, bed.Revision, [summary]), bed.Holder?.Name ?? string.Empty, bed.State);
            }
        }

        return null;
    }

    partial void TakeForTheBedCard(MoveInChange change)
    {
        if (change.HarborAccess is { } access)
        {
            var communities = _world.Coordinators.Single(person => person.Id == "CO1").CommunityIds;
            if (!access) communities.Remove("C1");
            else if (!communities.Contains("C1")) communities.Add("C1");
        }

        if (change.AvailabilityConnectionRestored is true) _dropped.Remove(CareEndpoints.MoveInAvailability);
        if (change.BedTaken is not true && change.FreeWindowBed is not true) return;
        var bed = _world.Communities.Single(community => community.Id == "C1").Buildings.SelectMany(building => building.Apartments)
            .SelectMany(apartment => apartment.Beds).Single(bed => bed.Id == "BED4");
        if (change.FreeWindowBed is true)
        {
            if (bed.State == BedState.Available) return;
            bed.State = BedState.Available;
            bed.Revision++;
            bed.Holder = null;
            bed.Hold = null;
        }
        else
        {
            if (bed.State == BedState.Held && bed.Holder?.ResidentId == "R104") return;
            MoveInSeed.TakeTheBed(_world);
        }

        var changed = new BedChanged("C1", SnapshotOf("C1", bed.Id)!.Reply.Beds.Single(), _world.Services.Max(service => service.Revision));
        var data = JsonSerializer.Serialize(changed, JsonSerializerOptions.Web);
        foreach (var feed in _availabilityStreams)
            if (feed.Community == changed.CommunityId && AvailabilityAccess(feed.Draft, feed.Community) == 200) feed.Stream.Push("bedChanged", data);
    }
}

internal sealed record AvailabilitySnapshot(AvailabilityReply Reply, string Holder, BedState State);
