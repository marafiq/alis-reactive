namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenARuleIsWrittenUnderATextsLength : AppPageTest
{
    private readonly List<string> _saves = new();

    private ILocator Saved => Page.Locator("#saved");

    [SetUp]
    public void WatchTheEndpoint()
    {
        _saves.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/CareNote", StringComparison.Ordinal))
                lock (_saves) _saves.Add(request.Method);
        };
    }

    private ILocator Field(string label)
        => Page.Locator("#note-form > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private Task Write(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [TestCase("Reviewer", "Wandered at the gate.", "Wandered at the gate", "A long observation needs a reviewer.", TestName = "a_length_above_a_value_holds_one_letter_past_it")]
    [TestCase("Initials", "Ate.", "Slept", "A short observation needs your initials.", TestName = "a_length_below_a_value_holds_one_letter_short_of_it")]
    [TestCase("Witness", "Walked the east hall once before supper.", "Walked the east hall once before supper", "An observation of forty letters or more needs a witness.", TestName = "a_value_on_the_left_at_most_the_length_holds_at_the_value")]
    public async Task a_rule_applies_only_while_the_length_of_a_text_holds(string field, string holds, string other, string message)
    {
        await Open("/Validation/CareNote");
        await Write(field, "");

        await Write("Observation", holds);
        await Save();
        await Expect(Field(field).GetByText(message)).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_saves) Assert.That(_saves, Is.Empty, "the browser held the request");

        await Write("Observation", other);
        await Save();
        await Expect(Saved).ToHaveTextAsync("saved");
        await Expect(Field(field).GetByText(message)).ToBeHiddenAsync();
    }

    [Test]
    public async Task a_text_left_blank_has_no_length_so_no_comparison_with_it_holds()
    {
        await Open("/Validation/CareNote");
        await Write("Initials", "");

        await Save();

        await Expect(Field("Observation").GetByText("Write what you observed.")).ToBeVisibleAsync();
        await Expect(Field("Initials").GetByText("A short observation needs your initials.")).ToBeHiddenAsync();
    }
}
