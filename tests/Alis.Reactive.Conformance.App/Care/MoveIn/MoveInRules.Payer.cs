using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInRules
{
    internal const string PayerCoverage = "Payer";

    partial void ThePayer() => RuleSet(PayerCoverage, () =>
    {
        When(m => m.Payer.Kind != PayerKind.Private, () =>
        {
            RuleFor(m => m.Payer.Primary).SetValidator(new CoverageRules());
            RuleFor(m => m.Payer.PortalUrl).Url().WithMessage("Enter a valid payer portal URL.");
            RuleFor(m => m.Payer.Primary.StartsOn).LessThanOrEqualTo(m => m.Resident.MoveInDate).WithMessage("Primary coverage starts after move-in.");
            RuleFor(m => m.Payer.Primary.EndsOn).GreaterThanOrEqualTo(m => m.Resident.MoveInDate).WithMessage("Primary coverage ends before move-in.");
        });
        When(m => m.Payer.HasSecondary, () =>
        {
            RuleFor(m => m.Payer.Secondary).NotNull().WithMessage("Enter the secondary coverage.");
            RuleFor(m => m.Payer.Secondary!).ChildRules(rules => rules.RuleSet(PayerCoverage, () =>
            {
                rules.RuleFor(s => s.PolicyNumber).NotEmpty().WithMessage("Enter the secondary policy number.").Matches("^POL-[0-9]+$").WithMessage("Enter a policy number starting with POL-.");
                rules.RuleFor(s => s.Provider).NotEmpty().WithMessage("Enter the secondary provider.");
                rules.RuleFor(s => s.ContributionPercent).ExclusiveBetween(0m, 100m).WithMessage("Secondary contribution must be above 0 and below 100 percent.");
            }));
            When(m => m.Payer.Secondary != null, () =>
            {
                RuleFor(m => m.Payer.Secondary!.StartsOn).LessThanOrEqualTo(m => m.Resident.MoveInDate).WithMessage("Secondary coverage starts after move-in.");
                RuleFor(m => m.Payer.Secondary!.EndsOn).GreaterThanOrEqualTo(m => m.Resident.MoveInDate).WithMessage("Secondary coverage ends before move-in.");
            });
            RuleFor(m => m.Payer).Custom((payer, context) =>
            {
                if (payer.Secondary is null || context.Model.Payer.Primary.ContributionPercent + payer.Secondary.ContributionPercent == 100m) return;
                var message = payer.Primary.ContributionPercent + payer.Secondary.ContributionPercent > 100m
                    ? "Secondary exceeds the remaining primary coverage." : "Secondary falls short of the remaining primary coverage.";
                context.AddFailure(m => m.Payer.Secondary!.ContributionPercent, message);
                context.AddFailure("The two policies must cover 100 percent in total.");
            }).ServerOnly();
        }).Otherwise(() => RuleFor(m => m.Payer.Secondary).Empty().WithMessage("Inactive secondary coverage must be removed."));
    });
}

internal sealed class CoverageRules : ReactiveValidator<CoverageInput>
{
    public CoverageRules() => RuleSet(MoveInRules.PayerCoverage, () =>
    {
        RuleFor(m => m.PolicyNumber).NotEmpty().WithMessage("Enter the primary policy number.").Matches("^POL-[0-9]+$").WithMessage("Enter a policy number starting with POL-.");
        RuleFor(m => m.ContributionPercent).InclusiveBetween(0m, 100m).WithMessage("Primary contribution must be between 0 and 100 percent.");
    });
}

internal sealed class PayerVerificationRules : ReactiveValidator<PayerVerificationPage>
{
    public PayerVerificationRules(MoveInReads reads)
    {
        RuleFor(m => m.Consent).Equal(true).WithMessage("Consent is required.");
        RuleFor(m => m.Policy.PolicyNumber).NotEmpty().WithMessage("Enter the policy number.").Matches("^POL-[0-9]+$").WithMessage("Enter a policy number starting with POL-.");
        WhenOnServer(page => reads.BenefitsRequireReference(page.DraftId), () =>
            RuleFor(page => page.Reference).NotEmpty().WithMessage("Benefits coverage requires an allocation reference.").ServerOnly());
        WhenAsync((page, token) => Task.FromResult(page.Consent), () =>
            RuleFor(m => m.Policy.PolicyNumber).MustAsync((page, number, token) => Task.FromResult(reads.PolicyRefusal(page) is null))
                .WithMessage(page => reads.PolicyRefusal(page) ?? "This policy cannot be verified."));
    }
}

internal static partial class MoveInValidators
{
    static partial void ForThePayerVerification(ReactiveFluentValidationBuilder validators) => validators.Add<PayerVerificationRules>();
}
