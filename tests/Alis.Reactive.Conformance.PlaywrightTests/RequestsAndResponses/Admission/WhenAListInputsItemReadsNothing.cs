using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListInputsItemReadsNothing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_40_0)]
    public async Task the_containers_read_and_its_request_stop_where_the_item_admits_no_null()
    {
        var bankHoliday = await NewWorld();
        await bankHoliday.Page.GotoAsync(CarePages.VisitPlan);
        await bankHoliday.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await bankHoliday.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Mon", Exact = true }).CheckAsync();
        await bankHoliday.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Bank holiday", Exact = true }).CheckAsync();
        await bankHoliday.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save plan", Exact = true }).ClickAsync();
        await Expect(bankHoliday.Page.Locator("#save-status")).ToHaveTextAsync("Saving the visit plan");
        await Expect(bankHoliday.Page.Locator("#plan-resident")).ToBeEmptyAsync();
        await Expect(bankHoliday.Page.Locator("#saved-plan")).ToBeEmptyAsync();
        Assert.That(bankHoliday.ConsoleErrors, Has.Count.EqualTo(1));

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.OpenVisitPlan);
        await open.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ada");
        await open.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Mon", Exact = true }).CheckAsync();
        await open.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Bank holiday", Exact = true }).CheckAsync();
        var sent = await open.Page.RunAndWaitForRequestAsync(() => open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save plan", Exact = true }).ClickAsync(), "**/open-visit-plan/plans");
        Assert.That(sent.PostData, Is.EqualTo("Resident=Ada&VisitDays=Mon&VisitDays="));
        await Expect(open.Page.Locator("#plan-resident")).ToHaveTextAsync("Ada");
        await Expect(open.Page.Locator("#saved-plan")).ToHaveTextAsync("Ada: 2 visit days");
    }
}
