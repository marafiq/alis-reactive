namespace Alis.Reactive.Framework.PlaywrightTests.Forms;

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
