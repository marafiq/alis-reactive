using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

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

    private ILocator ValidateLines => Page.Locator("#order-lines-validate-btn");
    private ILocator Result => Page.Locator("#order-lines-result");

    private ILocator Input(Expression<Func<ValidationShowcaseModel, object?>> member) => Page.Locator($"#{IdGenerator.For(member)}");
    private ILocator Error(Expression<Func<ValidationShowcaseModel, object?>> member) => Page.Locator($"#{IdGenerator.For(member)}_error");

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
        await Expect(Error(m => m.Lines[0].Sku)).ToHaveTextAsync("Line SKU is required.");
        await Expect(Error(m => m.Lines[1].Sku)).ToHaveTextAsync("Line SKU is required.");
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await Input(m => m.Lines[0].Sku).FillAsync("RX-100");
        await Input(m => m.Lines[0].Sku).BlurAsync();
        await Expect(Error(m => m.Lines[0].Sku)).ToBeHiddenAsync(new() { Timeout = 2000 });
        await Expect(Error(m => m.Lines[1].Sku)).ToHaveTextAsync("Line SKU is required.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_line_sku_confirmation_must_match_the_sku_of_its_own_line()
    {
        await NavigateAndBoot();
        await Input(m => m.Lines[1].Sku).FillAsync("RX-200");
        await Input(m => m.Lines[1].ConfirmSku).FillAsync("RX-201");

        await ClickWhenStable(ValidateLines);

        await Expect(Error(m => m.Lines[1].ConfirmSku)).ToHaveTextAsync("Line SKU confirmation must match.");
        await Expect(Error(m => m.Lines[0].ConfirmSku)).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await Input(m => m.Lines[1].ConfirmSku).FillAsync("RX-200");
        await Input(m => m.Lines[1].ConfirmSku).BlurAsync();
        await Expect(Error(m => m.Lines[1].ConfirmSku)).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_gift_note_is_asked_for_only_on_a_gift_wrapped_line()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ValidateLines);
        await Expect(Error(m => m.Lines[0].Sku)).ToHaveTextAsync("Line SKU is required.");
        await Expect(Error(m => m.Lines[0].GiftNote)).ToBeHiddenAsync();
        await Expect(Error(m => m.Lines[1].GiftNote)).ToBeHiddenAsync();

        await Input(m => m.Lines[1].GiftWrapped).CheckAsync();
        await ClickWhenStable(ValidateLines);
        await Expect(Error(m => m.Lines[1].GiftNote)).ToHaveTextAsync("Gift note is required when gift wrapped.");
        await Expect(Error(m => m.Lines[0].GiftNote)).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await Input(m => m.Lines[1].GiftNote).FillAsync("Rush wrap");
        await Input(m => m.Lines[1].GiftNote).BlurAsync();
        await Expect(Error(m => m.Lines[1].GiftNote)).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task every_delivery_needs_its_location_and_filling_one_clears_only_its_own_message()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ValidateLines);
        await Expect(Error(m => m.Lines[0].Deliveries[0].LocationCode)).ToHaveTextAsync("Delivery location is required.");
        await Expect(Error(m => m.Lines[1].Deliveries[0].LocationCode)).ToHaveTextAsync("Delivery location is required.");
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await Input(m => m.Lines[0].Deliveries[0].LocationCode).FillAsync("MED-CART");
        await Input(m => m.Lines[0].Deliveries[0].LocationCode).BlurAsync();
        await Expect(Error(m => m.Lines[0].Deliveries[0].LocationCode)).ToBeHiddenAsync(new() { Timeout = 2000 });
        await Expect(Error(m => m.Lines[1].Deliveries[0].LocationCode)).ToHaveTextAsync("Delivery location is required.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_access_code_is_asked_for_only_on_a_delivery_that_needs_one()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ValidateLines);
        await Expect(Error(m => m.Lines[0].Deliveries[0].LocationCode)).ToHaveTextAsync("Delivery location is required.");
        await Expect(Error(m => m.Lines[0].Deliveries[0].AccessCode)).ToBeHiddenAsync();
        await Expect(Error(m => m.Lines[1].Deliveries[0].AccessCode)).ToBeHiddenAsync();

        await Input(m => m.Lines[1].Deliveries[0].RequiresAccessCode).CheckAsync();
        await ClickWhenStable(ValidateLines);
        await Expect(Error(m => m.Lines[1].Deliveries[0].AccessCode)).ToHaveTextAsync("Access code is required when delivery requires it.");
        await Expect(Error(m => m.Lines[0].Deliveries[0].AccessCode)).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("Click to validate");

        await Input(m => m.Lines[1].Deliveries[0].AccessCode).FillAsync("A-42");
        await Input(m => m.Lines[1].Deliveries[0].AccessCode).BlurAsync();
        await Expect(Error(m => m.Lines[1].Deliveries[0].AccessCode)).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_order_whose_lines_are_complete_passes()
    {
        await NavigateAndBoot();
        for (var line = 0; line < 2; line++)
        {
            await Input(m => m.Lines[line].Sku).FillAsync($"RX-{line}00");
            await Input(m => m.Lines[line].ConfirmSku).FillAsync($"RX-{line}00");
            await Input(m => m.Lines[line].Deliveries[0].LocationCode).FillAsync("MED-CART");
        }

        await ClickWhenStable(ValidateLines);

        await Expect(Result).ToHaveTextAsync("Line validation passed!");
        AssertNoConsoleErrors();
    }
}
