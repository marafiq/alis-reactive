using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAMemberMatchesItsPresence
{
    private const string Type = "format\t1\ntype\tapi:Gauge@1\tapi\tGauge\tinterface\tGauge\n";
    private const string Required = Type + "member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\tfalse\tfalse\tfalse\n";
    private const string Optional = Type + "member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\ttrue\tfalse\tfalse\tfalse\n";

    [RefusedCase]
    public Task property_cannot_make_a_required_member_optional()
        => InGauge("public Property<int> Value => Property<int>(optional: {|#0:true|});", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task property_can_declare_an_optional_member()
        => InGauge("public Property<int> Value => Property<int>(optional: true);", Optional);

    [RefusedCase]
    public Task property_must_mark_an_optional_member()
        => InGauge("public Property<int> {|#0:Value|} => Property<int>();", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task property_can_declare_a_required_member()
        => InGauge("public Property<int> Value => Property<int>();", Required);

    [RefusedCase]
    public Task readonly_cannot_make_a_required_member_optional()
        => InGauge("public ReadOnly<int> Value => ReadOnly<int>(optional: {|#0:true|});", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task readonly_can_declare_an_optional_member()
        => InGauge("public ReadOnly<int> Value => ReadOnly<int>(optional: true);", Optional);

    [RefusedCase]
    public Task readonly_must_mark_an_optional_member()
        => InGauge("public ReadOnly<int> {|#0:Value|} => ReadOnly<int>();", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task readonly_can_declare_a_required_member()
        => InGauge("public ReadOnly<int> Value => ReadOnly<int>();", Required);

    [RefusedCase]
    public Task writeonly_cannot_make_a_required_member_optional()
        => InGauge("public WriteOnly<int> Value => WriteOnly<int>(optional: {|#0:true|});", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task writeonly_can_declare_an_optional_member()
        => InGauge("public WriteOnly<int> Value => WriteOnly<int>(optional: true);", Optional);

    [RefusedCase]
    public Task writeonly_must_mark_an_optional_member()
        => InGauge("public WriteOnly<int> {|#0:Value|} => WriteOnly<int>();", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task writeonly_can_declare_a_required_member()
        => InGauge("public WriteOnly<int> Value => WriteOnly<int>();", Required);

    [RefusedCase]
    public Task method_cannot_make_a_required_member_optional()
        => InGauge("public void Value() => Method(optional: {|#0:true|});", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task method_can_declare_an_optional_member()
        => InGauge("public void Value() => Method(optional: true);", Optional);

    [RefusedCase]
    public Task method_must_mark_an_optional_member()
        => InGauge("public void {|#0:Value|}() => Method();", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task method_can_declare_a_required_member()
        => InGauge("public void Value() => Method();", Required);

    [RefusedCase]
    public Task methodasync_cannot_make_a_required_member_optional()
        => InGauge("public void Value() => MethodAsync(optional: {|#0:true|});", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task methodasync_can_declare_an_optional_member()
        => InGauge("public void Value() => MethodAsync(optional: true);", Optional);

    [RefusedCase]
    public Task methodasync_must_mark_an_optional_member()
        => InGauge("public void {|#0:Value|}() => MethodAsync();", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task methodasync_can_declare_a_required_member()
        => InGauge("public void Value() => MethodAsync();", Required);

    [RefusedCase]
    public Task liveobject_cannot_make_a_required_member_optional()
        => InGauge("public Gauge Value => LiveObject<Gauge>(optional: {|#0:true|});", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task liveobject_can_declare_an_optional_member()
        => InGauge("public Gauge Value => LiveObject<Gauge>(optional: true);", Optional);

    [RefusedCase]
    public Task liveobject_must_mark_an_optional_member()
        => InGauge("public Gauge {|#0:Value|} => LiveObject<Gauge>();", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task liveobject_can_declare_a_required_member()
        => InGauge("public Gauge Value => LiveObject<Gauge>();", Required);

    [RefusedCase]
    public Task liveobjects_cannot_make_a_required_member_optional()
        => InGauge("public LiveObjects<Gauge> Value => LiveObjects<Gauge>(optional: {|#0:true|});", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task liveobjects_can_declare_an_optional_member()
        => InGauge("public LiveObjects<Gauge> Value => LiveObjects<Gauge>(optional: true);", Optional);

    [RefusedCase]
    public Task liveobjects_must_mark_an_optional_member()
        => InGauge("public LiveObjects<Gauge> {|#0:Value|} => LiveObjects<Gauge>();", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task liveobjects_can_declare_a_required_member()
        => InGauge("public LiveObjects<Gauge> Value => LiveObjects<Gauge>();", Required);

    [RefusedCase]
    public Task returnedobject_cannot_make_a_required_member_optional()
        => InGauge("public Gauge Value() => ReturnedObject<Gauge>(optional: {|#0:true|});", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task returnedobject_can_declare_an_optional_member()
        => InGauge("public Gauge Value() => ReturnedObject<Gauge>(optional: true);", Optional);

    [RefusedCase]
    public Task returnedobject_must_mark_an_optional_member()
        => InGauge("public Gauge {|#0:Value|}() => ReturnedObject<Gauge>();", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task returnedobject_can_declare_a_required_member()
        => InGauge("public Gauge Value() => ReturnedObject<Gauge>();", Required);

    [RefusedCase]
    public Task returnedobjects_cannot_make_a_required_member_optional()
        => InGauge("public LiveObjects<Gauge> Value() => ReturnedObjects<Gauge>(optional: {|#0:true|});", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task returnedobjects_can_declare_an_optional_member()
        => InGauge("public LiveObjects<Gauge> Value() => ReturnedObjects<Gauge>(optional: true);", Optional);

    [RefusedCase]
    public Task returnedobjects_must_mark_an_optional_member()
        => InGauge("public LiveObjects<Gauge> {|#0:Value|}() => ReturnedObjects<Gauge>();", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task returnedobjects_can_declare_a_required_member()
        => InGauge("public LiveObjects<Gauge> Value() => ReturnedObjects<Gauge>();", Required);

    [RefusedCase]
    public Task callback_cannot_make_a_required_member_optional()
        => InGauge("public Callback<Gauge, int> Value => Callback<Gauge, int>(optional: {|#0:true|});", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task callback_can_declare_an_optional_member()
        => InGauge("public Callback<Gauge, int> Value => Callback<Gauge, int>(optional: true);", Optional);

    [RefusedCase]
    public Task callback_must_mark_an_optional_member()
        => InGauge("public Callback<Gauge, int> {|#0:Value|} => Callback<Gauge, int>();", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task callback_can_declare_a_required_member()
        => InGauge("public Callback<Gauge, int> Value => Callback<Gauge, int>();", Required);

    [RefusedCase]
    public Task an_explicit_false_is_the_edit_location()
        => InGauge("public Property<int> Value => Property<int>(optional: {|#0:false|});", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task an_explicit_false_matches_a_required_member()
        => InGauge("public Property<int> Value => Property<int>(optional: false);", Required);

    [AcceptedCase]
    public Task a_nonconstant_mark_is_left_to_alis024()
        => InGauge("private bool IsOptional => true; public Property<int> Value => Property<int>(optional: IsOptional);", Required);

    [RefusedCase]
    public Task a_named_member_uses_its_optional_mark()
        => InGauge("public Property<int> Other => Property<int>(name: \"value\", optional: {|#0:true|});", Required, "Declaration member 'Gauge.Other' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task a_named_member_can_be_optional()
        => InGauge("public Property<int> Other => Property<int>(name: \"value\", optional: true);", Optional);

    [RefusedCase]
    public Task nullable_does_not_make_a_member_optional()
        => InGauge("public Gauge {|#0:Value|} => LiveObject<Gauge>(nullable: true);", Optional, "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task nullable_and_optional_are_independent_marks()
        => InGauge("public Gauge Value => LiveObject<Gauge>(nullable: true, optional: true);", Optional);

    [AcceptedCase]
    public Task no_table_means_no_presence_check()
        => InGauge("public Property<int> Value => Property<int>(optional: true);", null);

    [AcceptedCase]
    public Task invalid_source_keeps_its_compiler_error()
        => InGauge("public Property<int> Value => Property<int>(optional: {|CS0103:Unknown|});", Required);

    [AcceptedCase]
    public Task a_missing_member_is_left_to_alis016()
        => InGauge("public Property<int> Value => Property<int>(optional: true);", Type);

    [AcceptedCase]
    public Task an_unmirrored_object_is_not_checked()
        => Source("public sealed class Gauge : JsObject { public Property<int> Value => Property<int>(optional: true); }", Required);

    [RefusedCase]
    public Task a_partial_property_reports_once_at_its_definition()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed partial class Gauge : JsObject { public partial Property<int> {|#0:Value|} { get; } } public sealed partial class Gauge { public partial Property<int> Value => Property<int>(optional: true); }", Required, "Declaration member 'Gauge.Value' names required vendor member 'value'; remove optional: true");

    [AcceptedCase]
    public Task a_partial_property_can_match_the_mark()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed partial class Gauge : JsObject { public partial Property<int> Value { get; } } public sealed partial class Gauge { public partial Property<int> Value => Property<int>(optional: true); }", Optional);

    [RefusedCase]
    public Task a_base_supplies_its_optional_mark()
        => InGauge("public Property<int> {|#0:Value|} => Property<int>();", Type + Optional.Replace("format\t1\n", "").Replace("api:Gauge@1", "base").Replace("\tGauge\t", "\tBase\t") + "base\tapi:Gauge@1\t0\tbase\n", "Declaration member 'Gauge.Value' names optional vendor member 'value'; declare optional: true");

    [AcceptedCase]
    public Task an_own_required_member_shadows_an_optional_base()
        => InGauge("public Property<int> Value => Property<int>();", Required + Optional.Replace("format\t1\n", "").Replace("api:Gauge@1", "base").Replace("\tGauge\t", "\tBase\t") + "base\tapi:Gauge@1\t0\tbase\n");

    [AcceptedCase]
    public Task a_project_without_the_core_has_no_presence_marks()
    {
        var test = new Case<ALIS022MemberPresence>().WithoutTheCore().WithFile("Gauge.cs", "public sealed class Gauge { public int Value => 1; }");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", Optional));
        return test.RunAsync();
    }

    private static Task InGauge(string members, string? table, string? message = null)
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject { " + members + " }", table, message);

    private static Task Source(string source, string? table, string? message = null)
    {
        var test = new Case<ALIS022MemberPresence>().WithFile("Gauge.cs", "#nullable enable\nusing Alis.Reactive.Declarations; " + source);
        if (table is not null) test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", table));
        return test.Expecting(message is null ? [] : [new DiagnosticResult(ALIS022MemberPresence.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message)]).RunAsync();
    }
}
