namespace Alis.Reactive.Framework.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenASlotReleasesWhatItOwns : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator RingTheLoungeBell => Page.Locator("#lounge-bell").GetByRole(AriaRole.Button, new() { Name = "Ring the bell", Exact = true });

    [Test]
    public async Task unloading_a_slot_releases_every_handler_it_owns_even_when_releasing_one_fails_on_a_live_component()
    {
        await Open("/Slots/Lounge");
        await Button("Open the visitor card").ClickAsync();
        await Page.Locator("#visitor-bell").GetByRole(AriaRole.Button, new() { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(Page.Locator("#visitor-bell-heard")).ToHaveTextAsync("1");
        await RingTheLoungeBell.ClickAsync();
        await Expect(Page.Locator("#card-heard")).ToHaveTextAsync("1");
        await Page.Locator("#visitor-meter").GetByRole(AriaRole.Button, new() { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(Page.Locator("#visitor-meter-heard")).ToHaveTextAsync("2");

        await Button("Close the visitor card").ClickAsync();
        await Expect(Page.Locator("#visitor-slot")).ToBeEmptyAsync();
        await RingTheLoungeBell.ClickAsync();

        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#card-heard")).ToHaveTextAsync("1");
    }
}
