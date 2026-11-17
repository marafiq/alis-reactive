using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenThePackagesAreAudited
{
    private const string Auditor = "scripts/npm-audit.mjs";
    private const string Excused = "GHSA-fc5q-39xc-vjpf";
    private const string NotExcused = "GHSA-2222-3333-4444";

    [AcceptedCase]
    public void the_excused_advisory_passes_while_the_lock_holds_the_latest_version_of_its_package()
    {
        var audited = Audit([(Excused, "picomatch")], "4.0.7", "picomatch@4.0.7");

        Assert.That(audited.ExitCode, Is.Zero, audited.Errors);
        Assert.That(audited.Output, Does.Contain(Excused + " (picomatch 4.0.7, the latest published)"));
    }

    [RefusedCase]
    public void an_advisory_the_check_does_not_excuse_fails_by_its_id()
    {
        var audited = Audit([(Excused, "picomatch"), (NotExcused, "left-pad")], "4.0.7", "picomatch@4.0.7");

        Assert.That(audited.ExitCode, Is.EqualTo(1), audited.Output);
        Assert.That(audited.Errors, Does.Contain(NotExcused + " (left-pad <=4.0.7)"));
    }

    [RefusedCase]
    public void the_excused_advisory_on_another_package_fails_by_its_id()
    {
        var audited = Audit([(Excused, "minimatch")], "4.0.7", "picomatch@4.0.7");

        Assert.That(audited.ExitCode, Is.EqualTo(1), audited.Output);
        Assert.That(audited.Errors, Does.Contain(Excused + " (minimatch <=4.0.7)"));
    }

    [RefusedCase]
    public void the_excused_advisory_fails_once_a_newer_version_of_its_package_is_published()
    {
        var audited = Audit([(Excused, "picomatch")], "4.0.7", "picomatch@4.0.8");

        Assert.That(audited.ExitCode, Is.EqualTo(1), audited.Output);
        Assert.That(audited.Errors, Does.Contain("picomatch 4.0.8 is published and the lock holds 4.0.7"));
    }

    [RefusedCase]
    public void the_excused_advisory_fails_once_npm_audit_no_longer_reports_it()
    {
        var audited = Audit([], "4.0.8", "picomatch@4.0.8");

        Assert.That(audited.ExitCode, Is.EqualTo(1), audited.Output);
        Assert.That(audited.Errors, Does.Contain(Excused + " is no longer reported for picomatch"));
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
