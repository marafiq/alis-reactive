namespace Alis.Reactive.PlaywrightTests.IdGenerator;

[TestFixture]
public class WhenUsingCollisionFreeIds : PlaywrightTestBase
{
    private const string Path = "/Sandbox/IdGenerator";

    [Test]
    public async Task page_loads_with_unique_ids()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page).ToHaveTitleAsync("IdGenerator Test — Alis.Reactive Sandbox");

        // TypeScope is displayed — should contain the model class name with underscores
        var scopeText = await Page.Locator("#id-prefix").TextContentAsync();
        Assert.That(scopeText, Does.Contain("IdGeneratorModel"));
        Assert.That(scopeText, Does.Not.Contain("."), "Scope should use underscores, not dots");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unique_ids_exist_in_dom()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var scope = await Page.Locator("#id-prefix").TextContentAsync();

        // Elements with the full-name scope prefix exist in the form
        var scopedElements = Page.Locator($"#json-form [id^='{scope}__']");
        var count = await scopedElements.CountAsync();
        Assert.That(count, Is.GreaterThan(0), "Form should contain elements with full-name scope prefix");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task json_post_receives_correct_field_values()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit JSON" }).ClickAsync();

        var result = Page.Locator("#json-result");
        await Expect(result).ToContainTextAsync("Name=Test", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Amount=42.5", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Status=active", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("City=Seattle", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Zip=98101", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_receives_correct_field_values()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit FormData" }).ClickAsync();

        var result = Page.Locator("#form-result");
        await Expect(result).ToContainTextAsync("Name=Test", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Amount=42.5", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Status=active", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task name_attributes_preserved_for_model_binding()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // name attributes are NOT prefixed — they match the model property path
        var statusSelect = Page.Locator("#json-form select[name='Status']");
        await Expect(statusSelect).ToBeAttachedAsync(new() { Timeout = 5000 });

        var citySelect = Page.Locator("#json-form select[name='Address.City']");
        await Expect(citySelect).ToBeAttachedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task id_contains_double_underscore_delimiter()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // The native dropdown for Status should have id containing "__Status"
        var statusSelect = Page.Locator("#json-form select[name='Status']");
        var id = await statusSelect.GetAttributeAsync("id");
        Assert.That(id, Does.Contain("__Status"), "ID should contain '__' delimiter before property path");

        // The native dropdown for Address.City should have id containing "__Address_City"
        var citySelect = Page.Locator("#json-form select[name='Address.City']");
        var cityId = await citySelect.GetAttributeAsync("id");
        Assert.That(cityId, Does.Contain("__Address_City"), "Nested ID should use '__' delimiter and underscores for nesting");

        AssertNoConsoleErrors();
    }
}
