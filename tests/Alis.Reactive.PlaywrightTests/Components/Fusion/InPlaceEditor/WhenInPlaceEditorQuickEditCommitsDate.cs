using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

[TestFixture]
public class WhenInPlaceEditorQuickEditCommitsDate : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task typing_date_and_pressing_enter_posts_and_updates_display()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-dob .e-editable-value-wrapper");

        await Page.Locator("#card-dob .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-dob input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });
        await inner.FillAsync("5/15/2000");

        var requestTask = Page.WaitForRequestAsync(matchingRequest =>
            matchingRequest.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateDateOfBirth")
            && matchingRequest.Method == "POST",
            new() { Timeout = 10000 });

        // Enter keydown follows Syncfusion's submit path more reliably than a synthetic save-button click.
        await inner.PressAsync("Enter");

        var request = await requestTask;
        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("2000-05-15").Or.Contain("5/15/2000"),
            "POST body must carry the committed date");
        Assert.That(body, Does.Contain("resident-42"),
            "POST body must carry the ResidentId identity from the hidden field");

        await Expect(Page.Locator("#card-dob-display"))
            .Not.ToHaveTextAsync("unchanged", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
