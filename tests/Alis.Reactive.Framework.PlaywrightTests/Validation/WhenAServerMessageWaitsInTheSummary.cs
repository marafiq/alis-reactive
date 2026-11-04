namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenAServerMessageWaitsInTheSummary : AppPageTest
{
    private const string NoReason = "Say why the resident came back late.";

    private ILocator Summary => Page.Locator("[data-reactive-validation-summary]");

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private async Task SignBackInWithNoReason()
    {
        await Input("Resident").FillAsync("Ada Lovelace");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign the resident back in", Exact = true }).ClickAsync();
    }

    [Test]
    public async Task the_message_leaves_the_summary_once_its_input_commits_a_new_value()
    {
        await Open("/Validation/LateReturn");
        await SignBackInWithNoReason();
        await Expect(Summary.GetByText(NoReason)).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Add the reason", Exact = true }).ClickAsync();
        await Input("Reason").FillAsync("Missed the bus back from town");
        await Input("Reason").PressAsync("Tab");

        await Expect(Summary).ToBeHiddenAsync();
    }

    [Test]
    public async Task the_message_stays_in_the_summary_while_another_input_commits()
    {
        await Open("/Validation/LateReturn");
        await SignBackInWithNoReason();
        await Expect(Summary.GetByText(NoReason)).ToBeVisibleAsync();

        await Input("Resident").FillAsync("Tom Kilburn");
        await Input("Resident").PressAsync("Tab");
        await Expect(Page.Locator("#desk-told")).ToHaveTextAsync("the desk knows who is back");

        await Expect(Summary.GetByText(NoReason)).ToBeVisibleAsync();
    }
}
