namespace Alis.Reactive.Framework.PlaywrightTests.Realtime;

/// <summary>
/// One retry offer for every connection the server closed for good, a hub and a stream on one page: one click
/// retries each of them, and the offer stays while any of them is still closed. The server numbers the calls
/// it takes on each.
/// </summary>
[TestFixture]
public sealed class WhenSeveralConnectionsDrop : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator RetryOffer => Page.GetByText("The connection dropped.");

    [Test]
    public async Task one_click_on_retry_retries_every_connection_the_server_closed()
    {
        await Open("/Realtime/Line");
        await Expect(Line("line-call")).ToHaveTextAsync("1");
        await Expect(Line("board-call")).ToHaveTextAsync("1");
        await Button("Hang up the line").ClickAsync();
        await Expect(Line("line-closed")).ToHaveTextAsync("closed");
        await Button("Close the board for good").ClickAsync();
        await Expect(RetryOffer).ToBeVisibleAsync();
        await Button("Answer the line again").ClickAsync();
        await Expect(Line("line-answered")).ToHaveTextAsync("answered");
        await Button("Take the board back").ClickAsync();
        await Expect(Line("board-taken-back")).ToHaveTextAsync("taken back");

        await Button("Retry").ClickAsync();

        await Expect(Line("line-call")).ToHaveTextAsync("2");
        await Expect(Line("board-call")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task the_retry_offer_stays_while_any_connection_is_still_closed()
    {
        await Open("/Realtime/Line");
        await Expect(Line("line-call")).ToHaveTextAsync("1");
        await Expect(Line("board-call")).ToHaveTextAsync("1");
        await Button("Hang up the line").ClickAsync();
        await Expect(Line("line-closed")).ToHaveTextAsync("closed");
        await Button("Close the board for good").ClickAsync();
        await Expect(RetryOffer).ToBeVisibleAsync();
        await Button("Take the board back").ClickAsync();
        await Expect(Line("board-taken-back")).ToHaveTextAsync("taken back");

        await Button("Retry").ClickAsync();

        // A stream is open before its first event arrives: the board's welcome follows its open.
        await Expect(Line("board-call")).ToHaveTextAsync("2");
        await Expect(RetryOffer).ToBeVisibleAsync();
    }
}
