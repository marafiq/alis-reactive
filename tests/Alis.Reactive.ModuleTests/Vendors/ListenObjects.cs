using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.Vendors;

public sealed class ListenHost : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;
    public Event<ListenPayload> Changed => Event<ListenPayload>();
    public Event<ListenPeer> PeerChanged => Event<ListenPeer>();
    public Event<NoElementPeer> Unlocated => Event<NoElementPeer>();
    public Event<ListenFeed> FeedChanged => Event<ListenFeed>();
}

public sealed class ListenPayload : JsObject
{
    public ListenFeed Feed => LiveObject<ListenFeed>();
    public ListenFeed OptionalFeed => LiveObject<ListenFeed>(optional: true);
    public ListenFeed NullableFeed => LiveObject<ListenFeed>(nullable: true);
    public ListenPeer Peer => LiveObject<ListenPeer>();
    public NoElementPeer Unlocated => LiveObject<NoElementPeer>();
    public Property<string> Text => Property<string>();
    public WriteOnly<string> Secret => WriteOnly<string>();
    public Callback<Search, string> Format => Callback<Search, string>();
    public ReadOnly<string> Position => Argument<string>(2);
    public void Wait() => MethodAsync();
    public MethodResult<string> ReadText() => Method<string>();
    public ListenPeer ReturnedPeer() => ReturnedObject<ListenPeer>();
    public NoElementPeer ReturnedUnlocated() => ReturnedObject<NoElementPeer>();
}

public sealed class ListenFeed : EventedObject
{
    protected override EventListening Listening => EventListening.Methods("on", "off");
    public Event<ListenPayload> Updated => Event<ListenPayload>(name: "feed-updated");
    public Event Quiet => Event();
    public Property<string> Text => Property<string>();
    public MethodResult<string> ReadText() => Method<string>();
}

public sealed class ListenPeer : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Define("peer", "instances.0", "hear", "forget", elementPath: "host.0");
    public Event<ListenPayload> Updated => Event<ListenPayload>();
    public Property<string> Text => Property<string>();
}

public sealed class NoElementPeer : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Define("unlocated", "instances.0", "hear", "forget");
    public Event<ListenPayload> Updated => Event<ListenPayload>();
}
