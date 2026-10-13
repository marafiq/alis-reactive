using System.Collections.Concurrent;
using System.Text.Json;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

/// <summary>
/// As a care coordinator, I keep a resident's profile current: I clear a nickname the resident no
/// longer uses, I record when the resident was admitted, and I lock the profile while the care team
/// reviews it.
/// </summary>
[TestFixture]
public class WhenCoordinatorUpdatesResidentProfile : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";
    private const string NicknameEditorId = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentProfile__Nickname";

    // Each quick-edit card on the page renders its own plan before the profile form's, so the
    // profile's inputs are found by the ids the framework derives from the model.
    private static readonly string AdmissionDateId = IdGenerator.For<ResidentProfile>(m => m.AdmissionDate);
    private static readonly string ProfileLockedId = IdGenerator.For<ResidentProfile>(m => m.ProfileLocked);

    private DatePickerLocator AdmissionDate => new(Page, AdmissionDateId);
    private NativeCheckBoxLocator ProfileLocked => new(Page, ProfileLockedId);
    private ILocator AdmissionDateError => Page.Locator($"#{AdmissionDateId}_error");
    private ILocator NicknameValue => Page.Locator($"#{NicknameEditorId} .e-editable-value");
    private ILocator NicknameValueWrapper => Page.Locator($"#{NicknameEditorId} .e-editable-value-wrapper");
    private ILocator NicknameInput => Page.Locator($"#{NicknameEditorId} input.e-control");
    private ILocator LockedNote => Page.Locator("#profile-locked-note");
    private ILocator SaveProfile => Page.Locator("#submit-profile");
    private ILocator ProfileStatus => Page.Locator("#profile-status");

    private async Task OpenProfile()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#submit-profile");
    }

    [Test]
    public async Task clearing_the_nickname_saves_the_profile_with_no_nickname()
    {
        await OpenProfile();
        await Expect(NicknameValue).ToHaveTextAsync("Ellie");

        await Page.Locator("#clear-nickname").ClickAsync();

        // The editor shows its empty text only when its value is cleared; a write that did nothing
        // keeps "Ellie", and a null written as text shows "null".
        await Expect(NicknameValue).ToHaveTextAsync("Empty");

        var save = await Page.RunAndWaitForRequestAsync(
            () => SaveProfile.ClickAsync(),
            request => request.Method == "POST" && request.Url.Contains("/FusionInPlaceEditor/UpdateProfile"));

        using (var body = JsonDocument.Parse(save.PostData ?? "{}"))
        {
            Assert.That(body.RootElement.TryGetProperty("Nickname", out var nickname), Is.True,
                $"The profile posted no Nickname: {save.PostData}");
            Assert.That(nickname.ValueKind, Is.EqualTo(JsonValueKind.Null), save.PostData);
        }

        await Expect(ProfileStatus).ToHaveTextAsync("Saved", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_profile_locked_for_care_review_keeps_its_nickname_closed_to_edits()
    {
        await OpenProfile();
        await ProfileLocked.Check();

        await NicknameValueWrapper.First.ClickAsync();

        // The note shows only when the edit was asked for while locked; the nickname stays on show.
        await Expect(LockedNote).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(NicknameValue).ToBeVisibleAsync();
        await Expect(NicknameValue).ToHaveTextAsync("Ellie");
        await Expect(NicknameInput).ToHaveCountAsync(0);

        await ProfileLocked.Input.UncheckAsync();
        await NicknameValueWrapper.First.ClickAsync();

        await Expect(NicknameInput.First).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(LockedNote).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_admission_date_after_today_is_refused_and_today_is_saved()
    {
        await OpenProfile();
        var saves = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/FusionInPlaceEditor/UpdateProfile")) saves.Enqueue(request.Url);
        };
        var tomorrow = DateTime.Today.AddDays(1);
        var today = DateTime.Today;

        await AdmissionDate.SelectDate(tomorrow.Year, tomorrow.Month, tomorrow.Day);
        await SaveProfile.ClickAsync();

        await Expect(AdmissionDateError)
            .ToHaveTextAsync("'Admission Date' must be at most today.", new() { Timeout = 5000 });
        Assert.That(saves, Is.Empty, "A profile admitted tomorrow is not sent.");

        // Today is the boundary: "at most today" accepts it, where "before today" would not.
        await AdmissionDate.SelectDate(today.Year, today.Month, today.Day);
        await SaveProfile.ClickAsync();

        await Expect(ProfileStatus).ToHaveTextAsync("Saved", new() { Timeout = 5000 });
        await Expect(AdmissionDateError).ToBeHiddenAsync();
        Assert.That(saves, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }
}
