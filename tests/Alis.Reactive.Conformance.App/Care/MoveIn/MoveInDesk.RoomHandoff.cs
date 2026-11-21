using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private bool _roomDirectoryAvailable = true;

    private void TakeForTheRoomHandoff(MoveInChange change)
    {
        if (change.RoomHandoffLocked is { } locked) _world.Rules.RoomReviewLocked = locked;
        if (change.RoomDirectoryAvailable is { } available) _roomDirectoryAvailable = available;
        if (change.RoomSearchConnectionRestored) _dropped.Remove(CareEndpoints.MoveInRoomSearch);
    }

    internal PayerOwnerPage PayerOwnerOf(string draftId)
    {
        lock (_gate) return new PayerOwnerPage(draftId, DraftOf(draftId)?.Data.Payer.Primary.PolicyNumber ?? string.Empty);
    }

    internal string PayerContactOf(string draftId)
    {
        lock (_gate) return DraftOf(draftId)?.Data.Payer.ContactId ?? string.Empty;
    }

    internal OwnerReviewVerdict ReviewRoom(string draftId, string coordinator, Guid operation, RoomReviewRequest sent)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return new(404, null, MoveInErrors.Missing());
            if (!Admits(coordinator) || sent.CoordinatorId != coordinator) return new(403, null, MoveInErrors.Forbidden());
            if (draft.Status != DraftStatus.Open) return new(410, null, MoveInErrors.Closed());
            if (_world.Rules.RoomReviewLocked) return new(423, null, MoveInErrors.Refused("locked", "Room handoff is locked."));
            var community = CommunityOf(draft.Data.Location.CommunityId);
            if (sent.DraftId != draftId || sent.Reference != "ROOM" + sent.Room || community is null || !community.Buildings.Any(building => building.Apartments.Any(apartment => apartment.Number == sent.Room)))
                return new(400, null, MoveInErrors.Refused("unknown-room", "Choose a room in this community."));

            return RecordOwnerReview(draft, coordinator, operation, ReviewKind.Room, sent.Reference, $"Room {sent.Room}, accepted by {LabelOfTheCoordinator(coordinator).Name}.");
        }
    }

    internal (int Status, IReadOnlyList<string> Names) RoomNames(RoomSearchQuery query)
    {
        lock (_gate)
        {
            if (query.DraftId.Length == 0) return (200, []);
            if (DraftOf(query.DraftId) is not { } draft || !_world.Coordinators.Any(person => person.Id == draft.Data.CoordinatorId && person.CommunityIds.Contains(query.CommunityId)))
                return (403, []);
            if (!_roomDirectoryAvailable) return (503, []);
            var buildings = CommunityOf(query.CommunityId)?.Buildings ?? [];
            return (200, [.. buildings.Where(building => building.Name.Contains(query.Query, StringComparison.OrdinalIgnoreCase))
                .SelectMany(building => building.Apartments.Select(apartment => apartment.Number + " — " + building.Name))]);
        }
    }
}
