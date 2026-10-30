namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// One form two requests send, both validated by the form's one validator: each request is held by the
/// validator's rules, its message beside its own field and nothing sent, and goes once they hold.
/// </summary>
[TestFixture]
public sealed class WhenTwoRequestsValidateOneForm : AppPageTest
{
    private readonly List<string> _saves = new();

    // Every request that reached either endpoint, as the network carried it.
    [SetUp]
    public void WatchTheHandoverEndpoints()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.Contains("/Validation/Handover", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Url);
        };
    }

    private ILocator Saved => Page.Locator("#saved");

    private ILocator ResidentWanted =>
        Page.Locator("#handover-form > div")
            .Filter(new() { Has = Page.GetByLabel("Resident") })
            .GetByText("Name the resident.", new() { Exact = true });

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_draft_is_held_until_it_names_its_resident()
    {
        await Open("/Validation/Handover");

        await Click("Save the draft");
        await Expect(ResidentWanted).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the draft");

        await Page.GetByLabel("Resident").FillAsync("Ada");
        await Click("Save the draft");
        await Expect(Saved).ToHaveTextAsync("draft saved for Ada");
        await Expect(ResidentWanted).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_handover_is_held_until_it_names_its_resident()
    {
        await Open("/Validation/Handover");

        await Click("Hand over");
        await Expect(ResidentWanted).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the handover");

        await Page.GetByLabel("Resident").FillAsync("Bea");
        await Click("Hand over");
        await Expect(Saved).ToHaveTextAsync("handed over for Bea");
        await Expect(ResidentWanted).ToBeHiddenAsync();
    }
}
