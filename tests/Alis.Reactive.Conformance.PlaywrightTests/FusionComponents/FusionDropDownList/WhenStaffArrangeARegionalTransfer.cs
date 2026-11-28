using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffArrangeARegionalTransfer(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F20.M21_X1840_0)]
    public async Task a_regional_referral_reaches_the_last_listed_or_searched_ward()
    {
        var paged = await NewWorld();
        await paged.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await paged.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use paged directory", Exact = true }).ClickAsync();
        await paged.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "District referrals", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(paged.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await Expect(paged.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveCountAsync(30);
        await Expect(paged.Page.Locator("#district-navigation")).ToHaveTextAsync("Search by ward number outside the current page.");
        await Expect(paged.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option).Last).ToHaveTextAsync("District ward 030");
        await paged.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("End");
        await paged.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Enter");
        await Expect(paged.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "District referrals", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("District ward 030");
        await Expect(paged.Page.Locator("#district-referral")).ToHaveTextAsync("District ward 030 will receive the transfer.");

        var full = await NewWorld();
        await full.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await full.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use paged directory", Exact = true }).ClickAsync();
        await full.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Choose another directory format", Exact = true }).ClickAsync();
        await full.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use full directory", Exact = true }).ClickAsync();
        await full.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "District referrals", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(full.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await Expect(full.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveCountAsync(150);
        await Expect(full.Page.Locator("#district-navigation")).ToHaveTextAsync("Use End for the final district ward.");
        await Expect(full.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option).Last).ToHaveTextAsync("District ward 150");
        await full.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("End");
        await full.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Enter");
        await Expect(full.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "District referrals", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("District ward 150");
        await Expect(full.Page.Locator("#district-referral")).ToHaveTextAsync("District ward 150 will receive the transfer.");

        var distant = await NewWorld();
        await distant.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await distant.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use paged directory", Exact = true }).ClickAsync();
        await distant.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "District referrals", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(distant.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await distant.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("District ward 150");
        await Expect(distant.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "District ward 150" });
        await distant.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "District ward 150", Exact = true }).ClickAsync();
        await Expect(distant.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "District referrals", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("District ward 150");
        await Expect(distant.Page.Locator("#district-referral")).ToHaveTextAsync("District ward 150 will receive the transfer.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1841_0)]
    public async Task the_empty_directory_shows_the_actual_district_closure_or_vacancy_notice()
    {
        var routine = await NewWorld();
        await routine.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await routine.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Routine closure", Exact = true }).ClickAsync();
        await routine.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Emergency contacts", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(routine.Page.GetByText("Call the district transfer desk.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var isolated = await NewWorld();
        await isolated.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await isolated.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Isolation hold", Exact = true }).ClickAsync();
        await isolated.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Emergency contacts", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(isolated.Page.GetByText("Call the district isolation coordinator.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var normal = await NewWorld();
        await normal.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await normal.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Resume normal service", Exact = true }).ClickAsync();
        await normal.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Emergency contacts", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(normal.Page.GetByText("No ward vacancies.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1842_0)]
    public async Task the_regional_policy_search_applies_the_first_stop_query_to_each_local_value_type()
    {
        var all = await NewWorld();
        await all.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await all.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Travel route policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(all.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await all.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("all");
        await Expect(all.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Lift", "Courtyard" });
        await all.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");
        await Expect(all.Page.GetByRole(AriaRole.Dialog)).ToBeHiddenAsync();
        await all.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Notice policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(all.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await all.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("all");
        await Expect(all.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "0", "30" });
        await all.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");
        await Expect(all.Page.GetByRole(AriaRole.Dialog)).ToBeHiddenAsync();
        await all.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(all.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await all.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("all");
        await Expect(all.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "false", "true" });
        await all.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");

        var priority = await NewWorld();
        await priority.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Travel route policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("priority");
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Lift" });
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");
        await Expect(priority.Page.GetByRole(AriaRole.Dialog)).ToBeHiddenAsync();
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Notice policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("priority");
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "0" });
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");
        await Expect(priority.Page.GetByRole(AriaRole.Dialog)).ToBeHiddenAsync();
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("priority");
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "false" });
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1843_0)]
    public async Task the_regional_transport_release_respects_the_position_and_all_published_choices()
    {
        var routesAppend = await NewWorld();
        await routesAppend.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await routesAppend.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Append released transport", Exact = true }).ClickAsync();
        await routesAppend.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Released travel routes", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(routesAppend.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Lift", "Ambulance", "Courtyard" });

        var noticeAppend = await NewWorld();
        await noticeAppend.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await noticeAppend.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Append released transport", Exact = true }).ClickAsync();
        await noticeAppend.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Released notice intervals", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(noticeAppend.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "15", "0", "30" });

        var escortAppend = await NewWorld();
        await escortAppend.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await escortAppend.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Append released transport", Exact = true }).ClickAsync();
        await escortAppend.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Released escort choices", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(escortAppend.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "false", "true" });

        var routesPriority = await NewWorld();
        await routesPriority.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await routesPriority.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prioritize released transport", Exact = true }).ClickAsync();
        await routesPriority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Released travel routes", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(routesPriority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Ambulance", "Courtyard", "Lift" });

        var noticePriority = await NewWorld();
        await noticePriority.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await noticePriority.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prioritize released transport", Exact = true }).ClickAsync();
        await noticePriority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Released notice intervals", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(noticePriority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "0", "30", "15" });

        var escortPriority = await NewWorld();
        await escortPriority.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await escortPriority.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prioritize released transport", Exact = true }).ClickAsync();
        await escortPriority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Released escort choices", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(escortPriority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "true", "false" });
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1844_0)]
    public async Task staff_can_find_each_standby_policy_when_a_priority_service_is_unavailable()
    {
        var standby = await NewWorld();
        await standby.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await standby.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Travel route policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(standby.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await standby.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("standby");
        await Expect(standby.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Courtyard" });
        await standby.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");
        await Expect(standby.Page.GetByRole(AriaRole.Dialog)).ToBeHiddenAsync();
        await standby.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Notice policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(standby.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await standby.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("standby");
        await Expect(standby.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "30" });
        await standby.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");
        await Expect(standby.Page.GetByRole(AriaRole.Dialog)).ToBeHiddenAsync();
        await standby.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(standby.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await standby.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("standby");
        await Expect(standby.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "true" });
        await standby.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");

        var priority = await NewWorld();
        await priority.Page.GotoAsync(CarePages.RegionalWardDirectory);
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Travel route policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("priority");
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Lift" });
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");
        await Expect(priority.Page.GetByRole(AriaRole.Dialog)).ToBeHiddenAsync();
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Notice policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("priority");
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "0" });
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");
        await Expect(priority.Page.GetByRole(AriaRole.Dialog)).ToBeHiddenAsync();
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort policy", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("priority");
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "false" });
        await priority.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressAsync("Escape");
    }
}
