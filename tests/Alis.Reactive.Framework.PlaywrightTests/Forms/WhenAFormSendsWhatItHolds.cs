namespace Alis.Reactive.Framework.PlaywrightTests.Forms;

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

    [Test]
    public async Task an_input_bound_to_a_member_the_request_also_sets_wins_in_a_json_body_and_the_other_set_members_are_sent()
    {
        await Open("/Forms/Outing");
        await FillTheOuting();

        await Button("Book the outing with names set").ClickAsync();

        await Expect(Page.Locator("#outing-bound")).ToHaveTextAsync("resident Ann, companion Bo, note Back at five");
    }

    [Test]
    public async Task an_input_bound_to_a_member_the_request_also_sets_wins_in_form_data_and_the_other_set_members_are_sent()
    {
        await Open("/Forms/Outing");
        await FillTheOuting();

        await Button("Book the outing with names set, as form data").ClickAsync();

        await Expect(Page.Locator("#outing-bound")).ToHaveTextAsync("resident Ann, companion Bo, note Back at five");
    }
}
