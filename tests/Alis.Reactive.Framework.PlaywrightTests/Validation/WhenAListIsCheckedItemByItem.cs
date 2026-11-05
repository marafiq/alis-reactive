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
    public async Task a_list_that_stops_answers_for_its_first_failing_item_alone_and_checks_no_item_after_it()
    {
        await Open("/Validation/LinenRound");
        var secondRoomRefused = Field("Second room").GetByText("East wing rooms only.", new() { Exact = true });

        await Write("First room", "W-123");
        await Write("Second room", "W-456");
        await Save();
        await Expect(Field("First room").GetByText("East wing rooms only.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(secondRoomRefused).ToBeHiddenAsync();

        await Write("First room", "E-16");
        await Save();
        await Expect(secondRoomRefused).ToBeVisibleAsync();

        await Write("Second room", "E-18");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_list_of_child_validators_that_stops_answers_with_every_failure_of_its_first_failing_item_alone()
    {
        await Open("/Validation/LinenRound");
        var secondTagRefused = Field("Second trolley's tag").GetByText("Trolley tags start with T-.", new() { Exact = true });
        var secondWingRefused = Field("Second trolley's wing").GetByText("Send a trolley East or West.", new() { Exact = true });

        await Write("First trolley's tag", "Q-1");
        await Write("First trolley's wing", "North");
        await Write("Second trolley's tag", "Q-2");
        await Write("Second trolley's wing", "South");
        await Save();
        await Expect(Field("First trolley's tag").GetByText("Trolley tags start with T-.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Field("First trolley's wing").GetByText("Send a trolley East or West.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(secondTagRefused).ToBeHiddenAsync();
        await Expect(secondWingRefused).ToBeHiddenAsync();

        await Write("First trolley's tag", "T-1");
        await Write("First trolley's wing", "East");
        await Save();
        await Expect(secondTagRefused).ToBeVisibleAsync();
        await Expect(secondWingRefused).ToBeVisibleAsync();

        await Write("Second trolley's tag", "T-2");
        await Write("Second trolley's wing", "West");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_custom_check_on_a_list_that_stops_answers_for_its_first_failing_item_alone()
    {
        await Open("/Validation/LinenRound");
        var secondShelfRefused = Field("Second shelf").GetByText("Shelf X-2 is closed.", new() { Exact = true });

        await Write("First shelf", "X-1");
        await Write("Second shelf", "X-2");
        await Save();
        await Expect(Field("First shelf").GetByText("Shelf X-1 is closed.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(secondShelfRefused).ToBeHiddenAsync();

        await Write("First shelf", "S-1");
        await Save();
        await Expect(secondShelfRefused).ToBeVisibleAsync();

        await Write("Second shelf", "S-2");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_custom_check_on_a_child_validators_list_answers_beside_the_item_it_refuses()
    {
        await Open("/Validation/LinenRound");

        await Write("Second laundry bag", "L-13");
        await Save();
        await Expect(Field("Second laundry bag").GetByText("Bag L-13 is torn.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Field("First laundry bag").GetByText("is torn.")).ToBeHiddenAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make a custom check, so the request goes");

        await Write("Second laundry bag", "L-3");
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
