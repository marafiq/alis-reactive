using Alis.Reactive.Declarations;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Inner : JsObject
{
    public ReadOnly<string> Name => ReadOnly<string>();

    public Property<string> Text => Property<string>();

    public void Act() => Method();

    public MethodResult<string> Settle() => MethodAsync<string>();
}
