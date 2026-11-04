namespace Alis.Reactive.Framework.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenAStepWritesOverASlot : AppPageTest
{
    private async Task Render(string step) => await Page.GotoAsync($"{BaseUrl}/Slots/PassDesk?step={step}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_step_that_writes_an_element_beside_a_slot_is_not_refused()
    {
        await Open("/Slots/PassDesk?step=WriteBesideTheSlot");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Mark the pass printed", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#pass-status")).ToHaveTextAsync("printed");
    }

    [Test]
    public async Task a_step_that_sets_the_html_of_a_slot_its_own_plan_loads_into_is_refused_naming_unload()
    {
        await Render("ClearTheSlotsHtml");

        await Expect(Message(
            "The plan of /Slots/PassDesk.cshtml sets the HTML of element 'pass-slot', a slot the plan of " +
            "/Slots/PassDesk.cshtml loads or unloads, so what the slot loaded would keep running under content it no " +
            "longer shows. Empty the slot with p.Unload(\"pass-slot\"), and write the HTML into an element beside it.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_partials_step_that_sets_the_text_of_a_slot_the_pages_plan_loads_into_is_refused_naming_unload()
    {
        await Render("WriteTheSlotsTextFromAPartial");

        await Expect(Message(
            "The plan of /Slots/PassNotice.cshtml sets the text of element 'pass-slot', a slot the plan of " +
            "/Slots/PassDesk.cshtml loads or unloads, so what the slot loaded would keep running under content it no " +
            "longer shows. Empty the slot with p.Unload(\"pass-slot\"), and write the text into an element beside it.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_step_that_sets_the_text_of_a_slot_its_plan_unloads_is_refused_naming_unload()
    {
        await Render("WriteTheTextOfTheSlotItUnloads");

        await Expect(Message(
            "The plan of /Slots/PassDesk.cshtml sets the text of element 'pass-slot', a slot the plan of " +
            "/Slots/PassDesk.cshtml loads or unloads, so what the slot loaded would keep running under content it no " +
            "longer shows. Empty the slot with p.Unload(\"pass-slot\"), and write the text into an element beside it.")).ToBeVisibleAsync();
    }
}
