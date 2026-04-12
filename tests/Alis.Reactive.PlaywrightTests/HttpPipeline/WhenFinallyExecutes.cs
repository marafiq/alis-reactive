namespace Alis.Reactive.PlaywrightTests.HttpPipeline;

/// <summary>
/// Verifies that .Finally() cleanup commands execute after HTTP requests
/// regardless of success, error, or network failure.
/// </summary>
[TestFixture]
public class WhenFinallyExecutes : PlaywrightTestBase
{
    private Task ClickButton(string name) =>
        ClickWhenStable(Page.GetByRole(AriaRole.Button, new() { Name = name }));

    private async Task NavigateToHttpPage()
    {
        await NavigateToAndWaitForTextSignal(
            "/Sandbox/HttpPipeline/Http",
            "#load-first");
    }

    // ── Finally on Success ──────────────────────────────────

    [Test]
    public async Task finally_hides_spinner_after_successful_save()
    {
        await NavigateToHttpPage();

        await ClickButton("Save (Success)");

        await Expect(Page.Locator("#finally-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#finally-result")).ToHaveTextAsync("Saved successfully!");
        AssertNoConsoleErrors();
    }

    // ── Finally on Error ────────────────────────────────────

    [Test]
    public async Task finally_hides_spinner_after_server_500()
    {
        await NavigateToHttpPage();

        await ClickButton("Save (Fail 500)");

        await Expect(Page.Locator("#finally-spinner")).ToBeHiddenAsync();
        await Expect(Page.Locator("#finally-result"))
            .ToContainTextAsync("Server error (500)");
        AssertNoConsoleErrorsExcept("500", "http");
    }

    // ── Finally on Unhandled Status (THE BUG from issue #88) ──

    [Test]
    public async Task finally_hides_spinner_when_no_error_handler_matches()
    {
        await NavigateToHttpPage();

        await Page.RouteAsync("**/Sandbox/HttpPipeline/Http/FlakyEndpoint", async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 502,
                ContentType = "application/json",
                Body = "{\"error\": \"Bad Gateway\"}"
            });
        });

        await ClickButton("Save (Success)");

        await Expect(Page.Locator("#finally-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept("502", "http");
    }

    // ── Finally on Network Error ────────────────────────────

    [Test]
    public async Task finally_hides_spinner_on_network_error()
    {
        await NavigateToHttpPage();

        await Page.RouteAsync("**/Sandbox/HttpPipeline/Http/FlakyEndpoint", async route =>
        {
            await route.AbortAsync("connectionrefused");
        });

        await ClickButton("Save (Success)");

        await Expect(Page.Locator("#finally-spinner")).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept("network", "TypeError", "http", "ERR_");
    }
}
