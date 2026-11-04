namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

[TestFixture]
public sealed class WhenAPageLinksOnlyTheCoresStylesheet : AppPageTest
{
    private const string Red = "rgb(185, 28, 28)";

    private ILocator Pin => Page.GetByLabel("Pin", new() { Exact = true });

    private ILocator PinMessage => Page.Locator("#notice-form > div")
        .Filter(new() { Has = Pin })
        .GetByText("'Pin' must be 4 characters in length.", new() { Exact = true });

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [Test]
    public async Task a_refused_field_shows_its_message_in_red_and_a_red_border()
    {
        await Open("/Validation/Notice");

        await Pin.FillAsync("123");
        await Save();

        await Expect(PinMessage).ToBeVisibleAsync();
        await Expect(PinMessage).ToHaveCSSAsync("color", Red);
        await Expect(Pin).ToHaveCSSAsync("border-color", Red);
    }

    [Test]
    public async Task a_corrected_field_loses_its_message_and_its_border()
    {
        await Open("/Validation/Notice");
        await Pin.FillAsync("123");
        await Save();
        await Expect(Pin).ToHaveCSSAsync("border-color", Red);

        await Pin.FillAsync("1234");
        await Save();

        await Expect(Page.Locator("#saved")).ToHaveTextAsync("saved");
        await Expect(PinMessage).ToBeHiddenAsync();
        await Expect(Pin).Not.ToHaveCSSAsync("border-color", Red);
    }
}
