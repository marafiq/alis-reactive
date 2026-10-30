namespace Alis.Reactive.Framework.App.Slots;

/// <summary>The lounge: a bell on the page, and a slot the visitor card loads into.</summary>
public sealed class LoungePage;

/// <summary>The visitor card: it hears the lounge's bell, and has a bell of its own.</summary>
public sealed class VisitorCard;

/// <summary>A ring of the lounge's bell, told to whoever on the page listens.</summary>
public sealed class LoungeBellRang
{
    public int Times { get; set; }
}

public static class LoungeEvents
{
    public static readonly AppEvent<LoungeBellRang> LoungeBellRang = AppEvent.Declare<LoungeBellRang>();
}
