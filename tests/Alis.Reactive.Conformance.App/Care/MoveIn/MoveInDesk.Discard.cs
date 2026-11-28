namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    internal DraftActionsPage? DraftActionsOf(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return null;
            return new(draft.Id, ResidentOf(draftId), draft.Status, draft.Version);
        }
    }

    internal DiscardResult Discard(string draftId, int version, string coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (!Knows(coordinator)) return new(403, Error: MoveInErrors.Forbidden());
            if (DraftOf(draftId) is not { } draft) return new(404, Error: MoveInErrors.Missing());
            if (draft.Status == DraftStatus.Submitted) return new(422, Error: MoveInErrors.Refused("submitted", "A submitted admission cannot be discarded."));
            if (draft.Status == DraftStatus.Discarded) return new(410, Error: MoveInErrors.Closed());
            if (version != draft.Version) return new(409, StaleAt(draft));

            draft.Status = DraftStatus.Discarded;
            draft.Version++;
            draft.SavedAt = _world.Clock.Now;
            draft.SavedBy = coordinator;
            Record(draft, operation, "discard", draft.Id);
            return new(204);
        }
    }
}

internal sealed partial class MoveInReads
{
    internal DraftActionsPage? DraftActionsOf(string draftId) => Desk.DraftActionsOf(draftId);
}

internal sealed record DiscardResult(int Status, VersionConflict? Stale = null, ErrorEnvelope? Error = null);
