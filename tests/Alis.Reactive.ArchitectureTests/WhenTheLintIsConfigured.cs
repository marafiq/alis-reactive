using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenTheLintIsConfigured
{
    private const string App = "tests/Alis.Reactive.Conformance.App/";
    private const string Cases = "tests/Alis.Reactive.ArchitectureTests/LintCases/";
    private const string BesideTheFileThatFindsARootsPlans = "Alis.Reactive.Assets/runtime/Execution/";
    private const string TheFileThatReadsAContainersHiddenInputs = "Alis.Reactive.Assets/runtime/InputsAndIds/page.ts";
    private const string BesideTheFileThatReadsAContainersHiddenInputs = "Alis.Reactive.Assets/runtime/InputsAndIds/";
    private const string Equality = "eqeqeq";
    private const string BooleanCondition = "@typescript-eslint/strict-boolean-expressions";
    private const string ExhaustiveSwitch = "@typescript-eslint/switch-exhaustiveness-check";
    private const string FloatingPromise = "@typescript-eslint/no-floating-promises";
    private const string UnnecessaryCondition = "@typescript-eslint/no-unnecessary-condition";
    private const string NonNullAssertion = "@typescript-eslint/no-non-null-assertion";
    private const string RestrictedImport = "no-restricted-imports";
    private const string RestrictedSyntax = "no-restricted-syntax";
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

    [Test]
    public void the_conformance_apps_typescript_passes_the_lint_with_no_warning()
    {
        var (files, messages) = Lint(App);

        Assert.That(files, Is.Not.Empty, "The lint read no conformance TypeScript");
        Assert.That(messages, Is.Empty, "The lint reports:\n" + string.Join("\n", messages));
    }

    [RefusedCase]
    [TestCase("equals.refused.ts", Equality, 2)]
    [TestCase("text-as-a-condition.refused.ts", BooleanCondition, 2)]
    [TestCase("number-as-a-condition.refused.ts", BooleanCondition, 2)]
    [TestCase("missing-value-as-a-condition.refused.ts", BooleanCondition, 2)]
    [TestCase("missing-value-asserted-away.refused.ts", NonNullAssertion, 2)]
    [TestCase("switch-with-a-default.refused.ts", ExhaustiveSwitch, 4)]
    [TestCase("promise-left-floating.refused.ts", FloatingPromise, 2)]
    [TestCase("default-for-a-value-that-is-there.refused.ts", UnnecessaryCondition, 2)]
    public void a_consumers_typescript_is_held_to_each_typed_rule(string source, string rule, int line)
    {
        var written = WriteIn(App, File.ReadAllText(Path.Combine(Repository.Root, Cases + source)));
        try
        {
            var (_, messages) = Lint(written);

            Assert.That(messages, Is.EqualTo(new[] { new Message(written, rule, Error, line) }));
        }
        finally
        {
            File.Delete(Path.Combine(Repository.Root, written));
        }
    }

    [AcceptedCase]
    [TestCase("equals.accepted.ts")]
    [TestCase("text-as-a-condition.accepted.ts")]
    [TestCase("number-as-a-condition.accepted.ts")]
    [TestCase("missing-value-as-a-condition.accepted.ts")]
    [TestCase("switch-over-every-kind.accepted.ts")]
    [TestCase("promise-awaited.accepted.ts")]
    [TestCase("default-for-a-value-that-can-be-missing.accepted.ts")]
    [TestCase("wide-queries.refused.ts")]
    public void a_consumers_typed_code_and_its_own_marker_queries_are_accepted(string source)
    {
        var written = WriteIn(App, File.ReadAllText(Path.Combine(Repository.Root, Cases + source)));
        try
        {
            var (files, messages) = Lint(written);

            Assert.That(files, Is.EqualTo(new[] { written }));
            Assert.That(messages, Is.Empty);
        }
        finally
        {
            File.Delete(Path.Combine(Repository.Root, written));
        }
    }

    [RefusedCase]
    public void a_nested_slice_that_imports_a_slice_of_its_own_row_is_refused()
        => AssertRefused("rows/nested/left/reads-a-sibling.refused.ts", RestrictedImport, line: 1);

    [RefusedCase]
    public void a_nested_slice_that_imports_a_later_row_is_refused()
        => AssertRefused("rows/nested/left/reads-a-later-row.refused.ts", RestrictedImport, line: 1);

    [AcceptedCase]
    public void a_nested_slice_that_imports_its_parent_row_by_its_own_relative_path_is_accepted()
        => AssertAccepted("rows/nested/left/reads-its-parent.accepted.ts");

    [RefusedCase]
    public void a_nested_slice_that_names_a_folder_of_its_parent_row_in_place_of_its_file_is_refused()
        => AssertRefused("rows/nested/left/reads-a-folder-of-its-parent-row.refused.ts", RestrictedImport, line: 1);

    [AcceptedCase]
    public void a_nested_slice_that_imports_a_file_of_its_parent_row_named_like_a_folder_beside_it_is_accepted()
        => AssertAccepted("rows/nested/left/reads-a-file-of-its-parent-row-named-like-a-folder.accepted.ts");

    [AcceptedCase]
    public void a_nested_slice_that_imports_its_own_file_is_accepted()
        => AssertAccepted("rows/nested/left/reads-its-own-file.accepted.ts");

    [AcceptedCase]
    public void a_nested_slice_that_imports_an_earlier_row_is_accepted()
        => AssertAccepted("rows/nested/left/reads-an-earlier-row.accepted.ts");

    [AcceptedCase]
    public void a_folder_under_an_entry_with_a_star_that_imports_its_own_file_is_accepted()
        => AssertAccepted("rows/wings/east/reads-its-own-file.accepted.ts");

    [AcceptedCase]
    public void a_folder_under_an_entry_with_a_star_that_imports_an_earlier_row_is_accepted()
        => AssertAccepted("rows/wings/east/reads-an-earlier-row.accepted.ts");

    [RefusedCase]
    public void a_folder_under_an_entry_with_a_star_that_imports_a_folder_beside_its_own_is_refused()
        => AssertRefused("rows/wings/east/reads-a-folder-beside-it.refused.ts", RestrictedImport, line: 1);

    [RefusedCase]
    public void a_folder_under_an_entry_with_a_star_that_imports_a_later_row_is_refused()
        => AssertRefused("rows/wings/east/reads-a-later-row.refused.ts", RestrictedImport, line: 1);

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
    public void a_slice_that_imports_a_later_row_is_refused() => AssertRefused("rows/first/reads-a-later-row.refused.ts", RestrictedImport, line: 1);

    [RefusedCase]
    public void a_slice_that_reaches_a_later_row_through_its_own_folder_is_refused()
        => AssertRefused("rows/first/leaves-by-its-own-folder.refused.ts", RestrictedImport, line: 1);

    [RefusedCase]
    public void a_slice_that_imports_a_file_beside_the_table_on_a_later_row_is_refused()
        => AssertRefused("rows/first/reads-the-files-beside-the-table.refused.ts", RestrictedImport, line: 1);

    [AcceptedCase]
    public void a_slice_that_imports_its_own_file_is_accepted() => AssertAccepted("rows/first/reads-its-own-file.accepted.ts");

    [AcceptedCase]
    public void a_slice_that_imports_the_rows_before_its_own_is_accepted() => AssertAccepted("rows/second/reads-the-rows-before-its-own.accepted.ts");

    [RefusedCase]
    public void a_slice_that_imports_a_slice_of_its_own_row_is_refused() => AssertRefused("rows/second/reads-its-own-row.refused.ts", RestrictedImport, line: 1);

    [RefusedCase]
    public void a_slice_that_imports_a_slice_of_its_own_row_by_its_folder_is_refused()
        => AssertRefused("rows/second/reads-its-own-row-by-its-folder.refused.ts", RestrictedImport, line: 1);

    [AcceptedCase]
    public void a_file_beside_the_table_that_imports_an_earlier_row_is_accepted() => AssertAccepted("rows/reads-an-earlier-row.accepted.ts");

    [AcceptedCase]
    public void a_file_beside_the_table_that_imports_a_file_beside_it_is_accepted() => AssertAccepted("rows/reads-a-file-beside-it.accepted.ts");

    [RefusedCase]
    public void a_file_beside_the_table_that_imports_a_later_row_is_refused() => AssertRefused("rows/reads-a-later-row.refused.ts", RestrictedImport, line: 1);

    [RefusedCase]
    public void a_file_beside_the_table_that_imports_a_later_row_by_its_folder_is_refused()
        => AssertRefused("rows/reads-a-later-row-by-its-folder.refused.ts", RestrictedImport, line: 1);

    [AcceptedCase]
    public void a_file_beside_a_table_with_no_dot_that_imports_its_last_row_is_accepted()
        => AssertAccepted("package/reads-the-last-row.accepted.ts");

    [RefusedCase]
    public void a_slice_that_imports_the_file_beside_a_table_with_no_dot_is_refused()
        => AssertRefused("package/rooms/reads-the-file-beside-the-table.refused.ts", RestrictedImport, line: 1);

    [RefusedCase]
    public void a_file_whose_folder_has_no_row_that_imports_a_row_is_refused() => AssertRefused("rows/unlisted/reads-a-row.refused.ts", RestrictedImport, line: 1);

    [RefusedCase]
    public void each_query_wider_than_an_id_is_refused_at_its_call()
    {
        var (_, messages) = Lint(Cases + "wide-queries.refused.ts");

        Assert.That(messages, Is.EqualTo(Enumerable.Range(2, 7).Select(line => new Message(Cases + "wide-queries.refused.ts", RestrictedSyntax, Error, line))));
    }

    [RefusedCase]
    public void a_wide_query_beside_the_one_file_that_finds_a_roots_plans_is_refused()
    {
        var written = WriteIn(BesideTheFileThatFindsARootsPlans, "export function found(root: ParentNode): number {\n  return root.querySelectorAll(\"script\").length;\n}\n");
        try
        {
            var (_, messages) = Lint(written);

            Assert.That(messages, Is.EqualTo(new[] { new Message(written, RestrictedSyntax, Error, 2) }));
        }
        finally
        {
            File.Delete(Path.Combine(Repository.Root, written));
        }
    }

    [RefusedCase]
    public void a_wide_query_beside_the_one_file_that_reads_a_containers_hidden_inputs_is_refused()
    {
        var written = WriteIn(BesideTheFileThatReadsAContainersHiddenInputs, "export function found(root: ParentNode): number {\n  return root.querySelectorAll(\"input\").length;\n}\n");
        try
        {
            var (_, messages) = Lint(written);

            Assert.That(messages, Is.EqualTo(new[] { new Message(written, RestrictedSyntax, Error, 2) }));
        }
        finally
        {
            File.Delete(Path.Combine(Repository.Root, written));
        }
    }

    [AcceptedCase]
    public void the_one_file_that_reads_a_containers_hidden_inputs_queries_inside_its_element()
    {
        var (files, messages) = Lint(TheFileThatReadsAContainersHiddenInputs);

        Assert.That(files, Is.EqualTo(new[] { TheFileThatReadsAContainersHiddenInputs }), "The lint did not read the file");
        Assert.That(messages, Is.Empty);
    }

    [AcceptedCase]
    public void an_element_found_by_its_id_is_accepted() => AssertAccepted("element-by-its-id.accepted.ts");

    [AcceptedCase]
    public void a_member_named_like_a_query_that_is_read_and_not_called_is_accepted() => AssertAccepted("member-named-like-a-query.accepted.ts");

    [RefusedCase]
    [TestCase(Cases + "written/")]
    [TestCase(App)]
    public void an_inline_disable_changes_no_result(string folder)
    {
        var written = WriteIn(folder, "export function same(left: number, right: number): boolean {\n  // eslint-disable-next-line eqeqeq\n  return left == right;\n}\n");
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
    [TestCase(Cases + "written/", Cases)]
    [TestCase(App, App)]
    public void an_enum_is_refused_by_the_compiler(string folder, string project)
    {
        var written = WriteIn(folder, "export enum Kind {\n  Set,\n  Call,\n}\n");
        try
        {
            var (exitCode, output) = TypeCheckTheCases(project);

            Assert.That(exitCode, Is.Not.Zero, output);
            Assert.That(output, Does.Contain(written + "(1,13): error TS1294"));
        }
        finally
        {
            File.Delete(Path.Combine(Repository.Root, written));
        }
    }

    [AcceptedCase]
    [TestCase(Cases + "written/", Cases)]
    [TestCase(App, App)]
    public void a_union_of_literals_is_accepted_by_the_compiler(string folder, string project)
    {
        var written = WriteIn(folder, "export type Kind = \"set\" | \"call\";\n");
        try
        {
            var (exitCode, output) = TypeCheckTheCases(project);

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

    private static string WriteIn(string folder, string source)
    {
        var written = folder + "written-" + Guid.NewGuid().ToString("N") + ".ts";
        var path = Path.Combine(Repository.Root, written);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, source);
        return written;
    }

    private static (int ExitCode, string Output) TypeCheckTheCases(string project)
    {
        var (exitCode, output, errors) = Node("node_modules/typescript/bin/tsc", "-p", project);
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
