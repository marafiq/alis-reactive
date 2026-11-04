namespace Alis.Reactive.Framework.PlaywrightTests.Forms;

[TestFixture]
public sealed class WhenAPageHoldsTwoForms : AppPageTest
{
    private const string VisitedNoOne = "Say whom the visitor came to see.";
    private const string SignedByNoOne = "Say who signs for the parcel.";

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator SummaryOf(string formId) => Page.Locator($"#{formId}_validation_summary");

    [Test]
    public async Task each_form_shows_the_message_no_field_shows_in_its_own_summary()
    {
        await Open("/Forms/FrontDesk");
        await Button("Log the visit").ClickAsync();
        await Expect(Page.GetByText(VisitedNoOne)).ToBeVisibleAsync();
        await Button("Log the parcel").ClickAsync();
        await Expect(Page.GetByText(SignedByNoOne)).ToBeVisibleAsync();

        await Expect(SummaryOf("visit-form")).ToHaveTextAsync(VisitedNoOne);
        await Expect(SummaryOf("parcel-form")).ToHaveTextAsync(SignedByNoOne);
    }
}
