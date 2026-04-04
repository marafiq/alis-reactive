using System.Diagnostics;
using System.Net.Http;

namespace Alis.Reactive.PlaywrightTests.Support.Infrastructure;

internal sealed class SandboxServerProcess
{
    private readonly string _baseUrl;
    private readonly ServerOutputBuffer _output;
    private Process? _process;

    internal SandboxServerProcess(string baseUrl, ServerOutputBuffer output)
    {
        _baseUrl = baseUrl;
        _output = output;
    }

    internal async Task StartAsync(string projectDirectory)
    {
        _output.Clear();

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --no-build --project \"{projectDirectory}\" --no-launch-profile --urls {_baseUrl}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Environment =
                {
                    ["ASPNETCORE_ENVIRONMENT"] = "Development",
                    ["DOTNET_URLS"] = _baseUrl,
                    ["ALIS_NO_BROADCAST"] = "1"
                }
            }
        };

        _process.Start();
        _process.OutputDataReceived += (_, args) => _output.Record("stdout", args.Data);
        _process.ErrorDataReceived += (_, args) => _output.Record("stderr", args.Data);
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        using var http = new HttpClient();
        for (var attempt = 0; attempt < 60; attempt++)
        {
            if (_process.HasExited)
                throw _output.BuildStartupException("SandboxApp exited before becoming ready.");

            try
            {
                var response = await http.GetAsync(_baseUrl);
                if (response.IsSuccessStatusCode)
                    return;
            }
            catch
            {
            }

            await Task.Delay(500);
        }

        throw _output.BuildStartupException($"Server did not start within 30 seconds at {_baseUrl}.");
    }

    internal void Stop()
    {
        if (_process is not { HasExited: false })
            return;

        _process.Kill(entireProcessTree: true);
        _process.Dispose();
    }

    internal IReadOnlyCollection<string> RecentOutput() => _output.Snapshot();
}
