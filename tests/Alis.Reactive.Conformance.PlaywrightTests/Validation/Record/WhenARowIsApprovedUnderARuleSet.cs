using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenARowIsApprovedUnderARuleSet(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_52_0)]
    public async Task the_rows_rules_of_the_set_run_and_its_rules_of_no_set_do_not()
    {
        var tooLong = await NewWorld();
        await tooLong.Page.GotoAsync(CarePages.ShiftRoster);
        await tooLong.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Hours", Exact = true }).FillAsync("10");
        await tooLong.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Approve", Exact = true }).ClickAsync();
        await Expect(tooLong.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Hours", Exact = true }).GetByText("An approved shift is 8 hours at most.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(tooLong.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "An approved shift is 8 hours at most." });
        Assert.That(tooLong.SentTo(CareEndpoints.ShiftRosterShiftApprovals), Is.EqualTo(0));

        var approved = await NewWorld();
        await approved.Page.GotoAsync(CarePages.ShiftRoster);
        await approved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Hours", Exact = true }).FillAsync("8");
        await approved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).FillAsync("");
        await approved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Approve", Exact = true }).ClickAsync();
        await Expect(approved.Page.Locator("#roster-status")).ToHaveTextAsync("Shift 6 approved");
        await approved.Page.GotoAsync(CarePages.ShiftRoster);
        await Expect(approved.Page.Locator("#saved-shifts")).ToHaveTextAsync("Shift 6 approved");
    }
}
