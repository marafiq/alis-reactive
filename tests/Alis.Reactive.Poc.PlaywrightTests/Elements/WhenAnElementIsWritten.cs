namespace Alis.Reactive.Poc.PlaywrightTests.Elements;

[TestFixture]
public sealed class WhenAnElementIsWritten : AppPageTest
{
    [Test]
    public async Task a_class_name_is_added()
    {
        await Open("/Pumps/PumpCard");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Mark the pump as alarmed" }).ClickAsync();

        await Expect(Page.Locator("#pump-card")).ToHaveAttributeAsync("class", "pump-card alarmed");
    }

    [Test]
    public async Task a_class_name_that_reads_nothing_changes_no_class()
    {
        await Open("/Pumps/PumpCard");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Mark the pump by its last alarm" }).ClickAsync();

        await Expect(Page.Locator("#alarm")).ToHaveTextAsync("marked");
        await Expect(Page.Locator("#pump-card")).ToHaveAttributeAsync("class", "pump-card");
    }

    [Test]
    public async Task a_class_name_is_removed()
    {
        await Open("/Pumps/PumpCard");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Mark the pump as alarmed" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Clear the mark" }).ClickAsync();

        await Expect(Page.Locator("#pump-card")).ToHaveAttributeAsync("class", "pump-card");
    }

    [Test]
    public async Task a_class_name_absent_is_toggled_on()
    {
        await Open("/Pumps/PumpCard");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Flip the mark" }).ClickAsync();

        await Expect(Page.Locator("#pump-card")).ToHaveAttributeAsync("class", "pump-card alarmed");
    }

    [Test]
    public async Task a_class_name_present_is_toggled_off()
    {
        await Open("/Pumps/PumpCard");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Mark the pump as alarmed" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Flip the mark" }).ClickAsync();

        await Expect(Page.Locator("#pump-card")).ToHaveAttributeAsync("class", "pump-card");
    }

    [Test]
    public async Task html_set_on_an_element_shows_its_markup()
    {
        await Open("/Pumps/PumpCard");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the alarm in bold" }).ClickAsync();

        await Expect(Page.Locator("#alarm strong")).ToHaveTextAsync("occlusion");
    }

    [Test]
    public async Task html_set_from_a_value_that_reads_nothing_shows_nothing()
    {
        await Open("/Pumps/PumpCard");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the last alarm" }).ClickAsync();

        await Expect(Page.Locator("#alarm")).ToHaveTextAsync("");
    }
}
