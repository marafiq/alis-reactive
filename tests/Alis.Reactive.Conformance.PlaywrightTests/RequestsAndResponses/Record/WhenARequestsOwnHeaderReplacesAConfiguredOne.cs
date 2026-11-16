using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestsOwnHeaderReplacesAConfiguredOne(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_28_0)]
    public async Task the_endpoint_reads_one_x_requested_with_the_requests_own()
    {
        var byFetch = await NewWorld();
        await byFetch.Page.GotoAsync(CarePages.NotesRefresh);
        await byFetch.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh by fetch", Exact = true }).ClickAsync();
        await Expect(byFetch.Page.Locator("#notes-echo")).ToHaveTextAsync("Fetch");

        var configured = await NewWorld();
        await configured.Page.GotoAsync(CarePages.NotesRefresh);
        await configured.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh notes", Exact = true }).ClickAsync();
        await Expect(configured.Page.Locator("#notes-echo")).ToHaveTextAsync("XMLHttpRequest");
    }
}
