namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenAListIsCheckedItemByItem : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/LinenRound", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label)
        => Page.Locator("#round-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [Test]
    public async Task an_item_of_a_list_that_stops_answers_with_its_first_failing_check_alone()
    {
        await Open("/Validation/LinenRound");

        await Write("Second room", "W-123");
        await Save();
        await Expect(Field("Second room").GetByText("East wing rooms only.", new() { Exact = true })).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make these checks, so the request goes");

        await Write("Second room", "E-16");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task an_item_of_a_list_that_continues_answers_with_every_failing_check()
    {
        await Open("/Validation/LinenRound");

        await Write("Second cart", "X-12");
        await Save();
        await Expect(Field("Second cart").GetByText("Carts start with C-. Three characters a cart.", new() { Exact = true })).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make these checks, so the request goes");

        await Write("Second cart", "C-3");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }
}
