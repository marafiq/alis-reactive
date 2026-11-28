using System.Xml.Linq;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

[TestFixture]
public class WhenAPackageShips
{
    private const string SurfaceFolder = "PublicAPI";
    private const string Table = "SliceTree.txt";
    private const string SliceTree = "AlisSliceTree";
    private const string Condition = "Condition";

    private static readonly string[] ShippedPackages = ["Alis.Reactive", "Alis.Reactive.Fusion", "Alis.Reactive.Native", "Alis.Reactive.OpenTelemetry"];

    private static readonly string[] AWard = ["Wards/PublicAPI/net10.0/PublicAPI.Unshipped.txt", "Wards/SliceTree.txt", "Wards/Wards.csproj"];

    [Test]
    public void every_shipped_package_is_named_and_builds_with_its_slice_table()
        => Assert.That(
            Problems(ShippedPackages, Repository.TrackedFiles, file => File.ReadAllText(Path.Combine(Repository.Root, file))),
            Is.Empty,
            "ALIS009 reads a package's public surface, and ALIS010 to ALIS013 its slices, only where the build hands the compiler its slice table");

    [RefusedCase]
    public void a_package_built_with_the_slice_tree_off_is_refused()
        => Assert.That(
            Problems(["Wards"], AWard, _ => Project("<AlisSliceTree>false</AlisSliceTree>")),
            Is.EqualTo(new[] { "Wards builds with AlisSliceTree off" }));

    [RefusedCase]
    public void a_package_that_never_sets_the_slice_tree_is_refused()
        => Assert.That(Problems(["Wards"], AWard, _ => Project("<IsPackable>true</IsPackable>")), Is.EqualTo(new[] { "Wards builds with AlisSliceTree off" }));

    [RefusedCase]
    public void a_package_that_sets_the_slice_tree_under_a_condition_is_refused()
        => Assert.That(
            Problems(["Wards"], AWard, _ => Project("<AlisSliceTree Condition=\"'$(TargetFramework)' == 'net10.0'\">true</AlisSliceTree>")),
            Is.EqualTo(new[] { "Wards builds with AlisSliceTree off" }));

    [RefusedCase]
    public void a_package_that_sets_the_slice_tree_in_a_group_under_a_condition_is_refused()
        => Assert.That(
            Problems(["Wards"], AWard, _ => "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup Condition=\"'$(Configuration)' == 'Release'\"><AlisSliceTree>true</AlisSliceTree></PropertyGroup></Project>"),
            Is.EqualTo(new[] { "Wards builds with AlisSliceTree off" }));

    [AcceptedCase]
    public void a_named_package_built_with_the_slice_tree_on_that_tracks_its_table_is_accepted()
        => Assert.That(Problems(["Wards"], AWard, _ => Project("<AlisSliceTree>true</AlisSliceTree>")), Is.Empty);

    [RefusedCase]
    public void a_package_that_tracks_no_table_is_refused()
        => Assert.That(
            Problems(["Wards"], ["Wards/PublicAPI/net10.0/PublicAPI.Unshipped.txt", "Wards/Wards.csproj"], _ => Project("<AlisSliceTree>true</AlisSliceTree>")),
            Is.EqualTo(new[] { "Wards tracks no Wards/SliceTree.txt" }));

    [RefusedCase]
    public void a_package_that_tracks_no_project_file_of_its_name_is_refused()
        => Assert.That(
            Problems(["Wards"], ["Wards/PublicAPI/net10.0/PublicAPI.Unshipped.txt", "Wards/SliceTree.txt"], _ => Project("<AlisSliceTree>true</AlisSliceTree>")),
            Is.EqualTo(new[] { "Wards tracks no Wards/Wards.csproj" }));

    [RefusedCase]
    public void a_folder_that_holds_a_public_surface_and_is_not_named_is_refused()
        => Assert.That(
            Problems(["Wards"], [.. AWard, "Rooms/PublicAPI/net48/PublicAPI.Shipped.txt"], _ => Project("<AlisSliceTree>true</AlisSliceTree>")),
            Is.EqualTo(new[] { "Rooms holds a PublicAPI folder and is not named as a shipped package" }));

    [AcceptedCase]
    public void a_public_surface_below_the_first_folder_is_no_shipped_package()
        => Assert.That(Problems(["Wards"], [.. AWard, "poc/Rooms/PublicAPI/net48/PublicAPI.Shipped.txt"], _ => Project("<AlisSliceTree>true</AlisSliceTree>")), Is.Empty);

    [RefusedCase]
    public void a_named_package_that_holds_no_public_surface_is_refused()
        => Assert.That(
            Problems(["Wards"], ["Wards/SliceTree.txt", "Wards/Wards.csproj"], _ => Project("<AlisSliceTree>true</AlisSliceTree>")),
            Is.EqualTo(new[] { "Wards is named as a shipped package and holds no PublicAPI folder" }));

    private static string Project(string property) => $"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup>{property}</PropertyGroup></Project>";

    private static List<string> Problems(IReadOnlyList<string> named, IReadOnlyList<string> tracked, Func<string, string> read)
    {
        var holdingASurface = tracked
            .Select(file => file.Split('/'))
            .Where(parts => parts is [_, SurfaceFolder, ..])
            .Select(parts => parts[0])
            .Distinct()
            .OrderBy(package => package, StringComparer.Ordinal)
            .ToList();
        var problems = holdingASurface.Except(named).Select(package => $"{package} holds a PublicAPI folder and is not named as a shipped package").ToList();
        foreach (var package in named)
        {
            var project = $"{package}/{package}.csproj";
            var table = $"{package}/{Table}";
            if (!holdingASurface.Contains(package)) problems.Add($"{package} is named as a shipped package and holds no PublicAPI folder");
            if (!tracked.Contains(table)) problems.Add($"{package} tracks no {table}");
            if (!tracked.Contains(project)) problems.Add($"{package} tracks no {project}");
            else if (!BuildsWithTheSliceTreeOn(read(project))) problems.Add($"{package} builds with AlisSliceTree off");
        }

        return problems;
    }

    private static bool BuildsWithTheSliceTreeOn(string projectFile)
    {
        var written = XDocument.Parse(projectFile).Descendants(SliceTree).ToList();
        return written.Count > 0 && written.All(property => property.Value.Trim() == "true" && property.Attribute(Condition) is null && property.Parent?.Attribute(Condition) is null);
    }
}
