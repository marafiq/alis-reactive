using Alis.Reactive.Components;
using Alis.Reactive.Values;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/WardPager/ward-pager", "WardPager")]
public sealed class WardPager : Plugin
{
    public WardPager()
        : base("ward-pager", EventListening.Methods("on", "off"))
    {
    }

    public ReadOnly<string> LastHandled => ReadOnly<string>();

    public ReadOnly<string> LastLogged => ReadOnly<string>();

    public Event<PagerPaged> Paged => Event<PagerPaged>();

    public void Page(Arg<string> room) => Method([room]);

    public void Answer(Arg<string> room, Arg<string> by) => Method([room, by]);
}

public sealed class PagerPaged : JsObject
{
    public ReadOnly<string> Room => ReadOnly<string>();

    public Property<bool> Handled => Property<bool>();

    public PagerCall Call => LiveObject<PagerCall>();

    public PagerCall Relief => LiveObject<PagerCall>(nullable: true);
}

public sealed class PagerCall : EventedObject
{
    protected override EventListening Listening { get; } = EventListening.Methods("on", "off");

    public Event<CallAnswered> Answered => Event<CallAnswered>();

    public void Acknowledge() => MethodAsync();
}

public sealed class CallAnswered : JsObject
{
    public ReadOnly<string> By => ReadOnly<string>();

    public Property<bool> Logged => Property<bool>();
}
