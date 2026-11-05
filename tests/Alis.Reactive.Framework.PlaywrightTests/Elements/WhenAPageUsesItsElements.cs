namespace Alis.Reactive.Framework.PlaywrightTests.Elements;

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

    [Test]
    public async Task html_set_from_a_value_read_when_the_step_runs_shows_it_as_markup()
    {
        await Open("/Elements/Panel");

        await Click("Call the nurse to the east ward");

        await Expect(Page.Locator("#call-note").GetByText("Fall risk", new() { Exact = true })).ToHaveCSSAsync("font-weight", "700");
    }

    [Test]
    public async Task html_set_from_a_missing_value_shows_nothing()
    {
        await Open("/Elements/Panel");

        await Click("Call the nurse with no room");

        await Expect(Page.Locator("#call-heard")).ToHaveTextAsync("heard");
        await Expect(Page.Locator("#call-note")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_class_named_by_a_value_read_when_the_step_runs_is_added()
    {
        await Open("/Elements/Panel");

        await Click("Call the nurse to the east ward");

        await Expect(Page.Locator("#call-ward")).ToHaveClassAsync("chip ward-east");
    }

    [Test]
    public async Task a_missing_class_name_adds_no_class()
    {
        await Open("/Elements/Panel");

        await Click("Call the nurse with no room");

        await Expect(Page.Locator("#call-heard")).ToHaveTextAsync("heard");
        await Expect(Page.Locator("#call-ward")).ToHaveClassAsync("chip");
    }

    [Test]
    public async Task a_class_named_by_a_value_read_when_the_step_runs_is_toggled()
    {
        await Open("/Elements/Panel");

        await Click("Call the nurse to the east ward");

        await Expect(Page.Locator("#call-alert")).ToHaveCSSAsync("font-weight", "700");
    }

    [Test]
    public async Task a_missing_class_name_toggles_no_class()
    {
        await Open("/Elements/Panel");

        await Click("Call the nurse with no room");

        await Expect(Page.Locator("#call-heard")).ToHaveTextAsync("heard");
        await Expect(Page.Locator("#call-alert")).ToHaveClassAsync("chip");
    }

    [Test]
    public async Task a_class_named_by_a_value_read_when_the_step_runs_is_taken_off()
    {
        await Open("/Elements/Panel");

        await Click("Call the nurse to the east ward");

        await Expect(Page.Locator("#call-state")).ToHaveClassAsync("chip");
    }

    [Test]
    public async Task a_missing_class_name_takes_off_no_class()
    {
        await Open("/Elements/Panel");

        await Click("Call the nurse with no room");

        await Expect(Page.Locator("#call-heard")).ToHaveTextAsync("heard");
        await Expect(Page.Locator("#call-state")).ToHaveClassAsync("chip waiting");
    }

    [TestCase("PayloadWithNoField", "Dispatch payload must have at least one field. An event with no payload is declared with AppEvent.Declare() and raised with Dispatch(appEvent).", TestName = "a_dispatch_payload_with_no_field_is_refused")]
    [TestCase("SelectorThatIsNoMember", "Select a member of the payload, for example x => x.ResidentId.", TestName = "a_dispatch_payload_selector_that_is_no_member_is_refused")]
    [TestCase("MemberSetTwice", "'room' is assigned twice, or over members of its own; assign each path once.", TestName = "a_dispatch_payload_member_set_twice_is_refused")]
    [TestCase("ObjectSetOverItsMember", "'bed' is assigned twice, or over members of its own; assign each path once.", TestName = "a_dispatch_payload_object_set_over_a_member_already_set_is_refused")]
    [TestCase("MemberSetUnderItsObject", "'bed.number' is assigned under a value; assign the value or its members, not both.", TestName = "a_dispatch_payload_member_set_under_an_object_already_set_is_refused")]
    public async Task a_dispatch_the_page_cannot_carry_is_refused_where_it_renders(string mistake, string message)
    {
        await Page.GotoAsync($"{BaseUrl}/Elements/Refused?mistake={mistake}");

        await Expect(Page.GetByText(message).First).ToBeVisibleAsync();
    }
}
