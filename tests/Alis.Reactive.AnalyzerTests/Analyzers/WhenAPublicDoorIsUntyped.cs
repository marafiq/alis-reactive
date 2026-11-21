using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Analyzers;

[TestFixture]
public sealed class WhenAPublicDoorIsUntyped
{
    private const string Exempt = "using Alis.Reactive.Declarations; ";
    private const string VendorBoundary = "\"The vendor boundary: a library is described by the JavaScript names it uses.\"";

    [RefusedCase]
    public Task a_parameter_typed_object_is_refused_at_its_name()
        => InBed("public void Assign(object {|#0:resident|}) { }", Names(0, "Bed.Assign", "object"));

    [AcceptedCase]
    public Task a_parameter_of_a_type_parameter_is_accepted()
        => InBed("public void Assign<TResident>(TResident resident) { }");

    [RefusedCase]
    public Task a_parameter_typed_dynamic_is_refused_at_its_name()
        => InBed("public void Assign(dynamic {|#0:resident|}) { }", Names(0, "Bed.Assign", "dynamic"));

    [RefusedCase]
    public Task a_parameter_typed_as_a_type_object_is_refused_at_its_name()
        => InBed("public void Assign(System.Type {|#0:resident|}) { }", Names(0, "Bed.Assign", "System.Type"));

    [RefusedCase]
    public Task a_return_typed_object_is_refused_at_the_methods_name()
        => InBed("public object {|#0:Resident|}() => 1;", Names(0, "Bed.Resident", "object"));

    [AcceptedCase]
    public Task a_method_that_returns_nothing_is_accepted() => InBed("public void Resident() { }");

    [RefusedCase]
    public Task a_constructors_parameter_typed_object_is_refused_at_its_name()
        => InBed("public Bed(object {|#0:resident|}) { }", Names(0, "Bed.Bed", "object"));

    [RefusedCase]
    public Task a_primary_constructors_parameter_typed_object_is_refused_at_its_name()
        => InBeds("public sealed class Ward(object {|#0:resident|});", Names(0, "Ward.Ward", "object"));

    [RefusedCase]
    public Task an_operators_parameter_typed_object_is_refused_at_its_name()
        => InBed("public static Bed operator +(Bed bed, object {|#0:resident|}) => bed;", Names(0, "Bed.operator +", "object"));

    [RefusedCase]
    public Task a_conversions_parameter_typed_as_a_type_object_is_refused_at_its_name()
        => InBed("public static implicit operator Bed(System.Type {|#0:resident|}) => new();", Names(0, "Bed.implicit operator Bed", "System.Type"));

    [RefusedCase]
    public Task a_property_typed_object_is_refused_at_its_name()
        => InBed("public object {|#0:Resident|} { get; } = 1;", Names(0, "Bed.Resident", "object"));

    [AcceptedCase]
    public Task a_property_of_a_named_type_is_accepted() => InBed("public Bed? Next { get; }");

    [RefusedCase]
    public Task an_indexers_parameter_typed_object_is_refused_at_its_name()
        => InBed("public int this[object {|#0:resident|}] => 0;", Names(0, "Bed.this[]", "object"));

    [RefusedCase]
    public Task a_field_typed_object_is_refused_at_its_name()
        => InBed("public object {|#0:Resident|} = 1;", Names(0, "Bed.Resident", "object"));

    [RefusedCase]
    public Task an_event_whose_type_holds_object_is_refused_at_its_name()
        => InBed("public event System.Action<object>? {|#0:Assigned|};", Names(0, "Bed.Assigned", "object"));

    [RefusedCase]
    public Task a_delegates_parameter_typed_object_is_refused_at_its_name()
        => InBeds("public delegate void Assigned(object {|#0:resident|});", Names(0, "Assigned", "object"));

    [RefusedCase]
    public Task a_delegates_return_typed_object_is_refused_at_the_delegates_name()
        => InBeds("public delegate object {|#0:Chosen|}();", Names(0, "Chosen", "object"));

