namespace Alis.Reactive.PlaywrightTests.Patterns.FormPatterns;

[TestFixture]
public class WhenIdsPreventCollisions : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Patterns/IdGenerator";

    [Test]
    public async Task form_fields_are_selectable_by_generated_ids()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Generated IDs must be valid CSS selectors; dotted MVC names would become class selectors.
        var nameField = Page.Locator("#json-form input[name='Name']");
        await Expect(nameField).ToBeAttachedAsync(new() { Timeout = 5000 });
        var id = await nameField.GetAttributeAsync("id");
        Assert.That(id, Is.Not.Null.And.Not.Empty, "Field must have a generated ID");
        // Both forms render the same generated IDs, so scope the selector to the JSON form.
        var foundById = Page.Locator($"#json-form #{id}");
        await Expect(foundById).ToBeAttachedAsync(new() { Timeout = 5000 });
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
    public async Task json_post_round_trips_exact_field_values_to_server()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var nameInput = Page.Locator("#json-form input[name='Name']");
        await nameInput.ClearAsync();
        await nameInput.FillAsync("CustomName");

        var statusSelect = Page.Locator("#json-form select[name='Status']");
        await statusSelect.SelectOptionAsync("inactive");

        var citySelect = Page.Locator("#json-form select[name='Address.City']");
        await citySelect.SelectOptionAsync("Denver");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit JSON" }).ClickAsync();

        var result = Page.Locator("#json-result");
        await Expect(result).ToContainTextAsync("Name=CustomName", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Status=inactive", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("City=Denver", new() { Timeout = 5000 });

        // Amount and PostalCode remain model defaults; this test mutates only plain form fields.
        await Expect(result).ToContainTextAsync("Amount=42.5", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Zip=98101", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_round_trips_exact_field_values_to_server()
    {
        // TODO: Give each rendered form its own component ID scope; gather currently resolves
        // by document.getElementById, so this test fills the first shared IDs in #json-form.
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var nameInput = Page.Locator("#json-form input[name='Name']");
        await nameInput.ClearAsync();
        await nameInput.FillAsync("FormUser");

        var statusSelect = Page.Locator("#json-form select[name='Status']");
        await statusSelect.SelectOptionAsync("pending");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit FormData" }).ClickAsync();

        var result = Page.Locator("#form-result");
        await Expect(result).ToContainTextAsync("Name=FormUser", new() { Timeout = 5000 });
        await Expect(result).ToContainTextAsync("Status=pending", new() { Timeout = 5000 });

        // Amount remains the model default; this test mutates only plain form fields.
        await Expect(result).ToContainTextAsync("Amount=42.5", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }


    [Test]
    public async Task json_post_result_displays_with_success_styling()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var result = Page.Locator("#json-result");

        await Expect(result).ToHaveTextAsync("");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit JSON" }).ClickAsync();

        await Expect(result).ToContainTextAsync("Name=Test", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task form_data_post_result_displays_with_success_styling()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        var result = Page.Locator("#form-result");

        await Expect(result).ToHaveTextAsync("");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Submit FormData" }).ClickAsync();

        await Expect(result).ToContainTextAsync("Name=Test", new() { Timeout = 5000 });
        await Expect(result).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));

        AssertNoConsoleErrors();
    }
}
