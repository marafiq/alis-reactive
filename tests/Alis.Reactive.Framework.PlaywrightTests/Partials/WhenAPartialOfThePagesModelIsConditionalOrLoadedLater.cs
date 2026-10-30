namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// The page's validator names a member that a partial of the page's own model renders, and the partial is on the
/// page only sometimes: rendered with the page when a condition holds, or loaded into a slot later and unloaded
/// again (ADR-0003). The page's rule is the only one: it checks the partial's input while it is mounted and
/// reads the member as missing while it is not, as the server does, and the page's save sends the input.
/// </summary>
[TestFixture]
public sealed class WhenAPartialOfThePagesModelIsConditionalOrLoadedLater : AppPageTest
{
    private readonly List<string> _admissions = new();

    [SetUp]
    public void WatchTheAdmissionsEndpoint()
    {
        _admissions.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Partials/Admit", StringComparison.Ordinal))
                lock (_admissions) _admissions.Add(request.Method);
        };
    }

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private ILocator RoomField => Page.Locator("#room-line > div").Filter(new() { Has = Input("Room") });

    private ILocator AdmissionSummary => Page.Locator("#admission-messages [data-reactive-validation-summary]");

    private async Task ChooseARoom()
    {
        await Button("Choose a room").ClickAsync();
        await Expect(Input("Room")).ToBeVisibleAsync();
    }

    [Test]
    public async Task while_no_partial_renders_the_member_the_pages_rule_holds_the_save_in_the_summary()
    {
        await Open("/Partials/Admission");
        await Input("Resident").FillAsync("Ann");

        await Button("Admit").ClickAsync();

        await Expect(AdmissionSummary).ToContainTextAsync("Choose the resident's room.");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_admissions) Assert.That(_admissions, Is.Empty, "the browser held the request");
    }

    [Test]
    public async Task when_the_page_renders_the_partial_with_itself_the_pages_save_sends_its_input()
    {
        await Open("/Partials/Admission?showsRoom=true");
        await Input("Resident").FillAsync("Ann");
        await Input("Room").FillAsync("12");

        await Button("Admit").ClickAsync();

        await Expect(Page.Locator("#admitted")).ToHaveTextAsync("Ann admitted to room 12");
    }

    [Test]
    public async Task a_partial_loaded_later_and_left_empty_shows_the_pages_message_beside_its_field()
    {
        await Open("/Partials/Admission");
        await ChooseARoom();

        await Button("Admit").ClickAsync();

        await Expect(RoomField.GetByText("Choose the resident's room.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_admissions) Assert.That(_admissions, Is.Empty, "the browser held the request");
    }

    [Test]
    public async Task a_partial_loaded_later_and_filled_is_sent_by_the_pages_save()
    {
        await Open("/Partials/Admission");
        await Input("Resident").FillAsync("Ann");
        await ChooseARoom();
        await Input("Room").FillAsync("12");

        await Button("Admit").ClickAsync();

        await Expect(Page.Locator("#admitted")).ToHaveTextAsync("Ann admitted to room 12");
    }

    [Test]
    public async Task once_the_partial_is_unloaded_the_pages_rule_holds_the_save_again()
    {
        await Open("/Partials/Admission");
        await Input("Resident").FillAsync("Ann");
        await ChooseARoom();
        await Input("Room").FillAsync("12");
        await Button("Clear the room").ClickAsync();
        await Expect(Input("Room")).ToHaveCountAsync(0);

        await Button("Admit").ClickAsync();

        await Expect(AdmissionSummary).ToContainTextAsync("Choose the resident's room.");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_admissions) Assert.That(_admissions, Is.Empty, "the browser held the request");
    }

    [Test]
    public async Task the_servers_message_for_the_member_lands_beside_the_loaded_field()
    {
        await Open("/Partials/Admission");
        await ChooseARoom();
        await Input("Room").FillAsync("13");

        await Button("Admit").ClickAsync();

        await Expect(RoomField.GetByText("Room 13 is closed for repairs.")).ToBeVisibleAsync();
        await Expect(AdmissionSummary).ToBeHiddenAsync();
    }
}
