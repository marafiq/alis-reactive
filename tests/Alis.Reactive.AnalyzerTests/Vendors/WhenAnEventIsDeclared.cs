using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAnEventIsDeclared
{
    private const string Type = "format\t1\ntype\tapi:Gauge@1\tapi\tGauge\tinterface\tGauge\n";
    private const string Property = Type + "member\traised\tapi:Gauge@1\traised\tproperty\tfalse\tfalse\tfalse\ttrue\n";
    private const string Method = Type + "member\ton\tapi:Gauge@1\ton\tmethod\tfalse\tfalse\tfalse\ttrue\nevent\ton\ton/s0\tname\traised\tlistener/s0\n";
    private const string Missing = "Declaration member 'Gauge.Raised' names event 'raised', which type 'Gauge' in module 'api' and its bases do not declare; name an event in the vendor's TypeScript contract";

    [RefusedCase]
    public Task a_missing_event_is_refused()
        => InGauge("public Event {|#0:Raised|} => Event();", Type, Missing);

    [AcceptedCase]
    public Task a_callable_property_declares_an_event()
        => InGauge("public Event Raised => Event();", Property);

    [RefusedCase]
    public Task a_noncallable_property_declares_no_event()
        => InGauge("public Event {|#0:Raised|} => Event();", Property.Replace("\ttrue\n", "\tfalse\n"), Missing);

    [RefusedCase]
    public Task a_method_named_like_the_event_is_no_callback_property()
        => InGauge("public Event {|#0:Raised|} => Event();", Property.Replace("\tproperty\t", "\tmethod\t"), Missing);

    [AcceptedCase]
    public Task a_readonly_callback_still_declares_the_event()
        => InGauge("public Event Raised => Event();", Property.Replace("\tproperty\t", "\treadonly\t"));

    [AcceptedCase]
    public Task a_literal_on_any_method_declares_an_event()
        => InGauge("public Event Raised => Event();", Method);

    [RefusedCase]
    public Task an_explicit_event_name_is_the_edit_location()
        => InGauge("public Event Raised => Event({|#0:\"missing\"|});", Property, "Declaration member 'Gauge.Raised' names event 'missing', which type 'Gauge' in module 'api' and its bases do not declare; name an event in the vendor's TypeScript contract");

    [AcceptedCase]
    public Task an_explicit_event_name_is_read()
        => InGauge("public Event Other => Event(\"raised\");", Property);

    [AcceptedCase]
    public Task a_null_event_name_uses_camel_case()
        => InGauge("public Event Raised => Event(name: null);", Property);

    [RefusedCase]
    public Task a_payload_event_requires_the_same_name()
        => InGauge("public Event<Gauge> {|#0:Raised|} => Event<Gauge>();", Type, Missing);

    [AcceptedCase]
    public Task a_payload_event_names_a_declared_event()
        => InGauge("public Event<Gauge> Raised => Event<Gauge>();", Method);

    [AcceptedCase]
    public Task an_inherited_callback_property_declares_an_event()
        => InGauge("public Event Raised => Event();", Type + Property.Replace("format\t1\n", "").Replace("api:Gauge@1", "base").Replace("\tGauge\t", "\tBase\t") + "base\tapi:Gauge@1\t0\tbase\n");

    [AcceptedCase]
    public Task an_inherited_method_declares_an_event()
        => InGauge("public Event Raised => Event();", Type + Method.Replace("format\t1\n", "").Replace("api:Gauge@1", "base").Replace("\tGauge\t", "\tBase\t") + "base\tapi:Gauge@1\t0\tbase\n");

    [RefusedCase]
    public Task an_unrelated_type_does_not_supply_an_event()
        => InGauge("public Event {|#0:Raised|} => Event();", Type + Property.Replace("format\t1\n", "").Replace("api:Gauge@1", "base").Replace("\tGauge\t", "\tBase\t"), Missing);

    [RefusedCase]
    public Task a_base_cycle_terminates_without_an_event()
        => InGauge("public Event {|#0:Raised|} => Event();", Type + "base\tapi:Gauge@1\t0\tapi:Gauge@1\n", Missing);

    [AcceptedCase]
    public Task no_table_means_no_event_check()
        => InGauge("public Event Raised => Event();", null);

    [AcceptedCase]
    public Task an_unknown_root_is_left_to_alis015()
        => InGauge("public Event Raised => Event();", "format\t1\n");

    [AcceptedCase]
    public Task an_ambiguous_root_is_left_to_alis015()
        => InGauge("public Event Raised => Event();", Type + "type\tother\tapi\tGauge\tinterface\tGauge\n");

    [AcceptedCase]
    public Task an_unmirrored_reach_is_outside_this_piece()
        => Source("public sealed class Gauge : JsObject { public Event Raised => Event(); }", Type);

    [AcceptedCase]
    public Task a_nonconstant_name_is_left_to_alis024()
        => InGauge("private string Name => \"raised\"; public Event Raised => Event(Name);", Type);

    [AcceptedCase]
    public Task invalid_source_keeps_its_compiler_error()
        => InGauge("public Event Raised => Event({|CS0103:Unknown|});", Type);

    [RefusedCase]
    public Task a_partial_event_is_reported_once_at_its_definition()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed partial class Gauge : JsObject { public partial Event {|#0:Raised|} { get; } } public sealed partial class Gauge { public partial Event Raised => Event(); }", Type, Missing);

    [AcceptedCase]
    public Task a_partial_event_can_name_an_event()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed partial class Gauge : JsObject { public partial Event Raised { get; } } public sealed partial class Gauge { public partial Event Raised => Event(); }", Property);

    [AcceptedCase]
    public Task a_project_without_the_core_is_not_checked()
    {
        var test = new Case<ALIS020EventIsDeclared>().WithoutTheCore().WithFile("Gauge.cs", "public sealed class Gauge { public int Raised => 1; }");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", Type));
        return test.RunAsync();
    }

    [RefusedCase]
    public async Task a_string_first_parameter_names_no_event()
    {
        using var program = new VendorProgram("export interface Gauge { on(name: string, listener: () => void): void; }");
        await InGauge("public Event {|#0:Raised|} => Event();", string.Join("\n", await program.Extract()), Missing);
    }

    [AcceptedCase]
    public async Task a_literal_union_first_parameter_names_its_events()
    {
        using var program = new VendorProgram("export interface Gauge { on(name: 'raised' | 'closed', listener: () => void): void; }");
        await InGauge("public Event Raised => Event();", string.Join("\n", await program.Extract()));
    }

    [RefusedCase]
    public async Task a_nonfunction_second_parameter_names_no_event()
    {
        using var program = new VendorProgram("export interface Gauge { on(name: 'raised', listener: number): void; }");
        await InGauge("public Event {|#0:Raised|} => Event();", string.Join("\n", await program.Extract()), Missing);
    }

    [AcceptedCase]
    public async Task a_referenced_event_map_includes_its_bases()
    {
        using var program = new VendorProgram("export interface Base { raised: number; } export interface Events extends Base { closed: string; } export interface Gauge { on<K extends keyof Events>(name: K, listener: (value: Events[K]) => void): void; }");
        await InGauge("public Event Raised => Event();", string.Join("\n", await program.Extract()));
    }

    [RefusedCase]
    public async Task an_event_map_does_not_name_an_unlisted_event()
    {
        using var program = new VendorProgram("export interface Base { other: number; } export interface Events extends Base { closed: string; } export interface Gauge { on<K extends keyof Events>(name: K, listener: (value: Events[K]) => void): void; }");
        await InGauge("public Event {|#0:Raised|} => Event();", string.Join("\n", await program.Extract()), Missing);
    }

    [AcceptedCase]
    public async Task a_dom_event_map_is_read_by_reference()
    {
        using var program = new VendorProgram("export interface Gauge extends HTMLElement { }");
        program.Write("tsconfig.json", """{"compilerOptions":{"strict":true,"lib":["dom","es2022"],"types":[]},"files":["api.ts"]}""");
        await InGauge("public Event Raised => Event(\"click\");", string.Join("\n", await program.Extract()));
    }

    private static Task InGauge(string members, string? table, string? message = null)
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject { " + members + " }", table, message);

    private static Task Source(string source, string? table, string? message = null)
    {
        var test = new Case<ALIS020EventIsDeclared>().WithFile("Gauge.cs", "using Alis.Reactive.Declarations; " + source);
        if (table is not null) test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", table));
        return test.Expecting(message is null ? [] : [new DiagnosticResult("ALIS020", DiagnosticSeverity.Error).WithLocation(0).WithMessage(message)]).RunAsync();
    }
}
