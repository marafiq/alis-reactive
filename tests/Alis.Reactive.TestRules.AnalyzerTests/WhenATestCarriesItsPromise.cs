using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenATestCarriesItsPromise
{
    private const string TheWorldContract = "Alis.Reactive.Conformance.World.dll";
    private const string StillBeingWritten = "TestsAreStillBeingWritten";

    private const string AWorld = "using Alis.Reactive.Conformance.World;\n\n"
                                  + "namespace Alis.Reactive.Conformance.World\n{\n"
                                  + "    [AttributeUsage(AttributeTargets.Method)] public sealed class FailsForAttribute(string promise) : Attribute { public string Promise { get; } = promise; }\n"
                                  + "    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)] public sealed class HoldsAttribute(object gate) : Attribute { public object Gate { get; } = gate; }\n"
                                  + "    public enum RoundGate { Start, Finish }\n"
                                  + "    public static class Harness { public static void Arm(RoundGate gate) { } }\n"
                                  + "    public static class Promises\n    {\n"
                                  + "        public static class Rounds\n        {\n"
                                  + "            public const string a_round_starts = \"rounds start\";\n"
                                  + "            [Holds(RoundGate.Start)]\n"
                                  + "            public const string a_round_holds = \"rounds hold\";\n"
                                  + "        }\n    }\n}\n\n";

    private static Case<TEST012TestCarriesItsPromise> AProject(string tests)
        => new Case<TEST012TestCarriesItsPromise>().Without(TheWorldContract).WithFile("WhenARoundStarts.cs", AWorld + tests);

    [AcceptedCase]
    public Task a_test_that_carries_a_promise_constant_in_a_fixture_named_for_its_situation_is_accepted()
        => AProject(
                "public sealed class WhenARoundStarts\n{\n    [Test]\n    [FailsFor(Promises.Rounds.a_round_starts)]\n    public void the_round_is_listed() { }\n\n"
                + "    [FailsFor(Promises.Rounds.a_round_starts)]\n    private static void Listed() { }\n}\n"
                + "internal static class Rounds { internal static void Start() { } }\n")
            .WithTheBuildProperty(StillBeingWritten, "true")
            .RunAsync();

    [RefusedCase]
    public Task a_test_with_no_mark_is_refused_at_its_name()
        => AProject("public sealed class WhenARoundStarts\n{\n    [Test]\n    public void {|#0:the_round_is_listed|}() { }\n}\n")
            .WithTheBuildProperty(StillBeingWritten, "true")
            .Expecting(Report(0, "'the_round_is_listed' carries no [FailsFor]; a test names the one promise it proves"))
            .RunAsync();

    [RefusedCase]
    public Task a_promise_named_by_a_text_or_a_constant_of_the_tests_own_is_refused_at_the_value()
        => AProject(
                "public sealed class WhenARoundStarts\n{\n    private const string Mine = \"rounds start\";\n\n"
                + "    [Test]\n    [FailsFor({|#0:\"rounds start\"|})]\n    public void the_round_is_listed() { }\n\n"
                + "    [Test]\n    [FailsFor({|#1:Mine|})]\n    public void the_round_is_open() { }\n}\n")
            .WithTheBuildProperty(StillBeingWritten, "true")
            .Expecting(
                Report(0, "'the_round_is_listed' names its promise by a value of its own; [FailsFor] takes a constant of Promises, which the spec generates"),
                Report(1, "'the_round_is_open' names its promise by a value of its own; [FailsFor] takes a constant of Promises, which the spec generates"))
            .RunAsync();

    [RefusedCase]
    public Task a_fixture_not_named_when_and_its_situation_is_refused_at_its_name()
        => AProject(
                "public sealed class {|#0:RoundTests|}\n{\n    [Test]\n    [FailsFor(Promises.Rounds.a_round_starts)]\n    public void the_round_is_listed() { }\n}\n"
                + "public sealed class {|#1:Whenever|}\n{\n    [Test]\n    [FailsFor(Promises.Rounds.a_round_holds)]\n    public void the_round_holds() => Harness.Arm(RoundGate.Start);\n}\n")
            .Expecting(
                Report(0, "'RoundTests' holds a test and is not named When and its situation; name the fixture for the situation its tests share"),
                Report(1, "'Whenever' holds a test and is not named When and its situation; name the fixture for the situation its tests share"))
            .RunAsync();

    [RefusedCase]
    public Task a_test_that_never_arms_the_gate_its_promise_holds_is_refused_at_its_name()
        => AProject(
                "public sealed class WhenARoundHolds\n{\n    [Test]\n    [FailsFor(Promises.Rounds.a_round_holds)]\n"
                + "    public void {|#0:the_round_holds|}() => Harness.Arm(RoundGate.Finish);\n}\n")
            .WithTheBuildProperty(StillBeingWritten, "true")
            .Expecting(Report(0, "'the_round_holds' never arms RoundGate.Start, which its promise holds; arm the gate before the gesture and release it once it is held"))
            .RunAsync();

    [AcceptedCase]
    public Task a_test_that_arms_the_gate_its_promise_holds_inside_a_lambda_is_accepted()
        => AProject(
                "public sealed class WhenARoundHolds\n{\n    [Test]\n    [FailsFor(Promises.Rounds.a_round_holds)]\n    public void the_round_holds()\n    {\n"
                + "        Action arm = () => Harness.Arm(RoundGate.Start);\n        arm();\n    }\n}\n")
            .WithTheBuildProperty(StillBeingWritten, "true")
            .RunAsync();

    [RefusedCase]
    public Task a_promise_two_tests_take_is_refused_at_the_later_ones_mark()
        => AProject(
                "public sealed class WhenARoundStarts\n{\n    [Test]\n    [FailsFor(Promises.Rounds.a_round_starts)]\n    public void the_round_is_listed() { }\n\n"
                + "    [Test]\n    [{|#0:FailsFor(Promises.Rounds.a_round_starts)|}]\n    public void the_round_is_open() { }\n}\n")
            .WithTheBuildProperty(StillBeingWritten, "true")
            .Expecting(Report(0, "'the_round_is_open' proves Promises.Rounds.a_round_starts, which 'the_round_is_listed' proves already; a promise has one test"))
            .RunAsync();

    [RefusedCase]
    public Task a_promise_no_test_takes_is_refused_once_every_test_is_written()
        => AProject("public sealed class WhenARoundStarts\n{\n    [Test]\n    [FailsFor(Promises.Rounds.a_round_starts)]\n    public void the_round_is_listed() { }\n}\n")
            .WithTheBuildProperty(StillBeingWritten, "false")
            .Expecting(new DiagnosticResult(TEST012TestCarriesItsPromise.Id, DiagnosticSeverity.Error).WithMessage("No test carries Promises.Rounds.a_round_holds; every test of the spec is written from its promise"))
            .RunAsync();

    [RefusedCase]
    public Task a_promise_no_test_takes_is_refused_in_a_project_that_says_nothing_of_its_tests()
        => AProject("public sealed class WhenARoundStarts\n{\n    [Test]\n    [FailsFor(Promises.Rounds.a_round_starts)]\n    public void the_round_is_listed() { }\n}\n")
            .Expecting(new DiagnosticResult(TEST012TestCarriesItsPromise.Id, DiagnosticSeverity.Error).WithMessage("No test carries Promises.Rounds.a_round_holds; every test of the spec is written from its promise"))
            .RunAsync();

    [AcceptedCase]
    public Task every_promise_with_its_one_test_is_accepted_once_every_test_is_written()
        => AProject(
                "public sealed class WhenARoundStarts\n{\n    [Test]\n    [FailsFor(Promises.Rounds.a_round_starts)]\n    public void the_round_is_listed() { }\n\n"
                + "    [Test]\n    [FailsFor(Promises.Rounds.a_round_holds)]\n    public void the_round_holds() => Harness.Arm(RoundGate.Start);\n}\n")
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_world_contract_is_not_read()
        => new Case<TEST012TestCarriesItsPromise>()
            .Without(TheWorldContract)
            .WithFile("RoundTests.cs", "public sealed class RoundTests\n{\n    [Test]\n    public void the_round_is_listed() { }\n}\n")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => AProject(
                "public sealed class WhenARoundStarts\n{\n    [Test]\n    [FailsFor(Promises.Rounds.{|CS0117:no_such_promise|})]\n    public void the_round_is_listed() { }\n\n"
                + "    [Test]\n    [FailsFor({|CS0103:missing|})]\n    public void the_round_is_open() { }\n\n"
                + "    [Test]\n    [FailsFor({|CS1503:7|})]\n    public void the_round_is_closed() { }\n}\n")
            .WithTheBuildProperty(StillBeingWritten, "true")
            .RunAsync();

    private static DiagnosticResult Report(int at, string message)
        => new DiagnosticResult(TEST012TestCarriesItsPromise.Id, DiagnosticSeverity.Error).WithLocation(at).WithMessage(message);
}
