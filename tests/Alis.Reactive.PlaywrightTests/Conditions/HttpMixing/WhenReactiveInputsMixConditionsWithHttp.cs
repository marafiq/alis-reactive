using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.HttpMixing;

namespace Alis.Reactive.PlaywrightTests.Conditions.HttpMixing;

/// <summary>
/// Proves component .Reactive() pipelines can mix conditions with HTTP success
/// and error routes while recomputing page-visible branch state after each input change.
/// </summary>
[TestFixture]
public class WhenReactiveInputsMixConditionsWithHttp : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/HttpMixing";

    private PagePlan<HttpMixingModel> _plan = null!;

    private NativeTextBoxLocator ReactiveResidentName => _plan.TextBox(m => m.ReactiveResidentName);
    private NativeTextBoxLocator ReactiveFollowUpName => _plan.TextBox(m => m.ReactiveFollowUpName);
    private NativeTextBoxLocator ReactiveErrorCategory => _plan.TextBox(m => m.ReactiveErrorCategory);

    private ILocator ReactiveResidentPre => Page.Locator("#s9-pre");
    private ILocator ReactiveResidentHttpResult => Page.Locator("#s9-http-result");
    private ILocator ReactiveResidentBadge => Page.Locator("#s9-badge");
    private ILocator ReactiveResidentTail => Page.Locator("#s9-tail");

    private ILocator ReactiveSuccessPre => Page.Locator("#s10-pre");
    private ILocator ReactiveSuccessHttpResult => Page.Locator("#s10-http-result");
    private ILocator ReactiveSuccessClassification => Page.Locator("#s10-classification");
    private ILocator ReactiveSuccessTail => Page.Locator("#s10-tail");

    private ILocator ReactiveErrorPre => Page.Locator("#s11-pre");
    private ILocator ReactiveErrorStatus => Page.Locator("#s11-status");
    private ILocator ReactiveErrorMessage => Page.Locator("#s11-error-msg");
    private ILocator ReactiveErrorTail => Page.Locator("#s11-tail");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#s1-btn-active");
        _plan = await PagePlan<HttpMixingModel>.FromPage(Page);
    }

    private async Task AssertVipResidentPathAsync(string expectedName)
    {
        await Expect(ReactiveResidentPre).ToHaveTextAsync("saving");
        await Expect(ReactiveResidentHttpResult).ToHaveTextAsync(expectedName, new() { Timeout = 5000 });
        await Expect(ReactiveResidentBadge).ToBeVisibleAsync();
        await Expect(ReactiveResidentTail).ToHaveTextAsync("complete");
    }

    private async Task AssertStandardResidentPathAsync(string expectedName)
    {
        await Expect(ReactiveResidentPre).ToHaveTextAsync("saving");
        await Expect(ReactiveResidentHttpResult).ToHaveTextAsync(expectedName, new() { Timeout = 5000 });
        await Expect(ReactiveResidentBadge).ToBeHiddenAsync();
        await Expect(ReactiveResidentTail).ToHaveTextAsync("complete");
    }

    private async Task AssertLongSuccessPathAsync(string expectedName)
    {
        await Expect(ReactiveSuccessPre).ToHaveTextAsync("loading");
        await Expect(ReactiveSuccessHttpResult).ToHaveTextAsync(expectedName, new() { Timeout = 5000 });
        await Expect(ReactiveSuccessClassification).ToHaveTextAsync("long name");
        await Expect(ReactiveSuccessTail).ToHaveTextAsync("success complete");
    }

    private async Task AssertShortSuccessPathAsync(string expectedName)
    {
        await Expect(ReactiveSuccessPre).ToHaveTextAsync("loading");
        await Expect(ReactiveSuccessHttpResult).ToHaveTextAsync(expectedName, new() { Timeout = 5000 });
        await Expect(ReactiveSuccessClassification).ToHaveTextAsync("short name");
        await Expect(ReactiveSuccessTail).ToHaveTextAsync("success complete");
    }

    private async Task AssertSpecificErrorPathAsync()
    {
        await Expect(ReactiveErrorPre).ToHaveTextAsync("validating");
        await Expect(ReactiveErrorStatus).ToHaveTextAsync("failed", new() { Timeout = 5000 });
        await Expect(ReactiveErrorMessage).ToHaveTextAsync("missing required fields", new() { Timeout = 5000 });
        await Expect(ReactiveErrorTail).ToHaveTextAsync("error complete");
    }

    private async Task AssertGenericErrorPathAsync()
    {
        await Expect(ReactiveErrorPre).ToHaveTextAsync("validating");
        await Expect(ReactiveErrorStatus).ToHaveTextAsync("failed", new() { Timeout = 5000 });
        await Expect(ReactiveErrorMessage).ToHaveTextAsync("validation error", new() { Timeout = 5000 });
        await Expect(ReactiveErrorTail).ToHaveTextAsync("error complete");
    }

    [Test]
    public async Task vip_resident_names_keep_the_outer_condition_true_after_the_http_round_trip()
    {
        await NavigateAndBoot();

        await ReactiveResidentName.FillAndBlur("VIP Alice");

        await AssertVipResidentPathAsync("VIP Alice");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task standard_resident_names_clear_the_outer_condition_after_the_http_round_trip()
    {
        await NavigateAndBoot();

        await ReactiveResidentName.FillAndBlur("Bob");

        await AssertStandardResidentPathAsync("Bob");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_the_reactive_name_recomputes_the_outer_condition_without_stale_vip_state()
    {
        await NavigateAndBoot();

        await ReactiveResidentName.FillAndBlur("VIP Alice");
        await AssertVipResidentPathAsync("VIP Alice");

        await ReactiveResidentName.FillAndBlur("Bob");
        await AssertStandardResidentPathAsync("Bob");

        await ReactiveResidentName.FillAndBlur("VIP Nora");
        await AssertVipResidentPathAsync("VIP Nora");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task long_follow_up_labels_take_the_long_name_branch_inside_on_success()
    {
        await NavigateAndBoot();

        await ReactiveFollowUpName.FillAndBlur("Morning rounds");

        await AssertLongSuccessPathAsync("Morning rounds");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task short_follow_up_labels_take_the_short_name_branch_inside_on_success()
    {
        await NavigateAndBoot();

        await ReactiveFollowUpName.FillAndBlur("OT");

        await AssertShortSuccessPathAsync("OT");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_the_follow_up_label_recomputes_the_success_route_branch_without_stale_text()
    {
        await NavigateAndBoot();

        await ReactiveFollowUpName.FillAndBlur("Morning rounds");
        await AssertLongSuccessPathAsync("Morning rounds");

        await ReactiveFollowUpName.FillAndBlur("OT");
        await AssertShortSuccessPathAsync("OT");

        await ReactiveFollowUpName.FillAndBlur("Evening meds");
        await AssertLongSuccessPathAsync("Evening meds");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task required_categories_take_the_specific_branch_inside_on_error()
    {
        await NavigateAndBoot();

        await ReactiveErrorCategory.FillAndBlur("required");

        await AssertSpecificErrorPathAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task non_required_categories_fall_back_to_the_generic_branch_inside_on_error()
    {
        await NavigateAndBoot();

        await ReactiveErrorCategory.FillAndBlur("format");

        await AssertGenericErrorPathAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task changing_the_error_category_recomputes_the_error_route_branch_without_stale_messages()
    {
        await NavigateAndBoot();

        await ReactiveErrorCategory.FillAndBlur("required");
        await AssertSpecificErrorPathAsync();

        await ReactiveErrorCategory.FillAndBlur("format");
        await AssertGenericErrorPathAsync();

        await ReactiveErrorCategory.FillAndBlur("required");
        await AssertSpecificErrorPathAsync();
        AssertNoConsoleErrorsExcept("400");
    }
}
