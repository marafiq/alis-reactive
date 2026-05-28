using System.Text.Json;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules;

[TestFixture]
public sealed class WhenComplexArrayItemRulesApply : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/AllRules";
    private const string ModelScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationShowcaseModel__";

    private ILocator FirstLineSku => Page.Locator($"#{ModelScope}Lines_0__Sku");
    private ILocator FirstLineSkuError => Page.Locator($"#{ModelScope}Lines_0__Sku_error");
    private ILocator FirstLineConfirmSku => Page.Locator($"#{ModelScope}Lines_0__ConfirmSku");
    private ILocator FirstLineConfirmSkuError => Page.Locator($"#{ModelScope}Lines_0__ConfirmSku_error");
    private ILocator FirstLineGiftWrapped => Page.Locator($"#{ModelScope}Lines_0__GiftWrapped");
    private ILocator FirstLineGiftNote => Page.Locator($"#{ModelScope}Lines_0__GiftNote");
    private ILocator FirstLineGiftNoteError => Page.Locator($"#{ModelScope}Lines_0__GiftNote_error");
    private ILocator SecondLineSku => Page.Locator($"#{ModelScope}Lines_1__Sku");
    private ILocator SecondLineSkuError => Page.Locator($"#{ModelScope}Lines_1__Sku_error");

    [Test]
    public async Task clientruleeach_setvalidator_projects_complex_item_rules_to_rendered_item_inputs()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(Page.Locator("#order-lines-validate-btn"));
        await Expect(FirstLineSkuError).ToContainTextAsync("Line SKU is required.");
        await Expect(SecondLineSkuError).ToContainTextAsync("Line SKU is required.");

        await FirstLineSku.FillAsync("RX-100");
        await FirstLineSku.BlurAsync();
        await Expect(FirstLineSkuError).ToBeHiddenAsync(new() { Timeout = 2000 });
        await Expect(SecondLineSkuError).ToContainTextAsync("Line SKU is required.");

        await FirstLineConfirmSku.FillAsync("WRONG");
        await ClickWhenStable(Page.Locator("#order-lines-validate-btn"));
        await Expect(FirstLineConfirmSkuError).ToContainTextAsync("Line SKU confirmation must match.");

        await FirstLineConfirmSku.FillAsync("RX-100");
        await FirstLineConfirmSku.BlurAsync();
        await Expect(FirstLineConfirmSkuError).ToBeHiddenAsync(new() { Timeout = 2000 });

        await FirstLineGiftWrapped.CheckAsync();
        await ClickWhenStable(Page.Locator("#order-lines-validate-btn"));
        await Expect(FirstLineGiftNoteError).ToContainTextAsync("Gift note is required when gift wrapped.");

        await FirstLineGiftNote.FillAsync("Rush wrap");
        await FirstLineGiftNote.BlurAsync();
        await Expect(FirstLineGiftNoteError).ToBeHiddenAsync(new() { Timeout = 2000 });
        await Expect(SecondLineSkuError).ToContainTextAsync("Line SKU is required.");

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        using var plan = JsonDocument.Parse(planJson!);
        var validations = plan.RootElement
            .GetProperty("components")
            .GetProperty("order-lines-form")
            .GetProperty("container")
            .GetProperty("validationRules")
            .EnumerateArray()
            .Where(rule => rule.GetProperty("serverFieldName").GetString() is
                "Lines[0].Sku" or "Lines[0].ConfirmSku" or "Lines[0].GiftNote" or
                "Lines[1].Sku" or "Lines[1].ConfirmSku" or "Lines[1].GiftNote")
            .OrderBy(rule => rule.GetProperty("serverFieldName").GetString())
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(validations.Select(rule => rule.GetProperty("serverFieldName").GetString()), Is.EqualTo(new[]
            {
                "Lines[0].ConfirmSku",
                "Lines[0].GiftNote",
                "Lines[0].Sku",
                "Lines[1].ConfirmSku",
                "Lines[1].GiftNote",
                "Lines[1].Sku"
            }));
            Assert.That(validations.Select(rule => rule.GetProperty("component").GetString()), Is.EqualTo(new[]
            {
                $"{ModelScope}Lines_0__ConfirmSku",
                $"{ModelScope}Lines_0__GiftNote",
                $"{ModelScope}Lines_0__Sku",
                $"{ModelScope}Lines_1__ConfirmSku",
                $"{ModelScope}Lines_1__GiftNote",
                $"{ModelScope}Lines_1__Sku"
            }));
            Assert.That(validations.Select(rule => rule.GetProperty("rules")[0].GetProperty("name").GetString()), Is.EqualTo(new[]
            {
                "equalTo",
                "required",
                "required",
                "equalTo",
                "required",
                "required"
            }));
            Assert.That(validations
                    .Where(rule => rule.GetProperty("serverFieldName").GetString()!.EndsWith("GiftNote", StringComparison.Ordinal))
                    .Select(rule => rule.GetProperty("rules")[0].GetProperty("execution").GetProperty("activation").GetProperty("kind").GetString()),
                Is.EqualTo(new[] { "when", "when" }));
        });
        AssertNoConsoleErrors();
    }
}
