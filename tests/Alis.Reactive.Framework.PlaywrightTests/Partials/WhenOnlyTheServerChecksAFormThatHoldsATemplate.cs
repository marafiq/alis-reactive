namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// A form no request validates, whose error route shows the server's errors on it (<c>e.ValidationErrors(form)</c>),
/// holds a member's editor template, a partial with a plan of its own (ADR-0003 rule 2, grammar.md F8): the
/// server's message for the member lands beside the input the template rendered, which the form holds, and typing
/// into that input clears it (F7).
/// </summary>
[TestFixture]
public sealed class WhenOnlyTheServerChecksAFormThatHoldsATemplate : AppPageTest
{
    private const string NotOnFile = "That name is not on the resident's file.";

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private ILocator KinField => Page.Locator("#kin-card > div").Filter(new() { Has = Input("Next of kin") });

    private async Task BookTheSitting()
    {
        await Input("Visitor").FillAsync("Ada");
        await Input("Next of kin").FillAsync("Zed");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Book the sitting", Exact = true }).ClickAsync();
    }

    [Test]
    public async Task the_servers_message_for_the_member_lands_beside_the_input_the_template_rendered()
    {
        await Open("/Partials/Sitting");

        await BookTheSitting();

        await Expect(KinField.GetByText(NotOnFile)).ToBeVisibleAsync();
    }

    [Test]
    public async Task typing_into_the_templates_input_clears_the_servers_message()
    {
        await Open("/Partials/Sitting");
        await BookTheSitting();
        await Expect(KinField.GetByText(NotOnFile)).ToBeVisibleAsync();

        await Input("Next of kin").PressSequentiallyAsync("a");

        await Expect(KinField.GetByText(NotOnFile)).ToBeHiddenAsync();
    }
}
