using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

[TestFixture]
public class WhenInPlaceEditorQuickEditCommitsDropDown : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task selecting_a_new_option_and_pressing_enter_posts_and_updates_display()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-care-level .e-editable-value-wrapper");

        await Page.Locator("#card-care-level .e-editable-value-wrapper").First.ClickAsync();

        // Syncfusion DropDownList: the inner input is tabindex="-1" readonly; the
        // parent wrapper owns the click handler that opens the popup.
        var dropDownWrap = Page.Locator("#card-care-level .e-input-group").First;
        await Expect(dropDownWrap).ToBeVisibleAsync(new() { Timeout = 10000 });
        await dropDownWrap.ClickAsync(new() { Force = true });

        var item = Page.Locator("li.e-list-item", new() { HasTextString = "Memory Care" }).First;
        await Expect(item).ToBeVisibleAsync(new() { Timeout = 5000 });
        await item.ClickAsync();

        var requestTask = Page.WaitForRequestAsync(matchingRequest =>
            matchingRequest.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateCareLevel")
            && matchingRequest.Method == "POST",
            new() { Timeout = 10000 });

        // Save button is the user commit path for this inner editor type.
        await Page.Locator("#card-care-level .e-btn-save").First.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("memory-care"),
            "POST body must carry the chosen care-level id");
        Assert.That(body, Does.Contain("resident-42"),
            "POST body must carry the ResidentId from the hidden field");

        await Expect(Page.Locator("#card-care-level-display"))
            .ToContainTextAsync("Memory Care", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
