namespace Alis.Reactive.Framework.PlaywrightTests.Forms;

[TestFixture]
public sealed class WhenARequestSendsAFormItDoesNotName : AppPageTest
{
    [Test]
    public async Task a_request_that_sends_its_form_but_validates_none_is_refused_naming_both_fixes()
    {
        await Page.GotoAsync($"{BaseUrl}/Forms/Unnamed");

        await Expect(Page.GetByText(
            "The POST request to '/Forms/Bound' sends Form(), the inputs of the form it validates, but it validates no " +
            "form. Name the element whose inputs it sends, Form(\"formId\"), or validate that form first, " +
            "Validate<TValidator>(\"formId\").").First).ToBeVisibleAsync();
    }
}
