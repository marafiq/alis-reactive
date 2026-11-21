using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Alis.Reactive.Values;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/WardSignals/ward-signals", "CallBell")]
public sealed class CallBell : Component
{
    public static ComponentVendor BellVendor { get; } = ComponentVendor.DefineCallbackProperties("callbell", objectPath: "bell");

    public override ComponentVendor Vendor => BellVendor;

    public Event<BellRing> Ring => Event<BellRing>(name: "onring");
}

public sealed class BellRing : JsObject
{
    public ReadOnly<string> Room => ReadOnly<string>();
}

[Mirrors("Components/WardSignals/ward-signals", "WardFeed")]
public sealed class WardFeed : Component
{
    public static ComponentVendor FeedVendor { get; } = ComponentVendor.DefineReturnedUnsubscriber("wardfeed", objectPath: "feed", subscribe: "subscribe");

    public override ComponentVendor Vendor => FeedVendor;

    public Event<FeedUpdate> Update => Event<FeedUpdate>();
}

public sealed class FeedUpdate : JsObject
{
    public ReadOnly<string> Message => ReadOnly<string>();
}

[Mirrors("lib.dom", "HTMLElement")]
public sealed class BedBoard : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Event<BedAssigned> Assigned => Event<BedAssigned>(name: "bed-assigned");
}

public sealed class BedAssigned : JsObject
{
    public ReadOnly<BedDetail?> Detail => ReadOnly<BedDetail?>();
}

public sealed record BedDetail(string Bed);

[Mirrors("Components/WardSignals/ward-signals", "MedicineSearch")]
public sealed class MedicineSearch : Component
{
    public static ComponentVendor SearchVendor { get; } = ComponentVendor.Define("medsearch", objectPath: "search", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => SearchVendor;

    public Event<SearchLoad> Load => Event<SearchLoad>();

    [SuppressMessage("Alis.Reactive.Contract", "ALIS018", Justification = "Drift on purpose: an_argument_at_a_position_read_on_a_component_is_refused_where_the_view_renders proves the view refuses it where it renders.")]
    public ReadOnly<string> FirstArgument => Argument<string>(0);
}

public sealed class SearchLoad : JsObject
{
    public ReadOnly<string> Query => Argument<string>(0);

    public void Answer(Arg<IReadOnlyList<string>> items) => MethodAt(1, [items]);
}

public static class WardSignalsHtml
{
    public static Rendered<CallBell, TModel> CallBell<TModel>(this ReactivePlan<TModel> plan, string id)
        => plan.Render<CallBell>(id, Host(id, "data-call-bell"));

    public static Rendered<WardFeed, TModel> WardFeed<TModel>(this ReactivePlan<TModel> plan, string id)
        => plan.Render<WardFeed>(id, Host(id, "data-ward-feed"));

    public static Rendered<BedBoard, TModel> BedBoard<TModel>(this ReactivePlan<TModel> plan, string id)
        => plan.Render<BedBoard>(id, Host(id, "data-bed-board"));

    public static Rendered<MedicineSearch, TModel> MedicineSearch<TModel>(this ReactivePlan<TModel> plan, string id)
        => plan.Render<MedicineSearch>(id, Host(id, "data-medicine-search"));

    private static TagBuilder Host(string id, string marker)
    {
        var host = new TagBuilder("section");
        host.Attributes["id"] = id;
        host.Attributes[marker] = "";
        return host;
    }
}
