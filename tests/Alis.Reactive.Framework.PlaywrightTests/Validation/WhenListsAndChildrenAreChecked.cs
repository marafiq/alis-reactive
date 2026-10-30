namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// Rules on a list's items, on a child the form holds, and rules included whole: each check on an item is
/// made, and the server's answer for an item, for a child's member or for an included rule lands beside the
/// field it is about, even where the check or the condition around it is the server's alone.
/// </summary>
[TestFixture]
public sealed class WhenListsAndChildrenAreChecked : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    private ILocator Summary => Page.Locator("[data-reactive-validation-summary]");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Household", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label) =>
        Page.Locator("#household-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    private async Task ExpectNothingSent()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");
    }

    private void ExpectOneSent(string because)
    {
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), because);
    }

    [TestCase("", "Give each tag.", TestName = "the_first_check_on_each_item_is_made_in_the_browser")]
    [TestCase("eastern", "Five letters a tag.", TestName = "the_second_check_on_each_item_is_made_in_the_browser")]
    public async Task each_check_on_an_items_rule_is_made(string refused, string message)
    {
        await Open("/Validation/Household");

        await Write("Second tag", refused);
        await Save();
        await Expect(Field("Second tag").GetByText(message)).ToBeVisibleAsync();
        await ExpectNothingSent();

        await Write("Second tag", "west");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task an_items_check_only_the_server_makes_comes_back_beside_the_item()
    {
        await Open("/Validation/Household");

        await Write("Second room", "W-2");
        await Save();

        await Expect(Field("Second room").GetByText("East wing rooms only.")).ToBeVisibleAsync();
        await Expect(Field("First room").GetByText("East wing rooms only.")).ToBeHiddenAsync();
        await Expect(Summary).ToBeHiddenAsync();
        ExpectOneSent("the browser does not make the room's check, so the request goes");
    }

    [Test]
    public async Task a_child_under_a_condition_only_the_server_decides_answers_beside_its_field()
    {
        await Open("/Validation/Household");

        await Write("Phone to call", "");
        await Save();

        await Expect(Field("Phone to call").GetByText("Give a phone to call.")).ToBeVisibleAsync();
        await Expect(Summary).ToBeHiddenAsync();
        ExpectOneSent("the condition around the caller's rules is the server's, so the request goes");
    }

    [Test]
    public async Task a_childs_list_under_a_condition_only_the_server_decides_is_checked_by_the_server_alone()
    {
        await Open("/Validation/Household");

        await Write("Extension", "");
        await Save();

        await Expect(Field("Extension").GetByText("Give each extension.")).ToBeVisibleAsync();
        await Expect(Summary).ToBeHiddenAsync();
        ExpectOneSent("the condition around the caller's rules is the server's, so even its list's check goes to the server");
    }

    [Test]
    public async Task a_lists_items_under_a_condition_only_the_server_decides_answer_beside_their_field()
    {
        await Open("/Validation/Household");

        await Write("First visitor", "");
        await Save();

        await Expect(Field("First visitor").GetByText("Name each visitor.")).ToBeVisibleAsync();
        await Expect(Summary).ToBeHiddenAsync();
        ExpectOneSent("the condition around each visitor's rules is the server's, so the request goes");
    }

    [Test]
    public async Task rules_included_under_a_condition_only_the_server_decides_answer_beside_their_field()
    {
        await Open("/Validation/Household");

        await Write("Note", "Far too long a note");
        await Save();

        await Expect(Field("Note").GetByText("Ten characters a note.")).ToBeVisibleAsync();
        await Expect(Summary).ToBeHiddenAsync();
        ExpectOneSent("the condition around the included rules is the server's, so the request goes");
    }
}
