namespace Alis.Reactive.PlaywrightTests.Validation.NestedBugs;

/// <summary>
/// E2E tests proving nested client validation rules match server-side behavior.
/// </summary>
[TestFixture]
public class WhenNestedValidationRulesBind : PlaywrightTestBase
{
    // ── Nested condition field carries full path ───────────────────────

    private const string NestedConditionPath = "/Sandbox/Validation/NestedBugs/NestedCondition";
    private const string R1 = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Validation_NestedBugs_NestedAddressModel__";

    [Test]
    public async Task nested_condition_skips_rule_when_city_empty()
    {
        // City is empty → WhenFieldNotEmpty(City) is false → ConfirmCity NOT required
        await NavigateToAndWaitForBoot(NestedConditionPath);

        await Page.Locator($"#{R1}Name").FillAsync("Jane");
        // Leave City empty, leave ConfirmCity empty
        await ClickWhenStable(Page.Locator("#submit-btn"));

        // ConfirmCity "required when city is set" should NOT fire
        await Expect(Page.Locator($"#nested-form span[data-valmsg-for='Address.ConfirmCity']"))
            .Not.ToContainTextAsync("required when city");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task nested_condition_fires_rule_when_city_filled()
    {
        // City is filled → WhenFieldNotEmpty(City) is true → ConfirmCity IS required
        await NavigateToAndWaitForBoot(NestedConditionPath);

        await Page.Locator($"#{R1}Name").FillAsync("Jane");
        await Page.Locator($"#{R1}Address_City").FillAsync("Springfield");
        // Leave ConfirmCity empty
        await ClickWhenStable(Page.Locator("#submit-btn"));

        // ConfirmCity should show the conditional error
        await Expect(Page.Locator($"#nested-form span[data-valmsg-for='Address.ConfirmCity']"))
            .ToContainTextAsync("required when city");
        AssertNoConsoleErrors();
    }

    // ── Parent and child conditions compose ────────────────────────────

    private const string ParentChildPath = "/Sandbox/Validation/NestedBugs/ParentChild";
    private const string R2 = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Validation_NestedBugs_ParentChildModel__";

    [Test]
    public async Task parent_false_child_true_skips_validation()
    {
        // ParentFlag=false, ChildFlag=true → rules should NOT fire (parent condition blocks)
        await NavigateToAndWaitForBoot(ParentChildPath);

        // ParentFlag unchecked (default), check ChildFlag
        await Page.Locator($"#{R2}Child_ChildFlag").CheckAsync();
        // Leave ChildName empty
        await ClickWhenStable(Page.Locator("#submit-btn"));

        // No error for ChildName — parent condition is false
        await Expect(Page.Locator($"#parent-child-form span[data-valmsg-for='Child.ChildName']"))
            .Not.ToContainTextAsync("required");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task parent_true_child_false_skips_validation()
    {
        // ParentFlag=true, ChildFlag=false → rules should NOT fire (child condition blocks)
        await NavigateToAndWaitForBoot(ParentChildPath);

        await Page.Locator($"#{R2}ParentFlag").CheckAsync();
        // ChildFlag unchecked (default), leave ChildName empty
        await ClickWhenStable(Page.Locator("#submit-btn"));

        // No error for ChildName — child condition is false
        await Expect(Page.Locator($"#parent-child-form span[data-valmsg-for='Child.ChildName']"))
            .Not.ToContainTextAsync("required");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task both_flags_true_fires_validation()
    {
        // ParentFlag=true, ChildFlag=true → rules fire → ChildName required
        await NavigateToAndWaitForBoot(ParentChildPath);

        await Page.Locator($"#{R2}ParentFlag").CheckAsync();
        await Page.Locator($"#{R2}Child_ChildFlag").CheckAsync();
        // Leave ChildName empty
        await ClickWhenStable(Page.Locator("#submit-btn"));

        // Error should appear — both conditions true, field empty
        await Expect(Page.Locator($"#parent-child-form span[data-valmsg-for='Child.ChildName']"))
            .ToContainTextAsync("required");
        AssertNoConsoleErrors();
    }

    // ── Nested peer field carries full path ────────────────────────────

    [Test]
    public async Task nested_cross_property_validates_correct_peer()
    {
        // ConfirmCity must match City — the peer field reference must be "Address.City"
        await NavigateToAndWaitForBoot(NestedConditionPath);

        await Page.Locator($"#{R1}Name").FillAsync("Jane");
        await Page.Locator($"#{R1}Address_City").FillAsync("Springfield");
        await Page.Locator($"#{R1}Address_ConfirmCity").FillAsync("WrongCity");
        await ClickWhenStable(Page.Locator("#submit-btn"));

        // equalTo rule should fire — ConfirmCity != City
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

        // No error — values match
        await Expect(Page.Locator($"#nested-form span[data-valmsg-for='Address.ConfirmCity']"))
            .Not.ToContainTextAsync("must match");
        // Should succeed
        await Expect(Page.Locator("#result")).ToContainTextAsync("Saved");
        AssertNoConsoleErrors();
    }

    // ── Include inside WhenField carries condition ─────────────────────

    private const string IncludePath = "/Sandbox/Validation/NestedBugs/IncludeConditional";
    private const string R4 = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Validation_NestedBugs_IncludeModel__";

    [Test]
    public async Task not_employed_skips_included_rules()
    {
        // IsEmployed=false → Include(SharedEmploymentRules) should be skipped
        await NavigateToAndWaitForBoot(IncludePath);

        // IsEmployed unchecked (default), leave JobTitle + Department empty
        await ClickWhenStable(Page.Locator("#submit-btn"));

        // No errors — condition is false, included rules skipped
        await Expect(Page.Locator($"#include-form span[data-valmsg-for='JobTitle']"))
            .Not.ToContainTextAsync("required");
        await Expect(Page.Locator($"#include-form span[data-valmsg-for='Department']"))
            .Not.ToContainTextAsync("required");
        // Should succeed
        await Expect(Page.Locator("#result")).ToContainTextAsync("Saved");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task employed_fires_included_rules()
    {
        // IsEmployed=true → Include(SharedEmploymentRules) fires → JobTitle + Department required
        await NavigateToAndWaitForBoot(IncludePath);

        await Page.Locator($"#{R4}IsEmployed").CheckAsync();
        // Leave JobTitle + Department empty
        await ClickWhenStable(Page.Locator("#submit-btn"));

        // Both errors should appear
        await Expect(Page.Locator($"#include-form span[data-valmsg-for='JobTitle']"))
            .ToContainTextAsync("required");
        await Expect(Page.Locator($"#include-form span[data-valmsg-for='Department']"))
            .ToContainTextAsync("required");
        AssertNoConsoleErrors();
    }
}
