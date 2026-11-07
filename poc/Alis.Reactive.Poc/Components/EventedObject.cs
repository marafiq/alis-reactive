namespace Alis.Reactive.Components;

public abstract class EventedObject : JsObject
{
    protected internal abstract EventListening Listening { get; }
}
