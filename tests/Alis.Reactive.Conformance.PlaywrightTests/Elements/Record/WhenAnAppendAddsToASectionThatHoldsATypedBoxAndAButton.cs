using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAppendAddsToASectionThatHoldsATypedBoxAndAButton(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.M5_X5_3)]
    public async Task the_paragraph_shows_and_the_box_keeps_its_typed_code_and_the_button_its_handler()
    {
        var signed = await NewWorld();
        await signed.Page.GotoAsync(CarePages.NightChecks);
        await signed.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Night checks", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Witness code", Exact = true }).FillAsync("0200");
        await signed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add a check", Exact = true }).ClickAsync();
        await Expect(signed.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Night checks", Exact = true })
            .GetByText("Checked at 02:00", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await signed.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Night checks", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Sign off", Exact = true }).ClickAsync();
        await Expect(signed.Page.Locator("#sheet-status")).ToHaveTextAsync("Signed off");

        var refused = await NewWorld();
        await refused.Page.GotoAsync(CarePages.NightChecks);
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add a check", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Night checks", Exact = true })
            .GetByText("Checked at 02:00", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await refused.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Night checks", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Sign off", Exact = true }).ClickAsync();
        await Expect(refused.Page.Locator("#sheet-status")).ToHaveTextAsync("Code refused");
    }
}
