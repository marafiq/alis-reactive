using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowsReviewButtonIsClicked(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F3.X15_4_0)]
    public async Task the_handler_reads_the_id_of_that_row()
    {
        var second = await NewWorld();
        await second.Page.GotoAsync(CarePages.WingReview);
        await second.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Review", Exact = true }).Nth(1).ClickAsync();

        await Expect(second.Page.Locator("#reviewing")).ToHaveTextAsync("R-102");

        var first = await NewWorld();
        await first.Page.GotoAsync(CarePages.WingReview);
        await first.Page.GetByRole(AriaRole.Grid).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Review", Exact = true }).First.ClickAsync();

        await Expect(first.Page.Locator("#reviewing")).ToHaveTextAsync("R-101");
    }
}
