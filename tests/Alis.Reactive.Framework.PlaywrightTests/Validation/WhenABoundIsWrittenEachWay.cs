namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// A comparison written every way the validator takes it (against a value or another member, the checked
/// member and the other one each nullable or not) and a nullable range: each refuses at its bound in the
/// browser with nothing sent, or takes its bound, as its kind says, and the endpoint saves what it takes.
/// </summary>
[TestFixture]
public sealed class WhenABoundIsWrittenEachWay : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Bounds", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label) =>
        Page.Locator("#bounds-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [TestCase("MaybeAtLeastTwo", "1", "2", TestName = "a_nullable_at_least_a_value_takes_the_value")]
    [TestCase("MaybeBelowFive", "5", "4", TestName = "a_nullable_less_than_a_value_refuses_the_value")]
    [TestCase("MaybeAtMostFive", "6", "5", TestName = "a_nullable_at_most_a_value_takes_the_value")]
    [TestCase("AtLeastBase", "4", "5", TestName = "at_least_a_member_takes_the_members_value")]
    [TestCase("BelowBase", "5", "4", TestName = "less_than_a_member_refuses_the_members_value")]
    [TestCase("AtMostBase", "6", "5", TestName = "at_most_a_member_takes_the_members_value")]
    [TestCase("MaybeAboveBase", "5", "6", TestName = "a_nullable_greater_than_a_member_refuses_the_members_value")]
    [TestCase("MaybeAtLeastBase", "4", "5", TestName = "a_nullable_at_least_a_member_takes_the_members_value")]
    [TestCase("MaybeBelowBase", "5", "4", TestName = "a_nullable_less_than_a_member_refuses_the_members_value")]
    [TestCase("MaybeAtMostBase", "6", "5", TestName = "a_nullable_at_most_a_member_takes_the_members_value")]
    [TestCase("MaybeAboveMaybeBase", "5", "6", TestName = "a_nullable_greater_than_a_nullable_member_refuses_its_value")]
    [TestCase("MaybeBelowMaybeBase", "5", "4", TestName = "a_nullable_less_than_a_nullable_member_refuses_its_value")]
    [TestCase("MaybeAtMostMaybeBase", "6", "5", TestName = "a_nullable_at_most_a_nullable_member_takes_its_value")]
    [TestCase("AboveMaybeBase", "5", "6", TestName = "greater_than_a_nullable_member_refuses_its_value")]
    [TestCase("AtLeastMaybeBase", "4", "5", TestName = "at_least_a_nullable_member_takes_its_value")]
    [TestCase("BelowMaybeBase", "5", "4", TestName = "less_than_a_nullable_member_refuses_its_value")]
    [TestCase("AtMostMaybeBase", "6", "5", TestName = "at_most_a_nullable_member_takes_its_value")]
    [TestCase("MaybeOneToFive", "6", "5", TestName = "a_nullable_inclusive_range_takes_its_bound")]
    [TestCase("MaybeInsideTen", "10", "9", TestName = "a_nullable_exclusive_range_refuses_its_bound")]
    public async Task a_bound_refuses_and_takes_as_its_comparison_says(string field, string refused, string taken)
    {
        await Open("/Validation/Bounds");
        var message = Field(field).GetByText($"Out of bounds: {field}.", new() { Exact = true });

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
}