    [AcceptedCase]
    public Task a_delegate_of_named_types_is_accepted() => InBeds("public delegate int Chosen(string text);");

    [RefusedCase]
    public Task a_base_types_argument_of_object_is_refused_at_the_types_name()
        => InBeds("public sealed class {|#0:Ward|} : System.Collections.Generic.List<object>;", Names(0, "Ward", "object"));

    [AcceptedCase]
    public Task a_class_with_no_written_base_is_accepted() => InBeds("public sealed class Ward;");

    [RefusedCase]
    public Task an_interfaces_argument_of_object_is_refused_at_the_types_name()
        => InBeds("public interface IHolds<T>; public sealed class {|#0:Ward|} : IHolds<object>;", Names(0, "Ward", "object"));

    [AcceptedCase]
    public Task an_interfaces_argument_of_a_named_type_is_accepted()
        => InBeds("public interface IHolds<T>; public sealed class Ward : IHolds<int>;");

    [RefusedCase]
    public Task a_types_constraint_that_holds_object_is_refused_at_the_type_parameters_name()
        => InBeds("public interface IHolds<T>; public sealed class Ward<{|#0:TBed|}> where TBed : IHolds<object>;", Names(0, "Ward<TBed>", "object"));

    [RefusedCase]
    public Task a_methods_constraint_that_holds_object_is_refused_at_the_type_parameters_name()
        => InBeds(
            "public interface IHolds<T>; public sealed class Ward { public void Assign<{|#0:TBed|}>() where TBed : IHolds<object> { } }",
            Names(0, "Ward.Assign<TBed>", "object"));

    [AcceptedCase]
    public Task a_constraint_of_named_types_is_accepted()
        => InBeds("public interface IHolds<T>; public sealed class Ward<TBed> where TBed : class, IHolds<int>;");

    [RefusedCase]
    public Task a_type_argument_of_object_is_refused()
        => InBed("public void Assign(System.Collections.Generic.List<object> {|#0:residents|}) { }", Names(0, "Bed.Assign", "object"));

    [RefusedCase]
    public Task a_type_argument_two_levels_down_is_refused()
        => InBed(
            "public void Assign(System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<System.Type>> {|#0:residents|}) { }",
            Names(0, "Bed.Assign", "System.Type"));

    [RefusedCase]
    public Task an_arrays_element_of_object_is_refused()
        => InBed("public void Assign(params object[] {|#0:residents|}) { }", Names(0, "Bed.Assign", "object"));

    [RefusedCase]
    public Task a_delegates_argument_of_object_is_refused()
        => InBed("public void Assign(System.Func<int, object> {|#0:choose|}) { }", Names(0, "Bed.Assign", "object"));

    [RefusedCase]
    public Task an_expressions_argument_of_object_is_refused()
        => InBed(
            "public void Assign(System.Linq.Expressions.Expression<System.Func<Bed, object?>> {|#0:member|}) { }",
            Names(0, "Bed.Assign", "object"));

    [AcceptedCase]
    public Task an_expression_of_a_type_parameter_is_accepted()
        => InBed("public void Assign<TValue>(System.Linq.Expressions.Expression<System.Func<Bed, TValue>> member) { }");

    [RefusedCase]
    public Task a_tuples_element_of_object_is_refused()
        => InBed("public void Assign((object First, int Second) {|#0:pair|}) { }", Names(0, "Bed.Assign", "object"));

    [RefusedCase]
    public Task a_generic_holders_argument_of_object_is_refused()
        => InBeds(
            "public sealed class Ward<T> { public sealed class Room; } public sealed class Bed { public void Assign(Ward<object>.Room {|#0:room|}) { } }",
            Names(0, "Bed.Assign", "object"));

