namespace Alis.Reactive.Framework.PlaywrightTests.Forms;

/// <summary>
/// The summary of a form belongs to the form, not to a plan that joins it (ADR-0003, grammar.md F9): a line loaded
/// into the stay's form joins the form with a check of its own, and once it is cleared away the stay's own save
/// still shows the message no field shows in the form's summary.
/// </summary>
[TestFixture]
public sealed class WhenALoadedLineJoinsTheFormAndLeaves : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task the_forms_summary_outlives_a_slot_that_joined_it_and_left()
    {
        await Open("/Forms/Stay");
        await Button("Pick the guest").ClickAsync();
        await Expect(Page.GetByLabel("Guest", new() { Exact = true })).ToBeVisibleAsync();
        await Button("Clear the guest").ClickAsync();
        await Expect(Page.Locator("#guest-slot")).ToBeEmptyAsync();

        await Button("Book the stay").ClickAsync();

        await Expect(Page.Locator("#stay-messages").GetByText("Say when the stay starts.")).ToBeVisibleAsync();
    }
}
