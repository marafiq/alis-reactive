namespace Alis.Reactive.Declarations;

/// <summary>
/// A JavaScript object whose events a pipeline listens to: it states how its events are listened to.
/// </summary>
public abstract class EventedObject : JsObject
{
    /// <summary>Gets how the object's events are listened to and released.</summary>
    protected internal abstract EventListening Listening { get; }
}
