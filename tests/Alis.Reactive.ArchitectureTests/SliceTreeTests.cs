using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mono.Cecil;
using NUnit.Framework;

namespace Alis.Reactive.ArchitectureTests;

/// <summary>
/// ADR-0004 made mechanical: a package is an inverted dependency tree of concept slices. A slice is a
/// source folder of the package; the rows below are the ADR's tables, bottom up. A slice uses only
/// slices in lower rows, slices in one row never use each other, every folder has a row and every row
/// names a folder. A file that declares a public type has the package's namespace; every other file's
/// namespace is its folder. The type-to-folder map is read from source, and the references from the IL
/// of each target (Mono.Cecil): base types, interfaces, attributes, generic constraints and arguments,
/// fields, properties, events, signatures, locals and the operands of every instruction.
/// </summary>
[TestFixture]
public class SliceTreeTests
{
    // A row entry names a folder. "X/*" makes each immediate subfolder of X a slice of its own, all in
    // that row; "*" does the same at the package root; "." is the package root's own files.
    private sealed record Package(string Name, string[] Targets, string[][] Rows);

    private static readonly string[] BothTargets = ["net48", "net10.0"];

    private static readonly Package[] Packages =
    [
        new("Alis.Reactive", BothTargets,
        [
            ["Grammar", "WireTerms"],
            ["Members"],
            ["Vendors"],
            ["Inputs"],
            ["PlanModel/Writing"],
            ["PlanModel/Values"],
            ["PlanModel/Conditions"],
            ["PlanModel/Reactions", "PlanModel/Validation"],
            ["PlanModel/Requests"],
            ["PlanModel/Document"],
            ["Components"],
            ["PlanAuthoring/Events", "PlanAuthoring/Arrays"],
            ["PlanAuthoring/Elements"],
            ["PlanAuthoring/Pipelines"],
            ["PlanAuthoring/Conditions", "PlanAuthoring/Requests"],
            ["PlanAuthoring"],
            ["InputField"],
            ["Razor"],
            ["Validation"],
        ]),
        new("Alis.Reactive.DesignSystem", BothTargets,
        [
            ["Tokens"],
            ["Layout"],
        ]),
    ];

    public static IEnumerable<TestCaseData> Cases =>
        Packages.SelectMany(p => p.Targets.Select(t => new TestCaseData(p.Name, t).SetArgDisplayNames(p.Name, t)));

    [TestCaseSource(nameof(Cases))]
    public void slices_depend_only_downward_and_files_sit_in_their_namespace(string packageName, string targetFramework)
    {
        var package = Packages.Single(p => p.Name == packageName);
        var repo = RepositoryRoot();
        var packageDir = Path.Combine(repo, package.Name);
        var sources = SourceFiles(packageDir).ToList();
        var folders = sources.Select(s => s.Folder).Distinct(StringComparer.Ordinal).ToList();
        var folderOfType = TypeFolders(sources);

        var violations = new List<string>();

        var rowOf = new Dictionary<string, int>(StringComparer.Ordinal);
        var sliceOfFolder = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var row = 0; row < package.Rows.Length; row++)
        foreach (var entry in package.Rows[row])
        {
            var matched = false;
            foreach (var (folder, slice) in Expand(entry, folders))
            {
                matched = true;
                if (sliceOfFolder.TryGetValue(folder, out var other) && other != slice)
                    throw new InvalidOperationException($"{Display(folder)} falls under {other} and {slice}; the table names it twice.");
                sliceOfFolder[folder] = slice;
                if (rowOf.TryGetValue(slice, out var otherRow) && otherRow != row)
                    throw new InvalidOperationException($"{slice} is in rows {otherRow} and {row}.");
                rowOf[slice] = row;
            }
            if (!matched) violations.Add($"row entry without a folder: {entry}");
        }
        foreach (var folder in folders.Where(f => !sliceOfFolder.ContainsKey(f)))
        {
            var sample = string.Join(", ", sources.Where(s => s.Folder == folder).SelectMany(s => s.TopLevelTypes).Take(3));
            violations.Add($"folder without a row: {Display(folder)} ({sample})");
        }

        foreach (var source in sources)
        {
            var expected = source.DeclaresPublicType || source.Folder.Length == 0
                ? package.Name
                : package.Name + "." + source.Folder.Replace('/', '.');
            if (source.Namespace != expected)
                violations.Add($"namespace: {source.RelativePath} is {source.Namespace}, expected {expected}");
        }

