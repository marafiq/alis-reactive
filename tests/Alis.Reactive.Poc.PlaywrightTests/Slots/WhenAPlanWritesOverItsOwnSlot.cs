namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenAPlanWritesOverItsOwnSlot : AppPageTest
{
    [Test]
    public async Task a_plan_that_sets_the_text_of_its_slot_before_it_loads_it_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/TextBeforeItsSlot");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: The plan sets the text of element 'card', a slot this plan loads or unloads, so what the slot loaded would keep running under content it no longer shows. Empty the slot with Unload(\"card\"), and write the text into an element beside it.");
    }

    [Test]
    public async Task a_plan_that_sets_the_html_of_its_slot_after_it_unloads_it_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/HtmlAfterItsUnload");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: The plan sets the HTML of element 'card', a slot this plan loads or unloads, so what the slot loaded would keep running under content it no longer shows. Empty the slot with Unload(\"card\"), and write the HTML into an element beside it.");
    }
}
