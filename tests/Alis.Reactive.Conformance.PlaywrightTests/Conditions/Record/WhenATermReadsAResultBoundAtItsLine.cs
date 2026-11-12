using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATermReadsAResultBoundAtItsLine(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_15_0)]
    public async Task a_term_and_its_branch_read_what_the_method_answered_at_its_line()
    {
        var measured = await NewWorld();
        await measured.Page.GotoAsync(CarePages.FluidBalanceMeasuredBeforeItsWrite);
        await measured.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(measured.Page.Locator("#balance-status")).ToHaveTextAsync("Low");
        await Expect(measured.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Count", Exact = true })).ToHaveTextAsync("999");
        await measured.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Restore", Exact = true }).ClickAsync();
        await Expect(measured.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Count", Exact = true })).ToHaveTextAsync("600");

        var measuredLate = await NewWorld();
        await measuredLate.Page.GotoAsync(CarePages.FluidBalanceMeasuredAfterItsWrite);
        await measuredLate.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(measuredLate.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Count", Exact = true })).ToHaveTextAsync("999");
        await Expect(measuredLate.Page.Locator("#balance-status")).ToHaveTextAsync("");
    }
}
