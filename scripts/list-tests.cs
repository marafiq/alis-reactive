using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

const string World = "Alis.Reactive.Conformance.World";
const string Mark = "FailsForAttribute";
const string NUnit = "NUnit.Framework";
const string Test = "TestAttribute";

if (args.Length != 1 || !File.Exists(args[0]))
{
    Console.Error.WriteLine("usage: dotnet run scripts/list-tests.cs -- <a built Playwright test assembly>");
    Console.Error.WriteLine("Prints each test of the assembly as its promise, its fixture, its method and the folder of its fixture's file in the project, parted by tabs.");
    return 2;
}

using var file = File.OpenRead(args[0]);
using var assembly = new PEReader(file);
var metadata = assembly.GetMetadataReader();
if (!assembly.TryOpenAssociatedPortablePdb(args[0], path => File.Exists(path) ? File.OpenRead(path) : null, out var symbolsProvider, out _) || symbolsProvider is null)
{
    Console.Error.WriteLine($"{args[0]} has no portable PDB beside it, which holds the file of each fixture; build the project with one.");
    return 2;
}

using var symbolsOwner = symbolsProvider;
var symbols = symbolsOwner.GetMetadataReader();
var project = $"/{metadata.GetString(metadata.GetAssemblyDefinition().Name)}/";
var listed = new List<string>();

foreach (var typeHandle in metadata.TypeDefinitions)
{
    var type = metadata.GetTypeDefinition(typeHandle);
    foreach (var methodHandle in type.GetMethods())
    {
        var method = metadata.GetMethodDefinition(methodHandle);
        string? promise = null;
        var isATest = false;
        foreach (var attributeHandle in method.GetCustomAttributes())
        {
            var attribute = metadata.GetCustomAttribute(attributeHandle);
            var (space, name) = TypeOf(attribute);
            if (space == NUnit && name == Test) isATest = true;
            if (space == World && name == Mark) promise = TextOf(attribute);
        }

        if (isATest && promise is not null)
            listed.Add($"{promise}\t{metadata.GetString(type.Namespace)}.{metadata.GetString(type.Name)}\t{metadata.GetString(method.Name)}\t{FolderOf(type)}");
    }
}

listed.Sort(StringComparer.Ordinal);
foreach (var line in listed) Console.WriteLine(line);
return 0;

(string Namespace, string Name) TypeOf(CustomAttribute attribute)
{
    var holder = attribute.Constructor.Kind == HandleKind.MemberReference
        ? metadata.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent
        : metadata.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType();
    if (holder.Kind == HandleKind.TypeReference)
    {
        var reference = metadata.GetTypeReference((TypeReferenceHandle)holder);
        return (metadata.GetString(reference.Namespace), metadata.GetString(reference.Name));
    }

    var definition = metadata.GetTypeDefinition((TypeDefinitionHandle)holder);
    return (metadata.GetString(definition.Namespace), metadata.GetString(definition.Name));
}

string FolderOf(TypeDefinition type)
{
    var methods = type.GetMethods().Concat(type.GetNestedTypes().SelectMany(nested => metadata.GetTypeDefinition(nested).GetMethods()));
    var points = methods.SelectMany(method => symbols.GetMethodDebugInformation(method).GetSequencePoints());
    var written = points.Select(point => symbols.GetString(symbols.GetDocument(point.Document).Name).Replace('\\', '/')).First();
    var inTheProject = written.LastIndexOf(project, StringComparison.Ordinal);
    if (inTheProject < 0) throw new InvalidOperationException($"{written} is no file of the project folder {project}.");
    var path = written[(inTheProject + project.Length)..];
    return path.Contains('/') ? path[..path.LastIndexOf('/')] : string.Empty;
}

string? TextOf(CustomAttribute attribute)
{
    var value = metadata.GetBlobReader(attribute.Value);
    value.ReadUInt16();
    return value.ReadSerializedString();
}
