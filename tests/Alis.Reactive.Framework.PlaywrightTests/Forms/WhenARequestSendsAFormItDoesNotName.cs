namespace Alis.Reactive.Framework.PlaywrightTests.Forms;

/// <summary>
/// <c>Form()</c> sends the inputs of the form its request validates (ADR-0003 rule 2), so a request that sends
/// <c>Form()</c> and validates no form names no inputs: the page is refused when it renders, with the message the
/// developer reads, naming both fixes.
/// </summary>
[TestFixture]
public sealed class WhenARequestSendsAFormItDoesNotName : AppPageTest
{
    [Test]
    public async Task a_request_that_sends_its_form_but_validates_none_is_refused_naming_both_fixes()
    {
        // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
        await Page.GotoAsync($"{BaseUrl}/Forms/Unnamed");

        await Expect(Page.GetByText(
            "The POST request to '/Forms/Bound' sends Form(), the inputs of the form it validates, but it validates no " +
            "form. Name the element whose inputs it sends, Form(\"formId\"), or validate that form first, " +
            "Validate<TValidator>(\"formId\").").First).ToBeVisibleAsync();
    }
}
