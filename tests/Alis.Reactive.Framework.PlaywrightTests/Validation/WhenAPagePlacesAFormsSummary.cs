namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// A page places a form's summary with <c>Html.FormSummary(formId)</c>: the message no field of the form shows
/// comes back where the page placed it, and the plan renders no second summary after its script; a page that
/// places one form's summary twice is refused where it renders, naming where the summary first rendered.
/// </summary>
[TestFixture]
public sealed class WhenAPagePlacesAFormsSummary : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string path) => await Page.GotoAsync(BaseUrl + path);

    [Test]
    public async Task a_message_no_field_shows_comes_back_where_the_page_placed_the_forms_summary()
    {
        await Open("/Validation/OutingNotes");
        await Page.GetByLabel("Resident", new() { Exact = true }).FillAsync("Ada Lovelace");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign the resident out", Exact = true }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Region, new() { Name = "Notes for the desk" })
            .GetByText("Say when the resident will be back.")).ToBeVisibleAsync();
        await Expect(Page.Locator("[data-reactive-validation-summary]")).ToHaveCountAsync(1);
    }

    [Test]
    public async Task a_forms_summary_placed_twice_is_refused_naming_where_it_first_rendered()
    {
        await Render("/Validation/OutingNotes?placedTwice=true");

        await Expect(Page.GetByText(
            "The summary of form 'outing-form' placed in /Validation/OutingNotes.cshtml renders a second time on this page: " +
            "it was rendered where /Validation/OutingNotes.cshtml placed it. A form's summary renders once: place it before " +
            "the @Html.RenderPlan(plan) of the first plan that validates the form or shows its errors, and only once.").First)
            .ToBeVisibleAsync();
    }
}
