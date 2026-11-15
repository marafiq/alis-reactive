using System.Threading.Tasks;
using Alis.Reactive.Analyzers.Vendors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenADoorMatchesItsMemberKind
{
    private const string Type = "format\t1\ntype\tapi:Gauge@1\tapi\tGauge\tinterface\tGauge\n";
    private const string Property = Type + "member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\tfalse\tfalse\tfalse\n";
    private static readonly string Method = Property.Replace("\tproperty\t", "\tmethod\t").Replace("\tfalse\n", "\ttrue\n");
    private static readonly string ReadOnly = Property.Replace("\tproperty\t", "\treadonly\t");
    private static readonly string WriteOnly = Property.Replace("\tproperty\t", "\twriteonly\t");

    [RefusedCase]
    public Task property_cannot_declare_a_vendor_method()
        => InGauge("public Property<int> Value => {|#0:Property|}<int>();", Method, "Declaration member 'Gauge.Value' uses Property for vendor method 'value'; use Method or MethodAsync");

    [AcceptedCase]
    public Task property_can_declare_a_vendor_property()
        => InGauge("public Property<int> Value => Property<int>();", Method.Replace("\tmethod\t", "\tproperty\t"));

    [RefusedCase]
    public Task readonly_cannot_declare_a_vendor_method()
        => InGauge("public ReadOnly<int> Value => {|#0:ReadOnly|}<int>();", Method, "Declaration member 'Gauge.Value' uses ReadOnly for vendor method 'value'; use Method or MethodAsync");

    [AcceptedCase]
    public Task readonly_can_declare_a_vendor_property()
        => InGauge("public ReadOnly<int> Value => ReadOnly<int>();", Method.Replace("\tmethod\t", "\tproperty\t"));

    [RefusedCase]
    public Task writeonly_cannot_declare_a_vendor_method()
        => InGauge("public WriteOnly<int> Value => {|#0:WriteOnly|}<int>();", Method, "Declaration member 'Gauge.Value' uses WriteOnly for vendor method 'value'; use Method or MethodAsync");

    [AcceptedCase]
    public Task writeonly_can_declare_a_vendor_property()
        => InGauge("public WriteOnly<int> Value => WriteOnly<int>();", Method.Replace("\tmethod\t", "\tproperty\t"));

    [RefusedCase]
    public Task method_cannot_call_a_noncallable_property()
        => InGauge("public void Value() => {|#0:Method|}();", Property, "Declaration member 'Gauge.Value' uses Method for vendor member 'value', which cannot be called; use a property door that matches its access");

    [AcceptedCase]
    public Task method_can_call_a_callable_property()
        => InGauge("public void Value() => Method();", Property.Replace("\tfalse\n", "\ttrue\n"));

    [AcceptedCase]
    public Task method_can_call_a_vendor_method()
        => InGauge("public void Value() => Method();", Method);

    [RefusedCase]
    public Task methodasync_cannot_call_a_noncallable_property()
        => InGauge("public void Value() => {|#0:MethodAsync|}();", Property, "Declaration member 'Gauge.Value' uses MethodAsync for vendor member 'value', which cannot be called; use a property door that matches its access");

    [AcceptedCase]
    public Task methodasync_can_call_a_callable_property()
        => InGauge("public void Value() => MethodAsync();", Property.Replace("\tfalse\n", "\ttrue\n"));

    [AcceptedCase]
    public Task methodasync_can_call_a_vendor_method()
        => InGauge("public void Value() => MethodAsync();", Method);

    [RefusedCase]
    public Task property_cannot_write_a_readonly_member()
        => InGauge("public Property<int> Value => {|#0:Property|}<int>();", ReadOnly, "Declaration member 'Gauge.Value' uses Property for read-only vendor member 'value'; use ReadOnly");

    [RefusedCase]
    public Task writeonly_cannot_write_a_readonly_member()
        => InGauge("public WriteOnly<int> Value => {|#0:WriteOnly|}<int>();", ReadOnly, "Declaration member 'Gauge.Value' uses WriteOnly for read-only vendor member 'value'; use ReadOnly");

    [RefusedCase]
    public Task property_cannot_read_a_writeonly_member()
        => InGauge("public Property<int> Value => {|#0:Property|}<int>();", WriteOnly, "Declaration member 'Gauge.Value' uses Property for write-only vendor member 'value'; use WriteOnly");

    [RefusedCase]
    public Task readonly_cannot_read_a_writeonly_member()
        => InGauge("public ReadOnly<int> Value => {|#0:ReadOnly|}<int>();", WriteOnly, "Declaration member 'Gauge.Value' uses ReadOnly for write-only vendor member 'value'; use WriteOnly");

    [AcceptedCase]
    public Task readonly_can_read_a_readonly_member()
        => InGauge("public ReadOnly<int> Value => ReadOnly<int>();", ReadOnly);

    [AcceptedCase]
    public Task writeonly_can_write_a_writeonly_member()
        => InGauge("public WriteOnly<int> Value => WriteOnly<int>();", WriteOnly);

    [AcceptedCase]
    public Task a_readonly_callable_property_can_be_called()
        => InGauge("public void Value() => Method();", ReadOnly.Replace("\tfalse\n", "\ttrue\n"));

    [RefusedCase]
    public Task a_writeonly_callable_property_cannot_be_called()
        => InGauge("public void Value() => {|#0:Method|}();", WriteOnly.Replace("\tfalse\n", "\ttrue\n"), "Declaration member 'Gauge.Value' uses Method for vendor member 'value', which cannot be called; use a property door that matches its access");

    [AcceptedCase]
    public Task an_absent_member_is_left_to_alis016()
        => InGauge("public Property<int> Value => Property<int>();", Type);

    [AcceptedCase]
    public Task no_table_means_no_kind_check()
        => InGauge("public Property<int> Value => Property<int>();", null);

    [AcceptedCase]
    public Task invalid_source_keeps_its_compiler_error()
        => InGauge("public Property<int> Value => Property<int>(name: {|CS0103:Unknown|});", Method);

    [AcceptedCase]
    public Task a_project_without_the_core_has_no_property_doors()
    {
        var test = new Case<ALIS018DoorMatchesMemberKind>().WithoutTheCore().WithFile("Gauge.cs", "public sealed class Gauge { public int Value => 1; }");
        test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", Method));
        return test.RunAsync();
    }

    [RefusedCase]
    public async Task a_getter_only_vendor_property_refuses_writes()
    {
        using var program = new VendorProgram("export class Gauge { get value(): number { return 1; } }");
        var table = await program.Extract();
        await InGauge("public Property<int> Value => {|#0:Property|}<int>();", string.Join("\n", table), "Declaration member 'Gauge.Value' uses Property for read-only vendor member 'value'; use ReadOnly");
    }

    [AcceptedCase]
    public async Task a_getter_and_setter_vendor_property_accepts_writes()
    {
        using var program = new VendorProgram("export class Gauge { get value(): number { return 1; } set value(value: number) { } }");
        var table = await program.Extract();
        await InGauge("public Property<int> Value => Property<int>();", string.Join("\n", table));
    }

    [RefusedCase]
    public Task an_own_member_shadows_a_writable_base_member()
        => InGauge("public Property<int> Value => {|#0:Property|}<int>();", ReadOnly + "base\tapi:Gauge@1\t0\tbase\n" + Property.Replace("format\t1\n", "").Replace("api:Gauge@1", "base").Replace("\tGauge\t", "\tBase\t"), "Declaration member 'Gauge.Value' uses Property for read-only vendor member 'value'; use ReadOnly");

    [AcceptedCase]
    public Task property_can_write_and_read_a_writable_readable_member()
        => InGauge("public Property<int> Value => Property<int>();", Property);

    [AcceptedCase]
    public Task writeonly_can_write_a_writable_member()
        => InGauge("public WriteOnly<int> Value => WriteOnly<int>();", Property);

    [AcceptedCase]
    public Task readonly_can_read_a_readable_member()
        => InGauge("public ReadOnly<int> Value => ReadOnly<int>();", Property);

    [RefusedCase]
    public Task liveobject_cannot_read_a_vendor_method()
        => InGauge("public Gauge Value => {|#0:LiveObject|}<Gauge>();", Method, "Declaration member 'Gauge.Value' uses LiveObject for vendor method 'value'; use Method or MethodAsync");

    [AcceptedCase]
    public Task liveobject_can_read_a_vendor_property()
        => InGauge("public Gauge Value => LiveObject<Gauge>();", Method.Replace("\tmethod\t", "\tproperty\t"));

    [RefusedCase]
    public Task liveobject_cannot_read_a_writeonly_member()
        => InGauge("public Gauge Value => {|#0:LiveObject|}<Gauge>();", WriteOnly, "Declaration member 'Gauge.Value' uses LiveObject for write-only vendor member 'value'; use WriteOnly");

    [AcceptedCase]
    public Task liveobject_can_read_a_readonly_member()
        => InGauge("public Gauge Value => LiveObject<Gauge>();", ReadOnly);

    [RefusedCase]
    public Task liveobjects_cannot_read_a_vendor_method()
        => InGauge("public LiveObjects<Gauge> Value => {|#0:LiveObjects|}<Gauge>();", Method, "Declaration member 'Gauge.Value' uses LiveObjects for vendor method 'value'; use Method or MethodAsync");

    [AcceptedCase]
    public Task liveobjects_can_read_a_vendor_property()
        => InGauge("public LiveObjects<Gauge> Value => LiveObjects<Gauge>();", Method.Replace("\tmethod\t", "\tproperty\t"));

    [RefusedCase]
    public Task liveobjects_cannot_read_a_writeonly_member()
        => InGauge("public LiveObjects<Gauge> Value => {|#0:LiveObjects|}<Gauge>();", WriteOnly, "Declaration member 'Gauge.Value' uses LiveObjects for write-only vendor member 'value'; use WriteOnly");

    [AcceptedCase]
    public Task liveobjects_can_read_a_readonly_member()
        => InGauge("public LiveObjects<Gauge> Value => LiveObjects<Gauge>();", ReadOnly);

    [RefusedCase]
    public Task returnedobject_cannot_call_a_noncallable_member()
        => InGauge("public Gauge Value() => {|#0:ReturnedObject|}<Gauge>();", Property, "Declaration member 'Gauge.Value' uses ReturnedObject for vendor member 'value', which cannot be called; use a property door that matches its access");

    [AcceptedCase]
    public Task returnedobject_can_call_a_callable_property()
        => InGauge("public Gauge Value() => ReturnedObject<Gauge>();", Property.Replace("\tfalse\n", "\ttrue\n"));

    [AcceptedCase]
    public Task returnedobject_can_call_a_vendor_method()
        => InGauge("public Gauge Value() => ReturnedObject<Gauge>();", Method);

    [RefusedCase]
    public Task returnedobject_cannot_call_a_writeonly_member()
        => InGauge("public Gauge Value() => {|#0:ReturnedObject|}<Gauge>();", WriteOnly.Replace("\tfalse\n", "\ttrue\n"), "Declaration member 'Gauge.Value' uses ReturnedObject for vendor member 'value', which cannot be called; use a property door that matches its access");

    [AcceptedCase]
    public Task returnedobject_can_call_a_readonly_callable_property()
        => InGauge("public Gauge Value() => ReturnedObject<Gauge>();", ReadOnly.Replace("\tfalse\n", "\ttrue\n"));

    [RefusedCase]
    public Task returnedobjects_cannot_call_a_noncallable_member()
        => InGauge("public LiveObjects<Gauge> Value() => {|#0:ReturnedObjects|}<Gauge>();", Property, "Declaration member 'Gauge.Value' uses ReturnedObjects for vendor member 'value', which cannot be called; use a property door that matches its access");

    [AcceptedCase]
    public Task returnedobjects_can_call_a_callable_property()
        => InGauge("public LiveObjects<Gauge> Value() => ReturnedObjects<Gauge>();", Property.Replace("\tfalse\n", "\ttrue\n"));

    [AcceptedCase]
    public Task returnedobjects_can_call_a_vendor_method()
        => InGauge("public LiveObjects<Gauge> Value() => ReturnedObjects<Gauge>();", Method);

    [RefusedCase]
    public Task returnedobjects_cannot_call_a_writeonly_member()
        => InGauge("public LiveObjects<Gauge> Value() => {|#0:ReturnedObjects|}<Gauge>();", WriteOnly.Replace("\tfalse\n", "\ttrue\n"), "Declaration member 'Gauge.Value' uses ReturnedObjects for vendor member 'value', which cannot be called; use a property door that matches its access");

    [AcceptedCase]
    public Task returnedobjects_can_call_a_readonly_callable_property()
        => InGauge("public LiveObjects<Gauge> Value() => ReturnedObjects<Gauge>();", ReadOnly.Replace("\tfalse\n", "\ttrue\n"));

    [RefusedCase]
    public Task callback_cannot_name_a_noncallable_member()
        => InGauge("public Callback<Gauge, int> Value => {|#0:Callback|}<Gauge, int>();", Property, "Declaration member 'Gauge.Value' uses Callback for vendor member 'value', which is not a writable callable property; name a writable callable property");

    [RefusedCase]
    public Task callback_cannot_name_a_readonly_member()
        => InGauge("public Callback<Gauge, int> Value => {|#0:Callback|}<Gauge, int>();", ReadOnly.Replace("\tfalse\n", "\ttrue\n"), "Declaration member 'Gauge.Value' uses Callback for vendor member 'value', which is not a writable callable property; name a writable callable property");

    [RefusedCase]
    public Task callback_cannot_name_a_writeonly_member()
        => InGauge("public Callback<Gauge, int> Value => {|#0:Callback|}<Gauge, int>();", WriteOnly.Replace("\tfalse\n", "\ttrue\n"), "Declaration member 'Gauge.Value' uses Callback for vendor member 'value', which is not a writable callable property; name a writable callable property");

    [RefusedCase]
    public Task callback_cannot_name_a_method_member()
        => InGauge("public Callback<Gauge, int> Value => {|#0:Callback|}<Gauge, int>();", Method, "Declaration member 'Gauge.Value' uses Callback for vendor member 'value', which is not a writable callable property; name a writable callable property");

    [AcceptedCase]
    public Task callback_can_name_a_writable_callable_property()
        => InGauge("public Callback<Gauge, int> Value => Callback<Gauge, int>();", Property.Replace("\tfalse\n", "\ttrue\n"));

    private static Task InGauge(string members, string? table, string? message = null)
        => Source("[Mirrors(\"api\", \"Gauge\")] public sealed class Gauge : JsObject { " + members + " }", table, message);

    private static Task Source(string source, string? table, string? message = null)
    {
        var test = new Case<ALIS018DoorMatchesMemberKind>().WithFile("Gauge.cs", "using Alis.Reactive.Declarations; " + source);
        if (table is not null) test.TestState.AdditionalFiles.Add(("/TestProject/obj/vendor-contract.tsv", table));
        return test.Expecting(message is null ? [] : [new DiagnosticResult(ALIS018DoorMatchesMemberKind.Id, DiagnosticSeverity.Error).WithLocation(0).WithMessage(message)]).RunAsync();
    }
}
