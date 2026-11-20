using System.Globalization;
using System.Text.Json;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private readonly Dictionary<(string Draft, string Reference), SaveReceipt> _draftReceipts = [];
    private readonly Dictionary<(string Draft, int Version), DraftComparison> _draftComparisons = [];

    private VersionConflict StaleAt(DraftState draft)
    {
        var savedBy = LabelOfTheCoordinator(draft.SavedBy);
        _draftComparisons.TryAdd((draft.Id, draft.Version), new(draft.Version, savedBy.Name, draft.Data.Resident.Notes));
        var comparison = new InstructionReference(true, "CMP" + draft.Version.ToString(CultureInfo.InvariantCulture));
        return new VersionConflict(new VersionConflictDetail(new ConflictingDraft(draft.Id, draft.Version, savedBy, comparison)));
    }

    internal DraftComparison? ComparisonOf(string draftId, int revision)
    {
        lock (_gate) return _draftComparisons.GetValueOrDefault((draftId, revision));
    }

    internal MoveInPage? PageOf(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return null;

            var page = JsonSerializer.Deserialize<MoveInPage>(JsonSerializer.Serialize(draft.Data)) ?? throw new InvalidOperationException($"Draft {draft.Id} holds no page.");
            page.DraftId = draft.Id;
            page.Version = draft.Version;
            page.Status = draft.Status;
            page.Today = _world.Clock.Today;
            page.Diet.Referral = draft.ReferralDiet;
            page.Diet.UnavailableMeals = CommunityOf(page.Location.CommunityId)?.UnavailableMeals ?? DietaryNeeds.None;
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
            if (draft.Status != DraftStatus.Open) return new SaveVerdict(null, null, Closed: true);
            if (version != draft.Version) return new SaveVerdict(null, StaleAt(draft));

            if (sent.IncludesMedications)
            {
                var orders = sent.Medications.Rows;
                if (!draft.Data.Medications.Rows.Select(row => row.Id).SequenceEqual(orders.Select(row => row.Id), StringComparer.Ordinal))
                {
                    return new SaveVerdict(null, null, "Reopen the current medication orders.");
                }
                foreach (var row in orders)
                {
                    if (DrugRefusal(row) is { } refusal) return new SaveVerdict(null, null, refusal);
                }
                if (!SameOrders(draft.Data.Medications, sent.Medications)) draft.PharmacyClearanceId = null;
            }
            KeepTheDraft(draft, sent);
            draft.Version++;
            draft.SavedBy = coordinatorId;
            draft.SavedAt = _world.Clock.Now;
            var reference = "SAVE" + draft.Version.ToString(CultureInfo.InvariantCulture);
            Record(draft, operation, "save", reference);
            var receipt = new SaveReceipt(draft.Id, draft.Version, reference, LabelOfTheCoordinator(draft.SavedBy), draft.SavedAt);
            _draftReceipts.Add((draftId, reference), receipt);
            return new SaveVerdict(new SaveReply(receipt, new ReceiptReview(false, string.Empty)), null);
        }
    }

    private static void KeepTheDraft(DraftState draft, MoveInPage sent)
    {
        KeepTheLocation(draft.Data, sent);
        KeepTheResident(draft.Data, sent);
        KeepTheCare(draft.Data, sent);
        KeepTheContacts(draft.Data, sent);
        KeepThePayer(draft.Data, sent);
        draft.PayerVerificationId = draft.Data.Payer.VerificationId;
        KeepTheMedications(draft.Data, sent);
        if (sent.Services.Visits.Count > 0)
        {
            draft.Data.Services.Codes = sent.Services.Codes;
            draft.Data.Services.Visits = sent.Services.Visits;
        }
        KeepTheDiet(draft.Data, sent);
        KeepTheDocuments(draft.Data, sent);
        if (sent.Review.Reference is not null) draft.Data.Review.Reference = sent.Review.Reference;
    }

    private void TakeForTheDraft(MoveInChange change)
    {
        if ((!change.LuisSavesTransport && !change.LuisUpdatesContact) || PageOf(HelensDraft) is not { } page) return;
        if (change.LuisUpdatesContact)
        {
            page.Contacts.Rows[0].Phone = "5185550157";
            SaveContacts(page.DraftId, page.Version, "CO2", page.OperationId, new ContactEdit { Contacts = page.Contacts, Version = page.Version });
        }
        if (change.LuisSavesTransport)
        {
            page.Resident.Notes = "Luis arranged transport";
            Save(page.DraftId, page.Version, "CO2", page.OperationId, page);
        }
    }

    static partial void KeepTheLocation(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheResident(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheCare(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheContacts(MoveInPage saved, MoveInPage sent);

    static partial void KeepThePayer(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheMedications(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheDiet(MoveInPage saved, MoveInPage sent);

    static partial void KeepTheDocuments(MoveInPage saved, MoveInPage sent);

}

internal sealed record SaveVerdict(SaveReply? Saved, VersionConflict? Stale, string? MedicationRefusal = null, bool Closed = false);
