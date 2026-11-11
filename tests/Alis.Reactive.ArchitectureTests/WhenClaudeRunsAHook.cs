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
