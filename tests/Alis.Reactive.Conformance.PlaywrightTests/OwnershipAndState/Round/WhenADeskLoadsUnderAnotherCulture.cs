using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.OwnershipAndState;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADeskLoadsUnderAnotherCulture(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M18_F1.M18_X1_1)]
    public async Task each_strength_is_read_and_sent_under_its_own_plans_culture()
    {
        var boston = await NewWorld();
        await boston.Page.GotoAsync(CarePages.DoseStrengthsInGerman);
        await boston.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Boston desk", Exact = true }).ClickAsync();
        await boston.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Desk", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Strength", Exact = true }).FillAsync("1.5");
        await boston.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save desk", Exact = true }).ClickAsync();
        await Expect(boston.Page.Locator("#desk-strength")).ToHaveTextAsync("1.5");
        await boston.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Dose board", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Strength", Exact = true }).FillAsync("1,5");
        await boston.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(boston.Page.Locator("#board-strength")).ToHaveTextAsync("1.5");

        var german = await NewWorld();
        await german.Page.GotoAsync(CarePages.DoseStrengthsWithGermanDeskInGerman);
        await german.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open Boston desk", Exact = true }).ClickAsync();
        await german.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Desk", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Strength", Exact = true }).FillAsync("1,5");
        await german.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save desk", Exact = true }).ClickAsync();
        await Expect(german.Page.Locator("#desk-strength")).ToHaveTextAsync("1.5");
    }
}
