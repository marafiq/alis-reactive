using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepTheServerRenderedInASlotIsReplaced(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.X12_8_0)]
    public async Task the_first_load_releases_the_step_rendered_with_the_page_and_the_page_still_hears_the_bell()
    {
        var replaced = await NewWorld();
        await replaced.Page.GotoAsync(CarePages.MoveInSteps);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(replaced.Page.Locator("#resident-step-heard")).ToHaveTextAsync("1");
        await replaced.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ann");
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Next", Exact = true }).ClickAsync();
        await Expect(replaced.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Room", Exact = true })).ToBeVisibleAsync();
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(replaced.Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(replaced.Page.Locator("#resident-step-heard")).ToHaveTextAsync("1");

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.MoveInSteps);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#resident-step-heard")).ToHaveTextAsync("1");
        await kept.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Resident", Exact = true }).FillAsync("Ann");
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(kept.Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(kept.Page.Locator("#resident-step-heard")).ToHaveTextAsync("2");
    }
}
