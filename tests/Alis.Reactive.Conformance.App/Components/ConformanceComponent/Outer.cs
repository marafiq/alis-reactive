using Alis.Reactive.Declarations;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Outer : JsObject
{
    public ReadOnly<string> Name => ReadOnly<string>();

    public Property<string> Text => Property<string>();

    public Property<int> Count => Property<int>();

    public Middle Middle => LiveObject<Middle>();

    public void Act() => Method();
}
