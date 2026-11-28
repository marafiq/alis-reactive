namespace Alis.Reactive.Poc.PlaywrightTests.Partials;

[TestFixture]
public sealed class WhenMvcRendersAMembersView : AppPageTest
{
    [Test]
    public async Task the_pages_plan_reads_an_input_an_editor_template_named_for_a_member_renders()
    {
        await Open("/Families/FamilyByNamedTemplate");
        await Page.GetByLabel("Guardian").FillAsync("Lee Park");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Preview the family card" }).ClickAsync();

        await Expect(Page.Locator("#card-kin")).ToHaveTextAsync("Lee Park");
    }

    [Test]
    public async Task the_pages_plan_reads_an_input_a_template_the_model_names_by_its_ui_hint_renders()
    {
        await Open("/Families/FamilyByHint");
        await Page.GetByLabel("Guardian").FillAsync("Lee Park");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Preview the family card" }).ClickAsync();

        await Expect(Page.Locator("#card-kin")).ToHaveTextAsync("Lee Park");
    }

    [Test]
    public async Task the_pages_plan_reads_an_input_an_editor_for_a_list_renders_for_an_item()
    {
        await Open("/Families/ContactsByEditor");
        await Page.GetByLabel("Mobile phone").FillAsync("07700 900999");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Preview the family card" }).ClickAsync();

        await Expect(Page.Locator("#card-kin")).ToHaveTextAsync("07700 900999");
    }

    [Test]
    public async Task the_pages_plan_reads_an_input_a_partial_loaded_under_the_members_prefix_renders()
    {
        await Open("/Families/FamilyBySlot");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Load the next of kin" }).ClickAsync();
        await Page.GetByLabel("Next of kin").FillAsync("Max Park");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Preview the family card" }).ClickAsync();

        await Expect(Page.Locator("#card-kin")).ToHaveTextAsync("Max Park");
    }
}
