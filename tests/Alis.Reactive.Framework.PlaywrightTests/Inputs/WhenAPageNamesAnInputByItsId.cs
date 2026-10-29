namespace Alis.Reactive.Framework.PlaywrightTests.Inputs;

/// <summary>
/// The id an input is rendered with, derived from its member (I7, ADR-0003 rule 1): the full name of the class
/// that declares the member, with its dots, and the plus of a type nested in another, as underscores, then two
/// underscores, then the member's whole path with its dots as underscores; and the id <c>IdGenerator.For</c>
/// gives for a member, of a value type or typed, is the id of the input the field rendered for that member, so
/// a label the page writes with it names that input.
/// </summary>
[TestFixture]
public sealed class WhenAPageNamesAnInputByItsId : AppPageTest
{
    private ILocator Bound => Page.Locator("#bound");

    private ILocator Save => Page.GetByRole(AriaRole.Button, new() { Name = "Save the room", Exact = true });

    [Test]
    public async Task an_inputs_id_is_the_full_name_of_the_class_declaring_its_member_then_its_path_joined_by_underscores()
    {
        await Open("/Inputs/Room");

        await Page.GetByLabel("Phone, by the id written out").FillAsync("555");
        await Save.ClickAsync();

        await Expect(Bound).ToHaveTextAsync("bed 0, note none, phone 555");
    }

    [Test]
    public async Task the_id_idgenerator_gives_for_a_member_is_the_id_of_the_input_rendered_for_it()
    {
        await Open("/Inputs/Room");

        await Page.GetByLabel("Bed, by the id of its member").FillAsync("4");
        await Save.ClickAsync();

        await Expect(Bound).ToHaveTextAsync("bed 4, note none, phone none");
    }

    [Test]
    public async Task the_id_idgenerator_gives_for_a_typed_member_is_the_id_of_the_input_rendered_for_it()
    {
        await Open("/Inputs/Room");

        await Page.GetByLabel("Note, by the id of its typed member").FillAsync("quiet");
        await Save.ClickAsync();

        await Expect(Bound).ToHaveTextAsync("bed 0, note quiet, phone none");
    }
}
