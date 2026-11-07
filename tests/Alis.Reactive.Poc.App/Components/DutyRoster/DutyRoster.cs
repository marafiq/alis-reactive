using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/DutyRoster/duty-roster", "DutyRoster")]
public sealed class DutyRoster : Component
{
    public static ComponentVendor RosterVendor { get; } = ComponentVendor.Define("roster", objectPath: "instances.0", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => RosterVendor;

    public RosterShift Shift => LiveObject<RosterShift>();

    [SuppressMessage("Alis.Reactive.Contract", "ALIS025", Justification = "Drift on purpose: a_list_where_the_data_declares_an_object_stops_a_read_of_a_member_of_it_and_the_steps_after_it proves the browser stops on the pager line a roster hands as a list.")]
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
    [SuppressMessage("Alis.Reactive.Contract", "ALIS025", Justification = "Drift on purpose: a_read_of_an_override_the_apps_json_options_ignore_is_refused_with_its_type proves the view refuses a read of an override its JSON options leave unwritten.")]
    public override string Station { get; init; } = "";

    public string Callback { get; init; } = "switchboard";
}

public sealed record PagerLine(string Number);

public static class DutyRosterHtml
{
    public static Rendered<DutyRoster, TModel> DutyRoster<TModel>(this ReactivePlan<TModel> plan, string id, int nursesRequired, string shift, string onCallName, string onCallRole, string? onCallPager)
        => plan.Render<DutyRoster>(id, Roster(id, "1", nursesRequired, shift, onCallName, onCallRole, onCallPager));

    public static Rendered<DutyRoster, TModel> DutyRosterAwaitingCallback<TModel>(this ReactivePlan<TModel> plan, string id, string onCallName)
    {
        var roster = Roster(id, "1", 0, "", onCallName, "", null);
        roster.Attributes["data-on-call-callback"] = "awaited";
        return plan.Render<DutyRoster>(id, roster);
    }

    public static Rendered<DutyRoster, TModel> DutyRosterWithPagerLines<TModel>(this ReactivePlan<TModel> plan, string id, string onCallName, string onCallPager)
    {
        var roster = Roster(id, "1", 0, "", onCallName, "", null);
        roster.Attributes["data-on-call-pager-lines"] = onCallPager;
        return plan.Render<DutyRoster>(id, roster);
    }

    public static Rendered<DutyRoster, TModel> DutyRosterVersion2<TModel>(this ReactivePlan<TModel> plan, string id)
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
