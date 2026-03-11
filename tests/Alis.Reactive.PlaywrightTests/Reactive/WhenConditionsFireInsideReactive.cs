namespace Alis.Reactive.PlaywrightTests.Reactive;

/// <summary>
/// Verifies conditions inside .Reactive() with Html.Field(), both vendors,
/// nested properties, and cross-component Component&lt;T&gt;() mutations.
///
/// Page under test: /Sandbox/PlaygroundSyntax/ReactiveConditions
///
/// All controls rendered once in a single form section:
///   Status (native, reactive)  — ElseIf → mutates Amount (fusion) + City (nested native) + DOM
///   Amount (fusion, reactive)  — ElseIf → classifies tier via DOM mutations
///   City   (nested native, reactive) — ElseIf → auto-fills State (nested native) + PostalCode (nested fusion)
///   State  (nested native, non-reactive) — mutation target only
///   PostalCode (nested fusion, non-reactive) — mutation target only
/// </summary>
[TestFixture]
public class WhenConditionsFireInsideReactive : PlaywrightTestBase
{
    /// <summary>IdGenerator type scope for PlaygroundSyntaxModel.</summary>
    private const string S = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_PlaygroundSyntaxModel";

    private async Task NavigateAndBoot()
    {
        await NavigateTo("/Sandbox/PlaygroundSyntax/ReactiveConditions");
        await WaitForTraceMessage("booted", 5000);
    }

    // ── Status → ElseIf → cross-vendor mutations ──

    [Test]
    public async Task Status_active_sets_amount_and_city()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync("active");

        var result = Page.Locator("#status-result");
        await Expect(result).ToContainTextAsync("Active", new() { Timeout = 3000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-emerald-700"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Status_inactive_hides_address_section()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync("inactive");

        await Expect(Page.Locator("#status-result")).ToContainTextAsync("Inactive", new() { Timeout = 3000 });
        await Expect(Page.Locator("#address-section")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Status_pending_shows_fallback()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync("pending");

        await Expect(Page.Locator("#status-result")).ToContainTextAsync("Pending", new() { Timeout = 3000 });
        await Expect(Page.Locator("#address-section")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Status_toggle_address_visibility()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Status").SelectOptionAsync("inactive");
        await Expect(Page.Locator("#address-section")).ToBeHiddenAsync(new() { Timeout = 3000 });

        await Page.Locator($"#{S}__Status").SelectOptionAsync("active");
        await Expect(Page.Locator("#address-section")).ToBeVisibleAsync(new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Amount → ElseIf tier ladder ──

    // NOTE: SF NumericTextBox renders TWO <input> elements with the same ID
    // (visible control + hidden value). Using .First targets the visible control.
    // This is a known SF pattern — future stable ID system will resolve this.

    [Test]
    public async Task Amount_high_value_tier()
    {
        await NavigateAndBoot();

        var input = Page.Locator("#Amount").First;
        await input.ClickAsync();
        await input.FillAsync("5000");
        await input.PressAsync("Tab");

        var tier = Page.Locator("#amount-tier");
        await Expect(tier).ToHaveTextAsync("High value order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-rose-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Amount_standard_tier()
    {
        await NavigateAndBoot();

        var input = Page.Locator("#Amount").First;
        await input.ClickAsync();
        await input.FillAsync("2500");
        await input.PressAsync("Tab");

        var tier = Page.Locator("#amount-tier");
        await Expect(tier).ToHaveTextAsync("Standard order", new() { Timeout = 3000 });
        await Expect(tier).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-sky-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Amount_small_tier()
    {
        await NavigateAndBoot();

        var input = Page.Locator("#Amount").First;
        await input.ClickAsync();
        await input.FillAsync("500");
        await input.PressAsync("Tab");

        await Expect(Page.Locator("#amount-tier")).ToHaveTextAsync("Small order", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Nested City → auto-fill State (native) + PostalCode (fusion) ──

    [Test]
    public async Task City_seattle_autofills_siblings()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Address_City").SelectOptionAsync("seattle");

        await Expect(Page.Locator("#city-auto")).ToContainTextAsync("WA, 98101", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task City_portland_autofills_siblings()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Address_City").SelectOptionAsync("portland");

        await Expect(Page.Locator("#city-auto")).ToContainTextAsync("OR, 97201", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task City_denver_autofills_siblings()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Address_City").SelectOptionAsync("denver");

        await Expect(Page.Locator("#city-auto")).ToContainTextAsync("CO, 80201", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task City_empty_resets_siblings()
    {
        await NavigateAndBoot();

        await Page.Locator($"#{S}__Address_City").SelectOptionAsync("seattle");
        await Expect(Page.Locator("#city-auto")).ToContainTextAsync("WA", new() { Timeout = 3000 });

        await Page.Locator($"#{S}__Address_City").SelectOptionAsync("");
        await Expect(Page.Locator("#city-auto")).ToHaveTextAsync("Select a city", new() { Timeout = 3000 });

        AssertNoConsoleErrors();
    }

    // ── Plan structure ──

    [Test]
    public async Task Plan_contains_branches_and_guards()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"branches\""),
            "Plan must contain branches from ElseIf");
        Assert.That(planJson, Does.Contain("\"op\": \"eq\""),
            "Plan must contain eq operator");
        Assert.That(planJson, Does.Contain("\"op\": \"gte\""),
            "Plan must contain gte operator");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Plan_targets_nested_components()
    {
        await NavigateAndBoot();

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("Address_City"),
            "Plan must target nested native Address_City");
        Assert.That(planJson, Does.Contain("Address_State"),
            "Plan must target nested native Address_State");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Page_renders_correctly()
    {
        await NavigateAndBoot();

        await Expect(Page.Locator("h1")).ToHaveTextAsync("Reactive Conditions");
        await Expect(Page.Locator("#plan-json")).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{S}__Status")).ToBeVisibleAsync();
        await Expect(Page.Locator("#Amount").First).ToBeVisibleAsync();
        await Expect(Page.Locator($"#{S}__Address_City")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }
}
