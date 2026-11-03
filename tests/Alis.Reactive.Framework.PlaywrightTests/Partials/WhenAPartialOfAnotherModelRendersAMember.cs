namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// The community's person line, a partial of a model of its own, renders the concierge's next of kin under the
/// member's prefix, so its input binds as <c>NextOfKin.Name</c> though no class of the page's model declares it.
/// The page reaches the member by its binding name, as MVC binds it (grammar.md M8): <c>p.Bound</c> reads that
/// input, a named send sends it and the page's rule checks it, and the partial's own bound read names its member
/// under the prefix it was rendered with.
/// </summary>
[TestFixture]
public sealed class WhenAPartialOfAnotherModelRendersAMember : AppPageTest
{
    private readonly List<string> _saves = new();

    [SetUp]
    public void WatchTheConciergesEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Partials/SaveConcierge", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Url);
        };
    }

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private ILocator Field(string label) => Page.Locator(".alis-field").Filter(new() { Has = Input(label) });

    [Test]
    public async Task the_bound_value_of_the_member_is_what_the_partials_input_holds()
    {
        await Open("/Partials/Concierge");
        await Input("Next of kin").FillAsync("Bo");

        await Button("Read the next of kin").ClickAsync();

        await Expect(Page.Locator("#kin-bound")).ToHaveTextAsync("Bo");
    }

    [Test]
    public async Task a_named_send_of_the_member_sends_what_the_partials_input_holds()
    {
        await Open("/Partials/Concierge");
        await Input("Next of kin").FillAsync("Bo");

        await Button("Note the next of kin").ClickAsync();

        await Expect(Page.Locator("#kin-noted")).ToHaveTextAsync("next of kin Bo");
    }

    [Test]
    public async Task the_pages_rule_on_the_member_checks_the_partials_input()
    {
        await Open("/Partials/Concierge");
        await Input("Visitor").FillAsync("Ann");

        await Button("Save the visit").ClickAsync();
        await Expect(Field("Next of kin").GetByText("Name the next of kin.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the visit");

        await Input("Next of kin").FillAsync("Bo");
        await Button("Save the visit").ClickAsync();
        await Expect(Page.Locator("#concierge-saved")).ToHaveTextAsync("visitor Ann, next of kin Bo");
    }

    [Test]
    public async Task the_partials_own_bound_read_names_its_member_under_its_prefix()
    {
        await Open("/Partials/Concierge");
        await Input("Next of kin").FillAsync("Bo");

        await Button("Read the name back").ClickAsync();

        await Expect(Page.Locator("#name-read-back")).ToHaveTextAsync("Bo");
    }
}
