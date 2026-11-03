namespace Alis.Reactive.ClassNames.PlaywrightTests.Notices;

/// <summary>
/// An app that chose its class names at startup (Bootstrap's) and links a stylesheet written for them and no
/// framework stylesheet: its fields render with those names, a refused input carries the app's mark and shows
/// its message in the app's style, and a corrected input loses both.
/// </summary>
[TestFixture]
public sealed class WhenAnAppChoosesItsClassNames : AppPageTest
{
    private const string BootstrapRed = "rgb(220, 53, 69)";

    private ILocator Title => Page.GetByLabel("Title");

    private ILocator TitleLabel => Page.Locator("label").Filter(new() { HasText = "Title" });

    private ILocator TitleMessage => Page.GetByText("'Title' must not be empty.", new() { Exact = true });

    private Task Save() => Page.GetByRole(AriaRole.Button, new() { Name = "Save", Exact = true }).ClickAsync();

    [Test]
    public async Task a_field_its_label_and_its_required_marker_wear_the_apps_classes()
    {
        await Open("/");

        await Expect(Page.Locator("#notice-form > div")).ToHaveCSSAsync("margin-bottom", "16px");
        await Expect(TitleLabel).ToHaveCSSAsync("font-weight", "600");
        await Expect(TitleLabel.GetByText("*", new() { Exact = true })).ToHaveCSSAsync("color", BootstrapRed);
    }

    [Test]
    public async Task a_refused_input_carries_the_apps_mark_and_shows_its_message_in_the_apps_style()
    {
        await Open("/");

        await Save();

        await Expect(TitleMessage).ToBeVisibleAsync();
        await Expect(TitleMessage).ToHaveCSSAsync("color", BootstrapRed);
        await Expect(Title).ToHaveCSSAsync("border-color", BootstrapRed);
    }

    [Test]
    public async Task a_corrected_input_loses_the_apps_mark_and_its_message()
    {
        await Open("/");
        await Save();
        await Expect(Title).ToHaveCSSAsync("border-color", BootstrapRed);

        await Title.FillAsync("Lunch moves to noon");
        await Save();

        await Expect(Page.Locator("#saved")).ToHaveTextAsync("saved");
        await Expect(TitleMessage).ToBeHiddenAsync();
        await Expect(Title).Not.ToHaveCSSAsync("border-color", BootstrapRed);
    }
}
