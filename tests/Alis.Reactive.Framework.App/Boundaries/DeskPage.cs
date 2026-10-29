using Alis.Reactive.Framework.App.Components;

namespace Alis.Reactive.Framework.App.Boundaries;

/// <summary>The night desk, with its slate.</summary>
public sealed class DeskPage;

/// <summary>
/// The slate as a page declared it that thought its look-up answers at once: the slate answers a moment
/// later, with a promise.
/// </summary>
public sealed class HastySlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public TypedSource<string> Lookup(Arg<string> label) => Method<string>(label);
}

/// <summary>The slate as a page declared it that took its header for a list of text: the slate keeps an object there.</summary>
public sealed class ListedSlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public ReadOnly<IReadOnlyList<string>> Header => ReadOnly<IReadOnlyList<string>>();
}
