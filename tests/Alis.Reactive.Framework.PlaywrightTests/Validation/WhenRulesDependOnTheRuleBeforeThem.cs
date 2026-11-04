namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenRulesDependOnTheRuleBeforeThem : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

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

    private ILocator Field(string label)
        => Page.Locator("#trip-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [Test]
    public async Task a_dependent_rule_inside_a_lists_items_waits_in_the_browser_while_its_own_items_rule_fails()
    {
        await Open("/Validation/DayTrip");
        var firstRelationWanted = Field("First guest's relation").GetByText("Say how each guest is related.", new() { Exact = true });
        var secondRelationWanted = Field("Second guest's relation").GetByText("Say how each guest is related.", new() { Exact = true });

        await Write("First guest", "");
        await Write("First guest's relation", "");
        await Write("Second guest's relation", "");
        await Save();
        await Expect(Field("First guest").GetByText("Name each guest.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(secondRelationWanted).ToBeVisibleAsync();
        await Expect(firstRelationWanted).ToBeHiddenAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("First guest", "Cy");
        await Write("First guest's relation", "Son");
        await Write("Second guest's relation", "Friend");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_dependent_rule_inside_a_lists_items_waits_on_the_server_while_its_own_items_rule_fails()
    {
        await Open("/Validation/DayTrip");
        var firstPhoneWanted = Field("First badge holder's phone").GetByText("Give each badge holder's phone.", new() { Exact = true });
        var secondPhoneWanted = Field("Second badge holder's phone").GetByText("Give each badge holder's phone.", new() { Exact = true });

        await Write("First badge", "W-1");
        await Write("First badge holder's phone", "");
        await Write("Second badge holder's phone", "");
        await Save();
        await Expect(Field("First badge").GetByText("Visitor badges start with V-.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(secondPhoneWanted).ToBeVisibleAsync();
        await Expect(firstPhoneWanted).ToBeHiddenAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make the badge's check, so neither the phone's; the request goes");

        await Write("First badge", "V-3");
        await Write("First badge holder's phone", "555-0133");
        await Write("Second badge holder's phone", "555-0144");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_dependent_rule_inside_a_condition_that_does_not_hold_applies_no_more_than_the_rule_before_it()
    {
        await Open("/Validation/DayTrip");
        var licenceWanted = Field("Licence").GetByText("Give the driver's licence.", new() { Exact = true });

        await Write("Transport", "Van");
        await Write("Driver", "Bo");
        await Save();
        await Expect(licenceWanted).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Transport", "Walk");
        await Write("Driver", "");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(licenceWanted).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_dependent_rule_after_a_child_validator_is_the_servers_and_waits_while_the_child_fails()
    {
        await Open("/Validation/DayTrip");
        var courierWanted = Field("Courier").GetByText("Name the courier.", new() { Exact = true });

        await Write("Courier", "");
        await Save();
        await Expect(courierWanted).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "a rule after a child validator is the server's; the request goes");

        await Write("Pharmacy phone", "556-0101");
        await Save();
        await Expect(Field("Pharmacy phone").GetByText("Use a 555 number.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(courierWanted).ToBeHiddenAsync();

        await Write("Pharmacy phone", "555-0102");
        await Write("Courier", "Dot");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_dependent_rule_after_a_lists_browser_checks_is_decided_in_the_browser_and_waits_while_any_item_fails()
    {
        await Open("/Validation/DayTrip");
        var coachWanted = Field("Coach").GetByText("Name the coach.", new() { Exact = true });

        await Write("Coach", "");
        await Write("Second seat", "");
        await Save();
        await Expect(Field("Second seat").GetByText("Give each seat.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(coachWanted).ToBeHiddenAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Second seat", "1C");
        await Save();
        await Expect(coachWanted).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "every seat passes, so the browser asks for the coach and holds the request");

        await Write("Coach", "Red");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_dependent_rule_after_a_lists_browser_checks_applies_in_the_browser_when_the_form_holds_no_item()
    {
        await Open("/Validation/DayTrip?seats=false");

        await Write("Coach", "");
        await Save();
        await Expect(Field("Coach").GetByText("Name the coach.", new() { Exact = true })).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "no item fails, so the browser asks for the coach and holds the request");

        await Write("Coach", "Red");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_dependent_rule_after_a_list_inside_each_item_waits_in_the_browser_only_on_its_own_items_list()
    {
        await Open("/Validation/DayTrip");
        var firstPorterWanted = Field("First guest's porter").GetByText("Name each guest's porter.", new() { Exact = true });
        var secondPorterWanted = Field("Second guest's porter").GetByText("Name each guest's porter.", new() { Exact = true });

        await Write("First guest's bag", "");
        await Write("First guest's porter", "");
        await Write("Second guest's porter", "");
        await Save();
        await Expect(Field("First guest's bag").GetByText("Tag each bag.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(secondPorterWanted).ToBeVisibleAsync();
        await Expect(firstPorterWanted).ToBeHiddenAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("First guest's bag", "Green case");
        await Write("First guest's porter", "Lu");
        await Write("Second guest's porter", "Mo");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_dependent_rule_after_a_lists_rule_is_the_servers_and_waits_while_any_item_fails()
    {
        await Open("/Validation/DayTrip");
        var returnWanted = Field("Return by").GetByText("Say when the trip returns.", new() { Exact = true });

        await Write("Return by", "");
        await Save();
        await Expect(returnWanted).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make the stops' check, so neither the return's; the request goes");

        await Write("Second stop", "Zoo");
        await Save();
        await Expect(Field("Second stop").GetByText("Pick a stop on the route.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(returnWanted).ToBeHiddenAsync();

        await Write("Second stop", "Museum");
        await Write("Return by", "17:00");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }
}
