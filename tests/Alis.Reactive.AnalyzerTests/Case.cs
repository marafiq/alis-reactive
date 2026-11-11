using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Alis.Reactive.AnalyzerTests;

internal sealed class Case<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    internal const string Project = "TestProject";
    internal const string Table = "/TestProject/SliceTree.txt";

    public Case()
    {
        ReferenceAssemblies = new ReferenceAssemblies("net10.0");
        ReferTheHostsAssemblies(TestState);
    }

    internal bool AllowsUnsafe { get; init; }

    internal Case<TAnalyzer> WithTable(string rows)
    {
        TestState.AdditionalFiles.Add((Table, rows));
        return this;
    }

    internal Case<TAnalyzer> WithTheSliceTreeOn()
    {
        TestState.AnalyzerConfigFiles.Add(("/.globalconfig", "is_global = true\nbuild_property.AlisSliceTree = true\n"));
        return this;
    }

    internal Case<TAnalyzer> WithFile(string pathFromTheProject, string source)
    {
        TestState.Sources.Add(("/" + Project + "/" + pathFromTheProject, source));
        return this;
    }

    internal Case<TAnalyzer> WithFileAt(string path, string source)
    {
        TestState.Sources.Add((path, source));
        return this;
    }

    internal Case<TAnalyzer> WithAnotherProject(string name, string path, string source)
    {
        var project = TestState.AdditionalProjects[name];
        ReferTheHostsAssemblies(project);
        project.Sources.Add((path, source));
        TestState.AdditionalProjectReferences.Add(name);
        return this;
    }

    internal Case<TAnalyzer> Expecting(params DiagnosticResult[] reports)
    {
        ExpectedDiagnostics.AddRange(reports);
        return this;
    }

    private static void ReferTheHostsAssemblies(ProjectState project)
    {
        var hostAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        foreach (var path in hostAssemblies.Split(Path.PathSeparator)) project.AdditionalReferences.Add(MetadataReference.CreateFromFile(path));
    }

    protected override ParseOptions CreateParseOptions()
        => ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion.CSharp14);

    protected override CompilationOptions CreateCompilationOptions()
        => ((CSharpCompilationOptions)base.CreateCompilationOptions()).WithAllowUnsafe(AllowsUnsafe);
}
