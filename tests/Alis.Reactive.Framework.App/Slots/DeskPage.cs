
namespace Alis.Reactive.Framework.App.Slots;

public sealed class DeskPage
{
    public KinCard NextOfKin { get; set; } = new();
}

public sealed class KinCard
{
    public string? Name { get; set; }
}

public sealed class KinRules : ReactiveValidator<KinCard>
{
    public KinRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the next of kin.");
        RuleFor(x => x.Name).Must(name => name != "Nobody").WithMessage("Nobody is not a name.");
    }
}

public sealed class DeskRules : ReactiveValidator<DeskPage>
{
    public DeskRules()
    {
        RuleFor(x => x.NextOfKin).SetValidator(new KinRules());
    }
}

public sealed class KinEcho
{
    public string Summary { get; set; } = "";
}

public sealed class BellRang
{
    public int Times { get; set; }
}

public static class DeskEvents
{
    public static readonly AppEvent<BellRang> Rang = AppEvent.Declare<BellRang>();
}
