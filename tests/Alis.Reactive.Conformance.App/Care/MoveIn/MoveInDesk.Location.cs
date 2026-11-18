using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    internal IReadOnlyList<Community> Communities()
    {
        lock (_gate) return [.. _world.Communities];
    }

    internal bool CoordinatorServes(string coordinatorId, string communityId)
    {
        lock (_gate) return _world.Coordinators.Any(coordinator => coordinator.Id == coordinatorId && coordinator.CommunityIds.Contains(communityId));
    }

    internal IReadOnlyList<BuildingOffer> BuildingsOf(string communityId)
    {
        lock (_gate)
            return [.. (CommunityOf(communityId)?.Buildings ?? []).Select(building => new BuildingOffer(building.Id, building.Name, _world.Rules.ClosedBuildingIds.Contains(building.Id)))];
    }

    internal BuildingOffer? BuildingOf(string communityId, string buildingId)
    {
        lock (_gate)
            return BuildingAt(communityId, buildingId) is { } building ? new BuildingOffer(building.Id, building.Name, _world.Rules.ClosedBuildingIds.Contains(building.Id)) : null;
    }

    internal IReadOnlyList<ApartmentOffer> ApartmentsOf(string communityId, string buildingId, CareLevel level)
    {
        lock (_gate)
            return [.. (BuildingAt(communityId, buildingId)?.Apartments ?? []).Where(apartment => apartment.CareLevels.Contains(level)).Select(apartment => new ApartmentOffer(apartment.Id, apartment.Number))];
    }

    internal IReadOnlyList<BedOffer> BedsOf(string communityId, string buildingId, string apartmentId)
    {
        lock (_gate)
        {
            return [.. (BuildingAt(communityId, buildingId)?.Apartments.FirstOrDefault(found => found.Id == apartmentId)?.Beds ?? [])
                .Select(bed => new BedOffer(bed.Id, bed.Label, bed.Revision, bed.State == BedState.Available, bed.Holder?.Name ?? string.Empty))];
        }
    }

    partial void TakeForTheLocation(MoveInChange change)
    {
        if (change.ClosedBuilding is { } closed) _world.Rules.ClosedBuildingIds.Add(closed);
    }

    static partial void KeepTheLocation(MoveInPage saved, MoveInPage sent)
        => saved.Location = new LocationInput
        {
            CommunityId = sent.Location.CommunityId,
            BuildingId = sent.Location.BuildingId,
            ApartmentId = sent.Location.ApartmentId,
            BedId = sent.Location.BedId,
            BedRevision = sent.Location.BedRevision,
        };

    private Community? CommunityOf(string communityId) => _world.Communities.FirstOrDefault(found => found.Id == communityId);

    private Building? BuildingAt(string communityId, string buildingId) => CommunityOf(communityId)?.Buildings.FirstOrDefault(found => found.Id == buildingId);
}

internal sealed record BuildingOffer(string Id, string Name, bool Closed);

internal sealed record ApartmentOffer(string Id, string Number);

internal sealed record BedOffer(string Id, string Label, int Revision, bool Available, string Holder);
