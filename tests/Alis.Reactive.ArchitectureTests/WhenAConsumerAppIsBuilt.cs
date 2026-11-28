using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAConsumerAppIsBuilt
{
    private const string Packs = "nupkgs";
    private const string FreshNet10App = "tests/Alis.Reactive.Consumer.Net10";
    private const string FreshNet48App = "tests/Alis.Reactive.Consumer.Net48";
    private const string BrowserFiles = "staticwebassets/";
    private const string Listening = "Now listening on: ";
    private const string Core = "AlisReactive";
    private const string Components = "AlisReactive.Native";
    private const string TraceSink = "AlisReactive.OpenTelemetry";
    private const string Net10 = "net10.0";
    private const string Net48 = "net48";
    private const string AShiftWhoseValuesShareABit = "[Flags] public enum Shift { Early = 5, Late = 6 }";
    private const string AShiftOfDisjointValues = "[Flags] public enum Shift { Early = 1, Late = 2 }";
    private const string ShiftsValuesShareABit = "RotaRules.cs(17,41): error ALIS038: IsInEnum reads Shift, whose values Early (5) and Late (6) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other";
    private const string NoTargetOfItsOwn = "";
    private const string AProgramThatMapsTheStaticAssets = "using Microsoft.AspNetCore.Builder;\n\nvar app = WebApplication.CreateBuilder(args).Build();\napp.MapStaticAssets();\napp.Run();\n";

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

    [Test]
    public void git_lists_no_file_a_build_of_the_fresh_net10_app_wrote()
    {
        var app = Path.Combine(Repository.Root, FreshNet10App);
        var before = Listed(app);

        BuiltOnThePacks(app);

        Assert.That(
            Listed(app).Except(before).Select(path => $"git lists {path}, which a build of {FreshNet10App} wrote: a package writes only into the app's bin/ and obj/"),
            Is.Empty);
    }

    [Test]
    public void git_lists_no_file_a_build_of_the_fresh_net48_app_wrote()
    {
        var app = Path.Combine(Repository.Root, FreshNet48App);
        var before = Listed(app);

        BuiltByVisualStudiosMsBuildOnThePacks(app);

        Assert.That(
            Listed(app).Except(before).Select(path => $"git lists {path}, which a build of {FreshNet48App} wrote: a package writes only into the folder it owns, Content/<PackageId>/, beside a .gitignore holding *"),
            Is.Empty);
    }

    [Test]
    public async Task the_fresh_net10_apps_static_web_assets_serve_each_packages_browser_file_under_its_content_path()
    {
        var app = Path.Combine(Repository.Root, FreshNet10App);
        BuiltOnThePacks(app);
        var browserFiles = BrowserFilesOfThePacksItRestores(app);

        Assert.That(browserFiles, Is.Not.Empty, $"The packs {FreshNet10App} restores hold no browser file under {BrowserFiles}");
        Assert.That(await WhatTheAppDoesNotServe(app, Path.Combine(app, "bin", "Release", Net10, Path.GetFileName(FreshNet10App) + ".dll"), browserFiles), Is.Empty);
    }

    [RefusedCase]
    public async Task a_net10_app_that_maps_no_static_assets_is_refused_for_each_packages_browser_file_it_does_not_serve()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net10, Core, AShiftOfDisjointValues, NoTargetOfItsOwn);

        Assert.That(
            await WhatTheBuiltAppDoesNotServe(app),
            Is.EqualTo(new[]
            {
                "/_content/AlisReactive/js/alis-reactive-hub.js answers 404: the app's static web assets serve no file of its package there",
                "/_content/AlisReactive/js/alis-reactive.js answers 404: the app's static web assets serve no file of its package there",
            }));
    }

    [AcceptedCase]
    public async Task the_same_net10_app_mapping_its_static_assets_serves_each_packages_browser_file_as_its_pack_holds_it()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net10, Core, AShiftOfDisjointValues, NoTargetOfItsOwn);
        written.Write("App/Program.cs", AProgramThatMapsTheStaticAssets);

        Assert.That(await WhatTheBuiltAppDoesNotServe(app), Is.Empty);
    }

    [RefusedCase]
    public void a_file_an_alis_analyzer_refuses_fails_the_build_of_a_net10_app_that_names_a_consumer_package_alone_with_its_id()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net10, Components, AShiftWhoseValuesShareABit, NoTargetOfItsOwn);

        Assert.That(WhatTheBuildRefuses(app), Is.EqualTo(new[] { ShiftsValuesShareABit }));
    }

    [AcceptedCase]
    public void the_same_net10_app_with_its_enums_values_disjoint_builds()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net10, Components, AShiftOfDisjointValues, NoTargetOfItsOwn);

        Assert.That(WhatTheBuildRefuses(app), Is.Empty);
    }

    [RefusedCase]
    public void a_file_an_alis_analyzer_refuses_fails_the_build_of_a_net48_app_that_names_a_consumer_package_alone_with_its_id()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net48, Components, AShiftWhoseValuesShareABit, NoTargetOfItsOwn);

        Assert.That(WhatTheBuildRefuses(app), Is.EqualTo(new[] { ShiftsValuesShareABit }));
    }

    [AcceptedCase]
    public void the_same_net48_app_with_its_enums_values_disjoint_builds()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net48, Components, AShiftOfDisjointValues, NoTargetOfItsOwn);

        Assert.That(WhatTheBuildRefuses(app), Is.Empty);
    }

    [RefusedCase]
    public void a_build_target_that_writes_a_file_git_lists_is_refused()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net48, TraceSink, AShiftOfDisjointValues, ATargetThatWritesAFileGitLists);

        Assert.That(
            WhatTheBuildsLeave(app),
            Is.EqualTo(new[] { "git lists Content/Rounds/rounds.js, which a build wrote: a package writes only into the folder it owns, Content/<PackageId>/, beside a .gitignore holding *" }));
    }

    [RefusedCase]
    public void a_build_target_that_writes_its_file_on_every_build_is_refused()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net48, TraceSink, AShiftOfDisjointValues, ATargetThatWritesItsFileOnEveryBuild);

        Assert.That(
            WhatTheBuildsLeave(app),
            Is.EqualTo(new[] { "a second build of the unchanged app wrote Content/Rounds/rounds.js again: a package copies a file only when its content changes" }));
    }

    [AcceptedCase]
    public void a_net48_app_on_the_packs_is_left_with_nothing_git_lists_and_each_packages_browser_file_beside_its_ignore_file()
    {
        using var written = new WrittenFolder();
        var app = WriteApp(written, Net48, TraceSink, AShiftOfDisjointValues, NoTargetOfItsOwn);

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
        written.Write("App/Rounds/RotaRules.cs", RotaRules(shift));
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

    private static string RotaRules(string shift) => $$"""
        using System;
        using Alis.Reactive.Validation;

        namespace Rounds
        {
            {{shift}}

            public sealed class Rota
            {
                public Shift Shift { get; set; }
            }

            public sealed class RotaRules : ReactiveValidator<Rota>
            {
                public RotaRules()
                {
                    RuleFor(rota => rota.Shift).IsInEnum();
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

    private static void BuiltOnThePacks(string app)
    {
        var (output, exitCode) = Dotnet(app, "build", "-c", "Release", "-warnaserror", "--no-incremental", "-p:PACKAGE_VERSION=" + ThePacksVersion());
        if (exitCode != 0) throw new InvalidOperationException($"{FreshNet10App} did not build on the packs in {Packs}/:\n{output}");
    }

    private static void BuiltByVisualStudiosMsBuildOnThePacks(string app)
    {
        var (output, exitCode) = Run(app, "msbuild", ["/restore", "/t:Rebuild", "/p:Configuration=Release", "/warnaserror", "/p:PACKAGE_VERSION=" + ThePacksVersion(), "-tl:off", "-nologo", "-nodeReuse:false"]);
        if (exitCode != 0) throw new InvalidOperationException($"{FreshNet48App} did not build on the packs in {Packs}/ by Visual Studio's MSBuild:\n{output}");
    }

    private static IReadOnlyList<(string Address, byte[] Bytes)> BrowserFilesOfThePacksItRestores(string app)
    {
        using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(app, "obj", "project.assets.json")));
        var restored = assets.RootElement.GetProperty("libraries").EnumerateObject()
            .Select(library => library.Name.Split('/'))
            .Where(library => library[0].StartsWith(Core, StringComparison.Ordinal))
            .ToList();
        return restored.SelectMany(library =>
        {
            using var pack = ZipFile.OpenRead(Path.Combine(Repository.Root, Packs, $"{library[0]}.{library[1]}.nupkg"));
            return pack.Entries
                .Where(entry => entry.FullName.StartsWith(BrowserFiles, StringComparison.Ordinal) && entry.Name.Length > 0)
                .Select(entry => ($"/_content/{library[0]}/{entry.FullName[BrowserFiles.Length..]}", BytesOf(entry)))
                .ToList();
        }).OrderBy(file => file.Item1, StringComparer.Ordinal).ToList();
    }

    private static byte[] BytesOf(ZipArchiveEntry entry)
    {
        using var read = entry.Open();
        using var bytes = new MemoryStream();
        read.CopyTo(bytes);
        return bytes.ToArray();
    }

    private static async Task<IReadOnlyList<string>> WhatTheBuiltAppDoesNotServe(string app)
    {
        Restore(app);
        Built(app);
        return await WhatTheAppDoesNotServe(app, Path.Combine(app, "bin", "Debug", Net10, "App.dll"), BrowserFilesOfThePacksItRestores(app));
    }

    private static async Task<IReadOnlyList<string>> WhatTheAppDoesNotServe(string app, string assembly, IReadOnlyList<(string Address, byte[] Bytes)> browserFiles)
    {
        var said = new ConcurrentQueue<string>();
        var listening = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);
        var start = new ProcessStartInfo("dotnet", [assembly, "--urls", "http://127.0.0.1:0"])
        {
            WorkingDirectory = app,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            Environment = { ["ASPNETCORE_ENVIRONMENT"] = "Development" },
        };
        using var server = new Process { StartInfo = start };
        void Heard(string? line)
        {
            if (line is null)
            {
                listening.TrySetException(new InvalidOperationException($"{assembly} ended its output before it said \"{Listening}\":\n{string.Join('\n', said)}"));
                return;
            }

            said.Enqueue(line);
            var at = line.IndexOf(Listening, StringComparison.Ordinal);
            if (at >= 0) listening.TrySetResult(new Uri(line[(at + Listening.Length)..].Trim()));
        }

        server.OutputDataReceived += (_, line) => Heard(line.Data);
        server.ErrorDataReceived += (_, line) => Heard(line.Data);
        server.Start();
        server.BeginOutputReadLine();
        server.BeginErrorReadLine();
        try
        {
            using var browser = new HttpClient { BaseAddress = await listening.Task };
            var unserved = new List<string>();
            foreach (var (address, bytes) in browserFiles)
            {
                using var answer = await browser.GetAsync(address);
                var served = answer.IsSuccessStatusCode ? await answer.Content.ReadAsByteArrayAsync() : [];
                if (!answer.IsSuccessStatusCode) unserved.Add($"{address} answers {(int)answer.StatusCode}: the app's static web assets serve no file of its package there");
                else if (!served.AsSpan().SequenceEqual(bytes)) unserved.Add($"{address} serves other bytes than its package's pack holds");
            }

            return unserved;
        }
        finally
        {
            server.Kill(entireProcessTree: true);
            await server.WaitForExitAsync();
        }
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
