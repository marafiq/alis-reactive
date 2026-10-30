namespace Alis.Reactive.Framework.PlaywrightTests.Forms;

/// <summary>
/// A form is an element (ADR-0003 rule 2, grammar.md 5.1): <c>Form()</c> sends the mounted inputs inside the form
/// its request validates, and <c>Form(formId)</c> the inputs inside the element it names, whichever plan rendered
/// them; an input outside that element is not sent.
/// </summary>
[TestFixture]
public sealed class WhenAFormSendsWhatItHolds : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private async Task FillTheOuting()
    {
        await Input("Resident").FillAsync("Ann");
        await Input("Companion").FillAsync("Bo");
        await Input("Note for the desk").FillAsync("Back by six");
    }

    [Test]
    public async Task a_form_sends_the_inputs_inside_it_and_none_outside()
    {
        await Open("/Forms/Outing");
        await FillTheOuting();

        await Button("Book the outing").ClickAsync();

        await Expect(Page.Locator("#outing-bound")).ToHaveTextAsync("resident Ann, companion Bo, note none");
    }

    [Test]
    public async Task a_request_naming_an_element_sends_only_the_inputs_inside_it()
    {
        await Open("/Forms/Outing");
        await FillTheOuting();

        await Button("Save the companion").ClickAsync();

        await Expect(Page.Locator("#outing-bound")).ToHaveTextAsync("resident none, companion Bo, note none");
    }
}
