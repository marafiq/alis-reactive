using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenATestIsSkippedOrRepeated
{
    private const string Opening = "public sealed class WhenARoundStarts\n{\n    [Test]\n    public async Task the_round_is_listed()\n    {\n";
    private const string Closing = "\n        await Task.CompletedTask;\n    }\n}\n";
    private const string ARunsAction =
        "public sealed class AroundTheRunAttribute : Attribute, ITestAction\n{\n    public ActionTargets Targets => ActionTargets.Suite;\n"
        + "    public void BeforeTest(NUnit.Framework.Interfaces.ITest test) { }\n    public void AfterTest(NUnit.Framework.Interfaces.ITest test) { }\n}\n";

    [RefusedCase]
    public Task a_skipped_a_repeated_an_ordered_and_a_parameterized_test_are_refused_each_at_its_attribute()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                "public sealed class WhenARoundStarts\n{\n"
                + "    [Test, {|#0:Ignore|}(\"later\")] public void the_round_is_listed() { }\n"
                + "    [Test] [{|#1:Explicit|}] public void the_round_is_opened() { }\n"
                + "    [Test] [{|#2:Retry|}(3)] public void the_round_is_closed() { }\n"
                + "    [Test] [{|#3:Repeat|}(2)] public void the_round_is_signed() { }\n"
                + "    [Test] [{|#4:Order|}(1)] public void the_round_is_first() { }\n"
                + "    [{|#5:TestCase|}(1)] public void the_round_has_a_nurse(int nurses) { }\n"
                + "    [Test] public void the_round_has_a_ward([{|#6:Values|}(1, 2)] int ward) { }\n"
                + "}\n")
            .Expecting(
                NoAttribute(0, "Ignore"), NoAttribute(1, "Explicit"), NoAttribute(2, "Retry"), NoAttribute(3, "Repeat"), NoAttribute(4, "Order"),
                NoAttribute(5, "TestCase"), NoAttribute(6, "Values"))
            .RunAsync();

    [AcceptedCase]
    public Task the_three_attributes_a_test_takes_are_accepted()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                "[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]\n[Category(\"net10\")]\npublic sealed class WhenARoundStarts(string host)\n{\n"
                + "    [Test] public void the_round_is_listed() => Assert.That(host, Is.EqualTo(\"net10\"));\n}\n"
                + "public static class Hosts { public static readonly string[] Net10 = [\"net10\"]; }\n")
            .RunAsync();

    [RefusedCase]
    public Task an_attribute_of_nunit_on_a_fixture_an_assembly_or_by_its_full_name_is_refused()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                "using N = NUnit.Framework;\n[assembly: {|#0:LevelOfParallelism|}(2)]\n"
                + "[{|#1:TestFixture|}]\n[{|#2:NonParallelizable|}]\n[{|#3:Description|}(\"rounds\")]\npublic sealed class WhenARoundStarts\n{\n"
                + "    [Test] [NUnit.Framework.{|#4:Ignore|}(\"later\")] public void the_round_is_listed() { }\n"
                + "    [Test] [{|#5:IgnoreAttribute|}(\"later\")] public void the_round_is_opened() { }\n"
                + "    [Test] [N::{|#6:Ignore|}(\"later\")] public void the_round_is_closed() { }\n}\n")
            .Expecting(
                NoAttribute(0, "LevelOfParallelism"), NoAttribute(1, "TestFixture"), NoAttribute(2, "NonParallelizable"), NoAttribute(3, "Description"),
                NoAttribute(4, "Ignore"), NoAttribute(5, "IgnoreAttribute"), NoAttribute(6, "Ignore"))
            .RunAsync();

    [AcceptedCase]
    public Task an_attribute_that_is_not_nunits_is_accepted()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                "[Nightly]\npublic sealed class WhenARoundStarts\n{\n"
                + "    [Test] [FailsFor(\"harness WhenARoundStarts.the_round_is_listed\")] [Obsolete(\"moved\")] public void the_round_is_listed() { }\n}\n"
                + "public sealed class NightlyAttribute : Attribute, IComparable\n{\n    public int CompareTo(object? other) => 0;\n}\n")
            .RunAsync();

    [RefusedCase]
    public Task an_attribute_a_project_derives_from_nunits_or_builds_on_its_interfaces_is_refused()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                "[{|#0:Nightly|}]\npublic sealed class WhenARoundStarts\n{\n"
                + "    [Test] [{|#1:Later|}] public void the_round_is_listed() { }\n"
                + "    [Test] [{|#2:Deeper|}] public void the_round_is_opened() { }\n"
                + "    [Test] [{|#3:Again|}] public void the_round_is_closed() { }\n"
                + "    [{|#4:Before|}] public void Open() { }\n}\n"
                + "public sealed class NightlyAttribute() : CategoryAttribute(\"nightly\");\n"
                + "public class LaterAttribute() : IgnoreAttribute(\"later\");\n"
                + "public sealed class DeeperAttribute : LaterAttribute;\n"
                + "public sealed class BeforeAttribute : SetUpAttribute;\n"
                + "public sealed class AgainAttribute : Attribute, NUnit.Framework.Interfaces.IApplyToTest\n{\n"
                + "    public void ApplyToTest(NUnit.Framework.Internal.Test test) { }\n}\n")
            .Expecting(NoAttribute(0, "Nightly"), NoAttribute(1, "Later"), NoAttribute(2, "Deeper"), NoAttribute(3, "Again"), NoAttribute(4, "Before"))
            .RunAsync();

    [RefusedCase]
    public Task a_legacy_assertion_of_nunits_is_refused_at_its_name()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                "using NUnit.Framework.Legacy;\n"
                + Opening
                + "        ClassicAssert.{|#0:AreEqual|}(4, 4);\n"
                + "        StringAssert.{|#1:Contains|}(\"North\", \"North wing\");\n"
                + "        CollectionAssert.{|#2:IsEmpty|}(Array.Empty<string>());\n"
                + "        Action<bool> check = ClassicAssert.{|#3:IsTrue|};\n"
                + "        check(true);"
                + Closing)
            .Expecting(
                NoAssertion(0, "ClassicAssert.AreEqual"), NoAssertion(1, "StringAssert.Contains"), NoAssertion(2, "CollectionAssert.IsEmpty"),
                NoAssertion(3, "ClassicAssert.IsTrue"))
            .RunAsync();

    [RefusedCase]
    public Task a_member_of_assert_is_refused_in_a_project_without_nunits_legacy_assertions()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .Without("nunit.framework.legacy.dll")
            .WithFile("WhenARoundStarts.cs", Opening + "        Assert.{|#0:Pass|}();\n        Rounds.Ignore(\"later\");" + Closing + "internal static class Rounds { internal static void Ignore(string why) { } }\n")
            .Expecting(NoAssertion(0, "Assert.Pass"))
            .RunAsync();

    [RefusedCase]
    public Task a_set_up_attribute_outside_the_harness_is_refused_at_its_name()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                "[{|#0:Parallelizable|}]\npublic sealed class WhenARoundStarts\n{\n"
                + "    [{|#1:SetUp|}] public void Open() { }\n"
                + "    [{|#2:TearDown|}] public void Close() { }\n"
                + "    [{|#3:OneTimeSetUp|}] public void Start() { }\n"
                + "    [{|#4:OneTimeTearDown|}] public void Stop() { }\n"
                + "    [Test] public void the_round_is_listed() { }\n}\n"
                + "[{|#5:SetUpFixture|}]\npublic sealed class Run;\n")
            .Expecting(
                SetUp(0, "Parallelizable"), SetUp(1, "SetUp"), SetUp(2, "TearDown"), SetUp(3, "OneTimeSetUp"), SetUp(4, "OneTimeTearDown"), SetUp(5, "SetUpFixture"))
            .RunAsync();

    [AcceptedCase]
    public Task the_set_up_attributes_in_the_harness_are_accepted()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "Harness/Run.cs",
                "[SetUpFixture]\npublic sealed class Run\n{\n    [OneTimeSetUp] public void Start() { }\n    [OneTimeTearDown] public void Stop() { }\n}\n"
                + "[Parallelizable]\npublic abstract class ConformanceFixture\n{\n    [SetUp] public void Open() { }\n    [TearDown] public void Close() { }\n}\n")
            .RunAsync();

    [RefusedCase]
    public Task a_test_action_applied_outside_the_harness_is_refused_at_its_name()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                "[assembly: {|#0:AroundTheRun|}]\npublic sealed class WhenARoundStarts\n{\n    [Test] public void the_round_is_listed() { }\n}\n" + ARunsAction)
            .Expecting(SetUp(0, "AroundTheRun"))
            .RunAsync();

    [AcceptedCase]
    public Task a_test_action_applied_in_the_harness_is_accepted()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile("Harness/Run.cs", "[assembly: AroundTheRun]\n" + ARunsAction)
            .RunAsync();

    [RefusedCase]
    public Task an_attribute_no_test_takes_is_refused_in_the_harness_too()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "Harness/WhenTwoWorldsRun.cs",
                "public sealed class WhenTwoWorldsRun\n{\n    [Test] [{|#0:Retry|}(2)] public void each_world_lists_its_own() { }\n}\n")
            .Expecting(NoAttribute(0, "Retry"))
            .RunAsync();

    [RefusedCase]
    public Task a_member_of_assert_assume_or_warn_outside_assert_that_is_refused_at_its_name()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        Assert.{|#0:Ignore|}(\"later\");\n"
                + "        Assert.{|#1:Inconclusive|}(\"unknown\");\n"
                + "        Assert.{|#2:Pass|}();\n"
                + "        Assert.{|#3:Fail|}(\"never\");\n"
                + "        Assert.{|#4:Multiple|}(() => { });\n"
                + "        await Assert.{|#5:ThatAsync|}(() => Task.CompletedTask, Throws.Nothing);\n"
                + "        Assume.{|#6:That|}(true, Is.True);\n"
                + "        Warn.{|#7:Unless|}(true);\n"
                + "        Action skip = Assert.{|#8:Pass|};\n"
                + "        skip();"
                + Closing)
            .Expecting(
                NoAssertion(0, "Assert.Ignore"), NoAssertion(1, "Assert.Inconclusive"), NoAssertion(2, "Assert.Pass"), NoAssertion(3, "Assert.Fail"),
                NoAssertion(4, "Assert.Multiple"), NoAssertion(5, "Assert.ThatAsync"), NoAssertion(6, "Assume.That"), NoAssertion(7, "Warn.Unless"),
                NoAssertion(8, "Assert.Pass"))
            .RunAsync();

    [RefusedCase]
    public Task a_member_named_with_no_type_before_it_is_refused_at_its_name()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                "using static NUnit.Framework.Assert;\n"
                + Opening
                + "        {|#0:Pass|}();\n"
                + "        Action skip = {|#1:Inconclusive|};\n"
                + "        skip();"
                + Closing)
            .Expecting(NoAssertion(0, "Assert.Pass"), NoAssertion(1, "Assert.Inconclusive"))
            .RunAsync();

    [AcceptedCase]
    public Task assert_that_and_a_member_named_like_nunits_of_another_type_are_accepted()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        Assert.That(\"North\", Is.EqualTo(\"North\"));\n"
                + "        Assert.That(() => 4, Is.EqualTo(4));\n"
                + "        Rounds.Ignore(\"later\");\n"
                + "        Rounds.Pass();"
                + Closing
                + "internal static class Rounds { internal static void Ignore(string why) { } internal static void Pass() { } }\n")
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_nunit_is_not_read()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .Without("nunit.framework.dll")
            .WithFile("Rounds.cs", "[Obsolete(\"moved\")]\ninternal static class Rounds { internal static void Ignore(string why) { } internal static void Start() => Ignore(\"later\"); }\n")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST004TestIsNeverSkippedOrRepeated>()
            .WithFile("WhenARoundStarts.cs", Opening + "        Assert.{|CS0117:Skip|}(\"later\");" + Closing)
            .RunAsync();

    private static DiagnosticResult NoAttribute(int at, string attribute)
        => new DiagnosticResult(TEST004TestIsNeverSkippedOrRepeated.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'[{attribute}]' is no attribute a browser test takes; a test carries [Test] and its fixture [TestFixtureSource] and [Category], so remove it");

    private static DiagnosticResult SetUp(int at, string attribute)
        => new DiagnosticResult(TEST004TestIsNeverSkippedOrRepeated.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'[{attribute}]' sets a run up, which is the harness's alone; move it to a file under Harness");

    private static DiagnosticResult NoAssertion(int at, string member)
        => new DiagnosticResult(TEST004TestIsNeverSkippedOrRepeated.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{member}' is no assertion a browser test takes; a test asserts with Assert.That and Expect alone, so remove it");
}
