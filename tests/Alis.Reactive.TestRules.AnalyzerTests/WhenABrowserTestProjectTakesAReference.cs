using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.TestRules.AnalyzerTests;

[TestFixture]
public sealed class WhenABrowserTestProjectTakesAReference
{
    private const string HandedList = "AlisTestReferences";
    private const string WhatATestTakes = "package:Microsoft.Playwright,package:Microsoft.NET.Test.Sdk,package:NUnit,package:NUnit3TestAdapter,framework:Microsoft.NETCore.App,"
                                          + "analyzer:Alis.Reactive.Analyzers,build:Alis.Reactive.Conformance.App,project:Alis.Reactive.Conformance.World,analyzer:Alis.Reactive.TestRules";

    private const string ATest = "public sealed class WhenARoundStarts { [Test] public void the_round_is_listed() { } }";

    [AcceptedCase]
    public Task the_references_a_test_takes_are_accepted()
        => new Case<TEST001ProjectReferencesWhatATestTakes>()
            .WithTheBuildProperty(HandedList, WhatATestTakes)
            .WithFile("WhenARoundStarts.cs", ATest)
            .RunAsync();

    [RefusedCase]
    public Task a_package_a_test_does_not_take_is_refused_by_its_name()
        => new Case<TEST001ProjectReferencesWhatATestTakes>()
            .WithTheBuildProperty(HandedList, WhatATestTakes + ",package:Moq")
            .WithFile("WhenARoundStarts.cs", ATest)
            .Expecting(Refused("the package", "Moq"))
            .RunAsync();

    [RefusedCase]
    public Task the_framework_and_the_app_taken_for_their_types_are_refused()
        => new Case<TEST001ProjectReferencesWhatATestTakes>()
            .WithTheBuildProperty(HandedList, WhatATestTakes + ",project:Alis.Reactive,project:Alis.Reactive.Conformance.App")
            .WithFile("WhenARoundStarts.cs", ATest)
            .Expecting(Refused("the project", "Alis.Reactive"), Refused("the project", "Alis.Reactive.Conformance.App"))
            .RunAsync();

    [RefusedCase]
    public Task another_framework_another_analyzer_another_build_and_a_file_are_refused()
        => new Case<TEST001ProjectReferencesWhatATestTakes>()
            .WithTheBuildProperty(HandedList, WhatATestTakes + ",framework:Microsoft.AspNetCore.App,analyzer:Wards.Rules,build:Alis.Reactive.Fusion,file:Alis.Reactive")
            .WithFile("WhenARoundStarts.cs", ATest)
            .Expecting(
                Refused("the analyzer", "Wards.Rules"),
                Refused("the project named for its build", "Alis.Reactive.Fusion"),
                Refused("the file", "Alis.Reactive"),
                Refused("the framework", "Microsoft.AspNetCore.App"))
            .RunAsync();

    [RefusedCase]
    public Task a_project_that_hands_the_compiler_no_list_is_refused()
        => new Case<TEST001ProjectReferencesWhatATestTakes>()
            .WithFile("WhenARoundStarts.cs", ATest)
            .Expecting(NoList())
            .RunAsync();

    [RefusedCase]
    public Task a_project_that_hands_the_compiler_an_empty_list_is_refused()
        => new Case<TEST001ProjectReferencesWhatATestTakes>()
            .WithTheBuildProperty(HandedList, "")
            .WithFile("WhenARoundStarts.cs", ATest)
            .Expecting(NoList())
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_left_to_the_compiler()
        => new Case<TEST001ProjectReferencesWhatATestTakes>()
            .WithTheBuildProperty(HandedList, WhatATestTakes)
            .WithFile("WhenARoundStarts.cs", "public sealed class WhenARoundStarts { [Test] public void the_round_is_listed() { {|CS0103:missing|}(); } }")
            .RunAsync();

    private static DiagnosticResult Refused(string kind, string name)
        => new DiagnosticResult(TEST001ProjectReferencesWhatATestTakes.Id, DiagnosticSeverity.Error)
            .WithMessage($"This project references {kind} '{name}'; a browser test compiles against Playwright, NUnit and the world contract alone, so remove it");

    private static DiagnosticResult NoList()
        => new DiagnosticResult(TEST001ProjectReferencesWhatATestTakes.Id, DiagnosticSeverity.Error)
            .WithMessage("'TestProject' takes the test rules and hands the compiler no list of its references; import Alis.Reactive.TestRules.targets after its last reference");
}
