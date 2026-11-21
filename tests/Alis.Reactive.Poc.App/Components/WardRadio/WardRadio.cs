using Alis.Reactive.Components;
using Alis.Reactive.Values;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/WardRadio/ward-radio", "WardRadio")]
public sealed class WardRadio : Plugin
{
    public WardRadio()
        : base("ward-radio", EventListening.Methods("listen", "unlisten"))
    {
    }

    public Event<RadioBroadcast> Broadcast => Event<RadioBroadcast>();

    public void Announce(Arg<string> message) => Method([message]);
}

[Mirrors("Components/WardRadio/ward-radio", "WardRadio")]
public sealed class NightRadio : Plugin
{
    public NightRadio()
        : base("night-radio", EventListening.ReturnedUnsubscriber("listen"))
    {
    }

    public Event<RadioBroadcast> Broadcast => Event<RadioBroadcast>();

    public void Announce(Arg<string> message) => Method([message]);
}

public sealed class RadioBroadcast : JsObject
{
    public ReadOnly<string> Message => ReadOnly<string>();
}
