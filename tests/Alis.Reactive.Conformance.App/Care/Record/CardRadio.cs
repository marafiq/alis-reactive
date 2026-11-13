using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CardRadio : Plugin
{
    public CardRadio()
        : base("card-radio", EventListening.Methods("on", "off"))
    {
    }

    public Event Call => Event();

    public void Transmit() => Method();
}
