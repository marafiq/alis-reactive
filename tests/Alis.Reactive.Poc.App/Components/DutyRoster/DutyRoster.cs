using System.Globalization;
using System.Text.Json.Serialization;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class DutyRoster : Component
{
    public static ComponentVendor RosterVendor { get; } = ComponentVendor.Define("roster", objectPath: "instances.0", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => RosterVendor;

    public RosterShift Shift => LiveObject<RosterShift>();

    public ReadOnly<OnCallNurse> OnCall => ReadOnly<OnCallNurse>();

    public Property<string> Lead => Property<string>();

    public Property<int> Nurses => Property<int>();
}

public sealed class RosterShift : JsObject
{
    public Property<string> Label => Property<string>();
}

public abstract record OnCallStaff
{
    public abstract string Role { get; init; }

    public abstract string Station { get; init; }
}

public sealed record OnCallNurse(string Name, [property: JsonPropertyName("pager_line")] PagerLine Pager, [property: JsonIgnore] string Ward) : OnCallStaff
{
    public override string Role { get; init; } = "";

    [JsonIgnore]
    public override string Station { get; init; } = "";
}

public sealed record PagerLine(string Number);

public static class DutyRosterHtml
{
    public static Rendered<DutyRoster> DutyRoster<TModel>(this ReactivePlan<TModel> plan, string id, int nursesRequired, string shift, string onCallName, string onCallRole, string? onCallPager)
        => plan.Render<DutyRoster>(id, Roster(id, "1", nursesRequired, shift, onCallName, onCallRole, onCallPager));

    public static Rendered<DutyRoster> DutyRosterVersion2<TModel>(this ReactivePlan<TModel> plan, string id)
        => plan.Render<DutyRoster>(id, Roster(id, "2", 0, "", "", "", null));

    private static TagBuilder Roster(string id, string version, int nursesRequired, string shift, string onCallName, string onCallRole, string? onCallPager)
    {
        var roster = new TagBuilder("section");
        roster.Attributes["id"] = id;
        roster.Attributes["data-duty-roster"] = version;
        roster.Attributes["data-nurses-required"] = nursesRequired.ToString(CultureInfo.InvariantCulture);
        roster.Attributes["data-shift"] = shift;
        roster.Attributes["data-on-call-name"] = onCallName;
        roster.Attributes["data-on-call-role"] = onCallRole;
        if (onCallPager is not null) roster.Attributes["data-on-call-pager"] = onCallPager;
        return roster;
    }
}
