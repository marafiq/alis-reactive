namespace Alis.Reactive.Framework.PlaywrightTests.Realtime;

/// <summary>
/// A bed loaded into the ward's slot listens to the ward's line (a hub) and board (a stream); the page may
/// listen too. The server reports, live, how many of the page's line and board connections it holds open.
/// When the slot is unloaded, a connection only it listened to closes, is not offered for retry, reads as
/// closed and withdraws an offer it had; while the page still listens, the connection stays and only the
/// slot's handlers stop hearing it.
/// </summary>
[TestFixture]
public sealed class WhenASlotListensToALiveConnection : AppPageTest
{
    private ILocator Ward(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Bed => Page.Locator("#bed-slot");

    private ILocator RetryOffer => Page.GetByText("The connection dropped.");

    [Test]
    public async Task a_hub_closes_when_the_last_slot_listening_to_it_is_unloaded()
    {
        await Open("/Realtime/Ward");
        await Expect(Ward("hub-lines")).ToHaveTextAsync("0");
        await Button("Open the bed").ClickAsync();
        await Expect(Ward("bed-call")).ToHaveTextAsync("1");
        await Expect(Ward("hub-lines")).ToHaveTextAsync("1");

        await Button("Close the bed").ClickAsync();

        await Expect(Ward("hub-lines")).ToHaveTextAsync("0");
    }

    [Test]
    public async Task a_hub_the_page_stops_listening_to_is_not_offered_for_retry()
    {
        await Open("/Realtime/Ward");
        await Button("Open the bed").ClickAsync();
        await Expect(Ward("hub-lines")).ToHaveTextAsync("1");

        await Button("Close the bed").ClickAsync();

        await Expect(Ward("hub-lines")).ToHaveTextAsync("0");
        await Expect(RetryOffer).ToBeHiddenAsync();
    }

    [Test]
    public async Task unloading_the_slot_whose_hub_the_server_hung_up_on_withdraws_its_retry_offer()
    {
        await Open("/Realtime/Ward");
        await Button("Open the bed").ClickAsync();
        await Expect(Ward("bed-call")).ToHaveTextAsync("1");
        await Button("Hang up the line").ClickAsync();
        await Expect(RetryOffer).ToBeVisibleAsync();

        await Button("Close the bed").ClickAsync();

        await Expect(RetryOffer).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_slots_handler_of_a_hub_method_stops_when_the_slot_is_unloaded_and_the_pages_still_hears()
    {
        await Open("/Realtime/Ward?pageListens=true");
        await Expect(Ward("page-call")).ToHaveTextAsync("1");
        await Button("Open the bed").ClickAsync();
        await Expect(Bed.GetByRole(AriaRole.Heading, new() { Name = "Bed 4" })).ToBeVisibleAsync();
        await Button("Note first").ClickAsync();
        await Expect(Ward("page-note")).ToHaveTextAsync("first");
        await Expect(Ward("bed-note")).ToHaveTextAsync("first");

        await Button("Close the bed").ClickAsync();
        await Expect(Bed).ToBeEmptyAsync();
        await Button("Note second").ClickAsync();

        await Expect(Ward("page-note")).ToHaveTextAsync("second");
        await Expect(Ward("bed-note")).ToHaveTextAsync("first");
    }

    [Test]
    public async Task a_slots_reconnecting_handler_stops_when_the_slot_is_unloaded_and_the_pages_still_runs()
    {
        await Open("/Realtime/Ward?pageListens=true");
        await Expect(Ward("page-call")).ToHaveTextAsync("1");
        await Button("Open the bed").ClickAsync();
        await Expect(Bed.GetByRole(AriaRole.Heading, new() { Name = "Bed 4" })).ToBeVisibleAsync();
        await Button("Drop the line").ClickAsync();
        await Expect(Ward("page-dropped")).ToHaveTextAsync("dropped");
        await Expect(Ward("bed-dropped")).ToHaveTextAsync("dropped");
        await Expect(Ward("page-call")).ToHaveTextAsync("2");
        await Button("Clear what was heard").ClickAsync();
        await Expect(Ward("bed-dropped")).ToHaveTextAsync("");

        await Button("Close the bed").ClickAsync();
        await Expect(Bed).ToBeEmptyAsync();
        await Button("Drop the line").ClickAsync();

        await Expect(Ward("page-dropped")).ToHaveTextAsync("dropped");
        await Expect(Ward("bed-dropped")).ToHaveTextAsync("");
    }

    [Test]
    public async Task a_stream_closes_when_the_last_slot_listening_to_it_is_unloaded()
    {
        await Open("/Realtime/Ward");
        await Expect(Ward("board-lines")).ToHaveTextAsync("0");
        await Button("Open the bed").ClickAsync();
        await Expect(Ward("bed-board-call")).ToHaveTextAsync("1");
        await Expect(Ward("board-lines")).ToHaveTextAsync("1");

        await Button("Close the bed").ClickAsync();

        await Expect(Ward("board-lines")).ToHaveTextAsync("0");
    }

    [Test]
    public async Task a_stream_no_handler_listens_to_any_more_reads_as_closed()
    {
        await Open("/Realtime/Ward");
        await Button("Open the bed").ClickAsync();
        await Expect(Ward("bed-board-call")).ToHaveTextAsync("1");
        await Button("Read the board's state").ClickAsync();
        await Expect(Ward("board-state")).ToHaveTextAsync("1");

        await Button("Close the bed").ClickAsync();
        await Expect(Bed).ToBeEmptyAsync();
        await Button("Read the board's state").ClickAsync();

        await Expect(Ward("board-state")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task unloading_the_slot_whose_stream_the_server_closed_withdraws_its_retry_offer()
    {
        await Open("/Realtime/Ward");
        await Button("Open the bed").ClickAsync();
        await Expect(Ward("bed-board-call")).ToHaveTextAsync("1");
        await Button("Close the board for good").ClickAsync();
        await Expect(RetryOffer).ToBeVisibleAsync();

        await Button("Close the bed").ClickAsync();

        await Expect(RetryOffer).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_slots_handler_of_a_stream_stops_when_the_slot_is_unloaded_and_the_pages_still_hears()
    {
        await Open("/Realtime/Ward?pageListens=true");
        await Expect(Ward("board-lines")).ToHaveTextAsync("1");
        await Button("Open the bed").ClickAsync();
        await Expect(Bed.GetByRole(AriaRole.Heading, new() { Name = "Bed 4" })).ToBeVisibleAsync();
        await Button("Push the first news").ClickAsync();
        await Expect(Ward("page-news")).ToHaveTextAsync("first");
        await Expect(Ward("bed-news")).ToHaveTextAsync("first");

        await Button("Close the bed").ClickAsync();
        await Expect(Bed).ToBeEmptyAsync();
        await Button("Push the second news").ClickAsync();

        await Expect(Ward("page-news")).ToHaveTextAsync("second");
        await Expect(Ward("bed-news")).ToHaveTextAsync("first");
    }
}
