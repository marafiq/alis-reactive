namespace Alis.Reactive.Framework.PlaywrightTests.Rendering;

[TestFixture]
public sealed class WhenAViewsPrefixChangesAfterItsPlanStarts : AppPageTest
{
    private async Task Render(string path) => await Page.GotoAsync(BaseUrl + path);

    [Test]
    public async Task an_input_under_the_prefix_its_plan_started_under_is_sent_under_it()
    {
        await Open("/Rendering/Relief?prefix=BeforeThePlanStarts");

        await Page.GetByLabel("Relief nurse", new() { Exact = true }).FillAsync("Bea");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Take over the shift", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#relieved")).ToHaveTextAsync("relief Bea");
    }

    [Test]
    public async Task an_input_under_a_prefix_set_after_its_plan_started_is_refused_naming_editorfor()
    {
        await Render("/Rendering/Relief?prefix=AfterThePlanStarts");

        await Expect(Page.GetByText(
            "The field prefix of /Rendering/Relief.cshtml changed from none to 'Relief' after its plan started, so input " +
            "'Relief.Name' would post under a name its plan does not send or check. On .NET Framework a ViewDataDictionary " +
            "copied from ViewData shares the view's TemplateInfo, so giving the copy a prefix gives it to the view: render " +
            "the member with Html.EditorFor, or give the copy a new TemplateInfo { HtmlFieldPrefix = \"Relief\" }.").First)
            .ToBeVisibleAsync();
    }
}
