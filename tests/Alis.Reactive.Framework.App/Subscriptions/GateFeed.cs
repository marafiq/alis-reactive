namespace Alis.Reactive.Framework.App.Subscriptions;

public sealed class GateFeed : ServerPushStream
{
    public Event News => Event();

    public GateLine Line => Object<GateLine>();
}

public sealed class GateLine : JsObject
{
    public Event Cut => Event();
}
