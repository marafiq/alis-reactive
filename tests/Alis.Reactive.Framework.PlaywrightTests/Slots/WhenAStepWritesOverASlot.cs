namespace Alis.Reactive.Framework.PlaywrightTests.Slots;

/// <summary>
/// What a slot shows belongs to what was loaded into it (ADR-0003): a step that sets the text or HTML of an element
/// some plan of the page loads into or unloads (<c>Into</c>, <c>Unload</c>) would leave what the slot loaded running
/// under content it no longer shows, so the page is refused when it renders, naming <c>p.Unload</c>, whichever plan
/// writes the step: the plan that loads the slot, a partial's plan beside it, or a plan that unloads it. A step that
/// writes an element beside the slot is not refused.
/// </summary>
[TestFixture]
public sealed class WhenAStepWritesOverASlot : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
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
