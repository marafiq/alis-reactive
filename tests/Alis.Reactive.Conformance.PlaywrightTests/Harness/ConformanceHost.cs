using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

internal sealed class ConformanceHost
{
    private const string App = "Alis.Reactive.Conformance.App";
    private const string FreshNet10App = "Alis.Reactive.Consumer.Net10";
    private const string Listening = "Now listening on: ";

    private static readonly ConcurrentDictionary<string, Lazy<Task<ConformanceHost>>> Started = new(StringComparer.Ordinal);

    private readonly Process? _server;

    private ConformanceHost(Process? server, Uri address)
    {
        _server = server;
        Address = address;
    }

    internal Uri Address { get; }

    internal static Task<ConformanceHost> Of(string host)
    {
        if (host is not (HostNames.Net48 or HostNames.ConsumerNet48))
        {
            return Started.GetOrAdd(host, name => new Lazy<Task<ConformanceHost>>(() => Start(name))).Value;
        }

        if (!OperatingSystem.IsWindows() || Environment.GetEnvironmentVariable("ALIS_IIS_HOST") != host
            || !Uri.TryCreate(Environment.GetEnvironmentVariable("ALIS_IIS_ADDRESS"), UriKind.Absolute, out var address)
            || !address.IsLoopback || address.Scheme != Uri.UriSchemeHttp)
        {
            throw new InvalidOperationException($"The {host} fixture needs its running IIS Express site from scripts/playwright.sh --host {host}.");
        }

        return Task.FromResult(new ConformanceHost(null, address));
    }

    internal static void StopAll()
    {
        foreach (var started in Started.Values.Where(started => started.IsValueCreated && started.Value.IsCompletedSuccessfully))
        {
            if (started.Value.Result._server is { } server) Stop(server);
        }

        Started.Clear();
    }

    private static Task<ConformanceHost> Start(string host)
        => host == HostNames.ConsumerNet10
            ? Serve(FreshNet10App, [], FreshPages.Home, host)
            : Serve(App, ["--host", host], WorldEndpoints.Ready, host);

    private static async Task<ConformanceHost> Serve(string app, string[] hostArguments, string ready, string host)
    {
        var folder = AppFolder(app);
        var said = new ConcurrentQueue<string>();
        var listening = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);
        var server = new Process
        {
            StartInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = folder,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment = { ["ASPNETCORE_ENVIRONMENT"] = "Development", ["TZ"] = "America/New_York", ["LC_ALL"] = "fr_FR.UTF-8" },
            },
        };
        string[] arguments = [AppAssembly(folder, app), "--urls", "http://127.0.0.1:0", .. hostArguments, "--test-host", Environment.ProcessId.ToString(CultureInfo.InvariantCulture)];
        foreach (var argument in arguments) server.StartInfo.ArgumentList.Add(argument);

        server.OutputDataReceived += (_, line) => Heard(line.Data, said, listening, host);
        server.ErrorDataReceived += (_, line) => Heard(line.Data, said, listening, host);
        server.Start();
        try
        {
            if (OperatingSystem.IsWindows() && Environment.GetEnvironmentVariable("ALIS_SERVER_RECORDS") is { } records)
            {
                var file = Path.Combine(records, $"{server.Id}.{host}.server");
                File.WriteAllText(file, server.StartTime.ToUniversalTime().ToFileTimeUtc().ToString(CultureInfo.InvariantCulture));
            }

            server.BeginOutputReadLine();
            server.BeginErrorReadLine();
            var address = await listening.Task;
            using var answering = new HttpClient { BaseAddress = address };
            using var response = await answering.GetAsync(ready);
            response.EnsureSuccessStatusCode();
            return new ConformanceHost(server, address);
        }
        catch
        {
            Stop(server);
            throw;
        }
    }

    private static void Stop(Process server)
    {
        using (server)
        {
            if (server.HasExited) return;
            server.Kill(entireProcessTree: true);
            if (!server.WaitForExit(TimeSpan.FromSeconds(10))) throw new InvalidOperationException($"Server PID {server.Id} is still alive after termination.");
        }
    }

    private static void Heard(string? line, ConcurrentQueue<string> said, TaskCompletionSource<Uri> listening, string host)
    {
        if (line is null)
        {
            listening.TrySetException(new InvalidOperationException(
                $"The {host} host ended its output before it said \"{Listening}\". It said:{Environment.NewLine}{string.Join(Environment.NewLine, said)}"));
            return;
        }

        said.Enqueue(line);
        var at = line.IndexOf(Listening, StringComparison.Ordinal);
        if (at >= 0) listening.TrySetResult(new Uri(line[(at + Listening.Length)..].Trim()));
    }

    private static string AppFolder(string app)
    {
        for (var folder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory); folder is not null; folder = folder.Parent)
        {
            var project = Path.Combine(folder.FullName, app);
            if (File.Exists(Path.Combine(project, app + ".csproj"))) return project;
        }

        throw new DirectoryNotFoundException($"No {app}/{app}.csproj beside a folder above {TestContext.CurrentContext.TestDirectory}.");
    }

    private static string AppAssembly(string folder, string app)
    {
        var built = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        var assembly = Path.Combine(folder, "bin", built.Parent!.Name, built.Name, app + ".dll");
        return File.Exists(assembly) ? assembly : throw new FileNotFoundException($"{app} is not built at {assembly}: scripts/playwright.sh builds it before the tests that run on it.");
    }
}
