using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenATestBranches
{
    private const string Fields = "    private readonly bool _open = true;\n    private readonly bool _signed = true;\n    private readonly string? _ward = \"North\";\n"
                                  + "    private readonly int[]? _beds = [4];\n    private string? _nurse;\n\n";

    private const string Opening = "public sealed class WhenARoundStarts\n{\n" + Fields + "    [Test]\n    public async Task the_round_is_listed()\n    {\n";
    private const string Closing = "\n        await Task.CompletedTask;\n    }\n}\n";

    private const string Test = "the_round_is_listed";

    [RefusedCase]
    public Task a_conditional_statement_in_a_test_is_refused_at_its_keyword()
        => new Case<TEST005TestHasNoBranch>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        {|#0:if|} (_open) { }\n"
                + "        else {|#1:if|} (_signed) { }\n"
                + "        {|#2:switch|} (_ward) { case \"North\": break; }\n"
                + "        {|#3:try|} { } finally { }\n"
                + "        {|#4:goto|} done;\n"
                + "        done: ;"
                + Closing)
            .Expecting(Branches(0, "if"), Branches(1, "if"), Branches(2, "switch"), Branches(3, "try"), Branches(4, "goto"))
            .RunAsync();

    [RefusedCase]
    public Task a_conditional_operator_in_a_test_is_refused_at_its_operator()
        => new Case<TEST005TestHasNoBranch>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var rounds = _open {|#0:?|} 1 : 2;\n"
                + "        var ward = _ward {|#1:??|} \"South\";\n"
                + "        _nurse {|#2:??=|} \"Ada\";\n"
                + "        var length = _ward{|#3:?|}.Length;\n"
                + "        var bed = _beds{|#4:?|}[0];\n"
                + "        var both = _open {|#5:&&|} _signed;\n"
                + "        var either = _open {|#6:|||} _signed;\n"
                + "        var named = _ward {|#7:switch|} { \"North\" => 1, _ => 2 };"
                + Closing)
            .Expecting(
                Branches(0, "?:"), Branches(1, "??"), Branches(2, "??="), Branches(3, "?."), Branches(4, "?[]"), Branches(5, "&&"), Branches(6, "||"),
                Branches(7, "switch"))
            .RunAsync();

    [RefusedCase]
    public Task a_loop_in_a_test_is_refused_at_its_keyword()
        => new Case<TEST005TestHasNoBranch>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        {|#0:for|} (var bed = 0; bed < 4; bed++) { }\n"
                + "        {|#1:foreach|} (var bed in new[] { 1, 2 }) { }\n"
                + "        {|#2:while|} (Rounds.Left() > 0) { }\n"
                + "        {|#3:do|} { } while (Rounds.Left() > 0);\n"
                + "        await {|#4:foreach|} (var bed in Rounds.Beds()) { }"
                + Closing
                + "internal static class Rounds\n{\n    internal static int Left() => 0;\n\n"
                + "    internal static async System.Collections.Generic.IAsyncEnumerable<int> Beds()\n    {\n        await Task.CompletedTask;\n        yield return 4;\n    }\n}\n")
            .Expecting(Loops(0, "for"), Loops(1, "foreach"), Loops(2, "while"), Loops(3, "do"), Loops(4, "foreach"))
            .RunAsync();

    [RefusedCase]
    public Task a_branch_in_a_lambda_or_a_local_function_of_a_test_is_refused_for_the_test()
        => new Case<TEST005TestHasNoBranch>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        Func<int> rounds = () => _open {|#0:?|} 1 : 2;\n"
                + "        int Beds()\n        {\n            {|#1:if|} (_signed) return 4;\n            return 0;\n        }\n\n"
                + "        _ = rounds() + Beds();"
                + Closing)
            .Expecting(Branches(0, "?:"), Branches(1, "if"))
            .RunAsync();

    [AcceptedCase]
    public Task a_branch_and_a_loop_in_a_helper_are_accepted()
        => new Case<TEST005TestHasNoBranch>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        _ = Rounds.Of(_open, _ward);"
                + Closing)
            .WithFile(
                "Rounds.cs",
                "internal static class Rounds\n{\n"
                + "    internal static int Of(bool open, string? ward)\n    {\n"
                + "        if (open && ward?.Length > 0) return 1;\n"
                + "        for (var bed = 0; bed < 4; bed++) { }\n"
                + "        return open ? 2 : (ward ?? \"South\").Length;\n    }\n}\n")
            .RunAsync();

    [AcceptedCase]
    public Task a_test_of_one_path_is_accepted()
        => new Case<TEST005TestHasNoBranch>()
            .WithFile(
                "WhenARoundStarts.cs",
                Opening
                + "        var both = _open & _signed;\n"
                + "        var either = _open | _signed;\n"
                + "        var known = _ward is not null;\n"
                + "        var north = _ward is \"North\" or \"East\";\n"
                + "        using var held = new System.IO.MemoryStream();\n"
                + "        var length = _ward!.Length;\n"
                + "        _ = both == either == known == north == (length > 0);"
                + Closing)
            .RunAsync();

    [AcceptedCase]
    public Task a_conditional_in_an_attribute_of_a_test_is_outside_its_body()
        => new Case<TEST005TestHasNoBranch>()
            .WithFile(
                "WhenARoundStarts.cs",
                "public sealed class WhenARoundStarts\n{\n    private const bool Night = true;\n\n"
                + "    [Test]\n    [Category(Night && Night ? \"night\" : \"day\")]\n    public void the_round_is_listed() { }\n}\n")
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_nunit_is_not_read()
        => new Case<TEST005TestHasNoBranch>()
            .Without("nunit.framework.dll")
            .WithFile("Rounds.cs", "internal static class Rounds { internal static int Left(bool open) { if (open) return 1; return 0; } }\n")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST005TestHasNoBranch>()
            .WithFile("WhenARoundStarts.cs", Opening + "        var rounds = {|CS0103:missing|};" + Closing)
            .RunAsync();

    private static DiagnosticResult Branches(int at, string written)
        => new DiagnosticResult(TEST005TestHasNoBranch.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{Test}' branches at '{written}'; a test has no conditional, so write the one path its promise names");

    private static DiagnosticResult Loops(int at, string written)
        => new DiagnosticResult(TEST005TestHasNoBranch.Id, DiagnosticSeverity.Error).WithLocation(at)
            .WithMessage($"'{Test}' loops at '{written}'; a test has no loop, so write each step its promise names");
}
