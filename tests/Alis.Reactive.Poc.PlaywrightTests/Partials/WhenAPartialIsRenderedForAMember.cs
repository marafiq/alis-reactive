namespace Alis.Reactive.Poc.PlaywrightTests.Partials;

[TestFixture]
public sealed class WhenAPartialIsRenderedForAMember : AppPageTest
{
    [Test]
    public async Task a_partial_for_a_member_shows_the_value_its_member_holds()
    {
        await Open("/Families/Family");

        await Expect(Page.GetByLabel("Next of kin")).ToHaveValueAsync("Sam Park");
    }

    [Test]
    public async Task a_partial_for_the_model_itself_shows_the_model()
    {
        await Open("/Families/Family");

        await Expect(Page.Locator("#summary-resident")).ToHaveTextAsync("Helen Park");
    }

    [Test]
    public async Task a_partial_for_a_member_posts_its_input_under_the_members_name()
    {
        await Open("/Families/Family");
        await Page.GetByLabel("Next of kin").FillAsync("Max Park");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save family" }).ClickAsync();

        await Expect(Page.Locator("#saved-kin")).ToHaveTextAsync("Max Park");
    }

    [Test]
    public async Task a_partial_for_a_list_item_posts_its_input_at_the_items_position()
    {
        await Open("/Families/Family");
        await Page.GetByLabel("Mobile phone").FillAsync("07700 900999");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save family" }).ClickAsync();

        await Expect(Page.Locator("#saved-mobile")).ToHaveTextAsync("07700 900999");
    }

    [Test]
    public async Task a_partial_inside_a_partial_posts_its_input_under_both_members_names()
    {
        await Open("/Families/Family");
        await Page.GetByLabel("City").FillAsync("Leeds");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save family" }).ClickAsync();

        await Expect(Page.Locator("#saved-city")).ToHaveTextAsync("Leeds");
    }

    [Test]
    public async Task a_partial_for_a_member_that_holds_nothing_posts_its_input_under_the_members_name()
    {
        await Open("/Families/Family");
        await Page.GetByLabel("Guardian").FillAsync("Lee Park");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save family" }).ClickAsync();

        await Expect(Page.Locator("#saved-guardian")).ToHaveTextAsync("Lee Park");
    }

    [Test]
    public async Task the_pages_plan_reads_an_input_a_partial_for_its_member_renders()
    {
        await Open("/Families/Family");
        await Page.GetByLabel("Next of kin").FillAsync("Max Park");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Preview the family card" }).ClickAsync();

        await Expect(Page.Locator("#card-kin")).ToHaveTextAsync("Max Park");
    }

    [Test]
    public async Task the_pages_plan_reads_an_input_a_partial_for_a_list_item_renders()
    {
        await Open("/Families/Family");
        await Page.GetByLabel("Mobile phone").FillAsync("07700 900999");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Preview the family card" }).ClickAsync();

        await Expect(Page.Locator("#card-mobile")).ToHaveTextAsync("07700 900999");
    }

    [Test]
    public async Task each_list_items_partial_reads_its_own_input()
    {
        await Open("/Families/Family");
        await Page.GetByLabel("Mobile phone").FillAsync("07700 900999");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Dial Mobile" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Group, new() { Name = "Mobile" }).Locator("strong")).ToHaveTextAsync("07700 900999");
    }

    [Test]
    public async Task two_models_that_bind_one_name_keep_two_ids()
    {
        await Open("/Families/Family");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm the visit" }).ClickAsync();

        await Expect(Page.Locator("#visit-resident")).ToHaveTextAsync("Ada Byron");
    }

    [Test]
    public async Task the_pages_plan_reads_an_input_mvcs_partial_tag_helper_renders_for_a_member()
    {
        await Open("/Families/FamilyByTagHelper");
        await Page.GetByLabel("Next of kin").FillAsync("Max Park");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Preview the family card" }).ClickAsync();

        await Expect(Page.Locator("#card-kin")).ToHaveTextAsync("Max Park");
    }

    [Test]
    public async Task the_pages_plan_reads_an_input_an_editor_template_renders_for_a_member()
    {
        await Open("/Families/FamilyByEditor");
        await Page.GetByLabel("Next of kin").FillAsync("Max Park");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Preview the family card" }).ClickAsync();

        await Expect(Page.Locator("#card-kin")).ToHaveTextAsync("Max Park");
    }
}
