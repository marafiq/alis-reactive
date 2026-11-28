using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private const string VisitorSignIn = "visitor-sign-in";
    private readonly HashSet<string> _barredVisitors = new(StringComparer.OrdinalIgnoreCase);

    partial void TakeForTheReception(MoveInChange change)
    {
        if (change.BarredVisitor is { } visitor) _barredVisitors.Add(visitor);
    }

    internal ReceptionPage? ReceptionOf(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { Status: DraftStatus.Open } draft) return null;
            var page = draft.Data;
            if (!CoordinatorServes(page.CoordinatorId, page.Location.CommunityId)) return null;
            var community = _world.Communities.Single(c => c.Id == page.Location.CommunityId);
            var apartment = BuildingAt(community.Id, page.Location.BuildingId)?.Apartments.Find(a => a.Id == page.Location.ApartmentId);
            return new ReceptionPage
            {
                DraftId = draft.Id,
                ResidentId = page.ResidentId,
                CoordinatorId = page.CoordinatorId,
                OperationId = page.OperationId,
                Desk = community.Site + " desk",
                Copies = 1,
                Apartment = apartment?.Number ?? string.Empty,
            };
        }
    }

    internal (int Status, VisitorReceipt? Receipt, ErrorEnvelope? Error) SignIn(string draftId, ReceptionPage sent, bool navigation, string? coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, null, MoveInErrors.Missing());
            var page = draft.Data;
            if (coordinator != page.CoordinatorId || !CoordinatorServes(page.CoordinatorId, page.Location.CommunityId)) return (403, null, MoveInErrors.Forbidden());
            if (draft.Status != DraftStatus.Open) return (422, null, MoveInErrors.Closed());
            if (operation == Guid.Empty || operation != page.OperationId || sent.OperationId != operation || sent.CoordinatorId != coordinator ||
                sent.DraftId != draftId || sent.ResidentId != page.ResidentId || !Enum.IsDefined(sent.Relationship) || string.IsNullOrWhiteSpace(sent.Desk))
            {
                return (400, null, MoveInErrors.Refused("invalid-sign-in", "The visitor details do not match this admission."));
            }
            var name = sent.VisitorName.Trim();
            if (_barredVisitors.Contains(name)) return (422, null, MoveInErrors.Refused("barred-visitor", "This visitor is barred from signing in."));
            var resident = _world.Residents.Single(r => r.Id == page.ResidentId);
            var signIn = $"{name} signed in for {resident.Identity.Name} at {sent.Desk}";
            var consent = sent.Consent ? "Consent recorded" : "Consent pending";
            var badges = sent.Copies == 1 ? "1 badge" : $"{sent.Copies} badges";
            var entry = new AcknowledgementState
            {
                Id = $"REC{100 + _world.Acknowledgements.Count(a => a.Kind == VisitorSignIn)}",
                DraftId = draft.Id,
                Kind = VisitorSignIn,
                Reference = resident.Id,
                CoordinatorId = coordinator,
                Text = $"{signIn} · {sent.Relationship} · {consent} · {badges}",
            };
            _world.Acknowledgements.Add(entry);
            Record(draft, operation, VisitorSignIn, entry.Id, coordinator);
            return (200, new VisitorReceipt(draft.Id, signIn, navigation), null);
        }
    }

    internal (int Status, AdmissionHistoryPage? Page) AdmissionHistoryOf(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, null);
            if (!CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return (403, null);
            var entries = _world.History.Where(h => h.DraftId == draft.Id).OrderBy(h => h.Sequence).Select(h =>
                h.Action == VisitorSignIn
                    ? $"{h.ResultReference} · {_world.Acknowledgements.Single(a => a.DraftId == draft.Id && a.Id == h.ResultReference).Text}"
                    : $"{h.ResultReference} · {h.Action}").ToList();
            return (200, new AdmissionHistoryPage(draft.Id, entries));
        }
    }
}

internal sealed partial class MoveInReads
{
    internal ReceptionPage? ReceptionOf(string draftId) => Desk.ReceptionOf(draftId);
}

public sealed record VisitorReceipt(string DraftId, string SignIn, bool Navigation);

public sealed record AdmissionHistoryPage(string DraftId, IReadOnlyList<string> Entries);
