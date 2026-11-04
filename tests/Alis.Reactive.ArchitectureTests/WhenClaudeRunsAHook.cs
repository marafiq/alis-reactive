using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenClaudeRunsAHook
{
    private static readonly Regex ProjectPath = new(@"\$CLAUDE_PROJECT_DIR/([^""\s]+)", RegexOptions.Compiled);

    [Test]
    public void every_file_a_hook_command_runs_is_in_the_repo()
    {
        var cited = HookCommands().Select(hook => hook.Command)
            .SelectMany(command => ProjectPath.Matches(command).Select(match => match.Groups[1].Value))
            .ToList();
        var missing = cited.Where(path => !File.Exists(Path.Combine(Repository.Root, path))).ToList();

        Assert.That(cited, Is.Not.Empty, "No hook command in .claude/settings.json names a file under $CLAUDE_PROJECT_DIR");
        Assert.That(missing, Is.Empty, "A hook command runs a file the repo does not hold, so the hook never runs:\n" + string.Join("\n", missing));
    }

    [Test]
    public void an_edit_to_the_generated_plan_contract_is_refused()
    {
        var allowed = GeneratedContract()
            .SelectMany(file => new[] { "Edit", "Write" }.Select(tool => (Tool: tool, File: file)))
            .Where(edit => DecisionFor(edit.Tool, new { file_path = Path.Combine(Repository.Root, edit.File) }) != "deny")
            .Select(edit => $"{edit.Tool} {edit.File}")
            .ToList();

        Assert.That(allowed, Is.Empty, "The generated plan contract is regenerated, never hand-edited, yet a hook allowed:\n" + string.Join("\n", allowed));
    }

    [Test]
    public void an_edit_to_a_hand_written_file_beside_or_named_like_the_contract_is_allowed()
    {
        var generated = GeneratedContract();
        var contractFolder = generated[0][..generated[0].LastIndexOf('/')];
        var contractNames = generated.Select(Path.GetFileName).ToHashSet();
        var handWritten = Repository.TrackedFiles
            .Where(file => !generated.Contains(file))
            .Where(file => file.StartsWith(contractFolder + "/", StringComparison.Ordinal) || contractNames.Contains(Path.GetFileName(file)))
            .ToList();
        var refused = handWritten.Where(file => DecisionFor("Edit", new { file_path = Path.Combine(Repository.Root, file) }) == "deny").ToList();

        Assert.That(handWritten, Is.Not.Empty, $"No hand-written file sits beside the contract in {contractFolder} or shares its names");
        Assert.That(refused, Is.Empty, "A hook refused an edit to a hand-written file:\n" + string.Join("\n", refused));
    }

    [Test]
    public void a_raw_test_run_of_the_solution_or_of_each_playwright_project_is_refused()
    {
        var (browser, _) = TestProjects();
        var commands = browser.Select(project => $"dotnet test {project}")
            .Append("dotnet test")
            .Append("dotnet test Alis.Reactive.slnx")
            .ToList();
        var allowed = commands.Where(command => DecisionFor("Bash", new { command }) != "deny").ToList();

        Assert.That(browser, Is.Not.Empty, "No test project that references Microsoft.Playwright was found");
        Assert.That(allowed, Is.Empty, "Playwright runs only through scripts/playwright.sh, yet a hook allowed:\n" + string.Join("\n", allowed));
    }

    [Test]
    public void a_raw_test_run_of_each_non_browser_project_is_allowed()
    {
        var (_, other) = TestProjects();
        var refused = other.Select(project => $"dotnet test {project}")
            .Where(command => DecisionFor("Bash", new { command }) == "deny")
            .ToList();

        Assert.That(other, Is.Not.Empty, "No test project without Microsoft.Playwright was found");
        Assert.That(refused, Is.Empty, "A hook refused a test project that opens no browser:\n" + string.Join("\n", refused));
    }

    [Test]
    public void a_git_stash_in_any_form_is_refused()
    {
        string[] commands =
        [
            "git stash",
            "git stash pop",
            "git stash apply stash@{0}",
            "git stash drop stash@{0}",
            "git stash list",
            "git -C .worktrees/redesign stash push -m wip",
            "cd tests && git stash",
            "echo $(git stash list)",
        ];
        var allowed = commands.Where(command => DecisionFor("Bash", new { command }) != "deny").ToList();

        Assert.That(allowed, Is.Empty, "stash@{0} holds work that is never popped, applied or dropped, yet a hook allowed:\n" + string.Join("\n", allowed));
    }

    [Test]
    public void a_git_command_that_names_a_stash_only_in_its_text_is_allowed()
    {
        string[] commands =
        [
            "git status",
            "git log --grep stash",
            "git commit -m \"stash the draft\"",
            "git show --stat HEAD",
        ];
        var refused = commands.Where(command => DecisionFor("Bash", new { command }) == "deny").ToList();

        Assert.That(refused, Is.Empty, "A hook refused a git command that runs no stash:\n" + string.Join("\n", refused));
    }

    [Test]
    public void reading_the_user_secrets_is_refused()
    {
        string[] commands =
        [
            "dotnet user-secrets list",
            "dotnet user-secrets list --project Alis.Reactive.SandboxApp",
            "dotnet user-secrets --project Alis.Reactive.SandboxApp list",
            "cat ~/.microsoft/usersecrets/alis-sandbox/secrets.json",
        ];
        string[] files =
        [
            "/Users/dev/.microsoft/usersecrets/alis-sandbox/secrets.json",
            @"C:\Users\dev\AppData\Roaming\Microsoft\UserSecrets\alis-sandbox\secrets.json",
        ];
        var allowed = commands.Where(command => DecisionFor("Bash", new { command }) != "deny")
            .Concat(files.Where(file => DecisionFor("Read", new { file_path = file }) != "deny").Select(file => "Read " + file))
            .ToList();

        Assert.That(allowed, Is.Empty, "User secrets are never read, yet a hook allowed:\n" + string.Join("\n", allowed));
    }

    [Test]
    public void a_command_or_read_that_names_user_secrets_without_reading_them_is_allowed()
    {
        string[] commands =
        [
            "git grep -n UserSecretsId",
            "dotnet user-secrets --help",
        ];
        string[] files = [Path.Combine(Repository.Root, "Alis.Reactive.SandboxApp/Alis.Reactive.SandboxApp.csproj")];
        var refused = commands.Where(command => DecisionFor("Bash", new { command }) == "deny")
            .Concat(files.Where(file => DecisionFor("Read", new { file_path = file }) == "deny").Select(file => "Read " + file))
            .ToList();

        Assert.That(refused, Is.Empty, "A hook refused a command or read that prints no secret:\n" + string.Join("\n", refused));
    }

    private static IReadOnlyList<(string Event, string Matcher, string Command)> HookCommands()
    {
        using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(Repository.Root, ".claude/settings.json")));
        return settings.RootElement.GetProperty("hooks").EnumerateObject()
            .SelectMany(hookEvent => hookEvent.Value.EnumerateArray().SelectMany(group => group.GetProperty("hooks").EnumerateArray()
                .Select(hook => (hookEvent.Name,
                    group.TryGetProperty("matcher", out var matcher) ? matcher.GetString() ?? "" : "",
                    hook.GetProperty("command").GetString() ?? ""))))
            .ToList();
    }

    private static string DecisionFor(string tool, object toolInput)
    {
        var hooks = HookCommands().Where(hook => hook.Event == "PreToolUse" && Regex.IsMatch(tool, $"^(?:{hook.Matcher})$")).ToList();
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

    private static IReadOnlyList<string> GeneratedContract()
    {
        var schemas = Repository.TrackedFiles.Where(file => Path.GetFileName(file) == "plan.schema.json").ToList();
        Assert.That(schemas, Has.Count.EqualTo(1), "The generated plan schema is the one tracked plan.schema.json");
        var contract = schemas[0][..schemas[0].LastIndexOf('/')] + "/plan.ts";
        Assert.That(Repository.TrackedFiles, Does.Contain(contract), "The generated plan.ts sits beside plan.schema.json");
        return [contract, schemas[0]];
    }

    private static (IReadOnlyList<string> Browser, IReadOnlyList<string> Other) TestProjects()
    {
        var projects = Repository.TrackedFiles.Where(file => file.EndsWith(".csproj", StringComparison.Ordinal)).ToList();
        var opensABrowser = projects.Where(project => PackagesOf(project).Contains("Microsoft.Playwright")).ToHashSet();
        var testProjects = projects.Where(project => PackagesOf(project).Contains("Microsoft.NET.Test.Sdk")).ToList();
        var browser = testProjects.Where(project => opensABrowser.Contains(project) || ProjectsReferencedBy(project).Any(opensABrowser.Contains)).ToList();
        string Folder(string project) => project[..project.LastIndexOf('/')];
        return (browser.Select(Folder).ToList(), testProjects.Except(browser).Select(Folder).ToList());
    }

    private static HashSet<string> PackagesOf(string project)
        => XDocument.Load(Path.Combine(Repository.Root, project)).Descendants("PackageReference")
            .Select(reference => (string?)reference.Attribute("Include") ?? "")
            .ToHashSet(StringComparer.Ordinal);

    private static IEnumerable<string> ProjectsReferencedBy(string project)
    {
        var folder = Path.GetDirectoryName(Path.Combine(Repository.Root, project))!;
        return XDocument.Load(Path.Combine(Repository.Root, project)).Descendants("ProjectReference")
            .Select(reference => Path.GetRelativePath(Repository.Root, Path.GetFullPath(Path.Combine(folder, (string?)reference.Attribute("Include") ?? "")))
                .Replace('\\', '/'));
    }
}
