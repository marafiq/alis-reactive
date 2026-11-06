namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenAnItemIsSentWithoutItsList : AppPageTest
{
    private const string NoBag = "Bring each guest's bag.";

    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    private ILocator Summary => Page.Locator("#trip-form_validation_summary");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/DayTrip", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [Test]
    public async Task a_rule_on_an_items_list_reads_the_list_a_new_item_holds_when_the_request_sends_none_of_it()
    {
        await Open("/Validation/DayTrip?bothBags=false");

        await Save();

        await Expect(Summary.GetByText(NoBag, new() { Exact = true })).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");
    }

    [Test]
    public async Task a_rule_on_an_items_list_reads_the_items_the_request_sends()
    {
        await Open("/Validation/DayTrip");

        await Save();

        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Page.GetByText(NoBag)).ToBeHiddenAsync();
    }
}