    [RefusedCase]
    public Task a_pointers_target_of_a_type_object_is_refused()
        => new Case<ALIS009PublicDoorIsTyped> { AllowsUnsafe = true }
            .WithTable("Beds\n")
            .WithFile("Beds/Bed.cs", "namespace TestProject.Beds; public sealed class Bed { public unsafe void Assign(System.Type* {|#0:resident|}) { } }")
            .Expecting(Names(0, "Bed.Assign", "System.Type"))
            .RunAsync();

    [RefusedCase]
    public Task a_function_pointers_parameter_and_return_of_object_are_refused()
        => new Case<ALIS009PublicDoorIsTyped> { AllowsUnsafe = true }
            .WithTable("Beds\n")
            .WithFile(
                "Beds/Bed.cs",
                "namespace TestProject.Beds; public sealed class Bed { public unsafe void Assign(delegate*<object, void> {|#0:take|}, delegate*<object> {|#1:give|}) { } }")
            .Expecting(Names(0, "Bed.Assign", "object"), Names(1, "Bed.Assign", "object"))
            .RunAsync();

    [RefusedCase]
    public Task a_protected_members_parameter_is_refused()
        => InBeds("public abstract class Ward { protected void Assign(object {|#0:resident|}) { } }", Names(0, "Ward.Assign", "object"));

    [RefusedCase]
    public Task a_protected_internal_members_parameter_is_refused()
        => InBeds("public abstract class Ward { protected internal void Assign(object {|#0:resident|}) { } }", Names(0, "Ward.Assign", "object"));

    [AcceptedCase]
    public Task an_internal_member_is_no_door() => InBed("internal void Assign(object resident) { }");

    [AcceptedCase]
    public Task a_private_protected_member_is_no_door()
        => InBeds("public abstract class Ward { private protected void Assign(object resident) { } }");

    [AcceptedCase]
    public Task a_public_member_of_an_internal_type_is_no_door()
        => InBeds("internal sealed class Ward { public void Assign(object resident) { } }");

    [AcceptedCase]
    public Task a_public_type_inside_an_internal_type_is_no_door()
        => InBeds("internal sealed class Ward { public sealed class Room { public void Assign(object resident) { } } }");

    [RefusedCase]
    public Task a_public_type_inside_a_public_type_is_a_door()
        => InBeds("public sealed class Ward { public sealed class Room { public void Assign(object {|#0:resident|}) { } } }", Names(0, "Ward.Room.Assign", "object"));

    [RefusedCase]
    public Task an_interfaces_member_is_a_door()
        => InBeds("public interface IAssigns { void Assign(object {|#0:resident|}); }", Names(0, "IAssigns.Assign", "object"));

    [AcceptedCase]
    public Task a_member_the_compiler_writes_is_not_read() => InBeds("public abstract record Ward(int Size);");

    [RefusedCase]
    public Task a_records_parameter_is_refused_as_its_constructors_and_as_its_propertys()
        => InBeds(
            "public sealed record Ward(object {|#0:Resident|});",
            Names(0, "Ward.Ward", "object"),
            Names(0, "Ward.Resident", "object"));

    [RefusedCase]
    public Task a_partial_method_is_refused_once_where_it_is_defined()
        => InBeds(
            "public sealed partial class Bed { public partial void Assign(object {|#0:resident|}); } public sealed partial class Bed { public partial void Assign(object resident) { } }",
            Names(0, "Bed.Assign", "object"));

    [RefusedCase]
    public Task a_partial_property_is_refused_once_where_it_is_defined()
        => InBeds(
            "public sealed partial class Bed { public partial object {|#0:Resident|} { get; } } public sealed partial class Bed { public partial object Resident => 1; }",
            Names(0, "Bed.Resident", "object"));

    [RefusedCase]
    public Task a_classic_extensions_receiver_typed_object_is_refused_at_its_name()
        => InBeds("public static class BedDoors { public static void Assign(this object {|#0:bed|}) { } }", Names(0, "BedDoors.Assign", "object"));

