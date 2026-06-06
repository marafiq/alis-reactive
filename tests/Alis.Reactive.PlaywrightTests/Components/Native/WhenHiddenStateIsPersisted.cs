namespace Alis.Reactive.PlaywrightTests.Components.Native;

[TestFixture]
public class WhenHiddenStateIsPersisted : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeHiddenField";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_NativeHiddenFieldModel__";

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
    }

    [Test]
    public async Task page_loads_without_errors()
    {
        await NavigateAndBoot();
        await Expect(Page).ToHaveTitleAsync("NativeHiddenField — Alis.Reactive Sandbox");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_resident_id_has_seeded_value()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{ModelIdPrefix}ResidentId");
        await Expect(input).ToHaveValueAsync("RES-1042");
        await Expect(input).ToHaveAttributeAsync("type", "hidden");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task hidden_form_token_has_seeded_value()
    {
        await NavigateAndBoot();

        var input = Page.Locator($"#{ModelIdPrefix}FormToken");
        await Expect(input).ToHaveValueAsync("abc123");
        await Expect(input).ToHaveAttributeAsync("type", "hidden");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_resident_id_into_echo()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#resident-id-echo");
        await Expect(echo).ToHaveTextAsync("RES-1042", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task domready_reads_form_token_into_echo()
    {
        await NavigateAndBoot();

        var echo = Page.Locator("#form-token-echo");
        await Expect(echo).ToHaveTextAsync("abc123", new() { Timeout = 3000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task post_gather_includes_hidden_fields()
    {
        await NavigateAndBoot();

        var nameInput = Page.Locator($"#{ModelIdPrefix}ResidentName");
        await nameInput.FillAsync("Margaret Thompson");

        await Page.Locator("#submit-btn").ClickAsync();

        await Expect(Page.Locator("#echo-resident-id")).ToHaveTextAsync("RES-1042", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-form-token")).ToHaveTextAsync("abc123");
        await Expect(Page.Locator("#echo-resident-name")).ToHaveTextAsync("Margaret Thompson");
        await Expect(Page.Locator("#echo-field-count")).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task post_gather_sends_hidden_fields_even_without_visible_input()
    {
        await NavigateAndBoot();

        await Page.Locator("#submit-btn").ClickAsync();

        await Expect(Page.Locator("#echo-resident-id")).ToHaveTextAsync("RES-1042", new() { Timeout = 5000 });
        await Expect(Page.Locator("#echo-form-token")).ToHaveTextAsync("abc123");
        await Expect(Page.Locator("#echo-field-count")).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }
}
