using System.Text.Json;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules;

[TestFixture]
public sealed class WhenComplexArrayItemRulesApply : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/AllRules";
    private const string ModelScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ValidationShowcaseModel__";

    private ILocator FirstLineSku => Page.Locator($"#{ModelScope}Lines_0__Sku");
    private ILocator FirstLineSkuError => Page.Locator($"#{ModelScope}Lines_0__Sku_error");
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

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        using var plan = JsonDocument.Parse(planJson!);
        var validations = plan.RootElement
            .GetProperty("components")
            .GetProperty("order-lines-form")
            .GetProperty("container")
            .GetProperty("validationRules")
            .EnumerateArray()
            .Where(rule => rule.GetProperty("serverFieldName").GetString() is "Lines[0].Sku" or "Lines[1].Sku")
            .OrderBy(rule => rule.GetProperty("serverFieldName").GetString())
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(validations.Select(rule => rule.GetProperty("serverFieldName").GetString()), Is.EqualTo(new[] { "Lines[0].Sku", "Lines[1].Sku" }));
            Assert.That(validations.Select(rule => rule.GetProperty("component").GetString()), Is.EqualTo(new[] { $"{ModelScope}Lines_0__Sku", $"{ModelScope}Lines_1__Sku" }));
            Assert.That(validations.Select(rule => rule.GetProperty("rules")[0].GetProperty("name").GetString()), Is.EqualTo(new[] { "required", "required" }));
        });
        AssertNoConsoleErrors();
    }
}
