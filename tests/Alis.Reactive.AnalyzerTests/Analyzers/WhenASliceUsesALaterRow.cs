using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Analyzers;

[TestFixture]
public sealed class WhenASliceUsesALaterRow
{
    private const string SchemaThenTypeKnowledge = "Schema\nTypeKnowledge\n";

    private const string Reader = """
        namespace TestProject.TypeKnowledge;

        public interface IReader;

        public delegate void ReaderChanged();

        [System.AttributeUsage(System.AttributeTargets.All)]
        public sealed class MarkAttribute : System.Attribute;

        public struct Size
        {
            public int Value;
        }

        public sealed class Bag : System.Collections.IEnumerable
        {
            public void Add(int item)
            {
            }

            public System.Collections.IEnumerator GetEnumerator() => throw new System.NotSupportedException();
        }

        public class Reader : IReader
        {
            public static int Limit;

            public static int Count => Limit;

            public int Weight => Limit;

            public static event ReaderChanged Changed;

            public static int Measure() => 0;

            public int Weigh() => 0;

            public void Deconstruct(out int weight, out int limit)
            {
                weight = 0;
                limit = 0;
            }

            public static Reader operator +(Reader left, Reader right) => left;

            public static Reader operator -(Reader operand) => operand;

            public static Reader operator ++(Reader operand) => operand;

            public static Reader operator --(Reader operand) => operand;

            public static implicit operator int(Reader reader) => 0;
        }
        """;

