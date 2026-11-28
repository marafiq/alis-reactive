using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAMirroredTypeIsResolved
{
    private const string Gauge = "format\t1\ntype\tapi:Gauge@1\tapi\tGauge\tinterface\tGauge\n";
    private const string Missing = "Declaration 'Gauge' mirrors type 'Gauge' in module 'api', which the vendor contract lacks; name a type in the project's TypeScript program";
    private const string Ambiguous = "Declaration 'Gauge' mirrors type 'Gauge' in module 'api', which the vendor contract lists more than once; give each TypeScript declaration its own module or name";

    [RefusedCase]
    public Task a_type_absent_from_the_table_is_reported_at_the_type_argument()
        => Source("[Mirrors(\"api\", {|#0:\"Gauge\"|})] public sealed class Gauge : JsObject;", "format\t1\n", Missing);

    [AcceptedCase]
    public Task a_named_type_in_the_module_is_accepted()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject;", Gauge);

    [AcceptedCase]
    public Task no_table_means_the_project_has_not_opted_in()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject;", null);

    [RefusedCase]
    public Task a_name_in_another_module_does_not_resolve_the_type()
        => Source("[Mirrors(\"api\", {|#0:\"Gauge\"|})] public sealed class Gauge : JsObject;", "format\t1\ntype\tother:Gauge@1\tother\tGauge\tinterface\tGauge\n", Missing);

    [RefusedCase]
    public Task a_module_name_keeps_its_case()
        => Source("[Mirrors(\"api\", {|#0:\"Gauge\"|})] public sealed class Gauge : JsObject;", "format\t1\ntype\tApi:Gauge@1\tApi\tGauge\tinterface\tGauge\n", Missing);

    [RefusedCase]
    public Task a_type_name_keeps_its_case()
        => Source("[Mirrors(\"api\", {|#0:\"Gauge\"|})] public sealed class Gauge : JsObject;", "format\t1\ntype\tapi:gauge@1\tapi\tgauge\tinterface\tgauge\n", Missing);

    [RefusedCase]
    public Task two_declarations_of_one_name_are_ambiguous()
        => Source("[Mirrors(\"api\", {|#0:\"Gauge\"|})] public sealed class Gauge : JsObject;", Gauge + "type\tapi:Gauge@2\tapi\tGauge\tinterface\tGauge\n", Ambiguous);

    [RefusedCase]
    public Task the_same_type_in_two_tables_is_still_ambiguous()
    {
        var test = SourceCase("[Mirrors(\"api\", {|#0:\"Gauge\"|})] public sealed class Gauge : JsObject;");
        test.TestState.AdditionalFiles.Add(("/TestProject/first/vendor-contract.tsv", Gauge));
        test.TestState.AdditionalFiles.Add(("/TestProject/second/vendor-contract.tsv", Gauge));
        return test.Expecting(Refused(Ambiguous)).RunAsync();
    }

    [AcceptedCase]
    public Task a_type_in_the_second_table_is_resolved()
    {
        var test = SourceCase("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject;");
        test.TestState.AdditionalFiles.Add(("/TestProject/first/vendor-contract.tsv", "format\t1\n"));
        test.TestState.AdditionalFiles.Add(("/TestProject/second/vendor-contract.tsv", Gauge));
        return test.RunAsync();
    }

    [AcceptedCase]
    public Task two_modules_can_declare_the_same_type_name()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject;", Gauge + "type\tother:Gauge@1\tother\tGauge\tinterface\tGauge\n");

    [RefusedCase]
    public Task a_listed_instantiation_is_not_a_named_declaration()
        => Source("[Mirrors(\"api\", {|#0:\"Gauge\"|})] public sealed class Gauge : JsObject;", "format\t1\ntype\tapi:Gauge#1\tapi\tGauge\tlisted\tGauge<string>\n", Missing);

    [AcceptedCase]
    public Task a_declared_alias_can_be_mirrored()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject;", "format\t1\ntype\tapi:Gauge@1\tapi\tGauge\talias\tGauge\n");

    [AcceptedCase]
    public Task a_component_can_mirror_a_class()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Component { public override ComponentVendor Vendor => ComponentVendor.Native; }", "format\t1\ntype\tapi:Gauge@1\tapi\tGauge\tclass\tGauge\n");

    [AcceptedCase]
    public Task a_plugin_can_mirror_a_function()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : Plugin { public Gauge() : base(\"gauge\") { } }", "format\t1\ntype\tapi:Gauge@1\tapi\tGauge\tfunction\tGauge\n");

    [AcceptedCase]
    public Task an_evented_object_can_name_its_type()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : EventedObject { protected override EventListening Listening => EventListening.EventTarget; }", Gauge);

    [RefusedCase]
    public Task reordered_named_arguments_mark_the_type_name()
        => Source("[Mirrors(name: {|#0:nameof(Gauge)|}, module: \"api\")] public sealed class Gauge : JsObject;", "format\t1\n", Missing);

    [AcceptedCase]
    public Task constant_arguments_name_the_vendor_type()
        => Source("[Mirrors(\"a\" + \"pi\", nameof(Gauge))] public sealed class Gauge : JsObject;", Gauge);

    [AcceptedCase]
    public Task an_unrelated_attribute_is_not_a_vendor_identity()
        => Source("public sealed class MirrorsAttribute : System.Attribute { public MirrorsAttribute(string module, string name) { } } [Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject;", "format\t1\n");

    [AcceptedCase]
    public Task a_class_that_is_not_a_declaration_is_not_checked()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge;", "format\t1\n");

    [AcceptedCase]
    public Task a_declaration_without_mirrors_is_left_to_alis014()
        => Source("public sealed class Gauge : Plugin { public Gauge() : base(\"gauge\") { } }", "format\t1\n");

    [AcceptedCase]
    public Task a_project_without_the_core_has_no_vendor_identity()
    {
        var test = new Case<ALIS015MirroredTypeExists>().WithoutTheCore()
            .WithFile("Gauge.cs", "public sealed class MirrorsAttribute : System.Attribute { public MirrorsAttribute(string module, string name) { } } [Mirrors(\"api\", \"Gauge\")] public sealed class Gauge;");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", "format\t1\n"));
        return test.RunAsync();
    }

    [AcceptedCase]
    public Task an_incomplete_attribute_keeps_its_compiler_error()
        => Source("[Mirrors(\"api\", {|CS0103:Missing|})] public sealed class Gauge : JsObject;", "format\t1\n");

    [AcceptedCase]
    public Task an_unknown_format_is_not_read_as_format_one()
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject;", "format\t2\n");

    [AcceptedCase]
    public Task an_unrelated_additional_file_is_not_a_contract()
    {
        var test = SourceCase("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject;");
        test.TestState.AdditionalFiles.Add(("/TestProject/not-vendor-contract.tsv", "format\t1\n"));
        return test.RunAsync();
    }

    [AcceptedCase]
    public async Task the_reader_accepts_the_extractors_escaped_module_and_type()
    {
        using var program = new VendorProgram("declare module 'care\\tward\\nline\\r\\\\t' { export namespace Ward { interface Gauge { value: number; } } }");
        var table = await program.Extract();
        await Source("[Mirrors(\"care\\tward\\nline\\r\\\\t\", \"Ward.Gauge\")] public sealed class Gauge : JsObject;", string.Join("\n", table));
    }

    [RefusedCase]
    public async Task two_types_the_extractor_writes_are_ambiguous()
    {
        using var program = new VendorProgram("import './node_modules/care/a'; import './node_modules/care/b';");
        program.Write("node_modules/care/a.d.ts", "export interface Gauge { first: number; }");
        program.Write("node_modules/care/b.d.ts", "export interface Gauge { second: number; }");
        var table = await program.Extract();
        await Source("[Mirrors(\"care\", {|#0:\"Gauge\"|})] public sealed class Gauge : JsObject;", string.Join("\n", table),
            "Declaration 'Gauge' mirrors type 'Gauge' in module 'care', which the vendor contract lists more than once; give each TypeScript declaration its own module or name");
    }

    private static Task Source(string source, string? table, string? message = null)
    {
        var test = SourceCase(source);
        if (table is not null) test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", table));
        return test.Expecting(message is null ? [] : [Refused(message)]).RunAsync();
    }

    private static Case<ALIS015MirroredTypeExists> SourceCase(string source)
        => new Case<ALIS015MirroredTypeExists>().WithFile("Gauge.cs", "using Alis.Reactive.Declarations; using Alis.Reactive.Vendors; using Alis.Reactive.Plugins; " + source);

    private static DiagnosticResult Refused(string message)
        => new DiagnosticResult(ALIS015MirroredTypeExists.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message);
}
