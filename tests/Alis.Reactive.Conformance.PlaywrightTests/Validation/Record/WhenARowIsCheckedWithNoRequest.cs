using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenARowIsCheckedWithNoRequest(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_53_0)]
    public async Task the_rows_inputs_are_checked_under_the_names_past_its_prefix()
    {
        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.ShiftRoster);
        await cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).FillAsync("");
        await cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Choose a carer." });
        await Expect(cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).GetByText("Choose a carer.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(cleared.Page.Locator("#roster-status")).ToHaveTextAsync("");
        Assert.That(cleared.SentTo(CareEndpoints.ShiftRosterShifts), Is.EqualTo(0));

        var ready = await NewWorld();
        await ready.Page.GotoAsync(CarePages.ShiftRoster);
        await ready.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Check", Exact = true }).ClickAsync();
        await Expect(ready.Page.Locator("#roster-status")).ToHaveTextAsync("Shift 6 is ready");
    }
}
