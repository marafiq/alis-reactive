using System.Collections.Generic;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Alis.Reactive.Values;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/MedicineRound/medicine-round", "MedicineRound")]
public sealed class MedicineRound : Component
{
    public static ComponentVendor RoundVendor { get; } = ComponentVendor.Define("medicine-round", objectPath: "round", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => RoundVendor;

    public Event<RoundStart> Started => Event<RoundStart>();

    public Property<int?> Due => Property<int?>();

    public Property<int?> Missed => Property<int?>();

    public Property<IReadOnlyList<string>> Listed => Property<IReadOnlyList<string>>();

    public Property<string?> Resident => Property<string?>();
}

public sealed class RoundStart : JsObject
{
    public WriteOnly<string> Note => WriteOnly<string>();

    public Callback<RoundDose, string> Signature => Callback<RoundDose, string>();

    public LiveObjects<RoundDose> Doses => LiveObjects<RoundDose>();

    public RoundTrolley Trolley => LiveObject<RoundTrolley>();

    public MethodResult<int> DueCount() => Method<int>();

    public MethodResult<RoundChart> Chart() => MethodAsync<RoundChart>();

    public RoundDose Dose(Arg<int> position) => ReturnedObject<RoundDose>([position]);

    public LiveObjects<RoundDose> Missed() => ReturnedObjects<RoundDose>();
}

public sealed class RoundDose : JsObject
{
    public ReadOnly<string> Drug => ReadOnly<string>();

    public ReadOnly<bool> Due => ReadOnly<bool>();

    public ReadOnly<bool> Given => ReadOnly<bool>();

    public void Give() => Method();
}

public sealed class RoundTrolley : JsObject
{
    public WriteOnly<string> Label => WriteOnly<string>();

    public Callback<RoundDose, string> Signature => Callback<RoundDose, string>();
}

public sealed record RoundChart(string Resident);

public static class MedicineRoundHtml
{
    public static Rendered<MedicineRound, TModel> MedicineRound<TModel>(this ReactivePlan<TModel> plan, string id)
    {
        var round = new TagBuilder("section");
        round.Attributes["id"] = id;
        round.Attributes["data-medicine-round"] = "";
        return plan.Render<MedicineRound>(id, round);
    }
}
