using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARouteValueIsSentBesideAComponentsMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_beside_route)]
    public async Task the_placeholder_is_filled_from_the_route_member_and_the_body_carries_the_components_value()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpBesideRoute);
        var sent = await bench.Page.RunAndWaitForRequestAsync(() => bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**/groups/*/notes");
        Assert.That(sent.Method, Is.EqualTo("POST"));
        Assert.That(sent.Url, Does.EndWith("/groups/Alpha/notes"));
        Assert.That(sent.PostData, Is.EqualTo("{\"code\":\"a-1\"}"));
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Alpha: a-1");

        var regrouped = await NewWorld();
        await regrouped.Seed(Seed.TheNotesGroup("Beta"));
        await regrouped.Page.GotoAsync(BenchPages.HttpBesideRoute);
        var sentAgain = await regrouped.Page.RunAndWaitForRequestAsync(() => regrouped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**/groups/*/notes");
        Assert.That(sentAgain.Url, Does.EndWith("/groups/Beta/notes"));
    }
}
