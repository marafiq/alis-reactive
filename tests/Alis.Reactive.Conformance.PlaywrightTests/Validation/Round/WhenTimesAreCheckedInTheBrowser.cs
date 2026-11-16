using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTimesAreCheckedInTheBrowser(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F1.X16_27_0)]
    public async Task a_time_of_day_and_a_span_fail_their_orderings_before_sending_and_an_empty_start_passes()
    {
        var rota = await NewWorld();
        await rota.Page.GotoAsync(CarePages.ShiftPlan);
        await rota.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Starts", Exact = true }).FillAsync("06:30");
        await rota.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Length", Exact = true }).FillAsync("1.02:00:00");
        await rota.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(rota.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Starts", Exact = true })
            .GetByText("Shifts start at seven or later.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(rota.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Length", Exact = true })
            .GetByText("A shift lasts 23 hours at most.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(rota.Page.Locator("#saved")).ToBeEmptyAsync();
        Assert.That(rota.SentTo(CareEndpoints.ShiftPlanShifts), Is.EqualTo(0));
        await rota.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Starts", Exact = true }).ClearAsync();
        await rota.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Length", Exact = true }).FillAsync("23:00:00");
        await rota.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(rota.Page.Locator("#saved")).ToHaveTextAsync("none, 23:00:00");
        await Expect(rota.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);

        var seven = await NewWorld();
        await seven.Page.GotoAsync(CarePages.ShiftPlan);
        await seven.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Starts", Exact = true }).FillAsync("07:00");
        await seven.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Length", Exact = true }).FillAsync("23:00:00");
        await seven.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(seven.Page.Locator("#saved")).ToHaveTextAsync("07:00, 23:00:00");
        await Expect(seven.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
