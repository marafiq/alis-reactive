namespace Alis.Reactive.Framework.PlaywrightTests.Inputs;

/// <summary>
/// One member's field rendered twice on one plan (I8): with the same component it is one input, and the page
/// renders and sends it; with another component it is a defect of the view, refused when the page renders with
/// a message naming the member and both components, a generic one as C# writes it.
/// </summary>
[TestFixture]
public sealed class WhenAFieldIsRenderedTwice : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string path) => await Page.GotoAsync(BaseUrl + path);

    [Test]
    public async Task a_field_rendered_twice_with_the_same_component_is_one_input_and_the_page_sends_it()
    {
        await Open("/Inputs/Depart?rendering=WithTheSameComponent");

        await Page.GetByLabel("Visitor", new() { Exact = true }).FillAsync("Ada");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign out", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#bound")).ToHaveTextAsync("visitor Ada");
    }

    [Test]
    public async Task a_field_rendered_twice_with_another_component_is_refused_naming_both()
    {
        await Render("/Inputs/Depart?rendering=WithAnotherComponent");

        await Expect(Page.GetByText(
            "Input 'Visitor' is rendered twice with different components: " +
            "first [Alis_Reactive_Framework_App_Inputs_DepartPage__Visitor, entry, value, committed by committed, Entry, string], " +
            "then [Alis_Reactive_Framework_App_Inputs_DepartPage__Visitor, file-picker, files, committed by chosen, FilePicker, string].").First)
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task a_generic_component_in_the_refusal_is_named_as_csharp_writes_it()
    {
        await Render("/Inputs/Depart?rendering=TheKindWithAnotherComponent");

        await Expect(Page.GetByText(
            "Input 'Kind' is rendered twice with different components: " +
            "first [Alis_Reactive_Framework_App_Inputs_DepartPage__Kind, choice, value, committed by changed, Choice<VisitKind>, string], " +
            "then [Alis_Reactive_Framework_App_Inputs_DepartPage__Kind, entry, value, committed by committed, Entry, string].").First)
            .ToBeVisibleAsync();
    }
}
