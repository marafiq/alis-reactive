namespace Alis.Reactive.PlaywrightTests.BugHunting;

/// <summary>
/// Bug-hunting tests for rapid/double-click scenarios.
///
/// Hypothesis: The reactive runtime has no debounce mechanism for button clicks.
/// Each click fires a new reaction independently. Double-clicking a button that
/// triggers an HTTP POST will send two requests, potentially creating duplicate
/// records or causing race conditions in the response handlers.
///
/// In a senior living app, duplicate form submissions could create duplicate
/// resident records, billing entries, or care assessments — a critical data
/// integrity issue.
///
/// Page under test: /Sandbox/HttpPipeline/Http (Save button, Section 2)
/// </summary>
[TestFixture]
public class WhenButtonsAreClickedRapidly : PlaywrightTestBase
{
    private const string HttpPath = "/Sandbox/HttpPipeline/Http";

    private async Task NavigateAndBootHttp()
    {
        await NavigateToAndWaitForTextSignal(HttpPath, "#load-first");
    }

    // ── Bug Hunt 1: Double-click sends two HTTP requests ──

    [Test]
    public async Task double_click_on_save_sends_two_http_requests()
    {
        // Double-clicking a Save button should NOT send two requests.
        // If it does, the server could create duplicate records.
        await NavigateAndBootHttp();

        var requestCount = 0;
        await Page.RouteAsync("**/HttpPipeline/Http/Save", async route =>
        {
            requestCount++;
            await route.ContinueAsync();
        });

        // Double-click: two clicks in rapid succession
        var saveBtn = Page.Locator("#save-btn");
        await saveBtn.DblClickAsync();

        // Wait for both requests to complete
        await Page.WaitForTimeoutAsync(2000);

        if (requestCount > 1)
        {
            TestContext.Out.WriteLine(
                $"[BUG FOUND] Double-click on Save button sent {requestCount} HTTP requests — " +
                "no debounce protection. In production, this could create duplicate records.");
        }

        // Expected: only 1 request should be sent (framework should debounce)
        Assert.That(requestCount, Is.EqualTo(1),
            $"Double-click sent {requestCount} requests instead of 1 — no debounce protection");

        AssertNoConsoleErrors();
    }

    // ── Bug Hunt 2: Rapid clicks on search button ──

    [Test]
    public async Task rapid_clicks_on_search_sends_multiple_requests()
    {
        // Section 8: Search button. Three rapid clicks = three GET requests?
        await NavigateAndBootHttp();

        var requestCount = 0;
        await Page.RouteAsync("**/HttpPipeline/Http/Search**", async route =>
        {
            requestCount++;
            // Add artificial delay to simulate slow server
            await Task.Delay(500);
            await route.ContinueAsync();
        });

        // Click three times rapidly
        var searchBtn = Page.Locator("#search-btn");
        await searchBtn.ClickAsync(new() { Delay = 0 });
        await searchBtn.ClickAsync(new() { Delay = 0 });
        await searchBtn.ClickAsync(new() { Delay = 0 });

        // Wait for all requests to settle
        await Page.WaitForTimeoutAsync(3000);

        if (requestCount > 1)
        {
            TestContext.Out.WriteLine(
                $"[BUG FOUND] Three rapid clicks on Search sent {requestCount} HTTP requests — " +
                "no throttling. Last response wins but intermediate responses may flash stale data.");
        }

        // Expected: ideally only 1 request (latest) should be sent
        Assert.That(requestCount, Is.EqualTo(1),
            $"Rapid clicks sent {requestCount} requests instead of 1 — no throttling");

        AssertNoConsoleErrors();
    }

    // ── Bug Hunt 3: Parallel request with one failing ──

