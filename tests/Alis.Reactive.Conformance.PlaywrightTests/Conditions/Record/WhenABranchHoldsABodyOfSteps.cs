using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABranchHoldsABodyOfSteps(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_6_0)]
    public async Task every_step_of_the_body_runs_in_order_and_none_runs_when_its_condition_does_not_hold()
    {
        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.DischargeAccount);
        await cleared.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Balance", Exact = true }).FillAsync("0");
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        await Expect(cleared.Page.Locator("#discharge-status")).ToHaveTextAsync("Discharged");
        await Expect(cleared.Page.Locator("#discharge-log")).ToHaveTextAsync("Cleared");
        await Expect(cleared.Page.Locator("#discharge-progress")).ToHaveTextAsync("Done");

        var owing = await NewWorld();
        await owing.Page.GotoAsync(CarePages.DischargeAccount);
        await owing.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Balance", Exact = true }).FillAsync("40");
        await owing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Discharge", Exact = true }).ClickAsync();
        await Expect(owing.Page.Locator("#discharge-progress")).ToHaveTextAsync("Done");
        await Expect(owing.Page.Locator("#discharge-status")).ToHaveTextAsync("Pending");
        await Expect(owing.Page.Locator("#discharge-log")).ToHaveTextAsync("");
    }
}
