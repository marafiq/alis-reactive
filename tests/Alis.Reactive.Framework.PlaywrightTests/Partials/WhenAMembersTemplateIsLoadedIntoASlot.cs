namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// The page's validator names a member whose editor template is loaded into a slot in a request of its own, the
/// action rendering it for the member as EditorFor does (ViewData.TemplateInfo.HtmlFieldPrefix), so the template is
/// a view of the member's own model with a plan of its own (ADR-0003 rules 1 and 3). The page's rule on the member
/// checks the template's input, and the page's save sends it.
/// </summary>
[TestFixture]
public sealed class WhenAMembersTemplateIsLoadedIntoASlot : AppPageTest
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

    private async Task AddTheNextOfKin()
    {
        await Button("Add the next of kin").ClickAsync();
        await Expect(Input("Next of kin")).ToBeVisibleAsync();
    }

    [Test]
    public async Task the_pages_rule_on_the_member_shows_its_message_beside_the_loaded_templates_field()
    {
        await Open("/Partials/Reception");
        await AddTheNextOfKin();

        await Button("Save the visit").ClickAsync();

        await Expect(KinField.GetByText("Name the next of kin.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");
    }

    [Test]
    public async Task the_pages_save_sends_what_the_loaded_template_holds()
    {
        await Open("/Partials/Reception");
        await Input("Visitor").FillAsync("Ada");
        await AddTheNextOfKin();
        await Input("Next of kin").FillAsync("Bo");

        await Button("Save the visit").ClickAsync();

        await Expect(Page.Locator("#reception-saved")).ToHaveTextAsync("visitor Ada, next of kin Bo");
    }
}
