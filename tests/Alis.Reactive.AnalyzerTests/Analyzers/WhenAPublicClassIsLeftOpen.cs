using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Analyzers;

[TestFixture]
public sealed class WhenAPublicClassIsLeftOpen
{
    [RefusedCase]
    public Task a_public_class_nothing_derives_from_is_refused_at_its_name()
        => InSchema("public class {|#0:Node|};", Refused("TestProject.Schema.Node"));

    [RefusedCase]
    public Task a_public_record_nothing_derives_from_is_refused_at_its_name()
        => InSchema("public record {|#0:Node|}(int Size);", Refused("TestProject.Schema.Node"));

    [AcceptedCase]
    public Task a_sealed_class_is_accepted() => InSchema("public sealed class Node;");

    [AcceptedCase]
    public Task an_abstract_class_is_accepted() => InSchema("public abstract class Node;");

    [AcceptedCase]
    public Task a_static_class_is_accepted() => InSchema("public static class Node;");

    [AcceptedCase]
    public Task an_internal_class_is_accepted() => InSchema("internal class Node;");

    [AcceptedCase]
    public Task a_class_a_type_of_its_project_derives_from_is_accepted()
        => InSchema("public class Node; public sealed class Leaf : Node;");

    [AcceptedCase]
    public Task a_generic_class_a_type_of_its_project_derives_from_is_accepted()
        => InSchema("public class Node<T>; public sealed class Leaf : Node<int>;");

    [AcceptedCase]
    public Task a_project_that_hands_the_compiler_no_table_is_read_by_no_rule()
        => new Case<ALIS012PublicClassIsSealed>()
            .WithFile("Schema/Node.cs", "namespace TestProject.Schema; public class Node;")
            .RunAsync();

    [AcceptedCase]
    public Task a_file_under_obj_is_read_by_no_rule()
        => new Case<ALIS012PublicClassIsSealed>()
            .WithTable("Schema\n")
            .WithFile("obj/Debug/Generated.cs", "namespace TestProject.Schema; public class Generated;")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => InSchema("public sealed class Node { public {|CS0246:Missing|} Held; }");

    private static Task InSchema(string declarations, params DiagnosticResult[] reports)
        => new Case<ALIS012PublicClassIsSealed>()
            .WithTable("Schema\n")
            .WithFile("Schema/Node.cs", "namespace TestProject.Schema; " + declarations)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult Refused(string named)
        => new DiagnosticResult(ALIS012PublicClassIsSealed.Id, DiagnosticSeverity.Error)
            .WithLocation(0)
            .WithMessage($"'{named}' is public and nothing in the package derives from it; seal it");
}
