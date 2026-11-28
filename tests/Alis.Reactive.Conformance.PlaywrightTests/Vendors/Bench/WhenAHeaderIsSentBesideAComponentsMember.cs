using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHeaderIsSentBesideAComponentsMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_beside_header)]
    public async Task the_endpoint_reads_the_header_beside_the_body()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpBesideHeader);
        var sent = await bench.Page.RunAndWaitForRequestAsync(() => bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**/notes");
        Assert.That(sent.PostData, Is.EqualTo("{\"code\":\"a-1\"}"));
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("X-Mode Late");

        var headerless = await NewWorld();
        await headerless.Page.GotoAsync(BenchPages.HttpBesideHeader);
        await headerless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load without the header", Exact = true }).ClickAsync();
        await Expect(headerless.Page.Locator("#shown")).ToHaveTextAsync("X-Mode none");
    }
}
