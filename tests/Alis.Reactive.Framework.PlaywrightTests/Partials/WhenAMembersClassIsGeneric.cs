namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// An input binds a member a generic class declares (ADR-0003 rule 1): the id's scope is the class's
/// namespace and its name as C# writes it, every character outside letters, digits and underscores made an
/// underscore, then two underscores and the member's path, so a label the page writes with that id names it.
/// </summary>
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
