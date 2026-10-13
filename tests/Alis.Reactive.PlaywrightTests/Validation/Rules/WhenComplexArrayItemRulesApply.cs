namespace Alis.Reactive.PlaywrightTests.Validation.Rules;

/// <summary>
/// As a pharmacy coordinator entering an order, I want each line of the order, and each delivery of a
/// line, checked by its own rules, so that a mistake on one line is named on that line and a line
/// without the option is never asked for its details.
/// </summary>
/// <remarks>
/// The page's "Validate Lines" sends the lines to an endpoint that accepts anything, so every message
/// below is the browser's.
/// </remarks>
[TestFixture]
public sealed class WhenComplexArrayItemRulesApply : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/AllRules";
    private const string ModelScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationShowcaseModel__";

    private ILocator ValidateLines => Page.Locator("#order-lines-validate-btn");
    private ILocator Result => Page.Locator("#order-lines-result");

    private ILocator Input(int line, string field) => Page.Locator($"#{ModelScope}Lines_{line}__{field}");
    private ILocator Error(int line, string field) => Page.Locator($"#{ModelScope}Lines_{line}__{field}_error");
    private ILocator DeliveryInput(int line, string field) => Page.Locator($"#{ModelScope}Lines_{line}__Deliveries_0__{field}");
    private ILocator DeliveryError(int line, string field) => Page.Locator($"#{ModelScope}Lines_{line}__Deliveries_0__{field}_error");

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task every_line_needs_its_sku_and_filling_one_clears_only_its_own_message()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ValidateLines);
        await Expect(Error(0, "Sku")).ToContainTextAsync("Line SKU is required.");
        await Expect(Error(1, "Sku")).ToContainTextAsync("Line SKU is required.");
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await Input(0, "Sku").FillAsync("RX-100");
        await Input(0, "Sku").BlurAsync();
        await Expect(Error(0, "Sku")).ToBeHiddenAsync(new() { Timeout = 2000 });
        await Expect(Error(1, "Sku")).ToContainTextAsync("Line SKU is required.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_line_sku_confirmation_must_match_the_sku_of_its_own_line()
    {
        await NavigateAndBoot();
        await Input(1, "Sku").FillAsync("RX-200");
        await Input(1, "ConfirmSku").FillAsync("RX-201");

        await ClickWhenStable(ValidateLines);

        await Expect(Error(1, "ConfirmSku")).ToContainTextAsync("Line SKU confirmation must match.");
        await Expect(Error(0, "ConfirmSku")).ToBeHiddenAsync();

        await Input(1, "ConfirmSku").FillAsync("RX-200");
        await Input(1, "ConfirmSku").BlurAsync();
        await Expect(Error(1, "ConfirmSku")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_gift_note_is_asked_for_only_on_a_gift_wrapped_line()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ValidateLines);
        await Expect(Error(0, "Sku")).ToContainTextAsync("Line SKU is required.");
        await Expect(Error(0, "GiftNote")).ToBeHiddenAsync();
        await Expect(Error(1, "GiftNote")).ToBeHiddenAsync();

        await Input(1, "GiftWrapped").CheckAsync();
        await ClickWhenStable(ValidateLines);
        await Expect(Error(1, "GiftNote")).ToContainTextAsync("Gift note is required when gift wrapped.");
        await Expect(Error(0, "GiftNote")).ToBeHiddenAsync();

        await Input(1, "GiftNote").FillAsync("Rush wrap");
        await Input(1, "GiftNote").BlurAsync();
        await Expect(Error(1, "GiftNote")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task every_delivery_needs_its_location_and_filling_one_clears_only_its_own_message()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ValidateLines);
        await Expect(DeliveryError(0, "LocationCode")).ToContainTextAsync("Delivery location is required.");
        await Expect(DeliveryError(1, "LocationCode")).ToContainTextAsync("Delivery location is required.");

        await DeliveryInput(0, "LocationCode").FillAsync("MED-CART");
        await DeliveryInput(0, "LocationCode").BlurAsync();
        await Expect(DeliveryError(0, "LocationCode")).ToBeHiddenAsync(new() { Timeout = 2000 });
        await Expect(DeliveryError(1, "LocationCode")).ToContainTextAsync("Delivery location is required.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_access_code_is_asked_for_only_on_a_delivery_that_needs_one()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ValidateLines);
        await Expect(DeliveryError(0, "LocationCode")).ToContainTextAsync("Delivery location is required.");
        await Expect(DeliveryError(0, "AccessCode")).ToBeHiddenAsync();
        await Expect(DeliveryError(1, "AccessCode")).ToBeHiddenAsync();

        await DeliveryInput(1, "RequiresAccessCode").CheckAsync();
        await ClickWhenStable(ValidateLines);
        await Expect(DeliveryError(1, "AccessCode")).ToContainTextAsync("Access code is required when delivery requires it.");
        await Expect(DeliveryError(0, "AccessCode")).ToBeHiddenAsync();

        await DeliveryInput(1, "AccessCode").FillAsync("A-42");
        await DeliveryInput(1, "AccessCode").BlurAsync();
        await Expect(DeliveryError(1, "AccessCode")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_order_whose_lines_are_complete_passes()
    {
        await NavigateAndBoot();
        for (var line = 0; line < 2; line++)
        {
            await Input(line, "Sku").FillAsync($"RX-{line}00");
            await Input(line, "ConfirmSku").FillAsync($"RX-{line}00");
            await DeliveryInput(line, "LocationCode").FillAsync("MED-CART");
        }

        await ClickWhenStable(ValidateLines);

        await Expect(Result).ToHaveTextAsync("Line validation passed!");
        AssertNoConsoleErrors();
    }
}
