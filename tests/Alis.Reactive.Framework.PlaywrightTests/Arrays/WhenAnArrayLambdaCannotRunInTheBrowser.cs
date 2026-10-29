namespace Alis.Reactive.Framework.PlaywrightTests.Arrays;

/// <summary>
/// A lambda the array operations cannot run in the browser is refused where the page renders, with a message
/// that names the mistake, instead of a plan that would compute something else: a record as an order's key, a
/// method with no pure JavaScript form, a computed projection, a text test against the item or against a
/// character, a comparison that never reads the item, a cast to a flag, a member of a value the member table of
/// grammar.md 2.3 does not name, a day of the week read as a value, a cast that drops a fraction, a cast to a
/// narrower number, and a length of time ordered by a comparison or as an order's key: it reaches the browser as
/// its text, which does not order as the length does.
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

        await Expect(Message("'String.Substring' has no pure JavaScript form the plan can call on an item; the lambda may call ChildElement.GetAttribute, ChildElement.HasAttribute, String.Trim, String.TrimStart, String.TrimEnd, String.ToUpper, String.ToUpperInvariant, String.ToLower, String.ToLowerInvariant, and test with String.Contains, String.StartsWith, String.EndsWith, String.IsNullOrEmpty, Enumerable.Contains and Regex.IsMatch.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_computed_projection_is_refused()
    {
        await Render("ComputeAProjection");

        await Expect(Message("An array lambda reads the item (x => x), its members (x => x.Field, x => x.A.B), and the members, methods and operators of the member table (grammar.md 2.3); a computed object and arithmetic have no form the plan writes.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_text_test_against_the_item_is_refused()
    {
        await Render("TestTextAgainstTheItem");

        await Expect(Message("A text test's text (Contains, StartsWith, EndsWith, Regex.IsMatch's pattern) must be a constant or a captured value, not a read of the item: the plan writes it as a literal.")).ToBeVisibleAsync();
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

        await Expect(Message("An array predicate is a test of the item: a comparison (== != > >= < <=), && || !, a text test (Contains, StartsWith, EndsWith, string.IsNullOrEmpty, Regex.IsMatch), Contains over fixed values, or a boolean value. Got a Boolean?: x.Discharged")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_member_of_a_value_the_table_does_not_name_is_refused_with_the_members_it_names()
    {
        await Render("ReadAMemberWithNoJavaScriptForm");

        await Expect(Message("'DateTime.DayOfYear' is a member of a value .NET defines that has no JavaScript form; a lambda may read DateTime.Year, DateTime.Month, DateTime.Day, DateTime.Hour, DateTime.Minute, DateTime.Second, String.Length, DateTime.DayOfWeek (compared with a DayOfWeek), Nullable.Value and Nullable.HasValue. Project the value on the server.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_day_of_the_week_read_as_a_value_is_refused_because_the_browser_reads_its_number()
    {
        await Render("ReadADayOfTheWeekAsAValue");

        await Expect(Message("'DateTime.DayOfWeek' is read in the browser as getDay(), the day's number, so a lambda reads it only to compare it with a DayOfWeek (x => x.Admitted.DayOfWeek == DayOfWeek.Saturday). Project it on the server.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_cast_that_drops_a_fraction_is_refused_because_a_browsers_number_keeps_it()
    {
        await Render("CastToAWholeNumber");

        await Expect(Message("A cast from Double to Int32 changes the value as C# computes it (a whole number drops the fraction, a narrower number wraps), and a browser's number does not. Project the value on the server.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_cast_to_a_narrower_number_is_refused_because_a_browsers_number_does_not_wrap()
    {
        await Render("CastToANarrowerNumber");

        await Expect(Message("A cast from Int32 to Byte changes the value as C# computes it (a whole number drops the fraction, a narrower number wraps), and a browser's number does not. Project the value on the server.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_order_comparison_of_a_length_of_time_is_refused_because_its_text_does_not_order_as_its_length()
    {
        await Render("CompareALengthOfTime");

        await Expect(Message("A TimeSpan has no order the browser can compute as C# does: it reaches the browser as its text, and \"1.02:00:00\" sorts before \"23:00:00\". Order it on the server, or read the length as a number.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_order_by_a_length_of_time_that_may_be_missing_is_refused_because_its_text_does_not_order_as_its_length()
    {
        await Render("OrderByALengthOfTime");

        await Expect(Message("A TimeSpan has no order the browser can compute as C# does: it reaches the browser as its text, and \"1.02:00:00\" sorts before \"23:00:00\". Order it on the server, or read the length as a number.")).ToBeVisibleAsync();
    }
}
