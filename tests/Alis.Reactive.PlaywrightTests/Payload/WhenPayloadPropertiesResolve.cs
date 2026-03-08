namespace Alis.Reactive.PlaywrightTests.Payload;

[TestFixture]
public class WhenPayloadPropertiesResolve : PlaywrightTestBase
{
    [Test]
    public async Task IntProperty_renders_in_DOM()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#int-value")).ToHaveTextAsync("42");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task LongProperty_renders_in_DOM()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#long-value")).ToHaveTextAsync("9007199254740991");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task DoubleProperty_renders_in_DOM()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#double-value")).ToHaveTextAsync("3.14159");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task StringProperty_renders_in_DOM()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#string-value")).ToHaveTextAsync("hello world");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task BoolProperty_renders_in_DOM()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#bool-value")).ToHaveTextAsync("true");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task NestedAddress_Street_renders_in_DOM()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#address-street")).ToHaveTextAsync("123 Main St");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task NestedAddress_City_renders_in_DOM()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#address-city")).ToHaveTextAsync("Seattle");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task NestedAddress_Zip_renders_in_DOM()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        await Expect(Page.Locator("#address-zip")).ToHaveTextAsync("98101");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task StatusMessage_confirms_all_resolved()
    {
        await NavigateTo("/Sandbox/Payload");
        await WaitForTraceMessage("booted", 5000);

        var status = Page.Locator("#payload-status");
        await Expect(status).ToContainTextAsync("All payload properties resolved successfully");
        await Expect(status).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("text-green-600"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task PlanJson_contains_source_bindings()
    {
        await NavigateTo("/Sandbox/Payload");

        var planJson = await Page.Locator("#plan-json").TextContentAsync();
        Assert.That(planJson, Does.Contain("\"source\": \"evt.intValue\""), "intValue source");
        Assert.That(planJson, Does.Contain("\"source\": \"evt.address.city\""), "nested source");
        Assert.That(planJson, Does.Contain("\"source\": \"evt.address.zip\""), "nested zip source");
    }
}
