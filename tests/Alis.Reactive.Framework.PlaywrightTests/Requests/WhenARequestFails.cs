namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// A request that fails, and what runs around it: a request that cannot be built or whose answer cannot be
/// handled still runs its Finally, waits for a Finally that waits, and stops the steps after it; a route value
/// that is missing sends nothing (H1); a branch of a parallel pair that fails stops neither the other branch
/// nor what runs once both settle; a dropped connection and an answer whose JSON cannot be read run the
/// route for any error (H7); and an answer is read by its media type (H6). Each pipeline counts its attempt
/// on the page's tally, so a line shows the attempt at which its step last ran.
/// </summary>
[TestFixture]
public sealed class WhenARequestFails : AppPageTest
{
    private readonly List<string> _limitLookups = new();

    [SetUp]
    public void WatchTheLimitEndpoint()
    {
        _limitLookups.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Requests/Limits/", StringComparison.Ordinal))
                lock (_limitLookups) _limitLookups.Add(request.Url);
        };
    }

    private ILocator Line(string id) => Page.Locator($"#{id}");

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    private async Task PutTheLedgerAway()
    {
        await Click("Put the ledger away");
        await Expect(Line("ledger-shelf")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_request_that_cannot_be_built_runs_its_finally_and_stops_the_steps_after_it()
    {
        await Open("/Requests/Outage");
        await Click("Send the ledger's title");
        await Expect(Line("title-answer")).ToHaveTextAsync("ledger titled Night ledger");
        await Expect(Line("title-after")).ToHaveTextAsync("1");
        await PutTheLedgerAway();

        await Click("Send the ledger's title");

        await Expect(Line("title-tried")).ToHaveTextAsync("2");
        await Expect(Line("title-finished")).ToHaveTextAsync("2");
        await Expect(Line("title-after")).ToHaveTextAsync("1");
        await Click("Count the attempts");
        await Expect(Line("attempts")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task an_answer_that_cannot_be_handled_runs_the_finally_and_stops_the_steps_after_it()
    {
        await Open("/Requests/Outage");
        await Click("Write the answer into the ledger");
        await Expect(Line("answer-written")).ToHaveTextAsync("1");
        await Expect(Line("answer-after")).ToHaveTextAsync("1");
        await PutTheLedgerAway();

        await Click("Write the answer into the ledger");

        await Expect(Line("answer-finished")).ToHaveTextAsync("2");
        await Expect(Line("answer-written")).ToHaveTextAsync("1");
        await Expect(Line("answer-after")).ToHaveTextAsync("1");
        await Click("Count the attempts");
        await Expect(Line("attempts")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_finally_that_waits_runs_to_its_end_and_the_steps_after_a_failed_request_still_do_not_run()
    {
        await Open("/Requests/Outage");
        await Click("Send the title and save the keeper");
        await Expect(Line("save-after")).ToHaveTextAsync("1");
        await PutTheLedgerAway();

        await Click("Send the title and save the keeper");

        await Expect(Line("save-finished")).ToHaveTextAsync("2");
        await Expect(Line("save-status")).ToHaveTextAsync("saved");
        await Expect(Line("save-after")).ToHaveTextAsync("1");
        await Click("Count the attempts");
        await Expect(Line("attempts")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_route_value_that_is_missing_sends_nothing_and_stops_the_steps_after_it()
    {
        await Open("/Requests/Outage");

        await Click("Look up by the keeper's limit");
        await Expect(Line("limit-finished")).ToHaveTextAsync("1");
        await Expect(Line("limit-after")).ToBeEmptyAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_limitLookups) Assert.That(_limitLookups, Is.Empty, "nothing was sent without the limit");

        await Click("Give the keeper a limit of 3");
        await Click("Look up by the keeper's limit");
        await Expect(Line("limit-answer")).ToHaveTextAsync("limit 3");
        await Expect(Line("limit-after")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_parallel_branch_that_cannot_be_built_stops_neither_the_other_branch_nor_what_runs_once_both_settle()
    {
        await Open("/Requests/Outage");
        await Click("Ask the ledger and the desk at once");
        await Expect(Line("both-settled")).ToHaveTextAsync("1");
        await Expect(Line("both-ledger")).ToHaveTextAsync("1");
        await PutTheLedgerAway();

        await Click("Ask the ledger and the desk at once");

        await Expect(Line("both-desk")).ToHaveTextAsync("2");
        await Expect(Line("both-settled")).ToHaveTextAsync("2");
        await Expect(Line("both-ledger")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_parallel_branch_whose_answer_cannot_be_handled_stops_neither_the_other_branch_nor_what_runs_once_both_settle()
    {
        await Open("/Requests/Outage");
        await Click("Write two answers at once");
        await Expect(Line("pair-settled")).ToHaveTextAsync("1");
        await Expect(Line("pair-ledger")).ToHaveTextAsync("1");
        await PutTheLedgerAway();

        await Click("Write two answers at once");

        await Expect(Line("pair-desk")).ToHaveTextAsync("2");
        await Expect(Line("pair-settled")).ToHaveTextAsync("2");
        await Expect(Line("pair-ledger")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_dropped_connection_runs_the_route_for_any_error()
    {
        await Open("/Requests/Outage");

        await Click("Ask an endpoint that drops the connection");

        await Expect(Line("dropped")).ToHaveTextAsync("the connection dropped");
    }

    [Test]
    public async Task an_answer_whose_json_cannot_be_read_runs_the_route_for_any_error()
    {
        await Open("/Requests/Outage");

        await Click("Ask an endpoint whose answer is cut short");

        await Expect(Line("garbled")).ToHaveTextAsync("the answer could not be read");
    }

    [Test]
    public async Task an_answer_of_an_html_media_type_outside_text_is_read_as_text()
    {
        await Open("/Requests/Outage");

        await Click("Ask for a page in XHTML");

        await Expect(Line("xhtml")).ToHaveTextAsync("The night desk is open.");
    }

    [Test]
    public async Task an_answer_of_any_other_media_type_has_no_body_the_page_reads()
    {
        await Open("/Requests/Outage");

        await Click("Ask for a file of bytes");

        await Expect(Line("bytes")).ToHaveTextAsync("no text in the answer");
    }
}
