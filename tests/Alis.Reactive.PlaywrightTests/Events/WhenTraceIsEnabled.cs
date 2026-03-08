namespace Alis.Reactive.PlaywrightTests.Events;

[TestFixture]
public class WhenTraceIsEnabled : PlaywrightTestBase
{
    [Test]
    public async Task BootModuleLoadsAndCompletesSuccessfully()
    {
        await NavigateTo("/Sandbox/Events");
        await WaitForTraceMessage("booted", 5000);

        AssertTraceContains("boot", "booting");
        AssertTraceContains("boot", "booted");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task TraceOutputAppearsInConsoleDuringBoot()
    {
        await NavigateTo("/Sandbox/Events");
        await WaitForTraceMessage("booted", 5000);

        // The Events page sets trace level to 'trace' before boot.
        // Verify that alis boot trace messages appeared in console.
        var hasBootTrace = _consoleMessages.Any(m => m.Contains("[alis:boot]"));
        Assert.That(hasBootTrace, Is.True, "Boot trace messages visible in console");
    }
}
