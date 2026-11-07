namespace Alis.Reactive.Poc.PlaywrightTests.Patterns;

[TestFixture]
public sealed class WhenADosePatternIsRefused : AppPageTest
{
    [Test]
    public async Task a_pattern_with_digits_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternDigits");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "^\d")' holds '\d', which .NET and the browser read differently; write [0-9]. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_non_digits_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternNonDigits");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "^\D")' holds '\D', which .NET and the browser read differently; write [^0-9]. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_word_characters_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternWordCharacters");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "^\w")' holds '\w', which .NET and the browser read differently; write [a-zA-Z0-9_]. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_non_word_characters_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternNonWordCharacters");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "^\W")' holds '\W', which .NET and the browser read differently; write [^a-zA-Z0-9_]. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_white_space_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternWhiteSpace");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "\s")' holds '\s', which .NET and the browser read differently; write [ \t\n\r\f\v]. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_non_white_space_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternNonWhiteSpace");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "^\S")' holds '\S', which .NET and the browser read differently; write [^ \t\n\r\f\v]. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_start_of_text_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternStartOfText");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "\AA")' holds '\A', which .NET and the browser read differently; write ^. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_end_of_text_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternEndOfText");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "n\Z")' holds '\Z', which .NET and the browser read differently; write $. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_any_character_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternAnyCharacter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "^A.p")' holds '.', which .NET and the browser read differently; write [^\n]. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_digits_in_a_class_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternDigitsInAClass");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "[\d]")' holds '\d' inside a class, which .NET and the browser read differently; write 0-9. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_word_characters_in_a_class_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternWordCharactersInAClass");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "[\w]")' holds '\w' inside a class, which .NET and the browser read differently; write a-zA-Z0-9_. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_white_space_in_a_class_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternWhiteSpaceInAClass");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "[\s]")' holds '\s' inside a class, which .NET and the browser read differently; write  \t\n\r\f\v. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_word_boundary_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternWordBoundary");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "\bAsp")' holds '\b', which the browser does not read as .NET does; move the work to the server. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_named_group_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternNamedGroup");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "(?<initial>A)")' holds '(?<', which the browser does not read as .NET does; move the work to the server. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_atomic_group_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternAtomicGroup");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "(?>A)")' holds '(?>', which the browser does not read as .NET does; move the work to the server. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_needless_escape_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternNeedlessEscape");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "\-A")' holds '\-', which needs no escape; write -. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_short_hex_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternShortHex");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "\u12")' holds '\u' without 4 hex digits after it. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_trailing_escape_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternTrailingEscape");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "A\")' ends in '\', which escapes nothing; escape it as \\. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_bracket_first_in_a_class_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternBracketFirstInAClass");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "[]A]")' holds ']' first in a class, which the browser reads as the end of an empty class; escape it as \]. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_bracket_inside_a_class_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternBracketInsideAClass");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "[a[b]")' holds '[' inside a class, which .NET reads as a subtraction after '-'; escape it as \[. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_backward_range_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternBackwardRange");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "[z-a]")' holds a range from 'z' to 'a', which runs backwards. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_dash_after_a_range_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternDashAfterARange");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "[a-c-e]")' holds '-' right after a range; escape it as \-. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_unclosed_class_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternUnclosedClass");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "[A")' opens a class it does not close; close it with ']'. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_unclosed_group_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternUnclosedGroup");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "(A")' opens a group it does not close; close it with ')'. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_group_closing_nothing_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternGroupClosingNothing");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "A)")' holds ')' that closes no group; escape it as \). (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_bracket_outside_a_class_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternBracketOutsideAClass");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "A]")' holds ']' outside the construct it closes; escape it as \]. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_repeat_of_nothing_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternRepeatOfNothing");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "*A")' holds '*' where nothing it can repeat comes before it; escape it as \*. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_brace_that_is_no_count_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternBraceThatIsNoCount");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "A{x}")' holds '{' that is not a count such as {2} or {2,5}; escape it as \{. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_count_of_nothing_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternCountOfNothing");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "{2}A")' holds '{2}' where nothing it can repeat comes before it. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_with_backward_count_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternBackwardCount");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "A{5,2}")' holds the count '{5,2}', whose least is above its most. (Parameter 'predicate')""");
    }

    [Test]
    public async Task case_ignored_by_regex_options_is_refused_with_the_spelled_cases_door()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternIgnoringCase");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "^[a-d]", IgnoreCase, CultureInvariant)' compares case by RegexOptions.IgnoreCase, which .NET and the browser fold differently (s against ſ); spell both cases, such as [Aa]spirin, or move the work to the server. (Parameter 'predicate')""");
    }

    [Test]
    public async Task an_option_the_browser_does_not_read_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternWithMultiline");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, "^A", Multiline)' passes RegexOptions.Multiline, which the browser does not read as .NET does; drop it, or move the work to the server. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_read_from_the_item_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternFromTheItem");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, d.Label)' reads its pattern from the item; pass a pattern the view holds, so it is checked where the view renders. (Parameter 'predicate')""");
    }

    [Test]
    public async Task a_pattern_that_is_null_when_the_view_renders_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/PatternThatIsNull");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            """ArgumentException: 'IsMatch(d.Medicine, missing)' passes a pattern that is null when the view renders. (Parameter 'predicate')""");
    }
}
