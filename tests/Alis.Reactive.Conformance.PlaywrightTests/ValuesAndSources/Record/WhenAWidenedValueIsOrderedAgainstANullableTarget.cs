using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAWidenedValueIsOrderedAgainstANullableTarget(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_7_1)]
    public async Task the_intake_orders_over_the_target_and_within_it_and_no_order_holds_against_a_cleared_target()
    {
        var targeted = await NewWorld();
        await targeted.Page.GotoAsync(CarePages.FluidTargetCheck);
        var intake = targeted.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Intake", Exact = true });
        var check = targeted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true });
        await intake.FillAsync("1200");
        await check.ClickAsync();
        await Expect(targeted.Page.Locator("#target-badge")).ToHaveTextAsync("Over target");
        await intake.FillAsync("500");
        await check.ClickAsync();
        await Expect(targeted.Page.Locator("#target-badge")).ToHaveTextAsync("Within target");

        var untargeted = await NewWorld();
        await untargeted.Page.GotoAsync(CarePages.FluidTargetCheck);
        var target = untargeted.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Target", Exact = true });
        var checkAgain = untargeted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check", Exact = true });
        await untargeted.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Intake", Exact = true }).FillAsync("500");
        await target.ClearAsync();
        await checkAgain.ClickAsync();
        await Expect(untargeted.Page.Locator("#target-badge")).ToHaveTextAsync("Within target");
        await target.FillAsync("400");
        await checkAgain.ClickAsync();
        await Expect(untargeted.Page.Locator("#target-badge")).ToHaveTextAsync("Over target");
    }
}
