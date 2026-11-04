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

    [Test]
    public async Task a_custom_check_on_each_item_answers_beside_the_item_it_refuses_with_the_lists_name_and_the_items_value()
    {
        await Open("/Validation/LinenRound");

        await Write("Second bin", "B-13");
        await Save();
        await Expect(Field("Second bin").GetByText("'Bins' B-13 is out of service.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Field("First bin").GetByText("is out of service.")).ToBeHiddenAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make a custom check, so the request goes");

        await Write("Second bin", "B-3");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_custom_check_on_an_item_puts_a_failure_under_another_member_beside_that_members_input()
    {
        await Open("/Validation/LinenRound");

        await Write("Second bin", "B-99");
        await Save();
        await Expect(Field("Porter").GetByText("Bin B-99 goes to the porter; name one.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Field("Second bin").GetByText("goes to the porter")).ToBeHiddenAsync();

        await Write("Porter", "Ed");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }
}
