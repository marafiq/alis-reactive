namespace Alis.Reactive.Framework.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenASlotStartsWithServerRenderedContent : AppPageTest
{
    private ILocator Steps => Page.Locator("#move-in-step");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private Task RingTheBell() => Page.Locator("#move-in-bell").GetByRole(AriaRole.Button, new() { Name = "Ring the bell", Exact = true }).ClickAsync();

    [Test]
    public async Task a_step_rendered_with_the_page_starts_after_the_page_as_a_step_loaded_later_does()
    {
        await Open("/Slots/MoveIn");

        await Expect(Page.Locator("#move-in-progress")).ToHaveTextAsync("on the resident step");
    }

    [Test]
    public async Task a_step_rendered_with_the_page_hears_what_the_pages_ready_handler_raises()
    {
        await Open("/Slots/MoveIn");

        await Expect(Steps.Locator("#resident-taken-by")).ToHaveTextAsync("Dana");
    }

    [Test]
    public async Task a_step_rendered_with_the_page_stops_hearing_once_the_next_step_replaces_it()
    {
        await Open("/Slots/MoveIn");
        await RingTheBell();
        await Expect(Page.Locator("#resident-step-heard")).ToHaveTextAsync("1");

        await Page.GetByLabel("Resident", new() { Exact = true }).FillAsync("Ann");
        await Button("Next").ClickAsync();
        await Expect(Steps.GetByRole(AriaRole.Heading, new() { Name = "The room" })).ToBeVisibleAsync();
        await RingTheBell();

        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#resident-step-heard")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_step_rendered_with_the_page_stops_holding_the_pages_save_once_the_next_step_replaces_it()
    {
        await Open("/Slots/MoveIn");
        await Button("Save for later").ClickAsync();
        await Expect(Steps.GetByText("Name the resident.")).ToBeVisibleAsync();

        await Page.GetByLabel("Resident", new() { Exact = true }).FillAsync("Ann");
        await Button("Next").ClickAsync();
        await Expect(Steps.GetByRole(AriaRole.Heading, new() { Name = "The room" })).ToBeVisibleAsync();
        await Button("Save for later").ClickAsync();

        await Expect(Page.Locator("#saved")).ToHaveTextAsync("the move-in taken by Dana is saved for later");
    }

    [Test]
    public async Task a_step_rendered_with_the_page_stops_hearing_once_its_container_is_unloaded()
    {
        await Open("/Slots/MoveIn");
        await RingTheBell();
        await Expect(Page.Locator("#resident-step-heard")).ToHaveTextAsync("1");

        await Button("Clear the steps").ClickAsync();
        await Expect(Steps).ToBeEmptyAsync();
        await RingTheBell();

        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#resident-step-heard")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_part_rendered_inside_a_step_starts_after_the_step_as_it_would_loaded_there()
    {
        await Open("/Slots/MoveIn");

        await Expect(Steps.Locator("#needs-status")).ToHaveTextAsync("being asked");
    }

    [Test]
    public async Task a_part_rendered_inside_a_step_belongs_to_the_innermost_container_it_sits_in()
    {
        await Open("/Slots/MoveIn");
        await RingTheBell();
        await Expect(Page.Locator("#needs-heard")).ToHaveTextAsync("1");

        await Button("No special needs").ClickAsync();
        await Expect(Page.Locator("#resident-needs")).ToBeEmptyAsync();
        await RingTheBell();

        await Expect(Page.Locator("#resident-step-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#needs-heard")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_step_rendered_with_the_page_stops_holding_the_pages_save_once_its_container_is_unloaded()
    {
        await Open("/Slots/MoveIn");
        await Button("Save for later").ClickAsync();
        await Expect(Steps.GetByText("Name the resident.")).ToBeVisibleAsync();

        await Button("Clear the steps").ClickAsync();
        await Expect(Steps).ToBeEmptyAsync();
        await Button("Save for later").ClickAsync();

        await Expect(Page.Locator("#saved")).ToHaveTextAsync("the move-in taken by Dana is saved for later");
    }
}