        var dll = Path.Combine(packageDir, "bin", Configuration(), targetFramework, package.Name + ".dll");
        if (!File.Exists(dll))
            Assert.Fail($"{dll} is missing. Build the targets first: dotnet build {package.Name}/{package.Name}.csproj");

        var edges = new Dictionary<(string From, string To), string>();
        using (var module = ModuleDefinition.ReadModule(dll))
        {
            foreach (var type in AllTypes(module))
            {
                var from = FolderOf(type, package.Name, folderOfType, violations);
                if (from == null || !sliceOfFolder.TryGetValue(from, out var fromSlice)) continue;
                foreach (var referenced in ReferencedTypes(type, module))
                {
                    var to = FolderOf(referenced, package.Name, folderOfType, violations);
                    if (to == null || !sliceOfFolder.TryGetValue(to, out var toSlice) || toSlice == fromSlice) continue;
                    edges.TryAdd((fromSlice, toSlice), $"{TopLevel(type).FullName} -> {TopLevel(referenced).FullName}");
                }
            }
        }

        var upward = 0;
        var sameRow = 0;
        foreach (var edge in edges.OrderBy(e => e.Key.From, StringComparer.Ordinal).ThenBy(e => e.Key.To, StringComparer.Ordinal))
        {
            var fromRow = rowOf[edge.Key.From];
            var toRow = rowOf[edge.Key.To];
            if (toRow > fromRow)
            {
                upward++;
                violations.Add($"upward: {edge.Key.From} -> {edge.Key.To} ({edge.Value})");
            }
            else if (toRow == fromRow)
            {
                sameRow++;
                violations.Add($"same row: {edge.Key.From} -> {edge.Key.To} ({edge.Value})");
            }
        }

