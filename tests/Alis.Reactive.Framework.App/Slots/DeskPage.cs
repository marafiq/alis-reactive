using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Slots;

/// <summary>The front desk: a bell on the page, and a slot the next-of-kin card loads into.</summary>
public sealed class DeskPage
{
    public KinCard NextOfKin { get; set; } = new();
}

/// <summary>The next-of-kin card: a partial of its own model, rendered under the desk's NextOfKin prefix.</summary>
public sealed class KinCard
{
    public string? Name { get; set; }
}

/// <summary>The card's rules: a name the browser can check is there, and one only the server refuses.</summary>
public sealed class KinRules : ReactiveValidator<KinCard>
{
    public KinRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the next of kin.");
        RuleFor(x => x.Name).Must(name => name != "Nobody").WithMessage("Nobody is not a name.");
    }
}

/// <summary>The desk's rules, which check the next of kin by the card's own.</summary>
public sealed class DeskRules : ReactiveValidator<DeskPage>
{
    public DeskRules()
    {
        RuleFor(x => x.NextOfKin).SetValidator(new KinRules());
    }
}

/// <summary>What the endpoint bound, in its own words.</summary>
public sealed class KinEcho
{
    public string Summary { get; set; } = "";
}

/// <summary>A ring of the desk's bell, told to whoever on the page listens.</summary>
public sealed class BellRang
{
    public int Times { get; set; }
}

public static class DeskEvents
{
    public static readonly AppEvent<BellRang> Rang = AppEvent.Declare<BellRang>();
}
