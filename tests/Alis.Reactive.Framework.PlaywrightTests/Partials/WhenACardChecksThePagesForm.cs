namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// A card, a member's editor template that starts a plan of its own model, which no other view on the page starts,
/// carries the save that checks the page's form (ADR-0003, grammar.md F6): the message no field of the form shows
/// appears in that form's summary, which the server renders for the form.
/// </summary>
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