    [RefusedCase]
    public Task a_member_of_an_extension_block_is_read()
        => InBeds(
            "public sealed class Bed; public static class BedDoors { extension(Bed bed) { public void Assign(object {|#0:resident|}) { } } }",
            Names(0, "BedDoors.extension(Bed).Assign", "object"));

    [AcceptedCase]
    public Task the_receiver_of_an_extension_block_is_not_read()
        => InBeds("public static class BedDoors { extension(object bed) { public void Assign(int size) { } } }");

    [RefusedCase]
    public Task a_string_named_for_an_event_is_refused_at_its_name()
        => InBed("public void On(string {|#0:eventName|}) { }", NamedFor(0, "Bed.On", "eventName", "event"));

    [AcceptedCase]
    public Task a_string_named_for_a_text_is_accepted() => InBed("public void On(string text) { }");

    [RefusedCase]
    public Task every_listed_word_is_refused()
        => InBed(
            """
            public void On(
                string {|#0:action|}, string {|#1:collection|}, string {|#2:@event|}, string {|#3:field|}, string {|#4:kind|}, string {|#5:member|},
                string {|#6:method|}, string {|#7:op|}, string {|#8:owner|}, string {|#9:param|}, string {|#10:path|}, string {|#11:peer|},
                string {|#12:property|}, string {|#13:selector|}, string {|#14:subscribe|}, string {|#15:unsubscribe|}) { }
            """,
            NamedFor(0, "Bed.On", "action", "action"),
            NamedFor(1, "Bed.On", "collection", "collection"),
            NamedFor(2, "Bed.On", "event", "event"),
            NamedFor(3, "Bed.On", "field", "field"),
            NamedFor(4, "Bed.On", "kind", "kind"),
            NamedFor(5, "Bed.On", "member", "member"),
            NamedFor(6, "Bed.On", "method", "method"),
            NamedFor(7, "Bed.On", "op", "op"),
            NamedFor(8, "Bed.On", "owner", "owner"),
            NamedFor(9, "Bed.On", "param", "param"),
            NamedFor(10, "Bed.On", "path", "path"),
            NamedFor(11, "Bed.On", "peer", "peer"),
            NamedFor(12, "Bed.On", "property", "property"),
            NamedFor(13, "Bed.On", "selector", "selector"),
            NamedFor(14, "Bed.On", "subscribe", "subscribe"),
            NamedFor(15, "Bed.On", "unsubscribe", "unsubscribe"));

    [AcceptedCase]
    public Task every_name_the_designed_surface_gives_a_string_outside_the_vendor_boundary_is_accepted()
        => InBed(
            """
            public void On(
                string containerId, string elementId, string[] elementIds, string id, string name, string rowsId, string template,
                string text, string url, string value, string viewName) { }
            """);

    [RefusedCase]
    public Task a_word_after_a_small_letter_starts_at_its_capital()
        => InBed("public void On(string {|#0:valueMember|}) { }", NamedFor(0, "Bed.On", "valueMember", "member"));

    [RefusedCase]
    public Task a_word_after_a_run_of_capitals_starts_at_the_runs_last_capital()
        => InBed("public void On(string {|#0:DOMPath|}) { }", NamedFor(0, "Bed.On", "DOMPath", "path"));

    [RefusedCase]
    public Task a_word_written_in_capitals_is_one_word()
        => InBed("public void On(string {|#0:PATH|}) { }", NamedFor(0, "Bed.On", "PATH", "path"));

    [RefusedCase]
    public Task a_word_ends_at_an_underscore()
        => InBed("public void On(string {|#0:member_name|}) { }", NamedFor(0, "Bed.On", "member_name", "member"));

    [RefusedCase]
    public Task a_word_ends_at_a_digit()
        => InBed("public void On(string {|#0:path2|}) { }", NamedFor(0, "Bed.On", "path2", "path"));

