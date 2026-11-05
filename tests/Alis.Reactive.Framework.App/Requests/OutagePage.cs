namespace Alis.Reactive.Framework.App.Requests;

public sealed class OutagePage
{
    public string? Signer { get; set; }
}

public sealed class OutageRules : ReactiveValidator<OutagePage>
{
    public OutageRules()
    {
        RuleFor(x => x.Signer).NotEmpty().WithMessage("Sign for the ledger.");
    }
}

public sealed class LedgerNote
{
    public string? Title { get; set; }
}

public sealed class LimitRoute
{
    public int? Limit { get; set; }
}
