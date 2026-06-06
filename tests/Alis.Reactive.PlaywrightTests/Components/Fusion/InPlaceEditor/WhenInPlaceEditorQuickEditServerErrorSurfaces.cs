using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

[TestFixture]
public class WhenInPlaceEditorQuickEditServerErrorSurfaces : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task committing_boom_surfaces_server_500_in_error_element()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-nickname-error .e-editable-value-wrapper");

        await Page.Locator("#card-nickname-error .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-nickname-error input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });
        await inner.FillAsync("boom");

        var requestTask = Page.WaitForRequestAsync(matchingRequest =>
            matchingRequest.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateNickname")
            && matchingRequest.Method == "POST",
            new() { Timeout = 10000 });

        await inner.PressAsync("Enter");

        var request = await requestTask;
        Assert.That(request.PostData ?? "", Does.Contain("resident-42"),
            "POST body must carry the ResidentId even on failure-bound requests");

        var response = await request.ResponseAsync();
        Assert.That(response!.Status, Is.EqualTo(500), "Endpoint must return 500 for the literal 'boom'");

        await Expect(Page.Locator("#card-nickname-error-message"))
            .ToContainTextAsync("Nickname rejected by server", new() { Timeout = 5000 });

        // HTTP 500 surfaces as a "Failed to load resource" console error; that is the
        // whole point of this scenario. Do not call AssertNoConsoleErrors() here.
    }
}
