using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

/// <summary>
/// §A mixed form: click "Save Profile" → the Gather(IncludeAll()) includes every field in the plan,
/// including the three InPlaceEditor fields (Nickname, DateOfBirth, Allergies) alongside the other
/// Fusion inputs. Proves InPlaceEditor participates in the standard component-registration / gather path.
/// </summary>
[TestFixture]
public class WhenResidentProfileFormGathersInPlaceEditorValues : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task save_profile_posts_body_containing_all_fields()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#submit-profile");

        var requestTask = Page.WaitForRequestAsync(matchingRequest =>
            matchingRequest.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateProfile")
            && matchingRequest.Method == "POST",
            new() { Timeout = 10000 });

        await Page.Locator("#submit-profile").ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.Multiple(() =>
        {
            Assert.That(body, Does.Contain("Name"),          "NativeTextBox field");
            Assert.That(body, Does.Contain("CareLevelId"),   "FusionDropDownList field");
            Assert.That(body, Does.Contain("AdmissionDate"), "FusionDatePicker field");
            Assert.That(body, Does.Contain("MonthlyRate"),   "FusionNumericTextBox field");
            Assert.That(body, Does.Contain("Nickname"),      "FusionInPlaceEditor · Text");
            Assert.That(body, Does.Contain("DateOfBirth"),   "FusionInPlaceEditor · Date");
            Assert.That(body, Does.Contain("Allergies"),     "FusionInPlaceEditor · DropDownList");
        });

        await Expect(Page.Locator("#profile-status"))
            .ToContainTextAsync("Saved", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
