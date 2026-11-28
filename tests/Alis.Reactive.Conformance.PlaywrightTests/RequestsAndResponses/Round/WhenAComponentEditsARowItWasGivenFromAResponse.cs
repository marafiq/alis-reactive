using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentEditsARowItWasGivenFromAResponse(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_13_0)]
    public async Task a_chained_request_that_reads_the_responses_rows_sends_the_servers_rows()
    {
        var edited = await NewWorld();
        await edited.Page.GotoAsync(CarePages.SupplyRound);
        await edited.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(edited.Page.Locator("#supply-status")).ToHaveTextAsync("Ready to send");
        await edited.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sort", Exact = true }).ClickAsync();
        await edited.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Edit first", Exact = true }).ClickAsync();
        await edited.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send", Exact = true }).ClickAsync();
        await Expect(edited.Page.Locator("#supply-echo")).ToHaveTextAsync("Zinc, Aspirin; first count 7");

        var untouched = await NewWorld();
        await untouched.Page.GotoAsync(CarePages.SupplyRound);
        await untouched.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(untouched.Page.Locator("#supply-status")).ToHaveTextAsync("Ready to send");
        await untouched.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send", Exact = true }).ClickAsync();
        await Expect(untouched.Page.Locator("#supply-echo")).ToHaveTextAsync("Zinc, Aspirin; first count 7");
    }
}
