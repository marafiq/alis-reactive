using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnErrorRouteSetsAComponentFromItsErrorBody(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_response_error)]
    public async Task the_component_shows_the_message_the_error_body_gives()
    {
        var bench = await NewWorld();
        await bench.Seed(Seed.TheMoveEndpointAnswers(422));
        await bench.Page.GotoAsync(BenchPages.HttpResponseError);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("a-2 is closed");

        var moved = await NewWorld();
        await moved.Page.GotoAsync(BenchPages.HttpResponseError);
        await moved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(moved.Page.Locator("#shown")).ToHaveTextAsync("Answered");
        await Expect(moved.Page.Locator("#conformance").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Alpha");
    }
}
