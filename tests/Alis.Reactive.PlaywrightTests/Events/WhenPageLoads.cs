namespace Alis.Reactive.PlaywrightTests.Events;

[TestFixture]
public class WhenPageLoads : PlaywrightTestBase
{
    [Test]
    public async Task Events_page_renders_with_correct_title()
    {
        await NavigateTo("/Sandbox/Events");
        await Expect(Page).ToHaveTitleAsync("Events & Dispatch — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Events_page_shows_all_content_sections()
    {
        await NavigateTo("/Sandbox/Events");

        await Expect(Page.Locator("h1")).ToHaveTextAsync("Events & Dispatch");
        await Expect(Page.GetByText("Plan JSON")).ToBeVisibleAsync();
        await Expect(Page.GetByText("C# DSL")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Runtime Boot (TypeScript)")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Plan_json_section_shows_valid_plan()
    {
        await NavigateTo("/Sandbox/Events");

        var planJson = Page.Locator("#plan-json");
        await Expect(planJson).Not.ToBeEmptyAsync();

        var text = await planJson.TextContentAsync();
        Assert.That(text, Does.Contain("\"entries\""), "Plan has entries");
        Assert.That(text, Does.Contain("\"dom-ready\""), "Plan has dom-ready trigger");
        Assert.That(text, Does.Contain("\"custom-event\""), "Plan has custom-event trigger");
    }

    [Test]
    public async Task Home_page_renders_and_links_to_events()
    {
        await NavigateTo("/");

        await Expect(Page).ToHaveTitleAsync("Home — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();

        var link = Page.GetByRole(AriaRole.Link, new() { Name = "Events", Exact = true });
        await Expect(link).ToBeVisibleAsync();
        await link.ClickAsync();
        await Page.WaitForURLAsync("**/Sandbox/Events");
    }
}
