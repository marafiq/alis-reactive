using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestIsChainedAfterASuccess(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_chained)]
    public async Task the_chained_request_sends_what_the_component_holds_after_the_first_success()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpChained);
        var chained = await bench.Page.RunAndWaitForRequestAsync(() => bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**/items**");
        Assert.That(chained.Method, Is.EqualTo("GET"));
        Assert.That(chained.Url, Does.EndWith("/items?Code=a-2"));

        var refused = await NewWorld();
        await refused.Seed(Seed.TheMoveEndpointAnswers(409));
        await refused.Page.GotoAsync(BenchPages.HttpChained);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(refused.Page.Locator("#shown")).ToHaveTextAsync("Answered");
        await Expect(refused.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-1");
    }
}
