using System.Globalization;
using System.Text.Json;
using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    private bool _refuseServiceRecord;

    partial void TakeForTheServiceReview(MoveInChange change)
    {
        if (change.PharmacyCapacityExhausted) _world.Services.Single(service => service.Code == "PHA").Capacity = 0;
        if (change.RefuseServiceRecord) _refuseServiceRecord = true;
        if (change.AcceptanceConnectionRestored) _dropped.Remove(CareEndpoints.MoveInAcceptance);
        if (change.PharmacyPriceChanged)
        {
            MoveInSeed.ChangeThePrice(_world);
            PublishBedAndPrice("BED4");
        }
    }

    private List<(ServiceDefinition Service, int Visits)> ScheduledServices(ServiceInput selected)
        => [.. from service in _world.Services
               where service.Available && selected.Codes.Contains(service.Code)
               let visits = selected.Visits.Where(visit => visit.Active && visit.Code == service.Code).Sum(visit => visit.Count)
               where visits > 0
               select (service, visits)];

    internal ConformanceStart ReviewComponent(MoveInPage page)
    {
        lock (_gate)
        {
            var scheduled = ScheduledServices(page.Services).Where(line => line.Service.Capacity >= line.Visits).ToList();
            var visits = string.Join("&", scheduled.Select((line, index) => FormattableString.Invariant($"Visits[{index}].Code={Uri.EscapeDataString(line.Service.Code)}&Visits[{index}].Count={line.Visits}")));
            var day = _world.Clock.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return new ConformanceStart
            {
                Text = "Service review",
                Value = scheduled.FirstOrDefault().Service?.Code,
                Options = [.. scheduled.Select(line => line.Service.Code)],
                Count = scheduled.Count,
                Enabled = Admits(page.CoordinatorId),
                Outer = new OuterStart { InnerText = SiteOf(page.Location.CommunityId) + " allocation desk" },
                CurrentItem = scheduled.FirstOrDefault().Service?.Name,
                Items = [.. scheduled.Select(line => new ItemStart { Name = line.Service.Name, Value = line.Visits, Active = true })],
                Records = [.. scheduled.Select(line => new RecordStart { Name = line.Service.Name, Code = RecordCode.Text(line.Service.Code), Count = line.Visits, Day = day })],
                CheckEndpoint = MoveInEndpoints.ReviewCheckOf(page.DraftId),
                CountEndpoint = CarePages.MoveIns + page.DraftId + "/review/visits?" + visits,
                RecordEndpoint = CarePages.MoveIns + page.DraftId + "/review/services?" + visits,
            };
        }
    }

    internal (int Status, int Visits, Record? Record) CurrentServiceRecord(string draftId, ServiceInput requested)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, 0, null);
            if (!CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return (403, 0, null);
            var services = (from service in _world.Services
                            join visit in requested.Visits on service.Code equals visit.Code
                            select (Service: service, Visits: visit.Count)).ToList();
            return (200, services.Sum(line => line.Visits), _refuseServiceRecord ? null : new Record
            {
                Name = string.Join(", ", services.Select(line => line.Service.Name)),
                Code = string.Join(", ", services.Select(line => line.Service.Code)),
                Count = services.Count,
                Day = _world.Clock.Today,
            });
        }
    }

    internal ServiceReviewRead? ReviewRead(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return null;
            var r = draft.Data.Review;
            var reviewed = r.ReviewedAt is not null && r.Result is { } result
                ? $"{r.SelectedServiceCount} {(r.SelectedServiceCount == 1 ? "service" : "services")}, {r.VisitCount} visits. {result.Code}: {result.Name}."
                : "No service review on file.";
            var count = _world.History.Count(entry => entry.DraftId == draftId && entry.Action == "quote");
            var calculations = count == 1 ? "1 calculation on file." : $"{count} calculations on file.";
            return new ServiceReviewRead(draftId, _world.Clock.Now, reviewed, reviewed + " " + calculations, QuoteOnFile(draftId), AcceptedQuote(draftId));
        }
    }

    internal QuoteOfferPage? OfferOf(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return null;
            return new QuoteOfferPage(draftId, draft.Quote is { } q ? Snapshot(q) : null, draft.AcceptedQuoteRevision, _world.Services.Max(service => service.Revision));
        }
    }

    private static QuoteState Snapshot(QuoteState quote)
        => JsonSerializer.Deserialize<QuoteState>(JsonSerializer.Serialize(quote)) ?? throw new InvalidOperationException("The saved quote has no snapshot.");

    internal (int Status, SaveReply? Receipt, QuoteReply? Conflict) AcceptQuote(string draftId, QuoteAcceptance request, string coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, null, null);
            if (!CoordinatorServes(coordinator, draft.Data.Location.CommunityId) || !Admits(coordinator)) return (403, null, null);
            if (draft.Status != DraftStatus.Open) return (410, null, null);
            if (draft.Quote is not { } quote) return (404, null, null);
            var terms = quote.Pricing.Assistance;
            var current = AssistanceTerms.For(_world, draft, draft.Data.Payer.Kind, quote.Pricing.MonthlyTotal);
            if (!new CurrentQuoteAcceptanceRules(quote).Validate(request).IsValid || request.Version != draft.Version
                || quote.ExpiresAt <= _world.Clock.Now || quote.CareLevel != draft.Data.Care.Level || quote.PayerKind != draft.Data.Payer.Kind
                || terms.Required != current.Required || terms.Reference != current.Reference || terms.Contribution != current.Contribution
                || !_world.CareLevels.Any(rate => rate.Level == quote.CareLevel && rate.MonthlyBasePrice == quote.Pricing.Base)
                || quote.Pricing.Lines.Any(line => !_world.Services.Any(s => s.Code == line.ServiceCode && s.Available && s.MonthlyPrice == line.Amount && s.Revision == line.Revision)))
            {
                return (409, null, new QuoteReply(Snapshot(quote)));
            }
            if (terms.Required && !request.AcceptAssistance) return (400, null, null);
            draft.Data.AcceptedQuoteRevision = draft.AcceptedQuoteRevision = quote.Revision;
            terms.Accepted = terms.Required && request.AcceptAssistance;
            var reference = $"ACCEPT-Q{quote.Revision}";
            Record(draft, operation, "accept-quote", reference, coordinator);
            return (200, new SaveReply(new SaveReceipt(draftId, draft.Version, reference, LabelOfTheCoordinator(coordinator), _world.Clock.Now), new ReceiptReview(terms.Required, terms.Reference)), null);
        }
    }
}

internal sealed class CurrentQuoteAcceptanceRules : ReactiveValidator<QuoteAcceptance>
{
    internal CurrentQuoteAcceptanceRules(QuoteState quote)
        => RuleFor(acceptance => acceptance.Revision)
            .Must((acceptance, revision) => acceptance.QuoteId == quote.Id && revision == quote.Revision)
            .WithMessage("The displayed quote has changed.");
}

public sealed record ServiceReviewRead(string DraftId, DateTimeOffset ReviewedAt, string Summary, string SavedReview, string Quote, string Acceptance);

public sealed class QuoteOfferQuery
{
    public string DraftId { get; set; } = string.Empty;
    public int PreviousRevision { get; set; }
}

public sealed record QuoteOfferPage(string DraftId, QuoteState? Quote, int? AcceptedRevision, int CatalogueRevision)
{
    public int? PreviousRevision { get; init; }
    public bool AcceptAssistance { get; set; }
    public int Revision => Quote?.Revision ?? 0;
    public decimal Contribution => Quote?.Pricing.Assistance.Contribution ?? 0m;
}
