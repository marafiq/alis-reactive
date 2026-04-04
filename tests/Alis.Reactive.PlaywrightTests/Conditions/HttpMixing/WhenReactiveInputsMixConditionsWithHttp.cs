using Alis.Reactive.PlaywrightTests.Support.Controls;

namespace Alis.Reactive.PlaywrightTests.Conditions.HttpMixing;

[TestFixture]
public class WhenReactiveInputsMixConditionsWithHttp : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/HttpMixing";
    private HttpMixingPage _page = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#s1-btn-active");
        _page = new HttpMixingPage(Page);
    }

    private async Task AssertVipResidentPathAsync(string expectedName)
    {
        await Expect(_page.ReactiveResidentPre).ToHaveTextAsync("saving");
        await Expect(_page.ReactiveResidentHttpResult).ToHaveTextAsync(expectedName, new() { Timeout = 5000 });
        await Expect(_page.ReactiveResidentBadge).ToBeVisibleAsync();
        await Expect(_page.ReactiveResidentTail).ToHaveTextAsync("complete");
    }

    private async Task AssertStandardResidentPathAsync(string expectedName)
    {
        await Expect(_page.ReactiveResidentPre).ToHaveTextAsync("saving");
        await Expect(_page.ReactiveResidentHttpResult).ToHaveTextAsync(expectedName, new() { Timeout = 5000 });
        await Expect(_page.ReactiveResidentBadge).ToBeHiddenAsync();
        await Expect(_page.ReactiveResidentTail).ToHaveTextAsync("complete");
    }

    private async Task AssertLongSuccessPathAsync(string expectedName)
    {
        await Expect(_page.ReactiveSuccessPre).ToHaveTextAsync("loading");
        await Expect(_page.ReactiveSuccessHttpResult).ToHaveTextAsync(expectedName, new() { Timeout = 5000 });
        await Expect(_page.ReactiveSuccessClassification).ToHaveTextAsync("long name");
        await Expect(_page.ReactiveSuccessTail).ToHaveTextAsync("success complete");
    }

    private async Task AssertShortSuccessPathAsync(string expectedName)
    {
        await Expect(_page.ReactiveSuccessPre).ToHaveTextAsync("loading");
        await Expect(_page.ReactiveSuccessHttpResult).ToHaveTextAsync(expectedName, new() { Timeout = 5000 });
        await Expect(_page.ReactiveSuccessClassification).ToHaveTextAsync("short name");
        await Expect(_page.ReactiveSuccessTail).ToHaveTextAsync("success complete");
    }

    private async Task AssertSpecificErrorPathAsync()
    {
        await Expect(_page.ReactiveErrorPre).ToHaveTextAsync("validating");
        await Expect(_page.ReactiveErrorStatus).ToHaveTextAsync("failed", new() { Timeout = 5000 });
        await Expect(_page.ReactiveErrorMessage).ToHaveTextAsync("missing required fields", new() { Timeout = 5000 });
        await Expect(_page.ReactiveErrorTail).ToHaveTextAsync("error complete");
    }

    private async Task AssertGenericErrorPathAsync()
    {
        await Expect(_page.ReactiveErrorPre).ToHaveTextAsync("validating");
        await Expect(_page.ReactiveErrorStatus).ToHaveTextAsync("failed", new() { Timeout = 5000 });
        await Expect(_page.ReactiveErrorMessage).ToHaveTextAsync("validation error", new() { Timeout = 5000 });
        await Expect(_page.ReactiveErrorTail).ToHaveTextAsync("error complete");
    }

    [Test]
    public async Task vip_resident_names_keep_the_outer_condition_true_after_the_http_round_trip()
    {
        await NavigateAndBoot();

        await _page.ReactiveResidentName.FillAndBlur("VIP Alice");

        await AssertVipResidentPathAsync("VIP Alice");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task standard_resident_names_clear_the_outer_condition_after_the_http_round_trip()
    {
        await NavigateAndBoot();

        await _page.ReactiveResidentName.FillAndBlur("Bob");

        await AssertStandardResidentPathAsync("Bob");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_the_reactive_name_recomputes_the_outer_condition_without_stale_vip_state()
    {
        await NavigateAndBoot();

        await _page.ReactiveResidentName.FillAndBlur("VIP Alice");
        await AssertVipResidentPathAsync("VIP Alice");

        await _page.ReactiveResidentName.FillAndBlur("Bob");
        await AssertStandardResidentPathAsync("Bob");

        await _page.ReactiveResidentName.FillAndBlur("VIP Nora");
        await AssertVipResidentPathAsync("VIP Nora");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task long_follow_up_labels_take_the_long_name_branch_inside_on_success()
    {
        await NavigateAndBoot();

        await _page.ReactiveFollowUpName.FillAndBlur("Morning rounds");

        await AssertLongSuccessPathAsync("Morning rounds");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task short_follow_up_labels_take_the_short_name_branch_inside_on_success()
    {
        await NavigateAndBoot();

        await _page.ReactiveFollowUpName.FillAndBlur("OT");

        await AssertShortSuccessPathAsync("OT");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task changing_the_follow_up_label_recomputes_the_success_handler_branch_without_stale_text()
    {
        await NavigateAndBoot();

        await _page.ReactiveFollowUpName.FillAndBlur("Morning rounds");
        await AssertLongSuccessPathAsync("Morning rounds");

        await _page.ReactiveFollowUpName.FillAndBlur("OT");
        await AssertShortSuccessPathAsync("OT");

        await _page.ReactiveFollowUpName.FillAndBlur("Evening meds");
        await AssertLongSuccessPathAsync("Evening meds");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task required_categories_take_the_specific_branch_inside_on_error()
    {
        await NavigateAndBoot();

        await _page.ReactiveErrorCategory.FillAndBlur("required");

        await AssertSpecificErrorPathAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task non_required_categories_fall_back_to_the_generic_branch_inside_on_error()
    {
        await NavigateAndBoot();

        await _page.ReactiveErrorCategory.FillAndBlur("format");

        await AssertGenericErrorPathAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task changing_the_error_category_recomputes_the_error_handler_branch_without_stale_messages()
    {
        await NavigateAndBoot();

        await _page.ReactiveErrorCategory.FillAndBlur("required");
        await AssertSpecificErrorPathAsync();

        await _page.ReactiveErrorCategory.FillAndBlur("format");
        await AssertGenericErrorPathAsync();

        await _page.ReactiveErrorCategory.FillAndBlur("required");
        await AssertSpecificErrorPathAsync();
        AssertNoConsoleErrorsExcept("400");
    }

    private sealed class HttpMixingPage
    {
        private const string Scope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Conditions_HttpMixing_HttpMixingModel__";
        private readonly IPage _page;

        public HttpMixingPage(IPage page)
        {
            _page = page;
        }

        public NativeTextBoxLocator ReactiveResidentName => new(_page, Scope + "ReactiveResidentName");
        public NativeTextBoxLocator ReactiveFollowUpName => new(_page, Scope + "ReactiveFollowUpName");
        public NativeTextBoxLocator ReactiveErrorCategory => new(_page, Scope + "ReactiveErrorCategory");

        public ILocator ReactiveResidentPre => _page.Locator("#s9-pre");
        public ILocator ReactiveResidentHttpResult => _page.Locator("#s9-http-result");
        public ILocator ReactiveResidentBadge => _page.Locator("#s9-badge");
        public ILocator ReactiveResidentTail => _page.Locator("#s9-tail");

        public ILocator ReactiveSuccessPre => _page.Locator("#s10-pre");
        public ILocator ReactiveSuccessHttpResult => _page.Locator("#s10-http-result");
        public ILocator ReactiveSuccessClassification => _page.Locator("#s10-classification");
        public ILocator ReactiveSuccessTail => _page.Locator("#s10-tail");

        public ILocator ReactiveErrorPre => _page.Locator("#s11-pre");
        public ILocator ReactiveErrorStatus => _page.Locator("#s11-status");
        public ILocator ReactiveErrorMessage => _page.Locator("#s11-error-msg");
        public ILocator ReactiveErrorTail => _page.Locator("#s11-tail");
    }
}