    [RefusedCase]
    public Task a_word_ends_at_a_capital_that_ends_the_name()
        => InBed("public void On(string {|#0:pathA|}) { }", NamedFor(0, "Bed.On", "pathA", "path"));

    [AcceptedCase]
    public Task a_listed_word_inside_a_longer_word_is_not_that_word()
        => InBed("public void On(string remembered, string operation, string eventual, string pathology, string kindness) { }");

    [RefusedCase]
    public Task a_listed_words_plural_is_refused()
        => InBed(
            "public void On(string[] {|#0:members|}, params string[] {|#1:properties|}) { }",
            NamedFor(0, "Bed.On", "members", "member"),
            NamedFor(1, "Bed.On", "properties", "property"));

    [RefusedCase]
    public Task an_array_of_arrays_of_strings_is_read_as_strings()
        => InBed("public void On(string[][] {|#0:paths|}) { }", NamedFor(0, "Bed.On", "paths", "path"));

    [AcceptedCase]
    public Task a_number_named_for_a_member_is_accepted() => InBed("public void On(int memberCount) { }");

    [AcceptedCase]
    public Task a_typed_lambda_named_for_a_member_is_accepted()
        => InBed("public void On(System.Linq.Expressions.Expression<System.Func<Bed, string>> member) { }");

    [AcceptedCase]
    public Task a_string_the_compiler_fills_with_its_callers_name_file_or_argument_is_accepted()
        => InBed(
            """
            public void Declare(
                int size,
                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
                [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(size))] string memberExpression = "") { }
            """);

    [RefusedCase]
    public Task a_string_that_carries_another_attribute_is_refused()
        => InBed(
            "public void Declare([System.Diagnostics.CodeAnalysis.DisallowNull] string? {|#0:memberName|}) { }",
            NamedFor(0, "Bed.Declare", "memberName", "member"));

    [RefusedCase]
    public Task a_string_with_a_default_the_developer_fills_is_refused()
        => InBed("public void Declare(string {|#0:memberName|} = \"\") { }", NamedFor(0, "Bed.Declare", "memberName", "member"));

    [RefusedCase]
    public Task an_indexers_string_named_for_a_member_is_refused()
        => InBed("public int this[string {|#0:memberName|}] => 0;", NamedFor(0, "Bed.this[]", "memberName", "member"));

    [RefusedCase]
    public Task a_delegates_string_named_for_an_event_is_refused()
        => InBeds("public delegate void Raised(string {|#0:eventName|});", NamedFor(0, "Raised", "eventName", "event"));

    [AcceptedCase]
    public Task an_exemption_with_its_reason_exempts_its_method()
        => InBed(
            $"[TypedDslExemption({VendorBoundary})] public void Define(string name, string objectPath, string subscribe, string unsubscribe, object options) {{ }}",
            Exempt);

    [AcceptedCase]
    public Task an_exemption_with_its_reason_exempts_its_constructor()
        => InBed($"[TypedDslExemption({VendorBoundary})] public Bed(object resident) {{ }}", Exempt);

    [AcceptedCase]
    public Task an_exemption_with_its_reason_exempts_its_property()
        => InBed($"[TypedDslExemption({VendorBoundary})] public object Resident {{ get; }} = 1;", Exempt);

    [AcceptedCase]
    public Task an_exemption_whose_reason_is_a_constant_exempts()
        => InBed("private const string Why = \"Syncfusion's slot is typed object.\"; [TypedDslExemption(Why)] public void Assign(object resident) { }", Exempt);

    [RefusedCase]
    public Task an_exemption_exempts_its_own_member_alone()
        => InBed(
            $"[TypedDslExemption({VendorBoundary})] public void Define(string objectPath) {{ }} public void Find(string {{|#0:objectPath|}}) {{ }}",
            Exempt,
            NamedFor(0, "Bed.Find", "objectPath", "path"));

