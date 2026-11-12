using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenTheLintIsConfigured
{
    private const string Cases = "tests/Alis.Reactive.ArchitectureTests/LintCases/";
    private const string Equality = "eqeqeq";
    private const string BooleanCondition = "@typescript-eslint/strict-boolean-expressions";
    private const string ExhaustiveSwitch = "@typescript-eslint/switch-exhaustiveness-check";
    private const string FloatingPromise = "@typescript-eslint/no-floating-promises";
    private const string UnnecessaryCondition = "@typescript-eslint/no-unnecessary-condition";
    private const string NonNullAssertion = "@typescript-eslint/no-non-null-assertion";
    private const string RestrictedImport = "no-restricted-imports";
    private const int Error = 2;
    private const int Warning = 1;

    private sealed record Message(string File, string? Rule, int Severity, int Line);

    [Test]
    public void the_runtimes_typescript_passes_the_lint_with_no_warning()
    {
        var (files, messages) = Lint("Alis.Reactive.Assets");

        Assert.That(files, Is.Not.Empty, "The lint read no file under Alis.Reactive.Assets");
        Assert.That(messages, Is.Empty, "The lint reports:\n" + string.Join("\n", messages));
    }

    [RefusedCase]
    public void a_loose_equality_is_refused() => AssertRefused("equals.refused.ts", Equality, line: 2);

    [AcceptedCase]
    public void a_strict_equality_is_accepted() => AssertAccepted("equals.accepted.ts");

    [RefusedCase]
    public void a_text_as_a_condition_is_refused() => AssertRefused("text-as-a-condition.refused.ts", BooleanCondition, line: 2);

    [AcceptedCase]
    public void a_text_compared_with_the_empty_text_is_accepted() => AssertAccepted("text-as-a-condition.accepted.ts");

    [RefusedCase]
    public void a_number_as_a_condition_is_refused() => AssertRefused("number-as-a-condition.refused.ts", BooleanCondition, line: 2);

    [AcceptedCase]
    public void a_number_compared_with_zero_is_accepted() => AssertAccepted("number-as-a-condition.accepted.ts");

    [RefusedCase]
    public void a_missing_value_as_a_condition_is_refused() => AssertRefused("missing-value-as-a-condition.refused.ts", BooleanCondition, line: 2);

    [AcceptedCase]
    public void a_missing_value_compared_with_null_by_name_is_accepted() => AssertAccepted("missing-value-as-a-condition.accepted.ts");

    [RefusedCase]
    public void a_missing_value_asserted_away_is_refused() => AssertRefused("missing-value-asserted-away.refused.ts", NonNullAssertion, line: 2);

    [RefusedCase]
    public void a_switch_that_leaves_a_kind_to_its_default_is_refused() => AssertRefused("switch-with-a-default.refused.ts", ExhaustiveSwitch, line: 4);

    [AcceptedCase]
    public void a_switch_with_a_case_for_every_kind_is_accepted() => AssertAccepted("switch-over-every-kind.accepted.ts");

    [RefusedCase]
    public void a_promise_left_floating_is_refused() => AssertRefused("promise-left-floating.refused.ts", FloatingPromise, line: 2);

    [AcceptedCase]
    public void a_promise_awaited_is_accepted() => AssertAccepted("promise-awaited.accepted.ts");

    [RefusedCase]
    public void a_default_for_a_value_that_is_there_is_refused()
        => AssertRefused("default-for-a-value-that-is-there.refused.ts", UnnecessaryCondition, line: 2);

    [AcceptedCase]
    public void a_default_for_a_value_that_can_be_missing_is_accepted() => AssertAccepted("default-for-a-value-that-can-be-missing.accepted.ts");

    [RefusedCase]
    public void a_modules_pure_test_that_imports_an_internal_of_its_module_is_refused()
        => AssertRefused("module/reads-an-internal.refused.test.ts", RestrictedImport, line: 1);

    [RefusedCase]
    public void a_modules_pure_test_that_imports_through_another_folder_is_refused()
        => AssertRefused("module/reads-another-modules-index.refused.test.ts", RestrictedImport, line: 1);

    [AcceptedCase]
    public void a_modules_pure_test_that_imports_its_own_index_is_accepted() => AssertAccepted("module/reads-its-index.accepted.test.ts");

    [AcceptedCase]
    public void a_file_of_the_module_that_is_no_test_imports_its_internals() => AssertAccepted("module/reads-an-internal.accepted.ts");

    [RefusedCase]
    public void an_inline_disable_changes_no_result()
    {
        var written = Write("export function same(left: number, right: number): boolean {\n  // eslint-disable-next-line eqeqeq\n  return left == right;\n}\n");
        try
        {
            var (_, messages) = Lint(written);

            Assert.That(messages, Is.EqualTo(new[] { new Message(written, null, Warning, 2), new Message(written, Equality, Error, 3) }));
        }
        finally
        {
            File.Delete(Path.Combine(Repository.Root, written));
        }
    }

    [RefusedCase]
    public void an_enum_is_refused_by_the_compiler()
    {
        var written = Write("export enum Kind {\n  Set,\n  Call,\n}\n");
        try
        {
            var (exitCode, output) = TypeCheckTheCases();

            Assert.That(exitCode, Is.Not.Zero, output);
            Assert.That(output, Does.Contain(written + "(1,13): error TS1294"));
        }
        finally
        {
            File.Delete(Path.Combine(Repository.Root, written));
        }
    }

    [AcceptedCase]
    public void a_union_of_literals_is_accepted_by_the_compiler()
    {
        var written = Write("export type Kind = \"set\" | \"call\";\n");
        try
        {
            var (exitCode, output) = TypeCheckTheCases();

            Assert.That(exitCode, Is.Zero, output);
        }
        finally
        {
            File.Delete(Path.Combine(Repository.Root, written));
        }
    }

    private static void AssertRefused(string file, string rule, int line)
    {
        var (_, messages) = Lint(Cases + file);

        Assert.That(messages, Is.EqualTo(new[] { new Message(Cases + file, rule, Error, line) }));
    }

    private static void AssertAccepted(string file)
    {
        var (files, messages) = Lint(Cases + file);

        Assert.That(files, Is.EqualTo(new[] { Cases + file }), "The lint did not read the file");
        Assert.That(messages, Is.Empty);
    }

    private static string Write(string source)
    {
        var written = Cases + "written/" + Guid.NewGuid().ToString("N") + ".ts";
        var path = Path.Combine(Repository.Root, written);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, source);
        return written;
    }

    private static (int ExitCode, string Output) TypeCheckTheCases()
    {
        var (exitCode, output, errors) = Node("node_modules/typescript/bin/tsc", "-p", Cases);
        return (exitCode, output + errors);
    }

    private static (int ExitCode, string Output, string Errors) Node(string script, params string[] arguments)
    {
        var start = new ProcessStartInfo("node", [Path.Combine(Repository.Root, script), .. arguments])
        {
            WorkingDirectory = Repository.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var node = Process.Start(start) ?? throw new InvalidOperationException("node could not be started to run " + script);
        var output = node.StandardOutput.ReadToEndAsync();
        var errors = node.StandardError.ReadToEndAsync();
        node.WaitForExit();
        return (node.ExitCode, output.Result, errors.Result);
    }

    private static (IReadOnlyList<string> Files, IReadOnlyList<Message> Messages) Lint(string path)
    {
        var (exitCode, output, errors) = Node("node_modules/eslint/bin/eslint.js", "--format", "json", path);
        if (exitCode is not (0 or 1))
            throw new InvalidOperationException($"The lint exited with {exitCode} on {path}, so it read nothing: {errors}");

        using var report = JsonDocument.Parse(output);
        var files = new List<string>();
        var messages = new List<Message>();
        foreach (var result in report.RootElement.EnumerateArray())
        {
            var file = Path.GetRelativePath(Repository.Root, result.GetProperty("filePath").GetString()!).Replace('\\', '/');
            files.Add(file);
            foreach (var message in result.GetProperty("messages").EnumerateArray())
            {
                messages.Add(new Message(
                    file, message.GetProperty("ruleId").GetString(), message.GetProperty("severity").GetInt32(), message.GetProperty("line").GetInt32()));
            }
        }

        return (files, messages);
    }
}
