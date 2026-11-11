using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenATestWaitsOnTime
{
    private const string Opening = "public sealed class WhenARoundStarts\n{\n    private readonly IPage _page = null!;\n\n    [Test]\n    public async Task the_round_is_listed()\n    {\n";
    private const string Closing = "\n    }\n}\n";

    [RefusedCase]
    public Task playwrights_wait_on_time_is_refused_at_its_name()
        => new Case<TEST003TestWaitsForNoTime>()
            .WithFile("WhenARoundStarts.cs", Opening + "        await _page.{|#0:WaitForTimeoutAsync|}(500);" + Closing)
            .Expecting(Waits(0, "IPage.WaitForTimeoutAsync"))
            .RunAsync();

    [AcceptedCase]
    public Task a_navigation_the_test_awaits_is_accepted()
        => new Case<TEST003TestWaitsForNoTime>()
            .WithFile("WhenARoundStarts.cs", Opening + "        await _page.GotoAsync(\"/rounds\");" + Closing)
            .RunAsync();

    [RefusedCase]
    public Task a_delay_a_sleep_and_a_spin_are_refused_each_at_its_name()
        => new Case<TEST003TestWaitsForNoTime>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        await Task.{|#0:Delay|}(100);\n"
                + "        await Task.{|#1:Delay|}(TimeSpan.FromSeconds(1), CancellationToken.None);\n"
                + "        Thread.{|#2:Sleep|}(100);\n"
                + "        Thread.{|#3:SpinWait|}(10);\n"
                + "        new SpinWait().{|#4:SpinOnce|}();\n"
                + "        SpinWait.{|#5:SpinUntil|}(() => true);"
                + Closing)
            .Expecting(Waits(0, "Task.Delay"), Waits(1, "Task.Delay"), Waits(2, "Thread.Sleep"), Waits(3, "Thread.SpinWait"), Waits(4, "SpinWait.SpinOnce"), Waits(5, "SpinWait.SpinUntil"))
            .RunAsync();

    [AcceptedCase]
    public Task a_task_that_waits_on_no_time_and_a_delay_of_another_type_are_accepted()
        => new Case<TEST003TestWaitsForNoTime>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        await Task.WhenAll(Task.CompletedTask, Task.Yield().GetAwaiter().IsCompleted ? Task.CompletedTask : Task.CompletedTask);\n"
                + "        Rounds.Delay(100);\n"
                + "        Rounds.Sleep(100);"
                + Closing
                + "internal static class Rounds { internal static void Delay(int minutes) { } internal static void Sleep(int minutes) { } }\n")
            .RunAsync();

    [RefusedCase]
    public Task a_wait_in_a_helper_and_in_the_harness_is_refused_too()
        => new Case<TEST003TestWaitsForNoTime>()
            .WithFile("Rounds.cs", "internal static class Rounds { internal static Task Settle() => Task.{|#0:Delay|}(250); }")
            .WithFile("Harness/Hosts.cs", "internal static class Hosts { internal static void Settle() => Thread.{|#1:Sleep|}(250); }")
            .Expecting(Waits(0, "Task.Delay"), Waits(1, "Thread.Sleep"))
            .RunAsync();

    [RefusedCase]
    public Task a_timeout_a_test_sets_on_a_playwright_call_is_refused_at_the_property()
        => new Case<TEST003TestWaitsForNoTime>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync(\"Rounds\", new() { {|#0:Timeout|} = 3000 });\n"
                + "        var options = new LocatorClickOptions();\n"
                + "        options.{|#1:Timeout|} = 1000;\n"
                + "        await _page.GetByRole(AriaRole.Button).ClickAsync(options);"
                + Closing)
            .Expecting(ChosenWait(0, "LocatorAssertionsToHaveTextOptions"), ChosenWait(1, "LocatorClickOptions"))
            .RunAsync();

    [AcceptedCase]
    public Task a_timeout_the_harness_sets_and_a_timeout_of_another_type_are_accepted()
        => new Case<TEST003TestWaitsForNoTime>()
            .WithFile("Harness/TestWorld.cs", "internal static class TestWorld { internal static Task Open(IPage page) => page.GotoAsync(\"/\", new() { Timeout = 30000 }); }")
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        await _page.GetByRole(AriaRole.Button).ClickAsync(new() { Button = MouseButton.Middle });\n"
                + "        _ = new Budget { Timeout = 3 };"
                + Closing
                + "internal sealed class Budget { internal int Timeout { get; set; } }\n")
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_playwright_still_refuses_a_delay()
        => new Case<TEST003TestWaitsForNoTime>()
            .Without("Microsoft.Playwright.dll")
            .WithFile("Rounds.cs", "internal static class Rounds { internal static Task Start() => Task.CompletedTask; }")
            .RunAsync();

    [RefusedCase]
    public Task a_delay_in_a_project_with_no_playwright_is_refused()
        => new Case<TEST003TestWaitsForNoTime>()
            .Without("Microsoft.Playwright.dll")
            .WithFile("Rounds.cs", "internal static class Rounds { internal static Task Settle() => Task.{|#0:Delay|}(250); }")
            .Expecting(Waits(0, "Task.Delay"))
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST003TestWaitsForNoTime>()
            .WithFile("WhenARoundStarts.cs", Opening + "        await _page.{|CS1061:WaitForNothingAsync|}(500);" + Closing)
            .RunAsync();

    private static DiagnosticResult Waits(int at, string member)
        => new DiagnosticResult(TEST003TestWaitsForNoTime.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{member}' waits on time; wait for the state the promise names");

    private static DiagnosticResult ChosenWait(int at, string options)
        => new DiagnosticResult(TEST003TestWaitsForNoTime.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{options}' sets its own Timeout; every wait is the harness's, set from measurement, so remove it");
}
