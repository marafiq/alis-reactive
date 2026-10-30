namespace Alis.Reactive.Framework.PlaywrightTests.Plans;

/// <summary>
/// A partial of the page's own model joins the page's plan (<c>ResolvePlan</c>). What a save sends and where a
/// message goes no longer follow the plan id (ADR-0003 rule 2): a form sends the inputs inside it, so the page's
/// save sends a partial rendered inside the page's form, with the page or loaded into a slot there, and not a card
/// rendered outside it, which its own save sends; and the message no field shows goes to the summary of the form
/// a request checks, whichever plan's request it is.
/// </summary>
[TestFixture]
public sealed class WhenPlansShareAPlanId : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Field(string label) => Page.GetByLabel(label, new() { Exact = true });

    [Test]
    public async Task a_partials_plan_says_it_merges_into_the_pages_plan_and_the_pages_says_it_does_not()
    {
        await Open("/Plans/Visit");

        await Expect(Page.Locator("#resident-plan-merges")).ToHaveTextAsync("yes");
        await Expect(Page.Locator("#page-plan-merges")).ToHaveTextAsync("no");
    }

    [Test]
    public async Task a_partial_rendered_inside_the_pages_form_is_sent_by_the_pages_save()
    {
        await Open("/Plans/Visit");
        await Field("Visitor").FillAsync("Ada");
        await Field("Resident").FillAsync("Bea");

        await Button("Book the visit").ClickAsync();

        await Expect(Page.Locator("#booked")).ToHaveTextAsync("visitor Ada, resident Bea, room none, companion none");
    }

    [Test]
    public async Task a_partial_loaded_into_a_slot_inside_the_pages_form_is_sent_while_it_is_loaded()
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
    public async Task a_card_outside_the_pages_form_is_sent_by_its_own_save_alone()
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
    public async Task a_partials_own_request_shows_the_message_no_field_shows_in_the_summary_of_the_form_it_checks()
    {
        await Open("/Plans/Visit");
        await Field("Visitor").FillAsync("Ada");

        await Button("Book for the resident").ClickAsync();

        await Expect(Page.Locator("#visit_validation_summary")).ToHaveTextAsync("Say why the visit is booked.");
    }
}
