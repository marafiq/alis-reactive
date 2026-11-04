namespace Alis.Reactive.Framework.App.Slots;

public sealed class LoungePage;

public sealed class VisitorCard;

public sealed class LoungeBellRang
{
    public int Times { get; set; }
}

public static class LoungeEvents
{
    public static readonly AppEvent<LoungeBellRang> LoungeBellRang = AppEvent.Declare<LoungeBellRang>();
}
