using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAQueryValueIsSentBesideAComponentsMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_beside_query)]
    public async Task the_query_carries_both_members()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpBesideQuery);
        var sent = await bench.Page.RunAndWaitForRequestAsync(() => bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**/items**");
        Assert.That(sent.Method, Is.EqualTo("GET"));
        Assert.That(sent.Url, Does.EndWith("/items?Code=a-1&Day=2026-11-10"));

        var dayless = await NewWorld();
        await dayless.Page.GotoAsync(BenchPages.HttpGet);
        var sentWithoutTheDay = await dayless.Page.RunAndWaitForRequestAsync(() => dayless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**/items**");
        Assert.That(sentWithoutTheDay.Url, Does.EndWith("/items?Code=a-1"));
    }
}
