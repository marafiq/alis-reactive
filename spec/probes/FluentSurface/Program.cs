using System.Text.Json;
using Microsoft.CodeAnalysis;

var repository = Path.GetFullPath(args[0]);
var output = Option("--out") ?? throw new ArgumentException("usage: FluentSurface <repository> --out <path> [--plant <file>]");
var plant = Option("--plant");
var cellPlant = plant is not null && Path.GetFileName(plant).Contains(".F2.", StringComparison.Ordinal) ? plant : null;
var surfacePlant = cellPlant is null ? plant : null;

var surface = Project.Compile(Path.Combine(repository, "spec/api/HttpSurface/HttpSurface.csproj"), surfacePlant is null ? [] : [surfacePlant], withGenerated: false);
var views = Project.Compile(Path.Combine(repository, "spec/api/CshtmlStages/CshtmlStages.csproj"), [], withGenerated: true);
var compilations = new[] { ("spec/api/HttpSurface", (Compilation)surface), ("spec/api/CshtmlStages", views) };

foreach (var (project, compilation) in compilations)
{
    var errors = Project.Errors(compilation);
    if (errors.Count == 0) continue;
    Console.Error.WriteLine($"refused: {project} has {errors.Count} compile errors; the first: {errors[0]}");
    return 2;
}

var cellProject = Project.Compile(Path.Combine(repository, "spec/api/Cells/Cells.csproj"), [], withGenerated: false);
var cellCompilation = cellPlant is null ? cellProject : CellMatrix.Planted(cellProject, cellPlant);
var cellErrors = Project.Errors(cellCompilation);
if (cellErrors.Count > 0)
{
    Console.Error.WriteLine($"refused: spec/api/Cells has {cellErrors.Count} compile errors; the first: {cellErrors[0]}");
    return 2;
}
var cells = CellMatrix.Read(cellCompilation, repository);

var poc = surface.SourceModule.ReferencedAssemblySymbols.Single(assembly => assembly.Name == "Alis.Reactive.Poc");
var members = Surface.Read([poc, surface.Assembly, views.Assembly]);
var lambdas = compilations
    .SelectMany(entry => Lambdas.Read(entry.Item2, repository, file => file.StartsWith("spec/api/", StringComparison.Ordinal) && !file.Contains("/obj/", StringComparison.Ordinal)))
    .ToList();
var slots = lambdas
    .Where(parameter => parameter.Framework)
    .GroupBy(parameter => parameter.Slot)
    .OrderBy(group => group.Key, StringComparer.Ordinal)
    .Select(group => new
    {
        Slot = group.Key,
        Delegates = group.Select(parameter => parameter.Delegate).Distinct().Order(StringComparer.Ordinal).ToList(),
        Names = group.GroupBy(parameter => parameter.Name).OrderByDescending(names => names.Count()).ThenBy(names => names.Key, StringComparer.Ordinal)
            .ToDictionary(names => names.Key, names => names.Count()),
    })
    .ToList();

var document = new
{
    Compilations = compilations.Select(entry => new { Project = entry.Item1, Errors = 0, Trees = entry.Item2.SyntaxTrees.Count() }),
    Members = members,
    Lambdas = lambdas,
    Slots = slots,
    Cells = cells,
};
File.WriteAllText(output, JsonSerializer.Serialize(document, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }));
Console.WriteLine($"members {members.Count}, lambda parameters {lambdas.Count}, slots {slots.Count}");
return 0;

string? Option(string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}
