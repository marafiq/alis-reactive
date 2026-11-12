using Alis.Reactive.Declarations;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ElementSignal : JsObject
{
    public ReadOnly<string> Type => ReadOnly<string>();

    public ReadOnly<OpenedDetail?> Detail => ReadOnly<OpenedDetail?>();
}
