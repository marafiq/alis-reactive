namespace Alis.Reactive.PlanTests.Support;

/// <summary>The repository the tests run in: the folder holding Alis.Reactive.slnx.</summary>
internal static class Repository
{
    internal static readonly string Root = Find(AppContext.BaseDirectory);

    private static string Find(string from)
    {
        for (var directory = new DirectoryInfo(from); directory != null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Alis.Reactive.slnx"))) return directory.FullName;
        throw new InvalidOperationException("The repository root, the folder holding Alis.Reactive.slnx, was not found above " + from);
    }
}
