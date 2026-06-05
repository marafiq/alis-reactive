using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Alis.Reactive.PlaywrightTests;

/// <summary>
/// Runs one SandboxApp Kestrel instance on an isolated port for the Playwright test assembly.
/// </summary>
[SetUpFixture]
public class WebServerFixture
{
    private const int CapturedServerOutputLines = 200;
    private const int SandboxReadinessAttempts = 60;
    private const int SandboxReadinessDelayMs = 500;
    private static Process? _server;
    private static readonly string[] RequiredBootAssets =
    [
        "/",
        "/vendor/syncfusion/dist/ej2.min.js",
        "/scripts/alis-reactive.dev.js",
        "/css/design-system.dev.css",
        "/css/syncfusion.dev.css",
        "/css/sandbox.css",
        "/js/disable-sf-animations.js",
        "/js/sandbox-plugins.js"
    ];

    public static string BaseUrl { get; private set; } = "";

    [OneTimeSetUp]
    public async Task StartServer()
    {
        var port = GetAvailablePort();
        BaseUrl = $"http://localhost:{port}";

        var projectDir = FindProjectDir();
        var sandboxAssembly = FindSandboxAssembly();
        var output = new ServerOutputBuffer(CapturedServerOutputLines);

        _server = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{sandboxAssembly}\" --urls {BaseUrl}",
                WorkingDirectory = projectDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment =
                {
                    ["ASPNETCORE_ENVIRONMENT"] = "Development",
                    ["DOTNET_URLS"] = BaseUrl,
                    ["ALIS_NO_BROADCAST"] = "1"
                }
            }
        };

        TestContext.Progress.WriteLine($"[playwright:sandbox] starting {BaseUrl}");
        _server.OutputDataReceived += (_, e) => output.Capture("out", e.Data);
        _server.ErrorDataReceived += (_, e) => output.Capture("err", e.Data);
        _server.Start();
        _server.BeginOutputReadLine();
        _server.BeginErrorReadLine();

        using var http = new HttpClient();
        if (await WaitForSandboxReadiness(http, output))
        {
            TestContext.Progress.WriteLine($"[playwright:sandbox] ready {BaseUrl}");
            return;
        }

        StopServer();
        throw new Exception(
            $"Server did not start within 30 seconds at {BaseUrl}.{Environment.NewLine}{output.Render()}");
    }

    [OneTimeTearDown]
    public void StopServer()
    {
        var server = _server;
        _server = null;

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

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static async Task<bool> WaitForSandboxReadiness(HttpClient http, ServerOutputBuffer output)
    {
        for (var readinessAttempt = 0; readinessAttempt < SandboxReadinessAttempts; readinessAttempt++)
        {
            if (_server is { HasExited: true })
                throw new Exception(
                    $"Server exited before it started at {BaseUrl}.{Environment.NewLine}{output.Render()}");

            if (await AllBootAssetsRespond(http))
                return true;

            await Task.Delay(SandboxReadinessDelayMs);
        }

        return false;
    }

    private static async Task<bool> AllBootAssetsRespond(HttpClient http)
    {
        foreach (var asset in RequiredBootAssets)
        {
            if (!await BootAssetResponds(http, asset))
                return false;
        }

        return true;
    }

    private static async Task<bool> BootAssetResponds(HttpClient http, string assetPath)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + assetPath);
            using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string FindProjectDir()
    {
        var dir = TestContext.CurrentContext.TestDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "Alis.Reactive.SandboxApp", "Alis.Reactive.SandboxApp.csproj");
            if (File.Exists(candidate))
                return Path.GetDirectoryName(candidate)!;
            dir = Path.GetDirectoryName(dir);
        }

        // Fallback supports local runs launched from the repo root instead of test output.
        var repoRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
        return Path.Combine(repoRoot, "Alis.Reactive.SandboxApp");
    }

    private static string FindSandboxAssembly()
    {
        var candidate = Path.Combine(TestContext.CurrentContext.TestDirectory, "Alis.Reactive.SandboxApp.dll");
        if (File.Exists(candidate))
            return candidate;

        throw new FileNotFoundException(
            "The Playwright test output is missing Alis.Reactive.SandboxApp.dll. " +
            "Build tests/Alis.Reactive.PlaywrightTests before running Playwright tests.",
            candidate);
    }
}

internal sealed class ServerOutputBuffer
{
    private readonly int _capacity;
    private readonly Queue<string> _lines = new();
    private readonly object _gate = new();

    internal ServerOutputBuffer(int capacity)
    {
        _capacity = capacity;
    }

    internal void Capture(string stream, string? line)
    {
        if (line == null) return;

        lock (_gate)
        {
            if (_lines.Count == _capacity)
                _lines.Dequeue();

            _lines.Enqueue($"[{stream}] {line}");
        }
    }

    internal string Render()
    {
        lock (_gate)
        {
            if (_lines.Count == 0)
                return "No server output captured.";

            return "Recent server output:" + Environment.NewLine + string.Join(Environment.NewLine, _lines);
        }
    }
}
