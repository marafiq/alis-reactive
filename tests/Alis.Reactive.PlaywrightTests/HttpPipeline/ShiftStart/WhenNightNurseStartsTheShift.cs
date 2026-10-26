using System.Collections.Concurrent;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.ShiftStart;

/// <summary>
/// As the night nurse starting my shift, I want to check the floor's partners side by side, open the
/// wing's handover sheet, order the night snacks and sign the board, so that the shift starts with
/// everything I need to know and nothing waiting on anything else.
/// </summary>
/// <remarks>
/// The pharmacy, the lab and the kitchen are asked through the community's gateway at once, each check
/// carrying its own X-Partner header; the pharmacy answers, the lab has nothing pending and says so with
/// a 404, the kitchen answers, and the status line waits for all three. The handover sheet of the chosen
/// wing is loaded into the page as the server renders it: Maple's is written, Cedar's is not. The kitchen
/// takes its orders as a form post of the snack and the count, never the floor note; its crackers line
/// runs on the old system, whose answer the page cannot read. The board is signed for the chosen wing
/// with no answer to read, and an order still with the kitchen does not hold it up.
/// </remarks>
[TestFixture]
public class WhenNightNurseStartsTheShift : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/ShiftStart";
    private const string PartnersUrl = "**/Sandbox/HttpPipeline/ShiftStart/Partners";
    private const string OrderUrl = "**/Sandbox/HttpPipeline/ShiftStart/Order";
    private const string Checking = "Checking with the pharmacy, the lab and the kitchen...";
    private const string AllChecked = "All three checked.";
    private const string PharmacyStocked = "Riverside Pharmacy: the night cart is stocked.";
    private const string LabNothingPending = "Northgate Lab: no results are pending for this floor.";
    private const string KitchenOpen = "Kitchen: warm milk and crackers until 2 AM.";
    private const string NothingPending = "the server responded with a status of 404";

    private PagePlan<ShiftStartModel> _plan = null!;
    private ConcurrentQueue<IRequest> _orders = null!;

    private ILocator CheckPartners => Page.Locator("#check-partners-btn");
    private ILocator ChecksStatus => Page.Locator("#checks-status");
    private ILocator PharmacyAnswer => Page.Locator("#pharmacy-answer");
    private ILocator LabAnswer => Page.Locator("#lab-answer");
    private ILocator KitchenAnswer => Page.Locator("#kitchen-answer");
    private ILocator OpenSheet => Page.Locator("#open-sheet-btn");
    private ILocator SheetStatus => Page.Locator("#sheet-status");
    private ILocator Sheet => Page.Locator("#handover-sheet");
    private ILocator SignBoard => Page.Locator("#sign-board-btn");
    private ILocator SignStatus => Page.Locator("#sign-status");
    private ILocator OrderSnacks => Page.Locator("#order-snacks-btn");
    private ILocator OrderStatus => Page.Locator("#order-status");

    private async Task StartTheShift()
    {
        _orders = new ConcurrentQueue<IRequest>();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/ShiftStart/Order")) _orders.Enqueue(request);
        };
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<ShiftStartModel>.FromPage(Page);
    }

    // The gateway address is the same for every partner; the check for one partner is told apart by its
    // X-Partner header, held at the browser until the test releases it, then sent on unchanged.
    private async Task<SemaphoreSlim> HoldPartnerUntilReleased(string partner)
    {
        var released = new SemaphoreSlim(0);
        await Page.RouteAsync(PartnersUrl, async route =>
        {
            if (route.Request.Headers.TryGetValue("x-partner", out var asked) && asked == partner)
                await released.WaitAsync();
            await route.ContinueAsync();
        });
        return released;
    }

    private async Task<SemaphoreSlim> HoldOrderUntilReleased()
    {
        var released = new SemaphoreSlim(0);
        await Page.RouteAsync(OrderUrl, async route =>
        {
            await released.WaitAsync();
            await route.ContinueAsync();
        });
        return released;
    }

    private async Task OrderWarmMilk(string count = "2")
    {
        await _plan.NativeDropDown(m => m.Snack).Select("Warm milk");
        if (count.Length > 0) await _plan.TextBox(m => m.SnackCount).Fill(count);
        await _plan.TextBox(m => m.FloorNote).Fill("Mrs. Park is awake");
    }

    [Test]
    public async Task checking_the_partners_shows_each_partners_own_answer_then_that_all_three_were_checked()
    {
        await StartTheShift();

        await ClickWhenStable(CheckPartners);

        await Expect(PharmacyAnswer).ToHaveTextAsync(PharmacyStocked);
        await Expect(LabAnswer).ToHaveTextAsync(LabNothingPending);
        await Expect(KitchenAnswer).ToHaveTextAsync(KitchenOpen);
        await Expect(ChecksStatus).ToHaveTextAsync(AllChecked);
        AssertNoConsoleErrorsExcept(NothingPending);
    }

    [Test]
    public async Task a_kitchen_that_cannot_be_reached_is_reported_and_the_other_checks_still_finish()
    {
        await Page.RouteAsync(PartnersUrl, route =>
            route.Request.Headers.TryGetValue("x-partner", out var partner) && partner == "kitchen"
                ? route.AbortAsync("connectionrefused")
                : route.ContinueAsync());
        await StartTheShift();

        await ClickWhenStable(CheckPartners);

        await Expect(KitchenAnswer).ToHaveTextAsync("The kitchen cannot be reached; call extension 42.");
        await Expect(PharmacyAnswer).ToHaveTextAsync(PharmacyStocked);
        await Expect(LabAnswer).ToHaveTextAsync(LabNothingPending);
        await Expect(ChecksStatus).ToHaveTextAsync(AllChecked);
        AssertNoConsoleErrorsExcept(NothingPending, "net::ERR_CONNECTION_REFUSED", "[alis:http] fetch.network-error");
    }

    [Test]
    public async Task the_checks_are_not_called_done_until_the_slowest_partner_answers()
    {
        var labAnswers = await HoldPartnerUntilReleased("lab");
        await StartTheShift();

        await ClickWhenStable(CheckPartners);

        await Expect(PharmacyAnswer).ToHaveTextAsync(PharmacyStocked);
        await Expect(KitchenAnswer).ToHaveTextAsync(KitchenOpen);
        await Expect(ChecksStatus).ToHaveTextAsync(Checking);
        await Expect(LabAnswer).ToHaveTextAsync("");
        labAnswers.Release();
        await Expect(LabAnswer).ToHaveTextAsync(LabNothingPending);
        await Expect(ChecksStatus).ToHaveTextAsync(AllChecked);
        AssertNoConsoleErrorsExcept(NothingPending);
    }

    [Test]
    public async Task opening_the_handover_sheet_shows_the_wings_notes()
    {
        await StartTheShift();
        await _plan.NativeDropDown(m => m.Wing).Select("Maple");

        await ClickWhenStable(OpenSheet);

        await Expect(Sheet.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Maple wing handover");
        await Expect(Sheet.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[]
        {
            "Helen Park: refused dinner; offered a supplement at 7 PM.",
            "Arthur Bell: new walker delivered; needs a fitting check.",
            "Rosa Diaz: family visiting tomorrow at 10 AM.",
        });
        await Expect(SheetStatus).ToHaveTextAsync("Handover sheet opened.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_wing_whose_sheet_is_not_written_yet_says_so_and_shows_no_sheet()
    {
        await StartTheShift();
        await _plan.NativeDropDown(m => m.Wing).Select("Cedar");

        await ClickWhenStable(OpenSheet);

        await Expect(SheetStatus).ToHaveTextAsync("Cedar wing's handover sheet is not written yet.");
        await Expect(Sheet).ToHaveTextAsync("");
        AssertNoConsoleErrorsExcept(NothingPending);
    }

    [Test]
    public async Task a_missing_sheet_leaves_the_sheet_already_open_in_place()
    {
        await StartTheShift();
        await _plan.NativeDropDown(m => m.Wing).Select("Maple");
        await ClickWhenStable(OpenSheet);
        await Expect(Sheet.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Maple wing handover");

        await _plan.NativeDropDown(m => m.Wing).Select("Cedar");
        await ClickWhenStable(OpenSheet);

        await Expect(SheetStatus).ToHaveTextAsync("Cedar wing's handover sheet is not written yet.");
        await Expect(Sheet.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Maple wing handover");
        AssertNoConsoleErrorsExcept(NothingPending);
    }

    [Test]
    public async Task opening_the_sheet_without_choosing_a_wing_asks_for_one()
    {
        await StartTheShift();

        await ClickWhenStable(OpenSheet);

        await Expect(SheetStatus).ToHaveTextAsync("Choose a wing to open its handover sheet.");
        await Expect(Sheet).ToHaveTextAsync("");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    // The order is a form post of the two named fields: the kitchen reads them as a form, and the floor
    // note typed beside them never leaves the floor.
    [Test]
    public async Task ordering_snacks_sends_the_kitchen_only_the_snack_and_the_count_as_a_form()
    {
        await StartTheShift();
        await OrderWarmMilk();

        await ClickWhenStable(OrderSnacks);

        await Expect(OrderStatus).ToHaveTextAsync("The kitchen will send 2 Warm milk to the floor (no floor note).");
        Assert.That(_orders, Has.Count.EqualTo(1));
        var order = _orders.Single().PostData ?? "";
        Assert.That(order, Does.Contain("name=\"Snack\"").And.Contain("Warm milk").And.Contain("name=\"SnackCount\""));
        Assert.That(order, Does.Not.Contain("FloorNote"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ordering_without_saying_how_many_is_refused_by_the_kitchen()
    {
        await StartTheShift();
        await OrderWarmMilk(count: "");

        await ClickWhenStable(OrderSnacks);

        await Expect(OrderStatus).ToHaveTextAsync("Say how many.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    // The crackers line answers with JSON the page cannot read: that is no answer, and the page says so.
    [Test]
    public async Task a_kitchen_answer_the_page_cannot_read_is_reported_as_no_answer()
    {
        await StartTheShift();
        await _plan.NativeDropDown(m => m.Snack).Select("Crackers");
        await _plan.TextBox(m => m.SnackCount).Fill("1");

        await ClickWhenStable(OrderSnacks);

        await Expect(OrderStatus).ToHaveTextAsync("The kitchen's answer could not be read; call extension 42.");
        AssertNoConsoleErrorsExcept("[alis:http] fetch.body-unreadable");
    }

    [Test]
    public async Task signing_the_board_confirms_without_an_answer_to_read()
    {
        await StartTheShift();
        await _plan.NativeDropDown(m => m.Wing).Select("Maple");

        await ClickWhenStable(SignBoard);

        await Expect(SignStatus).ToHaveTextAsync("Board signed for the shift.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task signing_the_board_without_a_wing_asks_for_one()
    {
        await StartTheShift();

        await ClickWhenStable(SignBoard);

        await Expect(SignStatus).ToHaveTextAsync("Choose a wing before signing the board.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    // Two requests on one page keep their own loading lines and their own answers: the board is signed
    // while the kitchen still has the order, and each line ends with its own answer.
    [Test]
    public async Task an_order_still_with_the_kitchen_does_not_hold_up_signing_the_board()
    {
        var kitchenAnswers = await HoldOrderUntilReleased();
        await StartTheShift();
        await OrderWarmMilk();
        await _plan.NativeDropDown(m => m.Wing).Select("Maple");
        await ClickWhenStable(OrderSnacks);
        await Expect(OrderStatus).ToHaveTextAsync("Ordering from the kitchen...");

        await ClickWhenStable(SignBoard);

        await Expect(SignStatus).ToHaveTextAsync("Board signed for the shift.");
        await Expect(OrderStatus).ToHaveTextAsync("Ordering from the kitchen...");
        kitchenAnswers.Release();
        await Expect(OrderStatus).ToHaveTextAsync("The kitchen will send 2 Warm milk to the floor (no floor note).");
        await Expect(SignStatus).ToHaveTextAsync("Board signed for the shift.");
        AssertNoConsoleErrors();
    }
}
