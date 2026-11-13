using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoCardsBindOneMemberName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F3.X13_1_0)]
    public async Task each_cards_save_sends_its_own_value()
    {
        var both = await NewWorld();
        await both.Page.GotoAsync(CarePages.ChartCards);
        await both.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Fluids", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Millilitres", Exact = true }).FillAsync("250");
        await both.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Weight", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Kilograms", Exact = true }).FillAsync("70");
        await both.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Fluids", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await both.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Weight", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(both.Page.Locator("#fluids-charted")).ToHaveTextAsync("250");
        await Expect(both.Page.Locator("#weight-charted")).ToHaveTextAsync("70");

        var fluidsAlone = await NewWorld();
        await fluidsAlone.Page.GotoAsync(CarePages.ChartCards);
        await fluidsAlone.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Fluids", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Millilitres", Exact = true }).FillAsync("250");
        await fluidsAlone.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Fluids", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await fluidsAlone.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Weight", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(fluidsAlone.Page.Locator("#weight-charted")).ToHaveTextAsync("none");
        await Expect(fluidsAlone.Page.Locator("#fluids-charted")).ToHaveTextAsync("250");
    }
}
