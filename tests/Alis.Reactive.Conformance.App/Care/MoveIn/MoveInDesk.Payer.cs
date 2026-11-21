namespace Alis.Reactive.Conformance.App.Care;

internal sealed partial class MoveInDesk
{
    static partial void KeepThePayer(MoveInPage saved, MoveInPage sent)
    {
        if (!sent.IncludesPayer) return;
        sent.Payer.VerificationId = sent.Payer.Kind == saved.Payer.Kind && sent.Payer.Primary.PolicyNumber == saved.Payer.Primary.PolicyNumber && sent.Payer.ContactId == saved.Payer.ContactId
            ? saved.Payer.VerificationId : null;
        saved.Payer = sent.Payer;
    }

    internal string? PolicyRefusal(PayerVerificationPage page)
    {
        lock (_gate)
        {
            var policy = _world.Policies.Find(p => p.Number == page.Policy.PolicyNumber);
            if (policy is null || policy.ResidentId != page.ResidentId) return "This policy does not cover this resident.";
            if (!policy.Active) return $"{policy.Number} is inactive.";
            if (policy.Blocked || _world.Rules.BlockedPolicyNumbers.Contains(policy.Number)) return $"{policy.Number} is blocked.";
            return policy.AuthorisedContactId != page.ContactId ? "This contact cannot authorise the policy." : null;
        }
    }

    internal (int Status, VerificationReply? Reply, string? Reason) Verify(string draftId, string? community, string? reference, PayerVerificationPage page, string coordinator, Guid operation)
    {
        lock (_gate)
        {
            if (DraftOf(draftId) is not { } draft) return (404, null, null);
            if (draft.Status != DraftStatus.Open) return (410, null, null);
            if (page.DraftId != draftId || page.ResidentId != draft.Data.ResidentId || !CoordinatorServes(coordinator, draft.Data.Location.CommunityId)) return (403, null, null);
            if (reference is not null && community != draft.Data.Location.CommunityId) return (403, null, null);
            if (reference is null && community is not null) return (403, null, null);
            if (PolicyRefusal(page) is { } reason) return (422, null, reason);
            var payer = draft.Data.Payer;
            if (payer.Kind == PayerKind.Private || payer.Kind != page.FundingSource || payer.Primary.PolicyNumber != page.Policy.PolicyNumber || payer.ContactId != page.ContactId)
                return (422, null, "The saved payer changed. Reopen payer verification.");
            var sequence = _world.History.Count(entry => entry.Action == "payer-verification") + 100;
            var id = $"V{sequence}";
            var policy = page.Policy.PolicyNumber;
            _world.Reviews.Add(new ReviewState { Id = id, DraftId = draftId, Kind = ReviewKind.Payer, DraftVersion = draft.Version, Passed = true, Reference = policy, Message = reference is null ? "No allocation reference supplied" : $"Allocation reference {reference}, community {community}" });
            draft.PayerVerificationId = id;
            draft.Data.Payer.VerificationId = id;
            Record(draft, operation, "payer-verification", id, coordinator);
            var details = new InstructionReference(true, policy.Replace("-", "", StringComparison.Ordinal));
            return (200, new VerificationReply(new VerifiedPolicy(policy, new VerificationDecision(id, true, details))), null);
        }
    }

    internal string VerificationContext(string draftId)
    {
        lock (_gate)
        {
            var draft = DraftOf(draftId);
            return _world.Reviews.Find(r => r.DraftId == draftId && r.Id == draft?.PayerVerificationId && r.Kind == ReviewKind.Payer)?.Message ?? string.Empty;
        }
    }

    internal string? PolicyDetails(string draftId, string reference)
    {
        lock (_gate)
        {
            var draft = DraftOf(draftId);
            if (draft?.PayerVerificationId is null) return null;
            var verified = _world.Reviews.Find(r => r.Id == draft.PayerVerificationId && r.DraftId == draftId && r.Kind == ReviewKind.Payer && r.Passed);
            var number = verified?.Reference ?? draft.Data.Payer.Primary.PolicyNumber;
            if (number.Replace("-", "", StringComparison.Ordinal) != reference) return null;
            var policy = _world.Policies.Find(p => p.Number == number && p.ResidentId == draft.Data.ResidentId);
            return policy is null ? null : FormattableString.Invariant($"{policy.Provider} covers up to {policy.MonthlyLimit:0.00} per month.");
        }
    }
}

internal sealed partial class MoveInReads
{
    internal string PayerOnFile(PayerInput payer)
    {
        if (payer.Kind == PayerKind.Private) return "Private funding. No secondary coverage.";
        var primary = FormattableString.Invariant($"{payer.Kind}: {payer.Primary.Provider} {payer.Primary.PolicyNumber} at {payer.Primary.ContributionPercent:0.##}%.");
        return payer.Secondary is { } second ? primary + FormattableString.Invariant($" {second.Provider} {second.PolicyNumber} at {second.ContributionPercent:0.##}%.") : primary + " No secondary coverage.";
    }

    internal string VerificationContext(string draftId) => Desk.VerificationContext(draftId);

    internal bool BenefitsRequireReference(string draftId) => Desk.PageOf(draftId)?.Payer.Kind == PayerKind.Benefits;

    internal string? PolicyRefusal(PayerVerificationPage page) => Desk.PolicyRefusal(page);
}
