namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// A request has one of each part: one Gather, one Validate, one WhileLoading, one Response, one Finally and one
/// body format. A second call of any of them would silently replace what the first wrote, so it is refused where
/// the page renders, with the message that names the one call to write instead.
/// </summary>
[TestFixture]
public sealed class WhenAPartOfARequestIsWrittenTwice : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string part) => await Page.GotoAsync($"{BaseUrl}/Requests/WrittenTwice?part={part}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_second_gather_is_refused_naming_the_one_callback_that_gathers_every_member()
    {
        await Render("Gather");

        await Expect(Message("A request has one Gather; gather every member it sends in that one callback.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_validate_is_refused_naming_the_one_form_it_checks()
    {
        await Render("Validate");

        await Expect(Message("A request checks one form before it is sent; call Validate once, naming that form.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_while_loading_is_refused_naming_the_one_callback_its_steps_go_in()
    {
        await Render("WhileLoading");

        await Expect(Message("A request has one WhileLoading; write every step of it in that one callback.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_response_is_refused_naming_the_one_callback_its_routes_go_in()
    {
        await Render("Response");

        await Expect(Message("A request has one Response; write every route of it in that one callback.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_finally_is_refused_naming_the_one_callback_its_steps_go_in()
    {
        await Render("Finally");

        await Expect(Message("A request has one Finally; write every step of it in that one callback.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_second_body_format_is_refused_naming_the_one_call_that_picks_it()
    {
        await Render("BodyFormat");

        await Expect(Message("A request has one body format; call AsJson() or AsFormData() once.")).ToBeVisibleAsync();
    }
}
