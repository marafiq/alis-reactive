using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Slots;

/// <summary>
/// A resident's move-in, taken in steps inside one form: who takes it, and the step on show. The first step is
/// rendered with the page inside the container the steps load into; its Next loads the second step there.
/// </summary>
public sealed class MoveInPage
{
    public string? Coordinator { get; set; }
}

/// <summary>The move-in's rules for what the page itself holds.</summary>
public sealed class MoveInRules : ReactiveValidator<MoveInPage>
{
    public MoveInRules()
    {
        RuleFor(x => x.Coordinator).NotEmpty().WithMessage("Say who takes the move-in.");
    }
}

/// <summary>The first step, the resident: a partial of its own model, checked by its own rules on the page's form.</summary>
public sealed class MoveInResident
{
    public string? Name { get; set; }
}

/// <summary>The resident step's rules.</summary>
public sealed class MoveInResidentRules : ReactiveValidator<MoveInResident>
{
    public MoveInResidentRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the resident.");
    }
}

/// <summary>The resident's special needs: a part of the resident step, inside a container of the step's own.</summary>
public sealed class MoveInNeeds;

/// <summary>The second step, the room: a room may be chosen later, so the step checks nothing.</summary>
public sealed class MoveInRoom
{
    public string? Room { get; set; }
}

/// <summary>What the save endpoint kept, in its own words.</summary>
public sealed class MoveInSaved
{
    public string Summary { get; set; } = "";
}

/// <summary>The move-in opened, told by the page when it starts: who takes it.</summary>
public sealed class MoveInOpened
{
    public string? Coordinator { get; set; }
}

/// <summary>A ring of the move-in desk's bell, told to whoever on the page listens.</summary>
public sealed class MoveInBellRang
{
    public int Times { get; set; }
}

public static class MoveInEvents
{
    public static readonly AppEvent<MoveInOpened> Opened = AppEvent.Declare<MoveInOpened>();

    public static readonly AppEvent<MoveInBellRang> BellRang = AppEvent.Declare<MoveInBellRang>();
}
