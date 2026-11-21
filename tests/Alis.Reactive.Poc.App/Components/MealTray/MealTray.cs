using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/MealTray/meal-tray", "MealTray")]
public sealed class MealTray : Component
{
    public static ComponentVendor TrayVendor { get; } = ComponentVendor.Define("mealtray", objectPath: "tray", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => TrayVendor;

    public Event<TrayDelivery> Delivered => Event<TrayDelivery>();

    public Property<string> Portion => Property<string>(optional: true);

    public WriteOnly<string> Slip => WriteOnly<string>(optional: true);

    public TrayCover Cover => LiveObject<TrayCover>(optional: true);

    public LiveObjects<TraySide> Sides => LiveObjects<TraySide>(optional: true);

    [SuppressMessage("Alis.Reactive.Contract", "ALIS019", Justification = "Drift on purpose: an_optional_member_of_a_required_object_the_older_tray_holds_as_null_stops_the_pipeline proves the browser stops on a server the declaration requires and the older tray holds as null.")]
    public TrayServer Server => LiveObject<TrayServer>();

    public TrayServer Relief => LiveObject<TrayServer>(nullable: true);

    public LiveObjects<TraySide> SideOrders => LiveObjects<TraySide>(nullable: true);

    public Callback<TrayDish, string> Garnish => Callback<TrayDish, string>(optional: true);

    public void Warm() => Method(optional: true);

    public MethodResult<string> LastServed() => Method<string>(optional: true);

    public void Reheat() => MethodAsync(optional: true);

    public MethodResult<string> Temperature() => MethodAsync<string>(optional: true);

    public TrayLid Lid() => ReturnedObject<TrayLid>(optional: true);

    public LiveObjects<TraySide> Extras() => ReturnedObjects<TraySide>(optional: true);

    public TrayLid SpareLid() => ReturnedObject<TrayLid>(nullable: true);

    public LiveObjects<TraySide> SpareSides() => ReturnedObjects<TraySide>(nullable: true);
}

public sealed class TrayDelivery : JsObject
{
    public ReadOnly<string> Room => ReadOnly<string>();

    public ReadOnly<string> Note => ReadOnly<string>(optional: true);
}

public sealed class TrayCover : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();
}

public sealed class TraySide : JsObject
{
    public ReadOnly<string> Name => ReadOnly<string>();
}

public sealed class TrayLid : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();
}

public sealed class TrayServer : JsObject
{
    public ReadOnly<string> Badge => ReadOnly<string>(optional: true);
}

public sealed class TrayDish : JsObject
{
    public ReadOnly<string> Name => ReadOnly<string>();
}

public static class MealTrayHtml
{
    public static Rendered<MealTray, TModel> MealTray<TModel>(this ReactivePlan<TModel> plan, string id)
        => plan.Render<MealTray>(id, Tray(id, "current"));

    public static Rendered<MealTray, TModel> OlderMealTray<TModel>(this ReactivePlan<TModel> plan, string id)
        => plan.Render<MealTray>(id, Tray(id, "older"));

    public static Rendered<MealTray, TModel> UnsetMealTray<TModel>(this ReactivePlan<TModel> plan, string id)
        => plan.Render<MealTray>(id, Tray(id, "unset"));

    private static TagBuilder Tray(string id, string build)
    {
        var tray = new TagBuilder("section");
        tray.Attributes["id"] = id;
        tray.Attributes["data-meal-tray"] = build;
        return tray;
    }
}
