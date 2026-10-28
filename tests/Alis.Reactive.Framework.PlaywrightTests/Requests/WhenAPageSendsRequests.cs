namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// A page's requests, each reaching an endpoint that says what it received: where gathered members go (the
/// route, the query, the JSON body, form data, headers), values read when the request is sent (a component's
/// property, a plugin, a method's result: M4, and a value that is never missing in a member that may be empty),
/// the answer's routes (success, an error by status, any error),
/// and the order around a request (chained, parallel, while loading, finally).
/// </summary>
[TestFixture]
public sealed class WhenAPageSendsRequests : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_get_fills_its_route_from_the_member_named_like_the_placeholder_and_sends_the_rest_as_its_query()
    {
        await Open("/Requests/Desk");

        await Button("Get the resident").ClickAsync();

        await Expect(Line("resident")).ToHaveTextAsync("resident 1042 in Garden");
    }

    [Test]
    public async Task a_post_sends_its_members_as_the_json_body_with_values_read_when_it_is_sent()
    {
        await Open("/Requests/Desk");
        await Button("Add 4 to the tally").ClickAsync();

        await Button("Post a note").ClickAsync();

        await Expect(Line("note")).ToHaveTextAsync("note 'Back at three' titled Night, tally 4, amount 0");
    }

    [Test]
    public async Task a_put_fills_its_route_and_sends_its_body()
    {
        await Open("/Requests/Desk");

        await Button("Put the note").ClickAsync();

        await Expect(Line("put")).ToHaveTextAsync("put note 7: 'Changed'");
    }

    [Test]
    public async Task a_delete_reaches_the_endpoint_its_route_names()
    {
        await Open("/Requests/Desk");

        await Button("Delete the note").ClickAsync();

        await Expect(Line("deleted")).ToHaveTextAsync("deleted note 7");
    }

    [Test]
    public async Task form_data_carries_the_gathered_members_as_form_fields()
    {
        await Open("/Requests/Desk");

        await Button("Send as form data").ClickAsync();

        await Expect(Line("uploaded")).ToHaveTextAsync("form 'On paper' from form data, amount 12.5");
    }

    [Test]
    public async Task headers_carry_a_fixed_value_and_one_read_when_the_request_is_sent()
    {
        await Open("/Requests/Desk");

        await Button("Send with headers").ClickAsync();

        await Expect(Line("headers")).ToHaveTextAsync("shift Night at the Night desk");
    }

    [Test]
    public async Task a_method_result_fills_a_route_value_and_a_body_member_as_it_stands_when_sent()
    {
        await Open("/Requests/Desk");
        await Button("Place a line").ClickAsync();

        await Button("Send the total").ClickAsync();
        await Expect(Line("total")).ToHaveTextAsync("total 40.5 in the path");

        await Button("Post the total").ClickAsync();
        await Expect(Line("note")).ToHaveTextAsync("note 'Totals' titled nothing, tally 0, amount 40.5");
    }

    [Test]
    public async Task a_value_that_is_never_missing_fills_a_member_that_may_be_empty()
    {
        await Open("/Requests/Desk");
        await Button("Place a line").ClickAsync();

        await Button("Post the total as an estimate").ClickAsync();

        await Expect(Line("estimate")).ToHaveTextAsync("estimate 40.5");
    }

    [Test]
    public async Task an_error_status_the_page_names_runs_its_own_route_with_its_body()
    {
        await Open("/Requests/Desk");

        await Button("Ask for a taken room").ClickAsync();

        await Expect(Line("refused")).ToHaveTextAsync("The room is taken");
    }

    [Test]
    public async Task an_error_status_the_page_does_not_name_runs_the_route_for_any_error()
    {
        await Open("/Requests/Desk");

        await Button("Ask a broken endpoint").ClickAsync();

        await Expect(Line("refused")).ToHaveTextAsync("refused for another reason");
    }

    [Test]
    public async Task a_chained_request_runs_after_the_first_succeeds_and_reads_its_answer()
    {
        await Open("/Requests/Desk");

        await Button("Take both steps").ClickAsync();

        await Expect(Line("chained")).ToHaveTextAsync("second, after the second step");
    }

    [Test]
    public async Task a_chained_request_does_not_run_after_the_first_is_refused()
    {
        await Open("/Requests/Desk");

        await Button("Take a step after a refusal").ClickAsync();
        await Expect(Line("chained")).ToHaveTextAsync("the first step was refused");

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Line("chained")).ToHaveTextAsync("the first step was refused");
    }

    [Test]
    public async Task parallel_requests_each_run_their_own_route_and_on_all_settled_runs_after_both()
    {
        await Open("/Requests/Desk");

        await Button("Ask both at once").ClickAsync();

        await Expect(Line("slow-two")).ToHaveTextAsync("the quick one answered");
        await Expect(Line("settled")).ToHaveTextAsync("the slow one answered");
        await Expect(Line("slow-one")).ToHaveTextAsync("the slow one answered");
    }

    [Test]
    public async Task while_loading_runs_before_the_answer_and_the_answer_replaces_it()
    {
        await Open("/Requests/Desk");

        await Button("Load slowly").ClickAsync();

        await Expect(Line("loading")).ToHaveTextAsync("loading");
        await Expect(Line("loading")).ToHaveTextAsync("the slow endpoint answered");
    }

    [Test]
    public async Task finally_runs_after_an_answer_and_after_a_refusal()
    {
        await Open("/Requests/Desk");

        await Button("Load and finish").ClickAsync();
        await Expect(Line("finished")).ToHaveTextAsync("finished after the answer");

        await Button("Ask a broken endpoint").ClickAsync();
        await Expect(Line("finished")).ToHaveTextAsync("finished after the refusal");
    }
}
