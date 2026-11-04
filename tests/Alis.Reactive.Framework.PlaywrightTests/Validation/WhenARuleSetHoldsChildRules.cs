namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenARuleSetHoldsChildRules : AppPageTest
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
    public async Task inline_rules_written_in_no_set_take_the_set_of_the_rule_that_holds_them_in_the_browser_and_on_the_server()
    {
        await Open("/Validation/MoveIn?stage=Arrive");
        var phone = Field("Pharmacy phone");

        await Write("Pharmacy phone", "");
        await Save();
        await Expect(phone.GetByText("Give the pharmacy's phone.", new() { Exact = true })).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Pharmacy phone", "556-0101");
        await Save();
        await Expect(phone.GetByText("Use a 555 number.", new() { Exact = true })).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make the C# check, so the request goes");

        await Write("Pharmacy phone", "555-0102");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Arrive saved for Ada");

        await Open("/Validation/MoveIn");

        await Write("Pharmacy phone", "");
        await Save();

        await Expect(Saved).ToHaveTextAsync("saved for Ada");
        await Expect(Field("Pharmacy phone").GetByText("Give the pharmacy's phone.")).ToBeHiddenAsync();
    }
}
