using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace Alis.Reactive.TestRules.AnalyzerTests;

internal sealed class Case<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    internal const string Project = "TestProject";

    private const string Usings = "global using System;\nglobal using System.Threading;\nglobal using System.Threading.Tasks;\nglobal using System.Text.RegularExpressions;\n"
                                  + "global using Microsoft.Playwright;\nglobal using NUnit.Framework;\nglobal using Alis.Reactive.Conformance.World;\n"
                                  + "global using static Microsoft.Playwright.Assertions;\n";

    private static readonly string[] WhatATestTakes = ["Microsoft.Playwright.dll", "nunit.framework.dll", "Alis.Reactive.Conformance.World.dll"];

    public Case()
    {
        ReferenceAssemblies = new ReferenceAssemblies("net10.0");
        var hostAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        foreach (var path in hostAssemblies.Split(Path.PathSeparator)) TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(path));
        TestState.Sources.Add(("/" + Project + "/Usings.cs", Usings));
    }

    internal Case<TAnalyzer> WithFile(string pathFromTheProject, string source)
    {
        TestState.Sources.Add(("/" + Project + "/" + pathFromTheProject, source));
        return this;
    }

    internal Case<TAnalyzer> WithTheBuildProperty(string name, string value)
    {
        TestState.AnalyzerConfigFiles.Add(("/.globalconfig", "is_global = true\nbuild_property." + name + " = " + value + "\n"));
        return this;
    }

    internal Case<TAnalyzer> Without(string assembly)
    {
        TestState.AdditionalReferences.RemoveAll(reference => string.Equals(Path.GetFileName(reference.Display), assembly, StringComparison.Ordinal));
        if (WhatATestTakes.Contains(assembly, StringComparer.Ordinal))
        {
            var kept = string.Join("\n", TestState.Sources[0].content.ToString().Split('\n').Where(line => !line.Contains(Namespace(assembly), StringComparison.Ordinal)));
            TestState.Sources[0] = (TestState.Sources[0].filename, SourceText.From(kept));
        }

        return this;
    }

    internal Case<TAnalyzer> Expecting(params DiagnosticResult[] reports)
    {
        ExpectedDiagnostics.AddRange(reports);
        return this;
    }

    private static string Namespace(string assembly) => assembly switch
    {
        "Microsoft.Playwright.dll" => "Microsoft.Playwright",
        "nunit.framework.dll" => "NUnit.Framework",
        _ => "Alis.Reactive.Conformance.World",
    };

    protected override ParseOptions CreateParseOptions()
        => ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion.CSharp14);
}
