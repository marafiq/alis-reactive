using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestIsAHead(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.M6_X5_0)]
    public async Task its_route_reads_the_status_and_the_headers_and_an_action_under_get_alone_answers_405()
    {
        var badge = await NewWorld();
        await badge.Page.GotoAsync(CarePages.PhotoBadge);
        await badge.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check photo", Exact = true }).ClickAsync();
        await Expect(badge.Page.Locator("#photo-status")).ToHaveTextAsync("200");
        await Expect(badge.Page.Locator("#photo-version")).ToHaveTextAsync("\"v7\"");
        await badge.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the other", Exact = true }).ClickAsync();
        await Expect(badge.Page.Locator("#other-photo-status")).ToHaveTextAsync("405");

        var loaded = await NewWorld();
        await loaded.Page.GotoAsync(CarePages.PhotoBadge);
        await loaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load photo", Exact = true }).ClickAsync();
        await Expect(loaded.Page.Locator("#photo-status")).ToHaveTextAsync("200");
        await Expect(loaded.Page.Locator("#photo-size")).ToHaveTextAsync("2048");
    }
}
