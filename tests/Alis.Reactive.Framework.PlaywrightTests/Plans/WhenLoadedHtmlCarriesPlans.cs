namespace Alis.Reactive.Framework.PlaywrightTests.Plans;

[TestFixture]
public sealed class WhenLoadedHtmlCarriesPlans : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Slot => Page.Locator("#notice-slot");

    [Test]
    public async Task every_plan_in_loaded_html_runs()
    {
        await Open("/Plans/Noticeboard");
        await Button("Pin the notice").ClickAsync();
        await Expect(Slot.GetByRole(AriaRole.Heading, new() { Name = "Visiting hours change on Sunday" })).ToBeVisibleAsync();

        await Button("Mark the notice read").ClickAsync();
        await Button("Share the notice").ClickAsync();

        await Expect(Page.Locator("#notice-read")).ToHaveTextAsync("read");
        await Expect(Page.Locator("#notice-shared")).ToHaveTextAsync("shared");
    }

    [Test]
    public async Task loaded_html_whose_plan_is_not_json_is_refused_and_the_slot_keeps_what_it_showed()
    {
        await Open("/Plans/Noticeboard");
        await Button("Pin the notice").ClickAsync();
        await Expect(Slot.GetByRole(AriaRole.Heading, new() { Name = "Visiting hours change on Sunday" })).ToBeVisibleAsync();
        await Expect(Page.Locator("#pinned")).ToHaveTextAsync("the notice");

        await Button("Pin the torn notice").ClickAsync();

        await Expect(Page.Locator("#pinning")).ToHaveTextAsync("the torn notice");
        await Expect(Page.Locator("#pinned")).ToHaveTextAsync("the notice");
        await Expect(Slot.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Visiting hours change on Sunday");
        await Button("Mark the notice read").ClickAsync();
        await Expect(Page.Locator("#notice-read")).ToHaveTextAsync("read");
    }
}
