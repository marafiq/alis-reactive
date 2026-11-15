using System.Threading.Tasks;
using NUnit.Framework;

namespace Alis.Reactive.AnalyzerTests.Vendors;

[TestFixture]
public sealed class WhenAVendorDeclaresCalls
{
    [RefusedCase]
    public async Task each_overload_keeps_its_own_parameters_and_return()
    {
        using var program = new VendorProgram("""
            export interface Gauge {
                read(name: string, count?: number): string;
                read(count: number, name: string): number;
            }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("signature\tapi:Gauge@1.read/s0\tmember\tapi:Gauge@1.read\t0\tnone\tfalse\tfalse"));
        Assert.That(table, Does.Contain("signature\tapi:Gauge@1.read/s1\tmember\tapi:Gauge@1.read\t1\tnone\tfalse\tfalse"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.read/s0/p0/value/0\tparameter\tapi:Gauge@1.read/s0/p0\tvalue\t0\tother\t\tstring"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.read/s1/p0/value/0\tparameter\tapi:Gauge@1.read/s1/p0\tvalue\t0\tother\t\tnumber"));
        Assert.That(table, Does.Contain("parameter\tapi:Gauge@1.read/s0/p1\tapi:Gauge@1.read/s0\t1\ttrue\tfalse"));
        Assert.That(table, Does.Contain("parameter\tapi:Gauge@1.read/s1/p1\tapi:Gauge@1.read/s1\t1\tfalse\tfalse"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.read/s1/returns/0\tsignature\tapi:Gauge@1.read/s1\treturns\t0\tother\t\tnumber"));
    }

    [AcceptedCase]
    public async Task a_single_signature_has_only_its_own_ordinal()
    {
        using var program = new VendorProgram("export interface Gauge { read(name: string): number; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("signature\tapi:Gauge@1.read/s0\tmember\tapi:Gauge@1.read\t0\tnone\tfalse\tfalse"));
        Assert.That(table, Does.Not.Contain("signature\tapi:Gauge@1.read/s1\tmember\tapi:Gauge@1.read\t1\tnone\tfalse\tfalse"));
    }

    [RefusedCase]
    public async Task a_promised_return_is_awaited_and_keeps_its_nullability()
    {
        using var program = new VendorProgram("""
            interface Promise<T> { then(done: (value: T) => unknown): unknown; }
            export interface Gauge { read(): Promise<number | null>; }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("signature\tapi:Gauge@1.read/s0\tmember\tapi:Gauge@1.read\t0\tpromise\ttrue\tfalse"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.read/s0/returns/1\tsignature\tapi:Gauge@1.read/s0\treturns\t1\tother\t\tnumber"));
    }

    [RefusedCase]
    public async Task a_signature_that_returns_both_a_promise_and_a_value_is_mixed()
    {
        using var program = new VendorProgram("""
            interface Promise<T> { then(done: (value: T) => unknown): unknown; }
            export interface Gauge { read(): Promise<number> | string; }
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("signature\tapi:Gauge@1.read/s0\tmember\tapi:Gauge@1.read\t0\tmixed\tfalse\tfalse"));
    }

    [AcceptedCase]
    public async Task a_void_answer_takes_undefined()
    {
        using var program = new VendorProgram("export interface Gauge { changed: () => void; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("member\tapi:Gauge@1.changed\tapi:Gauge@1\tchanged\tproperty\tfalse\tfalse\tfalse\ttrue"));
        Assert.That(table, Does.Contain("signature\tapi:Gauge@1.changed/s0\tmember\tapi:Gauge@1.changed\t0\tnone\tfalse\ttrue"));
    }

    [RefusedCase]
    public async Task a_nullable_callback_answer_keeps_null_and_undefined_apart()
    {
        using var program = new VendorProgram("export interface Gauge { changed: () => number | undefined; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("signature\tapi:Gauge@1.changed/s0\tmember\tapi:Gauge@1.changed\t0\tnone\ttrue\ttrue"));
    }

    [AcceptedCase]
    public async Task a_types_own_call_and_a_functions_parameter_have_signatures()
    {
        using var program = new VendorProgram("export interface Gauge { (answer: (count: number) => string): void; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("signature\tapi:Gauge@1/s0\ttype\tapi:Gauge@1\t0\tnone\tfalse\ttrue"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1/s0/p0/value/0\tparameter\tapi:Gauge@1/s0/p0\tvalue\t0\tfunction\tapi:function#1\t"));
        Assert.That(table, Does.Contain("holds\tapi:function#1/s0/p0/value/0\tparameter\tapi:function#1/s0/p0\tvalue\t0\tother\t\tnumber"));
        Assert.That(table, Does.Contain("holds\tapi:function#1/s0/returns/0\tsignature\tapi:function#1/s0\treturns\t0\tother\t\tstring"));
    }

    [AcceptedCase]
    public async Task a_function_parameter_keeps_its_signature_when_it_holds_a_free_type()
    {
        using var program = new VendorProgram("export interface Gauge { read<T>(answer: (value: T) => T): void; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("holds\tapi:function#1/s0/p0/value/0\tparameter\tapi:function#1/s0/p0\tvalue\t0\tother\t\tT"));
        Assert.That(table, Does.Contain("holds\tapi:function#1/s0/returns/0\tsignature\tapi:function#1/s0\treturns\t0\tother\t\tT"));
    }

    [AcceptedCase]
    public async Task a_free_type_parameter_keeps_its_constraint_without_listing_instantiations()
    {
        using var program = new VendorProgram("export interface Item { name: string; } export interface Gauge { read<T extends Item>(value: T): T; }");
        var table = await program.Extract();
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.read/s0/p0/value/0\tparameter\tapi:Gauge@1.read/s0/p0\tvalue\t0\tother\t\tT"));
        Assert.That(table, Does.Contain("holds\tapi:Gauge@1.read/s0/p0/value/0/constraint/0\tholds\tapi:Gauge@1.read/s0/p0/value/0\tconstraint\t0\tobject\tapi:Item@1\t"));
    }

    [AcceptedCase]
    public async Task a_free_function_keeps_its_defaulted_and_rest_parameters()
    {
        using var program = new VendorProgram("""
            declare global { interface Array<T> { [index: number]: T; } }
            export function read(count = 1, ...names: string[]): void {}
            """);
        var table = await program.Extract();
        Assert.That(table, Does.Contain("type\tapi:read@1\tapi\tread\tfunction\tread"));
        Assert.That(table, Does.Contain("parameter\tapi:read@1/s0/p0\tapi:read@1/s0\t0\ttrue\tfalse"));
        Assert.That(table, Does.Contain("parameter\tapi:read@1/s0/p1\tapi:read@1/s0\t1\tfalse\ttrue"));
        Assert.That(table, Does.Contain("holds\tapi:read@1/s0/p1/value/0\tparameter\tapi:read@1/s0/p1\tvalue\t0\tother\t\tstring"));
    }
}
