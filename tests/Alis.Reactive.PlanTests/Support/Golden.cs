using System.Text.Json.Nodes;

namespace Alis.Reactive.PlanTests.Support;

internal static class Golden
{
    internal static readonly string Root = Path.Combine(Repository.Root, "tests", "Alis.Reactive.PlanTests", "Goldens");

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
}
