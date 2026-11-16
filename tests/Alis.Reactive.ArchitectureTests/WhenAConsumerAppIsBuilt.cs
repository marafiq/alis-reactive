using System.Diagnostics;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAConsumerAppIsBuilt
{
    private const string Packs = "nupkgs";
    private const string Core = "AlisReactive";
    private const string Components = "AlisReactive.Native";
    private const string TraceSink = "AlisReactive.OpenTelemetry";
    private const string Net10 = "net10.0";
    private const string Net48 = "net48";
    private const string AnUnmarkedShift = "public enum Shift { Early = 1, Late = 2 }";
    private const string AMarkedShift = "[Flags] public enum Shift { Early = 1, Late = 2 }";
    private const string ShiftIsNoFlagsEnum = "Startup.cs(12,30): error ALIS038: Flags<Shift>() names Shift as a flags enum, and Shift has no [Flags], so FluentValidation and System.Text.Json read it as one value; mark Shift [Flags], or take the call out";
    private const string NoTargetOfItsOwn = "";

    private const string ATargetThatWritesAFileGitLists = """
        <Target Name="WriteTheRoundsScript" AfterTargets="Build">
          <WriteLinesToFile File="Content/Rounds/rounds.js" Lines="rounds()" Overwrite="true" WriteOnlyWhenDifferent="true" />
        </Target>
        """;

    private const string ATargetThatWritesItsFileOnEveryBuild = """
        <Target Name="WriteTheRoundsScript" AfterTargets="Build">
          <WriteLinesToFile File="Content/Rounds/.gitignore" Lines="*" Overwrite="true" WriteOnlyWhenDifferent="true" />
          <WriteLinesToFile File="Content/Rounds/rounds.js" Lines="rounds()" Overwrite="true" />
        </Target>
        """;

    private static readonly DateTime Untouched = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly string[] VariablesThatPointAtTheRunsOwnFolders = ["NUGET_PACKAGES", "GIT_DIR", "GIT_WORK_TREE", "GIT_INDEX_FILE"];

    [RefusedCase]
    public void a_file_an_alis_analyzer_refuses_fails_the_build_of_a_net10_app_that_names_a_consumer_package_alone_with_its_id()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net10, Components, AnUnmarkedShift, NoTargetOfItsOwn);

        Assert.That(WhatTheBuildRefuses(app), Is.EqualTo(new[] { ShiftIsNoFlagsEnum }));
    }

    [AcceptedCase]
    public void the_same_net10_app_with_its_enum_marked_builds()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net10, Components, AMarkedShift, NoTargetOfItsOwn);

        Assert.That(WhatTheBuildRefuses(app), Is.Empty);
    }

    [RefusedCase]
    public void a_file_an_alis_analyzer_refuses_fails_the_build_of_a_net48_app_that_names_a_consumer_package_alone_with_its_id()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net48, Components, AnUnmarkedShift, NoTargetOfItsOwn);

        Assert.That(WhatTheBuildRefuses(app), Is.EqualTo(new[] { ShiftIsNoFlagsEnum }));
    }

    [AcceptedCase]
    public void the_same_net48_app_with_its_enum_marked_builds()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net48, Components, AMarkedShift, NoTargetOfItsOwn);

        Assert.That(WhatTheBuildRefuses(app), Is.Empty);
    }

    [RefusedCase]
    public void a_build_target_that_writes_a_file_git_lists_is_refused()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net48, TraceSink, AMarkedShift, ATargetThatWritesAFileGitLists);

        Assert.That(
            WhatTheBuildsLeave(app),
            Is.EqualTo(new[] { "git lists Content/Rounds/rounds.js, which a build wrote: a package writes only into the folder it owns, Content/<PackageId>/, beside a .gitignore holding *" }));
    }

    [RefusedCase]
    public void a_build_target_that_writes_its_file_on_every_build_is_refused()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net48, TraceSink, AMarkedShift, ATargetThatWritesItsFileOnEveryBuild);

        Assert.That(
            WhatTheBuildsLeave(app),
            Is.EqualTo(new[] { "a second build of the unchanged app wrote Content/Rounds/rounds.js again: a package copies a file only when its content changes" }));
    }

    [AcceptedCase]
    public void a_net48_app_on_the_packs_is_left_with_nothing_git_lists_and_each_packages_browser_file_beside_its_ignore_file()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net48, TraceSink, AMarkedShift, NoTargetOfItsOwn);

        Assert.That(WhatTheBuildsLeave(app), Is.Empty);
        Assert.That(
            WhatTheContentFolderHolds(app),
            Is.EqualTo(new[]
            {
                "AlisReactive.OpenTelemetry/.gitignore: *",
                "AlisReactive.OpenTelemetry/js/alis-reactive-opentelemetry.js",
                "AlisReactive/.gitignore: *",
                "AlisReactive/js/alis-reactive.js",
            }));
    }

    private static string WriteApp(WrittenFolder written, string framework, string package, string shift, string targetOfItsOwn)
    {
        var app = Path.GetDirectoryName(written.Write("App/App.csproj", Project(framework, package, targetOfItsOwn)))!;
        written.Write("App/nuget.config", TheirOwnNuGetConfig());
        written.Write("App/Directory.Build.props", "<Project />\n");
        written.Write("App/Directory.Build.targets", "<Project />\n");
        written.Write("App/Directory.Packages.props", "<Project>\n  <PropertyGroup>\n    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>\n  </PropertyGroup>\n</Project>\n");
        written.Write("App/.gitignore", "bin/\nobj/\n");
        written.Write("App/Rounds/Startup.cs", Startup(shift));
        if (framework == Net10) written.Write("App/Program.cs", "Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args).Build().Run();\n");
        return app;
    }

    private static string Project(string framework, string package, string targetOfItsOwn)
    {
        var sdk = framework == Net10 ? "Microsoft.NET.Sdk.Web" : "Microsoft.NET.Sdk";
        return $"""
            <Project Sdk="{sdk}">
              <PropertyGroup>
                <TargetFramework>{framework}</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="{package}" Version="{ThePacksVersion()}" />
              </ItemGroup>
            {targetOfItsOwn}
            </Project>
            """;
    }

    private static string Startup(string shift) => $$"""
        using System;
        using Alis.Reactive.Validation;

        namespace Rounds
        {
            {{shift}}

            public static class Startup
            {
                public static void Register(ReactiveFluentValidationBuilder validation)
                {
                    validation.Flags<Shift>();
                }
            }
        }
        """;

    private static string TheirOwnNuGetConfig() => $"""
        <?xml version="1.0" encoding="utf-8"?>
        <configuration>
          <config>
            <add key="globalPackagesFolder" value="obj/packages" />
          </config>
          <packageSources>
            <clear />
            <add key="packs" value="{Path.Combine(Repository.Root, Packs)}" />
            <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
          </packageSources>
          <packageSourceMapping>
            <packageSource key="packs">
              <package pattern="AlisReactive*" />
            </packageSource>
            <packageSource key="nuget.org">
              <package pattern="*" />
            </packageSource>
          </packageSourceMapping>
        </configuration>
        """;

    private static string ThePacksVersion()
    {
        var versions = Directory.EnumerateFiles(Path.Combine(Repository.Root, Packs), Core + ".*.nupkg")
            .Select(Path.GetFileName)
            .Select(name => name![(Core.Length + 1)..^".nupkg".Length])
            .Where(version => char.IsAsciiDigit(version[0]))
            .ToList();
        if (versions.Count != 1)
            throw new InvalidOperationException($"{Packs}/ holds {versions.Count} versions of {Core}, and a case builds on the one a run packed. Run scripts/delivery.sh, which packs at a version of its own and runs these cases on it.");
        return versions[0];
    }

    private static IReadOnlyList<string> WhatTheBuildRefuses(string app)
    {
        Restore(app);
        var (output, _) = Dotnet(app, "build", "--no-restore");
        return output.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Contains(": error ", StringComparison.Ordinal))
            .Select(line => Path.GetFileName(line[..line.LastIndexOf(" [", StringComparison.Ordinal)]))
            .Distinct()
            .ToList();
    }

    private static IReadOnlyList<string> WhatTheBuildsLeave(string app)
    {
        Git(app, "init", "--quiet");
        var before = Listed(app);
        Restore(app);
        Built(app);
        var afterTheFirst = ProjectFiles(app);
        foreach (var path in afterTheFirst) File.SetLastWriteTimeUtc(Path.Combine(app, path), Untouched);
        Built(app);
        var listed = Listed(app).Except(before)
            .Select(path => $"git lists {path}, which a build wrote: a package writes only into the folder it owns, Content/<PackageId>/, beside a .gitignore holding *");
        var writtenAgain = ProjectFiles(app)
            .Where(path => !afterTheFirst.Contains(path) || File.GetLastWriteTimeUtc(Path.Combine(app, path)) != Untouched)
            .Select(path => $"a second build of the unchanged app wrote {path} again: a package copies a file only when its content changes");
        return listed.Concat(writtenAgain).Order(StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<string> WhatTheContentFolderHolds(string app)
    {
        var content = Path.Combine(app, "Content");
        return Directory.EnumerateFiles(content, "*", SearchOption.AllDirectories)
            .Select(file => (Path: Path.GetRelativePath(content, file).Replace('\\', '/'), Full: file))
            .Select(file => Path.GetFileName(file.Path) == ".gitignore" ? $"{file.Path}: {string.Join(", ", File.ReadAllLines(file.Full))}" : file.Path)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static HashSet<string> ProjectFiles(string app)
        => Directory.EnumerateFiles(app, "*", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(app, file).Replace('\\', '/'))
            .Where(path => !path.StartsWith("bin/", StringComparison.Ordinal)
                           && !path.StartsWith("obj/", StringComparison.Ordinal)
                           && !path.StartsWith(".git/", StringComparison.Ordinal))
            .ToHashSet(StringComparer.Ordinal);

    private static HashSet<string> Listed(string app)
    {
        return Git(app, "status", "--porcelain=v1", "--untracked-files=all").Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => line[3..]).ToHashSet(StringComparer.Ordinal);
    }

    private static void Restore(string app)
    {
        var (output, exitCode) = Dotnet(app, "restore", "--configfile", Path.Combine(app, "nuget.config"));
        if (exitCode != 0) throw new InvalidOperationException($"The case's app did not restore from {Packs}/ and nuget.org:\n{output}");
    }

    private static void Built(string app)
    {
        var (output, exitCode) = Dotnet(app, "build", "--no-restore");
        if (exitCode != 0) throw new InvalidOperationException($"The case's app did not build:\n{output}");
    }

    private static (string Output, int ExitCode) Dotnet(string app, params string[] command)
        => Run(app, "dotnet", [.. command, "-tl:off", "-nologo", "--disable-build-servers", "-nodeReuse:false"]);

    private static string Git(string app, params string[] command)
    {
        var (output, exitCode) = Run(app, "git", command);
        if (exitCode != 0) throw new InvalidOperationException($"git {string.Join(' ', command)} exited with {exitCode} in {app}:\n{output}");
        return output;
    }

    private static (string Output, int ExitCode) Run(string app, string program, string[] arguments)
    {
        var start = new ProcessStartInfo(program, arguments)
        {
            WorkingDirectory = app,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var variable in VariablesThatPointAtTheRunsOwnFolders) start.Environment.Remove(variable);
        using var started = Process.Start(start) ?? throw new InvalidOperationException(program + " could not be started");
        var output = started.StandardOutput.ReadToEndAsync();
        var errors = started.StandardError.ReadToEndAsync();
        started.WaitForExit();
        return (output.Result + errors.Result, started.ExitCode);
    }
}
