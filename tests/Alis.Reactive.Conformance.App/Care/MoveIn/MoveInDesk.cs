using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk : IOwnedByAWorld
{
    private readonly Lock _gate = new();
    private readonly MoveInWorld _world = MoveInSeed.World();
    private readonly HashSet<string> _dropped = new(StringComparer.Ordinal);

    internal static MoveInDesk Opened() => new();

    public void Take(Seed seed)
    {
        if (seed.MoveIn is not { } change) return;

        lock (_gate)
        {
            if (change.DroppedEndpoint is { } endpoint) _dropped.Add(endpoint);
            TakeForTheLocation(change);
            TakeForTheResident(change);
            TakeForTheCare(change);
            TakeForTheContacts(change);
            TakeForThePayer(change);
            TakeForTheMedications(change);
            TakeForTheServices(change);
            TakeForTheDiet(change);
            TakeForTheDocuments(change);
            TakeForTheAllocationReview(change);
            TakeForTheServiceReview(change);
            TakeForThePayerVerification(change);
            TakeForTheWaitlist(change);
            TakeForTheDeposit(change);
            TakeForTheReception(change);
            TakeForTheBedCard(change);
            TakeForTheCoordinatorLine(change);
            TakeForTheOwners(change);
        }
    }

    internal bool Drops(string endpoint)
    {
        lock (_gate) return _dropped.Contains(endpoint);
    }

    partial void TakeForTheLocation(MoveInChange change);

    partial void TakeForTheResident(MoveInChange change);

    partial void TakeForTheCare(MoveInChange change);

    partial void TakeForTheContacts(MoveInChange change);

    partial void TakeForThePayer(MoveInChange change);

    partial void TakeForTheMedications(MoveInChange change);

    partial void TakeForTheServices(MoveInChange change);

    partial void TakeForTheDiet(MoveInChange change);

    partial void TakeForTheDocuments(MoveInChange change);

    partial void TakeForTheAllocationReview(MoveInChange change);

    partial void TakeForTheServiceReview(MoveInChange change);

    partial void TakeForThePayerVerification(MoveInChange change);

    partial void TakeForTheWaitlist(MoveInChange change);

    partial void TakeForTheDeposit(MoveInChange change);

    partial void TakeForTheReception(MoveInChange change);

    partial void TakeForTheBedCard(MoveInChange change);

    partial void TakeForTheCoordinatorLine(MoveInChange change);

    partial void TakeForTheOwners(MoveInChange change);

    private DraftState? DraftOf(string draftId) => _world.Drafts.FirstOrDefault(found => found.Id == draftId);

    private PersonLabel LabelOfTheCoordinator(string coordinatorId)
        => new(coordinatorId, _world.Coordinators.FirstOrDefault(found => found.Id == coordinatorId)?.Name ?? string.Empty);

    private void Record(DraftState draft, Guid operation, string action, string reference, string? actor = null)
        => _world.History.Add(new HistoryEntry
        {
            Sequence = _world.History.Count + 1,
            DraftId = draft.Id,
            OperationId = operation,
            Action = action,
            Actor = actor ?? draft.SavedBy,
            At = _world.Clock.Now,
            ResultReference = reference,
        });
}
