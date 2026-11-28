using System.Globalization;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    partial void TakeForTheOwners(MoveInChange change)
    {
        if (change.CurrentAssessment is { } reference && DraftOf("D100")?.Data.Care.Assisted is { } assisted)
            assisted.AssessmentCode = reference;
        TakeForTheRoomHandoff(change);
    }

    internal OwnerDeskPage OwnerDeskOf(string draftId)
    {
        lock (_gate)
        {
            var draft = DraftOf(draftId);
            var community = draft is null ? null : CommunityOf(draft.Data.Location.CommunityId);
            var capacity = community?.Buildings.SelectMany(building => building.Apartments).SelectMany(apartment => apartment.Beds).Count(bed => bed.State == BedState.Available) ?? 0;
            return new OwnerDeskPage(draftId, capacity, OwnerHistoryOf(draftId));
        }
    }

    internal OwnerHistoryPage OwnerHistoryOf(string draftId)
    {
        lock (_gate)
        {
            var reviews = _world.Reviews.Where(review => review.DraftId == draftId).ToList();
            return new OwnerHistoryPage(
                [.. reviews.Where(review => review.Kind == ReviewKind.Mobility).Select(review => review.Reference + ": " + review.Message)],
                [.. reviews.Where(review => review.Kind == ReviewKind.Room).Select(review => review.Reference + ": " + review.Message)],
                AssessmentOwnerOf(draftId).Reference, _world.Rules.RoomReviewLocked);
        }
    }

    internal AllocationOwnerPage AssessmentOwnerOf(string draftId)
    {
        lock (_gate) return new AllocationOwnerPage(draftId, DraftOf(draftId)?.Data.Care.Assisted?.AssessmentCode ?? string.Empty);
    }

    internal OwnerReviewVerdict ReviewMobility(string draftId, string reference, string coordinator, Guid operation, MobilityRequest sent)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return new(404, null, MoveInErrors.Missing());
            if (!Admits(coordinator)) return new(403, null, MoveInErrors.Forbidden());
            if (draft.Status != DraftStatus.Open) return new(410, null, MoveInErrors.Closed());
            if (draft.Data.Care.Assisted?.AssessmentCode != reference || reference.Length == 0)
                return new(422, null, MoveInErrors.Refused("superseded", "Assessment is superseded."));

            var ratio = sent.Ratio.ToString(CultureInfo.InvariantCulture);
            var message = sent.Description + ", ratio " + ratio + ".";
            if (sent.PreviousDescription is { } previous && previous != sent.Description)
                message += " Previously " + previous + ".";
            return RecordOwnerReview(draft, coordinator, operation, ReviewKind.Mobility, "MOB" + ratio, message);
        }
    }

    private OwnerReviewVerdict RecordOwnerReview(DraftState draft, string coordinator, Guid operation, ReviewKind kind, string reference, string message)
    {
        var count = _world.Reviews.Count(review => review.Kind == kind) + 1;
        _world.Reviews.Add(new ReviewState
        {
            Id = kind.ToString() + count.ToString(CultureInfo.InvariantCulture),
            DraftId = draft.Id,
            Kind = kind,
            DraftVersion = draft.Version,
            Passed = true,
            Reference = reference,
            Message = message,
        });
        Record(draft, operation, kind.ToString(), reference);
        return new(201, new SaveReply(new SaveReceipt(draft.Id, draft.Version, reference, LabelOfTheCoordinator(coordinator), _world.Clock.Now), new ReceiptReview(false, string.Empty)), null);
    }
}

internal sealed record OwnerReviewVerdict(int Status, SaveReply? Receipt, ErrorEnvelope? Error);
