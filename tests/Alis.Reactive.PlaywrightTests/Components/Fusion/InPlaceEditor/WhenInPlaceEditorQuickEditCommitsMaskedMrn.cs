namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

[TestFixture]
public class WhenInPlaceEditorQuickEditCommitsMaskedMrn : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionInPlaceEditor";

    [Test]
    public async Task committing_seeded_mrn_posts_raw_value_and_updates_display()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#card-mrn .e-editable-value-wrapper");

        // Syncfusion Mask stores raw characters in value; LLL-0000 is display formatting.
        await Expect(Page.Locator("#card-mrn .e-editable-value").First)
            .ToHaveTextAsync("MRN1234", new() { Timeout = 5000 });

        await Page.Locator("#card-mrn .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator("#card-mrn input.e-maskedtextbox").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Inside the editor, the mask formatter runs and the input shows MRN-1234.
        await Expect(inner).ToHaveValueAsync("MRN-1234", new() { Timeout = 3000 });

        var requestTask = Page.WaitForRequestAsync(matchingRequest =>
            matchingRequest.Url.Contains("/Sandbox/Components/FusionInPlaceEditor/UpdateMedicalRecordNumber")
            && matchingRequest.Method == "POST",
            new() { Timeout = 10000 });

        await inner.PressAsync("Enter");

        var request = await requestTask;
        var body = request.PostData ?? "";
        Assert.That(body, Does.Contain("MRN1234"),
            "POST body must carry the raw (mask-literal-stripped) value");
        Assert.That(body, Does.Not.Contain("MRN-1234"),
            "Syncfusion Mask strips literals on commit; the wire payload must not include the dash");
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

        await inner.FillAsync("");
        await inner.PressSequentiallyAsync("AB1234");

        var requestFired = false;
        void OnRequest(object? _, IRequest observedRequest)
        {
            if (observedRequest.Url.Contains("/UpdateMedicalRecordNumber"))
                requestFired = true;
        }
        Page.Request += OnRequest;
        try
        {
            await inner.PressAsync("Enter");
            // TODO: Replace this fixed negative-request wait with a behavior-focused no-POST proof.
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
