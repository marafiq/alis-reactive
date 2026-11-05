namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenARuleIsGivenAName : AppPageTest
{
    private readonly List<string> _saves = new();

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Nameplate", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [TestCase("Empty")]
    [TestCase("Spaces")]
    [TestCase("Null")]
    public async Task a_name_without_text_is_refused_where_the_page_reads_the_rules(string name)
    {
        await Page.GotoAsync($"{BaseUrl}/Validation/Nameplate?name={name}");

        await Expect(Page.GetByText("A name people read has text.").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_name_without_text_is_refused_where_an_endpoint_validates_with_the_rules()
    {
        await Open("/Validation/Nameplate");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save under a blank name", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#saved")).ToHaveTextAsync("A name people read has text. (Parameter 'displayName')");
    }

    [Test]
    public async Task a_name_with_text_is_said_in_the_rules_messages()
    {
        await Open("/Validation/Nameplate");

        await Page.GetByLabel("Resident", new() { Exact = true }).FillAsync("");
        await Save();
        await Expect(Page.Locator("#nameplate-form").GetByText("'Resident's name' must not be empty.", new() { Exact = true })).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Page.GetByLabel("Resident", new() { Exact = true }).FillAsync("Bea");
        await Save();
        await Expect(Page.Locator("#saved")).ToHaveTextAsync("saved Bea");
    }
}
