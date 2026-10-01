using System.Collections.Concurrent;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.RoomRate;

/// <summary>
/// As a community administrator, I want a room name of only spaces, a blank or $0 monthly rate, and an
/// undecided community fee to be asked for before the room is sent, so that I correct them on the page
/// instead of waiting for the server to refuse them; a $0 community fee is taken as a waived fee.
/// </summary>
/// <remarks>
/// Each test fills the field it is about first and a valid field last: leaving an invalid field shows
/// its message, which moves the Add Room button, so the click must not be what leaves it. An "asks for"
/// test then corrects the field and adds the room: exactly one add request proves the first Add Room
/// sent nothing, whenever a send would have happened.
/// </remarks>
[TestFixture]
public class WhenAdministratorAddsRoomRate : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/RoomRate";

    private PagePlan<RoomRateModel> _plan = null!;
    private ConcurrentQueue<string> _roomsSent = null!;

    private ILocator RoomRateStatus => Page.Locator("#room-rate-status");
    private ILocator AddRoom => Page.Locator("#add-room-btn");
    private NativeTextBoxLocator RoomName => _plan.TextBox(m => m.RoomName);
    private NativeTextBoxLocator MonthlyRate => _plan.TextBox(m => m.MonthlyRate);
    private NativeTextBoxLocator CommunityFee => _plan.TextBox(m => m.CommunityFee);

    private async Task NavigateAndBoot()
    {
        _roomsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Validation/RoomRate/Add")) _roomsSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<RoomRateModel>.FromPage(Page);
    }

    private void AssertTheRoomWasSentOnce()
    {
        Assert.That(_roomsSent, Has.Count.EqualTo(1), "Only the corrected room may be sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_room_name_of_only_spaces_is_asked_for()
    {
        await NavigateAndBoot();
        await RoomName.Fill("   ");
        await MonthlyRate.Fill("4200");
        await CommunityFee.Fill("1500");

        await ClickWhenStable(AddRoom);

        await Expect(_plan.ErrorFor(m => m.RoomName)).ToHaveTextAsync("Enter the room name.");
        await Expect(_plan.ErrorFor(m => m.MonthlyRate)).ToHaveTextAsync("");
        await Expect(RoomRateStatus).ToHaveTextAsync("No room added yet.");

        await RoomName.Fill("Maple 214");
        await ClickWhenStable(AddRoom);
        await Expect(RoomRateStatus).ToHaveTextAsync(
            "Maple 214 added to the rate sheet at $4,200.00 a month, community fee $1,500.00.");
        AssertTheRoomWasSentOnce();
    }

    [Test]
    public async Task leaving_the_room_name_with_only_spaces_asks_for_it_again()
    {
        await NavigateAndBoot();
        await MonthlyRate.Fill("4200");
        await CommunityFee.Fill("1500");
        await ClickWhenStable(AddRoom);
        await Expect(_plan.ErrorFor(m => m.RoomName)).ToHaveTextAsync("Enter the room name.");

        await RoomName.Fill("   ");
        await Expect(_plan.ErrorFor(m => m.RoomName)).ToHaveTextAsync("");
        await RoomName.Input.PressAsync("Tab");

        await Expect(_plan.ErrorFor(m => m.RoomName)).ToHaveTextAsync("Enter the room name.");

        await RoomName.Fill("Maple 214");
        await ClickWhenStable(AddRoom);
        await Expect(RoomRateStatus).ToHaveTextAsync(
            "Maple 214 added to the rate sheet at $4,200.00 a month, community fee $1,500.00.");
        AssertTheRoomWasSentOnce();
    }

    [Test]
    public async Task a_monthly_rate_of_zero_is_asked_for()
    {
        await NavigateAndBoot();
        await MonthlyRate.Fill("0");
        await RoomName.Fill("Maple 214");
        await CommunityFee.Fill("1500");

        await ClickWhenStable(AddRoom);

        await Expect(_plan.ErrorFor(m => m.MonthlyRate)).ToHaveTextAsync("Enter the monthly rate.");
        await Expect(_plan.ErrorFor(m => m.RoomName)).ToHaveTextAsync("");
        await Expect(RoomRateStatus).ToHaveTextAsync("No room added yet.");

        await MonthlyRate.Fill("4200");
        await ClickWhenStable(AddRoom);
        await Expect(RoomRateStatus).ToHaveTextAsync(
            "Maple 214 added to the rate sheet at $4,200.00 a month, community fee $1,500.00.");
        AssertTheRoomWasSentOnce();
    }

    [Test]
    public async Task clearing_the_monthly_rate_asks_for_it()
    {
        await NavigateAndBoot();
        await MonthlyRate.Clear();
        await RoomName.Fill("Maple 214");
        await CommunityFee.Fill("1500");

        await ClickWhenStable(AddRoom);

        await Expect(_plan.ErrorFor(m => m.MonthlyRate)).ToHaveTextAsync("Enter the monthly rate.");
        await Expect(_plan.ErrorFor(m => m.RoomName)).ToHaveTextAsync("");
        await Expect(RoomRateStatus).ToHaveTextAsync("No room added yet.");

        await MonthlyRate.Fill("4200");
        await ClickWhenStable(AddRoom);
        await Expect(RoomRateStatus).ToHaveTextAsync(
            "Maple 214 added to the rate sheet at $4,200.00 a month, community fee $1,500.00.");
        AssertTheRoomWasSentOnce();
    }

    [Test]
    public async Task leaving_the_community_fee_blank_asks_for_it()
    {
        await NavigateAndBoot();
        await RoomName.Fill("Maple 214");
        await MonthlyRate.Fill("4200");

        await ClickWhenStable(AddRoom);

        await Expect(_plan.ErrorFor(m => m.CommunityFee))
            .ToHaveTextAsync("Enter the community fee, or 0 if it is waived.");
        await Expect(RoomRateStatus).ToHaveTextAsync("No room added yet.");

        await CommunityFee.Fill("1500");
        await ClickWhenStable(AddRoom);
        await Expect(RoomRateStatus).ToHaveTextAsync(
            "Maple 214 added to the rate sheet at $4,200.00 a month, community fee $1,500.00.");
        AssertTheRoomWasSentOnce();
    }

    [Test]
    public async Task a_waived_community_fee_of_zero_is_accepted()
    {
        await NavigateAndBoot();
        await CommunityFee.Fill("0");
        await RoomName.Fill("Maple 214");
        await MonthlyRate.Fill("4200");

        await ClickWhenStable(AddRoom);

        await Expect(RoomRateStatus).ToHaveTextAsync(
            "Maple 214 added to the rate sheet at $4,200.00 a month, community fee waived.");
        AssertTheRoomWasSentOnce();
    }

    [Test]
    public async Task a_named_room_with_a_monthly_rate_is_added_to_the_rate_sheet()
    {
        await NavigateAndBoot();
        await RoomName.Fill("Maple 214");
        await MonthlyRate.Fill("4200");
        await CommunityFee.Fill("1500");

        await ClickWhenStable(AddRoom);

        await Expect(RoomRateStatus).ToHaveTextAsync(
            "Maple 214 added to the rate sheet at $4,200.00 a month, community fee $1,500.00.");
        AssertTheRoomWasSentOnce();
    }
}
