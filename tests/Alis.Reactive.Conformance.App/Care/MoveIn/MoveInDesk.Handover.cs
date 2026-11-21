namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private DateTimeOffset? _sessionExpiresAt;
    private int _flushedHistory;

    internal bool RenewSession()
    {
        lock (_gate)
        {
            if (!CoordinatorServes("CO1", "C1") || _sessionExpiresAt <= _world.Clock.Now) return false;
            _sessionExpiresAt = _world.Clock.Now.AddMinutes(30);
            return true;
        }
    }

    internal void FlushHistory()
    {
        lock (_gate) _flushedHistory = _world.History.Count;
    }

    internal string HandoverHistory(string draftId)
    {
        lock (_gate)
        {
            var entries = _world.History.Take(_flushedHistory).Where(entry => entry.DraftId == draftId)
                .Select(entry => $"{entry.ResultReference} · {entry.Action}").ToList();
            return entries.Count == 0 ? "No admission work handed over." : string.Join(", ", entries);
        }
    }

    internal AdmissionHandover? Handover(string draftId)
    {
        lock (_gate)
        {
            if (PageOf(draftId) is not { } page || !CoordinatorServes(page.CoordinatorId, page.Location.CommunityId)) return null;
            var location = page.Location;
            var quote = OfferOf(draftId)?.Quote;
            var bed = AvailabilityOf(location.CommunityId, location.BedId)?.Reply;
            var quoteSummary = quote is null ? "No quote on file." : FormattableString.Invariant($"{quote.Id} revision {quote.Revision}: {quote.Pricing.MonthlyTotal:0.00} USD");
            var bedSummary = bed?.Beds.FirstOrDefault(b => b.Id == location.BedId) is { } current
                ? $"{current.Id} revision {current.Revision}, {(current.Available ? "available" : "unavailable")}" : "No bed on file.";
            return new AdmissionHandover(draftId, page.Version, location.CommunityId, location.BedId, page.Care.Level, page.Payer.Kind, quoteSummary, bedSummary, HandoverHistory(draftId));
        }
    }
}

internal sealed partial class MoveInReads
{
    internal string HandoverHistory(string draftId) => Desk.HandoverHistory(draftId);
}

public sealed record AdmissionHandover(string DraftId, int Version, string CommunityId, string BedId, CareLevel Level, PayerKind Payer, string Quote, string Bed, string History);
