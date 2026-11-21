namespace Alis.Reactive.Poc.PlaywrightTests.Render;

[TestFixture]
public sealed class WhenAComponentStarts : AppPageTest
{
    [Test]
    public async Task a_component_starts_with_the_value_its_member_holds()
    {
        await Open("/Rounds/Round");

        await Expect(Dose().Locator("p").First).ToHaveTextAsync("0.125 mg");
    }

    [Test]
    public async Task a_component_starts_with_the_options_its_render_helper_writes_as_the_plan_writes_its_values()
    {
        await Open("/Rounds/Round");

        await Expect(Dose().Locator("p").Nth(1)).ToHaveTextAsync("oral, 08:00 and 20:00, due 2026-11-07");
    }

    [Test]
    public async Task a_component_whose_members_holder_holds_nothing_starts_with_nothing()
    {
        await Open("/Rounds/Round");

        await Expect(Page.GetByRole(AriaRole.Group, new() { Name = "Rescue dose" }).Locator("p").First).ToHaveTextAsync("no dose");
    }

    [Test]
    public async Task a_pipeline_reads_the_value_a_component_started_with()
    {
        await Open("/Rounds/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Use the dose as the rescue dose" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Group, new() { Name = "Rescue dose" }).Locator("p").First).ToHaveTextAsync("0.125 mg");
    }

    [Test]
    public async Task after_the_server_changes_a_saved_value_the_component_shows_the_value_the_server_left()
    {
        await Open("/Rounds/Round");
        await StepTheDoseUp();
        await StepTheDoseUp();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save the round" }).ClickAsync();

        await Expect(Dose().Locator("p").First).ToHaveTextAsync("0.25 mg");
    }

    [Test]
    public async Task after_the_server_changes_a_saved_text_the_text_box_shows_the_text_that_was_sent()
    {
        await Open("/Rounds/Round");
        await Page.GetByLabel("Note").FillAsync("  with food  ");
        await StepTheDoseUp();
        await StepTheDoseUp();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save the round" }).ClickAsync();

        await Expect(Page.GetByLabel("Note")).ToHaveValueAsync("  with food  ");
    }

    [Test]
    public async Task a_dose_stepped_under_a_culture_with_a_decimal_comma_posts_as_the_server_binds_it()
    {
        await Open("/Rounds/Round?culture=de-DE");
        await StepTheDoseUp();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save the round" }).ClickAsync();

        await Expect(Page.Locator("#saved-dose")).ToHaveTextAsync("0.25");
    }

    private ILocator Dose() => Page.GetByRole(AriaRole.Group, new() { Name = "Dose", Exact = true });

    private Task StepTheDoseUp() => Dose().GetByRole(AriaRole.Button, new() { Name = "Step the dose up" }).ClickAsync();
}
