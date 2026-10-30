namespace Alis.Reactive.Framework.PlaywrightTests.Forms;

/// <summary>
/// A form checks the tests written for it, and first clears every inline message inside it (ADR-0003 rule 2,
/// grammar.md F4): a section of the form saved on its own, by rules of its own, shows its message beside its
/// field, and the whole form's save, whose rules do not ask for that field, clears the message and is sent.
/// </summary>
[TestFixture]
public sealed class WhenAFormIsCheckedAfterOneOfItsSections : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private ILocator CompanionField => Page.Locator("#companion-section > div").Filter(new() { Has = Input("Companion") });

    [Test]
    public async Task checking_the_form_clears_the_message_a_section_inside_it_showed()
    {
        await Open("/Forms/Lunch");
        await Input("Resident").FillAsync("Ann");
        await Button("Save the companion").ClickAsync();
        await Expect(CompanionField.GetByText("Name the companion.")).ToBeVisibleAsync();

        await Button("Book the lunch").ClickAsync();

        await Expect(Page.Locator("#lunch-bound")).ToHaveTextAsync("lunch for Ann, companion none");
        await Expect(CompanionField.GetByText("Name the companion.")).ToBeHiddenAsync();
    }
}
