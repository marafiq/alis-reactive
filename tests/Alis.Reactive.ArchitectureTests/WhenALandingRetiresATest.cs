using System.Diagnostics;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenALandingRetiresATest
{
    private const string Reader = "scripts/regressions.py";
    private const string Sha = "a1b2c3d4e";
    private const string Kept = "Rounds.WhenARoundStarts(\"net10\").the_round_is_listed";
    private const string Retired = "Rounds.WhenARoundIsPosted(\"net10\").its_paper_form_holds_every_control";
    private const string Carrier = "Rounds.WhenARoundIsSent(\"net10\").its_container_sends_its_inputs";
    private const string RetiredLine = Retired + " | M1_F1.X1_1_0 | " + Carrier + "\n";

    [AcceptedCase]
    public void a_gone_test_its_line_retires_lands_when_its_carrier_is_new_and_passes()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [(Kept, "Passed"), (Retired, "Passed")], [(Kept, "Passed"), (Carrier, "Passed")], RetiredLine);

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"retired: {Retired}, its promise carried by {Carrier}",
            $"1 passed at {Sha}; 1 pass now that did not",
            $"no test that passed at {Sha} fails or is gone",
        }));
        Assert.That(read.ExitCode, Is.Zero);
    }

    [RefusedCase]
    public void a_gone_test_no_line_retires_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [(Kept, "Passed"), (Retired, "Passed")], [(Kept, "Passed"), (Carrier, "Passed")], "");

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"passed at {Sha}, gone now: {Retired}",
            $"2 passed at {Sha}; 1 pass now that did not",
            $"0 that passed at {Sha} fail now, 1 are gone and 0 retired lines are refused",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_retired_test_that_still_runs_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [(Kept, "Passed"), (Retired, "Passed")], [(Kept, "Passed"), (Retired, "Passed"), (Carrier, "Passed")], RetiredLine);

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"retired and still run: {Retired}",
            $"2 passed at {Sha}; 1 pass now that did not",
            $"0 that passed at {Sha} fail now, 0 are gone and 1 retired lines are refused",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_retired_test_whose_carrier_did_not_pass_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [(Kept, "Passed"), (Retired, "Passed")], [(Kept, "Passed"), (Carrier, "Failed")], RetiredLine);

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"passed at {Sha}, gone now: {Retired}",
            $"retired, and its carrier did not pass: {Retired} -> {Carrier}",
            $"2 passed at {Sha}; 0 pass now that did not",
            $"0 that passed at {Sha} fail now, 1 are gone and 1 retired lines are refused",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_retired_test_whose_carrier_ran_before_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [(Kept, "Passed"), (Retired, "Passed"), (Carrier, "Passed")], [(Kept, "Passed"), (Carrier, "Passed")], RetiredLine);

        Assert.That(read.Said, Is.EqualTo(new[]
        {
            $"passed at {Sha}, gone now: {Retired}",
            $"retired, and its carrier ran at {Sha}: {Retired} -> {Carrier}",
            $"3 passed at {Sha}; 0 pass now that did not",
            $"0 that passed at {Sha} fail now, 1 are gone and 1 retired lines are refused",
        }));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    [RefusedCase]
    public void a_retired_line_that_names_no_carrier_is_refused()
    {
        using var written = new WrittenFolder();

        var read = Read(written, [(Kept, "Passed"), (Retired, "Passed")], [(Kept, "Passed"), (Carrier, "Passed")], Retired + " | M1_F1.X1_1_0\n");

        Assert.That(read.Said, Is.Empty);
        Assert.That(read.Errors, Does.EndWith($"is not '<class.method> | <promise constant> | <class.method that now carries it>': {Retired} | M1_F1.X1_1_0\n"));
        Assert.That(read.ExitCode, Is.EqualTo(1));
    }

    private static (IReadOnlyList<string> Said, string Errors, int ExitCode) Read(
        WrittenFolder written, (string Id, string Outcome)[] before, (string Id, string Outcome)[] after, string retired)
    {
        var start = new ProcessStartInfo("python3", ["-I", Path.Combine(Repository.Root, Reader), Run(written, "before", before), Run(written, "after", after), Sha, written.Write("retired.txt", retired)])
        {
            WorkingDirectory = Repository.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var python = Process.Start(start) ?? throw new InvalidOperationException("python3 could not be started to run " + Reader);
        var output = python.StandardOutput.ReadToEndAsync();
        var errors = python.StandardError.ReadToEndAsync();
        python.WaitForExit();
        return (output.Result.Split('\n', StringSplitOptions.RemoveEmptyEntries), errors.Result, python.ExitCode);
    }

    private static string Run(WrittenFolder written, string folder, (string Id, string Outcome)[] ran)
    {
        var definitions = ran.Select((test, place) =>
        {
            var method = test.Id.LastIndexOf('.');
            return $"<UnitTest name=\"{test.Id[(method + 1)..]}\" id=\"t{place}\"><TestMethod className=\"{System.Security.SecurityElement.Escape(test.Id[..method])}\" name=\"{test.Id[(method + 1)..]}\" /></UnitTest>";
        });
        var results = ran.Select((test, place) => $"<UnitTestResult testId=\"t{place}\" outcome=\"{test.Outcome}\" />");
        var trx = written.Write(
            folder + "/run.trx",
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<TestRun xmlns=\"http://microsoft.com/schemas/VisualStudio/TeamTest/2010\">\n"
            + $"<Results>\n{string.Join("\n", results)}\n</Results>\n<TestDefinitions>\n{string.Join("\n", definitions)}\n</TestDefinitions>\n</TestRun>\n");
        return Path.GetDirectoryName(trx)!;
    }
}
