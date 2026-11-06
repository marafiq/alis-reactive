using System.Globalization;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class DutyRoster : Component
{
    public static ComponentVendor RosterVendor { get; } = ComponentVendor.Define("roster", objectPath: "instances.0", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => RosterVendor;

    public Property<string> Lead => Property<string>();

    public Property<int> Nurses => Property<int>();
}

public static class DutyRosterHtml
{
    public static Rendered<DutyRoster> DutyRoster<TModel>(this ReactivePlan<TModel> plan, string id, int nursesRequired)
    {
        var roster = new TagBuilder("section");
        roster.Attributes["id"] = id;
        roster.Attributes["data-duty-roster"] = "";
        roster.Attributes["data-nurses-required"] = nursesRequired.ToString(CultureInfo.InvariantCulture);
        return plan.Render<DutyRoster>(id, roster);
    }
}