        var summary = $"ADR-0004 lock ({package.Name}, {targetFramework}): {sources.Count} files in {folders.Count} folders, "
                      + $"{rowOf.Count} slices, {edges.Count} slice edges, {upward} upward, {sameRow} same-row, "
                      + $"{violations.Count(v => v.StartsWith("folder without", StringComparison.Ordinal))} folders without a row, "
                      + $"{violations.Count(v => v.StartsWith("row entry", StringComparison.Ordinal))} row entries without a folder, "
                      + $"{violations.Count(v => v.StartsWith("namespace:", StringComparison.Ordinal))} namespace mismatches.";
        TestContext.Out.WriteLine(summary);
        Assert.That(violations, Is.Empty, summary + "\n" + string.Join("\n", violations));
    }

    // ── the table: a row entry against the folders that exist ───────────────────────────────────────

    private static IEnumerable<(string Folder, string Slice)> Expand(string entry, IEnumerable<string> folders)
    {
        foreach (var folder in folders)
        {
            if (entry == ".")
            {
                if (folder.Length == 0) yield return (folder, ".");
            }
            else if (entry == "*")
            {
                if (folder.Length > 0) yield return (folder, folder.Split('/')[0]);
            }
            else if (entry.EndsWith("/*", StringComparison.Ordinal))
            {
                var parent = entry[..^2] + "/";
                if (folder.StartsWith(parent, StringComparison.Ordinal))
                    yield return (folder, parent + folder[parent.Length..].Split('/')[0]);
            }
            else if (folder == entry)
            {
                yield return (folder, entry);
            }
        }
    }

    private static string Display(string folder) => folder.Length == 0 ? "." : folder;

    // ── source: type -> folder, namespace per file, read through Roslyn's parser ─────────────────────

    private sealed record SourceFile(string RelativePath, string Folder, string Namespace, bool DeclaresPublicType, IReadOnlyList<string> TopLevelTypes);

    // The packages compile for both targets with #if splits, so each file is parsed under each target's
    // symbols and the declarations are the union.
    private static readonly CSharpParseOptions[] Targets =
    [
        new CSharpParseOptions(LanguageVersion.Preview, preprocessorSymbols: ["NET48", "NETFRAMEWORK"]),
        new CSharpParseOptions(LanguageVersion.Preview, preprocessorSymbols: ["NET10_0", "NET10_0_OR_GREATER", "NET6_0_OR_GREATER", "NET"]),
    ];

    private static IEnumerable<SourceFile> SourceFiles(string packageDir)
    {
        foreach (var path in Directory.EnumerateFiles(packageDir, "*.cs", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
        {
            var relative = Path.GetRelativePath(packageDir, path).Replace('\\', '/');
            if (relative.StartsWith("bin/", StringComparison.Ordinal) || relative.StartsWith("obj/", StringComparison.Ordinal)) continue;
            var folder = Path.GetDirectoryName(relative)!.Replace('\\', '/');
            var text = File.ReadAllText(path);
            var namespaces = new HashSet<string>(StringComparer.Ordinal);
            var types = new List<string>();
            var declaresPublic = false;
            foreach (var options in Targets)
            {
                var root = CSharpSyntaxTree.ParseText(text, options, path).GetCompilationUnitRoot();
                foreach (var ns in root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>())
                {
                    namespaces.Add(ns.Name.ToString());
                    foreach (var member in ns.Members)
                    {
                        switch (member)
                        {
                            case TypeDeclarationSyntax type:
                                types.Add(type.Identifier.Text + Arity(type.TypeParameterList));
                                declaresPublic |= type.Modifiers.Any(SyntaxKind.PublicKeyword);
                                break;
                            case EnumDeclarationSyntax @enum:
                                types.Add(@enum.Identifier.Text);
                                declaresPublic |= @enum.Modifiers.Any(SyntaxKind.PublicKeyword);
                                break;
                            case DelegateDeclarationSyntax @delegate:
                                types.Add(@delegate.Identifier.Text + Arity(@delegate.TypeParameterList));
                                declaresPublic |= @delegate.Modifiers.Any(SyntaxKind.PublicKeyword);
                                break;
                        }
                    }
                }
            }
            if (namespaces.Count != 1) throw new InvalidOperationException($"{relative} declares {namespaces.Count} namespaces; a file declares one.");
            if (types.Count == 0) throw new InvalidOperationException($"{relative} declares no type.");
            yield return new SourceFile(relative, folder, namespaces.Single(), declaresPublic, types.Distinct(StringComparer.Ordinal).ToList());
        }
    }

    // The arity suffix IL uses: Condition<TModel> is Condition`1.
    private static string Arity(TypeParameterListSyntax? typeParameters) =>
        typeParameters == null || typeParameters.Parameters.Count == 0 ? "" : "`" + typeParameters.Parameters.Count;

    private static Dictionary<(string Namespace, string Name), string> TypeFolders(IEnumerable<SourceFile> sources)
    {
        var map = new Dictionary<(string, string), string>();
        foreach (var source in sources)
        foreach (var type in source.TopLevelTypes)
        {
            var key = (source.Namespace, type);
            if (map.TryGetValue(key, out var folder) && folder != source.Folder)
                throw new InvalidOperationException($"{source.Namespace}.{type} is declared in {folder} and {source.Folder}.");
            map[key] = source.Folder;
        }
        return map;
    }

    // ── IL: every type a type refers to ─────────────────────────────────────────────────────────────

    private static IEnumerable<TypeDefinition> AllTypes(ModuleDefinition module)
    {
        var pending = new Stack<TypeDefinition>(module.Types);
        while (pending.Count > 0)
        {
            var type = pending.Pop();
            yield return type;
            foreach (var nested in type.NestedTypes) pending.Push(nested);
        }
    }

    private static TypeDefinition TopLevel(TypeDefinition type)
    {
        while (type.DeclaringType != null) type = type.DeclaringType;
        return type;
    }

    private static string? FolderOf(TypeDefinition type, string packageName, Dictionary<(string, string), string> folderOfType, List<string> violations)
    {
        var top = TopLevel(type);
        if (top.Name.StartsWith("<", StringComparison.Ordinal)) return null;
        if (folderOfType.TryGetValue((top.Namespace, top.Name), out var folder)) return folder;
        if (top.Namespace.StartsWith(packageName, StringComparison.Ordinal))
        {
            var message = $"type without a source file: {top.FullName}";
            if (!violations.Contains(message)) violations.Add(message);
        }
        return null;
    }
    private static IEnumerable<TypeDefinition> ReferencedTypes(TypeDefinition type, ModuleDefinition module)
    {
        var seen = new HashSet<TypeDefinition>();
        foreach (var reference in RawReferences(type))
        foreach (var unwrapped in Unwrap(reference))
        {
            if (unwrapped.Scope is not ModuleDefinition scope || scope != module) continue;
            var definition = unwrapped.Resolve();
            if (definition != null && seen.Add(definition)) yield return definition;
        }
    }

    private static IEnumerable<TypeReference> Unwrap(TypeReference reference)
    {
        switch (reference)
        {
            case GenericParameter:
                yield break;
            case GenericInstanceType generic:
                foreach (var argument in generic.GenericArguments)
                foreach (var inner in Unwrap(argument))
                    yield return inner;
                foreach (var inner in Unwrap(generic.ElementType))
                    yield return inner;
                yield break;
            case TypeSpecification specification:
                foreach (var inner in Unwrap(specification.ElementType))
                    yield return inner;
                yield break;
            default:
                yield return reference;
                yield break;
        }
    }

    private static IEnumerable<TypeReference> RawReferences(TypeDefinition type)
    {
        if (type.BaseType != null) yield return type.BaseType;
        foreach (var implemented in type.Interfaces) yield return implemented.InterfaceType;
        foreach (var reference in AttributeReferences(type.CustomAttributes)) yield return reference;
        foreach (var reference in ConstraintReferences(type.GenericParameters)) yield return reference;

        foreach (var field in type.Fields)
        {
            yield return field.FieldType;
            foreach (var reference in AttributeReferences(field.CustomAttributes)) yield return reference;
        }
        foreach (var property in type.Properties)
        {
            yield return property.PropertyType;
            foreach (var reference in AttributeReferences(property.CustomAttributes)) yield return reference;
        }
        foreach (var @event in type.Events)
            yield return @event.EventType;

        foreach (var method in type.Methods)
        {
            yield return method.ReturnType;
            foreach (var parameter in method.Parameters) yield return parameter.ParameterType;
            foreach (var reference in AttributeReferences(method.CustomAttributes)) yield return reference;
            foreach (var reference in AttributeReferences(method.MethodReturnType.CustomAttributes)) yield return reference;
            foreach (var reference in ConstraintReferences(method.GenericParameters)) yield return reference;
            if (!method.HasBody) continue;
            foreach (var variable in method.Body.Variables) yield return variable.VariableType;
            foreach (var instruction in method.Body.Instructions)
            {
                switch (instruction.Operand)
                {
                    case TypeReference typeOperand:
                        yield return typeOperand;
                        break;
                    case MethodReference methodOperand:
                        yield return methodOperand.DeclaringType;
                        yield return methodOperand.ReturnType;
                        foreach (var parameter in methodOperand.Parameters) yield return parameter.ParameterType;
                        if (methodOperand is GenericInstanceMethod genericMethod)
                            foreach (var argument in genericMethod.GenericArguments) yield return argument;
                        break;
                    case FieldReference fieldOperand:
                        yield return fieldOperand.DeclaringType;
                        yield return fieldOperand.FieldType;
                        break;
                }
            }
        }
    }

    private static IEnumerable<TypeReference> AttributeReferences(IEnumerable<CustomAttribute> attributes)
    {
        foreach (var attribute in attributes)
        {
            yield return attribute.AttributeType;
            foreach (var argument in attribute.ConstructorArguments)
                if (argument.Value is TypeReference typeArgument) yield return typeArgument;
            foreach (var named in attribute.Properties.Concat(attribute.Fields))
                if (named.Argument.Value is TypeReference typeArgument) yield return typeArgument;
        }
    }

    private static IEnumerable<TypeReference> ConstraintReferences(IEnumerable<GenericParameter> parameters)
    {
        foreach (var parameter in parameters)
        foreach (var constraint in parameter.Constraints)
            yield return constraint.ConstraintType;
    }

    // ── where the repository and the build are ──────────────────────────────────────────────────────

    private static string RepositoryRoot()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Alis.Reactive", "Alis.Reactive.csproj")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Alis.Reactive/Alis.Reactive.csproj not found above " + TestContext.CurrentContext.TestDirectory);
    }

    // The test runs from tests/.../bin/<Configuration>/net10.0; the core's assemblies sit under the same configuration.
    private static string Configuration()
    {
        var segments = TestContext.CurrentContext.TestDirectory.Split(Path.DirectorySeparatorChar);
        var bin = Array.LastIndexOf(segments, "bin");
        if (bin < 0 || bin + 1 >= segments.Length)
            throw new InvalidOperationException("No bin/<Configuration> segment in " + TestContext.CurrentContext.TestDirectory);
        return segments[bin + 1];
    }
}
