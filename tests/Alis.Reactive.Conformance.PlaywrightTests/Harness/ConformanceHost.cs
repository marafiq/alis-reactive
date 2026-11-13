using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

internal sealed class ConformanceHost
{
    private const string App = "Alis.Reactive.Conformance.App";
    private const string Listening = "Now listening on: ";

    private static readonly ConcurrentDictionary<string, Lazy<Task<ConformanceHost>>> Started = new(StringComparer.Ordinal);

    private readonly Process _server;

    private ConformanceHost(Process server, Uri address)
    {
        _server = server;
        Address = address;
    }

    internal Uri Address { get; }

    internal static Task<ConformanceHost> Of(string host)
        => Started.GetOrAdd(host, name => new Lazy<Task<ConformanceHost>>(() => Start(name))).Value;

    internal static void StopAll()
    {
        foreach (var started in Started.Values.Where(started => started.IsValueCreated && started.Value.IsCompletedSuccessfully))
        {
            var server = started.Value.Result._server;
            if (!server.HasExited) server.Kill(entireProcessTree: true);
            server.Dispose();
        }

        Started.Clear();
    }

    private static async Task<ConformanceHost> Start(string host)
    {
        var folder = AppFolder();
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
                Environment = { ["ASPNETCORE_ENVIRONMENT"] = "Development", ["TZ"] = "America/New_York" },
            },
        };
        foreach (var argument in new[] { AppAssembly(folder), "--urls", "http://127.0.0.1:0", "--host", host, "--test-host", Environment.ProcessId.ToString(CultureInfo.InvariantCulture) })
            server.StartInfo.ArgumentList.Add(argument);

        server.OutputDataReceived += (_, line) => Heard(line.Data, said, listening, host);
        server.ErrorDataReceived += (_, line) => Heard(line.Data, said, listening, host);
        server.Start();
        server.BeginOutputReadLine();
        server.BeginErrorReadLine();
        return new ConformanceHost(server, await listening.Task);
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

    private static string AppFolder()
    {
        for (var folder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory); folder is not null; folder = folder.Parent)
        {
            var app = Path.Combine(folder.FullName, App);
            if (File.Exists(Path.Combine(app, App + ".csproj"))) return app;
        }

        throw new DirectoryNotFoundException($"No {App}/{App}.csproj beside a folder above {TestContext.CurrentContext.TestDirectory}.");
    }

    private static string AppAssembly(string folder)
    {
        var built = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        var assembly = Path.Combine(folder, "bin", built.Parent!.Name, built.Name, App + ".dll");
        return File.Exists(assembly) ? assembly : throw new FileNotFoundException($"The app is not built at {assembly}: the test project builds it first.");
    }
}
