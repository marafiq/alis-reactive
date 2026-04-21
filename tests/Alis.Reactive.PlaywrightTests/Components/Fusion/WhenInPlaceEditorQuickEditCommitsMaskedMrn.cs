using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// MRN card: proves the Mask inner type commits cleanly with the raw (literal-stripped) value.
///
/// Syncfusion Mask stores only the user-entered characters in the outer <c>value</c> —
/// mask literals (the dash in <c>LLL-0000</c>) are a display formatter. The domain value,
/// the regex in the FluentValidator, and the wire payload all deal in the 7 raw characters.
/// </summary>
[TestFixture]
public class WhenInPlaceEditorQuickEditCommitsMaskedMrn : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task committing_seeded_mrn_posts_raw_value_and_updates_display()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-mrn .e-editable-value-wrapper");

        // SF Mask stores the raw characters in `.value` and applies the LLL-0000 format only
        // inside the live editor input — the collapsed display span shows the raw value as-is.
        await Expect(Page.Locator("#card-mrn .e-editable-value").First)
            .ToHaveTextAsync("MRN1234", new() { Timeout = 5000 });

        await Page.Locator("#card-mrn .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-mrn input.e-maskedtextbox").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Inside the editor, the mask formatter runs and the input shows MRN-1234.
        await Expect(inner).ToHaveValueAsync("MRN-1234", new() { Timeout = 3000 });

        var requestTask = Page.WaitForRequestAsync(req =>
            req.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateMedicalRecordNumber")
            && req.Method == "POST",
            new() { Timeout = 10000 });

        await inner.PressAsync("Enter");

        var request = await requestTask;
        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("MRN1234"),
            "POST body must carry the raw (mask-literal-stripped) value");
        Assert.That(body, Does.Not.Contain("MRN-1234"),
            "SF Mask strips literals on commit; the wire payload must not include the dash");
        Assert.That(body, Does.Contain("resident-42"),
            "POST body must carry the ResidentId identity from the hidden field");

        await Expect(Page.Locator("#card-mrn-display"))
            .Not.ToHaveTextAsync("unchanged", new() { Timeout = 5000 });
        await Expect(Page.Locator("#card-mrn-error")).ToHaveTextAsync("");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task committing_incomplete_mrn_shows_fluent_validator_format_error()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-mrn .e-editable-value-wrapper");

        await Page.Locator("#card-mrn .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-mrn input.e-maskedtextbox").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Type only 2 letters + 4 digits — fails the ^[A-Z]{3}\d{4}$ rule.
        await inner.FillAsync("");
        await inner.PressSequentiallyAsync("AB1234");

        var requestFired = false;
        void OnRequest(object? _, IRequest req)
        {
            if (req.Url.Contains("/UpdateMedicalRecordNumber"))
                requestFired = true;
        }
        Page.Request += OnRequest;
        try
        {
            await inner.PressAsync("Enter");
            await Page.WaitForTimeoutAsync(1000);
        }
        finally
        {
            Page.Request -= OnRequest;
        }

        Assert.That(requestFired, Is.False,
            "FluentValidator must block the POST when the raw value doesn't match ^[A-Z]{3}\\d{4}$");

        var externalErrorSlot = Page.Locator("#Alis_Reactive_SandboxApp_Areas_Sandbox_Models_MedicalRecordNumberQuickEdit__Value_error");
        await Expect(externalErrorSlot).Not.ToHaveTextAsync("", new() { Timeout = 3000 });
    }
}
