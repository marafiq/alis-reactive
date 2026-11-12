using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAPlaywrightRunEnds
{
    private const string Reader = "scripts/playwright-result.mjs";
    private const string TheRepositorysBudget = "tests/Alis.Reactive.Conformance.PlaywrightTests/Harness/measured.json";
    private const string Listed = "Rounds.WhenARoundStarts(\"net10\").the_round_is_listed";
    private const string Opened = "Rounds.WhenARoundStarts(\"net10\").the_round_is_opened";
    private const int RunsOfABudget = 20;

    [Test]
    public void the_budget_the_repository_holds_reads_a_green_run()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [Listed], [Ran(Listed, "Passed", 0.01)], Path.Combine(Repository.Root, TheRepositorysBudget));

        Assert.That(read.Said, Is.EqualTo(new[] { $"[playwright:result] rounds 1/1: 1 selected, 1 ran, 1 passed, 0 failed, 0 over budget, slowest 0.01s {Listed}" }));
        Assert.That(read.ExitCode, Is.Zero);
    }

    [AcceptedCase]
    public void a_green_run_prints_its_result_line_and_passes()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [Listed, Opened], [Ran(Listed, "Passed", 1.5), Ran(Opened, "Passed", 8)], Budget(written, 8, 2));

        Assert.That(read.Said, Is.EqualTo(new[] { $"[playwright:result] rounds 1/1: 2 selected, 2 ran, 2 passed, 0 failed, 0 over budget, slowest 8.00s {Opened}" }));
        Assert.That(read.ExitCode, Is.Zero);
    }

    [RefusedCase]
    public void a_run_that_wrote_no_trx_is_refused()
    {
        using var written = new WrittenFolder();
        var selection = Selection(written, [Listed]);
        var gone = Path.Combine(Path.GetDirectoryName(selection)!, "gone.trx");

        var read = Run("--selection", selection, "--trx", gone, "--budget", Budget(written, 8, 2));

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"[playwright:result] no TRX at {gone}; the test process wrote no result",
            "[playwright:result] rounds 1/1: no result",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_run_that_selected_no_test_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [], [], Budget(written, 8, 2));

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            "[playwright:result] the run selected no test",
            "[playwright:result] rounds 1/1: 0 selected, 0 ran, 0 passed, 0 failed, 0 over budget, slowest 0.00s no test",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_selected_test_that_did_not_run_or_was_skipped_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [Listed, Opened], [Ran(Listed, "Passed", 1), Ran(Opened, "NotExecuted", 0)], Budget(written, 8, 2));

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"[playwright:result] selected and not run: {Opened}",
            $"[playwright:result] rounds 1/1: 2 selected, 1 ran, 1 passed, 0 failed, 0 over budget, slowest 1.00s {Listed}",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_test_that_ran_and_was_not_selected_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [Listed], [Ran(Listed, "Passed", 1), Ran(Opened, "Passed", 2)], Budget(written, 8, 2));

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"[playwright:result] run and not selected: {Opened}",
            $"[playwright:result] rounds 1/1: 1 selected, 2 ran, 2 passed, 0 failed, 0 over budget, slowest 2.00s {Opened}",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_test_that_ran_twice_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [Listed], [Ran(Listed, "Failed", 1), Ran(Listed, "Passed", 1)], Budget(written, 8, 2));

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"[playwright:result] run 2 times: {Listed}",
            $"[playwright:result] failed (Failed): {Listed}",
            $"[playwright:result] rounds 1/1: 1 selected, 2 ran, 1 passed, 1 failed, 0 over budget, slowest 1.00s {Listed}",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_failed_test_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [Listed, Opened], [Ran(Listed, "Passed", 1), Ran(Opened, "Failed", 5.25)], Budget(written, 8, 2));

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"[playwright:result] failed (Failed): {Opened}",
            $"[playwright:result] rounds 1/1: 2 selected, 2 ran, 1 passed, 1 failed, 0 over budget, slowest 5.25s {Opened}",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_test_over_the_budget_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [Listed, Opened], [Ran(Listed, "Passed", 8.01), Ran(Opened, "Passed", 1)], Budget(written, 8, 2));

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"[playwright:result] over budget, 8.01s of 8s: {Listed}",
            $"[playwright:result] rounds 1/1: 2 selected, 2 ran, 2 passed, 0 failed, 1 over budget, slowest 8.01s {Listed}",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_server_left_alive_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Run("--selection", Selection(written, [Listed]), "--trx", Trx(written, [Ran(Listed, "Passed", 1)]), "--budget", Budget(written, 8, 2), "--servers-left", "1");

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            "[playwright:result] 1 server of the conformance app left alive after the run",
            $"[playwright:result] rounds 1/1: 1 selected, 1 ran, 1 passed, 0 failed, 0 over budget, slowest 1.00s {Listed}",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_budget_a_hand_chose_is_refused()
    {
        using var written = new WrittenFolder();
        var chosen = Budget(written, 30, 2);

        var read = Read(written, [Listed], [Ran(Listed, "Passed", 1)], chosen);

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"[playwright:result] the budget at {chosen} is 30s and its slowest test 2s; a budget is four times the slowest test measured, never chosen",
            $"[playwright:result] rounds 1/1: 1 selected, 1 ran, 1 passed, 0 failed, 0 over budget, slowest 1.00s {Listed}",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_budget_measured_over_fewer_runs_is_refused()
    {
        using var written = new WrittenFolder();
        var hurried = Budget(written, 8, 2, runs: RunsOfABudget - 1);

        var read = Read(written, [Listed], [Ran(Listed, "Passed", 1)], hurried);

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"[playwright:result] the budget at {hurried} is measured over 19 runs; a budget is measured over 20 green runs",
            $"[playwright:result] rounds 1/1: 1 selected, 1 ran, 1 passed, 0 failed, 0 over budget, slowest 1.00s {Listed}",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_run_with_no_measured_budget_is_refused()
    {
        using var written = new WrittenFolder();
        var selection = Selection(written, [Listed]);
        var gone = Path.Combine(Path.GetDirectoryName(selection)!, "measured.json");

        var read = Run("--selection", selection, "--trx", Trx(written, [Ran(Listed, "Passed", 1)]), "--budget", gone);

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"[playwright:result] no budget is measured at {gone}; scripts/playwright.sh --measure writes it",
            $"[playwright:result] rounds 1/1: 1 selected, 1 ran, 1 passed, 0 failed, 0 over budget, slowest 1.00s {Listed}",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [AcceptedCase]
    public void twenty_green_runs_write_a_budget_of_four_times_their_slowest_test()
    {
        using var written = new WrittenFolder();
        var selection = Selection(written, [Listed, Opened]);
        var budget = Path.Combine(Path.GetDirectoryName(selection)!, "measured.json");
        var runs = Enumerable.Range(1, RunsOfABudget).Select(run => Trx(written, [Ran(Listed, "Passed", 1), Ran(Opened, "Passed", run == 7 ? 2.125 : 1.5)], $"measure-{run}.trx"));

        var measured = Run(["--measure", "--selection", selection, "--budget", budget, .. runs]);

        Assert.That(measured.Said, Is.EqualTo(new[]
        {
            $"[playwright:measured] budget 8.52s, four times the slowest test of 20 green runs of 2 tests, 2.13s {Opened}",
        }));
        Assert.That(measured.ExitCode, Is.Zero);
        using var held = JsonDocument.Parse(File.ReadAllText(budget));
        Assert.That(held.RootElement.GetProperty("budgetSeconds").GetDouble(), Is.EqualTo(8.52));
        Assert.That(held.RootElement.GetProperty("slowestSeconds").GetDouble(), Is.EqualTo(2.13));
        Assert.That(held.RootElement.GetProperty("slowest").GetString(), Is.EqualTo(Opened));
        Assert.That(held.RootElement.GetProperty("runs").GetInt32(), Is.EqualTo(20));
        Assert.That(Read(written, [Listed, Opened], [Ran(Listed, "Passed", 1), Ran(Opened, "Passed", 8.52)], budget).ExitCode, Is.Zero);
    }

    [RefusedCase]
    public void nineteen_runs_write_no_budget()
    {
        using var written = new WrittenFolder();
        var selection = Selection(written, [Listed]);
        var budget = Path.Combine(Path.GetDirectoryName(selection)!, "measured.json");
        var runs = Enumerable.Range(1, RunsOfABudget - 1).Select(run => Trx(written, [Ran(Listed, "Passed", 1)], $"measure-{run}.trx"));

        var measured = Run(["--measure", "--selection", selection, "--budget", budget, .. runs]);

        Assert.That(measured.Errors, Does.Contain("a budget is measured over 20 green runs, and 19 TRX files were given."));
        Assert.That(measured.ExitCode, Is.EqualTo(2));
        Assert.That(File.Exists(budget), Is.False);
    }

    [RefusedCase]
    public void a_red_run_among_the_twenty_writes_no_budget()
    {
        using var written = new WrittenFolder();
        var selection = Selection(written, [Listed]);
        var budget = Path.Combine(Path.GetDirectoryName(selection)!, "measured.json");
        var runs = Enumerable.Range(1, RunsOfABudget).Select(run => Trx(written, [Ran(Listed, run == 20 ? "Failed" : "Passed", 1)], $"measure-{run}.trx")).ToList();

        var measured = Run(["--measure", "--selection", selection, "--budget", budget, .. runs]);

        Assert.That(measured.Said, Is.EqualTo(new[]
        {
            $"[playwright:result] {runs[19]}: failed (Failed): {Listed}",
            $"[playwright:measured] no budget is written: {runs[19]} is no green run of 'rounds'",
        }));
        Assert.That(measured.ExitCode, Is.EqualTo(1));
        Assert.That(File.Exists(budget), Is.False);
    }

    private static (string Id, string Outcome, double Seconds) Ran(string id, string outcome, double seconds) => (id, outcome, seconds);

    private static (IReadOnlyList<string> Said, string Errors, int ExitCode) Read(
        WrittenFolder written, string[] selected, (string Id, string Outcome, double Seconds)[] ran, string budget)
        => Run("--selection", Selection(written, selected), "--trx", Trx(written, ran), "--budget", budget);

    private static string Selection(WrittenFolder written, string[] cases)
        => written.Write("selection.json", JsonSerializer.Serialize(new { label = "rounds", filter = "", cases }));

    private static string Budget(WrittenFolder written, double budgetSeconds, double slowestSeconds, int runs = RunsOfABudget)
        => written.Write("budget.json", JsonSerializer.Serialize(new { budgetSeconds, slowestSeconds, slowest = Listed, runs }));

    private static string Trx(WrittenFolder written, (string Id, string Outcome, double Seconds)[] ran, string name = "run.trx")
    {
        var tests = ran.Select(one => one.Id).Distinct().Select((id, place) => (Id: id, Place: place)).ToList();
        var definitions = tests.Select(test =>
        {
            var method = test.Id.LastIndexOf('.');
            var fixture = System.Security.SecurityElement.Escape(test.Id[..method]);
            return $"<UnitTest name=\"{test.Id[(method + 1)..]}\" id=\"t{test.Place}\"><Execution id=\"e{test.Place}\" />"
                   + $"<TestMethod codeBase=\"tests.dll\" adapterTypeName=\"executor://nunit3testexecutor/\" className=\"{fixture}\" name=\"{test.Id[(method + 1)..]}\" /></UnitTest>";
        });
        var results = ran.Select(one =>
        {
            var test = tests.First(known => known.Id == one.Id);
            var duration = TimeSpan.FromSeconds(one.Seconds).ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture);
            return $"<UnitTestResult executionId=\"e{test.Place}\" testId=\"t{test.Place}\" testName=\"{one.Id[(one.Id.LastIndexOf('.') + 1)..]}\" duration=\"{duration}\" outcome=\"{one.Outcome}\" />";
        });
        return written.Write(
            name,
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<TestRun xmlns=\"http://microsoft.com/schemas/VisualStudio/TeamTest/2010\">\n"
            + $"<Results>\n{string.Join("\n", results)}\n</Results>\n<TestDefinitions>\n{string.Join("\n", definitions)}\n</TestDefinitions>\n</TestRun>\n");
    }

    private static (IReadOnlyList<string> Said, string Errors, int ExitCode) Run(params string[] arguments)
    {
        var start = new ProcessStartInfo("node", [Path.Combine(Repository.Root, Reader), .. arguments])
        {
            WorkingDirectory = Repository.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var node = Process.Start(start) ?? throw new InvalidOperationException("node could not be started to run " + Reader);
        var output = node.StandardOutput.ReadToEndAsync();
        var errors = node.StandardError.ReadToEndAsync();
        node.WaitForExit();
        return (output.Result.Split('\n', StringSplitOptions.RemoveEmptyEntries), errors.Result, node.ExitCode);
    }
}
