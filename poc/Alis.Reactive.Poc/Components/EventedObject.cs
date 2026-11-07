namespace Alis.Reactive.Components;

public abstract class EventedObject : JsObject
{
    protected EventedObject(EventListening listening)
    {
        Listening = listening;
    }

    internal EventListening Listening { get; }
}
