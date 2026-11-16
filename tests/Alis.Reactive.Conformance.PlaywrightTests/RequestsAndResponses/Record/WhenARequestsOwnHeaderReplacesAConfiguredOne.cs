using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.SiteHeader))]
[Category(Hosts.SiteHeaderName)]
public sealed class WhenARequestsOwnHeaderReplacesAConfiguredOne(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_28_0)]
    public async Task the_endpoint_reads_one_care_site_the_requests_own()
    {
        var fromSouth = await NewWorld();
        await fromSouth.Page.GotoAsync(CarePages.NotesRefresh);
        await fromSouth.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh from South", Exact = true }).ClickAsync();
        await Expect(fromSouth.Page.Locator("#notes-site")).ToHaveTextAsync("South");

        var configured = await NewWorld();
        await configured.Page.GotoAsync(CarePages.NotesRefresh);
        await configured.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh notes", Exact = true }).ClickAsync();
        await Expect(configured.Page.Locator("#notes-site")).ToHaveTextAsync("North");
    }
}
