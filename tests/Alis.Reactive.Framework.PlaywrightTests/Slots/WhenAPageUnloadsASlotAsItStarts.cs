namespace Alis.Reactive.Framework.PlaywrightTests.Slots;

/// <summary>
/// Content the server renders inside a slot starts after the page (grammar 3.5). When the page's ready handler
/// unloads that slot as the page starts, the content's owner is released before it starts, and a released owner
/// starts nothing: the draft's ready handler does not run. Kept, the draft starts after the page, as it does on
/// every page.
/// </summary>
[TestFixture]
public sealed class WhenAPageUnloadsASlotAsItStarts : AppPageTest
{
    private ILocator Status => Page.Locator("#handover-status");

    [Test]
    public async Task content_in_a_slot_the_pages_ready_handler_unloads_never_starts()
    {
        await Open("/Slots/Handover?afresh=true");

        await Expect(Status).ToHaveTextAsync("started afresh");
    }

    [Test]
    public async Task content_in_a_slot_the_page_keeps_starts_after_the_page()
    {
        await Open("/Slots/Handover");

        await Expect(Status).ToHaveTextAsync("the night shift's draft is restored");
    }
}
