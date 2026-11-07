using System;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class FluidBalance : Component
{
    public static ComponentVendor BalanceVendor { get; } = ComponentVendor.Define("fluid", objectPath: "balance", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => BalanceVendor;

    public Property<int> Intake => Property<int>();

    public Property<int?> Target => Property<int?>();

    public ReadOnly<int?> Weight => ReadOnly<int?>();

    public ReadOnly<bool> Rechecking => ReadOnly<bool>();

    public ReadOnly<DateTime> RecordedAt => ReadOnly<DateTime>();

    public void Recheck() => MethodAsync();

    public MethodResult<int> Measure() => Method<int>();

    public MethodResult<int> Recount() => MethodAsync<int>();
}

public static class FluidBalanceHtml
{
    public static Rendered<FluidBalance> FluidBalance<TModel>(this ReactivePlan<TModel> plan, string id)
    {
        var balance = new TagBuilder("section");
        balance.Attributes["id"] = id;
        balance.Attributes["data-fluid-balance"] = "";
        return plan.Render<FluidBalance>(id, balance);
    }
}
