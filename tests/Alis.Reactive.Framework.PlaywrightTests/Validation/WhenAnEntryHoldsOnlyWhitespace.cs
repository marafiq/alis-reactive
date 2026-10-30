namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// Whitespace typed where the server binds no text (a record number, a time of day, a letter, a link) is
/// nothing bound, read the way MVC binds it: <c>NotEmpty</c> holds the ride in the browser with its message,
/// and the server, sent the same entry unchecked, returns it with the same message, because the browser sends
/// what it read.
/// </summary>
[TestFixture]
public sealed class WhenAnEntryHoldsOnlyWhitespace : AppPageTest
{
    private readonly List<string> _bookings = new();

    // Every ride that reached the booking endpoint, as the network carried it.
    [SetUp]
    public void WatchTheBookingEndpoint()
    {
        _bookings.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Ride", StringComparison.Ordinal))
                lock (_bookings) _bookings.Add(request.Method);
        };
    }

    private ILocator RideStatus => Page.Locator("#ride-status");

    private ILocator Message(string label, string message) =>
        Page.Locator("#ride-form > div")
            .Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) })
            .GetByText(message, new() { Exact = true });

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_ride_with_every_detail_given_is_booked()
    {
        await Open("/Validation/Ride");

        await Click("Book the ride");

        await Expect(RideStatus).ToHaveTextAsync("booked from wing B at 14:30");
    }

    [TestCase("Resident record", "Give the resident's record number.", TestName = "a_record_number_of_only_whitespace_is_held_with_its_message")]
    [TestCase("Pick-up time", "Give the pick-up time.", TestName = "a_time_of_only_whitespace_is_held_with_its_message")]
    [TestCase("Wing", "Give the wing to collect from.", TestName = "a_letter_of_only_whitespace_is_held_with_its_message")]
    [TestCase("Directions link", "Paste the clinic's directions link.", TestName = "a_link_of_only_whitespace_is_held_with_its_message")]
    public async Task whitespace_is_held_in_the_browser(string label, string message)
    {
        await Open("/Validation/Ride");
        await Page.GetByLabel(label, new() { Exact = true }).FillAsync(" ");

        await Click("Book the ride");

        await Expect(Message(label, message)).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_bookings) Assert.That(_bookings, Is.Empty, "the browser held the ride");
    }

    [TestCase("Resident record", "Give the resident's record number.", TestName = "the_server_returns_a_record_number_of_only_whitespace_with_the_same_message")]
    [TestCase("Pick-up time", "Give the pick-up time.", TestName = "the_server_returns_a_time_of_only_whitespace_with_the_same_message")]
    [TestCase("Wing", "Give the wing to collect from.", TestName = "the_server_returns_a_letter_of_only_whitespace_with_the_same_message")]
    [TestCase("Directions link", "Paste the clinic's directions link.", TestName = "the_server_returns_a_link_of_only_whitespace_with_the_same_message")]
    public async Task whitespace_sent_unchecked_is_returned_by_the_server(string label, string message)
    {
        await Open("/Validation/Ride");
        await Page.GetByLabel(label, new() { Exact = true }).FillAsync(" ");

        await Click("Ask the desk to book it");

        await Expect(RideStatus).ToHaveTextAsync("returned by the desk");
        await Expect(Message(label, message)).ToBeVisibleAsync();
    }
}
