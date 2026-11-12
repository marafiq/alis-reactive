using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenABranchAwaitsAMethod(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_11_0)]
    public async Task the_step_after_the_branch_runs_once_the_method_settles_and_at_once_when_the_branch_is_not_taken()
    {
        var booked = await NewWorld();
        await booked.Page.GotoAsync(CarePages.RoomBooking);
        var armed = await booked.Arm(RunGate.Settle);

        await booked.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Room free", Exact = true }).CheckAsync();
        await booked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Book", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(booked.Page.Locator("#booking-summary")).ToHaveTextAsync("");
        await held.Release();

        await Expect(booked.Page.Locator("#booking-summary")).ToHaveTextAsync("Reserved");

        var taken = await NewWorld();
        await taken.Page.GotoAsync(CarePages.RoomBooking);
        await taken.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Book", Exact = true }).ClickAsync();
        await Expect(taken.Page.Locator("#booking-summary")).ToHaveTextAsync("Open");
    }
}
