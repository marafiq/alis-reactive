namespace Alis.Reactive.Framework.PlaywrightTests.Realtime;

/// <summary>
/// A page the server pushes to: server-sent events run their pipelines with their data, every time, and an
/// event with no data too; a stream the page closes reads as closed; and a SignalR hub the page sends to,
/// invokes and hears, each call answered on its own connection: a method named otherwise than its member, an
/// invoke whose next step waits for the server, an answer read, the connection id the server knows the page
/// by, and a client method called with anything but one object, which runs nothing.
/// </summary>
[TestFixture]
public sealed class WhenTheServerPushesToAPage : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_pushed_event_runs_its_pipeline_with_its_data()
    {
        await Open("/Realtime/Round");

        await Expect(Line("news")).ToHaveTextAsync("Ada is up");
        await Expect(Line("news-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task every_push_runs_the_pipeline_again_with_that_pushs_data()
    {
        await Open("/Realtime/Round");
        await Expect(Line("news-count")).ToHaveTextAsync("1");

        await Button("Push the next news").ClickAsync();
        await Expect(Line("news")).ToHaveTextAsync("news 2");
        await Expect(Line("news-count")).ToHaveTextAsync("2");

        await Button("Push the next news").ClickAsync();
        await Expect(Line("news-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_pushed_event_with_no_data_runs_its_pipeline()
    {
        await Open("/Realtime/Round");
        await Expect(Line("news-count")).ToHaveTextAsync("1");
        await Expect(Line("all-clear")).ToHaveTextAsync("");

        await Button("Push the all clear").ClickAsync();

        await Expect(Line("all-clear")).ToHaveTextAsync("all clear");
    }

    [Test]
    public async Task a_stream_the_page_closes_reads_as_closed()
    {
        await Open("/Realtime/Round");
        await Expect(Line("news-count")).ToHaveTextAsync("1");

        await Button("Close the stream").ClickAsync();

        await Expect(Line("stream-state")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_hub_method_the_page_sends_is_answered_by_the_client_method_the_server_calls_back()
    {
        await Open("/Realtime/Round");

        await Button("Clear the board").ClickAsync();

        await Expect(Line("board")).ToHaveTextAsync("cleared");
    }

    [Test]
    public async Task a_client_method_the_server_calls_with_an_argument_runs_its_pipeline_with_its_members()
    {
        await Open("/Realtime/Round");

        await Button("Ping Ben").ClickAsync();

        await Expect(Line("status-resident")).ToHaveTextAsync("Ben");
        await Expect(Line("status")).ToHaveTextAsync("up");
    }

    [Test]
    public async Task a_hub_method_the_page_invokes_answers_with_the_value_the_server_returned()
    {
        await Open("/Realtime/Round");

        await Button("Acknowledge as Ada").ClickAsync();

        await Expect(Line("ack")).ToHaveTextAsync("acknowledged by Ada");
    }

    [Test]
    public async Task a_hub_method_declared_under_another_name_calls_the_server_method_of_that_name()
    {
        await Open("/Realtime/Round");

        await Button("Sweep the board").ClickAsync();

        await Expect(Line("board")).ToHaveTextAsync("cleared");
    }

    // The server tells the page it is tidying before it completes the call; the step after the call writes
    // "tidied" over it only if it waited for the server.
    [Test]
    public async Task a_hub_method_the_page_invokes_holds_the_next_step_until_the_server_completes_it()
    {
        await Open("/Realtime/Round");

        await Button("Tidy up").ClickAsync();

        await Expect(Line("status-resident")).ToHaveTextAsync("everyone");
        await Expect(Line("status")).ToHaveTextAsync("tidied");
    }

    [Test]
    public async Task a_hub_method_the_page_invokes_with_an_argument_holds_the_next_step_until_the_server_completes_it()
    {
        await Open("/Realtime/Round");

        await Button("Tidy up for Ben").ClickAsync();

        await Expect(Line("status-resident")).ToHaveTextAsync("Ben");
        await Expect(Line("status")).ToHaveTextAsync("tidied");
    }

    [Test]
    public async Task a_hub_method_the_page_invokes_with_no_argument_answers_with_the_value_the_server_returned()
    {
        await Open("/Realtime/Round");

        await Button("Read the roster").ClickAsync();

        await Expect(Line("roster")).ToHaveTextAsync("Ada and Ben");
    }

    [Test]
    public async Task the_hubs_connection_id_is_the_one_the_server_knows_the_page_by()
    {
        await Open("/Realtime/Round");
        await Button("Acknowledge as Ada").ClickAsync();
        await Expect(Line("ack")).ToHaveTextAsync("acknowledged by Ada");

        await Button("Find me by my connection id").ClickAsync();

        await Expect(Line("found")).ToHaveTextAsync("found");
    }

    // The server calls StatusChanged with a bare word, then BoardCleared: once the board reads cleared, the
    // bare call has arrived.
    [Test]
    public async Task a_client_method_the_server_calls_with_anything_but_one_object_runs_no_pipeline()
    {
        await Open("/Realtime/Round");

        await Button("Ask for a bare status").ClickAsync();
        await Expect(Line("board")).ToHaveTextAsync("cleared");
        await Expect(Line("status-heard")).ToHaveTextAsync("");

        await Button("Ping Ben").ClickAsync();
        await Expect(Line("status-heard")).ToHaveTextAsync("heard");
    }

    [Test]
    public async Task the_hub_reads_as_connected_once_the_page_listens_to_it()
    {
        await Open("/Realtime/Round");
        await Button("Acknowledge as Ada").ClickAsync();
        await Expect(Line("ack")).ToHaveTextAsync("acknowledged by Ada");

        await Button("Read the hub's state").ClickAsync();

        await Expect(Line("hub-state")).ToHaveTextAsync("Connected");
    }
}
