namespace Alis.Reactive.Framework.PlaywrightTests.Slots;

/// <summary>
/// Unloading a slot releases every handler its owner holds (grammar 3.5), even when releasing one of them
/// fails: the visitor card's own bell takes its callback property away when it is destroyed, so its handler
/// cannot be released, and the card's handler of the lounge's bell is released all the same.
/// </summary>
[TestFixture]
public sealed class WhenASlotReleasesWhatItOwns : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator RingTheLoungeBell => Page.Locator("#lounge-bell").GetByRole(AriaRole.Button, new() { Name = "Ring the bell", Exact = true });

    [Test]
    public async Task unloading_a_slot_releases_every_handler_it_owns_even_when_releasing_one_fails()
    {
        await Open("/Slots/Lounge");
        await Button("Open the visitor card").ClickAsync();
        await Page.Locator("#visitor-bell").GetByRole(AriaRole.Button, new() { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(Page.Locator("#visitor-bell-heard")).ToHaveTextAsync("1");
        await RingTheLoungeBell.ClickAsync();
        await Expect(Page.Locator("#card-heard")).ToHaveTextAsync("1");

        await Button("Close the visitor card").ClickAsync();
        await Expect(Page.Locator("#visitor-slot")).ToBeEmptyAsync();
        await RingTheLoungeBell.ClickAsync();

        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#card-heard")).ToHaveTextAsync("1");
    }
}
