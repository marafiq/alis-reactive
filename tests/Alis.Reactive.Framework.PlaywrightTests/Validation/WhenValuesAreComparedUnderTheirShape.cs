namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// A comparison reads its value under the member's shape, the same in the browser as on the server: a
/// number, a flag, a date, an instant and text compared for equality and for order, and a number compared with
/// another member. Each refuses in the browser with nothing sent and takes what the server takes. Text the
/// server orders by its culture, which the browser cannot match, so an order of text is the server's alone.
/// </summary>
[TestFixture]
public sealed class WhenValuesAreComparedUnderTheirShape : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Sameness", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label) =>
        Page.Locator("#sameness-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [TestCase("Visitors", "4", "5", TestName = "a_number_equal_to_a_value_takes_only_the_value")]
    [TestCase("Beds", "0", "1", TestName = "a_number_not_equal_to_a_value_refuses_only_the_value")]
    [TestCase("Escorted", "false", "true", TestName = "a_flag_equal_to_true_takes_only_true")]
    [TestCase("Arrives", "2026-10-02", "2026-10-01", TestName = "a_date_equal_to_a_day_takes_only_that_day")]
    [TestCase("Called", "2026-10-01T10:00:00Z", "2026-10-01T09:00:00Z", TestName = "an_instant_equal_to_a_moment_takes_only_that_moment")]
    [TestCase("Consented", "false", "true", TestName = "a_flag_greater_than_false_takes_only_true")]
    [TestCase("Wards", "4", "3", TestName = "a_number_equal_to_another_member_takes_only_its_value")]
    public async Task a_comparison_refuses_and_takes_as_the_server_does(string field, string refused, string taken)
    {
        await Open("/Validation/Sameness");
        var message = Field(field).GetByText($"Not as required: {field}.", new() { Exact = true });

        await Page.GetByLabel(field, new() { Exact = true }).FillAsync(refused);
        await Save();
        await Expect(message).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Page.GetByLabel(field, new() { Exact = true }).FillAsync(taken);
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(message).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_range_of_text_is_the_servers_and_its_answer_lands_beside_the_field()
    {
        await Open("/Validation/Sameness");
        var message = Field("Initial").GetByText("Not as required: Initial.", new() { Exact = true });

        await Page.GetByLabel("Initial", new() { Exact = true }).FillAsync("Z");
        await Save();
        await Expect(message).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not order text, so the request goes");

        await Page.GetByLabel("Initial", new() { Exact = true }).FillAsync("C");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task an_order_of_text_is_the_servers_and_its_answer_lands_beside_the_field()
    {
        await Open("/Validation/Sameness");
        var message = Field("Surname").GetByText("Not as required: Surname.", new() { Exact = true });

        await Page.GetByLabel("Surname", new() { Exact = true }).FillAsync("Adams");
        await Save();
        await Expect(message).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not order text, so the request goes");

        await Page.GetByLabel("Surname", new() { Exact = true }).FillAsync("Zane");
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
    }
}
