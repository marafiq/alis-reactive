namespace Alis.Reactive.Framework.PlaywrightTests.Inputs;

/// <summary>
/// An input bound to a member of an enum, rendered by a choice that writes its markup to the field's writer
/// (I5): the member chosen is posted by its name and bound as that member; a required enum refuses the member
/// its default binds as, an enum with no member numbered 0 takes any of its members, and one that may hold
/// none refuses only none; a rule under a condition on the member applies while the input holds that member,
/// or one of a set of them; picking a member re-checks the input by the choice's own event; and a pipeline
/// compares the member a choice holds and sets a member into it.
/// </summary>
[TestFixture]
public sealed class WhenAnInputHoldsAnEnumMember : AppPageTest
{
    private readonly List<string> _saves = new();

    [SetUp]
    public void WatchTheCareEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Inputs/Care", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Bound => Page.Locator("#bound");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private ILocator Field(string label) => Page.Locator("#care-form > div").Filter(new() { Has = Input(label) });

    [Test]
    public async Task a_chosen_member_is_posted_by_its_name_and_bound_as_that_member()
    {
        await Open("/Inputs/Care");
        await Input("Level of care").SelectOptionAsync("Memory");
        await Input("Keyworker").FillAsync("Ann");

        await Button("Save the care").ClickAsync();

        await Expect(Bound).ToHaveTextAsync("level Memory (3), previously Residential (1), on the Night shift, equipment none, keyworker Ann");
    }

    [Test]
    public async Task a_required_enum_refuses_the_member_its_default_binds_as()
    {
        await Open("/Inputs/Care");
        await Input("Level of care").SelectOptionAsync("Independent");

        await Button("Save the care").ClickAsync();
        await Expect(Field("Level of care").GetByText("Choose a level of care.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Input("Level of care").SelectOptionAsync("Residential");
        await Button("Save the care").ClickAsync();
        await Expect(Bound).ToHaveTextAsync("level Residential (1), previously Residential (1), on the Night shift, equipment none, keyworker none");
    }

    [Test]
    public async Task a_required_enum_with_no_member_numbered_zero_takes_its_first_member()
    {
        await Open("/Inputs/Care");
        await Input("Shift").SelectOptionAsync("Day");

        await Button("Save the care").ClickAsync();

        await Expect(Bound).ToHaveTextAsync("level Residential (1), previously Residential (1), on the Day shift, equipment none, keyworker none");
    }

    [Test]
    public async Task a_required_enum_that_may_hold_none_refuses_none_and_takes_the_member_numbered_zero()
    {
        await Open("/Inputs/Care");
        await Input("Previous level").SelectOptionAsync("");

        await Button("Save the care").ClickAsync();
        await Expect(Field("Previous level").GetByText("Say what the care was before.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Input("Previous level").SelectOptionAsync("Independent");
        await Button("Save the care").ClickAsync();
        await Expect(Bound).ToHaveTextAsync("level Residential (1), previously Independent (0), on the Night shift, equipment none, keyworker none");
    }

    [Test]
    public async Task a_rule_under_a_condition_on_a_member_applies_while_the_input_holds_that_member()
    {
        await Open("/Inputs/Care");
        await Input("Keyworker").FillAsync("Ann");
        await Input("Level of care").SelectOptionAsync("Nursing");

        await Button("Save the care").ClickAsync();
        await Expect(Field("Lifting equipment").GetByText("Name the lifting equipment.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Input("Level of care").SelectOptionAsync("Memory");
        await Button("Save the care").ClickAsync();
        await Expect(Bound).ToHaveTextAsync("level Memory (3), previously Residential (1), on the Night shift, equipment none, keyworker Ann");
    }

    [Test]
    public async Task a_rule_under_a_set_of_members_applies_while_the_input_holds_one_of_them()
    {
        await Open("/Inputs/Care");
        await Input("Level of care").SelectOptionAsync("Memory");

        await Button("Save the care").ClickAsync();
        await Expect(Field("Keyworker").GetByText("Name the keyworker.")).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Input("Level of care").SelectOptionAsync("Residential");
        await Button("Save the care").ClickAsync();
        await Expect(Bound).ToHaveTextAsync("level Residential (1), previously Residential (1), on the Night shift, equipment none, keyworker none");
    }

    [Test]
    public async Task picking_a_member_rechecks_the_input_by_the_choices_own_event()
    {
        await Open("/Inputs/Care");

        await Input("Level of care").SelectOptionAsync("Independent");

        await Expect(Field("Level of care").GetByText("Choose a level of care.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_pipeline_compares_the_member_a_choice_holds_with_a_member()
    {
        await Open("/Inputs/Care");

        await Input("Level of care").SelectOptionAsync("Memory");
        await Button("Assess").ClickAsync();
        await Expect(Page.Locator("#assessed")).ToHaveTextAsync("memory care");

        await Input("Level of care").SelectOptionAsync("Nursing");
        await Button("Assess").ClickAsync();
        await Expect(Page.Locator("#assessed")).ToHaveTextAsync("not memory care");
    }

    [Test]
    public async Task a_member_set_into_a_choice_is_the_option_it_shows()
    {
        await Open("/Inputs/Care");

        await Button("Move to nursing care").ClickAsync();

        await Expect(Input("Level of care")).ToHaveValueAsync("Nursing");
    }
}
