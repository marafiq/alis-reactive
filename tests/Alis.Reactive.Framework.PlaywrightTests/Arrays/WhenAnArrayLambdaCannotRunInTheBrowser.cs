namespace Alis.Reactive.Framework.PlaywrightTests.Arrays;

/// <summary>
/// A lambda the array operations cannot run in the browser is refused where the page renders, with a message
/// that names the mistake, instead of a plan that would compute something else: a record as an order's key, a
/// method with no pure JavaScript form, a computed projection, a text test against the item or against a
/// character, a comparison that never reads the item, and a cast to a flag.
/// </summary>
[TestFixture]
public sealed class WhenAnArrayLambdaCannotRunInTheBrowser : AppPageTest
{
    // The page as a developer meets it: the request answers with the message it was refused with.
    private async Task Render(string mistake) =>
        await Page.GotoAsync($"{BaseUrl}/Arrays/Refused?mistake={mistake}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task an_order_by_a_record_is_refused_because_a_record_has_no_order()
    {
        await Render("OrderByARecord");

        await Expect(Message("OrderBy key must project to a scalar (string, number, date, bool, enum), not 'Keyworker'.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_method_with_no_pure_javascript_form_is_refused_with_the_methods_a_lambda_may_call()
    {
        await Render("CallAMethodWithNoJavaScriptForm");

        await Expect(Message("'String.Substring' has no pure JavaScript form the plan can call on an item; the lambda may call ChildElement.GetAttribute, String.Trim, String.TrimStart, String.TrimEnd, String.ToUpper, String.ToUpperInvariant, String.ToLower, String.ToLowerInvariant.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_computed_projection_is_refused()
    {
        await Render("ComputeAProjection");

        await Expect(Message("Element selector must be a member access (x => x.Field, x => x.A.B) or the element itself (x => x).")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_text_test_against_the_item_is_refused()
    {
        await Render("TestTextAgainstTheItem");

        await Expect(Message("String operation arguments (Contains/StartsWith/EndsWith) must be constants or captured values, not element-scope reads")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_text_test_against_a_character_is_refused()
    {
        await Render("TestTextAgainstACharacter");

        await Expect(Message("String operation arguments must be text. Got: H")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_comparison_that_never_reads_the_item_is_refused()
    {
        await Render("CompareWithoutTheItem");

        await Expect(Message("Array predicate comparison must reference the element on at least one side")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_predicate_that_is_no_test_of_the_item_is_refused_with_the_tests_a_predicate_may_make()
    {
        await Render("CastToAFlag");

        await Expect(Message("Array predicate supports comparisons (== != > >= < <=), logical (&& || !), string Contains/StartsWith/EndsWith, and boolean members.")).ToBeVisibleAsync();
    }
}
