namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenARadioCannotBeListenedTo : AppPageTest
{
    [Test]
    public async Task a_declaration_whose_object_lacks_its_unsubscribe_starts_no_handler()
    {
        await Open("/Cards/Radios");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Announce on the ward radio", Exact = true }).ClickAsync();
        await Expect(Page.Locator("#announced")).ToHaveTextAsync("Announced on the ward radio");

        await Expect(Page.Locator("#calls li")).ToHaveCountAsync(0);
    }

    [Test]
    public async Task a_subscribe_that_returns_no_remover_starts_no_handler()
    {
        await Open("/Cards/Radios");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Announce on the night radio", Exact = true }).ClickAsync();
        await Expect(Page.Locator("#announced")).ToHaveTextAsync("Announced on the night radio");

        await Expect(Page.Locator("#calls li")).ToHaveCountAsync(0);
    }
}
