using System.Threading.Tasks;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAVendorDeclaresMembers
{
    [RefusedCase]
    public async Task private_protected_hash_private_and_documented_private_members_are_left_out()
    {
        using var program = new VendorProgram("""
            export declare class Gauge {
                private secret: number;
                protected inherited: number;
                #hidden;
                /** @private */
                internal: number;
                value: number;
            }
            """);
        var table = await program.Extract();
        Assert.That(table, Is.EqualTo(new[]
        {
            "format\t1",
            "holds\tapi:Gauge@1.value/read/0\tmember\tapi:Gauge@1.value\tread\t0\tother\t\tnumber",
            "holds\tapi:Gauge@1.value/write/0\tmember\tapi:Gauge@1.value\twrite\t0\tother\t\tnumber",
            "member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\tfalse\tfalse\tfalse",
            "type\tapi:Gauge@1\tapi\tGauge\tclass\tGauge",
        }));
    }

    [AcceptedCase]
    public async Task a_public_member_is_written()
    {
        using var program = new VendorProgram("export declare class Gauge { public secret: number; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.secret\tapi:Gauge@1\tsecret\tproperty\tfalse\tfalse\tfalse\tfalse"));
    }

    [RefusedCase]
    public async Task a_getter_alone_is_read_only()
    {
        using var program = new VendorProgram("export declare class Gauge { get value(): number; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\treadonly\tfalse\tfalse\tfalse\tfalse"));
    }

    [RefusedCase]
    public async Task a_setter_alone_is_write_only()
    {
        using var program = new VendorProgram("export declare class Gauge { set value(value: number | null); }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\twriteonly\tfalse\tfalse\ttrue\tfalse"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.value/write/0\tmember\tapi:Gauge@1.value\twrite\t0\tnull\t\t"));
    }

    [AcceptedCase]
    public async Task a_getter_with_a_setter_keeps_the_two_types_separate()
    {
        using var program = new VendorProgram("export declare class Gauge { get value(): number; set value(value: string | null); }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\tfalse\ttrue\tfalse"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.value/read/0\tmember\tapi:Gauge@1.value\tread\t0\tother\t\tnumber"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.value/write/1\tmember\tapi:Gauge@1.value\twrite\t1\tother\t\tstring"));
    }

    [RefusedCase]
    public async Task a_readonly_property_takes_no_write()
    {
        using var program = new VendorProgram("export interface Gauge { readonly value: number | null; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\treadonly\tfalse\ttrue\tfalse\tfalse"));
    }

    [RefusedCase]
    public async Task an_explicit_undefined_read_does_not_take_a_null_write()
    {
        using var program = new VendorProgram("export interface Gauge { value: number | undefined; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\ttrue\tfalse\tfalse"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.value/read/0\tmember\tapi:Gauge@1.value\tread\t0\tundefined\t\t"));
    }

    [RefusedCase]
    public async Task a_null_read_and_write_are_marked()
    {
        using var program = new VendorProgram("export interface Gauge { value: number | null; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\ttrue\ttrue\tfalse"));
    }

    [AcceptedCase]
    public async Task an_optional_member_keeps_presence_apart_from_explicit_null()
    {
        using var program = new VendorProgram("export interface Gauge { value?: number; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\ttrue\tfalse\tfalse\tfalse"));
    }

    [RefusedCase]
    public async Task an_instantiated_setter_takes_its_resolved_type()
    {
        using var program = new VendorProgram("export declare class Base<T> { get value(): number; set value(value: T); } export declare class Gauge extends Base<string | null> {}");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Base#1.value\tapi:Base#1\tvalue\tproperty\tfalse\tfalse\ttrue\tfalse"));
        Assert.That(table, Does.Contain("holds\tapi:Base#1.value/write/1\tmember\tapi:Base#1.value\twrite\t1\tother\t\tstring"));
    }

    [AcceptedCase]
    public async Task an_untyped_member_admits_null_both_ways()
    {
        using var program = new VendorProgram("export interface Gauge { value: unknown; anything: any; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.value\tapi:Gauge@1\tvalue\tproperty\tfalse\ttrue\ttrue\tfalse"));
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.anything\tapi:Gauge@1\tanything\tproperty\tfalse\ttrue\ttrue\tfalse"));
    }
}
