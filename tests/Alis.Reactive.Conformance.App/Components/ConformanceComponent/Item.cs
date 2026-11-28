using Alis.Reactive.Declarations;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Item : JsObject
{
    public ReadOnly<string> Name => ReadOnly<string>();

    public ReadOnly<bool> Active => ReadOnly<bool>();

    public Property<int> Value => Property<int>();

    public Property<int?> OptionalCount => Property<int?>(optional: true);

    public Property<int> OptionalScore => Property<int>(optional: true);

    public void Act() => Method();

    public MethodResult<bool> Check() => Method<bool>();

    public MethodResult<string> Settle() => MethodAsync<string>();
}
