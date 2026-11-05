namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenAConditionWaits : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Transfer", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Url);
        };
    }

    private ILocator Field(string label)
        => Page.Locator("#transfer-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    [Test]
    public async Task a_rule_under_a_condition_that_waits_is_sent_and_answered_by_the_server()
    {
        await Open("/Validation/Transfer");
        await Write("Ward", "East");
        await Save();

        await Expect(Field("Approved by").GetByText("A full ward takes a transfer with an approver's name.")).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Has.Count.EqualTo(1), "the browser holds no rule under a condition that waits");
    }

    [Test]
    public async Task the_rule_holds_once_the_approver_is_named()
    {
        await Open("/Validation/Transfer");
        await Write("Ward", "East");
        await Write("Approved by", "Dr Bell");
        await Save();

        await Expect(Saved).ToHaveTextAsync("Ada moved to East");
    }

    [Test]
    public async Task a_ward_with_room_takes_the_transfer_without_an_approver()
    {
        await Open("/Validation/Transfer");
        await Save();

        await Expect(Saved).ToHaveTextAsync("Ada moved to West");
    }

    [Test]
    public async Task a_rule_unless_a_condition_that_waits_is_answered_by_the_server_only_while_the_condition_fails()
    {
        await Open("/Validation/Transfer");
        var message = Field("Escorted by").GetByText("A ward with room takes a transfer with an escort's name.");
        await Write("Escorted by", "");

        await Save();
        await Expect(message).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Has.Count.EqualTo(1), "the browser holds no rule under a condition that waits");

        await Write("Ward", "East");
        await Write("Approved by", "Dr Bell");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Ada moved to East");
        await Expect(message).ToBeHiddenAsync();
    }

    [Test]
    public async Task asked_without_waiting_the_validator_refuses_and_names_the_condition()
    {
        await Open("/Validation/Transfer");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save without waiting", Exact = true }).ClickAsync();

        await Expect(Saved).ToHaveTextAsync("TransferRules has a rule that waits (MustAsync, CustomAsync or WhenAsync); validate with ValidateAsync.");
    }
}
