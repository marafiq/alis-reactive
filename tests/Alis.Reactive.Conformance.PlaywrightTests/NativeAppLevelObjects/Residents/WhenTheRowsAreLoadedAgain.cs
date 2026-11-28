using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheRowsAreLoadedAgain(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F3.X17_37_0)]
    public async Task a_click_on_a_reloaded_rows_button_is_heard_once()
    {
        var reloaded = await NewWorld();
        await reloaded.Page.GotoAsync(CarePages.DischargeList);
        await reloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load residents", Exact = true }).ClickAsync();
        await Expect(reloaded.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show residents-slot; hide residents-slot");
        await reloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load residents", Exact = true }).ClickAsync();
        await Expect(reloaded.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show residents-slot; hide residents-slot; show residents-slot; hide residents-slot");
        await reloaded.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Bea", Exact = true }).GetByRole(AriaRole.Button).ClickAsync();
        await Expect(reloaded.Page.Locator("#count")).ToHaveTextAsync("1");
        Assert.That(reloaded.SentTo(CareEndpoints.BeasCountedDischarge), Is.EqualTo(1));

        var once = await NewWorld();
        await once.Page.GotoAsync(CarePages.DischargeList);
        await once.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load residents", Exact = true }).ClickAsync();
        await Expect(once.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show residents-slot; hide residents-slot");
        await once.Page.GetByRole(AriaRole.Listitem, new PageGetByRoleOptions { Name = "Bea", Exact = true }).GetByRole(AriaRole.Button).ClickAsync();
        await Expect(once.Page.Locator("#count")).ToHaveTextAsync("1");
        Assert.That(once.SentTo(CareEndpoints.BeasCountedDischarge), Is.EqualTo(1));
    }
}
