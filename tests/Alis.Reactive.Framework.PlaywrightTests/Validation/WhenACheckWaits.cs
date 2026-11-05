namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenACheckWaits : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/RoomMove", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label)
        => Page.Locator("#move-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [Test]
    public async Task a_check_that_waits_is_the_servers_and_its_failure_comes_back_beside_the_member_with_its_name_and_value()
    {
        await Open("/Validation/RoomMove");

        await Write("Room", "E-12");
        await Save();
        await Expect(Field("Room").GetByText("'Room' E-12 is taken.", new() { Exact = true })).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make a check that waits, so the request goes");

        await Write("Room", "E-14");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Ada moved to E-14");
    }

    [Test]
    public async Task a_check_that_waits_puts_a_failure_under_another_member_beside_that_members_input()
    {
        await Open("/Validation/RoomMove");

        await Write("Room", "E-13");
        await Save();
        await Expect(Field("Bed").GetByText("Room E-13 is shared; name the bed.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Field("Room").GetByText("Room E-13 is shared; name the bed.")).ToBeHiddenAsync();

        await Write("Bed", "Window");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Ada moved to E-13");
    }

    [Test]
    public async Task a_rule_with_a_check_that_waits_still_makes_its_other_checks_in_the_browser()
    {
        await Open("/Validation/RoomMove");

        await Write("Room", "");
        await Save();
        await Expect(Field("Room").GetByText("Name the room.", new() { Exact = true })).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Room", "E-14");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Ada moved to E-14");
    }

    [Test]
    public async Task a_check_that_waits_is_handed_the_cancellation_its_validation_was_given()
    {
        await Open("/Validation/RoomMove");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Call off the room book", Exact = true }).ClickAsync();
        await Expect(Saved).ToHaveTextAsync("The room book was called off.");

        await Save();
        await Expect(Saved).ToHaveTextAsync("Ada moved to E-14");
    }

    [Test]
    public async Task a_check_that_waits_in_a_child_validator_answers_beside_the_childs_member()
    {
        await Open("/Validation/RoomMove");

        await Write("Escort's badge", "V-0");
        await Save();
        await Expect(Field("Escort's badge").GetByText("Badge V-0 is revoked.", new() { Exact = true })).ToBeVisibleAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make a check that waits, so the request goes");

        await Write("Escort's badge", "V-2");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Ada moved to E-14");
    }

    [Test]
    public async Task a_check_that_waits_on_each_item_of_a_list_answers_beside_the_item_it_refuses()
    {
        await Open("/Validation/RoomMove");

        await Write("Second box", "Box-9");
        await Save();
        await Expect(Field("Second box").GetByText("The store has no room for Box-9.", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Field("First box").GetByText("The store has no room for")).ToBeHiddenAsync();
        lock (_saves) Assert.That(_saves, Is.EqualTo(new[] { "POST" }), "the browser does not make a check that waits, so the request goes");

        await Write("Second box", "Box-3");
        await Save();
        await Expect(Saved).ToHaveTextAsync("Ada moved to E-14");
    }

    [Test]
    public async Task asked_without_waiting_a_validator_whose_child_validator_waits_refuses_and_names_the_checks_that_wait()
    {
        await Open("/Validation/RoomMove");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save the escort without waiting", Exact = true }).ClickAsync();

        await Expect(Saved).ToHaveTextAsync("EscortedRoomMoveRules has a rule that waits (MustAsync, CustomAsync or WhenAsync); validate with ValidateAsync.");
    }

    [TestCase("AfterMustAsync")]
    [TestCase("AfterCustomAsync")]
    [TestCase("UnderWhenAsync")]
    public async Task dependent_rules_after_a_rule_that_waits_are_refused_where_the_page_reads_the_rules(string dependents)
    {
        await Page.GotoAsync($"{BaseUrl}/Validation/RoomMove?dependents={dependents}");

        await Expect(Page.GetByText(
            "DependentRules follows a rule on 'Room' that waits (MustAsync, CustomAsync or WhenAsync), and a condition does not wait: " +
            "write the dependent rules under WhenOnServer with the same test.").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task asked_without_waiting_the_validator_refuses_and_names_the_checks_that_wait()
    {
        await Open("/Validation/RoomMove");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save without waiting", Exact = true }).ClickAsync();

        await Expect(Saved).ToHaveTextAsync("RoomMoveRules has a rule that waits (MustAsync, CustomAsync or WhenAsync); validate with ValidateAsync.");
    }
}