    [Test]
    public async Task parallel_with_one_failure_still_fires_on_all_settled()
    {
        // Section 4: Parallel requests. If one fails, OnAllSettled should still fire
        // and the spinner should be hidden.
        await NavigateAndBootHttp();

        // Intercept Facilities endpoint to return 500 (Residents continues normally)
        await Page.RouteAsync("**/HttpPipeline/Http/Facilities", async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 500,
                ContentType = "application/json",
                Body = "{\"error\": \"Server Error\"}"
            });
        });

        // Click Parallel button
        await Page.Locator("#parallel-btn").ClickAsync();

        // Wait for both requests to settle
        await Page.WaitForTimeoutAsync(2000);

        // OnAllSettled should fire — spinner hidden and "completed" text shown
        await Expect(Page.Locator("#parallel-spinner"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        await Expect(Page.Locator("#parallel-all"))
            .ToHaveTextAsync("All parallel requests completed!", new() { Timeout = 5000 });

        // The successful request (Residents) should still show data
        var residentFirst = await Page.Locator("#parallel-resident-first").TextContentAsync();
        Assert.That(residentFirst, Is.Not.EqualTo("\u2014"),
            "Successful parallel request should populate its data");

        // The failed request (Facilities) should NOT populate (no OnError handler)
        await Expect(Page.Locator("#parallel-facility-first"))
            .ToHaveTextAsync("\u2014", new() { Timeout = 1000 });

        AssertNoConsoleErrorsExcept("500", "Internal Server Error");
    }

    // ── Bug Hunt 4: Parallel with both failing ──

    [Test]
    public async Task parallel_with_both_failing_still_fires_on_all_settled()
    {
        // Both parallel requests fail. OnAllSettled should still fire.
        await NavigateAndBootHttp();

        var requestCount = 0;
        await Page.RouteAsync("**/HttpPipeline/Http/Residents", async route =>
        {
            requestCount++;
            if (requestCount > 1) // Skip DomReady GET
            {
                await route.FulfillAsync(new()
                {
                    Status = 500,
                    ContentType = "application/json",
                    Body = "{\"error\": \"Server Error\"}"
                });
            }
            else
            {
                await route.ContinueAsync();
            }
        });

        await Page.RouteAsync("**/HttpPipeline/Http/Facilities", async route =>
        {
            await route.FulfillAsync(new()
            {
                Status = 500,
                ContentType = "application/json",
                Body = "{\"error\": \"Server Error\"}"
            });
        });

        await Page.Locator("#parallel-btn").ClickAsync();
        await Page.WaitForTimeoutAsync(2000);

        // OnAllSettled should STILL fire — spinner hidden
        await Expect(Page.Locator("#parallel-spinner"))
            .ToBeHiddenAsync(new() { Timeout = 5000 });

        await Expect(Page.Locator("#parallel-all"))
            .ToHaveTextAsync("All parallel requests completed!", new() { Timeout = 5000 });

        // Neither request populated data
        await Expect(Page.Locator("#parallel-facility-first"))
            .ToHaveTextAsync("\u2014", new() { Timeout = 1000 });

        AssertNoConsoleErrorsExcept("500", "Internal Server Error");
    }

    // ── Bug Hunt 5: Delete with Confirm — rapid OK clicks ──

    [Test]
    public async Task delete_confirm_ok_fires_only_one_request()
    {
        // Section 6: Delete with Confirm dialog.
        // Even after confirming, can the user rapidly trigger another delete?
        await NavigateAndBootHttp();

        var requestCount = 0;
        await Page.RouteAsync("**/HttpPipeline/Http/DeleteResident/**", async route =>
        {
            requestCount++;
            await route.ContinueAsync();
        });

        // Click Delete → Confirm dialog appears
        await Page.Locator("#delete-btn").ClickAsync();

        // Wait for dialog
        var dialog = Page.Locator("#alisConfirmDialog");
        await Expect(dialog).ToBeVisibleAsync(new() { Timeout = 5000 });

        // Click OK
        var okButton = dialog.Locator("button.e-primary");
        await okButton.ClickAsync();

        // Wait for the request to complete
        await Page.WaitForTimeoutAsync(2000);

        // Only one DELETE request should have been sent
        Assert.That(requestCount, Is.EqualTo(1),
            $"Confirm OK should have sent exactly 1 DELETE request, but sent {requestCount}");

        // Verify deletion succeeded
        await Expect(Page.Locator("#delete-id"))
            .Not.ToHaveTextAsync("\u2014", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
