using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffArrangeTheWardStation(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F20.M21_X1848_0)]
    public async Task arranging_a_station_changes_the_handover_sheet_without_changing_a_shared_roster()
    {
        var wide = await NewWorld();
        await wide.Page.GotoAsync(CarePages.WardStationWorkboard);
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Arrange my station", Exact = true }).ClickAsync();
        await wide.Page.GetByRole(AriaRole.Combobox).First.PressAsync("Alt+ArrowDown");
        var wideHandle = wide.Page.Locator("#Alis_Reactive_Conformance_App_Care_WardStationWorkboard__Ward_resize-popup");
        await Expect(wideHandle).ToBeVisibleAsync();
        await Expect(wide.Page.Locator("#board-help")).ToHaveTextAsync("Extend the roster into the receiving column to include arrival instructions.");
        await wideHandle.DragToAsync(wide.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Receiving-team instructions", Exact = true }));
        await Expect(wide.Page.GetByText("Call the porter before an evening transfer.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(wide.Page.GetByText("Maple accepts arrivals at the charge desk. Cedar uses the south entrance.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(wide.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option).Last).ToHaveTextAsync("Linden");
        await wide.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Linden", Exact = true }).ClickAsync();
        await Expect(wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare shift handover", Exact = true })).ToBeVisibleAsync();
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare shift handover", Exact = true }).ClickAsync();
        await Expect(wide.Page.Locator("#station-handover")).ToHaveTextAsync("Linden, extension 614. Meet the receiving nurse at the east entrance.");
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show transport reminders", Exact = true }).ClickAsync();
        await Expect(wide.Page.GetByText("Call the porter before an evening transfer.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var compact = await NewWorld();
        await compact.Page.GotoAsync(CarePages.WardStationWorkboard);
        await compact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Arrange my station", Exact = true }).ClickAsync();
        await compact.Page.GetByRole(AriaRole.Combobox).First.PressAsync("Alt+ArrowDown");
        var compactHandle = compact.Page.Locator("#Alis_Reactive_Conformance_App_Care_WardStationWorkboard__Ward_resize-popup");
        await Expect(compactHandle).ToBeVisibleAsync();
        await compactHandle.DragToAsync(compact.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Ward extensions", Exact = true }));
        await compact.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Maple", Exact = true }).ClickAsync();
        await compact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare shift handover", Exact = true }).ClickAsync();
        await Expect(compact.Page.Locator("#station-handover")).ToHaveTextAsync("Maple, extension 214.");
        await Expect(compact.Page.GetByText("Maple accepts arrivals at the charge desk. Cedar uses the south entrance.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var shared = await NewWorld();
        await shared.Page.GotoAsync(CarePages.WardStationWorkboard);
        await shared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Arrange my station", Exact = true }).ClickAsync();
        await shared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use shared station", Exact = true }).ClickAsync();
        await shared.Page.GetByRole(AriaRole.Combobox).First.PressAsync("Alt+ArrowDown");
        await Expect(shared.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Maple", Exact = true })).ToBeVisibleAsync();
        await Expect(shared.Page.Locator("#Alis_Reactive_Conformance_App_Care_WardStationWorkboard__Ward_resize-popup")).ToBeHiddenAsync();
        await Expect(shared.Page.Locator("#board-help")).ToHaveTextAsync("The shared station keeps its extension-only roster.");
        await shared.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Maple", Exact = true }).ClickAsync();
        await Expect(shared.Page.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");
        await shared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare shift handover", Exact = true }).ClickAsync();
        await Expect(shared.Page.Locator("#station-handover")).ToHaveTextAsync("Maple, extension 214.");
        await Expect(shared.Page.GetByText("Call the porter before an evening transfer.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1849_0)]
    public async Task the_handover_copy_uses_the_actual_roster_width_and_height()
    {
        var wide = await NewWorld();
        await wide.Page.GotoAsync(CarePages.WardStationWorkboard);
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use shared station", Exact = true }).ClickAsync();
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use tall ward roster", Exact = true }).ClickAsync();
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use short ward roster", Exact = true }).ClickAsync();
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use full handover width", Exact = true }).ClickAsync();
        await wide.Page.GetByRole(AriaRole.Combobox).First.PressAsync("Alt+ArrowDown");
        await Expect(wide.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option).Last).ToHaveTextAsync("Linden");
        await wide.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Linden", Exact = true }).ClickAsync();
        await Expect(wide.Page.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Linden");
        await Expect(wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare shift handover", Exact = true })).ToBeVisibleAsync();
        await wide.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare shift handover", Exact = true }).ClickAsync();
        await Expect(wide.Page.Locator("#station-handover")).ToHaveTextAsync("Linden, extension 614. Meet the receiving nurse at the east entrance.");
        await Expect(wide.Page.GetByText("After hours, contact the admission supervisor on pager 22.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var pocket = await NewWorld();
        await pocket.Page.GotoAsync(CarePages.WardStationWorkboard);
        await pocket.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use shared station", Exact = true }).ClickAsync();
        await pocket.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use full handover width", Exact = true }).ClickAsync();
        await pocket.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use pocket handover width", Exact = true }).ClickAsync();
        await pocket.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use tall ward roster", Exact = true }).ClickAsync();
        await pocket.Page.GetByRole(AriaRole.Combobox).First.PressAsync("Alt+ArrowDown");
        await pocket.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Cedar", Exact = true }).ClickAsync();
        await Expect(pocket.Page.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");
        await pocket.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare shift handover", Exact = true }).ClickAsync();
        await Expect(pocket.Page.Locator("#station-handover")).ToHaveTextAsync("Cedar, extension 319.");
        await Expect(pocket.Page.GetByText("After hours, contact the admission supervisor on pager 22.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1850_0)]
    public async Task the_foreground_action_assigns_the_ward_or_opens_the_urgent_review()
    {
        var allocation = await NewWorld();
        await allocation.Page.GotoAsync(CarePages.WardStationWorkboard);
        await allocation.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review urgent arrival first", Exact = true }).ClickAsync();
        await allocation.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Finish ward allocation first", Exact = true }).ClickAsync();
        await allocation.Page.GetByRole(AriaRole.Combobox).First.PressAsync("Alt+ArrowDown");
        await Expect(allocation.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        var firstWard = allocation.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Maple", Exact = true });
        await Expect(firstWard).ToBeVisibleAsync();
        await firstWard.ClickAsync(new LocatorClickOptions { Force = true });
        await Expect(allocation.Page.Locator("#station-assignment")).ToHaveTextAsync("Maple is assigned to the receiving team.");
        await Expect(allocation.Page.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");
        await Expect(allocation.Page.Locator("#station-priority")).ToHaveTextAsync("Choose the ward before reviewing the urgent arrival.");

        var urgent = await NewWorld();
        await urgent.Page.GotoAsync(CarePages.WardStationWorkboard);
        await urgent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Finish ward allocation first", Exact = true }).ClickAsync();
        await urgent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review urgent arrival first", Exact = true }).ClickAsync();
        await urgent.Page.GetByRole(AriaRole.Combobox).First.PressAsync("Alt+ArrowDown");
        await Expect(urgent.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await Expect(urgent.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Maple", Exact = true })).ToBeVisibleAsync();
        await urgent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review urgent arrival", Exact = true }).ClickAsync(new LocatorClickOptions { Force = true });
        await Expect(urgent.Page.Locator("#station-assignment")).ToHaveTextAsync("The duty nurse must review the urgent arrival.");
        await Expect(urgent.Page.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("");
        await Expect(urgent.Page.Locator("#station-priority")).ToHaveTextAsync("Review the urgent arrival before choosing a ward.");
    }
}
