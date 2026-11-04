using Alis.Reactive.Framework.App.Components;

namespace Alis.Reactive.Framework.App.Boundaries;

public sealed class DeskPage;

public sealed class HastySlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public TypedSource<string> Lookup(Arg<string> label) => Method<string>(label);
}

public sealed class ListedSlate : Component
{
    public override ComponentVendor Vendor => Slate.SlateVendor;

    public ReadOnly<IReadOnlyList<string>> Header => ReadOnly<IReadOnlyList<string>>();
}
