namespace Alis.Reactive.PlaywrightTests.Events;

/// <summary>
/// Verifies the Events page renders correctly: navigation works, plan JSON is valid,
/// and all step sections are present in the DOM before the chain fires.
/// These are precondition tests — if these fail, chain tests are meaningless.
/// </summary>
[TestFixture]
public class WhenPageLoads : PlaywrightTestBase
{
    [Test]
    public async Task home_page_links_to_sandbox_events()
    {
        // The home page must have a working navigation path to the Events page.
        // If routing or area registration breaks, this link disappears or 404s.
        await NavigateTo("/");
        await Expect(Page).ToHaveTitleAsync("Home — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();

        // Click the "Events & Dispatch" card link on the home page
        var link = Page.GetByRole(AriaRole.Link, new() { Name = "Events & Dispatch" });
        await Expect(link).ToBeVisibleAsync();
        await link.ClickAsync();
        await Page.WaitForURLAsync("**/Sandbox/Events");
        await Expect(Page).ToHaveTitleAsync("Events & Dispatch — Alis.Reactive Sandbox");
    }

    [Test]
    public async Task events_page_renders_plan_json()
    {
        // The plan JSON section must contain valid JSON with an entries array.
        // If plan.Render() or plan.RenderFormatted() breaks, this section is empty or malformed.
        await NavigateTo("/Sandbox/Events");

        var planJson = Page.Locator("#plan-json");
        await Expect(planJson).Not.ToBeEmptyAsync();

        var text = await planJson.TextContentAsync();
        Assert.That(text, Is.Not.Null.And.Not.Empty, "Plan JSON must not be empty");

        // Validate it's actual JSON with expected structure
        Assert.That(text, Does.Contain("\"entries\""), "Plan must have entries array");
        Assert.That(text, Does.Contain("\"dom-ready\""), "Plan must contain dom-ready trigger");
        Assert.That(text, Does.Contain("\"custom-event\""), "Plan must contain custom-event triggers");
        Assert.That(text, Does.Contain("\"dispatch\""), "Plan must contain dispatch commands");
        Assert.That(text, Does.Contain("\"mutate-element\""), "Plan must contain mutate-element commands");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task events_page_shows_all_three_steps()
    {
        // The three step elements must exist in the DOM — they are the mutation targets.
        // If the view removes or renames an element ID, the chain will throw at runtime.
        await NavigateTo("/Sandbox/Events");

        await Expect(Page.Locator("#step-1")).ToBeVisibleAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync();
        await Expect(Page.Locator("#step-3")).ToBeVisibleAsync();
        await Expect(Page.Locator("#chain-status")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task events_page_renders_with_correct_title()
    {
        await NavigateTo("/Sandbox/Events");
        await Expect(Page).ToHaveTitleAsync("Events & Dispatch — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_has_correct_entry_count()
    {
        // Parse the plan JSON from #plan-json element and verify it has exactly 4 entries,
        // matching the 4 Html.On() calls in the view (dom-ready, test, test-received, final).
        // Proves plan serialization includes all entries — if an entry silently drops,
        // the chain breaks and this test catches it before Playwright chain tests run.
        await NavigateTo("/Sandbox/Events");

        var planJson = Page.Locator("#plan-json");
        var text = await planJson.TextContentAsync();
        Assert.That(text, Is.Not.Null.And.Not.Empty, "Plan JSON must not be empty");

        var doc = System.Text.Json.JsonDocument.Parse(text!);
        var entries = doc.RootElement.GetProperty("entries");
        Assert.That(entries.GetArrayLength(), Is.EqualTo(4),
            "Plan must have exactly 4 entries (1 dom-ready + 3 custom-event)");

        AssertNoConsoleErrors();
    }
}
