using System.Globalization;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private const string TheAllocationReview = "allocation-review";

    internal bool MayAdmit(string coordinatorId)
    {
        lock (_gate) return Admits(coordinatorId);
    }

    internal bool? IsReadyForReview(string draftId)
    {
        lock (_gate) return DraftOf(draftId) is { } draft ? draft.Status == DraftStatus.Open && Admits(draft.Data.CoordinatorId) : null;
    }

    internal string SiteOf(string communityId)
    {
        lock (_gate) return CommunityOf(communityId)?.Site ?? string.Empty;
    }

    internal string BedOnFileOf(string bedId)
    {
        lock (_gate)
        {
            var bed = _world.Communities.SelectMany(community => community.Buildings).SelectMany(building => building.Apartments).SelectMany(apartment => apartment.Beds).FirstOrDefault(found => found.Id == bedId);
            if (bed is null) return "none";

            var held = bed.State switch
            {
                BedState.Available => "available",
                BedState.Held => "held by " + bed.Holder?.Name,
                BedState.Occupied => "occupied by " + bed.Holder?.Name,
                _ => "closed",
            };
            return $"{bed.Id} revision {bed.Revision.ToString(CultureInfo.InvariantCulture)}, {held}";
        }
    }

    internal IReadOnlyList<string> AllocationReviewsOf(string draftId)
    {
        lock (_gate)
            return [.. _world.Reviews.Where(review => review.DraftId == draftId && review.Kind == ReviewKind.Allocation).Select(review => $"{review.Id}: {review.Message}, {(review.Passed ? "Cleared" : "Refused")}, {review.Reference}")];
    }

    internal AllocationVerdict ReviewTheAllocation(string draftId, string coordinatorId, Guid operation, AllocationRequest asked)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return new AllocationVerdict(null, null, null);

            var community = CommunityOf(asked.Location.CommunityId);
            var building = community?.Buildings.FirstOrDefault(found => found.Id == asked.Location.BuildingId);
            var apartment = building?.Apartments.FirstOrDefault(found => found.Id == asked.Location.ApartmentId);
            var bed = apartment?.Beds.FirstOrDefault(found => found.Id == asked.Location.BedId);
            if (community is null || building is null || apartment is null || bed is null)
                return new AllocationVerdict(null, null, MoveInErrors.Refused("bed-misplaced", "The bed is not in the apartment chosen."));
            if (!apartment.CareLevels.Contains(asked.Level))
                return new AllocationVerdict(null, null, MoveInErrors.Refused("care-not-taken", $"Apartment {apartment.Number} does not take {asked.Level} care."));
            if (!asked.Ready)
                return new AllocationVerdict(null, null, MoveInErrors.Refused("not-ready", "The allocation is not ready."));
            if (bed.State != BedState.Available || bed.Revision != asked.Location.BedRevision)
            {
                var holder = new PersonLabel(bed.Holder?.ResidentId ?? string.Empty, bed.Holder?.Name ?? string.Empty);
                var conflict = new ConflictingBed(bed.Id, holder, new ConflictHold(bed.Hold?.ExpiresAt ?? _world.Clock.Now));
                return new AllocationVerdict(null, new BedConflict(new BedConflictDetail(conflict, new WaitlistOffer(bed.State == BedState.Held, "WT-" + community.Id))), null);
            }

            var number = (100 + _world.Reviews.Count(review => review.Kind == ReviewKind.Allocation)).ToString(CultureInfo.InvariantCulture);
            var reviewId = TheAllocationsReviews + number;
            _world.Reviews.Add(new ReviewState
            {
                Id = reviewId,
                DraftId = draft.Id,
                Kind = ReviewKind.Allocation,
                DraftVersion = asked.Version,
                Passed = true,
                Reference = asked.Reference ?? "Unassigned",
                Message = $"{bed.Id} revision {bed.Revision.ToString(CultureInfo.InvariantCulture)}",
            });
            _world.History.Add(new HistoryEntry
            {
                Sequence = _world.History.Count + 1,
                DraftId = draft.Id,
                OperationId = operation,
                Action = TheAllocationReview,
                Actor = coordinatorId,
                At = _world.Clock.Now,
                ResultReference = reviewId,
            });
            var shared = apartment.Beds.Any(other => other.Id != bed.Id && other.State != BedState.Available);
            var place = new ApartmentSummary(apartment.Id, apartment.Number, new BuildingSummary(building.Id, building.Name, new CommunitySummary(community.Id, community.Name)));
            return new AllocationVerdict(new AllocationReply(new AllocationDecision("Cleared", new BedSummary(bed.Id, place, bed.Revision, true), new InstructionReference(shared, TheAllocationsReceipts + number))), null, null);
        }
    }

    partial void TakeForTheAllocationReview(MoveInChange change)
    {
        if (change.CoordinatorWithoutAdmission is { } coordinator) _world.Coordinators.Single(found => found.Id == coordinator).CanAdmit = false;
    }

    private bool Admits(string coordinatorId) => _world.Coordinators.Any(coordinator => coordinator.Id == coordinatorId && coordinator.CanAdmit);
}

internal sealed record AllocationVerdict(AllocationReply? Cleared, BedConflict? Taken, ErrorEnvelope? Refused);
