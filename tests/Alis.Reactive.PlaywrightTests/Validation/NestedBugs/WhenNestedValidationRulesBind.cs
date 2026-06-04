namespace Alis.Reactive.PlaywrightTests.Validation.NestedBugs;

[TestFixture]
public class WhenNestedValidationRulesBind : PlaywrightTestBase
{
    private const string NestedConditionPath = "/Sandbox/Validation/NestedBugs/NestedCondition";
    private const string R1 = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Validation_NestedBugs_NestedAddressModel__";

    [Test]
    public async Task nested_condition_skips_rule_when_city_empty()
    {
        await NavigateToAndWaitForBoot(NestedConditionPath);

        await Page.Locator($"#{R1}Name").FillAsync("Jane");
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#nested-form span[data-valmsg-for='Address.ConfirmCity']"))
            .Not.ToContainTextAsync("required when city");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task nested_condition_fires_rule_when_city_filled()
    {
        await NavigateToAndWaitForBoot(NestedConditionPath);

        await Page.Locator($"#{R1}Name").FillAsync("Jane");
        await Page.Locator($"#{R1}Address_City").FillAsync("Springfield");
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#nested-form span[data-valmsg-for='Address.ConfirmCity']"))
            .ToContainTextAsync("required when city");
        AssertNoConsoleErrors();
    }

    private const string ParentChildPath = "/Sandbox/Validation/NestedBugs/ParentChild";
    private const string R2 = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Validation_NestedBugs_ParentChildModel__";

    [Test]
    public async Task parent_false_child_true_skips_validation()
    {
        await NavigateToAndWaitForBoot(ParentChildPath);

        await Page.Locator($"#{R2}Child_ChildFlag").CheckAsync();
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#parent-child-form span[data-valmsg-for='Child.ChildName']"))
            .Not.ToContainTextAsync("required");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task parent_true_child_false_skips_validation()
    {
        await NavigateToAndWaitForBoot(ParentChildPath);

        await Page.Locator($"#{R2}ParentFlag").CheckAsync();
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#parent-child-form span[data-valmsg-for='Child.ChildName']"))
            .Not.ToContainTextAsync("required");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task both_flags_true_fires_validation()
    {
        await NavigateToAndWaitForBoot(ParentChildPath);

        await Page.Locator($"#{R2}ParentFlag").CheckAsync();
        await Page.Locator($"#{R2}Child_ChildFlag").CheckAsync();
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#parent-child-form span[data-valmsg-for='Child.ChildName']"))
            .ToContainTextAsync("required");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_cross_property_validates_correct_peer()
    {
        await NavigateToAndWaitForBoot(NestedConditionPath);

        await Page.Locator($"#{R1}Name").FillAsync("Jane");
        await Page.Locator($"#{R1}Address_City").FillAsync("Springfield");
        await Page.Locator($"#{R1}Address_ConfirmCity").FillAsync("WrongCity");
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#nested-form span[data-valmsg-for='Address.ConfirmCity']"))
            .ToContainTextAsync("must match");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_cross_property_passes_when_matching()
    {
        await NavigateToAndWaitForBoot(NestedConditionPath);

        await Page.Locator($"#{R1}Name").FillAsync("Jane");
        await Page.Locator($"#{R1}Address_City").FillAsync("Springfield");
        await Page.Locator($"#{R1}Address_ConfirmCity").FillAsync("Springfield");
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#nested-form span[data-valmsg-for='Address.ConfirmCity']"))
            .Not.ToContainTextAsync("must match");
        await Expect(Page.Locator("#result")).ToContainTextAsync("Saved");
        AssertNoConsoleErrors();
    }

    private const string IncludePath = "/Sandbox/Validation/NestedBugs/IncludeConditional";
    private const string R4 = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Validation_NestedBugs_IncludeModel__";

    [Test]
    public async Task not_employed_skips_included_rules()
    {
        await NavigateToAndWaitForBoot(IncludePath);

        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#include-form span[data-valmsg-for='JobTitle']"))
            .Not.ToContainTextAsync("required");
        await Expect(Page.Locator($"#include-form span[data-valmsg-for='Department']"))
            .Not.ToContainTextAsync("required");
        await Expect(Page.Locator("#result")).ToContainTextAsync("Saved");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task employed_fires_included_rules()
    {
        await NavigateToAndWaitForBoot(IncludePath);

        await Page.Locator($"#{R4}IsEmployed").CheckAsync();
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#include-form span[data-valmsg-for='JobTitle']"))
            .ToContainTextAsync("required");
        await Expect(Page.Locator($"#include-form span[data-valmsg-for='Department']"))
            .ToContainTextAsync("required");
        AssertNoConsoleErrors();
    }
}
