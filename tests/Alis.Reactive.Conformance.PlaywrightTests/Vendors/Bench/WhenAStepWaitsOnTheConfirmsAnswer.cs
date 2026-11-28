using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepWaitsOnTheConfirmsAnswer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.lane_confirm)]
    public async Task the_value_moves_only_after_the_person_answers_ok()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.LaneConfirm);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Move", Exact = true }).ClickAsync();
        var asking = bench.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Confirm", Exact = true });
        await Expect(asking.GetByText("Move to a-2?", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-1");
        await asking.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "OK", Exact = true }).ClickAsync();
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-2");

        var cancelled = await NewWorld();
        await cancelled.Page.GotoAsync(BenchPages.LaneConfirm);
        await cancelled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Move", Exact = true }).ClickAsync();
        var refusing = cancelled.Page.GetByRole(AriaRole.Dialog, new PageGetByRoleOptions { Name = "Confirm", Exact = true });
        await refusing.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Cancel", Exact = true }).ClickAsync();
        await Expect(refusing).ToBeHiddenAsync();
        await Expect(cancelled.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Value", Exact = true })).ToHaveTextAsync("a-1");
    }
}
