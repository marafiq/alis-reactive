namespace Alis.Reactive.Framework.App.Slots;

/// <summary>The hall: a bell, and a slot the notice board loads into.</summary>
public sealed class HallPage;

/// <summary>The notice board: it hears the hall's bell, and holds a slot a note is pinned into.</summary>
public sealed class HallBoard;

/// <summary>A note pinned to the board: it hears the hall's bell, and carries a stamp its own script builds.</summary>
public sealed class HallNote;

/// <summary>A ring of the hall's bell, told to whoever on the page listens.</summary>
public sealed class HallBellRang
{
    public int Times { get; set; }
}

public static class HallEvents
{
    public static readonly AppEvent<HallBellRang> Rang = AppEvent.Declare<HallBellRang>();
}
