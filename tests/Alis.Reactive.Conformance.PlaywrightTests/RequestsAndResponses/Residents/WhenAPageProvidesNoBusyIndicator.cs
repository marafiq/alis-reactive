using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageProvidesNoBusyIndicator(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_20_0)]
    public async Task the_request_stops_before_it_sends_and_names_the_busy_indicator()
    {
        var bare = await NewWorld();
        await bare.Page.GotoAsync(CarePages.LoadWithNoBusyIndicator);
        await bare.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(bare.Page.Locator("#load-status")).ToHaveTextAsync("Loading");
        Assert.That(bare.ConsoleErrors, Has.Count.EqualTo(1));
        Assert.That(bare.ConsoleErrors.Single(), Is.EqualTo("No script provides the plugin 'busy'."));
        Assert.That(bare.SentTo(CareEndpoints.ResidentsLoaded), Is.EqualTo(0));

        var shown = await NewWorld();
        await shown.Page.GotoAsync(CarePages.LoadWithTheBusyIndicator);
        var sent = await shown.Page.RunAndWaitForRequestAsync(() => shown.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**" + CareEndpoints.ResidentsLoaded);
        Assert.That(sent.Method, Is.EqualTo("GET"));
        await Expect(shown.Page.Locator("#load-status")).ToHaveTextAsync("Loaded");
    }
}