    [RefusedCase]
    public Task a_base_type_of_a_later_row_is_refused_at_the_class_name()
        => InSchema("public class {|#0:Node|} : Reader;", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_base_call_of_a_later_row_is_refused_at_base_as_its_type()
        => InSchema("public class {|#0:Node|} : Reader { public Node() : {|#1:base|}() { } }", Uses(0, "Reader"), Uses(1, "Reader"));

    [RefusedCase]
    public Task a_base_call_in_a_class_header_is_refused_at_the_type()
        => InSchema("public class {|#0:Node|}() : {|#1:Reader|}();", Uses(0, "Reader"), Uses(1, "Reader"));

    [RefusedCase]
    public Task an_interface_of_a_later_row_is_refused_at_the_class_name()
        => InSchema("public sealed class {|#0:Node|} : IReader;", Uses(0, "IReader"));

    [RefusedCase]
    public Task a_types_constraint_of_a_later_row_is_refused_at_the_class_name()
        => InSchema("public sealed class {|#0:Node|}<T> where T : IReader;", Uses(0, "IReader"));

    [RefusedCase]
    public Task a_methods_constraint_of_a_later_row_is_refused_at_the_method_name()
        => InNode("public void {|#0:Take|}<T>() where T : IReader { }", Uses(0, "IReader"));

    [RefusedCase]
    public Task an_attribute_of_a_later_row_is_refused_where_it_is_written()
        => InSchema("[{|#0:Mark|}] public sealed class Node;", Uses(0, "MarkAttribute"));

    [RefusedCase]
    public Task a_type_an_attributes_argument_names_is_refused_where_it_is_written()
        => InSchema("[System.ComponentModel.TypeConverter(typeof({|#0:Reader|}))] public sealed class Node;", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_fields_type_of_a_later_row_is_refused_at_the_field_name()
        => InNode("public Reader {|#0:Held|};", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_propertys_type_of_a_later_row_is_refused_at_the_property_name()
        => InNode("public Reader {|#0:Held|} { get; set; }", Uses(0, "Reader"));

    [RefusedCase]
    public Task an_events_type_of_a_later_row_is_refused_at_the_event_name()
        => InNode("public event ReaderChanged {|#0:Changed|};", Uses(0, "ReaderChanged"));

    [RefusedCase]
    public Task a_parameters_type_of_a_later_row_is_refused_at_the_parameter_name()
        => InNode("public void Take(Reader {|#0:reader|}) { }", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_return_type_of_a_later_row_is_refused_at_the_method_name()
        => InNode("public Reader {|#0:Give|}() => null;", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_delegates_signature_of_a_later_row_is_refused_at_its_names()
        => InSchema("public delegate Reader {|#0:Give|}(Reader {|#1:reader|});", Uses(0, "Reader"), Uses(1, "Reader"));

    [RefusedCase]
    public Task a_locals_type_of_a_later_row_is_refused_at_the_local_name()
        => InNode("public void Run() { Reader {|#0:reader|} = null; }", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_local_whose_type_is_inferred_is_refused_at_the_local_name()
        => InNode("public void Run() { var {|#1:reader|} = new {|#0:Reader|}(); }", Uses(0, "Reader"), Uses(1, "Reader"));

    [RefusedCase]
    public Task a_local_declared_in_an_argument_is_refused_at_the_local_name()
        => InNode(
            "public void Run(System.Collections.Generic.Dictionary<int, Reader> {|#0:held|}) { held.TryGetValue(1, out var {|#1:reader|}); }",
            Uses(0, "Reader"), Uses(1, "Reader"));

    [RefusedCase]
    public Task a_call_of_a_later_row_is_refused_at_the_method_name()
        => InNode("public int Run() => Reader.{|#0:Measure|}();", Uses(0, "Reader.Measure"));

    [RefusedCase]
    public Task a_call_on_a_value_of_a_later_row_is_refused_at_the_method_name()
        => InNode("public int Run(Reader {|#0:reader|}) => reader.{|#1:Weigh|}();", Uses(0, "Reader"), Uses(1, "Reader.Weigh"));

    [RefusedCase]
    public Task a_creation_of_a_later_row_is_refused_at_the_type()
        => InNode("public object Run() => new {|#0:Reader|}();", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_call_the_compiler_adds_for_a_creation_is_no_second_use()
        => InNode("public object Run() => new {|#0:Bag|} { 1, 2 };", Uses(0, "Bag"));

    [RefusedCase]
    public Task a_creation_whose_type_is_not_written_is_refused_at_new()
        => InNode("public void Run() { Reader {|#0:reader|} = {|#1:new|}(); }", Uses(0, "Reader"), Uses(1, "Reader"));

    [RefusedCase]
    public Task a_member_read_of_a_later_row_is_refused_at_the_member_name()
        => InNode("public int Run() => Reader.{|#0:Count|};", Uses(0, "Reader.Count"));

    [RefusedCase]
    public Task a_member_written_of_a_later_row_is_refused_at_the_member_name()
        => InNode("public void Run() => Reader.{|#0:Limit|} = 3;", Uses(0, "Reader.Limit"));

    [RefusedCase]
    public Task an_event_of_a_later_row_is_refused_at_the_event_name()
        => InNode("public void Run() => Reader.{|#0:Changed|} += null;", Uses(0, "Reader.Changed"));

    [RefusedCase]
    public Task a_method_of_a_later_row_taken_as_a_value_is_refused_at_the_method_name()
        => InNode("public System.Func<int> Run() => Reader.{|#0:Measure|};", Uses(0, "Reader.Measure"));

    [RefusedCase]
    public Task an_operator_a_later_row_declares_is_refused_at_the_operator()
        => InNode(
            "public object Run(Reader {|#0:left|}, Reader {|#1:right|}) => left {|#2:+|} right;",
            Uses(0, "Reader"), Uses(1, "Reader"), Uses(2, "Reader.operator +"));

    [RefusedCase]
    public Task a_unary_operator_a_later_row_declares_is_refused_at_the_operator()
        => InNode("public object Run(Reader {|#0:operand|}) => {|#1:-|}operand;", Uses(0, "Reader"), Uses(1, "Reader.operator -"));

    [RefusedCase]
    public Task an_increment_a_later_row_declares_is_refused_at_the_operator()
        => InNode("public void Run(Reader {|#0:operand|}) { operand{|#1:++|}; }", Uses(0, "Reader"), Uses(1, "Reader.operator ++"));

    [RefusedCase]
    public Task a_decrement_a_later_row_declares_is_refused_at_the_operator()
        => InNode("public void Run(Reader {|#0:operand|}) { {|#1:--|}operand; }", Uses(0, "Reader"), Uses(1, "Reader.operator --"));

    [RefusedCase]
    public Task a_compound_assignment_a_later_row_declares_is_refused_at_the_operator()
        => InNode(
            "public void Run(Reader {|#0:left|}, Reader {|#1:right|}) { left {|#2:+=|} right; }",
            Uses(0, "Reader"), Uses(1, "Reader"), Uses(2, "Reader.operator +"));

    [RefusedCase]
    public Task a_conversion_a_later_row_declares_is_refused_at_the_converted_value()
        => InNode(
            "public int Run(Reader {|#0:reader|}) { int size = {|#1:reader|}; return size; }",
            Uses(0, "Reader"), Uses(1, "Reader.implicit operator int"));

    [RefusedCase]
    public Task a_cast_to_a_later_row_is_refused_at_the_type()
        => InNode("public object Run(object value) => ({|#0:Reader|})value;", Uses(0, "Reader"));

    [RefusedCase]
    public Task an_as_to_a_later_row_is_refused_at_the_type()
        => InNode("public object Run(object value) => value as {|#0:Reader|};", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_typeof_a_later_row_is_refused_at_the_type()
        => InNode("public System.Type Run() => typeof({|#0:Reader|});", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_nameof_a_later_rows_type_is_refused_at_the_type()
        => InNode("public string Run() => nameof({|#0:Reader|});", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_nameof_a_later_rows_member_is_refused_at_the_member_name()
        => InNode("public string Run() => nameof(Reader.{|#0:Count|});", Uses(0, "Reader.Count"));

    [RefusedCase]
    public Task a_using_static_of_a_later_row_is_refused_at_the_type()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable(SchemaThenTypeKnowledge)
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .WithFile("Schema/Node.cs", "using static {|#0:TestProject.TypeKnowledge.Reader|}; namespace TestProject.Schema; public sealed class Node;")
            .Expecting(Uses(0, "Reader"))
            .RunAsync();

    [RefusedCase]
    public Task a_using_alias_of_a_later_row_is_refused_at_the_type()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable(SchemaThenTypeKnowledge)
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .WithFile("Schema/Node.cs", "using Held = {|#0:TestProject.TypeKnowledge.Reader|}; namespace TestProject.Schema; public sealed class Node;")
            .Expecting(Uses(0, "Reader"))
            .RunAsync();

    [RefusedCase]
    public Task an_is_of_a_later_row_is_refused_at_the_type()
        => InNode("public bool Run(object value) => value is {|#0:Reader|};", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_sizeof_a_later_row_is_refused_at_the_type()
        => new Case<ALIS010SliceUsesEarlierRows> { AllowsUnsafe = true }
            .WithTable(SchemaThenTypeKnowledge)
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .WithFile("Schema/Node.cs", InTheSchemaNamespace("public sealed class Node { public unsafe int Run() => sizeof({|#0:Size|}); }"))
            .Expecting(Uses(0, "Size"))
            .RunAsync();

    [RefusedCase]
    public Task a_default_of_a_later_row_is_refused_at_the_type()
        => InNode("public object Run() => default({|#0:Size|});", Uses(0, "Size"));

    [RefusedCase]
    public Task an_array_of_a_later_row_is_refused_at_the_element_type()
        => InNode("public object Run() => new {|#0:Reader|}[2];", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_type_pattern_of_a_later_row_is_refused_at_the_type()
        => InNode("public int Run(object value) => value switch { {|#0:Reader|} => 1, _ => 0 };", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_declaration_pattern_of_a_later_row_is_refused_at_the_type()
        => InNode("public bool Run(object value) => value is {|#0:Reader|} held;", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_property_pattern_of_a_later_row_is_refused_at_the_type_and_the_member()
        => InNode("public bool Run(object value) => value is {|#0:Reader|} { {|#1:Weight|}: 0 };", Uses(0, "Reader"), Uses(1, "Reader.Weight"));

    [RefusedCase]
    public Task a_positional_pattern_of_a_later_row_is_refused_for_the_type_and_its_deconstruct()
        => InNode("public bool Run(object value) => value is {|#0:Reader|} (0, 0);", Uses(0, "Reader"), Uses(0, "Reader.Deconstruct"));

    [RefusedCase]
    public Task a_lambdas_parameter_of_a_later_row_is_refused_at_the_parameter_name()
        => InNode("public void Run() { System.Delegate held = (Reader {|#0:reader|}) => { }; }", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_lambdas_parameter_whose_type_is_inferred_is_refused_at_the_parameter_name()
        => InNode("public void Run() { System.Action<Reader> {|#0:held|} = {|#1:reader|} => { }; }", Uses(0, "Reader"), Uses(1, "Reader"));

    [RefusedCase]
    public Task a_local_functions_signature_of_a_later_row_is_refused_at_its_names()
        => InNode("public void Run() { Reader {|#0:Give|}(Reader {|#1:reader|}) => reader; Give(null); }", Uses(0, "Reader"), Uses(1, "Reader"));

    [RefusedCase]
    public Task a_type_argument_of_a_later_row_is_refused()
        => InNode("public System.Collections.Generic.List<Reader> {|#0:Held|};", Uses(0, "Reader"));

    [RefusedCase]
    public Task an_element_type_of_a_later_row_is_refused()
        => InNode("public Reader[] {|#0:Held|};", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_pointed_at_type_of_a_later_row_is_refused()
        => new Case<ALIS010SliceUsesEarlierRows> { AllowsUnsafe = true }
            .WithTable(SchemaThenTypeKnowledge)
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .WithFile("Schema/Node.cs", InTheSchemaNamespace("public sealed class Node { public unsafe Size* {|#0:Held|}; }"))
            .Expecting(Uses(0, "Size"))
            .RunAsync();

    [RefusedCase]
    public Task a_calls_type_argument_of_a_later_row_is_refused_at_the_method_name()
        => InNode("public object Run() => System.Array.{|#0:Empty<Reader>|}();", Uses(0, "Reader"));

    [RefusedCase]
    public Task a_slice_of_the_same_row_is_refused_as_its_own_row()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable("Schema\nOwnershipAndState Tracing\n")
            .WithFile("OwnershipAndState/Owner.cs", "namespace TestProject.OwnershipAndState; public sealed class Owner;")
            .WithFile("Tracing/Trace.cs", "namespace TestProject.Tracing; public sealed class Trace { public TestProject.OwnershipAndState.Owner {|#0:Held|}; }")
            .Expecting(Uses(0, "Owner", "its own row", "Tracing", "OwnershipAndState"))
            .RunAsync();

    [RefusedCase]
    public Task each_folder_under_a_folder_is_a_slice_of_its_row()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable("NativeComponents\nNativeComponents/*\n")
            .WithFile("NativeComponents/Field.cs", "namespace TestProject.NativeComponents; public sealed class Field { public TextBox {|#0:Box|}; }")
            .WithFile("NativeComponents/CheckBox/CheckBox.cs", "namespace TestProject.NativeComponents; public sealed class CheckBox;")
            .WithFile("NativeComponents/TextBox/TextBox.cs", "namespace TestProject.NativeComponents; public sealed class TextBox { public CheckBox {|#1:Check|}; }")
            .Expecting(
                Uses(0, "TextBox", "a later row", "NativeComponents", "NativeComponents/TextBox"),
                Uses(1, "CheckBox", "its own row", "NativeComponents/TextBox", "NativeComponents/CheckBox"))
            .RunAsync();

    [AcceptedCase]
    public Task a_use_of_an_earlier_row_is_accepted()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable(SchemaThenTypeKnowledge)
            .WithFile("Schema/Node.cs", "namespace TestProject.Schema; public sealed class Node { public static int Size() => 0; }")
            .WithFile("TypeKnowledge/Reader.cs", "namespace TestProject.TypeKnowledge; public sealed class Reader { public TestProject.Schema.Node Held; public int Run() => TestProject.Schema.Node.Size(); }")
            .RunAsync();

    [AcceptedCase]
    public Task a_use_of_the_slices_own_files_is_accepted()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable(SchemaThenTypeKnowledge)
            .WithFile("Schema/Node.cs", "namespace TestProject.Schema; public sealed class Node { public Edge Held; }")
            .WithFile("Schema/Edge.cs", "namespace TestProject.Schema; public sealed class Edge { public Node Held; }")
            .RunAsync();

    [AcceptedCase]
    public Task a_use_of_another_assembly_is_accepted()
        => InNode("public System.Text.StringBuilder Held = new System.Text.StringBuilder(); public int Run() => System.Math.Abs(1);");

    [AcceptedCase]
    public Task a_type_of_another_project_whose_file_sits_under_a_later_row_is_accepted()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable(SchemaThenTypeKnowledge)
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .WithFile("Schema/Node.cs", "namespace TestProject.Schema; public sealed class Node { public Other.Shared Held; }")
            .WithAnotherProject("Other", "/TestProject/TypeKnowledge/Shared.cs", "namespace Other; public sealed class Shared;")
            .RunAsync();

    [AcceptedCase]
    public Task a_folder_under_a_folder_uses_the_folders_own_row()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable("NativeComponents\nNativeComponents/*\n")
            .WithFile("NativeComponents/Field.cs", "namespace TestProject.NativeComponents; public sealed class Field;")
            .WithFile("NativeComponents/TextBox/TextBox.cs", "namespace TestProject.NativeComponents; public sealed class TextBox { public Field Held; }")
            .RunAsync();

    [AcceptedCase]
    public Task a_folder_inside_a_slice_is_that_slice()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable("NativeComponents/*\n")
            .WithFile("NativeComponents/TextBox/TextBox.cs", "namespace TestProject.NativeComponents; public sealed class TextBox { public Caret Held; }")
            .WithFile("NativeComponents/TextBox/Parts/Caret.cs", "namespace TestProject.NativeComponents; public sealed class Caret { public TextBox Held; }")
            .RunAsync();

    [AcceptedCase]
    public Task a_project_that_hands_the_compiler_no_table_is_read_by_no_rule()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .WithFile("Schema/Node.cs", InTheSchemaNamespace("public sealed class Node { public Reader Held; }"))
            .RunAsync();

    [AcceptedCase]
    public Task a_file_under_obj_or_outside_the_tables_folder_is_read_by_no_rule()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable(SchemaThenTypeKnowledge)
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .WithFile("obj/Debug/Generated.cs", InTheSchemaNamespace("public sealed class Generated { public Reader Held; }"))
            .WithFileAt("/Elsewhere/Linked.cs", InTheSchemaNamespace("public sealed class Linked { public Reader Held; }"))
            .RunAsync();

    [AcceptedCase]
    public Task a_type_the_table_cannot_place_is_passed()
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable(SchemaThenTypeKnowledge)
            .WithFile("Schema/Kinds.cs", "namespace TestProject; public sealed partial class Kinds;")
            .WithFile("TypeKnowledge/Kinds.cs", "namespace TestProject; public sealed partial class Kinds;")
            .WithFile("PathsAndWire/Encoder.cs", "namespace TestProject.PathsAndWire; public sealed class Encoder;")
            .WithFile("Schema/Node.cs", "namespace TestProject.Schema; public sealed class Node { public TestProject.Kinds Kinds; public TestProject.PathsAndWire.Encoder Encoder; }")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => InNode("public {|CS0246:Missing|} Held;");

    private static Task InNode(string members, params DiagnosticResult[] reports)
        => InSchema("public sealed class Node { " + members + " }", reports);

    private static Task InSchema(string declarations, params DiagnosticResult[] reports)
        => new Case<ALIS010SliceUsesEarlierRows>()
            .WithTable(SchemaThenTypeKnowledge)
            .WithFile("TypeKnowledge/Reader.cs", Reader)
            .WithFile("Schema/Node.cs", InTheSchemaNamespace(declarations))
            .Expecting(reports)
            .RunAsync();

    private static string InTheSchemaNamespace(string declarations)
        => "using TestProject.TypeKnowledge; namespace TestProject.Schema; " + declarations;

    private static DiagnosticResult Uses(int at, string symbol, string row = "a later row", string slice = "Schema", string its = "TypeKnowledge")
        => new DiagnosticResult(ALIS010SliceUsesEarlierRows.Id, DiagnosticSeverity.Error)
            .WithLocation(at)
            .WithMessage($"'{slice}' uses '{symbol}' of slice '{its}', {row} in SliceTree.txt: a slice uses only the rows before its own");
}
