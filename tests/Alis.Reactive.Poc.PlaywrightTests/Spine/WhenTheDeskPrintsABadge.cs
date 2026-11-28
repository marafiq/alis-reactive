namespace Alis.Reactive.Poc.PlaywrightTests.Spine;

[TestFixture]
public sealed class WhenTheDeskPrintsABadge : AppPageTest
{
    [Test]
    public async Task the_badge_shows_the_name_in_the_box_when_print_is_clicked()
    {
        await Open("/FrontDesk/Badge");

        await Page.GetByLabel("Visitor name").FillAsync("Omar Haddad");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Print badge" }).ClickAsync();

        await Expect(Page.Locator("#badge-name")).ToHaveTextAsync("Omar Haddad");
    }

    [Test]
    public async Task the_badge_stays_blank_until_print_is_clicked()
    {
        await Open("/FrontDesk/Badge");

        await Expect(Page.GetByLabel("Visitor name")).ToHaveValueAsync("Helen Park");
        await Expect(Page.Locator("#badge-name")).ToHaveTextAsync("");
    }
}
