namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

[TestFixture]
public sealed class WhenARequestIsGivenAPartTwice : AppPageTest
{
    private async Task Render(string part) => await Page.GotoAsync($"{BaseUrl}/Requests/Repeated?part={part}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_second_gather_is_refused()
    {
        await Render("Gather");

        await Expect(Message("A request has one Gather; set every member and header of it in that one callback.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_response_is_refused()
    {
        await Render("Response");

        await Expect(Message("A request has one Response; write every route of it in that one callback.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_while_loading_is_refused()
    {
        await Render("WhileLoading");

        await Expect(Message("A request has one WhileLoading; write every step of it in that one callback.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_finally_is_refused()
    {
        await Render("Finally");

        await Expect(Message("A request has one Finally; write every step of it in that one callback.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_form_to_validate_is_refused()
    {
        await Render("Validate");

        await Expect(Message("A request validates one form: call Validate<TValidator>(formId) once.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_body_format_is_refused()
    {
        await Render("BodyFormat");

        await Expect(Message("A request has one body format: call AsJson() or AsFormData() once.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_on_all_settled_is_refused()
    {
        await Render("OnAllSettled");

        await Expect(Message("Parallel requests have one OnAllSettled; write every step of it in that one callback.")).ToBeVisibleAsync();
    }
}
