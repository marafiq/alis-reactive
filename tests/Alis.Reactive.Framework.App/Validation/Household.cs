
namespace Alis.Reactive.Framework.App.Validation;

public sealed class Household
{
    public List<string?> Tags { get; set; } = new() { "ward", "east" };

    public List<string?> Rooms { get; set; } = new() { "E-1", "E-2" };

    public Caller Caller { get; set; } = new();

    public List<Visitor> Visitors { get; set; } = new() { new Visitor() };

    public string? Note { get; set; } = "Quiet";
}

public sealed class Caller
{
    public string? Phone { get; set; } = "555-0101";

    public List<string?> Extensions { get; set; } = new() { "12" };
}

public sealed class Visitor
{
    public string? Name { get; set; } = "Ada";
}

public sealed class VisitorRules : ReactiveValidator<Visitor>
{
    public VisitorRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name each visitor.");
    }
}

public sealed class CallerRules : ReactiveValidator<Caller>
{
    public CallerRules()
    {
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Give a phone to call.");
        RuleForEach(x => x.Extensions).NotEmpty().WithMessage("Give each extension.");
    }
}

public sealed class NoteRules : ReactiveValidator<Household>
{
    public NoteRules()
    {
        RuleFor(x => x.Note).MaximumLength(10).WithMessage("Ten characters a note.");
    }
}

public sealed class HouseholdRules : ReactiveValidator<Household>
{
    public HouseholdRules()
    {
        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Give each tag.")
            .MaximumLength(5).WithMessage("Five letters a tag.");
        RuleForEach(x => x.Rooms).Must(room => room != null && room.StartsWith("E-")).WithMessage("East wing rooms only.");
        WhenOnServer(x => x.Rooms.Count > 1, () =>
        {
            RuleFor(x => x.Caller).SetValidator(new CallerRules());
            RuleForEach(x => x.Visitors).SetValidator(new VisitorRules());
            Include(new NoteRules());
        });
    }
}
