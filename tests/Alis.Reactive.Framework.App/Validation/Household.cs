using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>A household: tags and rooms checked item by item, a caller checked by its own rules, and a note.</summary>
public sealed class Household
{
    public List<string?> Tags { get; set; } = new() { "ward", "east" };

    public List<string?> Rooms { get; set; } = new() { "E-1", "E-2" };

    public Caller Caller { get; set; } = new();

    public List<Visitor> Visitors { get; set; } = new() { new Visitor() };

    public string? Note { get; set; } = "Quiet";
}

/// <summary>Whom the desk calls about the household.</summary>
public sealed class Caller
{
    public string? Phone { get; set; } = "555-0101";

    public List<string?> Extensions { get; set; } = new() { "12" };
}

/// <summary>A visitor the household expects.</summary>
public sealed class Visitor
{
    public string? Name { get; set; } = "Ada";
}

/// <summary>A visitor's own rules.</summary>
public sealed class VisitorRules : ReactiveValidator<Visitor>
{
    public VisitorRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name each visitor.");
    }
}

/// <summary>The caller's own rules.</summary>
public sealed class CallerRules : ReactiveValidator<Caller>
{
    public CallerRules()
    {
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Give a phone to call.");
        RuleForEach(x => x.Extensions).NotEmpty().WithMessage("Give each extension.");
    }
}

/// <summary>Rules for a household's note, included where the household's rules say.</summary>
public sealed class NoteRules : ReactiveValidator<Household>
{
    public NoteRules()
    {
        RuleFor(x => x.Note).MaximumLength(10).WithMessage("Ten characters a note.");
    }
}

/// <summary>
/// Two checks on each tag; a check of each room only C# makes; and, while the household has more than one
/// room (a count only the server reads), the caller's rules, each visitor's and the note's.
/// </summary>
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
