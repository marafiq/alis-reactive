namespace Alis.Reactive.Framework.App.Subscriptions;

/// <summary>The gate's feed: its own news, and a line it holds that is cut now and then.</summary>
public sealed class GateFeed : ServerPushStream
{
    public Event News => Event();

    public GateLine Line => Object<GateLine>();
}

/// <summary>The line the feed holds, with an event of its own.</summary>
public sealed class GateLine : JsObject
{
    public Event Cut => Event();
}
