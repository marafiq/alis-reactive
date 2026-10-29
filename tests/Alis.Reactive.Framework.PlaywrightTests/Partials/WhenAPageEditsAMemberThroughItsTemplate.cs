namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// A page edits a member of its model through the member's editor template, a partial of the member's own
/// model rendered under the member's prefix (ADR-0003 rule 1: an input's id follows its member). The page
/// reads the template's input by the member, and a rule of the page's validator on the member, the browser's
/// or the server's, shows its message beside the template's field.
/// </summary>
[TestFixture]
public sealed class WhenAPageEditsAMemberThroughItsTemplate : AppPageTest
{
    private readonly List<string> _saves = new();

    [SetUp]
    public void WatchTheDesksEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Partials/SaveDesk", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private ILocator KinField => Page.Locator("#kin-card > div").Filter(new() { Has = Input("Next of kin") });

    private ILocator DeskSummary => Page.Locator("#desk-messages [data-reactive-validation-summary]");

    [Test]
    public async Task the_page_reads_the_templates_input_by_the_member()
    {
        await Open("/Partials/Desk");
        await Input("Next of kin").FillAsync("Bo");

        await Button("Read the next of kin").ClickAsync();

        await Expect(Page.Locator("#kin-read")).ToHaveTextAsync("Bo");
    }

    [Test]
    public async Task the_pages_rule_on_the_member_shows_its_message_beside_the_templates_field()
    {
        await Open("/Partials/Desk");

        await Button("Save the desk").ClickAsync();

        await Expect(KinField.GetByText("Name the next of kin.")).ToBeVisibleAsync();
        await Expect(DeskSummary).ToBeHiddenAsync();
    }

    [Test]
    public async Task the_servers_message_for_the_member_lands_beside_the_templates_field()
    {
        await Open("/Partials/Desk");
        await Input("Next of kin").FillAsync("Zed");

        await Button("Check the next of kin on file").ClickAsync();

        await Expect(KinField.GetByText("That name is not on the resident's file.")).ToBeVisibleAsync();
        await Expect(DeskSummary).ToBeHiddenAsync();
    }

    // The page's rule reads the template's input through alis.model, which keys inputs by plan id today.
    [Test, Explicit("ADR-0003 wave 2: the model table by id")]
    public async Task the_pages_rule_on_the_member_checks_what_the_template_holds()
    {
        await Open("/Partials/Desk");
        await Input("Next of kin").FillAsync("Bartholomew Fitzgerald");

        await Button("Save the desk").ClickAsync();

        await Expect(KinField.GetByText("Keep the name to 20 letters.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");
    }

    // The page's Form() sends its own plan's inputs, and its rule reads the template's through the plan id, today.
    [Test, Explicit("ADR-0003 wave 2: the model table by id")]
    public async Task the_pages_save_sends_what_the_members_template_rendered()
    {
        await Open("/Partials/Desk");
        await Input("Visitor").FillAsync("Ada");
        await Input("Next of kin").FillAsync("Bo");

        await Button("Save the desk").ClickAsync();

        await Expect(Page.Locator("#desk-saved")).ToHaveTextAsync("visitor Ada, next of kin Bo");
    }
}
