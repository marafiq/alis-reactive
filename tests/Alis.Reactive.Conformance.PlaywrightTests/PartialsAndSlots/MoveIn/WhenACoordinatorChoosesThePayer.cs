using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorChoosesThePayer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X115_0)]
    public async Task secondary_coverage_joins_the_primary_policy_on_the_saved_draft()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#payer-on-file")).ToHaveTextAsync("Insurance: Harbor Mutual POL-100 at 80%. No secondary coverage.");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Add secondary coverage", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary policy number", Exact = true }).FillAsync("POL-300");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary provider", Exact = true }).FillAsync("North Benefits");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary contribution percent", Exact = true }).FillAsync("20");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(world.Page.Locator("#payer-on-file")).ToHaveTextAsync("Insurance: Harbor Mutual POL-100 at 80%. North Benefits POL-300 at 20%.");

        var expired = await NewWorld();
        await expired.Page.GotoAsync(CarePages.MoveIn);
        var ends = expired.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Primary coverage ends", Exact = true }).GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true });
        await ends.PressAsync("ArrowDown");
        await ends.PressAsync("Enter");
        await expired.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Add secondary coverage", Exact = true }).CheckAsync();
        await Expect(expired.Page.Locator("#secondary-status")).ToHaveTextAsync("Coverage dates and allocation must permit secondary coverage.");
        Assert.That(expired.SentTo(CareEndpoints.MoveInSecondaryCoverage), Is.EqualTo(0));
        await expired.Page.GotoAsync(CarePages.MoveIn);
        await Expect(expired.Page.Locator("#payer-on-file")).ToHaveTextAsync("Insurance: Harbor Mutual POL-100 at 80%. No secondary coverage.");

        var unready = await NewWorld();
        await unready.Seed(MoveInChanges.TheCoordinatorMayNotAdmit("CO1"));
        await unready.Page.GotoAsync(CarePages.MoveIn);
        await unready.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Add secondary coverage", Exact = true }).CheckAsync();
        await Expect(unready.Page.Locator("#secondary-status")).ToHaveTextAsync("Coverage dates and allocation must permit secondary coverage.");
        Assert.That(unready.SentTo(CareEndpoints.MoveInSecondaryCoverage), Is.EqualTo(0));
        await unready.Page.GotoAsync(CarePages.MoveIn);
        await Expect(unready.Page.Locator("#payer-on-file")).ToHaveTextAsync("Insurance: Harbor Mutual POL-100 at 80%. No secondary coverage.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X116_0)]
    public async Task the_secondary_limit_is_exclusive_before_the_save_leaves()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Add secondary coverage", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary policy number", Exact = true }).FillAsync("POL-300");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary provider", Exact = true }).FillAsync("North Benefits");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary contribution percent", Exact = true }).FillAsync("100");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#move-in_summary").GetByText("Secondary contribution must be above 0 and below 100 percent.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        await Expect(world.Page.Locator("#payer-on-file")).ToHaveTextAsync("Insurance: Harbor Mutual POL-100 at 80%. No secondary coverage.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X117_0)]
    public async Task the_server_returns_both_failures_for_a_combined_contribution_over_one_hundred()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Add secondary coverage", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary policy number", Exact = true }).FillAsync("POL-300");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary provider", Exact = true }).FillAsync("North Benefits");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary contribution percent", Exact = true }).FillAsync("30");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#move-in_summary").GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "The two policies must cover 100 percent in total.", "Secondary exceeds the remaining primary coverage." });
        await Expect(world.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Payer", Exact = true }).GetByText("Secondary exceeds the remaining primary coverage.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(2));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        await Expect(world.Page.Locator("#payer-on-file")).ToHaveTextAsync("Insurance: Harbor Mutual POL-100 at 80%. No secondary coverage.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X118_0)]
    public async Task a_failed_secondary_lookup_keeps_the_saved_primary_policy()
    {
        var world = await NewWorld();
        await world.InstallTheClock();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInSecondaryCoverage));
        await world.HoldTheClock();
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Add secondary coverage", Exact = true }).CheckAsync();
        await world.RunTheClockUntilItSends(CareEndpoints.MoveInSecondaryCoverage, 4);
        await Expect(world.Page.Locator("#secondary-status")).ToHaveTextAsync("Secondary coverage could not be loaded.");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Secondary coverage could not be loaded.");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        await Expect(world.Page.Locator("#payer-on-file")).ToHaveTextAsync("Insurance: Harbor Mutual POL-100 at 80%. No secondary coverage.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X119_0)]
    public async Task a_late_secondary_response_cannot_join_private_funding()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        var secondary = await world.Arm(LanesGate.Respond, CareEndpoints.MoveInSecondaryCoverage);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Add secondary coverage", Exact = true }).CheckAsync();
        var secondaryHeld = await secondary.Held();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: secondary-payer");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Funding source", Exact = true }).SelectOptionAsync("Private");
        await Expect(world.Page.GetByText("Private funding", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary policy number", Exact = true })).ToHaveCountAsync(0);
        await secondaryHeld.Release();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(world.Page.GetByText("Private funding", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Secondary policy number", Exact = true })).ToHaveCountAsync(0);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(world.Page.Locator("#payer-on-file")).ToHaveTextAsync("Private funding. No secondary coverage.");
    }
}
