using Alis.Reactive.Declarations;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Posted : JsObject
{
    public ReadOnly<string> Message => ReadOnly<string>();
}
