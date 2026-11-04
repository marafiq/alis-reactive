namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

[TestFixture]
public sealed class WhenAMembersClassIsGeneric : AppPageTest
{
    [Test]
    public async Task the_scope_is_the_namespace_and_the_csharp_name_with_other_characters_as_underscores()
    {
        await Open("/Partials/Fluids");
        await Page.GetByLabel("Initials, by the id written out").FillAsync("AB");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Chart the fluids", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#charted")).ToHaveTextAsync("no ml, initials AB");
    }
}
