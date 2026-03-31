namespace Alis.Reactive.PlaywrightTests.BugHunting;

/// <summary>
/// Bug-hunting tests for HTTP error handling edge cases.
///
/// Hypothesis: When a server returns a status code not covered by any OnError handler
/// (e.g., 500 when only OnError(400) is registered), the routeHandlers function silently
/// returns without executing any handler. This means WhileLoading commands (like showing
/// a spinner) are never reversed — the spinner stays visible permanently.
///
/// This is tested by intercepting network requests via Playwright route() to simulate
/// server errors. No source code changes needed — just test-level network simulation.
///
/// Page under test: /Sandbox/HttpPipeline/Http
/// </summary>
[TestFixture]
public class WhenHttpErrorsLeaveStaleUi : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/Http";

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForTextSignal(Path, "#load-first");
    }

    // ── Bug Hunt 1: WhileLoading spinner persists on unhandled 500 ──

    [Test]
    public async Task save_spinner_persists_when_server_returns_500()
    {
        // Section 2 (Save) has WhileLoading → show spinner.
        // OnSuccess hides spinner. OnError(400) hides spinner.
        // But there is NO OnError(500) handler.
        // If the server returns 500, the spinner should ideally be hidden,
        // but the current implementation has no fallback → spinner stays visible.
        await NavigateAndBoot();

        // Intercept the Save endpoint to return 500
        await Page.RouteAsync("**/HttpPipeline/Http/Save", async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 500,
                ContentType = "application/json",
                Body = "{\"error\": \"Internal Server Error\"}"
            });
        });

        // Spinner should be hidden initially
        await Expect(Page.Locator("#save-spinner")).ToBeHiddenAsync();

        // Click Save → WhileLoading fires → spinner shows → 500 response → no handler
        await Page.Locator("#save-btn").ClickAsync();

        // Wait for the HTTP request to complete
        await Page.WaitForTimeoutAsync(1000);

        // BUG CHECK: Is the spinner still visible?
        // Expected: spinner should be hidden (some error handling should occur)
        // Actual: spinner remains visible because no 500 handler hides it
        var spinnerVisible = await Page.Locator("#save-spinner").IsVisibleAsync();

        if (spinnerVisible)
        {
            TestContext.Out.WriteLine("[BUG FOUND] WhileLoading spinner persists after unhandled 500 error on Save endpoint");
        }

        // The spinner should NOT persist after an error — user sees a frozen loading state
        await Expect(Page.Locator("#save-spinner"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });
    }

    [Test]
    public async Task put_spinner_persists_when_server_returns_500()
    {
        // Section 5 (PUT) has same pattern: WhileLoading + OnSuccess + OnError(400)
        // No OnError(500) handler.
        await NavigateAndBoot();

        // Intercept PUT endpoint to return 500
        await Page.RouteAsync("**/HttpPipeline/Http/UpdateResident", async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 500,
                ContentType = "application/json",
                Body = "{\"error\": \"Internal Server Error\"}"
            });
        });

        await Expect(Page.Locator("#put-spinner")).ToBeHiddenAsync();

        // Click Update → WhileLoading → spinner shows → 500 → no handler
        await Page.Locator("#put-btn").ClickAsync();
        await Page.WaitForTimeoutAsync(1000);

        var spinnerVisible = await Page.Locator("#put-spinner").IsVisibleAsync();

        if (spinnerVisible)
        {
            TestContext.Out.WriteLine("[BUG FOUND] WhileLoading spinner persists after unhandled 500 error on PUT endpoint");
        }

        await Expect(Page.Locator("#put-spinner"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });
    }

    // ── Bug Hunt 2: Chain spinner persists if first request fails ──

    [Test]
    public async Task chain_spinner_persists_when_first_request_fails()
    {
        // Section 3 (Chained): WhileLoading shows spinner.
        // Spinner is hidden only in the CHAINED request's OnSuccess.
        // If the first request fails, the chain never fires → spinner stays.
        await NavigateAndBoot();

        // Intercept the Residents endpoint to return 500
        // (only for the chain request, not the DomReady GET)
        var requestCount = 0;
        await Page.RouteAsync("**/HttpPipeline/Http/Residents", async route =>
        {
            requestCount++;
            if (requestCount > 1) // Skip the DomReady GET (first request)
            {
                await route.FulfillAsync(new()
                {
                    Status = 500,
                    ContentType = "application/json",
                    Body = "{\"error\": \"Internal Server Error\"}"
                });
            }
            else
            {
                await route.ContinueAsync();
            }
        });

        // Click Chain button → first request fails → chain never fires → spinner stays
        await Page.Locator("#chain-btn").ClickAsync();
        await Page.WaitForTimeoutAsync(1500);

        var spinnerVisible = await Page.Locator("#chain-spinner").IsVisibleAsync();

        if (spinnerVisible)
        {
            TestContext.Out.WriteLine("[BUG FOUND] Chain spinner persists when first request returns 500 — chained OnSuccess never fires to hide it");
        }

        await Expect(Page.Locator("#chain-spinner"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });
    }

    // ── Bug Hunt 3: Network error leaves spinner visible ──

    [Test]
    public async Task save_spinner_persists_on_network_error()
    {
        // Network errors (e.g., connection refused) are caught by the try/catch
        // in execRequest and routed to onError handlers with status 0.
        // But OnError(400) won't match status 0 → no handler → spinner stays.
        await NavigateAndBoot();

        // Intercept Save and abort the request (simulates network error)
        await Page.RouteAsync("**/HttpPipeline/Http/Save", async route =>
        {
            await route.AbortAsync("connectionrefused");
        });

        await Page.Locator("#save-btn").ClickAsync();
        await Page.WaitForTimeoutAsync(1000);

        var spinnerVisible = await Page.Locator("#save-spinner").IsVisibleAsync();

        if (spinnerVisible)
        {
            TestContext.Out.WriteLine("[BUG FOUND] WhileLoading spinner persists after network error (connection refused) — no handler for status 0");
        }

        await Expect(Page.Locator("#save-spinner"))
            .ToBeHiddenAsync(new() { Timeout = 3000 });

        // Also verify no unhandled JS errors from the network failure
        AssertNoConsoleErrorsExcept("net::ERR_FAILED", "Failed to fetch", "NetworkError");
    }
}
