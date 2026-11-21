using System.Diagnostics;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

static class Project
{
    public static CSharpCompilation Compile(string project, IEnumerable<string> extraFiles, bool withGenerated)
    {
        var evaluated = Evaluate(project);
        var folder = Path.GetDirectoryName(project)!;
        var properties = evaluated.GetProperty("Properties");
        var items = evaluated.GetProperty("Items");
        var parse = new CSharpParseOptions(
            Language(properties.GetProperty("LangVersion").GetString()),
            preprocessorSymbols: Split(properties.GetProperty("DefineConstants").GetString()));
        var sources = items.GetProperty("Compile").EnumerateArray()
            .Select(item => Path.GetFullPath(Path.Combine(folder, item.GetProperty("Identity").GetString()!)))
            .Concat(withGenerated ? Generated(folder, properties.GetProperty("IntermediateOutputPath").GetString()!.Replace('\\', '/')) : [])
            .Concat(extraFiles.Select(Path.GetFullPath));
        var trees = sources.Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), parse, path)).ToList();
        var references = items.GetProperty("ReferencePathWithRefAssemblies").EnumerateArray()
            .Select(item => (MetadataReference)MetadataReference.CreateFromFile(item.GetProperty("Identity").GetString()!));
        var options = new CSharpCompilationOptions(
            properties.GetProperty("OutputType").GetString() == "Exe" ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: properties.GetProperty("Nullable").GetString() == "enable" ? NullableContextOptions.Enable : NullableContextOptions.Disable,
            generalDiagnosticOption: properties.GetProperty("TreatWarningsAsErrors").GetString() == "true" ? ReportDiagnostic.Error : ReportDiagnostic.Default,
            specificDiagnosticOptions: Split(properties.GetProperty("NoWarn").GetString())
                .Select(id => id.All(char.IsDigit) ? "CS" + id.PadLeft(4, '0') : id)
                .Distinct()
                .Select(id => new KeyValuePair<string, ReportDiagnostic>(id, ReportDiagnostic.Suppress)));
        return CSharpCompilation.Create(properties.GetProperty("AssemblyName").GetString(), trees, references, options);
    }

    public static IReadOnlyList<Diagnostic> Errors(Compilation compilation) =>
        compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

    static JsonElement Evaluate(string project)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            ArgumentList =
            {
                "msbuild", project, "-nologo",
                "-t:FindReferenceAssembliesForReferences",
                "-getItem:Compile,ReferencePathWithRefAssemblies",
                "-getProperty:Nullable,LangVersion,OutputType,AssemblyName,DefineConstants,TreatWarningsAsErrors,NoWarn,IntermediateOutputPath",
            },
        };
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException("msbuild could not evaluate " + project + ": " + output.Trim());
        return JsonDocument.Parse(output).RootElement.Clone();
    }

    static IEnumerable<string> Generated(string folder, string intermediate) =>
        Directory.Exists(Path.Combine(folder, intermediate, "generated"))
            ? Directory.GetFiles(Path.Combine(folder, intermediate, "generated"), "*.g.cs", SearchOption.AllDirectories).Order(StringComparer.Ordinal)
            : throw new InvalidOperationException("no generated sources under " + Path.Combine(folder, intermediate, "generated") + ": build with -p:EmitCompilerGeneratedFiles=true");

    static LanguageVersion Language(string? text) =>
        LanguageVersionFacts.TryParse(text, out var version) ? version : throw new InvalidOperationException("unknown LangVersion " + text);

    static string[] Split(string? text) =>
        (text ?? "").Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
