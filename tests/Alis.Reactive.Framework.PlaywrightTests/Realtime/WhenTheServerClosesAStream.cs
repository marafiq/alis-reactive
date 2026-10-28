namespace Alis.Reactive.Framework.PlaywrightTests.Realtime;

/// <summary>
/// A server-sent event stream the server closes for good: it ends the stream and answers the browser's next
/// call with 204, which a browser does not retry. The server welcomes every call it takes and numbers it. The
/// page offers to retry the stream, a click on the offer opens it again with the page's handlers still
/// listening, and the offer goes once it is open; and an event whose data is not JSON runs no pipeline.
/// </summary>
[TestFixture]
public sealed class WhenTheServerClosesAStream : AppPageTest
{
    private ILocator Board(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator RetryOffer => Page.GetByText("The connection dropped.");

    [Test]
    public async Task a_stream_the_server_closes_for_good_is_offered_for_retry()
    {
        await Open("/Realtime/Line");
        await Expect(Board("board-call")).ToHaveTextAsync("1");
        await Expect(RetryOffer).ToBeHiddenAsync();

        await Button("Close the board for good").ClickAsync();

        await Expect(RetryOffer).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_click_on_retry_opens_a_stream_the_server_closed_again_and_its_pipelines_run()
    {
        await Open("/Realtime/Line");
        await Expect(Board("board-call")).ToHaveTextAsync("1");
        await Button("Close the board for good").ClickAsync();
        await Expect(RetryOffer).ToBeVisibleAsync();
        await Button("Take the board back").ClickAsync();
        await Expect(Board("board-taken-back")).ToHaveTextAsync("taken back");

        await Button("Retry").ClickAsync();

        await Expect(Board("board-call")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task the_retry_offer_goes_once_the_stream_is_open_again()
    {
        await Open("/Realtime/Line");
        await Expect(Board("board-call")).ToHaveTextAsync("1");
        await Button("Close the board for good").ClickAsync();
        await Expect(RetryOffer).ToBeVisibleAsync();
        await Button("Take the board back").ClickAsync();
        await Expect(Board("board-taken-back")).ToHaveTextAsync("taken back");

        await Button("Retry").ClickAsync();

        await Expect(Board("board-call")).ToHaveTextAsync("2");
        await Expect(RetryOffer).ToBeHiddenAsync();
    }

    [Test]
    public async Task an_event_whose_data_is_not_json_runs_no_pipeline()
    {
        await Open("/Realtime/Line");
        await Expect(Board("board-call")).ToHaveTextAsync("1");

        await Button("Push a garbled bulletin").ClickAsync();
        await Expect(Board("all-clear")).ToHaveTextAsync("all clear");
        await Expect(Board("bulletin")).ToHaveTextAsync("");

        await Button("Push a bulletin").ClickAsync();
        await Expect(Board("bulletin")).ToHaveTextAsync("a bulletin came");
    }
}
