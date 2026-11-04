namespace Alis.Reactive.Framework.App.Validation;

public sealed class LinenRound
{
    public List<string?> Rooms { get; set; } = new() { "E-12", "E-14" };

    public List<string?> Carts { get; set; } = new() { "C-1", "C-2" };
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
    }
}
