namespace Alis.Reactive.Poc.PlaywrightTests.Inputs;

[TestFixture]
public sealed class WhenANameIsTyped : AppPageTest
{
    [Test]
    public async Task a_text_box_runs_its_input_handler_when_the_person_types()
    {
        await Open("/Admissions/ResidentName");

        await Page.GetByLabel("Resident name").FillAsync("Ada Byron");

        await Expect(Page.Locator("#door-card")).ToHaveTextAsync("Ada Byron");
    }

    [Test]
    public async Task an_emptied_name_shows_that_a_resident_needs_one()
    {
        await Open("/Admissions/ResidentName");

        await Page.GetByLabel("Resident name").FillAsync("");

        await Expect(Page.Locator("#name-hint")).ToHaveTextAsync("A resident needs a name");
    }

    [Test]
    public async Task a_name_typed_after_an_empty_box_shows_that_a_name_is_given()
    {
        await Open("/Admissions/ResidentName");
        await Page.GetByLabel("Resident name").FillAsync("");
        await Expect(Page.Locator("#name-hint")).ToHaveTextAsync("A resident needs a name");

        await Page.GetByLabel("Resident name").FillAsync("Ada Byron");

        await Expect(Page.Locator("#name-hint")).ToHaveTextAsync("Name given");
    }

    [Test]
    public async Task leaving_a_changed_text_box_runs_its_change_handler_with_the_events_payload()
    {
        await Open("/Admissions/ResidentName");
        await Page.GetByLabel("Resident name").FillAsync("Ada Byron");

        await Page.GetByLabel("Resident name").PressAsync("Tab");

        await Expect(Page.Locator("#last-commit")).ToHaveTextAsync("change");
        await Expect(Page.Locator("#committed-by")).ToHaveTextAsync("a person");
    }

    [Test]
    public async Task leaving_an_unchanged_text_box_runs_no_change_handler()
    {
        await Open("/Admissions/ResidentName");
        await Page.GetByLabel("Resident name").FocusAsync();

        await Page.GetByLabel("Resident name").PressAsync("Tab");

        await Expect(Page.Locator("#last-left")).ToHaveTextAsync("blur");
        await Expect(Page.Locator("#last-commit")).ToHaveTextAsync("none yet");
    }

    [Test]
    public async Task leaving_a_text_box_runs_its_blur_handler_with_the_events_payload()
    {
        await Open("/Admissions/ResidentName");
        await Page.GetByLabel("Resident name").FocusAsync();

        await Page.GetByLabel("Resident name").PressAsync("Tab");

        await Expect(Page.Locator("#last-left")).ToHaveTextAsync("blur");
    }

    [Test]
    public async Task a_pipeline_writes_a_text_box_by_its_member()
    {
        await Open("/Admissions/ResidentName");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Clear the name" }).ClickAsync();

        await Expect(Page.GetByLabel("Resident name")).ToHaveValueAsync("");
    }

    [Test]
    public async Task a_pipeline_writes_the_value_the_view_rendered_into_a_text_box()
    {
        await Open("/Admissions/ResidentName");
        await Page.GetByLabel("Resident name").FillAsync("Ada Byron");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Restore the name on record" }).ClickAsync();

        await Expect(Page.GetByLabel("Resident name")).ToHaveValueAsync("Helen Park");
    }

    [Test]
    public async Task a_pipeline_writes_one_text_box_from_another()
    {
        await Open("/Admissions/ResidentName");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Copy the resident's name" }).ClickAsync();

        await Expect(Page.GetByLabel("Next of kin")).ToHaveValueAsync("Helen Park");
    }
}
