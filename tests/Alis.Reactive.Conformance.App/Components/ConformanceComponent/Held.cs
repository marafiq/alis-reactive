using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Held : JsObject
{
    public WriteOnly<string> WriteOnlyText => WriteOnly<string>();

    public LiveObjects<Item> Items => LiveObjects<Item>();

    public MethodResult<int> Total() => Method<int>();

    public MethodResult<int> SettleCount() => MethodAsync<int>();

    public Item ItemAt(Arg<int> position) => ReturnedObject<Item>([position]);

    public LiveObjects<Item> Matching() => ReturnedObjects<Item>();

    public Callback<Item, string> Answer => Callback<Item, string>();

    public Item Item => LiveObject<Item>();

    public void Act() => Method();
}
