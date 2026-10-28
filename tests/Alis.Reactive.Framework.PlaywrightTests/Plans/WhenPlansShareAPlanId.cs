namespace Alis.Reactive.Framework.PlaywrightTests.Plans;

/// <summary>
/// The plans of a page compose by the plan id they share, the model they are written for. A partial of the
/// page's own model joins the page's plan (<c>ResolvePlan</c>), rendered with the page or loaded into a slot,
/// so the page's form sends its inputs, and it renders no validation summary of its own. A partial of its own
/// model stands alone: the page's form sends none of its inputs.
/// </summary>
[TestFixture]
public sealed class WhenPlansShareAPlanId : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Field(string label) => Page.GetByLabel(label, new() { Exact = true });

    [Test]
    public async Task a_partial_of_the_pages_model_rendered_with_the_page_joins_the_pages_plan()
    {
        await Open("/Plans/Visit");
        await Field("Visitor").FillAsync("Ada");
        await Field("Resident").FillAsync("Bea");

        await Button("Book the visit").ClickAsync();

        await Expect(Page.Locator("#booked")).ToHaveTextAsync("visitor Ada, resident Bea, room none, companion none");
    }

    [Test]
    public async Task a_partial_of_the_pages_model_loaded_into_a_slot_joins_the_pages_plan_while_it_is_loaded()
    {
        await Open("/Plans/Visit");
        await Button("Choose a room").ClickAsync();
        await Field("Room").FillAsync("12");

        await Button("Book the visit").ClickAsync();
        await Expect(Page.Locator("#booked")).ToHaveTextAsync("visitor none, resident none, room 12, companion none");

        await Button("Leave the room unchosen").ClickAsync();
        await Expect(Page.Locator("#room-slot")).ToBeEmptyAsync();
        await Button("Book the visit").ClickAsync();
        await Expect(Page.Locator("#booked")).ToHaveTextAsync("visitor none, resident none, room none, companion none");
    }

    [Test]
    public async Task a_partial_of_its_own_model_stands_alone_so_only_its_own_form_sends_its_inputs()
    {
        await Open("/Plans/Visit");
        await Field("Visitor").FillAsync("Ada");
        await Field("Companion").FillAsync("Cy");

        await Button("Book the companion").ClickAsync();
        await Expect(Page.Locator("#companion-booked")).ToHaveTextAsync("visitor none, resident none, room none, companion Cy");

        await Button("Book the visit").ClickAsync();
        await Expect(Page.Locator("#booked")).ToHaveTextAsync("visitor Ada, resident none, room none, companion none");
    }

    [Test]
    public async Task a_partial_that_joins_the_pages_plan_leaves_the_message_no_field_shows_to_the_pages_summary()
    {
        await Open("/Plans/Visit");
        await Field("Visitor").FillAsync("Ada");

        await Button("Book without a reason").ClickAsync();

        await Expect(Page.Locator("#page-messages").GetByText("Say why the visit is booked.")).ToBeVisibleAsync();
    }
}
