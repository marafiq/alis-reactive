namespace Alis.Reactive.PlaywrightTests.Events;

[TestFixture]
public class WhenTraceIsEnabled : PlaywrightTestBase
{
    [Test]
    public async Task Boot_module_loads_and_completes_successfully()
    {
        await NavigateTo("/Sandbox/Events");
        await WaitForTraceMessage("booted", 5000);

        AssertTraceContains("boot", "booting");
        AssertTraceContains("boot", "booted");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task Trace_output_appears_in_console_during_boot()
    {
        await NavigateTo("/Sandbox/Events");
        await WaitForTraceMessage("booted", 5000);

        // The Events page sets trace level to 'trace' before boot.
        // Verify that alis boot trace messages appeared in console.
        var hasBootTrace = _consoleMessages.Any(m => m.Contains("[alis:boot]"));
        Assert.That(hasBootTrace, Is.True, "Boot trace messages visible in console");
    }
}
