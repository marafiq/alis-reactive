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
