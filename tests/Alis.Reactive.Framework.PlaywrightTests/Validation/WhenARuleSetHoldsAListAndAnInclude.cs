namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenARuleSetHoldsAListAndAnInclude : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.Contains("/Validation/MoveIn", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label)
        => Page.Locator("#movein-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [Test]
    public async Task a_lists_rule_in_a_set_checks_each_item_only_when_the_form_names_the_set()
    {
        await Open("/Validation/MoveIn?stage=Arrive");

        await Write("Second key", "");
        await Save();
        await Expect(Field("Second key").GetByText("Log each key handed over.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Field("First key").GetByText("Log each key handed over.")).ToBeHiddenAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Second key", "K-3");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Arrive saved for Ada");

        await Open("/Validation/MoveIn");

        await Write("Second key", "");
        await Save();

        await Expect(Saved).ToHaveTextAsync("saved for Ada");
        await Expect(Field("Second key").GetByText("Log each key handed over.")).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_child_validator_on_a_lists_items_runs_its_rules_of_the_set_its_rule_is_in_and_no_other()
    {
        await Open("/Validation/MoveIn?stage=Arrive");

        await Write("Guest moving in", "");
        await Write("Guest's phone", "");
        await Save();
        await Expect(Field("Guest moving in").GetByText("Name each guest moving in.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Field("Guest's phone").GetByText("Give each guest's phone.")).ToBeHiddenAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Guest moving in", "Cy");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Arrive saved for Ada");

        await Open("/Validation/MoveIn");

        await Write("Guest moving in", "");
        await Write("Guest's phone", "");
        await Save();
        await Expect(Field("Guest's phone").GetByText("Give each guest's phone.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Field("Guest moving in").GetByText("Name each guest moving in.")).ToBeHiddenAsync();

        await Write("Guest's phone", "555-0122");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved for Ada");
    }

    [Test]
    public async Task rules_an_include_in_no_set_holds_in_no_set_stay_silent_when_the_form_names_a_set()
    {
        await Open("/Validation/MoveIn?stage=Arrive");
        var policy = Field("Policy");

        await Write("Policy", "P-123456789");
        await Save();
        await Expect(policy.GetByText("A policy number has eight characters at most.", new() { Exact = true })).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Policy", "");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Arrive saved for Ada");
        await Expect(policy.GetByText("Give the policy number.")).ToBeHiddenAsync();

        await Open("/Validation/MoveIn");

        await Write("Policy", "");
        await Save();
        await Expect(Field("Policy").GetByText("Give the policy number.", new() { Exact = true })).ToBeVisibleAsync();

        await Write("Policy", "P-2002");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved for Ada");
    }

    [Test]
    public async Task rules_included_in_a_set_make_their_checks_of_that_set_in_the_browser_and_on_the_server_only_when_the_form_names_it()
    {
        await Open("/Validation/MoveIn?stage=Arrive");
        var insurer = Field("Insurer");

        await Write("Insurer", "");
        await Save();
        await Expect(insurer.GetByText("Name the insurer.", new() { Exact = true })).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Insurer", "Acme");
        await Save();
        await Expect(insurer.GetByText("Bill an insurer the home works with.", new() { Exact = true })).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make the C# check, so the request goes");

        await Write("Insurer", "Shield");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Arrive saved for Ada");

        await Open("/Validation/MoveIn");

        await Write("Insurer", "");
        await Save();

        await Expect(Saved).ToHaveTextAsync("saved for Ada");
        await Expect(Field("Insurer").GetByText("Name the insurer.")).ToBeHiddenAsync();
    }
}
