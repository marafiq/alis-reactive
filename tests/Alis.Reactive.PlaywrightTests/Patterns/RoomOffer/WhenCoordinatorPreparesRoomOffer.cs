using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.RoomOffer;

namespace Alis.Reactive.PlaywrightTests.Patterns.RoomOffer;

/// <summary>
/// As an admissions coordinator, I want the room offer to fill the deposit, the floor and the rate from the
/// care level and the wing I choose, and to route the deposit to whoever signs it off, so that the offer I
/// send a family is priced the way Maple Grove prices it.
/// </summary>
/// <remarks>
/// Assisted living asks a $2,500 deposit; a respite stay asks none and is offered no room, its suite being
/// assigned on arrival; memory care leaves the deposit to the coordinator. The executive director signs off
/// a deposit of $5,000 or more, flagged in red; the business office one of $1,000 or more; a smaller one
/// needs no sign-off. Maple rooms are on the first floor at $4,200 a month, Oak on the second at $4,800 and
/// Cedar on the third at $6,100. The offer letter quotes the room's rate, whether a wing filled it or the
/// coordinator typed one. The page writes the deposit and the rate into their fields, and the sign-off and
/// the quote follow those writes as they follow typing, because the numeric field raises its change for a
/// value written to it. A value written into a select raises no change in the browser, so the page never
/// leaves a select's own reaction to follow a write: starting over clears the wing, the floor and the rate
/// itself. The inputs' element ids are not exercised as such: nothing a coordinator sees depends on their
/// spelling, the plan carries each id it renders, and the tests find the inputs through the plan. The
/// room's members reach the server by their dotted names, which the sent offer proves.
/// </remarks>
[TestFixture]
public class WhenCoordinatorPreparesRoomOffer : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/RoomOffer";

    private static readonly Regex Flagged = new(@"\btext-red-700\b");

    private PagePlan<RoomOfferModel> _plan = null!;

    private NativeDropDownLocator CareLevel => _plan.NativeDropDown(m => m.CareLevel);
    private NumericTextBoxLocator Deposit => _plan.NumericTextBox(m => m.Deposit);
    private NativeDropDownLocator Wing => _plan.NativeDropDown(m => m.Room.Wing);
    private NativeDropDownLocator Floor => _plan.NativeDropDown(m => m.Room.Floor);
    private NumericTextBoxLocator MonthlyRate => _plan.NumericTextBox(m => m.Room.MonthlyRate);
    private ILocator ChosenFloor => Floor.Input.Locator("option:checked");
    private ILocator OfferSummary => Page.Locator("#offer-summary");
    private ILocator DepositSignOff => Page.Locator("#deposit-signoff");
    private ILocator RoomSection => Page.Locator("#room-section");
    private ILocator WingNote => Page.Locator("#wing-note");
    private ILocator QuotedRate => Page.Locator("#offer-rate");
    private ILocator OfferStatus => Page.Locator("#offer-status");
    private ILocator SendOffer => Page.Locator("#send-offer-btn");
    private ILocator StartOver => Page.Locator("#start-over-btn");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<RoomOfferModel>.FromPage(Page);
    }

    [Test]
    public async Task the_offer_starts_by_asking_for_a_care_level()
    {
        await NavigateAndBoot();

        await Expect(OfferSummary).ToHaveTextAsync("Choose a care level to start the offer.");
        await Expect(Deposit.Input).ToHaveValueAsync("$0");
        await Expect(DepositSignOff).ToHaveTextAsync("No sign-off is needed for this deposit.");
        await Expect(RoomSection).ToBeVisibleAsync();
        await Expect(WingNote).ToHaveTextAsync("Choose a wing to see its floor and rate.");
        await Expect(QuotedRate).ToHaveTextAsync("0");
        await Expect(OfferStatus).ToHaveTextAsync("Not sent yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_assisted_living_after_a_respite_stay_asks_a_2500_deposit_and_offers_a_room()
    {
        await NavigateAndBoot();
        await CareLevel.Select("Respite stay");
        await Expect(RoomSection).ToBeHiddenAsync();

        await CareLevel.Select("Assisted living");

        await Expect(Deposit.Input).ToHaveValueAsync("$2,500");
        await Expect(OfferSummary).ToHaveTextAsync("Assisted living: a $2,500 deposit holds the room.");
        await Expect(RoomSection).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_assisted_living_deposit_goes_to_the_business_office_to_sign_off()
    {
        await NavigateAndBoot();

        await CareLevel.Select("Assisted living");

        await Expect(DepositSignOff).ToHaveTextAsync("The business office signs off on this deposit.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_respite_stay_takes_no_deposit_and_offers_no_room()
    {
        await NavigateAndBoot();
        await CareLevel.Select("Assisted living");
        await Expect(Deposit.Input).ToHaveValueAsync("$2,500");

        await CareLevel.Select("Respite stay");

        await Expect(Deposit.Input).ToHaveValueAsync("$0");
        await Expect(OfferSummary).ToHaveTextAsync("A respite stay takes no deposit; the respite suite is assigned on arrival.");
        await Expect(RoomSection).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_care_after_a_respite_stay_offers_a_room_again()
    {
        await NavigateAndBoot();
        await CareLevel.Select("Respite stay");
        await Expect(RoomSection).ToBeHiddenAsync();

        await CareLevel.Select("Memory care");

        await Expect(OfferSummary).ToHaveTextAsync("Choose the room and the deposit for this offer.");
        await Expect(RoomSection).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_deposit_of_5000_goes_to_the_executive_director_flagged_and_4999_to_the_business_office()
    {
        await NavigateAndBoot();

        await Deposit.FillAndBlur("5000");

        await Expect(DepositSignOff).ToHaveTextAsync("The executive director signs off on a deposit of $5,000 or more.");
        await Expect(DepositSignOff).ToHaveClassAsync(Flagged);

        await Deposit.FillAndBlur("4999");

        await Expect(DepositSignOff).ToHaveTextAsync("The business office signs off on this deposit.");
        await Expect(DepositSignOff).Not.ToHaveClassAsync(Flagged);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_deposit_of_1000_goes_to_the_business_office_and_999_needs_no_sign_off()
    {
        await NavigateAndBoot();

        await Deposit.FillAndBlur("1000");

        await Expect(DepositSignOff).ToHaveTextAsync("The business office signs off on this deposit.");

        await Deposit.FillAndBlur("999");

        await Expect(DepositSignOff).ToHaveTextAsync("No sign-off is needed for this deposit.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task lowering_a_flagged_deposit_under_1000_needs_no_sign_off_and_drops_the_flag()
    {
        await NavigateAndBoot();
        await Deposit.FillAndBlur("6000");
        await Expect(DepositSignOff).ToHaveClassAsync(Flagged);

        await Deposit.FillAndBlur("500");

        await Expect(DepositSignOff).ToHaveTextAsync("No sign-off is needed for this deposit.");
        await Expect(DepositSignOff).Not.ToHaveClassAsync(Flagged);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_maple_wing_fills_its_first_floor_and_rate()
    {
        await NavigateAndBoot();

        await Wing.Select("Maple");

        await Expect(ChosenFloor).ToHaveTextAsync("First floor");
        await Expect(MonthlyRate.Input).ToHaveValueAsync("$4,200");
        await Expect(WingNote).ToHaveTextAsync("Maple rooms are on the first floor, beside the dining room.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_oak_wing_fills_its_second_floor_and_rate()
    {
        await NavigateAndBoot();

        await Wing.Select("Oak");

        await Expect(ChosenFloor).ToHaveTextAsync("Second floor");
        await Expect(MonthlyRate.Input).ToHaveValueAsync("$4,800");
        await Expect(WingNote).ToHaveTextAsync("Oak rooms are on the second floor, with garden views.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_the_cedar_wing_fills_its_third_floor_and_rate()
    {
        await NavigateAndBoot();

        await Wing.Select("Cedar");

        await Expect(ChosenFloor).ToHaveTextAsync("Third floor");
        await Expect(MonthlyRate.Input).ToHaveValueAsync("$6,100");
        await Expect(WingNote).ToHaveTextAsync("Cedar is the secured memory care wing, on the third floor.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_wing_clears_its_floor_and_rate()
    {
        await NavigateAndBoot();
        await Wing.Select("Maple");
        await Expect(MonthlyRate.Input).ToHaveValueAsync("$4,200");

        await Wing.Select("-- Choose a wing --");

        await Expect(ChosenFloor).ToHaveTextAsync("-- Choose a floor --");
        await Expect(MonthlyRate.Input).ToHaveValueAsync("$0");
        await Expect(WingNote).ToHaveTextAsync("Choose a wing to see its floor and rate.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_offer_letter_quotes_the_rate_of_the_chosen_wing()
    {
        await NavigateAndBoot();

        await Wing.Select("Maple");

        await Expect(QuotedRate).ToHaveTextAsync("4200");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_offer_letter_quotes_a_negotiated_rate_the_coordinator_types()
    {
        await NavigateAndBoot();
        await Wing.Select("Oak");
        await Expect(QuotedRate).ToHaveTextAsync("4800");

        await MonthlyRate.FillAndBlur("4650");

        await Expect(QuotedRate).ToHaveTextAsync("4650");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_the_offer_confirms_the_wing_floor_rate_and_deposit()
    {
        await NavigateAndBoot();
        await CareLevel.Select("Assisted living");
        await Wing.Select("Oak");
        await Expect(MonthlyRate.Input).ToHaveValueAsync("$4,800");

        await ClickWhenStable(SendOffer);

        await Expect(OfferStatus).ToHaveTextAsync("Offer sent for the Oak wing, floor 2, at $4,800.00 a month with a $2,500.00 deposit.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task starting_over_clears_the_offer()
    {
        await NavigateAndBoot();
        await Wing.Select("Oak");
        await CareLevel.Select("Respite stay");
        await Deposit.FillAndBlur("2500");
        await ClickWhenStable(SendOffer);
        await Expect(OfferStatus).ToHaveTextAsync("Offer sent for the Oak wing, floor 2, at $4,800.00 a month with a $2,500.00 deposit.");
        await Expect(RoomSection).ToBeHiddenAsync();

        await ClickWhenStable(StartOver);

        await Expect(CareLevel.Input).ToHaveValueAsync("");
        await Expect(Deposit.Input).ToHaveValueAsync("$0");
        await Expect(DepositSignOff).ToHaveTextAsync("No sign-off is needed for this deposit.");
        await Expect(OfferSummary).ToHaveTextAsync("Choose a care level to start the offer.");
        await Expect(RoomSection).ToBeVisibleAsync();
        await Expect(Wing.Input).ToHaveValueAsync("");
        await Expect(ChosenFloor).ToHaveTextAsync("-- Choose a floor --");
        await Expect(MonthlyRate.Input).ToHaveValueAsync("$0");
        await Expect(WingNote).ToHaveTextAsync("Choose a wing to see its floor and rate.");
        await Expect(QuotedRate).ToHaveTextAsync("0");
        await Expect(OfferStatus).ToHaveTextAsync("Not sent yet.");
        AssertNoConsoleErrors();
    }

    // The care level was truly cleared: choosing the same level again is a change, and sets its deposit.
    [Test]
    public async Task choosing_assisted_living_again_after_starting_over_asks_the_deposit_again()
    {
        await NavigateAndBoot();
        await CareLevel.Select("Assisted living");
        await ClickWhenStable(StartOver);
        await Expect(Deposit.Input).ToHaveValueAsync("$0");

        await CareLevel.Select("Assisted living");

        await Expect(Deposit.Input).ToHaveValueAsync("$2,500");
        await Expect(OfferSummary).ToHaveTextAsync("Assisted living: a $2,500 deposit holds the room.");
        AssertNoConsoleErrors();
    }
}
