namespace Alis.Reactive.Framework.App.Validation;

public sealed class PharmacyOrder
{
    public decimal Amount { get; set; } = 0.5m;

    public decimal Ledger { get; set; } = 100m;
}

public sealed class PharmacyOrderRules : ReactiveValidator<PharmacyOrder>
{
    public PharmacyOrderRules()
    {
        RuleFor(x => x.Amount).PrecisionScale(9, 8, ignoreTrailingZeros: true);
        RuleFor(x => x.Ledger).PrecisionScale(24, 0, ignoreTrailingZeros: true);
    }
}
