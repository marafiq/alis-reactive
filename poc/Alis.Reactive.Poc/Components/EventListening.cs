using Alis.Reactive.Plan;

namespace Alis.Reactive.Components;

public sealed class EventListening
{
    private EventListening(Listening listen)
    {
        Listen = listen;
    }

    public static EventListening EventTarget { get; } = new(MethodListening.EventTarget);

    public static EventListening CallbackProperties { get; } = new(new AssignmentListening());

    internal Listening Listen { get; }

    internal static EventListening Of(Listening listen) => new(listen);

    public static EventListening Methods(string subscribe, string unsubscribe)
        => new(new MethodListening(JavaScriptNames.Checked(subscribe), JavaScriptNames.Checked(unsubscribe)));

    public static EventListening ReturnedUnsubscriber(string subscribe)
        => new(new ReturnedListening(JavaScriptNames.Checked(subscribe)));
}
