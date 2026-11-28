using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Analyzers;

[TestFixture]
public sealed class WhenATableDisagreesWithItsProject
{
    private const string Node = "namespace TestProject.Schema; public sealed class Node;";
    private const string Reader = "namespace TestProject.TypeKnowledge; public sealed class Reader;";

    [RefusedCase]
    public Task an_entry_that_names_no_folder_is_refused_at_the_entry()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("Schema\nTypeKnowledge\n")
            .WithFile("Schema/Node.cs", Node)
            .Expecting(InTheTable(2, 1, 14, "entry 'TypeKnowledge' names no folder"))
            .RunAsync();

    [AcceptedCase]
    public Task an_entry_whose_folder_holds_a_file_is_accepted()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("Schema\nTypeKnowledge\n")
            .WithFile("Schema/Node.cs", Node)
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .RunAsync();

    [RefusedCase]
    public Task a_folder_under_two_slices_is_refused_at_the_second_entry()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("*\nNativeComponents/*\n")
            .WithFile("NativeComponents/TextBox/TextBox.cs", "namespace TestProject.NativeComponents; public sealed class TextBox;")
            .Expecting(InTheTable(2, 1, 19, "folder 'NativeComponents/TextBox' falls under 'NativeComponents' and 'NativeComponents/TextBox'"))
            .RunAsync();

    [AcceptedCase]
    public Task a_folder_and_each_folder_under_it_are_slices_of_their_own()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("NativeComponents\nNativeAppLevelObjects/* NativeComponents/*\n")
            .WithFile("NativeComponents/Field.cs", "namespace TestProject.NativeComponents; public sealed class Field;")
            .WithFile("NativeComponents/TextBox/TextBox.cs", "namespace TestProject.NativeComponents; public sealed class TextBox;")
            .WithFile("NativeAppLevelObjects/Drawer/Parts/Panel.cs", "namespace TestProject.NativeAppLevelObjects; public sealed class Panel;")
            .RunAsync();

    [RefusedCase]
    public Task a_slice_in_two_rows_is_refused_at_the_later_entry()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("Schema\nTypeKnowledge\nSchema\n")
            .WithFile("Schema/Node.cs", Node)
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .Expecting(InTheTable(3, 1, 7, "slice 'Schema' is in two rows"))
            .RunAsync();

    [AcceptedCase]
    public Task a_slice_written_twice_in_one_row_is_accepted()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("Schema Schema\nTypeKnowledge\n")
            .WithFile("Schema/Node.cs", Node)
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .RunAsync();

    [RefusedCase]
    public Task a_folder_with_no_row_is_refused_at_its_files_first_token()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("Schema\n")
            .WithFile("Schema/Node.cs", Node)
            .WithFile("PathsAndWire/Encoder.cs", "{|#0:namespace|} TestProject.PathsAndWire; public sealed class Encoder;")
            .Expecting(Report("folder 'PathsAndWire' has no row").WithLocation(0))
            .RunAsync();

    [RefusedCase]
    public Task a_file_beside_a_table_that_holds_no_dot_is_refused_at_its_first_token()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("Schema\n")
            .WithFile("Schema/Node.cs", Node)
            .WithFile("Marker.cs", "{|#0:namespace|} TestProject; public sealed class Marker;")
            .Expecting(Report("folder '.' has no row").WithLocation(0))
            .RunAsync();

    [AcceptedCase]
    public Task a_file_beside_a_table_that_holds_a_dot_is_accepted()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable(".\nSchema\n")
            .WithFile("Schema/Node.cs", Node)
            .WithFile("Marker.cs", "namespace TestProject; public sealed class Marker;")
            .RunAsync();

    [AcceptedCase]
    public Task a_file_under_obj_under_bin_or_outside_the_tables_folder_is_read_by_no_rule()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("Schema\n")
            .WithFile("Schema/Node.cs", Node)
            .WithFile("obj/Debug/Generated.cs", "namespace TestProject; public sealed class Generated;")
            .WithFile("bin/Debug/Copied.cs", "namespace TestProject; public sealed class Copied;")
            .WithFileAt("/Elsewhere/Linked.cs", "namespace TestProject; public sealed class Linked;")
            .RunAsync();

    [RefusedCase]
    public Task a_type_whose_parts_sit_in_two_slices_is_refused_at_its_name()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("Schema\nTypeKnowledge\n")
            .WithFile("Schema/Kinds.cs", "namespace TestProject; public static partial class {|#0:Kinds|};")
            .WithFile("TypeKnowledge/Kinds.cs", "namespace TestProject; public static partial class Kinds;")
            .Expecting(Report("'TestProject.Kinds' is declared in slices 'Schema' and 'TypeKnowledge'").WithLocation(0))
            .RunAsync();

    [AcceptedCase]
    public Task a_type_whose_parts_sit_in_one_slice_is_accepted()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("Schema\n")
            .WithFile("Schema/Kinds.cs", "namespace TestProject; public static partial class Kinds;")
            .WithFile("Schema/MoreKinds.cs", "namespace TestProject; public static partial class Kinds;")
            .RunAsync();

    [RefusedCase]
    public Task a_project_built_with_the_slice_tree_on_and_no_table_is_refused_with_no_location()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTheSliceTreeOn()
            .WithFile("Schema/Node.cs", Node)
            .Expecting(Report("'TestProject' is built with AlisSliceTree on and holds no SliceTree.txt"))
            .RunAsync();

    [AcceptedCase]
    public Task a_project_with_no_table_and_the_slice_tree_off_is_read_by_no_rule()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithFile("Anywhere/Node.cs", "public class Node;")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => new Case<ALIS013SliceTableMatchesItsProject>()
            .WithTable("Schema\n")
            .WithFile("Schema/Node.cs", "namespace TestProject.Schema; public sealed class Node { public {|CS0246:Missing|} Held; }")
            .RunAsync();

    private static DiagnosticResult Report(string disagreement)
        => new DiagnosticResult(ALIS013SliceTableMatchesItsProject.Id, DiagnosticSeverity.Error)
            .WithMessage("SliceTree.txt and the package disagree: " + disagreement);

    private static DiagnosticResult InTheTable(int line, int column, int endColumn, string disagreement)
        => Report(disagreement).WithSpan(Case<ALIS013SliceTableMatchesItsProject>.Table, line, column, line, endColumn);
}
