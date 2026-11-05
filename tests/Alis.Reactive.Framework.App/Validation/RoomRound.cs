namespace Alis.Reactive.Framework.App.Validation;

public sealed class RoomRound
{
    public List<string?>? Needs { get; set; }

    public List<string?> Rooms { get; set; } = new() { "E-1", "E-2" };

    public string? EastKey { get; set; } = "yes";

    public string? Helper { get; set; } = "yes";

    public string? Checklist { get; set; } = "yes";

    public string? Lift { get; set; } = "yes";

    public string? Porter { get; set; } = "yes";

    public string? Trolley { get; set; } = "yes";

    public RoundLead Lead { get; set; } = new();

    public List<string?> Wings { get; set; } = new() { "East" };

    public string? Badge { get; set; } = "yes";

    public string? Steward { get; set; } = "yes";

    public string? Deputy { get; set; } = "yes";
}

public sealed class RoundLead
{
    public string? Name { get; set; } = "Kim";
}

public sealed class RoomRoundRules : ReactiveValidator<RoomRound>
{
    public RoomRoundRules()
    {
        RuleFor(x => x.Rooms).NotEmpty().WithMessage("Name a room for the round.");
        RuleFor(x => x.Lead).NotNull().WithMessage("Name a lead for the round.");
        RuleFor(x => x.Lead).NotEmpty().WithMessage("Give the round a lead.");
        RuleFor(x => x.Steward).NotEmpty().When(x => x.Lead == null).WithMessage("A round with no lead takes a steward.");
        RuleFor(x => x.Deputy).NotEmpty().When(x => x.Lead != null).WithMessage("A round with a lead names a deputy.");
        RuleFor(x => x.Badge).NotEmpty().When(x => x.Wings.Contains("North")).WithMessage("The north wing asks for a badge.");
        RuleFor(x => x.EastKey).NotEmpty().When(x => x.Rooms.Contains("E-9")).WithMessage("Take the east key for E-9.");
        RuleFor(x => x.Porter).NotEmpty().When(x => x.Rooms.Count >= 2).WithMessage("Two rooms or more call for a porter.");
        RuleFor(x => x.Trolley).NotEmpty().When(x => x.Rooms.Count > 2).WithMessage("More than two rooms call for a trolley.");
        RuleFor(x => x.Helper).NotEmpty().When(x => x.Needs != null && x.Needs.Any()).WithMessage("A round with needs takes a helper.");
        RuleFor(x => x.Checklist).NotEmpty().When(x => x.Needs == null || !x.Needs.Any()).WithMessage("A round with no needs signs the checklist.");
        RuleFor(x => x.Lift).NotEmpty().When(x => x.Needs != null && x.Needs.Count > 1).WithMessage("Two needs or more call for the lift.");
    }
}
