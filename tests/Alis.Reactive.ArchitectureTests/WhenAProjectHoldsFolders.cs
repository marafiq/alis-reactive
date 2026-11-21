using System.Xml.Linq;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAProjectHoldsFolders
{
    private const string Solution = "Alis.Reactive.slnx";
    private const string Table = "SliceTree.txt";
    private const string SliceTree = "AlisSliceTree";
    private const string Condition = "Condition";

    private static readonly string[] BuildOutput = ["bin", "obj"];

    private static readonly Dictionary<string, string> CannotCarryATable = new(StringComparer.Ordinal)
    {
        ["Alis.Reactive.Analyzers"] = "an analyzer cannot load into its own build",
    };

    private static readonly Dictionary<string, string> EveryProjectCarriesOne = new(StringComparer.Ordinal);

    private static readonly string[] AWard = ["Wards/Wards.csproj", "Wards/Nurse.cs", "Wards/Rounds/Round.cs"];

    [Test]
    public void every_project_of_the_solution_that_holds_folders_builds_with_its_slice_table()
    {
        var projects = ProjectsOf(File.ReadAllText(Path.Combine(Repository.Root, Solution)));

        Assert.That(projects, Is.Not.Empty, "No project was found in " + Solution);
        Assert.That(
            Problems(projects, Repository.TrackedFiles, file => File.ReadAllText(Path.Combine(Repository.Root, file)), CannotCarryATable),
            Is.Empty,
            "A project that holds folders is vertically sliced: its SliceTree.txt orders them, and ALIS010 to ALIS013 hold it only where the build hands the compiler that table");
    }

    [RefusedCase]
    public void a_project_with_a_folder_of_csharp_and_no_table_is_refused()
        => Assert.That(
            Problems(["Wards/Wards.csproj"], AWard, _ => Project("<AlisSliceTree>true</AlisSliceTree>"), EveryProjectCarriesOne),
            Is.EqualTo(new[] { "Wards holds Rounds and tracks no Wards/SliceTree.txt" }));

    [RefusedCase]
    public void a_project_with_a_folder_of_csharp_built_with_the_slice_tree_off_is_refused()
        => Assert.That(
            Problems(["Wards/Wards.csproj"], [.. AWard, "Wards/SliceTree.txt"], _ => Project("<IsPackable>false</IsPackable>"), EveryProjectCarriesOne),
            Is.EqualTo(new[] { "Wards holds Rounds and builds with AlisSliceTree off" }));

    [RefusedCase]
    public void a_project_that_sets_the_slice_tree_under_a_condition_is_refused()
        => Assert.That(
            Problems(["Wards/Wards.csproj"], [.. AWard, "Wards/SliceTree.txt"], _ => Project("<AlisSliceTree Condition=\"'$(Configuration)' == 'Release'\">true</AlisSliceTree>"), EveryProjectCarriesOne),
            Is.EqualTo(new[] { "Wards holds Rounds and builds with AlisSliceTree off" }));

    [RefusedCase]
    public void a_listed_project_that_holds_no_folder_or_is_in_no_solution_is_refused()
        => Assert.That(
            Problems(
                ["Wards/Wards.csproj"],
                ["Wards/Wards.csproj", "Wards/Nurse.cs"],
                _ => Project("<IsPackable>false</IsPackable>"),
                new Dictionary<string, string> { ["Wards"] = "it is flat", ["Rooms"] = "it is gone" }),
            Is.EqualTo(new[]
            {
                "Rooms is listed as unable to carry a slice table and is no project of the solution; delete it from the list",
                "Wards is listed as unable to carry a slice table and holds no folder of C#; delete it from the list",
            }));

    [AcceptedCase]
    public void a_flat_project_a_sliced_project_and_a_listed_project_are_accepted()
        => Assert.That(
            Problems(
                ["Wards/Wards.csproj", "tests/Wards.Tests/Wards.Tests.csproj", "Wards.Analyzers/Wards.Analyzers.csproj"],
                [
                    .. AWard, "Wards/SliceTree.txt", "Wards/bin/Debug/Built.cs", "Wards/Rounds/notes.txt",
                    "tests/Wards.Tests/Wards.Tests.csproj", "tests/Wards.Tests/WhenARoundStarts.cs", "tests/Wards.Tests/Cases/round.ts",
                    "tests/Wards.Tests/obj/Release/Generated.cs",
                    "Wards.Analyzers/Wards.Analyzers.csproj", "Wards.Analyzers/Rounds/WARD001.cs",
                ],
                file => Project(file == "Wards/Wards.csproj" ? "<AlisSliceTree>true</AlisSliceTree>" : "<IsPackable>false</IsPackable>"),
                new Dictionary<string, string> { ["Wards.Analyzers"] = "an analyzer cannot load into its own build" }),
            Is.Empty);

    private static string Project(string property) => $"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup>{property}</PropertyGroup></Project>";

    private static List<string> ProjectsOf(string solution)
        => XDocument.Parse(solution).Descendants("Project").Select(project => project.Attribute("Path")?.Value ?? "").Where(path => path.Length > 0).ToList();

    private static List<string> Problems(
        IReadOnlyList<string> projects, IReadOnlyList<string> tracked, Func<string, string> read, IReadOnlyDictionary<string, string> cannotCarryATable)
    {
        var problems = new List<string>();
        var named = projects.ToDictionary(project => Path.GetFileNameWithoutExtension(project), StringComparer.Ordinal);
        problems.AddRange(cannotCarryATable.Keys.Where(listed => !named.ContainsKey(listed)).Order(StringComparer.Ordinal)
            .Select(listed => $"{listed} is listed as unable to carry a slice table and is no project of the solution; delete it from the list"));

        foreach (var (name, project) in named.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            var folder = project[..(project.LastIndexOf('/') + 1)];
            var held = FoldersOfCSharp(folder, tracked);
            if (cannotCarryATable.ContainsKey(name))
            {
                if (held.Count == 0) problems.Add($"{name} is listed as unable to carry a slice table and holds no folder of C#; delete it from the list");
                continue;
            }

            if (held.Count == 0) continue;

            var folders = string.Join(", ", held);
            if (!tracked.Contains(folder + Table)) problems.Add($"{name} holds {folders} and tracks no {folder}{Table}");
            else if (!BuildsWithTheSliceTreeOn(read(project))) problems.Add($"{name} holds {folders} and builds with AlisSliceTree off");
        }

        return problems;
    }

    private static List<string> FoldersOfCSharp(string folder, IReadOnlyList<string> tracked)
        => tracked
            .Where(file => file.StartsWith(folder, StringComparison.Ordinal) && file.EndsWith(".cs", StringComparison.Ordinal))
            .Select(file => file[folder.Length..].Split('/'))
            .Where(parts => parts.Length > 1 && !BuildOutput.Contains(parts[0]))
            .Select(parts => parts[0])
            .Distinct()
            .Order(StringComparer.Ordinal)
            .ToList();

    private static bool BuildsWithTheSliceTreeOn(string projectFile)
    {
        var written = XDocument.Parse(projectFile).Descendants(SliceTree).ToList();
        return written.Count > 0 && written.All(property => property.Value.Trim() == "true" && property.Attribute(Condition) is null && property.Parent?.Attribute(Condition) is null);
    }
}
