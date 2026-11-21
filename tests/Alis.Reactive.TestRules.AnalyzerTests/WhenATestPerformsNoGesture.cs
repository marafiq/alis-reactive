using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenATestPerformsNoGesture
{
    private const string Opening = "public sealed class WhenARoundStarts\n{\n    private readonly IPage _page = null!;\n\n";
    private const string HeldGate = "public sealed class HeldGate\n{\n    [Gesture] public Task Release() => Task.CompletedTask;\n\n"
                                    + "    [FailsFor(\"harness WhenARoundStarts.the_world_is_seeded\")] public Task Seed() => Task.CompletedTask;\n}\n";

    [RefusedCase]
    public Task a_test_that_only_finds_and_expects_is_refused_at_its_name()
        => new Case<TEST013TestPerformsItsGesture>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "    [Test]\n    public async Task {|#0:the_round_is_listed|}()\n    {\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync(\"Rounds\");\n    }\n}\n")
            .Expecting(NoGesture(0, "the_round_is_listed"))
            .RunAsync();

    [AcceptedCase]
    public Task a_gesture_of_a_page_a_locator_the_keyboard_or_the_mouse_is_accepted()
        => new Case<TEST013TestPerformsItsGesture>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "    [Test] public Task the_round_is_opened() => _page.GotoAsync(\"/rounds\");\n\n"
                + "    [Test] public Task the_round_is_started() => _page.GetByRole(AriaRole.Button).ClickAsync();\n\n"
                + "    [Test] public Task the_round_is_confirmed() => _page.Keyboard.PressAsync(\"Enter\");\n\n"
                + "    [Test] public Task the_round_is_pointed_at() => _page.Mouse.MoveAsync(4, 4);\n}\n")
            .RunAsync();

    [RefusedCase]
    public Task a_gesture_a_helper_performs_counts_as_none()
        => new Case<TEST013TestPerformsItsGesture>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "    [Test]\n    public async Task {|#0:the_round_is_listed|}()\n    {\n"
                + "        await Open();\n"
                + "        await Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync(\"Rounds\");\n    }\n\n"
                + "    private Task Open() => _page.GotoAsync(\"/rounds\");\n}\n")
            .Expecting(NoGesture(0, "the_round_is_listed"))
            .RunAsync();

    [AcceptedCase]
    public Task a_gesture_in_a_lambda_or_a_local_function_of_the_test_is_accepted()
        => new Case<TEST013TestPerformsItsGesture>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "    [Test]\n    public async Task the_round_is_sent()\n    {\n"
                + "        var sent = await _page.RunAndWaitForRequestAsync(() => _page.GetByRole(AriaRole.Button).ClickAsync(), \"**/rounds\");\n"
                + "        Assert.That(sent.Method, Is.EqualTo(\"POST\"));\n    }\n\n"
                + "    [Test]\n    public async Task the_round_is_typed()\n    {\n"
                + "        Task Type() => _page.GetByLabel(\"Ward\").FillAsync(\"North\");\n"
                + "        await Type();\n    }\n}\n")
            .RunAsync();

    [RefusedCase]
    public Task a_member_of_playwright_outside_the_gestures_counts_as_none()
        => new Case<TEST013TestPerformsItsGesture>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "    [Test] public Task {|#0:the_round_is_clicked_by_a_selector|}() => _page.ClickAsync(\"#start\");\n\n"
                + "    [Test] public Task {|#1:the_round_is_focused|}() => _page.GetByRole(AriaRole.Button).FocusAsync();\n}\n")
            .Expecting(NoGesture(0, "the_round_is_clicked_by_a_selector"), NoGesture(1, "the_round_is_focused"))
            .RunAsync();

    [RefusedCase]
    public Task a_member_named_like_a_gesture_of_another_type_counts_as_none()
        => new Case<TEST013TestPerformsItsGesture>()
            .WithFile(
                "WhenARoundStarts.cs",
                "public sealed class WhenARoundStarts\n{\n    private readonly Wards.IMouse _mouse = null!;\n\n"
                + "    [Test] public Task {|#0:the_round_is_pointed_at|}() => _mouse.MoveAsync(4, 4);\n}\n"
                + "namespace Wards { public interface IMouse { Task MoveAsync(int x, int y); } }\n")
            .Expecting(NoGesture(0, "the_round_is_pointed_at"))
            .RunAsync();

    [AcceptedCase]
    public Task a_call_of_a_member_that_carries_the_gesture_mark_is_accepted()
        => new Case<TEST013TestPerformsItsGesture>()
            .WithFile(
                "WhenARoundStarts.cs",
                "public sealed class WhenARoundStarts\n{\n    private readonly HeldGate _held = new();\n\n"
                + "    [Test] public Task the_held_answer_is_released() => _held.Release();\n}\n"
                + HeldGate)
            .RunAsync();

    [RefusedCase]
    public Task a_call_of_a_harness_member_that_carries_another_mark_counts_as_none()
        => new Case<TEST013TestPerformsItsGesture>()
            .WithFile(
                "WhenARoundStarts.cs",
                "public sealed class WhenARoundStarts\n{\n    private readonly HeldGate _held = new();\n\n"
                + "    [Test] public Task {|#0:the_world_is_seeded|}() => _held.Seed();\n}\n"
                + HeldGate)
            .Expecting(NoGesture(0, "the_world_is_seeded"))
            .RunAsync();

    [AcceptedCase]
    public Task a_method_that_is_no_test_needs_no_gesture()
        => new Case<TEST013TestPerformsItsGesture>()
            .WithFile(
                "Rounds.cs",
                "internal static class Rounds\n{\n    internal static ILocator Rows(IPage page) => page.GetByRole(AriaRole.Listitem);\n}\n")
            .RunAsync();

    [RefusedCase]
    public Task a_test_with_no_gesture_is_refused_in_a_project_with_no_world_contract()
        => new Case<TEST013TestPerformsItsGesture>()
            .Without("Alis.Reactive.Conformance.World.dll")
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "    [Test] public Task the_round_is_opened() => _page.GotoAsync(\"/rounds\");\n\n"
                + "    [Test] public Task {|#0:the_round_is_listed|}() => Expect(_page.GetByRole(AriaRole.Heading)).ToHaveTextAsync(\"Rounds\");\n}\n")
            .Expecting(NoGesture(0, "the_round_is_listed"))
            .RunAsync();

    [AcceptedCase]
    public Task the_gesture_mark_counts_in_a_project_with_no_playwright()
        => new Case<TEST013TestPerformsItsGesture>()
            .Without("Microsoft.Playwright.dll")
            .WithFile(
                "WhenARoundStarts.cs",
                "public sealed class WhenARoundStarts\n{\n    private readonly HeldGate _held = new();\n\n"
                + "    [Test] public Task the_held_answer_is_released() => _held.Release();\n}\n"
                + HeldGate)
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_nunit_is_not_read()
        => new Case<TEST013TestPerformsItsGesture>()
            .Without("nunit.framework.dll")
            .WithFile("Rounds.cs", "internal static class Rounds { internal static ILocator Rows(IPage page) => page.GetByRole(AriaRole.Listitem); }\n")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST013TestPerformsItsGesture>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening + "    [Test] public Task the_round_is_opened() => _page.{|CS1061:GoAsync|}(\"/rounds\");\n}\n")
            .RunAsync();

    private static DiagnosticResult NoGesture(int at, string test)
        => new DiagnosticResult(TEST013TestPerformsItsGesture.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{test}' performs no gesture itself; a test makes the gesture its promise names in its own body, so write it there");
}
