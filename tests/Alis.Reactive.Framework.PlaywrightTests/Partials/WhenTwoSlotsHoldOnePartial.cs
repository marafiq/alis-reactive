namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

[TestFixture]
public sealed class WhenTwoSlotsHoldOnePartial : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Slot(string name) => Page.GetByRole(AriaRole.Group, new() { Name = name, Exact = true });

    [Test]
    public async Task unloading_one_slot_keeps_the_input_the_other_slot_holds()
    {
        await Open("/Partials/DoubleBooked");
        await Button("Add the first next of kin").ClickAsync();
        await Slot("The first next of kin").GetByRole(AriaRole.Textbox).FillAsync("Ann");
        await Button("Add the second next of kin").ClickAsync();
        await Slot("The second next of kin").GetByRole(AriaRole.Textbox).FillAsync("Bo");
        await Button("Remove the second next of kin").ClickAsync();
        await Expect(Slot("The second next of kin").GetByRole(AriaRole.Textbox)).ToHaveCountAsync(0);

        await Button("Save the visit").ClickAsync();

        await Expect(Page.Locator("#double-saved")).ToHaveTextAsync("visitor none, next of kin Ann");
    }
}
