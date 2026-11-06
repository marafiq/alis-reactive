using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Alis.Reactive.Poc.ArchitectureTests;

[TestFixture]
public sealed class WhenTheCoreIsCompiled
{
    private const string Core = "Alis.Reactive";

    private static readonly IReadOnlyDictionary<string, string[]> Reads = new Dictionary<string, string[]>
    {
        ["Plan"] = [],
        ["Writing"] = ["Plan"],
        ["Values"] = ["Plan", "Writing"],
        ["Components"] = ["Plan", "Writing", "Values"],
        ["Pipelines"] = ["Plan", "Writing", "Values", "Components"],
        ["Elements"] = ["Components", "Pipelines"],
        ["Hosting"] = ["Plan", "Writing", "Values", "Components", "Pipelines"],
    };

    [Test]
    public void each_module_reads_only_the_modules_its_row_names()
    {
        var compilation = CoreCompilation();
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.ToString()).ToList();
        Assert.That(errors, Is.Empty, "The core compiles in this test as it does in its build.");

        var crossings = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var tree in compilation.SyntaxTrees)
        {
            var model = compilation.GetSemanticModel(tree);
            var module = ModuleOf(tree);
            foreach (var name in tree.GetRoot().DescendantNodes().OfType<SimpleNameSyntax>())
            {
                var read = ModuleOf(model.GetSymbolInfo(name).Symbol);
                var allowed = read is null || read == module || Reads[module].Contains(read);
                if (!allowed)
                    crossings.Add($"{module} reads {read} at {Path.GetFileName(tree.FilePath)}:{name.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
            }
        }

        Assert.That(crossings, Is.Empty, "A module reads only the modules its row names.");
    }

    private static string ModuleOf(SyntaxTree tree)
    {
        var declared = tree.GetRoot().DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().Single().Name.ToString();
        var module = declared.StartsWith(Core + ".", StringComparison.Ordinal) ? declared[(Core.Length + 1)..] : declared;
        if (!Reads.ContainsKey(module))
            Assert.Fail($"'{declared}' in {Path.GetFileName(tree.FilePath)} is no module of the core. A module is a row of this test, added on purpose.");
        return module;
    }

    private static string? ModuleOf(ISymbol? symbol)
    {
        var declared = symbol?.ContainingNamespace?.ToDisplayString();
        if (declared is null || !declared.StartsWith(Core + ".", StringComparison.Ordinal)) return null;
        var top = declared[(Core.Length + 1)..].Split('.')[0];
        return top;
    }

    private static CSharpCompilation CoreCompilation()
    {
        var folder = Path.Combine(RepositoryRoot(), "poc", "Alis.Reactive.Poc");
        var parse = new CSharpParseOptions(LanguageVersion.CSharp14);
        var trees = Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), parse, path))
            .ToList();
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToImmutableArray();
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable);
        return CSharpCompilation.Create("Alis.Reactive.Poc", trees, references, options);
    }

    private static string RepositoryRoot()
    {
        for (var folder = TestContext.CurrentContext.TestDirectory; folder is not null; folder = Path.GetDirectoryName(folder))
        {
            if (Directory.Exists(Path.Combine(folder, "poc", "Alis.Reactive.Poc")))
                return folder;
        }

        throw new DirectoryNotFoundException("No poc/Alis.Reactive.Poc above the test's folder.");
    }
}
