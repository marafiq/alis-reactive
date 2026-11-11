using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Analyzers;

[TestFixture]
public sealed class WhenAFileNamesItsNamespace
{
    [RefusedCase]
    public Task a_namespace_of_another_folder_is_refused_at_its_name()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithTable("Schema\n")
            .WithFile("Schema/Node.cs", "namespace {|#0:TestProject.PathsAndWire|}; public sealed class Node;")
            .Expecting(Refused("Schema/Node.cs", "TestProject.PathsAndWire", "TestProject.Schema"))
            .RunAsync();

    [AcceptedCase]
    public Task the_root_namespace_and_the_first_folder_is_accepted()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithTable("Schema\n")
            .WithFile("Schema/Node.cs", "namespace TestProject.Schema; public sealed class Node;")
            .RunAsync();

    [RefusedCase]
    public Task a_namespace_that_follows_a_deeper_folder_is_refused_for_the_first_folder()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithTable("NativeComponents/*\n")
            .WithFile("NativeComponents/TextBox/TextBox.cs", "namespace {|#0:TestProject.NativeComponents.TextBox|} { public sealed class TextBox; }")
            .Expecting(Refused("NativeComponents/TextBox/TextBox.cs", "TestProject.NativeComponents.TextBox", "TestProject.NativeComponents"))
            .RunAsync();

    [AcceptedCase]
    public Task a_file_in_a_deeper_folder_in_its_first_folders_namespace_is_accepted()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithTable("NativeComponents/*\n")
            .WithFile("NativeComponents/TextBox/TextBox.cs", "namespace TestProject.NativeComponents { public sealed class TextBox; }")
            .RunAsync();

    [RefusedCase]
    public Task a_file_beside_the_table_in_a_folders_namespace_is_refused_for_the_root_alone()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithTable(".\n")
            .WithFile("Marker.cs", "namespace {|#0:TestProject.Schema|}; public sealed class Marker;")
            .Expecting(Refused("Marker.cs", "TestProject.Schema", "TestProject"))
            .RunAsync();

    [AcceptedCase]
    public Task a_file_beside_the_table_in_the_root_namespace_is_accepted()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithTable(".\n")
            .WithFile("Marker.cs", "namespace TestProject; public sealed class Marker;")
            .RunAsync();

    [RefusedCase]
    public Task types_declared_in_no_namespace_are_refused_at_the_first_types_name()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithTable("Schema\n")
            .WithFile("Schema/Node.cs", "public sealed class {|#0:Node|}; public sealed class Edge;")
            .Expecting(Refused("Schema/Node.cs", "<global namespace>", "TestProject.Schema"))
            .RunAsync();

    [AcceptedCase]
    public Task a_file_that_declares_no_type_is_accepted()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithTable("Schema\n")
            .WithFile("Schema/Assembly.cs", "[assembly: System.CLSCompliant(false)]")
            .RunAsync();

    [AcceptedCase]
    public Task a_project_that_hands_the_compiler_no_table_is_read_by_no_rule()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithFile("Schema/Node.cs", "namespace TestProject.PathsAndWire; public sealed class Node;")
            .RunAsync();

    [AcceptedCase]
    public Task a_file_under_obj_is_read_by_no_rule()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithTable("Schema\n")
            .WithFile("obj/Debug/Generated.cs", "namespace Anything.At.All; public sealed class Generated;")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => new Case<ALIS011NamespaceIsTheFirstFolder>()
            .WithTable("Schema\n")
            .WithFile("Schema/Node.cs", "namespace TestProject.Schema; public sealed class Node { public {|CS0246:Missing|} Held; }")
            .RunAsync();

    private static DiagnosticResult Refused(string file, string found, string expected)
        => new DiagnosticResult(ALIS011NamespaceIsTheFirstFolder.Id, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithMessage($"'{file}' is in namespace '{found}'; a file's namespace is its project's root namespace and its first folder, '{expected}'");
}
