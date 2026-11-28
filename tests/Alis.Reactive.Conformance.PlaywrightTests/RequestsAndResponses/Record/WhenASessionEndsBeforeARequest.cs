using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASessionEndsBeforeARequest(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_2_0)]
    public async Task an_api_endpoints_401_runs_the_route_for_401_and_a_redirect_to_the_sign_in_page_runs_the_catch_all()
    {
        var signedOut = await NewWorld();
        await signedOut.Seed(Seed.TheSessionSignedOut());
        await signedOut.Page.GotoAsync(CarePages.SessionNotes);
        await signedOut.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh notes", Exact = true }).ClickAsync();
        await Expect(signedOut.Page.Locator("#notes")).ToHaveTextAsync("Session expired");
        await signedOut.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh from the chart", Exact = true }).ClickAsync();
        await Expect(signedOut.Page.Locator("#chart-notes")).ToHaveTextAsync("Request failed");

        var signedIn = await NewWorld();
        await signedIn.Page.GotoAsync(CarePages.SessionNotes);
        await signedIn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh notes", Exact = true }).ClickAsync();
        await Expect(signedIn.Page.Locator("#notes")).ToHaveTextAsync("Ate well at lunch");
        await signedIn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh from the chart", Exact = true }).ClickAsync();
        await Expect(signedIn.Page.Locator("#chart-notes")).ToHaveTextAsync("Ate well at lunch");
    }
}
