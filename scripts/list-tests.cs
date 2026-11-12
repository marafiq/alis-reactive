using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

const string World = "Alis.Reactive.Conformance.World";
const string Mark = "FailsForAttribute";
const string NUnit = "NUnit.Framework";
const string Test = "TestAttribute";

if (args.Length != 1 || !File.Exists(args[0]))
{
    Console.Error.WriteLine("usage: dotnet run scripts/list-tests.cs -- <a built Playwright test assembly>");
    Console.Error.WriteLine("Prints each test of the assembly as its promise, its fixture and its method, parted by tabs.");
    return 2;
}

using var file = File.OpenRead(args[0]);
using var assembly = new PEReader(file);
var metadata = assembly.GetMetadataReader();
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
            listed.Add($"{promise}\t{metadata.GetString(type.Namespace)}.{metadata.GetString(type.Name)}\t{metadata.GetString(method.Name)}");
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

string? TextOf(CustomAttribute attribute)
{
    var value = metadata.GetBlobReader(attribute.Value);
    value.ReadUInt16();
    return value.ReadSerializedString();
}
