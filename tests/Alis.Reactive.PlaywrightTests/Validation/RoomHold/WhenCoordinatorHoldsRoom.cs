using System.Collections.Concurrent;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.RoomHold;

/// <summary>
/// As a coordinator, I want to be told why the server refused a room hold, even when the reason is
/// about the room rather than anything I typed, so that I choose another room instead of guessing.
/// </summary>
/// <remarks>
/// The room travels in a hidden field, so neither the page's own "choose a room" check nor the
/// server's refusal has a message slot beside an input; the hold form's summary, which the page places in
/// the hold's card, is where both show. Which rooms are held is known only to the server's hold book, which it asks asynchronously.
/// The reservations office's own validator, registered by its module, never reaches the page either.
/// </remarks>
[TestFixture]
public class WhenCoordinatorHoldsRoom : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/RoomHold";
    private const string RoomHeldElsewhere = "Maple 214 is already held for another family. Choose another room.";

    private PagePlan<RoomHoldModel> _plan = null!;
    private ConcurrentQueue<string> _holdsSent = null!;

    private ILocator ValidationSummary => Page.Locator("[data-reactive-validation-summary]");
    private ILocator ChooseMaple214 => Page.Locator("#choose-maple-214-btn");
    private ILocator ChooseMaple216 => Page.Locator("#choose-maple-216-btn");
    private ILocator PlaceHold => Page.Locator("#place-hold-btn");
    private ILocator RoomHoldStatus => Page.Locator("#room-hold-status");
    private NativeTextBoxLocator ProspectName => _plan.TextBox(m => m.ProspectName);
    private NativeCheckBoxLocator HoldTermsAgreed => _plan.NativeCheckBox(m => m.FamilyAgreedToHoldTerms);

    private async Task NavigateAndBoot()
    {
        _holdsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Validation/RoomHold/Hold")) _holdsSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<RoomHoldModel>.FromPage(Page);
    }

    // Leaving the field first lets its message settle, so the click lands on Place Hold.
    private async Task WriteProspectName(string name)
    {
        await ProspectName.Fill(name);
        await Page.Keyboard.PressAsync("Tab");
    }

    [Test]
    public async Task a_room_another_family_holds_is_refused_with_the_reason()
    {
        await NavigateAndBoot();
        await Expect(ValidationSummary).ToBeHiddenAsync();
        await ClickWhenStable(ChooseMaple214);
        await WriteProspectName("Jane Smith");
        await HoldTermsAgreed.Check();

        await ClickWhenStable(PlaceHold);

        // The room has a browser rule but no message slot, so the server's refusal of it goes to the summary.
        await Expect(ValidationSummary).ToBeVisibleAsync();
        await Expect(ValidationSummary).ToHaveTextAsync(RoomHeldElsewhere);
        await Expect(_plan.ErrorFor(m => m.ProspectName)).ToBeHiddenAsync();
        await Expect(RoomHoldStatus).ToHaveTextAsync("No room held yet.");

        // Only the server knows which rooms are held, so placing the hold again asks it again.
        await Page.RunAndWaitForResponseAsync(
            async () => await ClickWhenStable(PlaceHold),
            "**/Sandbox/Validation/RoomHold/Hold**");
        Assert.That(_holdsSent, Has.Count.EqualTo(2), "Each Place Hold asks the server once.");
        await Expect(ValidationSummary).ToHaveTextAsync(RoomHeldElsewhere);
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    // The page places the hold form's summary in the hold's card, above the form, so the reason is read where
    // the coordinator is working, not after the page.
    [Test]
    public async Task the_reason_shows_in_the_holds_card_where_the_page_places_it()
    {
        await NavigateAndBoot();
        await ClickWhenStable(ChooseMaple214);
        await WriteProspectName("Jane Smith");
        await HoldTermsAgreed.Check();

        await ClickWhenStable(PlaceHold);

        await Expect(Page.GetByRole(AriaRole.Region, new() { Name = "The hold" }).GetByText(RoomHeldElsewhere)).ToBeVisibleAsync();
    }

    [Test]
    public async Task choosing_a_free_room_after_a_refusal_holds_it_and_clears_the_reason()
    {
        await NavigateAndBoot();
        await ClickWhenStable(ChooseMaple214);
        await WriteProspectName("Jane Smith");
        await HoldTermsAgreed.Check();
        await ClickWhenStable(PlaceHold);
        await Expect(ValidationSummary).ToBeVisibleAsync();
        await Expect(ValidationSummary).ToHaveTextAsync(RoomHeldElsewhere);

        await ClickWhenStable(ChooseMaple216);
        await ClickWhenStable(PlaceHold);

        await Expect(RoomHoldStatus).ToHaveTextAsync("Maple 216 is held for Jane Smith.");
        await Expect(ValidationSummary).ToBeHiddenAsync();
        Assert.That(_holdsSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    // The office's one-hold-per-prospect rule asks the office's hold book, which only the server can:
    // the hold is sent and the server answers by the name.
    [Test]
    public async Task a_prospect_who_already_holds_a_room_is_refused_by_the_reservations_office()
    {
        await NavigateAndBoot();
        await ClickWhenStable(ChooseMaple216);
        await WriteProspectName(ReservationsOfficeValidator.ProspectWithHold);
        await HoldTermsAgreed.Check();

        await ClickWhenStable(PlaceHold);

        await Expect(_plan.ErrorFor(m => m.ProspectName))
            .ToHaveTextAsync("Ruth Carter already holds Maple 218. Release that hold before placing another.");
        await Expect(RoomHoldStatus).ToHaveTextAsync("No room held yet.");
        Assert.That(_holdsSent, Has.Count.EqualTo(1), "The office's rule is the server's alone, so the hold is sent.");

        await WriteProspectName("Jane Smith");
        await ClickWhenStable(PlaceHold);
        await Expect(RoomHoldStatus).ToHaveTextAsync("Maple 216 is held for Jane Smith.");
        await Expect(_plan.ErrorFor(m => m.ProspectName)).ToBeHiddenAsync();
        Assert.That(_holdsSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task placing_a_hold_without_choosing_a_room_asks_for_a_room_in_the_summary()
    {
        await NavigateAndBoot();
        await WriteProspectName("Jane Smith");
        await HoldTermsAgreed.Check();

        await ClickWhenStable(PlaceHold);

        await Expect(ValidationSummary).ToBeVisibleAsync();
        await Expect(ValidationSummary).ToHaveTextAsync("Choose a room to hold.");
        await Expect(RoomHoldStatus).ToHaveTextAsync("No room held yet.");

        await ClickWhenStable(ChooseMaple216);
        await ClickWhenStable(PlaceHold);
        await Expect(RoomHoldStatus).ToHaveTextAsync("Maple 216 is held for Jane Smith.");
        await Expect(ValidationSummary).ToBeHiddenAsync();
        Assert.That(_holdsSent, Has.Count.EqualTo(1), "A hold with no room is not sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_missing_name_is_asked_for_beside_the_field_not_in_the_summary()
    {
        await NavigateAndBoot();
        await ClickWhenStable(ChooseMaple216);
        await HoldTermsAgreed.Check();

        await ClickWhenStable(PlaceHold);

        await Expect(_plan.ErrorFor(m => m.ProspectName)).ToHaveTextAsync("Enter the prospective resident's name.");
        await Expect(ValidationSummary).ToBeHiddenAsync();

        await WriteProspectName("Jane Smith");
        await Expect(_plan.ErrorFor(m => m.ProspectName)).ToBeHiddenAsync();
        await ClickWhenStable(PlaceHold);
        await Expect(RoomHoldStatus).ToHaveTextAsync("Maple 216 is held for Jane Smith.");
        Assert.That(_holdsSent, Has.Count.EqualTo(1), "Only the named hold may be sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_hold_the_family_has_not_agreed_to_is_asked_for_beside_the_agreement()
    {
        await NavigateAndBoot();
        await ClickWhenStable(ChooseMaple216);
        await WriteProspectName("Jane Smith");

        await ClickWhenStable(PlaceHold);

        await Expect(_plan.ErrorFor(m => m.FamilyAgreedToHoldTerms))
            .ToHaveTextAsync("The family must agree that the hold lapses after 48 hours.");
        await Expect(RoomHoldStatus).ToHaveTextAsync("No room held yet.");

        await HoldTermsAgreed.Check();
        await ClickWhenStable(PlaceHold);
        await Expect(RoomHoldStatus).ToHaveTextAsync("Maple 216 is held for Jane Smith.");
        await Expect(_plan.ErrorFor(m => m.FamilyAgreedToHoldTerms)).ToHaveTextAsync("");
        Assert.That(_holdsSent, Has.Count.EqualTo(1), "A hold the family has not agreed to is not sent.");
        AssertNoConsoleErrors();
    }
}
