namespace Alis.Reactive.Framework.PlaywrightTests.Inputs;

/// <summary>
/// One partial of its own model rendered for two members of the page's model, under two field prefixes (I9):
/// each rendering sends its inputs under its own member and checks its own inputs, a rule for each item of a
/// list checks the item a rendering renders under its member, a step that names one of the partial's inputs by
/// its member reaches the input its own rendering rendered, and a plain form post sends each input under the
/// binding path its field gave it.
/// </summary>
[TestFixture]
public sealed class WhenAPartialIsRenderedForTwoMembers : AppPageTest
{
    private readonly List<string> _saves = new();

    [SetUp]
    public void WatchTheContactsEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Inputs/Contacts", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Bound => Page.Locator("#contacts-bound");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private ILocator Field(string label) => Page.Locator("section > div").Filter(new() { Has = Input(label) });

    [Test]
    public async Task each_rendering_sends_its_inputs_under_its_own_member()
    {
        await Open("/Inputs/Contacts");
        await Input("Primary contact name").FillAsync("Ann");
        await Input("Alternate contact name").FillAsync("Bob");

        await Button("Save the primary contact").ClickAsync();
        await Expect(Bound).ToHaveTextAsync("primary Ann (555 0100), alternate none (none)");

        await Button("Save the alternate contact").ClickAsync();
        await Expect(Bound).ToHaveTextAsync("primary none (none), alternate Bob (555 0199)");
    }

    [Test]
    public async Task each_rendering_checks_its_own_inputs()
    {
        await Open("/Inputs/Contacts");
        await Input("Alternate contact name").FillAsync("Bob");

        await Button("Save the alternate contact").ClickAsync();
        await Expect(Bound).ToHaveTextAsync("primary none (none), alternate Bob (555 0199)");

        await Button("Save the primary contact").ClickAsync();
        await Expect(Field("Primary contact name").GetByText("Name the contact.")).ToBeVisibleAsync();
        await Expect(Field("Alternate contact name").GetByText("Name the contact.")).ToBeHiddenAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "only the alternate's save was sent");
    }

    [Test]
    public async Task a_rule_for_each_item_checks_the_item_a_rendering_renders_under_its_member()
    {
        await Open("/Inputs/Contacts");
        await Input("Primary contact name").FillAsync("Ann");
        await Input("Primary contact phone").FillAsync("");

        await Button("Save the primary contact").ClickAsync();

        await Expect(Field("Primary contact phone").GetByText("Give a phone number.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");
    }

    [Test]
    public async Task a_step_naming_an_input_by_its_member_reaches_the_input_its_own_rendering_rendered()
    {
        await Open("/Inputs/Contacts");
        await Input("Primary contact name").FillAsync("Ann");
        await Input("Alternate contact name").FillAsync("Bob");

        await Button("Read back the alternate contact").ClickAsync();

        await Expect(Page.Locator("#alternate-read")).ToHaveTextAsync("Bob");
    }

    [Test]
    public async Task a_plain_form_post_sends_each_input_under_the_binding_path_its_field_gave_it()
    {
        await Open("/Inputs/Contacts");
        await Input("Primary contact name").FillAsync("Ann");
        await Input("Alternate contact name").FillAsync("Bob");

        await Button("Post both contacts").ClickAsync();

        await Expect(Page.GetByText("Posted: primary Ann (555 0100), alternate Bob (555 0199)")).ToBeVisibleAsync();
    }
}
