using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASplitButtonIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F44.M21_X1025_0)]
    public async Task the_button_shows_the_views_content_and_a_step_reads_it_at_its_id()
    {
        var book = await NewWorld();
        await book.Page.GotoAsync(CarePages.VisitActionsToBook);
        await Expect(book.Page.Locator("#visit-actions")).ToHaveTextAsync("Book a visit");
        await book.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show action", Exact = true }).ClickAsync();

        await Expect(book.Page.Locator("#visit-action")).ToHaveTextAsync("Book a visit");

        var cancel = await NewWorld();
        await cancel.Page.GotoAsync(CarePages.VisitActionsToCancel);
        await Expect(cancel.Page.Locator("#visit-actions")).ToHaveTextAsync("Cancel a visit");
        await cancel.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show action", Exact = true }).ClickAsync();

        await Expect(cancel.Page.Locator("#visit-action")).ToHaveTextAsync("Cancel a visit");
    }
}
