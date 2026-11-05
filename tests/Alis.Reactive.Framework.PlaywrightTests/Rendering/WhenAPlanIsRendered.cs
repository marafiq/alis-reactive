namespace Alis.Reactive.Framework.PlaywrightTests.Rendering;

[TestFixture]
public sealed class WhenAPlanIsRendered : AppPageTest
{
    private async Task Render(string path) => await Page.GotoAsync(BaseUrl + path);

    [Test]
    public async Task a_plan_rendered_in_the_layouts_last_section_takes_what_an_earlier_section_wrote()
    {
        await Open("/Rendering/Handover");

        await Page.GetByLabel("Resident", new() { Exact = true }).FillAsync("Ann");
        await Page.GetByLabel("Notes for the next shift", new() { Exact = true }).FillAsync("Slept well");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Hand over", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#handed")).ToHaveTextAsync("resident Ann, notes Slept well");
    }

    [Test]
    public async Task an_input_written_after_its_plan_rendered_is_refused_naming_the_section()
    {
        await Render("/Rendering/NotesInASection");

        await Expect(Page.GetByText(
            "Input 'Notes' is written after the plan of /Rendering/NotesInASection.cshtml rendered, so the plan the page " +
            "runs does not hold it. Razor runs a @section after the view's body: render the plan in a section the layout " +
            "renders after the others, such as @section Scripts { @Html.RenderPlan(plan) }, or write the input in the body.").First)
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task behavior_written_after_its_plan_rendered_is_refused_naming_the_section()
    {
        await Render("/Rendering/SaveInASection");

        await Expect(Page.GetByText(
            "Behavior is written after the plan of /Rendering/SaveInASection.cshtml rendered, so the plan the page " +
            "runs does not hold it. Razor runs a @section after the view's body: render the plan in a section the layout " +
            "renders after the others, such as @section Scripts { @Html.RenderPlan(plan) }, or write the behavior in the body.").First)
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task a_plan_rendered_twice_is_refused()
    {
        await Render("/Rendering/RenderedTwice");

        await Expect(Page.GetByText(
            "The plan of /Rendering/RenderedTwice.cshtml is rendered twice, so every handler it holds would run twice: " +
            "render it once, after the last behavior written to it.").First)
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task a_partials_plan_rendered_in_the_partials_body_runs_beside_the_page_plan_rendered_after_it()
    {
        await Open("/Rendering/Rounds?card=InTheCardsBody");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Record the vitals", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#vitals-state")).ToHaveTextAsync("recorded");
    }

    [Test]
    public async Task a_partials_plan_a_helper_renders_when_its_markup_is_written_runs_beside_the_page_plan()
    {
        await Open("/Rendering/Rounds?card=WhenItsButtonIsWritten");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Record the vitals", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#vitals-state")).ToHaveTextAsync("recorded");
    }
}
