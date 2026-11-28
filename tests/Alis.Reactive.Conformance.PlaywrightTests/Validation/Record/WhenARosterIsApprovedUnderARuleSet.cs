using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenARosterIsApprovedUnderARuleSet(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_50_0)]
    public async Task each_rows_rule_of_the_set_runs_and_its_rules_of_no_set_do_not()
    {
        var tooLong = await NewWorld();
        await tooLong.Page.GotoAsync(CarePages.ShiftRoster);
        await tooLong.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Hours", Exact = true }).FillAsync("10");
        await tooLong.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Approve all", Exact = true }).ClickAsync();
        await Expect(tooLong.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true })
            .GetByText("An approved shift is 8 hours at most.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(tooLong.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Roster", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "An approved shift is 8 hours at most." });
        await Expect(tooLong.Page.Locator("#roster-status")).ToHaveTextAsync("");
        Assert.That(tooLong.SentTo(CareEndpoints.ShiftRosterApprovals), Is.EqualTo(0));

        var approved = await NewWorld();
        await approved.Page.GotoAsync(CarePages.ShiftRoster);
        await approved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Hours", Exact = true }).FillAsync("8");
        await approved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Approve all", Exact = true }).ClickAsync();
        await Expect(approved.Page.Locator("#roster-status")).ToHaveTextAsync("Approved shifts 5, 6, 7");
        await approved.Page.GotoAsync(CarePages.ShiftRoster);
        await Expect(approved.Page.Locator("#approval")).ToHaveTextAsync("Approved");
    }
}
