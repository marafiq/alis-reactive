namespace Alis.Reactive.Framework.PlaywrightTests.Conditions;

/// <summary>
/// The conditions a page is refused for when it renders, with the message the developer reads: an ElseIf
/// after the Else, a second Else, and a negative minimum length.
/// </summary>
[TestFixture]
public sealed class WhenABranchIsWrittenWrongly : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string path) => await Page.GotoAsync(BaseUrl + path);

    [Test]
    public async Task an_else_if_after_the_else_is_refused()
    {
        await Render("/Conditions/ElseIfAfterElse");

        await Expect(Page.GetByText("An ElseIf cannot follow the Else of its When.").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_else_is_refused()
    {
        await Render("/Conditions/ElseTwice");

        await Expect(Page.GetByText("A When has one Else.").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_order_of_a_length_of_time_is_refused()
    {
        await Render("/Conditions/SpanOrder");

        await Expect(Page.GetByText("A TimeSpan has no order the browser can compute as C# does").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_negative_minimum_length_is_refused()
    {
        await Render("/Conditions/NegativeMinLength");

        await Expect(Page.GetByText("Condition minimum text length must be zero or greater.").First).ToBeVisibleAsync();
    }
}
