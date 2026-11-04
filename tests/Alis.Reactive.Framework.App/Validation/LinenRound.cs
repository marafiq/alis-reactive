namespace Alis.Reactive.Framework.App.Validation;

public sealed class LinenRound
{
    public List<string?> Rooms { get; set; } = new() { "E-12", "E-14" };

    public List<string?> Carts { get; set; } = new() { "C-1", "C-2" };

    public List<string?> Bins { get; set; } = new() { "B-1", "B-2" };

    public string? Porter { get; set; }
}

public sealed class LinenRoundRules : ReactiveValidator<LinenRound>
{
    public LinenRoundRules()
    {
        RuleForEach(x => x.Rooms).Cascade(CascadeMode.Stop)
            .Must(room => room != null && room.StartsWith("E-")).WithMessage("East wing rooms only.")
            .Must(room => room != null && room.Length == 4).WithMessage("Four characters a room.");
        RuleForEach(x => x.Carts)
            .Must(cart => cart != null && cart.StartsWith("C-")).WithMessage("Carts start with C-.")
            .Must(cart => cart != null && cart.Length == 3).WithMessage("Three characters a cart.");
        RuleForEach(x => x.Bins).Custom((bin, context) =>
        {
            if (bin == "B-13")
                context.AddFailure("'{PropertyName}' {PropertyValue} is out of service.");
            if (bin == "B-99" && string.IsNullOrWhiteSpace(context.Model.Porter))
                context.AddFailure(x => x.Porter, "Bin {PropertyValue} goes to the porter; name one.");
        });
    }
}
