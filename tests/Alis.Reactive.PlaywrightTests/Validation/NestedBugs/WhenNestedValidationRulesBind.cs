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

        // With no city there is nothing to confirm: the page sends the form and the server saves it.
        await Expect(Page.Locator("#result")).ToHaveTextAsync("Saved");
        await Expect(Page.Locator($"#nested-form span[data-valmsg-for='Address.ConfirmCity']")).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task nested_condition_fires_rule_when_city_filled()
    {
        await NavigateToAndWaitForBoot(NestedConditionPath);

        await Page.Locator($"#{R1}Name").FillAsync("Jane");
        await Page.Locator($"#{R1}Address_City").FillAsync("Springfield");
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#nested-form span[data-valmsg-for='Address.ConfirmCity']"))
            .ToHaveTextAsync("Confirm city is required when city is set.");
        await Expect(Page.Locator("#result")).ToHaveTextAsync("");
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

        // The child's condition holds, but the parent's does not: nothing is asked for and the form is saved.
        await Expect(Page.Locator("#result")).ToHaveTextAsync("Saved");
        await Expect(Page.Locator($"#parent-child-form span[data-valmsg-for='Child.ChildName']")).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task parent_true_child_false_skips_validation()
    {
        await NavigateToAndWaitForBoot(ParentChildPath);

        await Page.Locator($"#{R2}ParentFlag").CheckAsync();
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator("#result")).ToHaveTextAsync("Saved");
        await Expect(Page.Locator($"#parent-child-form span[data-valmsg-for='Child.ChildName']")).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task both_flags_true_fires_validation()
    {
        await NavigateToAndWaitForBoot(ParentChildPath);

        await Page.Locator($"#{R2}ParentFlag").CheckAsync();
        await Page.Locator($"#{R2}Child_ChildFlag").CheckAsync();
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#parent-child-form span[data-valmsg-for='Child.ChildName']"))
            .ToHaveTextAsync("Child name required when child flag is checked.");
        await Expect(Page.Locator("#result")).ToHaveTextAsync("");
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
            .ToHaveTextAsync("Confirm city must match city.");
        await Expect(Page.Locator("#result")).ToHaveTextAsync("");
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

        await Expect(Page.Locator("#result")).ToHaveTextAsync("Saved");
        await Expect(Page.Locator($"#nested-form span[data-valmsg-for='Address.ConfirmCity']")).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    private const string IncludePath = "/Sandbox/Validation/NestedBugs/IncludeConditional";
    private const string R4 = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Validation_NestedBugs_IncludeModel__";

    [Test]
    public async Task not_employed_skips_included_rules()
    {
        await NavigateToAndWaitForBoot(IncludePath);

        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator("#result")).ToHaveTextAsync("Saved");
        await Expect(Page.Locator($"#include-form span[data-valmsg-for='JobTitle']")).ToHaveTextAsync("");
        await Expect(Page.Locator($"#include-form span[data-valmsg-for='Department']")).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task employed_fires_included_rules()
    {
        await NavigateToAndWaitForBoot(IncludePath);

        await Page.Locator($"#{R4}IsEmployed").CheckAsync();
        await ClickWhenStable(Page.Locator("#submit-btn"));

        await Expect(Page.Locator($"#include-form span[data-valmsg-for='JobTitle']"))
            .ToHaveTextAsync("Job title is required.");
        await Expect(Page.Locator($"#include-form span[data-valmsg-for='Department']"))
            .ToHaveTextAsync("Department is required.");
        await Expect(Page.Locator("#result")).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }
}
