using System.Diagnostics;
using System.Text.Json;

namespace Alis.Reactive.ArchitectureTests;

internal static class Scripts
{
    private const string Reader = "tests/Alis.Reactive.ArchitectureTests/read-scripts.mjs";
    private static readonly Lazy<ScriptsRead> OfTheRepository = new(
        () => Read(Repository.TrackedFiles.Where(file => Path.GetExtension(file) is ".ts" or ".mjs" or ".js").ToList()));

    internal sealed record ScriptsRead(IReadOnlyCollection<string> Identifiers, IReadOnlyList<string> Code, IReadOnlyList<string> Comments);

    internal static ScriptsRead Tracked => OfTheRepository.Value;

    internal static ScriptsRead Read(IReadOnlyList<string> files)
    {
        var start = new ProcessStartInfo("node", [Path.Combine(Repository.Root, Reader)])
        {
            WorkingDirectory = Repository.Root,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        using var node = Process.Start(start) ?? throw new InvalidOperationException("node could not be started to read " + Reader);
        var output = node.StandardOutput.ReadToEndAsync();
        var errors = node.StandardError.ReadToEndAsync();
        node.StandardInput.Write(string.Join('\n', files));
        node.StandardInput.Close();
        node.WaitForExit();
        if (node.ExitCode != 0)
            throw new InvalidOperationException($"{Reader} exited with {node.ExitCode}, so no TypeScript or JavaScript was read: {errors.Result}");

        using var read = JsonDocument.Parse(output.Result);
        string[] Strings(string property) => read.RootElement.GetProperty(property).EnumerateArray().Select(item => item.GetString()!).ToArray();
        return new ScriptsRead(Strings("identifiers"), Strings("code"), Strings("comments"));
    }
}
