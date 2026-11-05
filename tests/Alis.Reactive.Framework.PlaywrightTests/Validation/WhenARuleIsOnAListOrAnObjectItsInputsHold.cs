namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenARuleIsOnAListOrAnObjectItsInputsHold : AppPageTest
{
    private const string NoRoom = "Name a room for the round.";
    private const string NoLead = "Name a lead for the round.";
    private const string EmptyLead = "Give the round a lead.";
    private const string Badge = "The north wing asks for a badge.";
    private const string EastKey = "Take the east key for E-9.";
    private const string Porter = "Two rooms or more call for a porter.";
    private const string Trolley = "More than two rooms call for a trolley.";
    private const string Helper = "A round with needs takes a helper.";
    private const string Checklist = "A round with no needs signs the checklist.";
    private const string Lift = "Two needs or more call for the lift.";

    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/RoomRound", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label)
        => Page.Locator("#round-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Pick(string button) => Page.GetByRole(AriaRole.Button, new() { Name = button, Exact = true }).ClickAsync();

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [Test]
    public async Task a_rule_on_a_list_its_item_inputs_hold_reads_the_items_they_hold()
    {
        await Open("/Validation/RoomRound");

        await Save();

        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Page.GetByText(NoRoom)).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_rule_that_an_object_is_bound_is_left_to_the_server()
    {
        await Open("/Validation/RoomRound");

        await Save();

        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Page.GetByText(NoLead)).ToBeHiddenAsync();
        await Expect(Page.GetByText(EmptyLead)).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_list_beside_the_form_reads_the_item_inputs_the_page_renders_for_it()
    {
        await Open("/Validation/RoomRound");
        await Write("Badge", "");

        await Write("Wing", "North");
        await Save();
        await Expect(Field("Badge").GetByText(Badge)).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Wing", "East");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Field("Badge").GetByText(Badge)).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_list_its_item_inputs_hold_holds_the_item_one_of_them_names()
    {
        await Open("/Validation/RoomRound");
        await Write("East key", "");

        await Write("Second room", "E-9");
        await Save();
        await Expect(Field("East key").GetByText(EastKey)).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Second room", "E-2");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Field("East key").GetByText(EastKey)).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_list_its_item_inputs_hold_counts_one_item_for_each_of_them()
    {
        await Open("/Validation/RoomRound");
        await Write("Porter", "");
        await Write("Trolley", "");

        await Save();
        await Expect(Field("Porter").GetByText(Porter)).ToBeVisibleAsync();
        await Expect(Field("Trolley").GetByText(Trolley)).ToBeHiddenAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Porter", "Sam");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task any_holds_once_the_list_one_input_holds_has_an_item()
    {
        await Open("/Validation/RoomRound");
        await Write("Helper", "");

        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Field("Helper").GetByText(Helper)).ToBeHiddenAsync();

        await Pick("Pick the east wing tag");
        await Save();
        await Expect(Field("Helper").GetByText(Helper)).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Has.Count.EqualTo(1), "the browser held the second request");
    }

    [Test]
    public async Task a_negated_any_holds_while_the_list_one_input_holds_has_no_item()
    {
        await Open("/Validation/RoomRound");
        await Write("Checklist", "");

        await Save();
        await Expect(Field("Checklist").GetByText(Checklist)).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Pick("Pick the east wing tag");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Field("Checklist").GetByText(Checklist)).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_count_compares_the_items_the_list_one_input_holds()
    {
        await Open("/Validation/RoomRound");
        await Write("Lift", "");

        await Pick("Pick the east wing tag");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Field("Lift").GetByText(Lift)).ToBeHiddenAsync();

        await Pick("Pick the north wing tag");
        await Save();
        await Expect(Field("Lift").GetByText(Lift)).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Has.Count.EqualTo(1), "the browser held the second request");
    }
}
