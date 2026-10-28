namespace Alis.Reactive.Framework.PlaywrightTests.Elements;

/// <summary>
/// The page's own elements shown and hidden and their classes taken off and toggled; the app's one-per-page
/// component reached by the id its declaration names; a component rendered for another model's member read
/// by that member; a typed event raised with no payload heard, its payload's members reading nothing; and the
/// dispatches a page is refused for where it renders.
/// </summary>
[TestFixture]
public sealed class WhenAPageUsesItsElements : AppPageTest
{
    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    private ILocator Notice => Page.GetByText("Visiting hours have changed.", new() { Exact = true });

    private ILocator Quiet => Page.GetByText("Quiet hours from nine.", new() { Exact = true });

    [Test]
    public async Task an_element_is_shown_and_hidden()
    {
        await Open("/Elements/Panel");
        await Expect(Notice).ToBeHiddenAsync();

        await Click("Show the notice");
        await Expect(Notice).ToBeVisibleAsync();

        await Click("Hide the notice");
        await Expect(Notice).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_class_is_taken_off_and_toggled()
    {
        await Open("/Elements/Panel");
        await Expect(Quiet).ToBeHiddenAsync();

        await Click("Take gone off the quiet hours");
        await Expect(Quiet).ToBeVisibleAsync();

        await Click("Toggle gone on the quiet hours");
        await Expect(Quiet).ToBeHiddenAsync();

        await Click("Toggle gone on the quiet hours");
        await Expect(Quiet).ToBeVisibleAsync();
    }

    [Test]
    public async Task the_apps_one_per_page_component_is_reached_by_the_id_its_declaration_names()
    {
        await Open("/Elements/Panel");
        await Expect(Page.Locator("#app-banner")).ToHaveTextAsync("Banner: nothing yet · flashed 0 times");

        await Click("Post lunch on the banner");

        await Expect(Page.Locator("#app-banner")).ToHaveTextAsync("Banner: Lunch is served · flashed 1 times");
    }

    [Test]
    public async Task a_component_rendered_for_another_models_member_is_read_by_that_member()
    {
        await Open("/Elements/Panel");
        await Page.GetByLabel("Visitor", new() { Exact = true }).FillAsync("Bea");

        await Click("Read the visitor");

        await Expect(Page.Locator("#visitor-read")).ToHaveTextAsync("Bea");
    }

    [Test]
    public async Task a_typed_event_raised_with_no_payload_is_heard_and_its_members_read_nothing()
    {
        await Open("/Elements/Panel");

        await Click("Call the nurse with no room");

        await Expect(Page.Locator("#call-heard")).ToHaveTextAsync("heard");
        await Expect(Page.Locator("#call-room")).ToHaveTextAsync("");
    }

    [TestCase("PayloadWithNoField", "Dispatch payload must have at least one field. An event with no payload is declared with AppEvent.Declare() and raised with Dispatch(appEvent).", TestName = "a_dispatch_payload_with_no_field_is_refused")]
    [TestCase("SelectorThatIsNoMember", "Select a member of the payload, for example x => x.ResidentId.", TestName = "a_dispatch_payload_selector_that_is_no_member_is_refused")]
    public async Task a_dispatch_the_page_cannot_carry_is_refused_where_it_renders(string mistake, string message)
    {
        await Page.GotoAsync($"{BaseUrl}/Elements/Refused?mistake={mistake}");

        await Expect(Page.GetByText(message).First).ToBeVisibleAsync();
    }
}
