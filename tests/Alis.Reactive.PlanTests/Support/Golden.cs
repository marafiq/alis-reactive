using System.Text.Json.Nodes;

namespace Alis.Reactive.PlanTests.Support;

/// <summary>
/// The plan a test authored, against the file holding the plan it wrote the last time it was reviewed.
/// A difference is a change of the grammar a page carries, and is read as one. Rewrite the files with
/// <c>ALIS_UPDATE_GOLDENS=1</c>, then review the diff before committing it.
/// </summary>
internal static class Golden
{
    private static readonly string Root = Path.Combine(FindRepository(AppContext.BaseDirectory), "tests", "Alis.Reactive.PlanTests", "Goldens");

    internal static void Matches(JsonNode plan, string fixture, string test)
    {
        var path = Path.Combine(Root, fixture, test + ".json");
        var written = plan.Pretty() + Environment.NewLine;
        if (Environment.GetEnvironmentVariable("ALIS_UPDATE_GOLDENS") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, written);
            return;
        }

        Assert.That(File.Exists(path), Is.True, $"No golden at {path}; write it with ALIS_UPDATE_GOLDENS=1 and review it.");
        Assert.That(written, Is.EqualTo(File.ReadAllText(path)), $"The plan differs from {path}: a change of the grammar the page carries.");
    }

    private static string FindRepository(string from)
    {
        for (var directory = new DirectoryInfo(from); directory != null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "Alis.Reactive.slnx"))) return directory.FullName;
        throw new InvalidOperationException("The repository root, the folder holding Alis.Reactive.slnx, was not found above " + from);
    }
}
