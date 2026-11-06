namespace Alis.Reactive.Framework.App.Validation;

public sealed class Ledger
{
    public List<LedgerLine> Lines { get; set; } = new() { new LedgerLine("Linen", 2) };
}

public sealed class LedgerLine
{
    public LedgerLine(string item)
        : this(item, 1)
    {
    }

    public LedgerLine(string item, int count)
    {
        Item = item;
        Count = count;
    }

    public string? Item { get; set; }

    public int Count { get; set; }
}

public sealed class LedgerRules : ReactiveValidator<Ledger>
{
    public LedgerRules()
    {
        RuleForEach(x => x.Lines).ChildRules(line => line.RuleFor(x => x.Item).NotEmpty().WithMessage("Name each item."));
    }
}
