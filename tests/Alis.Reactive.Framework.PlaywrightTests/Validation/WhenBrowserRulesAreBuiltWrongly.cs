namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenBrowserRulesAreBuiltWrongly : AppPageTest
{
    private async Task Render(string mistake) => await Page.GotoAsync($"{BaseUrl}/Validation/RuleSetRefused?mistake={mistake}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_rule_named_for_a_call_on_a_member_is_refused()
    {
        await Render("NameARuleForACallOnAMember");

        await Expect(Message("A browser rule names a member of the model, like x => x.Address.City; x => x.Visitor.Trim() does not.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task the_items_of_a_member_that_holds_no_list_are_refused()
    {
        await Render("RuleTheItemsOfAMemberThatHoldsNone");

        await Expect(Message("'Guests' is not a collection.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_range_bounded_by_two_kinds_of_value_is_refused()
    {
        await Render("BoundARangeByTwoKindsOfValue");

        await Expect(Message(
            "Client validation range bounds must have the same shape. Lower bound is 'number', upper bound is 'date'.")).ToBeVisibleAsync();
    }
}
