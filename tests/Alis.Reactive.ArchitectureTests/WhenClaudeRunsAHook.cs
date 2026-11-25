using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenClaudeRunsAHook
{
    private static readonly Regex ProjectPath = new(@"\$CLAUDE_PROJECT_DIR/([^""\s]+)", RegexOptions.Compiled);
    private const string ContractFolder = "Alis.Reactive.Assets/runtime/Schema/";
    private static readonly string[] GeneratedContract = [ContractFolder + "plan.ts", ContractFolder + "plan.schema.json"];
    private static readonly string[] OneFactFromTheContract =
        [ContractFolder + "index.ts", "Alis.Reactive.Assets/runtime/Execution/plan.ts", "Alis.Reactive.Assets/runtime/Execution/plan.schema.json"];

    private static readonly string[] CommandsThisMachineGetsWrong =
    [
        "git grep -n -E \"foo\\b\" -- \"*.cs\"",
        "git -C /repo grep -n -E \"a\\sb\" -- \"*.md\"",
        "git grep -c -E \"x|y\\d\" -- spec",
        "scripts/playwright.sh --filter \"FullyQualifiedName~Rooms\"",
        "grep -rn --include=\"*.cs\" foo .",
        "codex exec -s read-only \"brief\"",
        "caffeinate -i codex exec -s read-only \"brief\" > out.log 2>&1",
        "for unit in \"ci Handoff\" \"skill BuildAScreen\"; do set -- $unit; echo \"$1\"; done",
        "names=\"a b\"; for name in $names; do echo \"$name\"; done",
        "TSC=\"node tsc -b\"\n$TSC > tsc.log 2>&1",
        "c=$(git rev-list -1 HEAD); git show $c:spec/MISTAKES.md",
        "echo \"$dir:t\"",
        "set -- a/b; echo $1:h",
        "git checkout -- spec/NOTES.md spec/STATE.json",
        "cd ../core-leads && git checkout -- spec/NOTES.md",
        "git -C ../core-leads checkout -- spec/NOTES.md",
        "git restore spec/NOTES.md",
        "grep -c -F \"`WhenAJsonSaveLeavesOutTheLeadsName`\" use-cases.md",
    ];

    private static readonly string[] OneFactFromACommandThisMachineGetsWrong =
    [
        "git grep -n -E \"a[[:space:]]b\" -- \"*.cs\"",
        "git grep -n -F \"\\b\" -- \"*.ts\"",
        "scripts/playwright.sh --test M22_F2.M22_X11_0",
        "scripts/playwright.sh --all",
        "git grep -n foo -- \"*.cs\"",
        "pgrep -f \"codex exec -s read-only\"",
        "caffeinate -i codex exec -s read-only \"brief\" < /dev/null > out.log 2>&1",
        "for name in a b; do set -- one two; echo \"$name $1\"; done",
        "cat > run.sh <<'EOF'\nfor unit in $units; do set -- $unit; $TOOL \"$1\"; done\nEOF\nbash run.sh",
        "S=/tmp/work; \"$S/tool\" --version; $S/tool --version; echo $S done",
        "bash -c 'for name in $names; do echo \"$name\"; done'",
        "c=$(git rev-list -1 HEAD); git show \"${c}:spec/MISTAKES.md\"",
        "echo \"$PATH:/usr/local/bin\" \"$host:8080\"",
        "awk -F: '{ print $1:h }' file",
        "git checkout HEAD -- spec/NOTES.md spec/STATE.json",
        "git -C ../core-leads checkout 830817f4b -- spec/NOTES.md",
        "git restore --source=HEAD spec/NOTES.md",
        "git restore --staged spec/NOTES.md",
        "git checkout -b feature/x origin/main",
        "grep -c -F '`WhenAJsonSaveLeavesOutTheLeadsName`' use-cases.md",
        "echo \"a \\`literal\\` backtick\"",
        "cat > note.md <<'EOF'\nthe `door`\nEOF",
    ];

    [Test]
    public void every_file_a_hook_command_runs_is_in_the_repo()
    {
        var (cited, missing) = HookFiles(TheRepositorysSettings());

        Assert.That(cited, Is.Not.Empty, "No hook command in .claude/settings.json names a file under $CLAUDE_PROJECT_DIR");
        Assert.That(missing, Is.Empty, "A hook command runs a file the repo does not hold, so the hook never runs:\n" + string.Join("\n", missing));
    }

    [RefusedCase]
    public void a_hook_command_that_runs_a_file_the_repo_does_not_hold_is_refused()
        => Assert.That(HookFiles(SettingsRunning(".claude/hooks/gone.mjs")).Missing, Is.EqualTo(new[] { ".claude/hooks/gone.mjs" }));

    [AcceptedCase]
    public void a_hook_command_that_runs_a_file_the_repo_holds_is_accepted()
    {
        var (cited, missing) = HookFiles(SettingsRunning(".claude/hooks/protect-generated.mjs"));

        Assert.That(cited, Is.EqualTo(new[] { ".claude/hooks/protect-generated.mjs" }));
        Assert.That(missing, Is.Empty);
    }

    [RefusedCase]
    public void an_edit_to_the_generated_plan_contract_is_refused()
    {
        var allowed = GeneratedContract
            .SelectMany(file => new[] { "Edit", "Write" }.Select(tool => (Tool: tool, File: file)))
            .Where(edit => DecisionFor(edit.Tool, new { file_path = Path.Combine(Repository.Root, edit.File) }) != "deny")
            .Select(edit => $"{edit.Tool} {edit.File}")
            .ToList();

        Assert.That(allowed, Is.Empty, "The generated plan contract is regenerated, never hand-edited, yet a hook allowed:\n" + string.Join("\n", allowed));
    }

    [AcceptedCase]
    public void an_edit_to_a_hand_written_file_beside_or_named_like_the_contract_is_allowed()
    {
        var refused = OneFactFromTheContract.Where(file => DecisionFor("Edit", new { file_path = Path.Combine(Repository.Root, file) }) == "deny").ToList();

        Assert.That(refused, Is.Empty, "A hook refused an edit to a hand-written file:\n" + string.Join("\n", refused));
    }

    [RefusedCase]
    public void a_command_in_a_shape_this_machine_gets_wrong_is_refused()
    {
        var allowed = CommandsThisMachineGetsWrong.Where(command => DecisionFor("Bash", new { command }) != "deny").ToList();

        Assert.That(allowed, Is.Empty, "A hook allowed a command in a shape this machine gets wrong:\n" + string.Join("\n", allowed));
    }

    [AcceptedCase]
    public void a_command_one_fact_from_such_a_shape_is_allowed()
    {
        var refused = OneFactFromACommandThisMachineGetsWrong.Where(command => DecisionFor("Bash", new { command }) == "deny").ToList();

        Assert.That(refused, Is.Empty, "A hook refused a command this machine runs as written:\n" + string.Join("\n", refused));
    }

    [Test]
    public void the_generated_plan_contract_is_tracked_where_a_hook_refuses_an_edit()
        => Assert.That(
            ContractFilesNotTracked(Repository.TrackedFiles),
            Is.Empty,
            "A hook refuses an edit to the generated plan contract at a path the repository tracks no such file at, so it protects nothing:");

    [RefusedCase]
    public void a_plan_contract_generated_into_another_folder_is_refused()
        => Assert.That(
            ContractFilesNotTracked(["Alis.Reactive.Assets/runtime/Execution/plan.ts", ContractFolder + "plan.schema.json"]),
            Is.EqualTo(new[] { ContractFolder + "plan.ts" }));

    [AcceptedCase]
    public void a_plan_contract_tracked_in_the_folder_a_hook_refuses_is_accepted()
        => Assert.That(ContractFilesNotTracked([ContractFolder + "index.ts", .. GeneratedContract]), Is.Empty);

    private static List<string> ContractFilesNotTracked(IReadOnlyList<string> tracked) => GeneratedContract.Except(tracked).ToList();

    private static string TheRepositorysSettings() => File.ReadAllText(Path.Combine(Repository.Root, ".claude/settings.json"));

    private static string SettingsRunning(string file)
        => JsonSerializer.Serialize(new
        {
            hooks = new { PreToolUse = new[] { new { matcher = "Edit", hooks = new[] { new { type = "command", command = $"node \"$CLAUDE_PROJECT_DIR/{file}\"" } } } } },
        });

    private static (IReadOnlyList<string> Cited, IReadOnlyList<string> Missing) HookFiles(string settings)
    {
        var cited = HookCommands(settings).Select(hook => hook.Command)
            .SelectMany(command => ProjectPath.Matches(command).Select(match => match.Groups[1].Value))
            .ToList();
        return (cited, cited.Where(path => !File.Exists(Path.Combine(Repository.Root, path))).ToList());
    }

    private static IReadOnlyList<(string Event, string Matcher, string Command)> HookCommands(string settingsText)
    {
        using var settings = JsonDocument.Parse(settingsText);
        return settings.RootElement.GetProperty("hooks").EnumerateObject()
            .SelectMany(hookEvent => hookEvent.Value.EnumerateArray().SelectMany(group => group.GetProperty("hooks").EnumerateArray()
                .Select(hook => (hookEvent.Name,
                    group.TryGetProperty("matcher", out var matcher) ? matcher.GetString() ?? "" : "",
                    hook.GetProperty("command").GetString() ?? ""))))
            .ToList();
    }

    private static string DecisionFor(string tool, object toolInput)
    {
        var hooks = HookCommands(TheRepositorysSettings()).Where(hook => hook.Event == "PreToolUse" && Regex.IsMatch(tool, $"^(?:{hook.Matcher})$")).ToList();
        Assert.That(hooks, Is.Not.Empty, $"No PreToolUse hook in .claude/settings.json matches {tool}");
        return hooks.Any(hook => Run(hook.Command, tool, toolInput) == "deny") ? "deny" : "allow";
    }

    private static string Run(string command, string tool, object toolInput)
    {
        var start = new ProcessStartInfo("sh", ["-c", command])
        {
            WorkingDirectory = Repository.Root,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.Environment["CLAUDE_PROJECT_DIR"] = Repository.Root;
        using var hook = Process.Start(start) ?? throw new InvalidOperationException("The hook could not be started: " + command);
        hook.StandardInput.Write(JsonSerializer.Serialize(new { hook_event_name = "PreToolUse", tool_name = tool, tool_input = toolInput, cwd = Repository.Root }));
        hook.StandardInput.Close();
        var output = hook.StandardOutput.ReadToEnd();
        var errors = hook.StandardError.ReadToEnd();
        hook.WaitForExit();
        if (hook.ExitCode != 0) throw new InvalidOperationException($"The hook `{command}` exited with {hook.ExitCode}: {errors}");
        if (output.Trim().Length == 0) return "allow";

        using var decision = JsonDocument.Parse(output);
        return decision.RootElement.GetProperty("hookSpecificOutput").GetProperty("permissionDecision").GetString() ?? "allow";
    }
}
