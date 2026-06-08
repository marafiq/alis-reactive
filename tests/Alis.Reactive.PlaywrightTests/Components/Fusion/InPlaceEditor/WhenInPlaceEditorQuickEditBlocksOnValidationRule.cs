namespace Alis.Reactive.PlaywrightTests.Components.Fusion.InPlaceEditor;

// MonthlyRate validation uses the framework field slot for client-declared rules and server-only errors.
[TestFixture]
public class WhenInPlaceEditorQuickEditBlocksOnValidationRule : PlaywrightTestBase
{
    private const string PagePath = "/Sandbox/Components/FusionInPlaceEditor";
    private const string MonthlyRateCardId = "card-monthly-rate";
    private const string MonthlyRateEditorId = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_MonthlyRateQuickEdit__Value";
    private const string UpdateMonthlyRateEndpoint = "/Sandbox/Components/FusionInPlaceEditor/UpdateMonthlyRate";
    private const string ServerOnlyDuplicateRate = "7777";

    [Test]
    public async Task entering_zero_aborts_post_via_fluent_validation()
    {
        await NavigateToAndWaitForVisibleSignal(PagePath, $"#{MonthlyRateCardId} .e-editable-value-wrapper");

        await Page.Locator($"#{MonthlyRateCardId} .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator($"#{MonthlyRateCardId} input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 10000 });
        await inner.FillAsync("0");

        var postFired = false;
        _ = Page.WaitForRequestAsync(matchingRequest =>
            matchingRequest.Url.Contains(UpdateMonthlyRateEndpoint)
            && matchingRequest.Method == "POST",
            new() { Timeout = 2500 })
            .ContinueWith(t => { postFired = !t.IsFaulted && t.Result != null; return t; });

        await inner.PressAsync("Enter");

        // TODO: Replace this fixed negative-request wait with a behavior-focused no-POST proof.
        await Page.WaitForTimeoutAsync(2800);
        Assert.That(postFired, Is.False, "FluentValidator must abort the POST when Value violates GreaterThan(0).");

        // Field errors render in the framework {elementId}_error slot, not Syncfusion validationRules.
        var errorSlot = Page.Locator($"#{MonthlyRateEditorId}_error");
        await Expect(errorSlot).Not.ToBeEmptyAsync(new() { Timeout = 3000 });
    }

    [Test]
    public async Task entering_server_only_duplicate_rate_surfaces_server_400_in_same_field_slot()
    {
        await NavigateToAndWaitForVisibleSignal(PagePath, $"#{MonthlyRateCardId} .e-editable-value-wrapper");

        await Page.Locator($"#{MonthlyRateCardId} .e-editable-value-wrapper").First.ClickAsync();
        var inner = Page.Locator($"#{MonthlyRateCardId} input.e-control").First;
        await Expect(inner).ToBeVisibleAsync(new() { Timeout = 10000 });
        await inner.FillAsync(ServerOnlyDuplicateRate);

        var requestTask = Page.WaitForRequestAsync(matchingRequest =>
            matchingRequest.Url.Contains(UpdateMonthlyRateEndpoint)
            && matchingRequest.Method == "POST",
            new() { Timeout = 10000 });

        await inner.PressAsync("Enter");

        var request = await requestTask;
        Assert.That(request.PostData ?? "", Does.Contain(ServerOnlyDuplicateRate),
            "Client-declared rules must pass so the commit reaches the server.");

        var errorSlot = Page.Locator($"#{MonthlyRateEditorId}_error");
        await Expect(errorSlot).ToContainTextAsync("server-only check", new() { Timeout = 5000 });
    }
}
