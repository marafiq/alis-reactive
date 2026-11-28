using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotReleasesAComponentItsCodeDestroyed(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F2.X12_7_0)]
    public async Task the_release_leaves_it_alone_and_the_next_panel_hears_its_raise()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.PanelReopening);
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the panel", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed alarm panel", Exact = true })).ToBeVisibleAsync();
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Destroy", Exact = true }).ClickAsync();
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the panel", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed alarm panel", Exact = true })).ToBeHiddenAsync();
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the panel", Exact = true }).ClickAsync();
        await Expect(record.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed alarm panel", Exact = true })).ToBeVisibleAsync();
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#raised-heard")).ToHaveTextAsync("raised");
        Assert.That(record.ConsoleErrors, Is.Empty);

        var first = await NewWorld();
        await first.Page.GotoAsync(CarePages.PanelReopening);
        await first.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the panel", Exact = true }).ClickAsync();
        await Expect(first.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Bed alarm panel", Exact = true })).ToBeVisibleAsync();
        await first.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(first.Page.Locator("#raised-heard")).ToHaveTextAsync("raised");
    }
}
