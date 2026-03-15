namespace Alis.Reactive.PlaywrightTests.Payload;

[TestFixture]
public class WhenPayloadPropertiesResolve : PlaywrightTestBase
{
    [Test]
    public async Task Int_property_renders_in_dom()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#int-value")).ToHaveTextAsync("42");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Long_property_renders_in_dom()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#long-value")).ToHaveTextAsync("9007199254740991");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Double_property_renders_in_dom()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#double-value")).ToHaveTextAsync("3.14159");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task String_property_renders_in_dom()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#string-value")).ToHaveTextAsync("hello world");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Bool_property_renders_in_dom()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#bool-value")).ToHaveTextAsync("true");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Nested_address_street_renders_in_dom()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#address-street")).ToHaveTextAsync("123 Main St");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Nested_address_city_renders_in_dom()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#address-city")).ToHaveTextAsync("Seattle");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Nested_address_zip_renders_in_dom()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#address-zip")).ToHaveTextAsync("98101");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Status_message_confirms_all_resolved()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#payload-status");
        await Expect(status).ToContainTextAsync("All payload properties resolved successfully");
        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Plan_json_contains_source_bindings()
    {
        await NavigateTo("/Sandbox/Payload");

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        // Source is now structured BindSource: { "kind": "event", "path": "..." }
        Assert.That(planJson, Does.Contain("\"path\": \"evt.intValue\""), "intValue source");
        Assert.That(planJson, Does.Contain("\"path\": \"evt.address.city\""), "nested source");
        Assert.That(planJson, Does.Contain("\"path\": \"evt.address.zip\""), "nested zip source");
    }
}
