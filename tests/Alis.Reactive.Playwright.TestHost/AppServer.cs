using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using NUnit.Framework;

namespace Alis.Reactive.Playwright.TestHost;

/// <summary>
/// Runs, on a free port, the web app a Playwright test assembly is written against: the app whose entry
/// assembly the test project names. NUnit runs a setup fixture only from the test assembly itself, so each
/// test project declares one in its root namespace:
/// <code>[SetUpFixture] public sealed class Sandbox() : AppServer(typeof(Program).Assembly);</code>
/// One app per test assembly: the address every page test opens is the assembly's.
/// </summary>
/// <param name="app">The app's entry assembly, as the test project references it.</param>
/// <param name="environment">Environment variables the app runs with, beside the Development environment.</param>
public abstract class AppServer(Assembly app, params (string Name, string Value)[] environment)
{
    private const int CapturedOutputLines = 200;
    private const int ReadinessAttempts = 60;
    private const int ReadinessDelayMs = 500;
    private static Process? _server;
    private static string? _baseUrl;

    internal static string BaseUrl => _baseUrl ?? throw new InvalidOperationException(
        "No app is running for this test assembly. Declare the app in the test project's root namespace: " +
        "[SetUpFixture] public sealed class App() : AppServer(typeof(Program).Assembly);");

    [OneTimeSetUp]
    public async Task StartApp()
    {
        if (_server is not null)
            throw new InvalidOperationException("An app is already running for this test assembly: declare one AppServer per test project.");

        var name = app.GetName().Name!;
        var url = $"http://localhost:{FreePort()}";
        var output = new OutputTail(CapturedOutputLines);
        var server = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{app.Location}\" --urls {url}",
                WorkingDirectory = ContentRoot(name),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment = { ["ASPNETCORE_ENVIRONMENT"] = "Development" },
            },
        };
        foreach (var (key, value) in environment)
            server.StartInfo.Environment[key] = value;

        TestContext.Progress.WriteLine($"[playwright:app] starting {name} {url}");
        server.OutputDataReceived += (_, e) => output.Capture("out", e.Data);
        server.ErrorDataReceived += (_, e) => output.Capture("err", e.Data);
        _server = server;
        server.Start();
        server.BeginOutputReadLine();
        server.BeginErrorReadLine();

        using var http = new HttpClient();
        if (await AnswersHome(http, server, url, output))
        {
            _baseUrl = url;
            TestContext.Progress.WriteLine($"[playwright:app] ready {name} {url}");
            return;
        }

        StopApp();
        throw new TimeoutException(
            $"{name} did not answer {url}/ within {ReadinessAttempts * ReadinessDelayMs / 1000} seconds.{Environment.NewLine}{output.Render()}");
    }

    [OneTimeTearDown]
    public void StopApp()
    {
        var server = _server;
        _server = null;
        _baseUrl = null;
        if (server is null)
            return;

        try
        {
            if (!server.HasExited)
                server.Kill(entireProcessTree: true);
        }
        finally
        {
            server.Dispose();
        }
    }

    private static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    // The app is ready when its home page answers: the pipeline that serves the page serves its bundles,
    // and the runner refuses to start when a bundle is missing or stale.
    private static async Task<bool> AnswersHome(HttpClient http, Process server, string url, OutputTail output)
    {
        for (var attempt = 0; attempt < ReadinessAttempts; attempt++)
        {
            if (server.HasExited)
                throw new InvalidOperationException($"The app exited before it answered {url}/.{Environment.NewLine}{output.Render()}");

            try
            {
                using var response = await http.GetAsync(url + "/", HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch (HttpRequestException)
            {
                // Not listening yet.
            }

            await Task.Delay(ReadinessDelayMs);
        }

        return false;
    }

    // The app's own folder, where its views and wwwroot are: <name>/<name>.csproj above the test output.
    private static string ContentRoot(string name)
    {
        for (var dir = TestContext.CurrentContext.TestDirectory; dir is not null; dir = Path.GetDirectoryName(dir))
        {
            var project = Path.Combine(dir, name, name + ".csproj");
            if (File.Exists(project))
                return Path.GetDirectoryName(project)!;
        }

        throw new DirectoryNotFoundException(
            $"No {name}/{name}.csproj above {TestContext.CurrentContext.TestDirectory}: an app's folder is named for its assembly.");
    }

    private sealed class OutputTail(int capacity)
    {
        private readonly Queue<string> _lines = new();
        private readonly object _gate = new();

        internal void Capture(string stream, string? line)
        {
            if (line is null) return;

            lock (_gate)
            {
                if (_lines.Count == capacity)
                    _lines.Dequeue();
                _lines.Enqueue($"[{stream}] {line}");
            }
        }

        internal string Render()
        {
            lock (_gate)
            {
                return _lines.Count == 0
                    ? "No app output captured."
                    : "Recent app output:" + Environment.NewLine + string.Join(Environment.NewLine, _lines);
            }
        }
    }
}
