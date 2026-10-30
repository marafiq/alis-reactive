using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>Values compared for equality and order under every shape: numbers, flags, dates, instants and text. Rooms is 3.</summary>
public sealed class SamenessForm
{
    public int Rooms { get; set; } = 3;

    public int Visitors { get; set; } = 5;

    public int Beds { get; set; } = 1;

    public bool Escorted { get; set; } = true;

    public DateTime Arrives { get; set; } = new(2026, 10, 1);

    public DateTimeOffset Called { get; set; } = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

    public bool Consented { get; set; } = true;

    public int Wards { get; set; } = 3;

    public string Surname { get; set; } = "Mill";

    public string Initial { get; set; } = "B";

    public List<string>? Wings { get; set; }

    public List<string>? FormerWings { get; set; }
}

/// <summary>One rule per shape a comparison reads its value under.</summary>
public sealed class SamenessRules : ReactiveValidator<SamenessForm>
{
    public SamenessRules()
    {
        RuleFor(x => x.Visitors).Equal(5).WithMessage("Not as required: Visitors.");
        RuleFor(x => x.Beds).NotEqual(0).WithMessage("Not as required: Beds.");
        RuleFor(x => x.Escorted).Equal(true).WithMessage("Not as required: Escorted.");
        RuleFor(x => x.Arrives).Equal(new DateTime(2026, 10, 1)).WithMessage("Not as required: Arrives.");
        RuleFor(x => x.Called).Equal(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero)).WithMessage("Not as required: Called.");
        RuleFor(x => x.Consented).GreaterThan(false).WithMessage("Not as required: Consented.");
        RuleFor(x => x.Wards).Equal(x => x.Rooms).WithMessage("Not as required: Wards.");
        RuleFor(x => x.Surname).GreaterThan("M").WithMessage("Not as required: Surname.");
        RuleFor(x => x.Initial).InclusiveBetween("A", "M").WithMessage("Not as required: Initial.");
        RuleFor(x => x.Wings).NotEqual(x => x.FormerWings).WithMessage("Not as required: Wings.");
    }
}
