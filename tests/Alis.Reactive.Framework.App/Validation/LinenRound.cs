namespace Alis.Reactive.Framework.App.Validation;

public sealed class LinenRound
{
    public List<string?> Rooms { get; set; } = new() { "E-12", "E-14" };

    public List<string?> Carts { get; set; } = new() { "C-1", "C-2" };

    public List<string?> Bins { get; set; } = new() { "B-1", "B-2" };

    public string? Porter { get; set; }

    public List<LinenTrolley> Trolleys { get; set; } = new()
    {
        new LinenTrolley { Tag = "T-1", Wing = "East" },
        new LinenTrolley { Tag = "T-2", Wing = "West" },
    };

    public List<string?> Shelves { get; set; } = new() { "S-1", "S-2" };

    public LinenLaundry Laundry { get; set; } = new();
}

public sealed class LinenTrolley
{
    public string? Tag { get; set; }

    public string? Wing { get; set; }
}

public sealed class LinenLaundry
{
    public List<string?> Bags { get; set; } = new() { "L-1", "L-2" };
}

public sealed class LinenTrolleyRules : ReactiveValidator<LinenTrolley>
{
    public LinenTrolleyRules()
    {
        RuleFor(x => x.Tag).Must(tag => tag != null && tag.StartsWith("T-")).WithMessage("Trolley tags start with T-.");
        RuleFor(x => x.Wing).Must(wing => wing is "East" or "West").WithMessage("Send a trolley East or West.");
    }
}

public sealed class LinenLaundryRules : ReactiveValidator<LinenLaundry>
{
    public LinenLaundryRules()
    {
        RuleForEach(x => x.Bags).Custom((bag, context) =>
        {
            if (bag == "L-13")
                context.AddFailure($"Bag {bag} is torn.");
        });
    }
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
        RuleForEach(x => x.Trolleys).Cascade(CascadeMode.Stop).SetValidator(new LinenTrolleyRules());
        RuleForEach(x => x.Shelves).Cascade(CascadeMode.Stop).Custom((shelf, context) =>
        {
            if (shelf != null && shelf.StartsWith("X-"))
                context.AddFailure($"Shelf {shelf} is closed.");
        });
        RuleFor(x => x.Laundry).SetValidator(new LinenLaundryRules());
        RuleForEach(x => x.Bins).Custom((bin, context) =>
        {
            if (bin == "B-13")
                context.AddFailure("'{PropertyName}' {PropertyValue} is out of service.");
            if (bin == "B-99" && string.IsNullOrWhiteSpace(context.Model.Porter))
                context.AddFailure(x => x.Porter, "Bin {PropertyValue} goes to the porter; name one.");
        });
    }
}
