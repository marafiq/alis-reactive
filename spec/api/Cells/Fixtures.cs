using System.Collections.Generic;
using Alis.Reactive.Components;
using Alis.Reactive.Values;

namespace Cells;

public sealed partial class CellBoard : Component
{
    public static ComponentVendor BoardVendor { get; } = ComponentVendor.Define("cell-board", objectPath: "board", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => BoardVendor;

    public Property<string?> Note => Property<string?>();

    public WriteOnly<string?> Caption => WriteOnly<string?>();

    public ReadOnly<string?> Wing => ReadOnly<string?>();

    public ReadOnly<bool> Full => ReadOnly<bool>();

    public MethodResult<string?> Describe() => Method<string?>();

    public MethodResult<string?> DescribeLater() => MethodAsync<string?>();

    public Event<CellChanged> Changed => Event<CellChanged>();
}

public sealed class CellPlugin : Plugin
{
    public CellPlugin()
        : base("cell-plugin")
    {
    }

    public ReadOnly<string?> Wing => ReadOnly<string?>();

    public void Mark(Arg<string?> text) => Method([text]);
}

public sealed class CellChanged : JsObject
{
    public ReadOnly<string?> Wing => ReadOnly<string?>();
}

public sealed class CellView
{
    public string? Wing { get; init; }
}

public sealed class CellPage
{
    public string? Wing { get; init; }
}

public sealed class CellRequest
{
    public string? Note { get; init; }
}

public sealed class CellReply
{
    public string? Wing { get; init; }

    public IReadOnlyList<CellResident> Residents { get; init; } = [];
}

public sealed class CellResident
{
    public string Ward { get; init; } = "";

    public string? Wing { get; init; }
}
