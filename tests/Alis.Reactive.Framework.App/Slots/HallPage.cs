namespace Alis.Reactive.Framework.App.Slots;

public sealed class HallPage;

public sealed class HallBoard;

public sealed class HallNote;

public sealed class HallBellRang
{
    public int Times { get; set; }
}

public static class HallEvents
{
    public static readonly AppEvent<HallBellRang> Rang = AppEvent.Declare<HallBellRang>();
}
