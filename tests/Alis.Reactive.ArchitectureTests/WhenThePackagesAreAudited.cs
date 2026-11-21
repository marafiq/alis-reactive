using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenThePackagesAreAudited
{
    private const string Auditor = "scripts/npm-audit.mjs";
    private const string Retired = "GHSA-fc5q-39xc-vjpf";
    private const string NotExcused = "GHSA-2222-3333-4444";

    [AcceptedCase]
    public void a_report_with_no_advisories_passes()
    {
        var audited = Audit([], "4.0.7", "picomatch@4.0.7");

        Assert.That(audited.ExitCode, Is.Zero, audited.Errors);
    }

    [RefusedCase]
    public void an_advisory_the_check_does_not_excuse_fails_by_its_id()
    {
        var audited = Audit([(NotExcused, "left-pad")], "4.0.7", "picomatch@4.0.7");

        Assert.That(audited.ExitCode, Is.EqualTo(1), audited.Output);
        Assert.That(audited.Errors, Does.Contain(NotExcused + " (left-pad <=4.0.7)"));
    }

    [RefusedCase]
    public void a_retired_advisory_fails_if_npm_reports_it_again()
    {
        var audited = Audit([(Retired, "picomatch")], "4.0.7", "picomatch@4.0.7");

        Assert.That(audited.ExitCode, Is.EqualTo(1), audited.Output);
        Assert.That(audited.Errors, Does.Contain(Retired + " (picomatch <=4.0.7)"));
    }

    private static (string Output, string Errors, int ExitCode) Audit((string Id, string Package)[] advisories, string lockedVersion, string latest)
    {
        var folder = Directory.CreateTempSubdirectory("npm-audit-").FullName;
        var report = Path.Combine(folder, "report.json");
        var lockFile = Path.Combine(folder, "package-lock.json");
        File.WriteAllText(report, JsonSerializer.Serialize(Report(advisories)));
        File.WriteAllText(lockFile, JsonSerializer.Serialize(Lock(lockedVersion)));
        try
        {
            return Run([Path.Combine(Repository.Root, Auditor), "--report", report, "--lock", lockFile, "--latest", latest]);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static Dictionary<string, object> Report((string Id, string Package)[] advisories) => new()
    {
        ["auditReportVersion"] = 2,
        ["vulnerabilities"] = advisories.ToDictionary(
            advisory => advisory.Package,
            advisory => new
            {
                name = advisory.Package,
                via = new[]
                {
                    new { name = advisory.Package, title = "a planted advisory", url = "https://github.com/advisories/" + advisory.Id, range = "<=4.0.7" },
                },
            }),
    };

    private static Dictionary<string, object> Lock(string version) => new()
    {
        ["packages"] = new Dictionary<string, object>
        {
            [""] = new { name = "alis-reactive" },
            ["node_modules/picomatch"] = new { version },
        },
    };

    private static (string Output, string Errors, int ExitCode) Run(string[] arguments)
    {
        var start = new ProcessStartInfo("node", arguments)
        {
            WorkingDirectory = Repository.Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var started = Process.Start(start) ?? throw new InvalidOperationException("node could not be started to run " + Auditor);
        var output = started.StandardOutput.ReadToEndAsync();
        var errors = started.StandardError.ReadToEndAsync();
        started.WaitForExit();
        return (output.Result, errors.Result, started.ExitCode);
    }
}
