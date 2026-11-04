namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

[TestFixture]
public sealed class WhenTwoPartialsOfOtherModelsBindOneName : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private async Task CorrectTheRoomAndThePhone()
    {
        await Open("/Partials/Corrections");
        await Input("Room").FillAsync("12B");
        await Input("Phone").FillAsync("555-0101");
    }

    [Test]
    public async Task a_partials_named_send_sends_the_input_it_rendered()
    {
        await CorrectTheRoomAndThePhone();

        await Button("Save the room").ClickAsync();

        await Expect(Page.Locator("#room-saved")).ToHaveTextAsync("room 12B");
    }

    [Test]
    public async Task a_partials_bound_value_is_the_input_it_rendered()
    {
        await CorrectTheRoomAndThePhone();

        await Button("Read the room").ClickAsync();

        await Expect(Page.Locator("#room-read")).ToHaveTextAsync("12B");
    }

    [Test]
    public async Task a_partial_whose_input_left_the_page_sends_nothing_rather_than_another_partials_input()
    {
        await CorrectTheRoomAndThePhone();
        await Button("Save the room").ClickAsync();
        await Expect(Page.Locator("#room-saved")).ToHaveTextAsync("room 12B");

        await Button("Put the room's field away").ClickAsync();
        await Expect(Input("Room")).ToHaveCountAsync(0);
        await Button("Save the room").ClickAsync();

        await Expect(Page.Locator("#room-saved")).ToHaveTextAsync("room none");
    }
}
