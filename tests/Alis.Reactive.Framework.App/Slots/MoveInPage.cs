
namespace Alis.Reactive.Framework.App.Slots;

public sealed class MoveInPage
{
    public string? Coordinator { get; set; }
}

public sealed class MoveInRules : ReactiveValidator<MoveInPage>
{
    public MoveInRules()
    {
        RuleFor(x => x.Coordinator).NotEmpty().WithMessage("Say who takes the move-in.");
    }
}

public sealed class MoveInResident
{
    public string? Name { get; set; }
}

public sealed class MoveInResidentRules : ReactiveValidator<MoveInResident>
{
    public MoveInResidentRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the resident.");
    }
}

public sealed class MoveInNeeds;

public sealed class MoveInRoom
{
    public string? Room { get; set; }
}

public sealed class MoveInSaved
{
    public string Summary { get; set; } = "";
}

public sealed class MoveInOpened
{
    public string? Coordinator { get; set; }
}

public sealed class MoveInBellRang
{
    public int Times { get; set; }
}

public static class MoveInEvents
{
    public static readonly AppEvent<MoveInOpened> Opened = AppEvent.Declare<MoveInOpened>();

    public static readonly AppEvent<MoveInBellRang> BellRang = AppEvent.Declare<MoveInBellRang>();
}
