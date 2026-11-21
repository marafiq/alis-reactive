using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenMarkupRaisesWhatDispatchRaises(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F2.M1_X10_0)]
    public async Task a_member_that_holds_null_reads_null_and_only_a_missing_payload_reads_nothing()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.EscortDesk);
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Late, no guide", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#guide")).ToHaveTextAsync("No guide");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Nothing known", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#guide")).ToHaveTextAsync("No guide");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "No details", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#guide")).ToHaveTextAsync("");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send again", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#guide")).ToHaveTextAsync("No guide");

        var ruth = await NewWorld();
        await ruth.Page.GotoAsync(CarePages.EscortDesk);
        await ruth.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ruth escorts", Exact = true }).ClickAsync();
        await Expect(ruth.Page.Locator("#guide")).ToHaveTextAsync("Ruth");
    }
}
