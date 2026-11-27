using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffAmendABookedTransfer(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F20.M21_X1845_0)]
    public async Task a_route_amendment_keeps_or_releases_the_original_transport_booking()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.TransferAmendment);
        var booking = world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Route amendment", Exact = true }).GetByRole(AriaRole.Combobox);
        await world.Page.GetByLabel("Keep confirmed transfer safeguards").CheckAsync();
        await booking.First.PressAsync("Home");
        await Expect(booking.Last).ToHaveValueAsync("Lift");
        await Expect(world.Page.Locator("#route-approval")).ToHaveTextAsync("Travel route accepted.");
        await Expect(world.Page.Locator("#route-release")).ToHaveTextAsync("There is no prior travel route to cancel.");

        await booking.First.PressAsync("End");
        await Expect(world.Page.Locator("#route-approval")).ToHaveTextAsync("Keep the lift booking until an outdoor escort is available.");
        await Expect(booking.Last).ToHaveValueAsync("Lift");
        await Expect(world.Page.Locator("#route-release")).ToHaveTextAsync("There is no prior travel route to cancel.");

        await world.Page.GetByLabel("Keep confirmed transfer safeguards").UncheckAsync();
        await booking.First.PressAsync("End");
        await Expect(booking.Last).ToHaveValueAsync("Courtyard");
        await Expect(world.Page.Locator("#route-approval")).ToHaveTextAsync("Travel route accepted.");
        await Expect(world.Page.Locator("#route-release")).ToHaveTextAsync("Cancel the lift booking.");

        await booking.First.PressAsync("Home");
        await Expect(booking.Last).ToHaveValueAsync("Lift");
        await Expect(world.Page.Locator("#route-release")).ToHaveTextAsync("Release the outdoor escort.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1846_0)]
    public async Task a_notice_amendment_distinguishes_immediate_departure_from_prior_notice()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.TransferAmendment);
        var booking = world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Notice amendment", Exact = true }).GetByRole(AriaRole.Combobox);
        await world.Page.GetByLabel("Keep confirmed transfer safeguards").CheckAsync();
        await booking.First.PressAsync("End");
        await Expect(booking.Last).ToHaveValueAsync("30");
        await Expect(world.Page.Locator("#notice-approval")).ToHaveTextAsync("Notice interval accepted.");
        await Expect(world.Page.Locator("#notice-release")).ToHaveTextAsync("There is no earlier notice to withdraw.");

        await booking.First.PressAsync("Home");
        await Expect(world.Page.Locator("#notice-approval")).ToHaveTextAsync("Keep thirty minutes while the advance-notice requirement is active.");
        await Expect(booking.Last).ToHaveValueAsync("30");
        await Expect(world.Page.Locator("#notice-release")).ToHaveTextAsync("There is no earlier notice to withdraw.");

        await world.Page.GetByLabel("Keep confirmed transfer safeguards").UncheckAsync();
        await booking.First.PressAsync("Home");
        await Expect(booking.Last).ToHaveValueAsync("0");
        await Expect(world.Page.Locator("#notice-approval")).ToHaveTextAsync("Notice interval accepted.");
        await Expect(world.Page.Locator("#notice-release")).ToHaveTextAsync("Withdraw the thirty-minute notice.");

        await booking.First.PressAsync("End");
        await Expect(booking.Last).ToHaveValueAsync("30");
        await Expect(world.Page.Locator("#notice-release")).ToHaveTextAsync("Withdraw the immediate departure.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1847_0)]
    public async Task an_escort_amendment_protects_or_releases_the_actual_previous_arrangement()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.TransferAmendment);
        var booking = world.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Escort amendment", Exact = true }).GetByRole(AriaRole.Combobox);
        await world.Page.GetByLabel("Keep confirmed transfer safeguards").CheckAsync();
        await booking.First.PressAsync("End");
        await Expect(booking.Last).ToHaveValueAsync("true");
        await Expect(world.Page.Locator("#escort-approval")).ToHaveTextAsync("Escort arrangement accepted.");
        await Expect(world.Page.Locator("#escort-release")).ToHaveTextAsync("There is no earlier escort arrangement.");

        await booking.First.PressAsync("Home");
        await Expect(world.Page.Locator("#escort-approval")).ToHaveTextAsync("Keep the bedside escort while the escort requirement is active.");
        await Expect(booking.Last).ToHaveValueAsync("true");
        await Expect(world.Page.Locator("#escort-release")).ToHaveTextAsync("There is no earlier escort arrangement.");

        await world.Page.GetByLabel("Keep confirmed transfer safeguards").UncheckAsync();
        await booking.First.PressAsync("Home");
        await Expect(booking.Last).ToHaveValueAsync("false");
        await Expect(world.Page.Locator("#escort-approval")).ToHaveTextAsync("Escort arrangement accepted.");
        await Expect(world.Page.Locator("#escort-release")).ToHaveTextAsync("Release the previously reserved escort.");

        await booking.First.PressAsync("End");
        await Expect(booking.Last).ToHaveValueAsync("true");
        await Expect(world.Page.Locator("#escort-release")).ToHaveTextAsync("Update the prior independent departure.");
    }
}
