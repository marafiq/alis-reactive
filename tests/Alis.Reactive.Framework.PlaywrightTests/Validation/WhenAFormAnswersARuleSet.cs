namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// A form that names a rule set is checked by that set's rules alone, in the browser and on the server, and a
/// form that names none by the rules in no set. The set reaches a child validator through a rule in the set,
/// and an included validator written in no set.
/// </summary>
[TestFixture]
public sealed class WhenAFormAnswersARuleSet : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.Contains("/Validation/Discharge", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label) =>
        Page.Locator("#discharge-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    [Test]
    public async Task a_form_that_names_a_set_is_held_by_that_sets_rule_and_saved_once_it_holds()
    {
        await Open("/Validation/Discharge?stage=Draft");
        await Write("Summary", "");
        await Save();
        await Expect(Field("Summary").GetByText("Write the summary before saving a draft.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Summary", "Home.");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Draft saved for Ada");
    }

    [Test]
    public async Task a_form_that_names_a_set_is_not_checked_by_another_sets_rules_on_either_side()
    {
        await Open("/Validation/Discharge?stage=Draft");
        await Write("Summary", "Short");
        await Write("Signed by", "Nurse Kay");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Draft saved for Ada");
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the sign-off's length rule did not hold the draft, and its signature rule did not refuse it");
    }

    [Test]
    public async Task a_sets_browser_rule_holds_the_form_that_names_the_set()
    {
        await Open("/Validation/Discharge?stage=SignOff");
        await Write("Summary", "Short");
        await Save();
        await Expect(Field("Summary").GetByText("A signed-off summary says more.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");
    }

    [Test]
    public async Task a_sets_rule_the_server_alone_makes_is_answered_by_the_server_under_that_set()
    {
        await Open("/Validation/Discharge?stage=SignOff");
        await Write("Signed by", "Nurse Kay");
        await Save();
        await Expect(Field("Signed by").GetByText("Only a doctor on the ward signs off.")).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make this check, so the request goes");

        await Write("Signed by", "Dr Ng");
        await Save();
        await Expect(Saved).ToHaveTextAsync("SignOff saved for Ada");
    }

    [TestCase("Draft")]
    [TestCase("SignOff")]
    public async Task a_rule_declared_in_two_sets_holds_a_form_that_names_either(string stage)
    {
        await Open("/Validation/Discharge?stage=" + stage);
        await Write("Resident", "");
        await Save();
        await Expect(Field("Resident").GetByText("Name the resident.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");
    }

    [Test]
    public async Task a_form_that_names_no_set_is_checked_by_the_rules_in_no_set_alone()
    {
        await Open("/Validation/Discharge");
        await Write("Ward", "");
        await Save();
        await Expect(Field("Ward").GetByText("Name the ward.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Ward", "East");
        await Write("Summary", "");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved for Ada");
    }

    [Test]
    public async Task a_form_that_names_a_set_is_not_checked_by_the_rules_in_no_set()
    {
        await Open("/Validation/Discharge?stage=Draft");
        await Write("Ward", "");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Draft saved for Ada");
    }

    [Test]
    public async Task a_set_reaches_a_child_validator_through_a_rule_in_the_set()
    {
        await Open("/Validation/Discharge?stage=SignOff");
        await Write("Carrier", "");
        await Save();
        await Expect(Field("Carrier").GetByText("Name who takes the resident home.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Open("/Validation/Discharge?stage=Draft");
        await Write("Carrier", "");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Draft saved for Ada");
    }

    [Test]
    public async Task a_set_reaches_an_included_validator_written_in_no_set()
    {
        await Open("/Validation/Discharge?stage=Draft");
        await Write("Medication", "");
        await Save();
        await Expect(Field("Medication").GetByText("List the medication to hand back.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Open("/Validation/Discharge?stage=SignOff");
        await Write("Medication", "");
        await Save();
        await Expect(Saved).ToHaveTextAsync("SignOff saved for Ada");
    }

    [Test]
    public async Task a_validation_naming_a_set_the_validator_does_not_declare_is_refused_naming_the_declared_sets()
    {
        await Open("/Validation/Discharge");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save under a stage the rules do not declare", Exact = true }).ClickAsync();
        await Expect(Saved).ToHaveTextAsync("DischargeRules declares no rule set 'Drafts'. It declares: Draft, SignOff.");
    }
}
