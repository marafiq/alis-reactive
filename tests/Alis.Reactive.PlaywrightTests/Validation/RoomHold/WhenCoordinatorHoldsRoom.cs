using System.Collections.Concurrent;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.RoomHold;

/// <summary>
/// As a coordinator, I want to be told why the server refused a room hold, even when the reason is
/// about the room rather than anything I typed, so that I choose another room instead of guessing.
/// </summary>
/// <remarks>
/// The room travels in a hidden field, so the server's refusal has no message slot beside an input;
/// the page's validation summary is where it shows.
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

        await ClickWhenStable(PlaceHold);

        await Expect(ValidationSummary).ToBeVisibleAsync();
        await Expect(ValidationSummary).ToHaveTextAsync(RoomHeldElsewhere);
        await Expect(_plan.ErrorFor(m => m.ProspectName)).ToBeHiddenAsync();
        await Expect(RoomHoldStatus).ToHaveTextAsync("No room held yet.");

        await ClickWhenStable(PlaceHold);
        await Expect(ValidationSummary).ToHaveTextAsync(RoomHeldElsewhere);
        Assert.That(_holdsSent, Has.Count.EqualTo(2), "Each Place Hold asks the server once.");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task choosing_a_free_room_after_a_refusal_holds_it_and_clears_the_reason()
    {
        await NavigateAndBoot();
        await WriteProspectName("Jane Smith");
        await ClickWhenStable(PlaceHold);
        await Expect(ValidationSummary).ToBeVisibleAsync();
        await Expect(ValidationSummary).ToHaveTextAsync(RoomHeldElsewhere);

        await ClickWhenStable(ChooseMaple216);
        await ClickWhenStable(PlaceHold);

        await Expect(RoomHoldStatus).ToHaveTextAsync("Maple 216 is held for Jane Smith.");
        await Expect(ValidationSummary).ToBeHiddenAsync();
        Assert.That(_holdsSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task a_missing_name_is_asked_for_beside_the_field_not_in_the_summary()
    {
        await NavigateAndBoot();
        await ClickWhenStable(ChooseMaple216);

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
}
