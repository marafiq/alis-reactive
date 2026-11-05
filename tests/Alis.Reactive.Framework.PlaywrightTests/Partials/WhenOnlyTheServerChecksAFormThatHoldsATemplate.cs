namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

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
