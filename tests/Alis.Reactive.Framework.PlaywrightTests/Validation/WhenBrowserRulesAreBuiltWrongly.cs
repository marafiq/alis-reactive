namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// Browser rules a validation integration builds by hand (<c>BrowserRuleSet</c>, <c>BrowserRule</c>) wrongly,
/// refused where the page that describes them renders, with the message the developer reads: a rule named for
/// a call on a member rather than a member, the items of a member that holds no list, and a range whose bounds
/// are two kinds of value.
/// </summary>
[TestFixture]
public sealed class WhenBrowserRulesAreBuiltWrongly : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
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
        await Render("DeclareItemsOfAMemberThatHoldsNone");

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
