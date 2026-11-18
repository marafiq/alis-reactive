using System.Globalization;
using System.Text.Json;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    internal MoveInPage? PageOf(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return null;

            var page = JsonSerializer.Deserialize<MoveInPage>(JsonSerializer.Serialize(draft.Data)) ?? throw new InvalidOperationException($"Draft {draft.Id} holds no page.");
            page.DraftId = draft.Id;
            page.Version = draft.Version;
            page.Today = _world.Clock.Today;
            return page;
        }
    }

    internal bool IsClosed(string draftId)
    {
        lock (_gate) return DraftOf(draftId) is { Status: not DraftStatus.Open };
    }

    internal bool Knows(string coordinatorId)
    {
        lock (_gate) return _world.Coordinators.Any(coordinator => coordinator.Id == coordinatorId);
    }

    internal SaveVerdict Save(string draftId, int version, string coordinatorId, Guid operation, MoveInPage sent)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return new SaveVerdict(null, null);
            if (version != draft.Version)
            {
                var comparison = new InstructionReference(true, "CMP" + draft.Version.ToString(CultureInfo.InvariantCulture));
                return new SaveVerdict(null, new VersionConflict(new VersionConflictDetail(new ConflictingDraft(draft.Id, draft.Version, LabelOfTheCoordinator(draft.SavedBy), comparison))));
            }

            KeepTheLocation(draft.Data, sent);
            KeepTheResident(draft.Data, sent);
            KeepTheCare(draft.Data, sent);
            KeepTheContacts(draft.Data, sent);
            KeepThePayer(draft.Data, sent);
            KeepTheMedications(draft.Data, sent);
            KeepTheServices(draft.Data, sent);
            KeepTheDiet(draft.Data, sent);
            KeepTheDocuments(draft.Data, sent);
            KeepTheReview(draft.Data, sent);
            draft.Version++;
            draft.SavedBy = coordinatorId;
            draft.SavedAt = _world.Clock.Now;
            var reference = "SAVE" + draft.Version.ToString(CultureInfo.InvariantCulture);
            Record(draft, operation, "save", reference);
            return new SaveVerdict(new SaveReply(new SaveReceipt(draft.Id, draft.Version, reference, LabelOfTheCoordinator(draft.SavedBy), draft.SavedAt), new ReceiptReview(false, string.Empty)), null);
        }
    }

    static partial void KeepTheLocation(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheResident(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheCare(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheContacts(MoveInPage saved, MoveInPage sent);

    static partial void KeepThePayer(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheMedications(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheServices(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheDiet(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheDocuments(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheReview(MoveInPage saved, MoveInPage sent);
}

internal sealed record SaveVerdict(SaveReply? Saved, VersionConflict? Stale);
