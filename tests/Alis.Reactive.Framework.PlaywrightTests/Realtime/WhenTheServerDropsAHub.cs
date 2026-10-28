namespace Alis.Reactive.Framework.PlaywrightTests.Realtime;

/// <summary>
/// A hub the server drops, hangs up on or refuses. The server welcomes every call it takes and numbers it, so
/// the call number says which connection the page holds. A dropped connection runs its Reconnecting pipeline,
/// comes back under a new id the server knows the page by and runs its Reconnected pipeline; a connection the
/// server closes for good runs its Close pipeline and the page offers to retry it, a click on the offer calls
/// the hub again and the offer goes once it answers; a start the server refuses is tried again, a start
/// refused on every try ends in the offer; and a page with no offer still hears the close.
/// </summary>
[TestFixture]
public sealed class WhenTheServerDropsAHub : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator RetryOffer => Page.GetByText("The connection dropped.");

    [Test]
    public async Task a_hub_the_server_drops_runs_its_reconnecting_pipeline()
    {
        await Open("/Realtime/Line");
        await Expect(Line("line-call")).ToHaveTextAsync("1");
        await Expect(Line("line-dropped")).ToHaveTextAsync("");

        await Button("Drop the line").ClickAsync();

        await Expect(Line("line-dropped")).ToHaveTextAsync("dropped");
    }

    [Test]
    public async Task a_hub_the_server_drops_comes_back_and_runs_its_reconnected_pipeline()
    {
        await Open("/Realtime/Line");
        await Expect(Line("line-call")).ToHaveTextAsync("1");
        await Expect(Line("line-back")).ToHaveTextAsync("");

        await Button("Drop the line").ClickAsync();

        await Expect(Line("line-call")).ToHaveTextAsync("2");
        await Expect(Line("line-back")).ToHaveTextAsync("back");
    }

    [Test]
    public async Task the_id_a_hub_comes_back_under_is_the_one_the_server_now_knows_the_page_by()
    {
        await Open("/Realtime/Line");
        await Expect(Line("line-call")).ToHaveTextAsync("1");

        await Button("Drop the line").ClickAsync();

        await Expect(Line("line-found")).ToHaveTextAsync("found");
    }

    [Test]
    public async Task a_hub_the_server_hangs_up_on_runs_its_close_pipeline()
    {
        await Open("/Realtime/Line");
        await Expect(Line("line-call")).ToHaveTextAsync("1");
        await Expect(Line("line-closed")).ToHaveTextAsync("");

        await Button("Hang up the line").ClickAsync();

        await Expect(Line("line-closed")).ToHaveTextAsync("closed");
    }

    [Test]
    public async Task a_hub_the_server_hangs_up_on_is_offered_for_retry()
    {
        await Open("/Realtime/Line");
        await Expect(Line("line-call")).ToHaveTextAsync("1");
        await Expect(RetryOffer).ToBeHiddenAsync();

        await Button("Hang up the line").ClickAsync();

        await Expect(RetryOffer).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_click_on_retry_calls_a_hub_the_server_hung_up_on_again()
    {
        await Open("/Realtime/Line");
        await Expect(Line("line-call")).ToHaveTextAsync("1");
        await Button("Hang up the line").ClickAsync();
        await Expect(RetryOffer).ToBeVisibleAsync();
        await Button("Answer the line again").ClickAsync();
        await Expect(Line("line-answered")).ToHaveTextAsync("answered");

        await Button("Retry").ClickAsync();

        await Expect(Line("line-call")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task the_retry_offer_goes_once_the_hub_answers_again()
    {
        await Open("/Realtime/Line");
        await Expect(Line("line-call")).ToHaveTextAsync("1");
        await Button("Hang up the line").ClickAsync();
        await Expect(RetryOffer).ToBeVisibleAsync();
        await Button("Answer the line again").ClickAsync();
        await Expect(Line("line-answered")).ToHaveTextAsync("answered");

        await Button("Retry").ClickAsync();

        await Expect(Line("line-call")).ToHaveTextAsync("2");
        await Expect(RetryOffer).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_hub_the_server_refuses_at_first_is_called_again_until_it_answers()
    {
        await Open("/Realtime/Line?refusals=1");

        await Expect(Line("line-call")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_hub_the_server_refuses_on_every_try_is_offered_for_retry()
    {
        await Open("/Realtime/Line?refusals=4");
        await Expect(RetryOffer).ToBeHiddenAsync();

        // The page tries four times, 0, 2, 10 and 30 seconds apart, before it offers to retry.
        await Expect(RetryOffer).ToBeVisibleAsync(new() { Timeout = 60_000 });
        await Expect(Line("line-call")).ToHaveTextAsync("");
    }

    [Test]
    public async Task a_hub_the_server_hangs_up_on_runs_its_close_pipeline_on_a_page_that_offers_no_retry()
    {
        await Open("/Realtime/Line?offersRetry=false");
        await Expect(Line("line-call")).ToHaveTextAsync("1");

        await Button("Hang up the line").ClickAsync();

        await Expect(Line("line-closed")).ToHaveTextAsync("closed");
    }
}
