using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    partial void TakeForTheDeposit(MoveInChange change)
    {
        if (change.DepositConnectionRestored) _dropped.Remove(CareEndpoints.MoveInDeposits);
        if (change.RequiredDeposit is not { } amount || DraftOf(HelensDraft) is not { Quote: { } quote } draft) return;
        quote.Pricing.Deposit = amount;
        quote.Revision++;
        draft.AcceptedQuoteRevision = draft.Data.AcceptedQuoteRevision = null;
        quote.Pricing.Assistance.Accepted = false;
    }

    internal DepositPage? DepositForm(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { Status: DraftStatus.Open, Quote: { } quote } draft || !CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return null;
            return new DepositPage { DraftId = draft.Id, OperationId = draft.Data.OperationId, Amount = quote.Pricing.Deposit, RequiredAmount = quote.Pricing.Deposit, Currency = "USD" };
        }
    }

    internal string DepositOnFile(string draftId)
    {
        lock (_gate)
        {
            if (DraftOf(draftId)?.Quote is not { } quote) return "No quote.";
            var payments = _world.Payments.Where(p => p.DraftId == draftId && p.Status == PaymentStatus.Authorised).ToList();
            if (payments.Count == 0) return FormattableString.Invariant($"No authorised payment. {quote.Pricing.Deposit:0.00} USD due.");
            var receipt = payments[^1];
            return FormattableString.Invariant($"{payments.Count} authorised payment{(payments.Count == 1 ? "" : "s")}. {receipt.ReceiptId}: {receipt.Amount:0.00} {receipt.Currency}, card ending {receipt.LastFour}. {BalanceFor(draftId, quote).Due:0.00} USD due.");
        }
    }

    private DepositBalance BalanceFor(string draftId, QuoteState quote)
    {
        var paid = _world.Payments.Where(p => p.DraftId == draftId && p.Status == PaymentStatus.Authorised).Sum(p => p.Amount);
        var due = Math.Max(0m, quote.Pricing.Deposit - paid);
        return new DepositBalance(due, due == 0m);
    }

    internal (int Status, PaymentReply? Reply) ReadDeposit(string draftId, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, null);
            if (!CoordinatorServes(draft.Data.CoordinatorId, draft.Data.Location.CommunityId)) return (403, null);
            return DepositReceipt(draft, operation) is { } receipt ? (200, receipt) : (404, null);
        }
    }

    private PaymentReply? DepositReceipt(DraftState draft, Guid operation)
    {
        var receipt = _world.Payments.Find(p => p.DraftId == draft.Id && p.OperationId == operation && p.Status == PaymentStatus.Authorised);
        return draft.Quote is { } quote && receipt is not null ? new PaymentReply(new DepositResult(receipt, BalanceFor(draft.Id, quote))) : null;
    }

    internal (int Status, PaymentReply? Reply, string Reason) AuthoriseDeposit(string draftId, DepositPage sent, string? coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { Quote: { } quote } draft) return (404, null, "No deposit quote.");
            if (sent.DraftId != draftId || coordinator != draft.Data.CoordinatorId || !CoordinatorServes(coordinator, draft.Data.Location.CommunityId)
                || !_world.Coordinators.Any(c => c.Id == coordinator && c.CanTakeDeposit))
            {
                return (403, null, "This coordinator cannot authorise the deposit.");
            }
            if (operation == Guid.Empty || sent.OperationId != operation || draft.Data.OperationId != operation) return (400, null, "Use this draft's deposit operation.");
            if (draft.DepositReceiptId is not null)
            {
                return DepositReceipt(draft, operation) is { } existing
                    ? (200, existing, string.Empty)
                    : (409, null, "This draft already has an authorised deposit.");
            }
            if (draft.Status != DraftStatus.Open) return (410, null, "This draft is closed.");
            if (sent.Amount != quote.Pricing.Deposit) return (409, null, FormattableString.Invariant($"The required deposit changed to {quote.Pricing.Deposit:0.00} USD."));
            if (draft.AcceptedQuoteRevision != quote.Revision || quote.ExpiresAt <= _world.Clock.Now) return (409, null, "Accept the current quote before paying.");
            var digits = sent.CardNumber.Replace(" ", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal);
            if (digits.Length < 4) return (400, null, "Enter a valid card number.");
            if (_world.Rules.DeclinedCardNumbers.Contains(digits)) return (402, null, "The deposit was declined.");
            var receipt = new PaymentState
            {
                ReceiptId = $"PAY{100 + _world.Payments.Count}",
                DraftId = draftId,
                OperationId = operation,
                Amount = sent.Amount,
                Currency = sent.Currency,
                LastFour = digits[^4..],
                Status = PaymentStatus.Authorised,
                CreatedAt = _world.Clock.Now,
            };
            _world.Payments.Add(receipt);
            draft.DepositReceiptId = receipt.ReceiptId;
            Record(draft, operation, "authorise-deposit", receipt.ReceiptId, coordinator);
            return (201, new PaymentReply(new DepositResult(receipt, BalanceFor(draftId, quote))), string.Empty);
        }
    }
}

internal sealed partial class MoveInReads
{
    internal DepositPage? DepositForm(string draftId) => Desk.DepositForm(draftId);
    internal string DepositOnFile(string draftId) => Desk.DepositOnFile(draftId);
}

internal static partial class MoveInValidators
{
    static partial void ForTheDeposit(ReactiveFluentValidationBuilder validators) => validators.Add<DepositRules>();
}

public sealed class DepositRules : ReactiveValidator<DepositPage>
{
    public DepositRules()
    {
        RuleFor(p => p.Cardholder).NotEmpty().WithMessage("Enter the cardholder.");
        RuleFor(p => p.CardNumber).Cascade(CascadeMode.Stop).NotEmpty().CreditCard().WithMessage("Enter a valid card number.").MinimumLength(4);
        RuleFor(p => p.Amount).GreaterThan(0m).Equal(p => p.RequiredAmount).WithMessage("Enter the required deposit.").PrecisionScale(7, 2, true);
        RuleFor(p => p.Currency).Equal("USD");
        RuleFor(p => p.Consent).Equal(true).WithMessage("Authorise this deposit first.");
    }
}
