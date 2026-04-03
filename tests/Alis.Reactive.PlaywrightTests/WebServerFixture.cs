using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Alis.Reactive.PlaywrightTests;

/// <summary>
/// Starts the SandboxApp Kestrel server for Playwright tests.
/// One instance per test run (assembly-level setup).
/// Uses a random available port so parallel sessions don't collide.
/// </summary>
[SetUpFixture]
public class WebServerFixture
{
    private static Process? _server;
    private static readonly object _serverOutputLock = new();
    private static readonly Queue<string> _serverOutput = new();
    private const int MaxServerOutputLines = 200;
    public static string BaseUrl { get; private set; } = "";

    [OneTimeSetUp]
    public async Task StartServer()
    {
        var port = GetAvailablePort();
        BaseUrl = $"http://127.0.0.1:{port}";

        var projectDir = FindProjectDir();
        ClearServerOutput();

        _server = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --no-build --project \"{projectDir}\" --no-launch-profile --urls {BaseUrl}",
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

        _server.Start();
        _server.OutputDataReceived += (_, args) => RecordServerOutput("stdout", args.Data);
        _server.ErrorDataReceived += (_, args) => RecordServerOutput("stderr", args.Data);
        _server.BeginOutputReadLine();
        _server.BeginErrorReadLine();

        // Wait for server to be ready (up to 30s)
        using var http = new HttpClient();
        for (var i = 0; i < 60; i++)
        {
            if (_server.HasExited)
                throw BuildServerStartupException("SandboxApp exited before becoming ready.");

            try
            {
                var response = await http.GetAsync(BaseUrl);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch
            {
                // Server not ready yet
            }
            await Task.Delay(500);
        }

        throw BuildServerStartupException($"Server did not start within 30 seconds at {BaseUrl}.");
    }

    [OneTimeTearDown]
    public void StopServer()
    {
        if (_server is { HasExited: false })
        {
            _server.Kill(entireProcessTree: true);
            _server.Dispose();
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

    private static string FindProjectDir()
    {
        // Walk up from test output directory to find the SandboxApp project
        var dir = TestContext.CurrentContext.TestDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, "Alis.Reactive.SandboxApp", "Alis.Reactive.SandboxApp.csproj");
            if (File.Exists(candidate))
                return Path.GetDirectoryName(candidate)!;
            dir = Path.GetDirectoryName(dir);
        }

        // Fallback: relative from repo root
        var repoRoot = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));
        return Path.Combine(repoRoot, "Alis.Reactive.SandboxApp");
    }

    private static void RecordServerOutput(string stream, string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

        lock (_serverOutputLock)
        {
            _serverOutput.Enqueue($"[{stream}] {line}");
            while (_serverOutput.Count > MaxServerOutputLines)
                _serverOutput.Dequeue();
        }
    }

    private static void ClearServerOutput()
    {
        lock (_serverOutputLock)
        {
            _serverOutput.Clear();
        }
    }

    private static Exception BuildServerStartupException(string message)
    {
        var output = SnapshotServerOutput();
        if (output.Count == 0)
            return new Exception(message);

        var builder = new StringBuilder()
            .AppendLine(message)
            .AppendLine("Recent SandboxApp output:");

        foreach (var line in output)
            builder.AppendLine(line);

        return new Exception(builder.ToString());
    }

    private static IReadOnlyCollection<string> SnapshotServerOutput()
    {
        lock (_serverOutputLock)
        {
            return _serverOutput.ToArray();
        }
    }
}
