namespace Alis.Reactive.Framework.PlaywrightTests.Inputs;

/// <summary>
/// One member's field rendered twice on a page (I8): two elements cannot share one id, so a field rendered a
/// second time is refused when the page renders, by the same plan or by another, with a message naming the
/// element id and both views; a field rendered once renders and is sent.
/// </summary>
[TestFixture]
public sealed class WhenAFieldIsRenderedTwice : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string path) => await Page.GotoAsync(BaseUrl + path);

    [Test]
    public async Task a_field_rendered_once_renders_and_the_page_sends_it()
    {
        await Open("/Inputs/Depart");

        await Page.GetByLabel("Visitor", new() { Exact = true }).FillAsync("Ada");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign out", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#bound")).ToHaveTextAsync("visitor Ada");
    }

    [Test]
    public async Task a_field_rendered_twice_on_one_plan_is_refused_naming_its_element_id()
    {
        await Render("/Inputs/Depart?rendering=TwiceOnOnePlan");

        await Expect(Page.GetByText(
            "Input 'Visitor' renders element id 'Alis_Reactive_Framework_App_Inputs_DepartPage__Visitor' a second time on this page: " +
            "first in /Inputs/Depart.cshtml, then in /Inputs/Depart.cshtml. An element id names one element, so the page would " +
            "reach only the first: render the field once, or each copy under a field prefix of its own (Html.EditorFor, or a " +
            "partial given a ViewDataDictionary whose TemplateInfo has its own HtmlFieldPrefix).").First)
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task a_field_a_partials_plan_renders_again_is_refused_naming_both_views()
    {
        await Render("/Inputs/Depart?rendering=AgainInAPartial");

        await Expect(Page.GetByText(
            "Input 'Visitor' renders element id 'Alis_Reactive_Framework_App_Inputs_DepartPage__Visitor' a second time on this page: " +
            "first in /Inputs/Depart.cshtml, then in /Inputs/DepartPass.cshtml.").First)
            .ToBeVisibleAsync();
    }
}
