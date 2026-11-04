using System.Diagnostics;

namespace Alis.Reactive.ArchitectureTests;

internal static class Repository
{
    internal static readonly string Root = Find(AppContext.BaseDirectory);

    private static readonly Lazy<IReadOnlyList<string>> Tracked = new(ListTrackedFiles);

    internal static IReadOnlyList<string> TrackedFiles => Tracked.Value;

    private static string Find(string from)
    {
        for (var directory = new DirectoryInfo(from); directory != null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Alis.Reactive.slnx"))) return directory.FullName;
        throw new InvalidOperationException("The repository root, the folder holding Alis.Reactive.slnx, was not found above " + from);
    }

    private static IReadOnlyList<string> ListTrackedFiles()
    {
        using var git = Process.Start(new ProcessStartInfo("git", "ls-files -z")
        {
            WorkingDirectory = Root,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        }) ?? throw new InvalidOperationException("git could not be started in " + Root);
        var listing = git.StandardOutput.ReadToEnd();
        git.WaitForExit();
        if (git.ExitCode != 0) throw new InvalidOperationException($"git ls-files exited with {git.ExitCode} in {Root}");

        return listing.Split('\0', StringSplitOptions.RemoveEmptyEntries)
            .Where(path => !path.StartsWith(".claude/worktrees/", StringComparison.Ordinal)
                           && !path.StartsWith(".worktrees/", StringComparison.Ordinal))
            .Where(path => File.Exists(Path.Combine(Root, path)))
            .ToList();
    }
}
