namespace Alis.Reactive.PlaywrightTests.CoreBehaviors;

[TestFixture]
public class WhenPlanBoots : PlaywrightTestBase
{
    [Test]
    public async Task home_page_links_to_sandbox_events()
    {
        await NavigateTo("/");
        await Expect(Page).ToHaveTitleAsync("Home — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();

        var link = Page.GetByRole(AriaRole.Link, new() { Name = "Events & Dispatch" });
        await Expect(link).ToBeVisibleAsync();
        await link.ClickAsync();
        await Page.WaitForURLAsync("**/Sandbox/CoreBehaviors/Events");
        await Expect(Page).ToHaveTitleAsync("Events & Dispatch — Alis.Reactive Sandbox");
    }

    [Test]
    public async Task events_page_renders_plan_json()
    {
        await NavigateTo("/Sandbox/CoreBehaviors/Events");

        var planJson = Page.Locator("#plan-json");
        await Expect(planJson).Not.ToBeEmptyAsync();

        var text = await planJson.TextContentAsync();
        Assert.That(text, Is.Not.Null.And.Not.Empty, "Plan JSON must not be empty");

        Assert.That(text, Does.Contain("\"behaviors\""), "Plan must have behaviors array");
        Assert.That(text, Does.Contain("\"page-ready\""), "Plan must contain page-ready trigger");
        Assert.That(text, Does.Contain("\"document-event\""), "Plan must contain document-event triggers");
        Assert.That(text, Does.Contain("\"dispatch\""), "Plan must contain dispatch reactions");
        Assert.That(text, Does.Contain("\"set\""), "Plan must contain set reactions");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task events_page_shows_all_three_steps()
    {
        // These controlled element IDs are the set reaction targets for the event chain.
        await NavigateTo("/Sandbox/CoreBehaviors/Events");

        await Expect(Page.Locator("#step-1")).ToBeVisibleAsync();
        await Expect(Page.Locator("#step-2")).ToBeVisibleAsync();
        await Expect(Page.Locator("#step-3")).ToBeVisibleAsync();
        await Expect(Page.Locator("#chain-status")).ToBeVisibleAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task events_page_renders_with_correct_title()
    {
        await NavigateTo("/Sandbox/CoreBehaviors/Events");
        await Expect(Page).ToHaveTitleAsync("Events & Dispatch — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task plan_json_has_correct_entry_count()
    {
        // The Events view declares four Html.On calls: page-ready, test, test-received, final.
        await NavigateTo("/Sandbox/CoreBehaviors/Events");

        var planJson = Page.Locator("#plan-json");
        var text = await planJson.TextContentAsync();
        Assert.That(text, Is.Not.Null.And.Not.Empty, "Plan JSON must not be empty");

        var doc = System.Text.Json.JsonDocument.Parse(text!);
        var entries = doc.RootElement.GetProperty("behaviors");
        Assert.That(entries.GetArrayLength(), Is.EqualTo(4),
            "Plan must have exactly 4 behaviors (1 page-ready + 3 document-event)");

        AssertNoConsoleErrors();
    }
}
