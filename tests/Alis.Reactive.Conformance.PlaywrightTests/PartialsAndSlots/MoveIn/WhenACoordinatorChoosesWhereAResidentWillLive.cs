using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorChoosesWhereAResidentWillLive(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X10_0)]
    public async Task each_list_offers_what_the_choices_before_it_hold_and_a_choice_alone_saves_nothing()
    {
        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.MoveIn);
        await Expect(chosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose a building", "Cedar", "Willow" });
        await chosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await Expect(chosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose an apartment", "Apartment 7" });
        await chosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await Expect(chosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose a bed", "Window", "Door, held by Nora Patel" });
        await chosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(chosen.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await chosen.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED5");
        await Expect(chosen.Page.Locator("#bed-status")).ToHaveTextAsync("Door bed held by Nora Patel");
        await chosen.Page.GotoAsync(CarePages.MoveIn);
        await Expect(chosen.Page.Locator("#saved-location")).ToHaveTextAsync("C1 / B1 / A7 / BED4 revision 12");
        await Expect(chosen.Page.Locator("#saved-version")).ToHaveTextAsync("7");

        var elsewhere = await NewWorld();
        await elsewhere.Page.GotoAsync(CarePages.MoveIn);
        await elsewhere.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B2");
        await Expect(elsewhere.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose an apartment" });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X11_0)]
    public async Task an_empty_community_is_refused_beside_its_list_and_sends_no_lookup()
    {
        var emptied = await NewWorld();
        await emptied.Page.GotoAsync(CarePages.MoveIn);
        await Expect(emptied.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToBeVisibleAsync();
        await emptied.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Community", Exact = true }).SelectOptionAsync("");
        await Expect(emptied.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Community", Exact = true })
            .GetByText("Choose a community.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(emptied.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToHaveCountAsync(0);
        Assert.That(emptied.SentTo(CareEndpoints.MoveInBuildings), Is.EqualTo(1));
        await emptied.Page.GotoAsync(CarePages.MoveIn);
        await Expect(emptied.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Community", Exact = true })).ToHaveValueAsync("C1");

        var moved = await NewWorld();
        await moved.Page.GotoAsync(CarePages.MoveIn);
        await Expect(moved.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToBeVisibleAsync();
        await moved.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Community", Exact = true }).SelectOptionAsync("C2");
        await Expect(moved.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose a building", "Elm" });
        Assert.That(moved.SentTo(CareEndpoints.MoveInBuildings), Is.EqualTo(2));
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X12_0)]
    public async Task a_building_the_world_holds_closed_is_refused_by_the_server_with_its_reason()
    {
        var closed = await NewWorld();
        await closed.Seed(MoveInChanges.TheBuildingIsClosed("B1"));
        await closed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(closed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose a building", "Cedar, closed", "Willow" });
        await closed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await Expect(closed.Page.Locator("#building-status")).ToHaveTextAsync("Cedar is closed.");
        await Expect(closed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true })).ToHaveCountAsync(0);
        await closed.Page.GotoAsync(CarePages.MoveIn);
        await closed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await Expect(closed.Page.Locator("#building-status")).ToHaveTextAsync("Cedar is closed.");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.MoveIn);
        await Expect(open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose a building", "Cedar", "Willow" });
        await open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await Expect(open.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose an apartment", "Apartment 7" });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X13_0)]
    public async Task a_lookup_whose_connection_drops_says_so_and_offers_nothing_it_did_not_load()
    {
        var dropped = await NewWorld();
        await dropped.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInApartments));
        await dropped.InstallTheClock();
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(dropped.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToBeVisibleAsync();
        await dropped.HoldTheClock();
        await dropped.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await dropped.RunTheClockUntilItSends(CareEndpoints.MoveInApartments, 4);
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The request failed.");
        await Expect(dropped.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose a building", "Cedar", "Willow" });
        await Expect(dropped.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true })).ToHaveCountAsync(0);
        await dropped.Page.GotoAsync(CarePages.MoveIn);
        await Expect(dropped.Page.Locator("#saved-location")).ToHaveTextAsync("C1 / B1 / A7 / BED4 revision 12");

        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.MoveIn);
        await answered.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await Expect(answered.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose an apartment", "Apartment 7" });
        await Expect(answered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: none");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X14_0)]
    public async Task a_lookup_a_newer_choice_replaced_loads_nothing_and_the_draft_moves_only_at_its_save()
    {
        var replaced = await NewWorld();
        var armed = await replaced.Arm(LanesGate.Respond, CareEndpoints.MoveInBuildings);
        await replaced.Page.GotoAsync(CarePages.MoveIn);
        await Expect(replaced.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Move-in D100", Exact = true })).ToBeVisibleAsync();
        var harborHouse = await armed.Held();
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Community", Exact = true }).SelectOptionAsync("C2");
        await Expect(replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose a building", "Elm" });
        await harborHouse.Release();
        await Expect(replaced.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose a building", "Elm" });
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B3");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A21");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED21");
        await Expect(replaced.Page.Locator("#bed-status")).ToHaveTextAsync("Bed 21 bed available");
        await replaced.Page.GotoAsync(CarePages.MoveIn);
        await Expect(replaced.Page.Locator("#saved-location")).ToHaveTextAsync("C1 / B1 / A7 / BED4 revision 12");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Community", Exact = true }).SelectOptionAsync("C2");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B3");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A21");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED21");
        await Expect(replaced.Page.Locator("#bed-status")).ToHaveTextAsync("Bed 21 bed available");
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(replaced.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await replaced.Page.GotoAsync(CarePages.MoveIn);
        await Expect(replaced.Page.Locator("#saved-location")).ToHaveTextAsync("C2 / B3 / A21 / BED21 revision 6");
        await Expect(replaced.Page.Locator("#saved-version")).ToHaveTextAsync("8");

        var alone = await NewWorld();
        var only = await alone.Arm(LanesGate.Respond, CareEndpoints.MoveInBuildings);
        await alone.Page.GotoAsync(CarePages.MoveIn);
        await Expect(alone.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Move-in D100", Exact = true })).ToBeVisibleAsync();
        var held = await only.Held();
        await Expect(alone.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true })).ToHaveCountAsync(0);
        await held.Release();
        await Expect(alone.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).GetByRole(AriaRole.Option))
            .ToHaveTextAsync(new[] { "Choose a building", "Cedar", "Willow" });
    }
}
