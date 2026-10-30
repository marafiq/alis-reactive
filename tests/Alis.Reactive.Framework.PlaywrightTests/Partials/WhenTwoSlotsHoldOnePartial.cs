namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// Two slots each load the same template in a request of their own, so the server cannot refuse the second copy
/// and the page holds its input twice under one id. An id names one element (grammar.md M6): each slot's unload
/// releases exactly the input it loaded, so the copy the other slot holds is still the one the page reads.
/// </summary>
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