    [RefusedCase]
    public Task an_exemption_with_an_empty_reason_is_refused_at_its_reason_and_exempts_nothing()
        => InBed(
            "[TypedDslExemption({|#0:\"\"|})] public void Define(string {|#1:objectPath|}) { }",
            Exempt,
            SaysWhy(0),
            NamedFor(1, "Bed.Define", "objectPath", "path"));

    [RefusedCase]
    public Task an_exemption_whose_reason_is_white_space_is_refused_at_its_reason()
        => InBed(
            "[TypedDslExemption({|#0:\" \\t\"|})] public void Assign(object {|#1:resident|}) { }",
            Exempt,
            SaysWhy(0),
            Names(1, "Bed.Assign", "object"));

    [RefusedCase]
    public Task an_exemption_whose_reason_is_null_is_refused_at_its_reason()
        => InBed("[TypedDslExemption({|#0:null!|})] public void Assign(int resident) { }", Exempt, SaysWhy(0));

    [RefusedCase]
    public Task an_exemption_whose_reason_is_an_empty_constant_is_refused_where_it_is_written()
        => InBed("private const string Why = \"\"; [TypedDslExemption(reason: {|#0:Why|})] public void Assign(int resident) { }", Exempt, SaysWhy(0));

    [RefusedCase]
    public Task an_exemption_with_an_empty_reason_is_refused_on_a_member_that_is_no_door()
        => InBed("[TypedDslExemption({|#0:\"\"|})] private void Assign(object resident) { }", Exempt, SaysWhy(0));

    [RefusedCase]
    public Task an_attribute_of_the_same_name_that_is_not_the_cores_exempts_nothing()
        => InBeds(
            """
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public sealed class TypedDslExemptionAttribute(string reason) : System.Attribute { public string Reason => reason; }
            public sealed class Bed { [TypedDslExemption("The vendor boundary.")] public void Assign(object {|#0:resident|}) { } }
            """,
            Names(0, "Bed.Assign", "object"));

    [RefusedCase]
    public Task a_project_that_references_no_core_is_read_all_the_same()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithoutTheCore()
            .WithTable("Beds\n")
            .WithFile("Beds/Bed.cs", "namespace TestProject.Beds; public sealed class Bed { public void Assign(object {|#0:resident|}) { } }")
            .Expecting(Names(0, "Bed.Assign", "object"))
            .RunAsync();

    [AcceptedCase]
    public Task an_override_of_a_member_of_an_assembly_that_references_no_core_is_not_read()
        => InBed("public override bool Equals(object? obj) => true; public override int GetHashCode() => 0;");

    [AcceptedCase]
    public Task a_method_that_implements_a_member_of_an_assembly_that_references_no_core_is_not_read()
        => InBeds("public sealed class Bed : System.IComparable { public int CompareTo(object? obj) => 0; }");

    [AcceptedCase]
    public Task a_property_that_implements_a_member_of_an_assembly_that_references_no_core_is_not_read()
        => InBeds("public sealed class Beds : System.Collections.IEnumerator { public object Current => 1; public bool MoveNext() => false; public void Reset() { } }");

    [RefusedCase]
    public Task a_method_that_implements_a_member_of_its_own_project_is_read()
        => InBeds(
            "public interface IAssigns { void Assign(object {|#0:resident|}); } public sealed class Bed : IAssigns { public void Assign(object {|#1:resident|}) { } }",
            Names(0, "IAssigns.Assign", "object"),
            Names(1, "Bed.Assign", "object"));

    [RefusedCase]
    public Task a_method_that_implements_a_member_of_its_own_project_is_read_where_the_project_references_no_core()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithoutTheCore()
            .WithTable("Beds\n")
            .WithFile(
                "Beds/Bed.cs",
                "namespace TestProject.Beds; internal interface IAssigns { void Assign(object resident); } public sealed class Bed : IAssigns { public void Assign(object {|#0:resident|}) { } }")
            .Expecting(Names(0, "Bed.Assign", "object"))
            .RunAsync();

