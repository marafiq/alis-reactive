namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

[TestFixture]
public sealed class WhenACardChecksThePagesForm : AppPageTest
{
    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    [Test]
    public async Task the_message_no_field_shows_appears_in_the_summary_of_the_form_the_card_checks()
    {
        await Open("/Partials/Handover");
        await Input("Room").FillAsync("12");
        await Input("Keyholder").FillAsync("Tom");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Hand the keys over", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#handover-form_validation_summary")).ToHaveTextAsync("Say how the keyholder is related.");
    }
}
