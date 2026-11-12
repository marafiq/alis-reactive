using Alis.Reactive.Declarations;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Change : JsObject
{
    public ReadOnly<string?> Value => ReadOnly<string?>();

    public ReadOnly<string?> PreviousValue => ReadOnly<string?>();

    public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
}
