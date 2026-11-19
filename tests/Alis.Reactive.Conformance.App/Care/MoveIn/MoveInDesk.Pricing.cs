using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    partial void TakeForTheServices(MoveInChange change)
    {
        if (change.ReadyForAdmission) MoveInSeed.MakeReady(_world);
        if (change.AwaitsFirstQuote && DraftOf("D100") is { } draft) draft.Quote = null;
    }

    internal (int Status, QuoteReply? Reply, string? Reason) CalculateQuote(string draftId, QuoteRequest request, string coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, null, null);
            if (!CoordinatorServes(coordinator, draft.Data.Location.CommunityId)) return (403, null, null);
            if (draft.Status != DraftStatus.Open) return (410, null, null);
            if (!request.Review.Ready || request.Review.SelectedServiceCount < 1 || request.Review.Result is null) return (409, null, "The service desk has not cleared this quote.");
            if (request.Version != draft.Version) return (409, null, "The draft version has changed.");
            var rate = _world.CareLevels.Find(r => r.Level == request.CareLevel);
            if (rate is null) return (409, null, "The care level has no current price.");
            var lines = new List<PriceLine>();
            foreach (var code in request.Services.Codes.Distinct())
            {
                var service = _world.Services.Find(s => s.Code == code);
                if (service is null || !service.Available) return (409, null, "A selected service is no longer available.");
                var capacity = new ServiceCapacityRules(service).Validate(request.Services);
                if (!capacity.IsValid) return (409, null, capacity.Errors[0].ErrorMessage);
                lines.Add(new PriceLine { ServiceCode = code, Name = service.Name, Amount = service.MonthlyPrice, Revision = service.Revision });
            }
            if (lines.Count == 0) return (400, null, "Choose at least one service.");
            var reviewed = ScheduledServices(request.Services);
            if (request.Review.SelectedServiceCount != reviewed.Count || request.Review.VisitCount != reviewed.Sum(line => line.Visits)
                || request.Review.Result.Count != reviewed.Count || request.Review.Result.Code != string.Join(", ", reviewed.Select(line => line.Service.Code))
                || request.Review.Result.Name != string.Join(", ", reviewed.Select(line => line.Service.Name)))
            {
                return (409, null, "The reviewed services have changed. Refresh the service review.");
            }
            var total = rate.MonthlyBasePrice + lines.Sum(l => l.Amount);
            var assistance = AssistanceTerms.For(_world, draft, request.PayerKind, total);
            var quote = new QuoteState
            {
                Id = "Q" + draft.Id[1..],
                Revision = lines.Max(line => line.Revision),
                CareLevel = request.CareLevel,
                PayerKind = request.PayerKind,
                ExpiresAt = _world.Clock.Now.AddHours(1),
                Pricing = new QuotePricing { Base = rate.MonthlyBasePrice, Lines = lines, MonthlyTotal = total, Deposit = draft.Quote?.Pricing.Deposit ?? 500.00m, Assistance = assistance },
            };
            if (draft.Quote is { } previous)
            {
                if (SameTerms(previous, quote))
                {
                    quote.Revision = previous.Revision;
                    quote.Pricing.Assistance.Accepted = previous.Pricing.Assistance.Accepted;
                }
                else
                {
                    quote.Revision = Math.Max(quote.Revision, previous.Revision + 1);
                    draft.AcceptedQuoteRevision = null;
                }
            }
            draft.Data.Review = request.Review;
            draft.Data.AcceptedQuoteRevision = draft.AcceptedQuoteRevision;
            draft.Quote = quote;
            Record(draft, operation, "quote", quote.Id, coordinator);
            return (200, new QuoteReply(Snapshot(quote)), null);
        }
    }

    private static bool SameTerms(QuoteState previous, QuoteState current)
    {
        static IEnumerable<(string, decimal, int)> Charges(QuotePricing pricing)
            => pricing.Lines.OrderBy(line => line.ServiceCode, StringComparer.Ordinal).Select(line => (line.ServiceCode, line.Amount, line.Revision));

        var before = previous.Pricing;
        var after = current.Pricing;
        return previous.CareLevel == current.CareLevel && previous.PayerKind == current.PayerKind
            && previous.ExpiresAt == current.ExpiresAt && before.Base == after.Base && before.Deposit == after.Deposit && before.MonthlyTotal == after.MonthlyTotal
            && before.Assistance.Required == after.Assistance.Required && before.Assistance.Reference == after.Assistance.Reference
            && before.Assistance.Contribution == after.Assistance.Contribution && Charges(before).SequenceEqual(Charges(after));
    }

    internal (int Status, string? Terms) AssistanceOf(string draftId, string reference, int revision)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, null);
            if (!CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return (403, null);
            if (draft.Quote is not { } quote || quote.Revision != revision || quote.Pricing.Assistance.Reference != reference) return (409, null);
            return (200, FormattableString.Invariant($"{draft.Data.Payer.Primary.Provider} contribution: {quote.Pricing.Assistance.Contribution:0.00}"));
        }
    }

    internal string QuoteOnFile(string draftId)
    {
        lock (_gate)
        {
            var count = _world.History.Count(e => e.DraftId == draftId && e.Action == "quote");
            var countText = count == 1 ? "1 calculation on file." : $"{count} calculations on file.";
            return DraftOf(draftId)?.Quote is { } q
                ? FormattableString.Invariant($"{q.Id} revision {q.Revision}: {q.Pricing.MonthlyTotal:0.00} USD, assistance {q.Pricing.Assistance.Contribution:0.00} USD. {countText}")
                : "No quote on file. " + countText;
        }
    }

    internal string AcceptedQuote(string draftId)
    {
        lock (_gate) return DraftOf(draftId)?.AcceptedQuoteRevision is { } revision ? $"Accepted revision {revision}" : "Unaccepted";
    }
}

internal sealed class ServiceCapacityRules : ReactiveValidator<ServiceInput>
{
    internal ServiceCapacityRules(ServiceDefinition service)
        => RuleFor(input => input.Visits)
            .Must(visits => service.Capacity >= Math.Max(1, visits.Where(visit => visit.Active && visit.Code == service.Code).Sum(visit => visit.Count)))
            .WithMessage($"{service.Name} has no capacity.");
}

internal sealed partial class MoveInReads
{
    internal bool OffersServices(MoveInPage page) => Desk.CoordinatorServes(page.CoordinatorId, page.Location.CommunityId);
    internal List<ServiceDefinition> ServiceChoices() => Desk.ServiceChoices();
    internal ServiceReviewRead ReviewRead(string draftId) => Desk.ReviewRead(draftId) ?? throw new InvalidOperationException("The quote needs its draft.");
}

public sealed record ServiceReviewPage(string DraftId, ConformanceStart ComponentStart);
