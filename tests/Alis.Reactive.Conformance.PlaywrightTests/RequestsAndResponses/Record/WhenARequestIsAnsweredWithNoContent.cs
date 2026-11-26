using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenARequestIsAnsweredWithNoContent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_30_0)]
    public async Task a_json_route_runs_the_catch_all_and_a_route_of_kind_none_runs_its_success()
    {
        var review = await NewWorld();
        await review.Page.GotoAsync(CarePages.CarePlanReview);
        await review.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark reviewed and show the reviewer", Exact = true }).ClickAsync();
        await Expect(review.Page.Locator("#reviewer")).ToHaveTextAsync("content");
        await review.Page.ReloadAsync();
        await Expect(review.Page.Locator("#reviews")).ToHaveTextAsync("1");
        await review.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark reviewed", Exact = true }).ClickAsync();
        await Expect(review.Page.Locator("#review-status")).ToHaveTextAsync("Reviewed");
        await review.Page.ReloadAsync();
        await Expect(review.Page.Locator("#reviews")).ToHaveTextAsync("2");
    }
}
