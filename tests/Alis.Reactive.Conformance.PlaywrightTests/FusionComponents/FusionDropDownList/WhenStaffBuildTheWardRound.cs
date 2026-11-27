using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffBuildTheWardRound(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F20.M21_X1810_0)]
    public async Task the_day_and_night_rounds_print_their_own_destinations_and_referral_keys()
    {
        var day = await NewWorld();
        await day.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await day.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Day round", Exact = true }).ClickAsync();
        await day.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round directory", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await day.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record referral key", Exact = true }).ClickAsync();
        await Expect(day.Page.Locator("#round-key")).ToHaveTextAsync("M1");
        await day.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(day.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
        await Expect(day.Page.GetByText("North wing", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await day.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round directory", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await day.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print round sheet", Exact = true }).ClickAsync();
        await Expect(day.Page.Locator("#round-sheet")).ToHaveTextAsync("Maple to Cedar");
        await Expect(day.Page.Locator("#round-desk")).ToHaveTextAsync("First contact: Maple");

        var night = await NewWorld();
        await night.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Night round", Exact = true }).ClickAsync();
        await night.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round directory", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record referral key", Exact = true }).ClickAsync();
        await Expect(night.Page.Locator("#round-key")).ToHaveTextAsync("N3");
        await night.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(night.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Oak night", "Willow night" });
        await Expect(night.Page.GetByText("Night one", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await night.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round directory", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await night.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print round sheet", Exact = true }).ClickAsync();
        await Expect(night.Page.Locator("#round-sheet")).ToHaveTextAsync("Oak night to Willow night");
        await Expect(night.Page.Locator("#round-desk")).ToHaveTextAsync("First contact: Oak");

        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the round", Exact = true }).ClickAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print round sheet", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#round-sheet")).ToHaveTextAsync("No destinations on this round.");
        await Expect(closed.Page.Locator("#round-desk")).ToHaveTextAsync("Contact the placement office.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1811_0)]
    public async Task withdrawing_a_bed_affects_only_the_identified_ward()
    {
        var code = await NewWorld();
        await code.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await code.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Withdraw bed M1", Exact = true }).ClickAsync();
        await code.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(code.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Maple", Exact = true })).ToHaveAttributeAsync("aria-disabled", "true");
        await code.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await code.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(code.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Willow");

        var position = await NewWorld();
        await position.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await position.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Withdraw the second bed", Exact = true }).ClickAsync();
        await position.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(position.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Cedar", Exact = true })).ToHaveAttributeAsync("aria-disabled", "true");
        await position.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await position.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(position.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Willow");

        var row = await NewWorld();
        await row.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await row.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Withdraw the Oak referral", Exact = true }).ClickAsync();
        await row.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(row.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Oak", Exact = true })).ToHaveAttributeAsync("aria-disabled", "true");
        await row.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await row.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(row.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Willow");

        var unknown = await NewWorld();
        await unknown.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await unknown.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Withdraw referral R9", Exact = true }).ClickAsync();
        await unknown.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(unknown.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");
        await unknown.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await unknown.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("c");
        await Expect(unknown.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed availability", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1812_0)]
    public async Task the_referral_lookup_keeps_missing_zero_and_false_distinct()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check standard referral", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#lookup-route")).ToHaveTextAsync("Lift team.");
        await Expect(desk.Page.Locator("#lookup-interval")).ToHaveTextAsync("Immediate transfer.");
        await Expect(desk.Page.Locator("#lookup-escort")).ToHaveTextAsync("Independent travel.");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check alternate referral", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#lookup-route")).ToHaveTextAsync("Outdoor escort.");
        await Expect(desk.Page.Locator("#lookup-interval")).ToHaveTextAsync("Thirty minutes of notice.");
        await Expect(desk.Page.Locator("#lookup-escort")).ToHaveTextAsync("Reserve an escort.");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check incomplete referral", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#lookup-route")).ToHaveTextAsync("Review the route.");
        await Expect(desk.Page.Locator("#lookup-interval")).ToHaveTextAsync("Review the notice.");
        await Expect(desk.Page.Locator("#lookup-escort")).ToHaveTextAsync("Review the escort.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1813_0)]
    public async Task released_primitive_choices_follow_the_requested_round_order()
    {
        var end = await NewWorld();
        await end.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await end.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release transfer routes", Exact = true }).ClickAsync();
        await end.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Route releases", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(end.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Lift", "Courtyard", "Tunnel" });
        await end.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Route releases", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await end.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release notice intervals", Exact = true }).ClickAsync();
        await end.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interval releases", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(end.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "15", "0", "30" });
        await end.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interval releases", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await end.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release independent travel", Exact = true }).ClickAsync();
        await end.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort releases", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(end.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "true", "false" });
        await end.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort releases", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");

        var priority = await NewWorld();
        await priority.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await priority.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prioritize transfer routes", Exact = true }).ClickAsync();
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Route releases", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Courtyard", "Tunnel", "Lift" });
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Route releases", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await priority.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prioritize notice intervals", Exact = true }).ClickAsync();
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interval releases", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "0", "30", "15" });
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Interval releases", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await priority.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prioritize independent travel", Exact = true }).ClickAsync();
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort releases", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "false", "true" });
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort releases", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1814_0)]
    public async Task ward_name_searches_follow_the_active_matching_policy()
    {
        var exact = await NewWorld();
        await exact.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await exact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Exact ward search", Exact = true }).ClickAsync();
        await exact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read ward search guidance", Exact = true }).ClickAsync();
        await Expect(exact.Page.Locator("#name-guidance")).ToHaveTextAsync("Enter the initial letters with the ward’s exact accents and capital letters.");
        await exact.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward name matching", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(exact.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await exact.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("Éla");
        await Expect(exact.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Élan", "Élan House" });

        var broad = await NewWorld();
        await broad.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await broad.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Broad ward search", Exact = true }).ClickAsync();
        await broad.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read ward search guidance", Exact = true }).ClickAsync();
        await Expect(broad.Page.Locator("#name-guidance")).ToHaveTextAsync("Enter any part of the ward name. Accents and capital letters are optional.");
        await broad.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward name matching", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(broad.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await broad.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("elan");
        await Expect(broad.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Élan", "North Élan", "Élan House", "ELAN", "Elan House", "élan" });

        var ends = await NewWorld();
        await ends.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await ends.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Match name endings", Exact = true }).ClickAsync();
        await ends.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read ward search guidance", Exact = true }).ClickAsync();
        await Expect(ends.Page.Locator("#name-guidance")).ToHaveTextAsync("Enter the final letters of the ward name. Accents and capital letters are optional.");
        await ends.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward name matching", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(ends.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await ends.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("elan");
        await Expect(ends.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Élan", "North Élan", "ELAN", "élan" });
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1815_0)]
    public async Task the_round_search_applies_the_requested_query_and_referral_field_map()
    {
        var day = await NewWorld();
        await day.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await day.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round query", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(day.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await day.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("d");
        await Expect(day.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple" });

        var night = await NewWorld();
        await night.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await night.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round query", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(night.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await night.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("n");
        await Expect(night.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Willow" });

        var handover = await NewWorld();
        await handover.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await handover.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round query", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(handover.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await handover.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("h");
        await Expect(handover.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Willow night" });

        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round query", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(desk.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await desk.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("r");
        await Expect(desk.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Oak", "Willow" });

        var limited = await NewWorld();
        await limited.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await limited.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round query", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(limited.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await limited.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("s");
        await Expect(limited.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple" });

        var mapped = await NewWorld();
        await mapped.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await mapped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Round query", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(mapped.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await mapped.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("m");
        await Expect(mapped.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Willow night" });
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1816_0)]
    public async Task the_directory_supplies_route_notice_and_escort_choices()
    {
        var routes = await NewWorld();
        await routes.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await routes.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Route directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(routes.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await routes.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("r");
        await Expect(routes.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Courtyard", "Tunnel" });

        var notices = await NewWorld();
        await notices.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await notices.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Notice directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(notices.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await notices.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("n");
        await Expect(notices.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "0", "30" });

        var independent = await NewWorld();
        await independent.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await independent.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(independent.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await independent.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("i");
        await Expect(independent.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "false" });

        var assisted = await NewWorld();
        await assisted.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await assisted.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(assisted.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await assisted.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("a");
        await Expect(assisted.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "true" });

        var none = await NewWorld();
        await none.Page.GotoAsync(CarePages.WardRoundCatalogue);
        await none.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Route directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(none.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await none.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("x");
        await Expect(none.Page.GetByText("No matching transfer choices.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
