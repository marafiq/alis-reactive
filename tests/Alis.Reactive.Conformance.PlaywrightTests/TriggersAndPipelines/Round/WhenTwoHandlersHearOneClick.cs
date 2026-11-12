using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTwoHandlersHearOneClick(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F1.X11_10_0)]
    public async Task they_run_in_the_order_the_plan_subscribed_them_the_first_to_its_awaited_step_and_the_second_to_its_end()
    {
        var heardFirst = await NewWorld();
        await heardFirst.Page.GotoAsync(CarePages.NurseCall);
        var armed = await heardFirst.Arm(RunGate.Settle);

        await heardFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call the nurse", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(heardFirst.Page.Locator("#pager")).ToHaveTextAsync("Heard");
        await Expect(heardFirst.Page.Locator("#desk")).ToHaveTextAsync("Waiting");
        await held.Release();

        await Expect(heardFirst.Page.Locator("#desk")).ToHaveTextAsync("Acknowledged");

        var copiedFirst = await NewWorld();
        await copiedFirst.Page.GotoAsync(CarePages.NurseCallCopiedFirst);
        await copiedFirst.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call the nurse", Exact = true }).ClickAsync();
        await Expect(copiedFirst.Page.Locator("#desk")).ToHaveTextAsync("Acknowledged");
        await Expect(copiedFirst.Page.Locator("#pager")).ToHaveTextAsync("Idle");
    }
}
