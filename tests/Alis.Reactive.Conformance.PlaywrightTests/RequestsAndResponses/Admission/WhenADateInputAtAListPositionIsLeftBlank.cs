using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADateInputAtAListPositionIsLeftBlank(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_43_0)]
    public async Task the_containers_read_and_its_request_stop_since_leaving_it_out_would_end_the_list_there()
    {
        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.VisitDates);
        await cleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await cleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Second visit", Exact = true }).FillAsync("");
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save visits", Exact = true }).ClickAsync();
        await Expect(cleared.Page.Locator("#save-status")).ToHaveTextAsync("Saving the visits");
        await Expect(cleared.Page.Locator("#visits-resident")).ToBeEmptyAsync();
        await Expect(cleared.Page.Locator("#saved-visits")).ToBeEmptyAsync();
        Assert.That(cleared.ConsoleErrors, Has.Count.EqualTo(1));

        var dated = await NewWorld();
        await dated.Page.GotoAsync(CarePages.VisitDates);
        await dated.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        var sent = await dated.Page.RunAndWaitForRequestAsync(() => dated.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save visits", Exact = true }).ClickAsync(), "**/visit-dates/visits");
        Assert.That(sent.PostData, Is.EqualTo("Dates%5B0%5D=2026-11-30T00%3A00%3A00&Dates%5B1%5D=2026-12-01T00%3A00%3A00&Resident=Ada"));
        await Expect(dated.Page.Locator("#visits-resident")).ToHaveTextAsync("Ada");
        await Expect(dated.Page.Locator("#saved-visits")).ToHaveTextAsync("Ada: visits on 2026-11-30, 2026-12-01");
    }
}
