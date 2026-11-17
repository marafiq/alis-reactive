using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Tracing;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Tracing))]
[Category(Hosts.TracingName)]
public sealed class WhenATracedOrderIsCheckedOnTheServer(string host) : ConformanceFixture(host)
{
    private const string Render = "alis.render";
    private const string Run = Render + " > alis.run";
    private const string Send = Run + " > alis.send";
    private const string Orders = Send + " > POST " + CareEndpoints.OrderCheckOrders;

    [Test]
    [FailsFor(Promises.M26_F1.X20_9_0)]
    public async Task its_check_is_an_activity_under_the_attempt_naming_the_validator_the_rule_set_and_the_failure_count()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(CarePages.OrderCheck);
        await refused.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Strength", Exact = true }).FillAsync("900");
        await refused.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Order", Exact = true })
            .GetByText("Strength is above the ward's limit.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await refused.Page.GotoAsync(WorldPages.Collector);
        await Expect(refused.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " completed",
            Send + " completed",
            Orders + " completed 400",
            Orders + " > alis.validate validator RoundValidator rule set default failure count 1",
        ]);

        var saved = await NewWorld();
        await saved.Page.GotoAsync(CarePages.OrderCheck);
        await saved.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Strength", Exact = true }).FillAsync("90");
        await saved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(saved.Page.Locator("#saved")).ToHaveTextAsync("Order saved");
        await saved.Page.GotoAsync(WorldPages.Collector);
        await Expect(saved.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(
        [
            Render,
            Render + " > alis.boot completed",
            Run + " completed",
            Send + " completed",
            Orders + " completed 200",
            Orders + " > alis.validate validator RoundValidator rule set default failure count 0",
        ]);
    }
}