    [RefusedCase]
    public Task a_method_that_implements_a_member_of_the_core_is_read()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithoutTheCore()
            .WithAProjectThatReferencesNoCore("Alis.Reactive", "/Alis.Reactive/IAssigns.cs", "namespace Alis.Reactive; public interface IAssigns { void Assign(object resident); }")
            .WithTable("Beds\n")
            .WithFile("Beds/Bed.cs", "namespace TestProject.Beds; public sealed class Bed : Alis.Reactive.IAssigns { public void Assign(object {|#0:resident|}) { } }")
            .Expecting(Names(0, "Bed.Assign", "object"))
            .RunAsync();

    [RefusedCase]
    public Task a_public_method_that_implements_an_internal_interfaces_member_is_read()
        => InBeds(
            "internal interface IAssigns { void Assign(object resident); } public sealed class Bed : IAssigns { public void Assign(object {|#0:resident|}) { } }",
            Names(0, "Bed.Assign", "object"));

    [AcceptedCase]
    public Task an_explicit_implementation_is_no_door()
        => InBeds("internal interface IAssigns { void Assign(object resident); } public sealed class Bed : IAssigns { void IAssigns.Assign(object resident) { } }");

    [RefusedCase]
    public Task a_method_that_implements_a_member_of_an_assembly_that_references_the_core_is_read()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithAnotherProject("Brought", "/Brought/IAssigns.cs", "namespace Brought; public interface IAssigns { void Assign(object resident); }")
            .WithTable("Beds\n")
            .WithFile("Beds/Bed.cs", "namespace TestProject.Beds; public sealed class Bed : Brought.IAssigns { public void Assign(object {|#0:resident|}) { } }")
            .Expecting(Names(0, "Bed.Assign", "object"))
            .RunAsync();

    [AcceptedCase]
    public Task a_method_that_implements_a_member_of_a_project_that_references_no_core_is_not_read()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithAProjectThatReferencesNoCore("Vendor", "/Vendor/IAssigns.cs", "namespace Vendor; public interface IAssigns { void Assign(object resident); }")
            .WithTable("Beds\n")
            .WithFile("Beds/Bed.cs", "namespace TestProject.Beds; public sealed class Bed : Vendor.IAssigns { public void Assign(object resident) { } }")
            .RunAsync();

    [RefusedCase]
    public Task an_override_of_a_member_of_an_assembly_that_references_the_core_is_read()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithAnotherProject("Brought", "/Brought/Assigns.cs", "namespace Brought; public abstract class Assigns { public abstract void Assign(object resident); }")
            .WithTable("Beds\n")
            .WithFile("Beds/Bed.cs", "namespace TestProject.Beds; public sealed class Bed : Brought.Assigns { public override void Assign(object {|#0:resident|}) { } }")
            .Expecting(Names(0, "Bed.Assign", "object"))
            .RunAsync();

    [AcceptedCase]
    public Task an_override_of_an_override_of_a_member_of_a_project_that_references_no_core_is_not_read()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithAProjectThatReferencesNoCore(
                "Vendor",
                "/Vendor/Assigns.cs",
                """
                namespace Vendor;
                public abstract class Assigns
                {
                    public abstract object Resident { get; }
                    public abstract event System.Action<object> Assigned;
                    public abstract void Assign(object resident);
                }
                """)
            .WithTable("Beds\n")
            .WithFile(
                "Beds/Bed.cs",
                """
                namespace TestProject.Beds;
                public abstract class Ward : Vendor.Assigns
                {
                    public override object Resident => 1;
                    public override event System.Action<object>? Assigned;
                    public override void Assign(object resident) { }
                }
                public sealed class Bed : Ward
                {
                    public override object Resident => 2;
                    public override event System.Action<object>? Assigned;
                    public override void Assign(object resident) { }
                }
                """)
            .RunAsync();

