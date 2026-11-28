using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionOrdersANull(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F2.X8_12_0)]
    public async Task the_branch_runs_none_of_its_steps_and_the_else_runs()
    {
        var unread = await NewWorld();
        await unread.Page.GotoAsync(CarePages.VitalSigns);
        await unread.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "No pulse", Exact = true }).ClickAsync();
        await Expect(unread.Page.Locator("#pulse-status")).ToHaveTextAsync("Unknown");
        await Expect(unread.Page.Locator("#pulse-log")).ToHaveTextAsync("");

        var read = await NewWorld();
        await read.Page.GotoAsync(CarePages.VitalSigns);
        await read.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Pulse 70", Exact = true }).ClickAsync();
        await Expect(read.Page.Locator("#pulse-status")).ToHaveTextAsync("Calm");
        await Expect(read.Page.Locator("#pulse-log")).ToHaveTextAsync("checked");
    }
}
