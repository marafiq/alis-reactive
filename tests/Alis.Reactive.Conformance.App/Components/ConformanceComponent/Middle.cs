using Alis.Reactive.Declarations;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Middle : JsObject
{
    public ReadOnly<string> Name => ReadOnly<string>();

    public Property<string> Text => Property<string>();

    public Inner Inner => LiveObject<Inner>();
}