    [RefusedCase]
    public Task a_door_in_a_file_outside_the_tables_folder_is_read()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithTable("Beds\n")
            .WithFileAt("/Elsewhere/Bed.cs", "namespace Elsewhere; public sealed class Bed { public void Assign(object {|#0:resident|}) { } }")
            .Expecting(Names(0, "Bed.Assign", "object"))
            .RunAsync();

    [AcceptedCase]
    public Task a_door_in_a_file_a_generator_writes_is_not_read()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithTable("Beds\n")
            .WithFile("Beds/Bed.g.cs", "namespace TestProject.Beds; public sealed class Bed { public void Assign(object resident) { } }")
            .RunAsync();

    [RefusedCase]
    public Task a_project_whose_table_holds_no_row_is_read()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithTable("")
            .WithFile("Bed.cs", "namespace TestProject; public sealed class Bed { public void Assign(object {|#0:resident|}) { } }")
            .Expecting(Names(0, "Bed.Assign", "object"))
            .RunAsync();

    [AcceptedCase]
    public Task a_project_that_hands_the_compiler_no_table_is_not_read()
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithFile("Beds/Bed.cs", "namespace TestProject.Beds; public sealed class Bed { public void Assign(object resident, string memberName) { } }")
            .RunAsync();

    [AcceptedCase]
    public Task source_that_does_not_compile_is_accepted_with_its_error()
        => InBed("[{|CS7036:TypedDslExemption|}] public void Assign(object resident, {|CS0246:Missing|} other) { }", Exempt);

    [AcceptedCase]
    public Task an_exemption_whose_reason_does_not_compile_is_accepted_with_its_error()
        => InBed("[TypedDslExemption({|CS0103:Missing|})] public void Assign(object resident) { }", Exempt);

    [AcceptedCase]
    public Task an_exemption_whose_reason_is_no_constant_is_accepted_with_its_error()
        => InBed("[TypedDslExemption({|CS0182:string.Concat(\"a\", \"b\")|})] public void Assign(object resident) { }", Exempt);

    private static Task InBed(string members, params DiagnosticResult[] reports) => InBed(members, "", reports);

    private static Task InBed(string members, string usings, params DiagnosticResult[] reports)
        => InBeds(usings, "public sealed class Bed { " + members + " }", reports);

    private static Task InBeds(string declarations, params DiagnosticResult[] reports) => InBeds("", declarations, reports);

    private static Task InBeds(string usings, string declarations, params DiagnosticResult[] reports)
        => new Case<ALIS009PublicDoorIsTyped>()
            .WithTable("Beds\n")
            .WithFile("Beds/Bed.cs", usings + "namespace TestProject.Beds; " + declarations)
            .Expecting(reports)
            .RunAsync();

    private static DiagnosticResult Names(int at, string door, string untyped)
        => new DiagnosticResult(ALIS009PublicDoorIsTyped.Id, DiagnosticSeverity.Error)
            .WithLocation(at)
            .WithMessage($"'{door}' is a public door and names '{untyped}': a door is typed; write the value's own type, a type parameter or a typed source");

    private static DiagnosticResult NamedFor(int at, string door, string parameter, string word)
        => new DiagnosticResult(ALIS009PublicDoorIsTyped.Id, DiagnosticSeverity.Error)
            .WithLocation(at)
            .WithMessage($"'{door}' takes the string '{parameter}', named for '{word}': a public door names a member, field, event, method or path by a typed lambda or a declaration, never by a string");

    private static DiagnosticResult SaysWhy(int at)
        => new DiagnosticResult(ALIS009PublicDoorIsTyped.Id, DiagnosticSeverity.Error)
            .WithLocation(at)
            .WithMessage("An exemption from ALIS009 says why; give TypedDslExemption its reason");
}
