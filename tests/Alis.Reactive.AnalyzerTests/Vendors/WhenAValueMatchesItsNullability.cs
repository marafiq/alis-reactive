using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAValueMatchesItsNullability
{
    private const string Type = "format\t1\ntype\tapi:Gauge@1\tapi\tGauge\tinterface\tGauge\n";
    private const string Value = Type + "member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\tfalse\tfalse\tfalse\n";
    private const string Nullable = Type + "member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\ttrue\ttrue\tfalse\n";
    private const string Method = Type + "member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tmethod\tfalse\tfalse\tfalse\ttrue\n";
    private const string Result = Method + "signature\tvalue/s0\tmember\tapi:Gauge@1.value\t0\tnone\ttrue\tfalse\n";

    [RefusedCase]
    public Task property_must_admit_a_nullable_read()
        => InGauge("public Property<int> Value => Property<{|#0:int|}>();", Nullable, "Declaration member 'Gauge.Value' reads null or undefined from vendor member 'value'; declare a nullable C# type");

    [AcceptedCase]
    public Task property_admits_a_nullable_read()
        => InGauge("public Property<int?> Value => Property<int?>();", Nullable);

    [AcceptedCase]
    public Task property_accepts_a_nonnullable_read()
        => InGauge("public Property<int> Value => Property<int>();", Value);

    [RefusedCase]
    public Task readonly_must_admit_a_nullable_read()
        => InGauge("public ReadOnly<string> Value => ReadOnly<{|#0:string|}>();", Nullable, "Declaration member 'Gauge.Value' reads null or undefined from vendor member 'value'; declare a nullable C# type");

    [AcceptedCase]
    public Task readonly_admits_a_nullable_read()
        => InGauge("public ReadOnly<string?> Value => ReadOnly<string?>();", Nullable);

    [AcceptedCase]
    public Task readonly_accepts_a_nonnullable_read()
        => InGauge("public ReadOnly<string> Value => ReadOnly<string>();", Value);

    [RefusedCase]
    public Task property_cannot_write_null_to_a_nonnullable_member()
        => InGauge("public Property<string?> Value => Property<{|#0:string?|}>();", Value, "Declaration member 'Gauge.Value' writes null to vendor member 'value', which does not take null; declare a non-nullable C# type");

    [AcceptedCase]
    public Task property_can_write_a_nonnullable_value()
        => InGauge("public Property<string> Value => Property<string>();", Value);

    [AcceptedCase]
    public Task property_can_write_null_to_a_nullable_member()
        => InGauge("public Property<string?> Value => Property<string?>();", Nullable);

    [RefusedCase]
    public Task writeonly_cannot_write_null_to_a_nonnullable_member()
        => InGauge("public WriteOnly<int?> Value => WriteOnly<{|#0:int?|}>();", Value, "Declaration member 'Gauge.Value' writes null to vendor member 'value', which does not take null; declare a non-nullable C# type");

    [AcceptedCase]
    public Task writeonly_can_write_a_nonnullable_value()
        => InGauge("public WriteOnly<int> Value => WriteOnly<int>();", Value);

    [AcceptedCase]
    public Task writeonly_can_write_null_to_a_nullable_member()
        => InGauge("public WriteOnly<int?> Value => WriteOnly<int?>();", Nullable);

    [AcceptedCase]
    public Task a_readonly_value_may_widen_to_nullable()
        => InGauge("public ReadOnly<int?> Value => ReadOnly<int?>();", Value);

    [AcceptedCase]
    public Task a_writeonly_value_may_narrow_from_nullable()
        => InGauge("public WriteOnly<string> Value => WriteOnly<string>();", Nullable);

    [AcceptedCase]
    public Task a_writeonly_door_does_not_read_null()
        => InGauge("public WriteOnly<int> Value => WriteOnly<int>();", Nullable);

    [AcceptedCase]
    public Task an_oblivious_reference_type_proves_neither_mark()
        => InGauge("\n#nullable disable\npublic ReadOnly<string> Value => ReadOnly<string>();\n", Nullable);

    [RefusedCase]
    public Task liveobject_requires_nullable_for_a_nullable_object()
        => InGauge("public Gauge {|#0:Value|} => LiveObject<Gauge>();", Nullable, "Declaration member 'Gauge.Value' reads a live object that vendor member 'value' can hold as null or undefined; declare nullable: true");

    [AcceptedCase]
    public Task liveobject_admits_a_nullable_object()
        => InGauge("public Gauge Value => LiveObject<Gauge>(nullable: true);", Nullable);

    [AcceptedCase]
    public Task liveobject_permits_nullable_for_a_nonnullable_object()
        => InGauge("public Gauge Value => LiveObject<Gauge>(nullable: true);", Value);

    [RefusedCase]
    public Task liveobjects_requires_nullable_for_a_nullable_object()
        => InGauge("public LiveObjects<Gauge> {|#0:Value|} => LiveObjects<Gauge>();", Nullable, "Declaration member 'Gauge.Value' reads a live object that vendor member 'value' can hold as null or undefined; declare nullable: true");

    [AcceptedCase]
    public Task liveobjects_admits_a_nullable_object()
        => InGauge("public LiveObjects<Gauge> Value => LiveObjects<Gauge>(nullable: true);", Nullable);

    [AcceptedCase]
    public Task liveobjects_permits_nullable_for_a_nonnullable_object()
        => InGauge("public LiveObjects<Gauge> Value => LiveObjects<Gauge>(nullable: true);", Value);

    [RefusedCase]
    public Task returnedobject_requires_nullable_for_a_nullable_object()
        => InGauge("public Gauge {|#0:Value|}() => ReturnedObject<Gauge>();", Result, "Declaration member 'Gauge.Value' reads a live object that vendor member 'value' can hold as null or undefined; declare nullable: true");

    [AcceptedCase]
    public Task returnedobject_admits_a_nullable_object()
        => InGauge("public Gauge Value() => ReturnedObject<Gauge>(nullable: true);", Result);

    [AcceptedCase]
    public Task returnedobject_permits_nullable_for_a_nonnullable_object()
        => InGauge("public Gauge Value() => ReturnedObject<Gauge>(nullable: true);", Result.Replace("\ttrue\tfalse\n", "\tfalse\tfalse\n"));

    [RefusedCase]
    public Task returnedobjects_requires_nullable_for_a_nullable_object()
        => InGauge("public LiveObjects<Gauge> {|#0:Value|}() => ReturnedObjects<Gauge>();", Result, "Declaration member 'Gauge.Value' reads a live object that vendor member 'value' can hold as null or undefined; declare nullable: true");

    [AcceptedCase]
    public Task returnedobjects_admits_a_nullable_object()
        => InGauge("public LiveObjects<Gauge> Value() => ReturnedObjects<Gauge>(nullable: true);", Result);

    [AcceptedCase]
    public Task returnedobjects_permits_nullable_for_a_nonnullable_object()
        => InGauge("public LiveObjects<Gauge> Value() => ReturnedObjects<Gauge>(nullable: true);", Result.Replace("\ttrue\tfalse\n", "\tfalse\tfalse\n"));

    [RefusedCase]
    public Task method_reads_its_nullable_result()
        => InGauge("public MethodResult<int> Value() => Method<{|#0:int|}>();", Result, "Declaration member 'Gauge.Value' reads null or undefined from vendor member 'value'; declare a nullable C# type");

    [AcceptedCase]
    public Task method_admits_its_nullable_result()
        => InGauge("public MethodResult<int?> Value() => Method<int?>();", Result);

    [RefusedCase]
    public Task method_with_arguments_reads_its_nullable_result()
        => InGauge("public MethodResult<int> Value() => Method<{|#0:int|}>([]);", Result, "Declaration member 'Gauge.Value' reads null or undefined from vendor member 'value'; declare a nullable C# type");

    [AcceptedCase]
    public Task method_with_arguments_admits_its_nullable_result()
        => InGauge("public MethodResult<int?> Value() => Method<int?>([]);", Result);

    [AcceptedCase]
    public Task method_without_a_result_does_not_read_null()
        => InGauge("public void Value() => Method();", Result);

    [RefusedCase]
    public Task methodasync_reads_its_nullable_result()
        => InGauge("public MethodResult<int> Value() => MethodAsync<{|#0:int|}>();", Result.Replace("\tnone\t", "\tpromise\t"), "Declaration member 'Gauge.Value' reads null or undefined from vendor member 'value'; declare a nullable C# type");

    [AcceptedCase]
    public Task methodasync_admits_its_nullable_result()
        => InGauge("public MethodResult<int?> Value() => MethodAsync<int?>();", Result.Replace("\tnone\t", "\tpromise\t"));

    [RefusedCase]
    public Task methodasync_with_arguments_reads_its_nullable_result()
        => InGauge("public MethodResult<int> Value() => MethodAsync<{|#0:int|}>([]);", Result.Replace("\tnone\t", "\tpromise\t"), "Declaration member 'Gauge.Value' reads null or undefined from vendor member 'value'; declare a nullable C# type");

    [AcceptedCase]
    public Task methodasync_with_arguments_admits_its_nullable_result()
        => InGauge("public MethodResult<int?> Value() => MethodAsync<int?>([]);", Result.Replace("\tnone\t", "\tpromise\t"));

    [AcceptedCase]
    public Task methodasync_without_a_result_does_not_read_null()
        => InGauge("public void Value() => MethodAsync();", Result.Replace("\tnone\t", "\tpromise\t"));

    [RefusedCase]
    public Task an_explicit_false_nullable_mark_is_the_edit_location()
        => InGauge("public Gauge Value => LiveObject<Gauge>(nullable: {|#0:false|});", Nullable, "Declaration member 'Gauge.Value' reads a live object that vendor member 'value' can hold as null or undefined; declare nullable: true");

    [AcceptedCase]
    public Task an_explicit_true_nullable_mark_matches()
        => InGauge("public Gauge Value => LiveObject<Gauge>(nullable: true);", Nullable);

    [AcceptedCase]
    public Task a_nonconstant_nullable_mark_is_left_to_alis024()
        => InGauge("private bool NullableValue => false; public Gauge Value => LiveObject<Gauge>(nullable: NullableValue);", Nullable);

    [RefusedCase]
    public Task a_nullable_result_on_a_later_overload_is_read()
        => InGauge("public MethodResult<int> Value() => Method<{|#0:int|}>();", Result.Replace("\ttrue\tfalse\n", "\tfalse\tfalse\n") + "signature\tvalue/s1\tmember\tapi:Gauge@1.value\t1\tnone\ttrue\tfalse\n", "Declaration member 'Gauge.Value' reads null or undefined from vendor member 'value'; declare a nullable C# type");

    [AcceptedCase]
    public Task all_nonnullable_results_fit_a_nonnullable_declaration()
        => InGauge("public MethodResult<int> Value() => Method<int>();", Result.Replace("\ttrue\tfalse\n", "\tfalse\tfalse\n") + "signature\tvalue/s1\tmember\tapi:Gauge@1.value\t1\tnone\tfalse\tfalse\n");

    [RefusedCase]
    public Task an_inherited_member_keeps_its_nullability()
        => InGauge("public Property<int> Value => Property<{|#0:int|}>();", Type + Nullable.Replace("format\t1\n", "").Replace("api:Gauge@1", "base").Replace("\tGauge\t", "\tBase\t") + "base\tapi:Gauge@1\t0\tbase\n", "Declaration member 'Gauge.Value' reads null or undefined from vendor member 'value'; declare a nullable C# type");

    [AcceptedCase]
    public Task an_own_member_shadows_base_nullability()
        => InGauge("public Property<int> Value => Property<int>();", Value + Nullable.Replace("format\t1\n", "").Replace("api:Gauge@1", "base").Replace("\tGauge\t", "\tBase\t") + "base\tapi:Gauge@1\t0\tbase\n");

    [RefusedCase]
    public Task documented_null_cites_numerictextboxs_tag()
        => InGauge("public Property<decimal> Value => Property<{|#0:decimal|}>();", Nullable + "default-null\tapi:Gauge@1.value\t@syncfusion/ej2-inputs\tsrc/numerictextbox/numerictextbox.d.ts\t67\ttrue\ttrue\n", "Declaration member 'Gauge.Value' reads null from vendor member 'value', documented @default null in module '@syncfusion/ej2-inputs' at 'src/numerictextbox/numerictextbox.d.ts:67'; declare a nullable C# type");

    [AcceptedCase]
    public Task documented_null_accepts_a_nullable_numerictextbox_value()
        => InGauge("public Property<decimal?> Value => Property<decimal?>();", Nullable + "default-null\tapi:Gauge@1.value\t@syncfusion/ej2-inputs\tsrc/numerictextbox/numerictextbox.d.ts\t67\ttrue\ttrue\n");

    [RefusedCase]
    public Task an_explicit_null_type_does_not_blame_the_documentation()
        => InGauge("public Property<decimal> Value => Property<{|#0:decimal|}>();", Nullable + "default-null\tapi:Gauge@1.value\t@syncfusion/ej2-inputs\tsrc/numerictextbox/numerictextbox.d.ts\t67\tfalse\tfalse\n", "Declaration member 'Gauge.Value' reads null or undefined from vendor member 'value'; declare a nullable C# type");

    [RefusedCase]
    public Task a_documented_live_object_cites_its_tag()
        => InGauge("public Gauge {|#0:Value|} => LiveObject<Gauge>();", Nullable + "default-null\tapi:Gauge@1.value\t@syncfusion/ej2-inputs\tsrc/numerictextbox/numerictextbox.d.ts\t67\ttrue\ttrue\n", "Declaration member 'Gauge.Value' reads a live object documented @default null in module '@syncfusion/ej2-inputs' at 'src/numerictextbox/numerictextbox.d.ts:67' on vendor member 'value'; declare nullable: true");

    [AcceptedCase]
    public Task a_documented_live_object_admits_null()
        => InGauge("public Gauge Value => LiveObject<Gauge>(nullable: true);", Nullable + "default-null\tapi:Gauge@1.value\t@syncfusion/ej2-inputs\tsrc/numerictextbox/numerictextbox.d.ts\t67\ttrue\ttrue\n");

    [AcceptedCase]
    public Task a_missing_member_is_left_to_alis016()
        => InGauge("public Property<int?> Value => Property<int?>();", Type);

    [AcceptedCase]
    public Task no_table_means_no_nullability_check()
        => InGauge("public Property<int?> Value => Property<int?>();", null);

    [AcceptedCase]
    public Task invalid_source_keeps_its_compiler_error()
        => InGauge("public Property<int> Value => Property<int>(name: {|CS0103:Unknown|});", Nullable);

    [AcceptedCase]
    public Task an_unmirrored_object_is_not_checked()
        => Source("public sealed class Gauge : JsObject { public Property<int> Value => Property<int>(); }", Nullable);

    [RefusedCase]
    public Task a_partial_property_marks_its_definition_once()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed partial class Gauge : JsObject { public partial Property<int> {|#0:Value|} { get; } } public sealed partial class Gauge { public partial Property<int> Value => Property<int>(); }", Nullable, "Declaration member 'Gauge.Value' reads null or undefined from vendor member 'value'; declare a nullable C# type");

    [AcceptedCase]
    public Task a_nullable_partial_property_is_accepted()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed partial class Gauge : JsObject { public partial Property<int?> Value { get; } } public sealed partial class Gauge { public partial Property<int?> Value => Property<int?>(); }", Nullable);

    [RefusedCase]
    public Task a_callback_answer_cannot_write_null()
        => InGauge("public Callback<Gauge, string?> Value => Callback<Gauge, {|#0:string?|}>();", Result.Replace("\tmethod\t", "\tproperty\t").Replace("\ttrue\tfalse\n", "\tfalse\tfalse\n") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tother\t\tstring\n", "Declaration member 'Gauge.Value' answers vendor callback 'value' with null, which its result does not take; declare a non-nullable result type");

    [AcceptedCase]
    public Task a_callback_answer_can_write_a_nonnullable_value()
        => InGauge("public Callback<Gauge, string> Value => Callback<Gauge, string>();", Result.Replace("\tmethod\t", "\tproperty\t").Replace("\ttrue\tfalse\n", "\tfalse\tfalse\n") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tother\t\tstring\n");

    [AcceptedCase]
    public Task a_callback_answer_can_write_null_when_its_result_takes_null()
        => InGauge("public Callback<Gauge, string?> Value => Callback<Gauge, string?>();", Result.Replace("\tmethod\t", "\tproperty\t") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tnull\t\t\n");

    [RefusedCase]
    public Task a_callback_returning_undefined_does_not_take_null()
        => InGauge("public Callback<Gauge, string?> Value => Callback<Gauge, {|#0:string?|}>();", Result.Replace("\tmethod\t", "\tproperty\t").Replace("\ttrue\tfalse\n", "\ttrue\ttrue\n") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tundefined\t\t\n", "Declaration member 'Gauge.Value' answers vendor callback 'value' with null, which its result does not take; declare a non-nullable result type");

    [RefusedCase]
    public Task a_callback_answer_must_fit_every_overload()
        => InGauge("public Callback<Gauge, string?> Value => Callback<Gauge, {|#0:string?|}>();", Result.Replace("\tmethod\t", "\tproperty\t") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tnull\t\t\nsignature\tvalue/s1\tmember\tapi:Gauge@1.value\t1\tnone\tfalse\tfalse\nholds\tvalue/s1/returns/0\tsignature\tvalue/s1\treturns\t0\tother\t\tstring\n", "Declaration member 'Gauge.Value' answers vendor callback 'value' with null, which its result does not take; declare a non-nullable result type");

    [AcceptedCase]
    public Task a_callback_with_an_unknown_result_is_not_guessed()
        => InGauge("public Callback<Gauge, string?> Value => Callback<Gauge, string?>();", Result.Replace("\tmethod\t", "\tproperty\t"));

    [AcceptedCase]
    public Task a_callback_with_an_untyped_result_takes_null()
        => InGauge("public Callback<Gauge, string?> Value => Callback<Gauge, string?>();", Result.Replace("\tmethod\t", "\tproperty\t").Replace("\ttrue\tfalse\n", "\ttrue\ttrue\n") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tother\t\tany\n");

    [AcceptedCase]
    public Task a_project_without_the_core_has_no_value_doors()
    {
        var test = new Case<ALIS019ValueNullability>().WithoutTheCore().WithFile("Gauge.cs", "public sealed class Gauge { public int Value => 1; }");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", Nullable));
        return test.RunAsync();
    }

    [AcceptedCase]
    public Task a_callback_answer_can_fit_every_overload()
        => InGauge("public Callback<Gauge, string?> Value => Callback<Gauge, string?>();", Result.Replace("\tmethod\t", "\tproperty\t") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tnull\t\t\nsignature\tvalue/s1\tmember\tapi:Gauge@1.value\t1\tnone\ttrue\tfalse\nholds\tvalue/s1/returns/0\tsignature\tvalue/s1\treturns\t0\tnull\t\t\n");

    [AcceptedCase]
    public Task a_callback_with_undefined_result_can_answer_a_nonnullable_value()
        => InGauge("public Callback<Gauge, string> Value => Callback<Gauge, string>();", Result.Replace("\tmethod\t", "\tproperty\t").Replace("\ttrue\tfalse\n", "\ttrue\ttrue\n") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tundefined\t\t\n");

    [AcceptedCase]
    public Task an_explicit_null_type_accepts_a_nullable_value()
        => InGauge("public Property<decimal?> Value => Property<decimal?>();", Nullable + "default-null\tapi:Gauge@1.value\t@syncfusion/ej2-inputs\tsrc/numerictextbox/numerictextbox.d.ts\t67\tfalse\tfalse\n");

    [AcceptedCase]
    public Task an_invalid_return_conversion_keeps_its_compiler_error()
        => InGauge("public Property<int> Value => {|CS0029:Property<string?>()|};", Value);

    [RefusedCase]
    public Task a_callback_cannot_answer_null_for_a_promise_of_nullable()
        => InGauge("public Callback<Gauge, string?> Value => Callback<Gauge, {|#0:string?|}>();", Result.Replace("\tmethod\t", "\tproperty\t").Replace("\tnone\t", "\tpromise\t") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tnull\t\t\n", "Declaration member 'Gauge.Value' answers vendor callback 'value' with null, which its result does not take; declare a non-nullable result type");

    [AcceptedCase]
    public Task a_callback_nonnullable_answer_leaves_its_promise_type_to_alis037()
        => InGauge("public Callback<Gauge, string> Value => Callback<Gauge, string>();", Result.Replace("\tmethod\t", "\tproperty\t").Replace("\tnone\t", "\tpromise\t") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tnull\t\t\n");

    [AcceptedCase]
    public Task a_callback_return_union_can_itself_take_null()
        => InGauge("public Callback<Gauge, string?> Value => Callback<Gauge, string?>();", Result.Replace("\tmethod\t", "\tproperty\t").Replace("\tnone\t", "\tmixed\t") + "holds\tvalue/s0/returns/0\tsignature\tvalue/s0\treturns\t0\tnull\t\t\n");

    private static Task InGauge(string members, string? table, string? message = null)
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject { " + members + " }", table, message);

    private static Task Source(string source, string? table, string? message = null)
    {
        var test = new Case<ALIS019ValueNullability>().WithFile("Gauge.cs", "#nullable enable\nusing Alis.Reactive.Declarations; " + source);
        if (table is not null) test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", table));
        return test.Expecting(message is null ? [] : [new DiagnosticResult(ALIS019ValueNullability.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message)]).RunAsync();
    }
}
