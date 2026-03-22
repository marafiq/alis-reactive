using System.Diagnostics;

namespace Alis.Reactive.PlaywrightTests;

/// <summary>
/// Starts the SandboxApp Kestrel server for Playwright tests.
/// One instance per test run (assembly-level setup).
/// </summary>
[SetUpFixture]
public class WebServerFixture
{
    private static Process? _server;
    public static string BaseUrl { get; private set; } = "";

    [OneTimeSetUp]
    public async Task StartServer()
    {
        var port = 5220;
        BaseUrl = $"http://localhost:{port}";

        var projectDir = FindProjectDir();

        _server = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectDir}\" --no-launch-profile --urls {BaseUrl}",
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

        // Wait for server to be ready (up to 30s)
        using var http = new HttpClient();
        for (var i = 0; i < 60; i++)
        {
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

        throw new Exception($"Server did not start within 30 seconds at {BaseUrl}");
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
}
