using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotsContentRendersAnIdThePageHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F2.X14_7_0)]
    public async Task the_slots_content_runs_none_of_its_handlers_and_the_console_holds_one_error()
    {
        var reception = await NewWorld();
        await reception.Page.GotoAsync(CarePages.ReceptionWithANameSection);
        await reception.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await reception.Page.Locator("#section").GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await reception.Page.Locator("#section").GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).PressAsync("Tab");
        await Expect(reception.Page.Locator("#section-note")).ToHaveTextAsync("");
        Assert.That(reception.ConsoleErrors, Has.Count.EqualTo(1));

        var phoneSection = await NewWorld();
        await phoneSection.Page.GotoAsync(CarePages.ReceptionWithAPhoneSection);
        await phoneSection.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await phoneSection.Page.Locator("#section").GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("0101");
        await phoneSection.Page.Locator("#section").GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).PressAsync("Tab");
        await Expect(phoneSection.Page.Locator("#section-note")).ToHaveTextAsync("0101");
    }
}
