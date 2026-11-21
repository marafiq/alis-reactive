using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASuccessRouteSetsAComponentFromItsBody(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_response_success)]
    public async Task the_component_holds_the_code_the_body_gives()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpResponseSuccess);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-2");

        var refused = await NewWorld();
        await refused.Seed(Seed.TheMoveEndpointAnswers(409));
        await refused.Page.GotoAsync(BenchPages.HttpResponseSuccess);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(refused.Page.Locator("#shown")).ToHaveTextAsync("Answered");
        await Expect(refused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-1");
    }
}
