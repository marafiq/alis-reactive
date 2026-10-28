namespace Alis.Reactive.Framework.PlaywrightTests.Plans;

/// <summary>
/// The names a page gives its plan that the plan cannot take, each refused when the page renders with the
/// message the developer reads: a component or an element plan with no id, a plugin named with a space or not
/// named, a header with no name, a route placeholder no route value can be named, an error route for a
/// status HTTP does not have, a value read through another plan's component, and a response read outside the
/// routes of its request.
/// </summary>
[TestFixture]
public sealed class WhenAPageNamesWhatAPlanCannotTake : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string mistake) => await Page.GotoAsync($"{BaseUrl}/Plans/Refused?mistake={mistake}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_component_rendered_with_no_id_is_refused()
    {
        await Render("RenderAComponentWithNoId");

        await Expect(Message("Element ID required.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_element_plan_with_no_id_is_refused()
    {
        await Render("GiveAnElementPlanNoId");

        await Expect(Message("Element ID required.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_plugin_named_with_a_space_is_refused()
    {
        await Render("NameAPluginWithASpace");

        await Expect(Message("Plugin name must not contain whitespace.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_plugin_given_no_name_is_refused_naming_what_is_missing()
    {
        await Render("GiveAPluginNoName");

        await Expect(Message("Plugin name must not be empty.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_header_given_no_name_is_refused_naming_what_is_missing()
    {
        await Render("GiveAHeaderNoName");

        await Expect(Message("Header name must not be empty.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_route_placeholder_no_route_value_can_be_named_is_refused()
    {
        await Render("NameARoutePlaceholderWithADash");

        await Expect(Message(
            "URL template '/Plans/Rooms/{room-number}' is invalid: invalid placeholder '{room-number}'. Names must match [a-zA-Z0-9_] (ASCII only).")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_value_read_through_a_component_of_another_plan_is_refused()
    {
        await Render("ReadAnotherPlansComponent");

        await Expect(Message("Element 'slate' is used by a step before the plan declared it.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_response_read_outside_the_routes_of_its_request_is_refused()
    {
        await Render("ReadAResponseOutsideItsRoutes");

        await Expect(Message(
            "reads a response outside the routes of the request that receives it. Read a response inside OnSuccess, OnError, Chained or Finally of its request.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_error_route_for_a_status_http_does_not_have_is_refused()
    {
        await Render("RouteAStatusHttpDoesNotHave");

        await Expect(Message(
            "HTTP response status code must be between 100 and 599. Use OnError(...) without a status code for network/client failures.")).ToBeVisibleAsync();
    }
}
